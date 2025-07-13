using ProtoBuf;
using Scabra.Rpc.Client;
using System;
using System.Linq;

namespace Scabra.Rpc
{
    public partial class RpcTests
    {
        [ProtoContract]
        public class ComplexEntity
        {
            [ProtoMember(1)]
            public int Id { get; set; }

            [ProtoMember(2)]
            public string Name { get; set; }

            [ProtoMember(3)]
            public int[] Marks { get; set; }

            [ProtoMember(4)]
            public string Description { get; set; }

            public ComplexEntity() { }
        }

        public interface IRpcTestService
        {
            void DoVoidJob();
            
            int DoPrimitiveJob(int parameter);

            ComplexEntity DoComplexJob(ComplexEntity entity, int? nullInt, int notNullInt, ComplexEntity nullEntity);

            ComplexEntity DoNullReferenceReturnJob();

            int? DoNullValueReturnJob();
        }

        public class RpcTestService : IRpcTestService
        {
            public bool IsDoVoidJobCalled { get; private set; }
            public bool IsDoPrimitiveJobCalled { get; private set; }
            public bool IsDoComplexJobCalled { get; private set; }
            public bool DoesDoComplexJobHaveCorrectArguments { get; private set; }
            public bool IsDoNullReferenceReturnJobCalled { get; private set; }
            public bool IsDoNullValueReturnJobCalled { get; private set; }

            public void DoVoidJob()
            {
                IsDoVoidJobCalled = true;
            }

            public int DoPrimitiveJob(int parameter)
            {
                IsDoPrimitiveJobCalled = true;

                return parameter + 1;
            }

            public ComplexEntity DoComplexJob(ComplexEntity entity, int? nullInt, int notNullInt, ComplexEntity nullEntity)
            {
                IsDoComplexJobCalled = true;

                DoesDoComplexJobHaveCorrectArguments = 
                    entity != null && 
                    nullInt == null && 
                    notNullInt == 193 && 
                    nullEntity == null;

                return new ComplexEntity() 
                {
                    Id = entity.Id + 1,
                    Name = entity.Name + "_resname",
                    Description = entity.Description + "_resdesc",
                    Marks = entity.Marks.Select(i => i * 2).ToArray()
                };
            }

            public ComplexEntity DoNullReferenceReturnJob()
            {
                IsDoNullReferenceReturnJobCalled = true;

                return null;
            }

            public int? DoNullValueReturnJob() 
            {
                IsDoNullValueReturnJobCalled = true;

                return null; 
            }
        }

        public class RpcTestServiceProxy : IRpcTestService
        {
            private readonly IScabraRpcChannel _channel;

            public RpcTestServiceProxy(IScabraRpcChannel channel)
            {
                _channel = channel ?? throw new ArgumentNullException(nameof(channel));
            }

            public void DoVoidJob()
            {
                _channel.InvokeMethod<IRpcTestService>(nameof(DoVoidJob));
            }

            public void DoVoidJob_WithNoneVoidInvokeMethod()
            {
                _channel.InvokeMethod<IRpcTestService, int?>(nameof(DoVoidJob));
            }

            public int DoPrimitiveJob(int parameter)
            {
                return _channel.InvokeMethod<IRpcTestService, int>(nameof(DoPrimitiveJob), parameter);
            }

            public void DoPrimitiveJob_WithVoidInvokeMethod(int parameter)
            {
                _channel.InvokeMethod<IRpcTestService>(nameof(DoPrimitiveJob), parameter);
            }

            public ComplexEntity DoComplexJob(ComplexEntity entity, int? nullInt, int notNullInt, ComplexEntity nullEntity)
            {
                return _channel.InvokeMethod<IRpcTestService, ComplexEntity>(nameof(DoComplexJob), entity, nullInt, notNullInt, nullEntity);
            }

            public ComplexEntity DoNullReferenceReturnJob()
            {
                return _channel.InvokeMethod<IRpcTestService, ComplexEntity>(nameof(DoNullReferenceReturnJob));
            }

            public int? DoNullValueReturnJob()
            {
                return _channel.InvokeMethod<IRpcTestService, int?>(nameof(DoNullValueReturnJob));
            }
        }
    }
}
