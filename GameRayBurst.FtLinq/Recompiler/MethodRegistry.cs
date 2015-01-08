using System.Collections.Generic;

namespace GameRayBurst.FtLinq.Recompiler
{
    public sealed class MethodRegistry<TMethodType, TInputType>
        where TMethodType : class, IMethod<TInputType>
    {
        private readonly List<TMethodType> myMethods = new List<TMethodType>();

        internal MethodRegistry() {} 

        public void Register(TMethodType method)
        {
            myMethods.Add(method);
        }

        public void Unregister(TMethodType method)
        {
            myMethods.Remove(method);
        }

        internal TMethodType FindMethod(TInputType input)
        {
            for (int i = myMethods.Count - 1; i >= 0; --i)
            {
                var method = myMethods[i];
                if (method.CanHandle(input))
                    return method;
            }

            return null;
        }
    }
}
