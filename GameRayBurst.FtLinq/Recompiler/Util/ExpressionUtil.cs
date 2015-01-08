using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using GameRayBurst.FtLinq.Recompiler.Iterators;
using IQToolkit;

namespace GameRayBurst.FtLinq.Recompiler.Util
{
    internal static class ExpressionUtil
    {
        public static Expression For(ParameterExpression[] variables, Expression initializer, Expression check,
            Expression increment, LabelTarget continueLabel, LabelTarget breakLabel, Expression body)
        {
            var startLabel = Expression.Label();
            var loopBody = Expression.Label();

            var block = Expression.Block(
                variables,
                new[]
                    {
                        initializer,
                        Expression.Goto(startLabel),
                        Expression.Label(loopBody),
                        body,
                        Expression.Label(continueLabel ?? Expression.Label()),
                        increment,
                        Expression.Label(startLabel),
                        Expression.IfThen(check, Expression.Goto(loopBody)),
                        Expression.Label(breakLabel ?? Expression.Label())
                    }
                );

            return block;
        }

        public static Expression While(Expression check, LabelTarget continueLabel, LabelTarget breakLabel, Expression body)
        {
            var loopBody = Expression.Label();

            var block = Expression.Block(
                Expression.Goto(continueLabel),
                Expression.Label(loopBody),
                body,
                Expression.Label(continueLabel),
                Expression.IfThen(check, Expression.Goto(loopBody)),
                Expression.Label(breakLabel));

            return block;
        }

        internal sealed class ForEachState : IterationStateBase
        {
            public readonly Expression Source;
            public readonly ParameterExpression Item;
            public readonly Type EnumeratorType;
            public readonly MethodInfo GetEnumeratorMethod;

            public ForEachState(Expression source, bool createSequenceEmptyVar)
                : base(createSequenceEmptyVar)
            {
                Source = source;

                var ienumerable = source.Type.GetImplementedEnumerableInterface();

                GetEnumeratorMethod = source.Type.GetMethod("GetEnumerator", new Type[0]);
                if (GetEnumeratorMethod != null)
                {
                    if (GetEnumeratorMethod.GetParameters().Length != 0
                    || (!typeof(IEnumerator).IsAssignableFrom(GetEnumeratorMethod.ReturnType)
                        && (!GetEnumeratorMethod.ReturnType.IsGenericType
                            || !typeof(IEnumerator<>).IsAssignableFrom(GetEnumeratorMethod.ReturnType.GetGenericTypeDefinition()))))
                        throw new FtlException(source, "Improper GetEnumerator implementation");
                }
                else if (ienumerable != null)
                {
                    GetEnumeratorMethod = source.Type.FindInterfaceMethod(ienumerable, "GetEnumerator");
                }
                else
                {
                    throw new FtlException(source, "Improper GetEnumerator implementation");
                }

                EnumeratorType = GetEnumeratorMethod.ReturnType;
                var elementType = ienumerable.IsGenericType ? ienumerable.GetGenericArguments()[0] : typeof(object);
                Item = Expression.Variable(elementType);
            }

            public override ParameterExpression ItemVariable
            {
                get { return Item; }
            }

            public override Expression Count
            {
                get { return null; }
            }
        }

        public static IIterationState CreateForEachIterationState(Expression source, bool createEmptySequenceVar)
        {
            return new ForEachState(source, createEmptySequenceVar);
        }

        public static Expression ForEach(IIterationState iterationState, Expression body)
        {
            var state = (ForEachState) iterationState;
            var source = state.Source;
            var enumeratorType = state.EnumeratorType;
            var currentItem = state.Item;

            var enumerator = Expression.Variable(enumeratorType);

            var moveNext = enumeratorType.FindInterfaceMethod(typeof (IEnumerator), "MoveNext");
            var dispose = enumeratorType.FindInterfaceMethod(typeof (IDisposable), "Dispose");
            var currentProp = enumeratorType.GetProperty("Current");

            var block = Expression.Block(
                new[] {enumerator, currentItem},
                new[]
                    {
                        Expression.Assign(enumerator, Expression.Call(source, state.GetEnumeratorMethod)),
                        state.SequenceEmptyVariable != null
                            ? state.InitializeSequenceEmptyVariable()
                            : Expression.Empty(),
                        While(Expression.IsTrue(Expression.Call(enumerator, moveNext)),
                            state.ContinueLabel, state.BreakLabel,
                            Expression.Block(
                                new[] {currentItem},
                                new[]
                                    {
                                        Expression.Assign(currentItem, Expression.Property(enumerator, currentProp)),
                                        state.SequenceEmptyVariable != null
                                            ? Expression.Assign(state.SequenceEmptyVariable, Expression.Constant(false))
                                            : (Expression) Expression.Empty(),
                                        body
                                    }
                                )
                        ),
                        dispose != null ? (Expression) Expression.Call(enumerator, dispose) : Expression.Empty()
                    });

            return block;
        }

