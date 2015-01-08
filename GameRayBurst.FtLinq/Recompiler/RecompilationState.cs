using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using GameRayBurst.FtLinq.Recompiler.Util;

namespace GameRayBurst.FtLinq.Recompiler
{
    public delegate void PostRecompilationStepDelegate(Expression nestedBlock);

    public class RecompilationState
    {
        private readonly LambdaExpression myLambda;

        /// <summary>
        /// Contains the linear chain of fluent calls to LINQ methods that take as a first parameter
        /// a source IEnumerable and return an IEnumerable in order from the outermost call
        /// (the aggregation method) to the innermost call (the source provider).
        /// </summary>
        public readonly ReadOnlyCollection<Call> ExpressionList;
        public int CurrentCall { get; set; }

        public readonly IAggregationMethod AggregationMethod;
        public ParameterExpression AggregationVariable { get; private set; }

        public ParameterExpression InputVariable { get; set; }
        public ParameterExpression SequenceEmptyVariable { get; private set; }

        public LabelTarget BreakLabel { get; private set; }
        public LabelTarget ContinueLabel { get; set; }

        private readonly Dictionary<Tuple<string, MethodCallExpression>, object> myLocalContext =
            new Dictionary<Tuple<string, MethodCallExpression>, object>();

        public IList<ParameterExpression> Variables
        {
            get { return mySteps.Peek().Variables; }
        }

        public IList<Expression> Body
        {
            get { return mySteps.Peek().Body; }
        }

        public MethodCallExpression CurrentMethodCallExpression
        {
            get { return ExpressionList[CurrentCall].MethodCallExpression; }
        }

        public MethodInfo CurrentMethod
        {
            get { return ExpressionList[CurrentCall].Method; }
        }

        public ReadOnlyCollection<MethodCallExpression> Options
        {
            get { return ExpressionList[CurrentCall].Options; }
        }

