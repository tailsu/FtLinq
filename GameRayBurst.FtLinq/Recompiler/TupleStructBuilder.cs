using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using GameRayBurst.FtLinq.Recompiler.Util;

namespace GameRayBurst.FtLinq.Recompiler
{
    internal struct MyTuple : IComparable<MyTuple>
    {
        public static IComparer<string> SecondaryComparer;

        public int Primary;
        public string Secondary;

        public int CompareTo(MyTuple other)
        {
            if (Primary != other.Primary)
                return Primary - other.Primary; // e.g. descending default sort
            return SecondaryComparer.Compare(Secondary, other.Secondary);
        }
    }

    internal enum TupleFieldRole
    {
        SortAscending,
        SortDescending,
        Value,
    }

    internal class TupleStructBuilder
    {
        private static AssemblyBuilder theAssemblyBuilder;
        private static ModuleBuilder theModuleBuilder;
        private static int tupleCounter;
        private static readonly object sync = new object();

        private readonly TypeBuilder myTupleStruct;

        private class FieldSortInfo
        {
            public FieldBuilder Field;
            public FieldBuilder Comparer;
            public TupleFieldRole Role;
            public bool UseDefaultComparer;
        }

        private readonly List<FieldSortInfo> myFields = new List<FieldSortInfo>();

        private Type myConstructedType;

        public TupleStructBuilder()
        {
            lock (sync)
            {
                if (theAssemblyBuilder == null)
                {
#if DEBUG
                    theAssemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("GameRayBurst.FtLinq.Runtime"), AssemblyBuilderAccess.RunAndSave);
                    theModuleBuilder = theAssemblyBuilder.DefineDynamicModule("GameRayBurst.FtLinq.Runtime", "GameRayBurst.FtLinq.Runtime.dll");
#else
                    theAssemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("GameRayBurst.FtLinq.Runtime"), AssemblyBuilderAccess.Run);
                    theModuleBuilder = theAssemblyBuilder.DefineDynamicModule("GameRayBurst.FtLinq.Runtime");
#endif
                }

