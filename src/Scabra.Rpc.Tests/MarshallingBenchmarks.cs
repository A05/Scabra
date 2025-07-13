using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging.Abstractions;
using ProtoBuf;
using Scabra.Rpc.Client;
using Scabra.Rpc.Server;
using System;

namespace Scabra.Rpc
{
    public class MarshallingBenchmarks
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
        }

        internal interface IMarshallingBenchmarkService
        {
            ComplexEntity DoJob(ComplexEntity p1, int p2, ComplexEntity p3, int p4);
        }

        internal class MarshallingBenchmarkService : IMarshallingBenchmarkService
        {
            public ComplexEntity DoJob(ComplexEntity p1, int p2, ComplexEntity p3, int p4) => p1;
        }

        private readonly ClientMarshaller _clientMarshaller;
        private readonly ServerMarshaller _serverMarshaller;        
        private readonly byte[] _callData, _replyData;
        private readonly object[] _callArgs;

        public MarshallingBenchmarks() 
        {
            _clientMarshaller = CreateClientMarshaller();
            _serverMarshaller = CreateServerMarshaller();

            var service = new MarshallingBenchmarkService();
            _serverMarshaller.RegisterService(typeof(IMarshallingBenchmarkService), service);

            var p1 = new ComplexEntity() { IntValue = 1, StringValue = "some string 1", ArrayOfInts = new int[] { 1, 2, 3, 4, 5 } };
            var p2 = 25;
            var p3 = new ComplexEntity() { IntValue = 3, StringValue = "some string 3", ArrayOfInts = new int[] { 6, 7, 8, 9, 0 } };
            var p4 = 48;

            _callArgs = new object[] { p1, p2, p3, p4 };

            _callData = _clientMarshaller.MarshalCall<IMarshallingBenchmarkService, ComplexEntity>(
                nameof(IMarshallingBenchmarkService.DoJob), _callArgs);

            _replyData = _serverMarshaller.MarshalReply(_callArgs[1], null);
        }

        [Benchmark]
        public void FullTrip() 
        {
            var callData = _clientMarshaller.MarshalCall<IMarshallingBenchmarkService, ComplexEntity>(
                nameof(IMarshallingBenchmarkService.DoJob), _callArgs);

            _serverMarshaller.UnmarshalCall(callData, out var invoker, out var args);

            var replyData = _serverMarshaller.MarshalReply(_callArgs[1], null);

            var complextEntityValue = _clientMarshaller.UnmarshalReply<ComplexEntity>(replyData);

            if (complextEntityValue == null) throw new Exception(); // This check eliminates dead code.
        }

        [Benchmark]
        public void MarshalCall()
        {
            var callData = _clientMarshaller.MarshalCall<IMarshallingBenchmarkService, ComplexEntity>(
                nameof(IMarshallingBenchmarkService.DoJob), _callArgs);

            if (callData == null) throw new Exception(); // This check eliminates dead code.
        }

        [Benchmark]
        public void UnmarshalCall()
        {
            _serverMarshaller.UnmarshalCall(_callData, out var invoker, out var args);

            if (invoker == null || args == null) throw new Exception(); // This check eliminates dead code.
        }

        [Benchmark]
        public void MarshalReply()
        {
            var replyData = _serverMarshaller.MarshalReply(_callArgs[1], null);

            var complextEntityValue = _clientMarshaller.UnmarshalReply<ComplexEntity>(replyData);

            if (replyData == null) throw new Exception(); // This check eliminates dead code.
        }

        [Benchmark]
        public void UnmarshalReply()
        {
            var complextEntityValue = _clientMarshaller.UnmarshalReply<ComplexEntity>(_replyData);

            if (complextEntityValue == null) throw new Exception(); // This check eliminates dead code.
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