        public void PushBlock(PostRecompilationStepDelegate postRecompilationStep)
        {
            mySteps.Push(new RecompilationStep(postRecompilationStep, CurrentCall));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Name must be unique only within the context of the current call.</param>
        /// <param name="param"></param>
        public void CreateLocal(string name, ParameterExpression param)
        {
            StoreContext(name, param);
        }

        public ParameterExpression GetLocal(string name)
        {
            return (ParameterExpression) GetContext(name);
        }

        public void StoreContext(string name, object context)
        {
            myLocalContext.Add(Tuple.Create(name, CurrentMethodCallExpression), context);
        }

        public object GetContext(string name)
        {
            object result;
            myLocalContext.TryGetValue(Tuple.Create(name, CurrentMethodCallExpression), out result);
            return result;
        }

        public IIterationState IterationState { get; private set; }

        public IEnumerable<MethodCallExpression> GetOptions<TOutput>(Expression<Func<IEnumerable<int>, TOutput>> optionMethod)
        {
            var body = (MethodCallExpression) optionMethod.Body;
            var method = body.Method;
            return Options.Where(opt => ReferenceEquals(opt.Method, method));
        }

        public void CreateIteratedSliceBoundsSettersAndTransitionState(
            ParameterExpression indexVar, ParameterExpression upperBoundVar, Expression countExpr,
            out Expression indexInitExpr, out Expression countInitExpr)
        {
            // optimized forms:
            // .Skip()
            // .Take()
            // .Skip().Take()

            var i = CurrentCall;
            var next = i >= 1 ? ExpressionList[i - 1] : null;
            var nextnext = i >= 2 ? ExpressionList[i - 2] : null;

            bool gotSkip = next != null && ReflectionUtil.IsMethodFromEnumerable(next.Method, "Skip");
            if (gotSkip)
            {
                indexInitExpr = Expression.Assign(indexVar, next.MethodCallExpression.Arguments[1]);
                CurrentCall--;
            }
            else
            {
                indexInitExpr = Expression.Assign(indexVar, Expression.Constant(0));
            }

            var take = next != null && ReflectionUtil.IsMethodFromEnumerable(next.Method, "Take") ? next
                : nextnext != null && ReflectionUtil.IsMethodFromEnumerable(nextnext.Method, "Take") ? nextnext
                    : null;

            if (take != null && (ReferenceEquals(take, next) || gotSkip))
            {
                var minMethod = typeof(Math).GetMethod("Min", new[] { typeof(int), typeof(int) });
                countInitExpr = Expression.Assign(upperBoundVar,
                    Expression.Call(minMethod, countExpr, take.MethodCallExpression.Arguments[1]));
                CurrentCall--;
            }
            else
            {
                countInitExpr = Expression.Assign(upperBoundVar, countExpr);
            }
        }

        private readonly Stack<RecompilationStep> mySteps = new Stack<RecompilationStep>();

        private const int AggregationMethodIndex = 0;
        private int IterationMethodIndex { get { return ExpressionList.Count - 1; } }

        internal RecompilationState(LambdaExpression lambda)
        {
            myLambda = lambda;
            ExpressionList = LinqParser.ParseLinqExpression(lambda.Body).AsReadOnly();

            var topFunc = ExpressionList[0].MethodCallExpression;

            AggregationMethod = FtlConfiguration.AggregationMethods.FindMethod(topFunc.Method);
            if (AggregationMethod == null)
                throw new FtlException(topFunc, "Unsupported aggregation method. Lookup expression: {0}");
        }

        internal LambdaExpression BuildImperativeVersion()
        {
            var sourceExpr = ExpressionList.Last().OriginalExpression;
            if (sourceExpr.NodeType == ExpressionType.Convert)
                sourceExpr = ((UnaryExpression) sourceExpr).Operand;

            var iterationMethod = FtlConfiguration.IterationMethods.FindMethod(sourceExpr.Type);
            if (iterationMethod == null)
                throw new FtlException(sourceExpr, "Unsupported iteration method. Lookup expression: {0}");

            // prepare global block
            PushBlock(null);
            var globalBody = Body;
            var globalVariables = Variables;
            var topFunc = ExpressionList[0].MethodCallExpression;

            var sourceVar = Expression.Variable(sourceExpr.Type);
            globalVariables.Add(sourceVar);
            globalBody.Add(Expression.Assign(sourceVar, sourceExpr));

            // prepare iteration state
            CurrentCall = AggregationMethodIndex;
            bool specialHandlesEmptySequences = AggregationMethod.SpecialHandlesEmptySequences(topFunc.Method);

            CurrentCall = IterationMethodIndex;
            IterationState = iterationMethod.CreateIterationState(sourceVar, specialHandlesEmptySequences);

            // check if the input parameter is null
            if (!sourceVar.Type.IsValueType && sourceExpr is ParameterExpression)
            {
                globalBody.Add(Expression.IfThen(
                    Expression.Equal(sourceVar, Expression.Constant(null)),
                    ExpressionUtil.Throw(typeof (ArgumentNullException), "source")));
            }

            // create and initialize aggregation variable
            CurrentCall = AggregationMethodIndex;
            AggregationVariable = AggregationMethod.CreateAggregationVariable(this, topFunc.Method.ReturnType);

            // create main iteration loop
            CurrentCall = IterationMethodIndex - 1;
            InputVariable = IterationState.ItemVariable;
            SequenceEmptyVariable = IterationState.SequenceEmptyVariable;
            ContinueLabel = IterationState.ContinueLabel;
            BreakLabel = IterationState.BreakLabel;
                    
            PushBlock(null); // main iteration block
            while (CurrentCall != AggregationMethodIndex)
            {
                var transformExpr = ExpressionList[CurrentCall];
                var transformMethod = FtlConfiguration.TransformMethods.FindMethod(transformExpr.Method);
                if (transformMethod == null)
                    throw new FtlException(transformExpr.OriginalExpression, "Unsupported transform method: {0}");

                transformMethod.CreateStateVariables(this, globalVariables, globalBody);
                transformMethod.CreateTransform(this);
                transformMethod.TransitionState(this);
            }

            AggregationMethod.AggregateItem(this);

            while (mySteps.Count > 2)
            {
                var step = mySteps.Pop();
                var nestedBlock = Expression.Block(step.Variables, step.Body);
                CurrentCall = step.CallIndex;
                step.Delegate(nestedBlock);
            }

            var iterationStep = mySteps.Pop();
            var iterationBlock = Expression.Block(iterationStep.Variables, iterationStep.Body);

            CurrentCall = IterationMethodIndex;
            iterationMethod.CreateIterationBlock(this, IterationState, iterationBlock);

            // create return from method
            CurrentCall = AggregationMethodIndex;
            AggregationMethod.CreateReturnExpression(this);

            var globalStep = mySteps.Pop();
            var queryBlock = Expression.Block(globalStep.Variables, globalStep.Body);

            return Expression.Lambda(queryBlock, myLambda.Parameters);
        }

        internal void EmitDiagnostic(string message)
        {
            string str = String.Format("FtLinq: {0}. LINQ expression: {1}", message, myLambda);
            FtlConfiguration.EmitDiagnostic(str, CurrentMethodCallExpression);
        }

        internal class RecompilationStep
        {
            public readonly List<ParameterExpression> Variables = new List<ParameterExpression>();
            public readonly List<Expression> Body = new List<Expression>();
            public readonly PostRecompilationStepDelegate Delegate;
            public readonly int CallIndex;

            public RecompilationStep(PostRecompilationStepDelegate @delegate, int callIndex)
            {
                Delegate = @delegate;
                CallIndex = callIndex;
            }
        }
    }
}