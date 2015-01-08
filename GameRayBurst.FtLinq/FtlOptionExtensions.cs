using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GameRayBurst.FtLinq.Recompiler;

namespace GameRayBurst.FtLinq
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class FtlMethodOptionAttribute : Attribute
    {
        public string ModifiedMethodName { get; set; }
        public Type ModifiedMethodClass { get; set; }
        public bool IsGlobal { get; set; }
    }

    internal sealed class MethodOptionTransformMethod : TransformMethodBase
    {
        public override bool CanHandle(MethodInfo input)
        {
            return input.GetCustomAttributes(typeof (FtlMethodOptionAttribute), false).Length > 0;
        }

        public override void CreateTransform(RecompilationState state)
        {
        }
    }

    public static class FtlOptionExtensions
    {
        [FtlMethodOption(ModifiedMethodName = "ToList", ModifiedMethodClass = typeof(Enumerable))]
        public static IEnumerable<T> PreallocateOutputFromInputLength<T>(this IEnumerable<T> source)
        {
            return source;
        }
    }
}
