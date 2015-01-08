using System;
using System.Linq.Expressions;
using System.Reflection;
using GameRayBurst.FtLinq.Recompiler.Util;

namespace GameRayBurst.FtLinq.Recompiler.Enumerable
{
    internal sealed class ToArray : AggregationMethodBase
    {
        public const int InitialArraySize = 4;

        static ToArray()
        {
            //-OrderBy.MethodsSupportingSorting.Add(typeof(System.Linq.Enumerable).GetMethod("ToArray"));
        }

        public override bool CanHandle(MethodInfo input)
        {
            return ReflectionUtil.IsMethodFromEnumerable(input, "ToArray");
        }

        public override ParameterExpression CreateAggregationVariable(RecompilationState state, Type aggregationVariableType)
        {
            state.EmitDiagnostic("ToArray is slow; try to use ToList instead");

            var counter = Expression.Variable(typeof (int));
            state.Variables.Add(counter);
            state.CreateLocal("ToArray", counter);
            state.Body.Add(Expression.Assign(counter, Expression.Constant(0)));
            
            return base.CreateAggregationVariable(state, aggregationVariableType);
        }

        public override void AggregateItem(RecompilationState state)
        {
            var array = state.AggregationVariable;
            var elementType = array.Type.GetElementType();
            var counter = state.GetLocal("ToArray");

            var newArray = Expression.Variable(array.Type);

            var arrayReviserExpr =
                Expression.IfThenElse(Expression.Equal(array, Expression.Constant(null)),
                    Expression.Assign(array, Expression.NewArrayBounds(elementType, Expression.Constant(InitialArraySize))),
                    Expression.IfThen(Expression.Equal(Expression.ArrayLength(array), counter),
                    Expression.Block(new[] { newArray },
                        Expression.Assign(newArray, Expression.NewArrayBounds(elementType, Expression.MultiplyChecked(counter, Expression.Constant(2)))),
                        CopyArray(array, newArray, counter),
                        Expression.Assign(array, newArray)
                    )));

            state.Body.Add(arrayReviserExpr);
            state.Body.Add(Expression.Assign(Expression.ArrayAccess(array, counter), state.InputVariable));
            state.Body.Add(Expression.PreIncrementAssign(counter));
        }

        private static Expression CopyArray(Expression sourceArray, Expression destArray, Expression count)
        {
            var copyMethod = typeof(Array).GetMethod("Copy", new[] { typeof(Array), typeof(int), typeof(Array), typeof(int), typeof(int) });
            return Expression.Call(copyMethod, sourceArray, Expression.Constant(0), destArray, Expression.Constant(0), count);
        }

        public override void CreateReturnExpression(RecompilationState state)
        {
            var buffer = state.AggregationVariable;
            var elementType = buffer.Type.GetElementType();
            var counter = state.GetLocal("ToArray");

            var rightsizedBuffer = Expression.Variable(buffer.Type);

            state.Body.Add(
                Expression.IfThenElse(Expression.Equal(buffer, Expression.Constant(null)),
                    Expression.Assign(buffer, Expression.NewArrayBounds(elementType, Expression.Constant(0))),
                    Expression.IfThen(Expression.NotEqual(counter, Expression.ArrayLength(buffer)), 
                        Expression.Block(new[] { rightsizedBuffer },
                            Expression.Assign(rightsizedBuffer, Expression.NewArrayBounds(elementType, counter)),
                            CopyArray(buffer, rightsizedBuffer, counter),
                            Expression.Assign(buffer, rightsizedBuffer)))));
            state.Body.Add(buffer);
        }
    }
}
