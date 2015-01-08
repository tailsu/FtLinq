using System;
using System.Linq.Expressions;

namespace GameRayBurst.FtLinq
{
    public class FtlException : Exception
    {
        public Expression ErrorExpression { get; private set; }

        internal FtlException(Expression errorExpression, string message)
            : base(FormatMessage(errorExpression, message))
        {
            ErrorExpression = errorExpression;
        }

        private static string FormatMessage(Expression errorExpression, string message)
        {
            if (message.Contains("{0}"))
                return String.Format(message, errorExpression);

            return message;
        }
    }
}
