using System.Linq.Expressions;

namespace GameRayBurst.FtLinq.Recompiler.Iterators
{
    internal abstract class IterationStateBase : IIterationState
    {
        private readonly ParameterExpression mySequenceEmptyVar;

        protected IterationStateBase(bool createSequenceEmptyVar)
        {
            if (createSequenceEmptyVar)
            {
                mySequenceEmptyVar = Expression.Variable(typeof (bool));
            }

            ContinueLabel = Expression.Label();
            BreakLabel = Expression.Label();
        }

        public abstract ParameterExpression ItemVariable { get; }
        public abstract Expression Count { get; }

        public ParameterExpression SequenceEmptyVariable
        {
            get { return mySequenceEmptyVar; }
        }

        public LabelTarget BreakLabel { get; private set; }
        public LabelTarget ContinueLabel { get; private set; }

        public Expression InitializeSequenceEmptyVariable()
        {
            if (mySequenceEmptyVar == null)
                return null;
            return Expression.Assign(mySequenceEmptyVar, Expression.Constant(true));
        }
    }
}