        public static int GetLambdaParameterCount(this Expression lambda)
        {
            var expr = lambda as LambdaExpression;
            if (expr != null)
                return expr.Parameters.Count;

            var delegateType = lambda.Type;
            var method = delegateType.GetMethod("Invoke");
            if (method != null)
                return method.GetParameters().Length;

            throw new FtlException(lambda, "Expression does not represent a lambda or delegate");
        }

        public static Type GetLambdaReturnType(this Expression maybeLambda)
        {
            var expr = maybeLambda as LambdaExpression;
            if (expr != null)
                return expr.Body.Type; // the body type may be more concrete than the return type and allow better optimization

            var delegateType = maybeLambda.Type;
            if (typeof(Delegate).IsAssignableFrom(delegateType))
            {
                var method = delegateType.GetMethod("Invoke");
                if (method != null)
                    return method.ReturnType;
            }

            return maybeLambda.Type;
        }

        public static Expression RewriteCall(this Expression lambda, params ParameterExpression[] substituteParameters)
        {
            var lambdaExpr = lambda as LambdaExpression;
            if (lambdaExpr != null)
            {
                if (lambdaExpr.Parameters.Count != substituteParameters.Length)
                    throw new FtlException(lambdaExpr, "Mismatch between lambda parameters and substitution parameters.");

                var body = lambdaExpr.Body;
                return ExpressionReplacer.ReplaceAll(body, lambdaExpr.Parameters.ToArray(), substituteParameters);
            }
            else
            {
                return Expression.Invoke(lambda, substituteParameters);
            }
        }

        public static bool IsNullable(this Expression expr)
        {
            return Nullable.GetUnderlyingType(expr.Type) != null;
        }

        public static Expression NewNullable(Type nullable, Expression ctorValue)
        {
            return Expression.New(nullable.GetConstructor(new[] {Nullable.GetUnderlyingType(nullable)}), ctorValue);
        }

        private static Expression CreateBinaryNullableNullingExpression(Expression checkExpression
            , Expression opExpression, Expression assignmentFallbackValue)
        {
            var resultType = opExpression.Type;
            var nullableType = typeof (Nullable<>).MakeGenericType(resultType);

            if (assignmentFallbackValue == null)
                assignmentFallbackValue = Expression.Constant(null, nullableType);
            else
            {
                if (assignmentFallbackValue.Type != nullableType)
                    assignmentFallbackValue = NewNullable(nullableType, assignmentFallbackValue);
            }

            return Expression.Condition(checkExpression,
                NewNullable(nullableType, opExpression),
                assignmentFallbackValue);
        }

        public static Expression CreateNullableBinaryExpression(Expression a, Expression b, ExpressionType op, bool isAggregationExpression)
        {
            var aNullable = IsNullable(a);
            var bNullable = IsNullable(b);

            Expression opExpression;
            var fallbackValue = isAggregationExpression ? b : null;

            if (aNullable && bNullable)
            {
                opExpression = CreateBinaryNullableNullingExpression(
                    Expression.AndAlso(Expression.Property(a, "HasValue"), Expression.Property(b, "HasValue")),
                    Expression.MakeBinary(op, Expression.Property(a, "Value"), Expression.Property(b, "Value")),
                    fallbackValue);
            }
            else if (aNullable)
            {
                opExpression = CreateBinaryNullableNullingExpression(Expression.Property(a, "HasValue"),
                    Expression.MakeBinary(op, Expression.Property(a, "Value"), b),
                    fallbackValue);
            }
            else if (bNullable)
            {
                opExpression = CreateBinaryNullableNullingExpression(Expression.Property(b, "HasValue"),
                    Expression.MakeBinary(op, a, Expression.Property(b, "Value")),
                    fallbackValue);
            }
            else opExpression = Expression.MakeBinary(op, a, b);

            return opExpression;
        }

        public static Expression Throw(Type exceptionType, params string[] ctorParams)
        {
            var ctor = exceptionType.GetConstructor(ctorParams.Select(p => p.GetType()).ToArray());
            return Expression.Throw(Expression.New(ctor, ctorParams.Select(Expression.Constant)));
        }

        public static void Dump(this LambdaExpression lambda, string assemblyName)
        {
            var assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(
                new AssemblyName(assemblyName), AssemblyBuilderAccess.RunAndSave);
            var module = assembly.DefineDynamicModule(assemblyName, assemblyName + ".dll");

            var type = module.DefineType("DumpClass", TypeAttributes.Public | TypeAttributes.Class);
            var method = type.DefineMethod(
                "DumpedLambda", MethodAttributes.Public | MethodAttributes.Static,
                lambda.ReturnType, lambda.Parameters.Select(p => p.Type).ToArray());
            lambda.CompileToMethod(method);
            var t = type.CreateType();
            assembly.Save(assemblyName + ".dll");
        }

        public static bool IsIdentityTransform(this LambdaExpression lambda)
        {
            return lambda.Parameters.Count == 1
                   && lambda.Parameters[0].Type == lambda.ReturnType
                   && lambda.Body == lambda.Parameters[0];
        }
    }
}
