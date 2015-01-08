using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameRayBurst.FtLinq.Tests
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class DontDecompileAttribute : Attribute {}

    [TestClass]
    public class Dumper
    {
        private static AssemblyBuilder dynamicAssembly;
        private static ModuleBuilder dynamicModule;
        private static TypeBuilder dynamicClass;
        internal static string CompiledQueriesDumpFile;

        [AssemblyInitialize]
        [Conditional("DEBUG")]
        public static void Initialize(TestContext context)
        {
            CompiledQueriesDumpFile = @"..\..\..\CompiledTestQueries.dll";

            Ftl.QueryCompiled += (sender, args) => Dump(args.OriginalExpression, args.RecompiledExpression);
        }

        [AssemblyCleanup]
        [Conditional("DEBUG")]
        public static void Finish()
        {
            SaveDumpAssembly();
        }

        private static void Dump(LambdaExpression original, LambdaExpression recompiled)
        {
            var assemblyFilePath = CompiledQueriesDumpFile;
            if (assemblyFilePath == null)
                return;

            var frames = new StackTrace().GetFrames();
            string methodName = null;
            foreach (var frame in frames)
            {
                var frameMethod = frame.GetMethod();
                if (frameMethod.DeclaringType.Assembly.GetName().Name != "GameRayBurst.FtLinq.Tests")
                    continue;

                if (frameMethod.GetCustomAttributes(true).OfType<Attribute>()
                    .Any(attr => attr is DontDecompileAttribute))
                    return;

                if (!frameMethod.GetCustomAttributes(true).OfType<Attribute>()
                    .Any(attr => attr.GetType().Name == "TestMethodAttribute"))
                    continue;

                methodName = frameMethod.Name;
                break;
            }
            Debug.Assert(methodName != null);

            var fileName = Path.GetFileName(assemblyFilePath);
            var name = Path.GetFileNameWithoutExtension(fileName);

            var curDir = Environment.CurrentDirectory;
            Environment.CurrentDirectory = Path.GetDirectoryName(assemblyFilePath);

            if (dynamicAssembly == null)
                dynamicAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(name), AssemblyBuilderAccess.RunAndSave);
            if (dynamicModule == null)
                dynamicModule = dynamicAssembly.DefineDynamicModule(fileName, fileName);

            if (dynamicClass == null)
                dynamicClass = dynamicModule.DefineType("CompiledTestQueries", TypeAttributes.Class | TypeAttributes.Public);

            var method = dynamicClass.DefineMethod(methodName + "_Recompiled", MethodAttributes.Static | MethodAttributes.Public,
                recompiled.ReturnType, recompiled.Parameters.Select(p => p.Type).ToArray());

            try
            {
                recompiled.CompileToMethod(method);
            }
            catch (InvalidOperationException)
            {
                // if the recompiled lambda contains a closure, then it can't be compiled to a method
                var defaultLambda = Expression.Lambda(Expression.Default(recompiled.ReturnType), recompiled.Parameters);
                defaultLambda.CompileToMethod(method);
            }

            var originalField = dynamicClass.DefineField(methodName + "_Original", typeof (string), FieldAttributes.Literal | FieldAttributes.Public);
            originalField.SetConstant(original.Body.ToString());

            Environment.CurrentDirectory = curDir;
        }

        private static void SaveDumpAssembly()
        {
            if (dynamicAssembly == null)
                return;

            var assemblyFilePath = CompiledQueriesDumpFile;
            Debug.Assert(assemblyFilePath != null);

            var curDir = Environment.CurrentDirectory;
            Environment.CurrentDirectory = Path.GetDirectoryName(assemblyFilePath);

            dynamicClass.CreateType();
            dynamicAssembly.Save(Path.GetFileName(assemblyFilePath));

            Environment.CurrentDirectory = curDir;
        }
    }
}
