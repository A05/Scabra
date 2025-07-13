using ProtoBuf;
using System.Collections.Generic;

namespace Scabra.Rpc
{
    public partial class MarshallingTests
    {
        [ProtoContract]
        internal class ComplexEntity
        {
            [ProtoMember(1)]
            public int IntValue { get; set; }

            [ProtoMember(2)]
            public string StringValue { get; set; }

            [ProtoMember(3)]
            public int[] ArrayOfInts { get; set; }

            [ProtoMember(4)]
            public List<int> ListOfInts { get; set; }

            [ProtoMember(5)]
            public IEnumerable<int> IEnumerableOfInts { get; set; }

            [ProtoMember(6)]
            public IList<string> IListOfStrings { get; set; }

            [ProtoMember(7)]
            public IEnumerable<string> IEnumerableOfStrings { get; set; }

            [ProtoMember(8)]
            public Dictionary<int, int> DictionaryValue { get; set; }

            [ProtoMember(9)]
            public ComplexEntity NullValue { get; set; }
        }

        internal interface IMarshallingTestService
        {
            void NoParametersAndNoReturnValue();

            int PrimitiveParametersAndPrimitiveReturnValue(int p1, int p2, int p3, int p4, int p5);

            int? PrimitiveNullableParametersAndNullablePrimitiveReturnValue(int? p1, int? p2, int? p3, int? p4, int? p5);

            ComplexEntity ComplexParametersAndComplextReturnValue(
                ComplexEntity p1,
                ComplexEntity p2,
                ComplexEntity p3,
                ComplexEntity p4,
                ComplexEntity p5);
        }

        internal class MarshallingTestService : IMarshallingTestService
        {
            public bool IsNoParametersAndNoReturnValueCalled;
            public bool IsPrimitiveParametersAndPrimitiveReturnValueCalled;
            public bool IsPrimitiveNullableParametersAndNullablePrimitiveReturnValueCalled;
            public bool IsComplexParametersAndComplextReturnValueCalled;

            public int?[] PrimitiveArguments = new int?[5];
            public ComplexEntity[] ComplexArguments = new ComplexEntity[5];

            public void NoParametersAndNoReturnValue() 
            {
                IsNoParametersAndNoReturnValueCalled = true;
            }

            public int PrimitiveParametersAndPrimitiveReturnValue(int p1, int p2, int p3, int p4, int p5)
            {
                IsPrimitiveParametersAndPrimitiveReturnValueCalled = true;

                PrimitiveArguments[0] = p1;
                PrimitiveArguments[1] = p2;
                PrimitiveArguments[2] = p3;
                PrimitiveArguments[3] = p4;
                PrimitiveArguments[4] = p5;

                return p3;
            }

            public int? PrimitiveNullableParametersAndNullablePrimitiveReturnValue(int? p1, int? p2, int? p3, int? p4, int? p5)
            {
                IsPrimitiveNullableParametersAndNullablePrimitiveReturnValueCalled = true;

                PrimitiveArguments[0] = p1;
                PrimitiveArguments[1] = p2;
                PrimitiveArguments[2] = p3;
                PrimitiveArguments[3] = p4;
                PrimitiveArguments[4] = p5;

                return p3;
            }

            public ComplexEntity ComplexParametersAndComplextReturnValue(
                ComplexEntity p1, 
                ComplexEntity p2, 
                ComplexEntity p3, 
                ComplexEntity p4, 
                ComplexEntity p5)
            {
                IsComplexParametersAndComplextReturnValueCalled = true;

                ComplexArguments[0] = p1;
                ComplexArguments[1] = p2;
                ComplexArguments[2] = p3;
                ComplexArguments[3] = p4;
                ComplexArguments[4] = p5;

                return p3;
            }
        }

        internal interface IMarshallingTestServiceWithMaxParametersNumberExceeded
        {
            void SomeMethod(
                int p1, int p2, int p3, int p4, int p5, int p6, int p7, int p8,
                int p9, int p10, int p11, int p12, int p13, int p14, int p15, int p16,
                int p17);
        }

        internal class MarshallingTestServiceWithMaxParametersNumberExceeded : IMarshallingTestServiceWithMaxParametersNumberExceeded
        {
            public void SomeMethod(int p1, int p2, int p3, int p4, int p5, int p6, int p7, int p8, int p9, int p10, int p11, int p12, int p13, int p14, int p15, int p16, int p17) {}
        }

        internal interface IMarshallingTestServiceWithOverloadedMethods
        {
            void SomeMethod(int p1);
            void SomeMethod(int p1, int p2);
        }

        internal class MarshallingTestServiceWithOverloadedMethods : IMarshallingTestServiceWithOverloadedMethods
        {
            public void SomeMethod(int p1) {}
            public void SomeMethod(int p1, int p2) {}
        }

        // Interface hierarchy

        internal interface IMarshallingTestService1 { void SomeMethod1(); }
        internal interface IMarshallingTestService2 : IMarshallingTestService1 { void SomeMethod2(); }
        internal interface IMarshallingTestService3 { void SomeMethod3(); }

        internal class MarshallingTestServiceWithInterfaceHierarchy : IMarshallingTestService2, IMarshallingTestService3
        {
            public void SomeMethod1() { }
            public void SomeMethod2() { }
            public void SomeMethod3() { }
        }
    }
}
