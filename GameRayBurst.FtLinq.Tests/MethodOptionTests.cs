using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using GameRayBurst.FtLinq.Recompiler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace GameRayBurst.FtLinq.Tests.Options
{
    internal static class TestOptionsExtensions
    {
        public static IEnumerable<T> FooTest<T>(this IEnumerable<T> source, string[] expectedOptions)
        {
            return source;
        }

        public static IEnumerable<T> BarTest<T>(this IEnumerable<T> source, string[] expectedOptions)
        {
            return source;
        }

        public static IEnumerable<T> YafTest<T>(this IEnumerable<T> source, string[] expectedOptions)
        {
            return source;
        }
    }

    internal static class AlternativeOptionsExtensions
    {
        public static IEnumerable<T> FooTest<T>(IEnumerable<T> source, string[] expectedOptions)
        {
            return source;
        }
    }

    internal static class TestMethodOptions
    {
        [FtlMethodOption(ModifiedMethodName = "FooTest", ModifiedMethodClass = typeof(TestOptionsExtensions))]
        public static IEnumerable<T> FooTest_OptionWithClassAndName<T>(this IEnumerable<T> source)
        {
            return source;
        }

        [FtlMethodOption(ModifiedMethodName = "FooTest")]
        public static IEnumerable<T> FooTest_OptionWithName<T>(this IEnumerable<T> source)
        {
            return source;
        }

        [FtlMethodOption(ModifiedMethodClass = typeof(TestOptionsExtensions))]
        public static IEnumerable<T> Test_OptionWithClass<T>(this IEnumerable<T> source)
        {
            return source;
        }

        [FtlMethodOption]
        public static IEnumerable<T> GenericLocalOption<T>(this IEnumerable<T> source)
        {
            return source;
        }

        [FtlMethodOption(IsGlobal = true)]
        public static IEnumerable<T> GlobalOption<T>(this IEnumerable<T> source)
        {
            return source;
        }

        [FtlMethodOption(ModifiedMethodName = "FooTest", ModifiedMethodClass = typeof(TestOptionsExtensions))]
        [FtlMethodOption(ModifiedMethodName = "BarTest", ModifiedMethodClass = typeof(TestOptionsExtensions))]
        public static IEnumerable<T> OptionForSeveralMethods<T>(this IEnumerable<T> source)
        {
            return source;
        }

        [FtlMethodOption, FtlMethodOption(IsGlobal = true)]
        public static IEnumerable<T> GlobalAndLocalOption<T>(this IEnumerable<T> source)
        {
            return source;
        }

        [FtlMethodOption(IsGlobal = true, ModifiedMethodClass = typeof(TestOptionsExtensions), ModifiedMethodName = "FooTest")]
        public static IEnumerable<T> SpecificGlobalOption<T>(this IEnumerable<T> source)
        {
            return source;
        }
    }

    internal class OptionVerifier : TransformMethodBase
    {
        private readonly string myInputName;

        public bool TransformCalled;
        public Action<RecompilationState> AssertState;

        public OptionVerifier(string inputName)
        {
            myInputName = inputName;
        }

        public override bool CanHandle(MethodInfo input)
        {
            return input.Name == myInputName;
        }

        public override void CreateTransform(RecompilationState state)
        {
            var expectedOptionsExpr = (NewArrayExpression) state.CurrentMethodCallExpression.Arguments[1];
            var expectedOptionMethods = expectedOptionsExpr.Expressions.Cast<ConstantExpression>().Select(expr => (string) expr.Value).ToList();
            var givenOptions = state.Options.Select(opt => opt.Method.Name).ToList();
            CollectionAssert.AreEquivalent(expectedOptionMethods, givenOptions);

            if (AssertState != null)
                AssertState(state);
            TransformCalled = true;
        }
    }

    [TestClass]
    public class MethodOptionTests
    {
        private static readonly ITransformMethod fooTestRecompiler = new OptionVerifier("FooTest");
        private static readonly ITransformMethod barTestRecompiler = new OptionVerifier("BarTest");
        private static readonly ITransformMethod yafTestRecompiler = new OptionVerifier("YafTest");

        [TestInitialize]
        public void Init()
        {
            FtlConfiguration.TransformMethods.Register(fooTestRecompiler);
            FtlConfiguration.TransformMethods.Register(barTestRecompiler);
            FtlConfiguration.TransformMethods.Register(yafTestRecompiler);
        }

        [TestCleanup]
        public void Cleanup()
        {
            FtlConfiguration.TransformMethods.Unregister(fooTestRecompiler);
            FtlConfiguration.TransformMethods.Unregister(barTestRecompiler);
            FtlConfiguration.TransformMethods.Unregister(yafTestRecompiler);
        }

        [TestMethod]
        public void MethodOption_NoOptions()
        {
            Ftl.Compile((int[] source) => source.FooTest(new string[0]).ToList());
            Ftl.Compile((int[] source) => source.BarTest(new string[0]).ToList());
            Ftl.Compile((int[] source) => source.YafTest(new string[0]).ToList());
        }

        [TestMethod]
        public void MethodOptionConstrainedByClassAndName_SetOnRightMethod()
        {
            Ftl.Compile((int[] source) => source
                .FooTest_OptionWithClassAndName()
                .FooTest(new[] {"FooTest_OptionWithClassAndName"})
                .ToList());
        }

        [TestMethod]
        public void MethodOptionConstrainedByName_SetOnRightMethod()
        {
            Ftl.Compile((int[] source) => source
                .FooTest_OptionWithName()
                .FooTest(new[] {"FooTest_OptionWithName"})
                .ToList());
        }

        [TestMethod]
        public void MethodOptionConstrainedByClass_SetOnRightMethod()
        {
            Ftl.Compile((int[] source) => source
                .Test_OptionWithClass()
                .FooTest(new[] { "Test_OptionWithClass" })
                .ToList());
        }

        [TestMethod]
        public void MethodOptionConstrainedByClassAndName_SetOnWrongMethod_Throws()
        {
            FtlAssert.Throws<FtlException>(() =>
                Ftl.Compile((int[] source) => source
                    .FooTest_OptionWithClassAndName()
                    .BarTest(new[] { "FooTest_OptionWithClassAndName" })
                    .ToList()));
        }

        [TestMethod]
        public void MethodOptionConstrainedByName_SetOnWrongMethod_Throws()
        {
            FtlAssert.Throws<FtlException>(() =>
                Ftl.Compile((int[] source) => source
                    .FooTest_OptionWithName()
                    .BarTest(new[] { "FooTest_OptionWithName" })
                    .ToList()));
        }

        [TestMethod]
        public void MethodOptionConstrainedByClass_SetOnWrongMethod_Throws()
        {
            FtlAssert.Throws<FtlException>(() =>
                Ftl.Compile((int[] source) => AlternativeOptionsExtensions.FooTest(
                        source.Test_OptionWithClass(), new[] { "Test_OptionWithClass" })
                    .ToList()));
        }

        [TestMethod]
        public void MethodOption_MethodAgnosticOption()
        {
            Ftl.Compile((int[] source) => source
                .GenericLocalOption()
                .FooTest(new[] { "GenericLocalOption" })
                .GenericLocalOption()
                .BarTest(new[] { "GenericLocalOption" })
                .ToList());
        }

        [TestMethod]
        public void MethodOptionSpecificToMultipleMethods_SetToOneOfThose()
        {
            Ftl.Compile((int[] source) => source
                .OptionForSeveralMethods()
                .FooTest(new[] { "OptionForSeveralMethods" })
                .ToList());

            Ftl.Compile((int[] source) => source
                .OptionForSeveralMethods()
                .BarTest(new[] { "OptionForSeveralMethods" })
                .ToList());

            FtlAssert.Throws<FtlException>(() =>
                Ftl.Compile((int[] source) => source
                    .OptionForSeveralMethods()
                    .YafTest(new[] { "OptionForSeveralMethods" })
                    .ToList()));
        }

        [TestMethod]
        public void MethodOption_SeveralOptions()
        {
            Ftl.Compile((int[] source) => source
                .FooTest_OptionWithClassAndName()
                .FooTest_OptionWithName()
                .Test_OptionWithClass()
                .FooTest(new[] { "FooTest_OptionWithClassAndName", "FooTest_OptionWithName", "Test_OptionWithClass" })
                .ToList());
        }

        [TestMethod]
        public void MethodOption_GlobalOption()
        {
            Ftl.Compile((int[] source) => source
                .GlobalOption()
                .GenericLocalOption()
                .FooTest(new[] { "GenericLocalOption", "GlobalOption" })
                .GenericLocalOption()
                .BarTest(new[] { "GenericLocalOption", "GlobalOption" })
                .ToList());

            Ftl.Compile((int[] source) => source
                .GenericLocalOption()
                .FooTest(new[] { "GenericLocalOption" })
                .GlobalOption()
                .FooTest_OptionWithName()
                .FooTest(new[] { "FooTest_OptionWithName", "GlobalOption" })
                .GenericLocalOption()
                .BarTest(new[] { "GenericLocalOption", "GlobalOption" })
                .ToList());
        }

        [TestMethod]
        public void MethodOptionBothGlobalAndLocal_Throws()
        {
            FtlAssert.Throws<FtlException>(() =>
                Ftl.Compile((int[] source) => source
                    .GlobalAndLocalOption()
                    .FooTest(new[] { "GlobalAndLocalOption" })
                    .ToList()));
        }

        [TestMethod]
        public void MethodOptionGlobalAndSpecific_Throws()
        {
            FtlAssert.Throws<FtlException>(() =>
                Ftl.Compile((int[] source) => source
                    .SpecificGlobalOption()
                    .FooTest(new[] { "SpecificGlobalOption" })
                    .ToList()));
        }
    }

    [TestClass]
    public class MethodOptionApiTests
    {
        private OptionVerifier transform;

        [TestCleanup]
        public void Cleanup()
        {
            if (transform != null)
            {
                FtlConfiguration.TransformMethods.Unregister(transform);
                transform = null;
            }
        }

        [TestMethod]
        public void RecompilationState_GetOptions()
        {
            transform = new OptionVerifier("FooTest")
                {
                    AssertState = state =>
                        {
                            Assert.AreEqual(1, state.GetOptions(e => e.GenericLocalOption()).Count());
                            Assert.AreEqual(0, state.GetOptions(e => e.FooTest_OptionWithName()).Count());
                        }
                };
            FtlConfiguration.TransformMethods.Register(transform);

            Ftl.Compile((int[] source) => source.GenericLocalOption().FooTest(new[] { "GenericLocalOption" }).ToList());
            Assert.IsTrue(transform.TransformCalled);
        }
    }
}
