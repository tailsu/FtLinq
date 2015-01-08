using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GameRayBurst.FtLinq.Recompiler.Util
{
    internal static class Generic
    {
        private abstract class ArgLabel<T> { }
        private abstract class ArgTerminator { }
        private abstract class FunArgLabel<T> { }
        private abstract class FunArgTerminator {}

        public static Type Arg(int index)
        {
            return Arg(typeof (ArgLabel<>), typeof (ArgTerminator), index);
        }

        public static int GetArgIndex(Type arg)
        {
            return GetIndex(typeof(ArgLabel<>), typeof(ArgTerminator), arg);
        }

        public static Type FunArg(int index)
        {
            return Arg(typeof(FunArgLabel<>), typeof(FunArgTerminator), index);
        }

        public static int GetFunArgIndex(Type arg)
        {
            return GetIndex(typeof(FunArgLabel<>), typeof(FunArgTerminator), arg);
        }

        private static Type Arg(Type label, Type terminator, int index)
        {
            var type = terminator;
            for (int i = 0; i < index; ++i)
                type = label.MakeGenericType(type);
            return type;
        }

        private static int GetIndex(Type label, Type terminator, Type arg)
        {
            int index = 0;
            while (arg.IsGenericType)
            {
                if (arg.GetGenericTypeDefinition() != label)
                    return -1;

                arg = arg.GetGenericArguments()[0];
                index++;
            }
            return arg == terminator ? index : -1;
        }
    }

    internal static class ReflectionUtil
    {
        public static PropertyInfo GetIndexer(this Type type)
        {
            return type.GetProperties().SingleOrDefault(p => p.GetIndexParameters().Length > 0);
        }

        public static Type FindGenericInterfaceInstance(this Type type, Type genericInterface)
        {
            return type.GetInterfaces().SingleOrDefault(intf => intf.IsGenericType && genericInterface == intf.GetGenericTypeDefinition());
        }

        public static Type GetImplementedEnumerableInterface(this Type type)
        {
            return type.FindGenericInterfaceInstance(typeof (IEnumerable<>)) ??
                   type.GetInterfaces().FirstOrDefault(intf => intf == typeof (IEnumerable));
        }

        public static bool IsMethodFromEnumerable(MethodInfo method, string methodName)
        {
            return method.DeclaringType == typeof(System.Linq.Enumerable)
                   && method.Name == methodName;
        }

        public static bool IsMethodFromFtlEnumerable(MethodInfo method, string methodName)
        {
            return method.DeclaringType == typeof (FtLinq.FtlEnumerable)
                && method.Name == methodName;
        }

        public static MethodInfo FindInterfaceMethod(this Type type, Type interfaceType, string methodName)
        {
            if (type.IsInterface)
            {
                return interfaceType.IsAssignableFrom(type)
                    ? interfaceType.GetMethod(methodName)
                    : null;
            }
            
            var intfMap = type.GetInterfaceMap(interfaceType);
            var implementationIndex = intfMap.InterfaceMethods
                .IndexOf(method => method.Name == methodName);

            return implementationIndex != -1 ? intfMap.TargetMethods[implementationIndex] : null;
        }

        public static IEnumerable<Type> GetInheritanceChain(this Type type)
        {
            while (type != null)
            {
                yield return type;
                type = type.BaseType;
            }
        }

        public static Type GetEnumerableElementType(this Type type)
        {
            var enumT = type.FindGenericInterfaceInstance(typeof (IEnumerable<>));
            if (enumT != null)
                return enumT.GetGenericArguments()[0];

            return typeof (object);
        }

        public static MethodInfo GetMethodLike(this Type type, string name, params Type[] paramTypes)
        {
            return (from method in type.GetMethods()
                    where method.Name == name
                    let methodParameterTypes = method.GetParameters().Select(p => p.ParameterType)
                    let typeComparer = new RecursiveTypeLikeComparer(method.GetGenericArguments(), type.GetGenericArguments())
                    where methodParameterTypes.SequenceEqual(paramTypes, typeComparer)
                    select method).FirstOrDefault();
        }

        private class RecursiveTypeLikeComparer : IEqualityComparer<Type>
        {
            private readonly Type[] myGenericMethodParams;
            private readonly Type[] myGenericTypeParams;

            public RecursiveTypeLikeComparer(Type[] genericMethodParams, Type[] genericTypeParams)
            {
                myGenericMethodParams = genericMethodParams;
                myGenericTypeParams = genericTypeParams;
            }

            public bool Equals(Type x, Type y)
            {
                if (!x.IsGenericParameter && !y.IsGenericParameter && x == y)
                    return true;

                if (x.ContainsGenericParameters)
                {
                    var genericType = ReplaceGenericArguments(y);
                    return x == genericType;
                }
                
                return false;
            }

            public int GetHashCode(Type obj)
            {
                throw new NotImplementedException();
            }

            private Type ReplaceGenericArguments(Type description)
            {
                if (description.IsArray)
                {
                    var elementType = description.GetElementType();
                    bool isVector = elementType.MakeArrayType() == description;
                    var genericElementType = ReplaceGenericArguments(elementType);
                    return isVector ? genericElementType.MakeArrayType() : genericElementType.MakeArrayType(description.GetArrayRank());
                }

                var funArg = Generic.GetFunArgIndex(description);
                if (funArg != -1)
                    return funArg < myGenericMethodParams.Length ? myGenericMethodParams[funArg] : description;

                var arg = Generic.GetArgIndex(description);
                if (arg != -1)
                    return arg < myGenericTypeParams.Length ? myGenericTypeParams[arg] : description;

                if (description.IsGenericType)
                {
                    var def = description.GetGenericArguments();
                    var genericTypeArgs = def.Select(ReplaceGenericArguments).ToArray();
                    var genericDef = description.GetGenericTypeDefinition().MakeGenericType(genericTypeArgs);
                    return genericDef;
                }

                return description;
            }
        }
    }
}
