using System;
using System.Linq.Expressions;
using System.Reflection;

namespace GameRayBurst.FtLinq.Recompiler
{
    public abstract class AggregationMethodBase : IAggregationMethod
    {
        public abstract bool CanHandle(MethodInfo input);
        public abstract void AggregateItem(RecompilationState state);

        public virtual bool SpecialHandlesEmptySequences(MethodInfo method)
        {
            return false;
        }

        public virtual ParameterExpression CreateAggregationVariable(RecompilationState state, Type aggregationVariableType)
        {
            var variable = CreateDefaultAggregationVariable(state, aggregationVariableType);
            CreateDefaultInitializer(state, variable);
            return variable;
        }

        protected static ParameterExpression CreateDefaultAggregationVariable(RecompilationState state, Type aggregationVariableType)
        {
            var variable = Expression.Variable(aggregationVariableType);
            state.Variables.Add(variable);
            return variable;
        }

        protected static void CreateDefaultInitializer(RecompilationState state, ParameterExpression variable)
        {
            var ctor = variable.Type.GetConstructor(new Type[0]);
            var initExpr = ctor != null
                ? (Expression) Expression.New(variable.Type)
                : Expression.Default(variable.Type);

            state.Body.Add(Expression.Assign(variable, initExpr));
        }

        public virtual void CreateReturnExpression(RecompilationState state)
        {
            state.Body.Add(state.AggregationVariable);
        }
    }
}
