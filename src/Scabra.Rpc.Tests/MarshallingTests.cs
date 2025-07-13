using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Scabra.Rpc.Client;
using Scabra.Rpc.Server;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Scabra.Rpc
{
    [TestFixture]
    public partial class MarshallingTests
    {
        [Test]
        public void should_not_register_service_if_it_doesnot_implement_interface()
        {
            var sMarshaller = CreateServerMarshaller();

            var ex = Assert.Throws<RpcScabraException>(() =>
                sMarshaller.RegisterService(typeof(IMarshallingTestService), new object()));

            Assert.That(ex.ErrorCode, Is.EqualTo(RpcErrorCode.InvalidArgument));
            Assert.That(ex.Message, Does.StartWith("The provided service is not"));
        }

        [Test]
        public void should_not_register_service_if_max_number_of_parameters_is_exceeded()
        {
            var sMarshaller = CreateServerMarshaller();
            var service = new MarshallingTestServiceWithMaxParametersNumberExceeded();

            var ex = Assert.Throws<RpcScabraException>(() =>
                sMarshaller.RegisterService(typeof(IMarshallingTestServiceWithMaxParametersNumberExceeded), service));

            Assert.That(ex.ErrorCode, Is.EqualTo(RpcErrorCode.Unimplemented));
            Assert.That(ex.Message, Does.StartWith("Maximum number"));
        }

        [Test]
        public void should_not_register_service_with_overloaded_methods()
        {
            var sMarshaller = CreateServerMarshaller();
            var service = new MarshallingTestServiceWithOverloadedMethods();

            var ex = Assert.Throws<RpcScabraException>(() =>
                sMarshaller.RegisterService(typeof(IMarshallingTestServiceWithOverloadedMethods), service));

            Assert.That(ex.ErrorCode, Is.EqualTo(RpcErrorCode.Unimplemented));
            Assert.That(ex.Message, Does.StartWith("Method overloading"));
        }

        [Test]
        public void should_not_register_service_if_its_contract_is_not_defined_by_interface()
        {
            var sMarshaller = CreateServerMarshaller();
            var service = new MarshallingTestServiceWithOverloadedMethods();

            var ex = Assert.Throws<RpcScabraException>(() =>
                sMarshaller.RegisterService(typeof(MarshallingTestServiceWithOverloadedMethods), service));

            Assert.That(ex.ErrorCode, Is.EqualTo(RpcErrorCode.Unimplemented));
            Assert.That(ex.Message, Does.Contain("must be defined by an interface"));
        }

        [Test]
        public void should_register_service_methods_of_all_inteface_hierarchy()
        {
            var cMarshaller = CreateClientMarshaller();
            var sMarshaller = CreateServerMarshaller();
            var service = new MarshallingTestServiceWithInterfaceHierarchy();

            sMarshaller.RegisterService(typeof(IMarshallingTestService2), service);

            var callData = cMarshaller.MarshalCall<IMarshallingTestService2, Marshaller.Void>(
                nameof(IMarshallingTestService2.SomeMethod1), new object[0]);

            Assert.That(callData, Is.Not.Null);

            sMarshaller.UnmarshalCall(callData, out var invoker, out var args);

            callData = cMarshaller.MarshalCall<IMarshallingTestService2, Marshaller.Void>(
                nameof(IMarshallingTestService2.SomeMethod2), new object[0]);

            Assert.That(callData, Is.Not.Null);

            sMarshaller.UnmarshalCall(callData, out invoker, out args);

            var ex = Assert.Throws<RpcScabraException>(() =>
                cMarshaller.MarshalCall<IMarshallingTestService2, Marshaller.Void>(
                    nameof(IMarshallingTestService3.SomeMethod3), new object[0]));

            Assert.That(ex.ErrorCode, Is.EqualTo(RpcErrorCode.NotFound));
        }

        [Test]
        public void should_marshal_call()
        {
            var cMarshaller = CreateClientMarshaller();
            var sMarshaller = CreateServerMarshaller();
            var service = new MarshallingTestService();

            sMarshaller.RegisterService(typeof(IMarshallingTestService), service);

            //
            // No Parameters And No Return Value
            //

            var callData = cMarshaller.MarshalCall<IMarshallingTestService, Marshaller.Void>(
                nameof(IMarshallingTestService.NoParametersAndNoReturnValue),
                new object[0]);

            Assert.That(callData, Is.Not.Null);

            sMarshaller.UnmarshalCall(callData, out var invoker, out var args);

            Assert.That(invoker, Is.Not.Null);
            Assert.That(args, Is.Not.Null.And.Empty);

            var reply = invoker.Invoke(args);

            Assert.That(service.IsNoParametersAndNoReturnValueCalled, Is.True);
            Assert.That(reply, Is.Null);            

            //
            // Primitive Parameters And Primitive Return Value
            //

            callData = cMarshaller.MarshalCall<IMarshallingTestService, int>(
                nameof(IMarshallingTestService.PrimitiveParametersAndPrimitiveReturnValue),
                new object[] { 21, 22, 23, 24, 25 });

            Assert.That(callData, Is.Not.Null);

            sMarshaller.UnmarshalCall(callData, out invoker, out args);

            Assert.That(invoker, Is.Not.Null);
            Assert.That(args, Is.Not.Null.And.Length.EqualTo(5));

            reply = invoker.Invoke(args);

            Assert.That(service.IsPrimitiveParametersAndPrimitiveReturnValueCalled, Is.True);
            Assert.That(service.PrimitiveArguments[0].GetValueOrDefault(), Is.EqualTo(21));
            Assert.That(service.PrimitiveArguments[1].GetValueOrDefault(), Is.EqualTo(22));
            Assert.That(service.PrimitiveArguments[2].GetValueOrDefault(), Is.EqualTo(23));
            Assert.That(service.PrimitiveArguments[3].GetValueOrDefault(), Is.EqualTo(24));
            Assert.That(service.PrimitiveArguments[4].GetValueOrDefault(), Is.EqualTo(25));
            Assert.That(reply, Is.Not.Null.And.TypeOf<int>()); // the third arg
            Assert.That((int)reply, Is.EqualTo(23));

            //
            // Primitive NullableParameters And NullablePrimitive Return Value
            //

            callData = cMarshaller.MarshalCall<IMarshallingTestService, int?>(
                nameof(IMarshallingTestService.PrimitiveNullableParametersAndNullablePrimitiveReturnValue),
                new object[] { null, 22, null, 24, null });

            Assert.That(callData, Is.Not.Null);

            sMarshaller.UnmarshalCall(callData, out invoker, out args);

            Assert.That(invoker, Is.Not.Null);
            Assert.That(args, Is.Not.Null.And.Length.EqualTo(5));

            reply = invoker.Invoke(args);

            Assert.That(service.IsPrimitiveNullableParametersAndNullablePrimitiveReturnValueCalled, Is.True);
            Assert.That(service.PrimitiveArguments[0].HasValue, Is.False);
            Assert.That(service.PrimitiveArguments[1].GetValueOrDefault(), Is.EqualTo(22));
            Assert.That(service.PrimitiveArguments[2].HasValue, Is.False);
            Assert.That(service.PrimitiveArguments[3].GetValueOrDefault(), Is.EqualTo(24));
            Assert.That(service.PrimitiveArguments[4].HasValue, Is.False);
            Assert.That(reply, Is.Null); // the third arg

            //
            // Complex Parameters And Complext Return Value
            //

            callData = cMarshaller.MarshalCall<IMarshallingTestService, ComplexEntity>(
                nameof(IMarshallingTestService.ComplexParametersAndComplextReturnValue),
                new ComplexEntity[] { 
                    new() { IntValue = 21 }, 
                    null,
                    new() { IntValue = 23 }, 
                    null,
                    new() { IntValue = 25 } });

            Assert.That(callData, Is.Not.Null);

            sMarshaller.UnmarshalCall(callData, out invoker, out args);

            Assert.That(invoker, Is.Not.Null);
            Assert.That(args, Is.Not.Null.And.Length.EqualTo(5));

            reply = invoker.Invoke(args);

            Assert.That(service.IsComplexParametersAndComplextReturnValueCalled, Is.True);
            Assert.That(service.ComplexArguments[0], Is.Not.Null);
            Assert.That(service.ComplexArguments[0].IntValue, Is.EqualTo(21));
            Assert.That(service.ComplexArguments[1], Is.Null);            
            Assert.That(service.ComplexArguments[2], Is.Not.Null);
            Assert.That(service.ComplexArguments[2].IntValue, Is.EqualTo(23));
            Assert.That(service.ComplexArguments[3], Is.Null);
            Assert.That(service.ComplexArguments[4], Is.Not.Null);
            Assert.That(service.ComplexArguments[4].IntValue, Is.EqualTo(25));
            Assert.That(reply, Is.Not.Null.And.TypeOf<ComplexEntity>()); // the third arg
            Assert.That(((ComplexEntity)reply).IntValue, Is.EqualTo(23));
        }

        [Test]
        public void should_not_marshal_call_if_service_is_not_registered()
        {
            var cMarshaller = CreateClientMarshaller();
            var sMarshaller = CreateServerMarshaller();
            var service = new MarshallingTestService();

            var callData = cMarshaller.MarshalCall<IMarshallingTestService, Marshaller.Void>(
                nameof(IMarshallingTestService.NoParametersAndNoReturnValue),
                new object[0]);

            Assert.That(callData, Is.Not.Null);

            var ex = Assert.Throws<RpcScabraException>(() =>
                sMarshaller.UnmarshalCall(callData, out var invoker, out var args));

            Assert.That(ex.ErrorCode, Is.EqualTo(RpcErrorCode.NotFound));
            Assert.That(ex.Message, Does.EndWith("is not registered."));
        }

        [Test]
        public void should_not_marshal_call_if_method_does_not_exist()
        {
            var cMarshaller = CreateClientMarshaller();
            var sMarshaller = CreateServerMarshaller();
            var service = new MarshallingTestService();

            sMarshaller.RegisterService(typeof(IMarshallingTestService), service);

            // Catch missing method on the client side.

            var ex = Assert.Throws<RpcScabraException>(() =>
                cMarshaller.MarshalCall<IMarshallingTestService, int>(
                    "NameOfMethodThatDoesNotExist",
                    new object[0]));

            Assert.That(ex.ErrorCode, Is.EqualTo(RpcErrorCode.NotFound));
            Assert.That(ex.Message, Does.Contain("not found in"));

            // Catch missing method on the server side.

            var callData = cMarshaller.MarshalCallInternal(
                typeof(IMarshallingTestService).FullName,
                "NameOfMethodThatDoesNotExist",
                new object[0]);

            Assert.That(callData, Is.Not.Null);

            ex = Assert.Throws<RpcScabraException>(() =>
                sMarshaller.UnmarshalCall(callData, out var invoker, out var args));

            Assert.That(ex.ErrorCode, Is.EqualTo(RpcErrorCode.NotFound));
            Assert.That(ex.Message, Does.EndWith("is not registered."));
        }

        [Test]
        public void should_not_marshal_call_if_args_length_does_not_match()
        {
            var cMarshaller = CreateClientMarshaller();
            var sMarshaller = CreateServerMarshaller();
            var service = new MarshallingTestService();

            sMarshaller.RegisterService(typeof(IMarshallingTestService), service);

            // args length is less than declared

            var ex = Assert.Throws<RpcScabraException>(() => 
                cMarshaller.MarshalCall<IMarshallingTestService, int>(
                    nameof(IMarshallingTestService.PrimitiveParametersAndPrimitiveReturnValue),
                    new object[0]));

            Assert.That(ex.ErrorCode, Is.EqualTo(RpcErrorCode.InvalidArgument));
            Assert.That(ex.Message, Does.StartWith("Number of arguments"));

            // args length is greater than declared

            ex = Assert.Throws<RpcScabraException>(() => 
                cMarshaller.MarshalCall<IMarshallingTestService, int>(
                    nameof(IMarshallingTestService.PrimitiveParametersAndPrimitiveReturnValue),
                    new object[] { 1, 2, 3, 4, 5, 6 }));

            Assert.That(ex.ErrorCode, Is.EqualTo(RpcErrorCode.InvalidArgument));
            Assert.That(ex.Message, Does.StartWith("Number of arguments"));
        }

        [Test]
        public void should_not_marshal_call_if_args_types_do_not_match()
        {
            var cMarshaller = CreateClientMarshaller();
            var sMarshaller = CreateServerMarshaller();
            var service = new MarshallingTestService();

            sMarshaller.RegisterService(typeof(IMarshallingTestService), service);

            // args length is less than declared

            var callData = cMarshaller.MarshalCall<IMarshallingTestService, int>(
                nameof(IMarshallingTestService.PrimitiveParametersAndPrimitiveReturnValue),
                new object[] { 1M, 2, 3, 4, 5 });

            Assert.That(callData, Is.Not.Null);

            var ex = Assert.Throws<RpcScabraException>(() =>
                sMarshaller.UnmarshalCall(callData, out var invoker, out var args));

            Assert.That(ex.ErrorCode, Is.EqualTo(RpcErrorCode.Unimplemented));
            Assert.That(ex.Message, Does.StartWith("Failed to deserialize"));
        }

        [Test]
        public void should_not_marshal_call_if_max_number_of_args_is_exceeded_in_client()
        {
            var cMarshaller = CreateClientMarshaller();
            var sMarshaller = CreateServerMarshaller();
            var service = new MarshallingTestService();

            sMarshaller.RegisterService(typeof(IMarshallingTestService), service);

            var exceededLengthArgs = new object[Marshaller.MaxArgsLength + 1];

            var ex = Assert.Throws<RpcScabraException>(() => 
                cMarshaller.MarshalCall<IMarshallingTestService, int>(
                    nameof(IMarshallingTestService.PrimitiveParametersAndPrimitiveReturnValue),
                    exceededLengthArgs));

            Assert.That(ex.ErrorCode, Is.EqualTo(RpcErrorCode.InvalidArgument));
            Assert.That(ex.Message, Does.StartWith("Maximum number"));
        }

        [Test]
        public void should_marshal_reply()
        {
            var cMarshaller = CreateClientMarshaller();
            var sMarshaller = CreateServerMarshaller();

            //
            // Primitive type
            //

            var replyData = sMarshaller.MarshalReply(25, null);
            var intValue = cMarshaller.UnmarshalReply<int>(replyData);

            Assert.That(intValue, Is.EqualTo(25));

            //
            // Complex type
            //

            var complexEntity = new ComplexEntity()
            {
                IntValue = 21,
                StringValue = "some string value",
                ArrayOfInts = new int[] { 31, 32 },
                ListOfInts = new List<int>() { 41, 42 },
                IEnumerableOfInts = new int[] { 51, 52 },
                IListOfStrings = new List<string>() { "str 10", "str 20" },
                IEnumerableOfStrings = new string[] { "str 30", "str 40" },
                DictionaryValue = new Dictionary<int, int>() { { 61, 62 }, { 63, 64 } },
                NullValue = null
            };

            replyData = sMarshaller.MarshalReply(complexEntity, null);
            var complextEntityValue = cMarshaller.UnmarshalReply<ComplexEntity>(replyData);

            Assert.That(complextEntityValue, Is.Not.Null);
            Assert.That(complextEntityValue.IntValue, Is.EqualTo(21));
            Assert.That(complextEntityValue.StringValue, Is.EqualTo("some string value"));

            Assert.That(complextEntityValue.ArrayOfInts, Is.Not.Null.And.Length.EqualTo(2));
            Assert.That(complextEntityValue.ArrayOfInts[0], Is.EqualTo(31));
            Assert.That(complextEntityValue.ArrayOfInts[1], Is.EqualTo(32));

            Assert.That(complextEntityValue.ListOfInts, Is.Not.Null.And.Count.EqualTo(2));
            Assert.That(complextEntityValue.ListOfInts[0], Is.EqualTo(41));
            Assert.That(complextEntityValue.ListOfInts[1], Is.EqualTo(42));

            Assert.That(complextEntityValue.IEnumerableOfInts, Is.Not.Null);
            Assert.That(complextEntityValue.IEnumerableOfInts.Count(), Is.EqualTo(2));
            Assert.That(complextEntityValue.IEnumerableOfInts.ElementAt(0), Is.EqualTo(51));
            Assert.That(complextEntityValue.IEnumerableOfInts.ElementAt(1), Is.EqualTo(52));

            Assert.That(complextEntityValue.IListOfStrings, Is.Not.Null.And.Count.EqualTo(2));
            Assert.That(complextEntityValue.IListOfStrings[0], Is.EqualTo("str 10"));
            Assert.That(complextEntityValue.IListOfStrings[1], Is.EqualTo("str 20"));

            Assert.That(complextEntityValue.IEnumerableOfStrings, Is.Not.Null);
            Assert.That(complextEntityValue.IEnumerableOfStrings.Count(), Is.EqualTo(2));
            Assert.That(complextEntityValue.IEnumerableOfStrings.ElementAt(0), Is.EqualTo("str 30"));
            Assert.That(complextEntityValue.IEnumerableOfStrings.ElementAt(1), Is.EqualTo("str 40"));

            Assert.That(complextEntityValue.DictionaryValue, Is.Not.Null.And.Count.EqualTo(2));
            Assert.That(complextEntityValue.DictionaryValue.ContainsKey(61), Is.True);
            Assert.That(complextEntityValue.DictionaryValue[61], Is.EqualTo(62));
            Assert.That(complextEntityValue.DictionaryValue.ContainsKey(63), Is.True);
            Assert.That(complextEntityValue.DictionaryValue[63], Is.EqualTo(64));

            Assert.That(complextEntityValue.NullValue, Is.Null);
        }

        [Test]
        public void should_marshal_null_reply()
        {
            var cMarshaller = CreateClientMarshaller();
            var sMarshaller = CreateServerMarshaller();
                        
            var replyData = sMarshaller.MarshalReply(null, null);
            Assert.That(replyData, Is.Not.Null);

            var intValue = cMarshaller.UnmarshalReply<int?>(replyData);
            Assert.That(intValue, Is.Null);
            Assert.That(intValue.HasValue, Is.False);

            var ex = Assert.Throws<RpcScabraException>(() => cMarshaller.UnmarshalReply<float>(replyData));
            Assert.That(ex.ErrorCode, Is.EqualTo(RpcErrorCode.Unimplemented));
            Assert.That(ex.Message, Does.StartWith("Failed to deserialize"));

            var complexEntityValue = cMarshaller.UnmarshalReply<ComplexEntity>(replyData);
            Assert.That(complexEntityValue, Is.Null);

            var objValue = cMarshaller.UnmarshalReply<object>(replyData);
            Assert.That(objValue, Is.Null);
        }

        [Test]
        public void should_marshal_reply_with_unresolvable_type()
        {
            var cMarshaller = CreateClientMarshaller();
            var sMarshaller = CreateServerMarshaller();

            var reply = new object(); // Object type is unresolvable one.

            var replyData = sMarshaller.MarshalReply(reply, null);
            Assert.That(replyData, Is.Not.Null);

            var ex = Assert.Throws<RpcScabraException>(() => cMarshaller.UnmarshalReply<object>(replyData));
            Assert.That(ex.ErrorCode, Is.EqualTo(RpcErrorCode.Unimplemented));
            Assert.That(ex.Message, Is.EqualTo("Failed to serialize reply."));
            Assert.That(ex.InnerException, Is.Null);
        }

        [Test]
        public void should_marshal_reply_with_exception_and_without_reply()
        {
            var cMarshaller = CreateClientMarshaller();
            var sMarshaller = CreateServerMarshaller();

            var replyData = sMarshaller.MarshalReply(null, new InvalidOperationException("some description"));

            var ex = Assert.Throws<RpcScabraException>(() => cMarshaller.UnmarshalReply<int?>(replyData));
            Assert.That(ex.ErrorCode, Is.EqualTo(RpcErrorCode.Unknown));
            Assert.That(ex.Message, Is.EqualTo("Exception was thrown by handler."));
            Assert.That(ex.InnerException, Is.Null);

            replyData = sMarshaller.MarshalReply(null, new RpcScabraException(RpcErrorCode.InvalidArgument, "some description"));

            ex = Assert.Throws<RpcScabraException>(() => cMarshaller.UnmarshalReply<int?>(replyData));
            Assert.That(ex.ErrorCode, Is.EqualTo(RpcErrorCode.InvalidArgument));
            Assert.That(ex.Message, Is.EqualTo("some description"));
            Assert.That(ex.InnerException, Is.Null);
        }

        [Test]
        public void should_marshal_reply_with_exception_and_with_reply()
        {
            var cMarshaller = CreateClientMarshaller();
            var sMarshaller = CreateServerMarshaller();

            var replyData = sMarshaller.MarshalReply(new ComplexEntity(), new InvalidOperationException("some description"));

            var ex = Assert.Throws<RpcScabraException>(() => cMarshaller.UnmarshalReply<int?>(replyData));
            Assert.That(ex.ErrorCode, Is.EqualTo(RpcErrorCode.Unknown));
            Assert.That(ex.Message, Is.EqualTo("Exception was thrown by handler."));
            Assert.That(ex.InnerException, Is.Null);

            replyData = sMarshaller.MarshalReply(new ComplexEntity(), new RpcScabraException(RpcErrorCode.Unavailable, "some other description"));

            ex = Assert.Throws<RpcScabraException>(() => cMarshaller.UnmarshalReply<int?>(replyData));
            Assert.That(ex.ErrorCode, Is.EqualTo(RpcErrorCode.Unavailable));
            Assert.That(ex.Message, Is.EqualTo("some other description"));
            Assert.That(ex.InnerException, Is.Null);
        }

        private ClientMarshaller CreateClientMarshaller(IScabraSecurityHandler securityHandler = null)
        {
            return new ClientMarshaller(
                new RpcProtoBufPayloadSerializer(Marshaller.MaxArgsLength),
                securityHandler ?? new NullScabraSecurityHandler());
        }

        private ServerMarshaller CreateServerMarshaller(IScabraSecurityHandler securityHandler = null)
        {
            return new ServerMarshaller(
                new RpcProtoBufPayloadSerializer(Marshaller.MaxArgsLength), 
                securityHandler ?? new NullScabraSecurityHandler(),
                NullLogger.Instance);
        }
    }
}