                myTupleStruct = theModuleBuilder.DefineType(
                    "TupleStruct_" + tupleCounter,
                    TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed, typeof (ValueType));
                tupleCounter++;
            }
        }

        public void AddField(string fieldName, Type fieldType, Type customComparer, TupleFieldRole role)
        {
            var field = myTupleStruct.DefineField(fieldName, fieldType, FieldAttributes.Public);
            var info = new FieldSortInfo
                {
                    Field = field,
                    Role = role,
                    UseDefaultComparer = customComparer == null,
                };

            if (customComparer == null && !fieldType.GetInterfaces().Contains(typeof (IComparable<>).MakeGenericType(fieldType)))
            {
                customComparer = typeof (IComparer<>).MakeGenericType(fieldType);
            }

            if (customComparer != null)
            {
                var comparerField = myTupleStruct.DefineField(fieldName + "_Comparer",
                    customComparer, FieldAttributes.Public | FieldAttributes.Static);
                info.Comparer = comparerField;
            }

            myFields.Add(info);
        }

        public Type CreateType()
        {
            if (myConstructedType == null)
            {
                CompleteType();
                myConstructedType = myTupleStruct.CreateType();
            }

            return myConstructedType;
        }

        private void CompleteType()
        {
            var comparableIntfType = typeof (IComparable<>).MakeGenericType(myTupleStruct);
            myTupleStruct.AddInterfaceImplementation(comparableIntfType);
            var compareMethod = myTupleStruct.DefineMethod("CompareTo",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.NewSlot | MethodAttributes.HideBySig,
                typeof(int), new[] { myTupleStruct });

            var il = compareMethod.GetILGenerator();
            il.DeclareLocal(typeof (int)); // local 0
            var valuesUnequal = il.DefineLabel();

            int lastSortFieldIndex = myFields.LastIndexOf(fsi => fsi.Role != TupleFieldRole.Value);

            for (int fieldIdx = 0; fieldIdx < myFields.Count; ++fieldIdx)
            {
                var fieldInfo = myFields[fieldIdx];
                if (fieldInfo.Role == TupleFieldRole.Value)
                    continue;

                var descending = fieldInfo.Role == TupleFieldRole.SortDescending;
                var comparer = fieldInfo.Comparer;
                var field = fieldInfo.Field;
                bool isLast = fieldIdx == lastSortFieldIndex;

                if (comparer == null)
                {
                    if (!TryEmitSubtractionComparison(il, field, descending))
                    {
                        EmitIComparableComparison(il, field, descending);
                    }
                }
                else
                {
                    EmitIComparerComparison(il, field, descending, comparer);
                }
                il.Emit(OpCodes.Stloc_0);

                if (!isLast)
                {
                    il.Emit(OpCodes.Ldloc_0);
                    il.Emit(OpCodes.Brtrue, valuesUnequal);
                }
                else
                {
                    il.MarkLabel(valuesUnequal);
                    il.Emit(OpCodes.Ldloc_0);
                    il.Emit(OpCodes.Ret);
                }
            }
        }

        private bool TryEmitSubtractionComparison(ILGenerator il, FieldBuilder field, bool descending)
        {
            var type = field.FieldType;
            if (type == typeof(byte) || type == typeof(sbyte)
                || type == typeof(short) || type == typeof(ushort)
                || type == typeof(char)
                || type == typeof(int) || type == typeof(uint)
                || type == typeof(float) || type == typeof(double))
            {
                EmitLoad(il, true, descending);
                il.Emit(OpCodes.Ldfld, field);
                EmitLoad(il, false, descending);
                il.Emit(OpCodes.Ldfld, field);
                il.Emit(OpCodes.Sub);
                return true;
            }

            return false;
        }

        private void EmitIComparableComparison(ILGenerator il, FieldBuilder field, bool descending)
        {
            var fieldType = field.FieldType;
            var comparableType = typeof(IComparable<>).MakeGenericType(field.FieldType);
            var compareToIntf = comparableType.GetMethod("CompareTo");
            var interfaceMapping = fieldType.GetInterfaceMap(comparableType);
            var compareToIdx = Array.IndexOf(interfaceMapping.InterfaceMethods, compareToIntf);
            var compareToImpl = interfaceMapping.TargetMethods[compareToIdx];

            EmitLoad(il, true, descending);
            EmitLoadThisForCall(il, field, compareToImpl);
            EmitLoad(il, false, descending);
            il.Emit(OpCodes.Ldfld, field);

            //TODO: use Call instead of Callvirt when it's safe
            il.Emit(OpCodes.Callvirt, compareToImpl.IsPublic ? compareToImpl : compareToIntf);
        }

        private void EmitIComparerComparison(ILGenerator il, FieldBuilder field, bool descending, FieldBuilder comparer)
        {
            var icomparer = typeof (IComparer<>).MakeGenericType(field.FieldType);
            var compareToIntf = icomparer.GetMethod("Compare");
            var compareToImpl = compareToIntf;
            if (!comparer.FieldType.IsInterface)
            {
                var interfaceMapping = comparer.FieldType.GetInterfaceMap(icomparer);
                var compareToIdx = Array.IndexOf(interfaceMapping.InterfaceMethods, compareToIntf);
                compareToImpl = interfaceMapping.TargetMethods[compareToIdx];
            }

            EmitLoadThisForCall(il, comparer, compareToImpl);
            EmitLoad(il, true, descending);
            il.Emit(OpCodes.Ldfld, field);
            EmitLoad(il, false, descending);
            il.Emit(OpCodes.Ldfld, field);

            //TODO: use Call instead of Callvirt when it's safe
            il.Emit(OpCodes.Callvirt, compareToImpl.IsPublic ? compareToImpl : compareToIntf);
        }

        private void EmitLoadThisForCall(ILGenerator il, FieldBuilder fieldThis, MethodInfo method)
        {
            var isStatic = fieldThis.IsStatic;
            if (!method.IsPublic || !fieldThis.FieldType.IsValueType)
            {
                il.Emit(isStatic ? OpCodes.Ldsfld : OpCodes.Ldfld, fieldThis);
                if (fieldThis.FieldType.IsValueType)
                    il.Emit(OpCodes.Box, fieldThis.FieldType);
            }
            else
            {
                il.Emit(isStatic ? OpCodes.Ldsflda : OpCodes.Ldflda, fieldThis);
            }
        }

        private void EmitLoad(ILGenerator il, bool loadFirst, bool descending)
        {
            if (loadFirst == !descending)
                il.Emit(OpCodes.Ldarg_0);
            else
                il.Emit(OpCodes.Ldarga_S, 1);
        }

        public void Dump()
        {
#if DEBUG
            theAssemblyBuilder.Save("GameRayBurst.FtLinq.Runtime.dll");
#endif
        }

        public Expression CreateIEnumerableSortExpression(Type sourceElementType, ParameterExpression valuesSource,
            string valueFieldName, Func<ParameterExpression, Dictionary<string, Expression>> fieldInitializerFunc)
        {
            var listType = typeof (List<>).MakeGenericType(myConstructedType);
            var compositeKeyList = Expression.Variable(listType, "compositeKeyList");
            var elementVar = Expression.Variable(myConstructedType, "tupleElement");

            var forEachState = ExpressionUtil.CreateForEachIterationState(valuesSource, false);
            var fieldInitializers = fieldInitializerFunc(forEachState.ItemVariable);

            var initList = new List<Expression>();
            foreach (var initializerKvp in fieldInitializers)
            {
                var fieldName = initializerKvp.Key;
                var initializer = initializerKvp.Value;
                var field = myConstructedType.GetField(fieldName);
                initList.Add(Expression.Assign(Expression.Field(elementVar, field), initializer));
            }

            var initLoop = ExpressionUtil.ForEach(forEachState,
                Expression.Block(new[] { elementVar },
                    Expression.Assign(elementVar, Expression.Default(myConstructedType)),
                    Expression.Block(initList),
                    Expression.Assign(Expression.Field(elementVar, valueFieldName), forEachState.ItemVariable),
                    Expression.Call(compositeKeyList, listType.GetMethod("Add"), elementVar)
                ));

            var sortMethod = listType.GetMethodLike("Sort", typeof(IComparer<>).MakeGenericType(myConstructedType));

            var resultVar = Expression.Variable(sourceElementType.MakeArrayType(), "resultVar");
            var iVar = Expression.Variable(typeof (int), "i");
            var valueProjection = ExpressionUtil.For(
                new[] {iVar},
                Expression.Assign(iVar, Expression.Constant(0)),
                Expression.LessThan(iVar, Expression.ArrayLength(resultVar)),
                Expression.PreIncrementAssign(iVar),
                null, null,
                Expression.Assign(
                    Expression.ArrayAccess(resultVar, iVar),
                    Expression.Field(Expression.MakeIndex(compositeKeyList, listType.GetIndexer(), new[] {iVar}), valueFieldName)));

            return Expression.Block(new[] { resultVar, compositeKeyList },
                Expression.Assign(compositeKeyList, Expression.New(listType)),
                initLoop,
                Expression.Call(compositeKeyList, sortMethod, Expression.Constant(null, sortMethod.GetParameters().First().ParameterType)),
                Expression.Assign(resultVar, Expression.NewArrayBounds(sourceElementType, Expression.Property(compositeKeyList, "Count"))),
                valueProjection,
                resultVar
                );
        }

        public Expression CreateComparerInitializationExpression(Dictionary<string, Expression> comparers)
        {
            var assignments = new List<Expression>();
            foreach (var fi in myFields)
            {
                Expression comparer;
                if (fi.Comparer == null)
                    continue;

                if (!comparers.TryGetValue(fi.Field.Name, out comparer))
                {
                    if (!fi.UseDefaultComparer)
                        throw new FtlException(Expression.Empty(), "No comparer specified for field.");
                    comparer = Expression.Property(null, typeof(Comparer<>).MakeGenericType(fi.Field.FieldType), "Default");
                }

                var comparerField = myConstructedType.GetField(fi.Comparer.Name);
                assignments.Add(Expression.Assign(Expression.Field(null, comparerField), comparer));
            }

            return assignments.Count > 0 ? (Expression) Expression.Block(assignments) : Expression.Empty();
        }
    }
}
