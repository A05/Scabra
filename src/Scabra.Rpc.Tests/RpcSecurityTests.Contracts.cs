using Scabra.Rpc.Client;
using System;
using System.Security;
using System.Text;

namespace Scabra.Rpc
{
    public partial class RpcSecurityTests
    {
        private class TestSecurityHandler : IScabraSecurityHandler
        {
            private string _secret;

            public void SetCorrectSecret() => _secret = "correct";
            public void SetInvalidSecret() => _secret = "invalid";

            public void TakeSecurityMeasures(object secret)
            {
                switch ((string)secret)
                {
                    case "correct":
                        return;
                    case "invalid":
                        throw new SecurityException();
                    default:
                        throw new NotSupportedException();
                }
            }

            public object DecodeSecret(byte[] bytes)
            {
                return Encoding.ASCII.GetString(bytes);
            }

            public object GetSecret() => _secret;

            public byte[] EncodeSecret(object secret)
            {
                return Encoding.ASCII.GetBytes((string)secret);
            }
        }

        public interface IRpcProtectedTestService
        {
            void DoJob();
        }

        public class RpcProtectedTestService : IRpcProtectedTestService
        {
            public bool IsDoJobCalled { get; private set; }

            public void DoJob()
            {
                IsDoJobCalled = true;
            }
        }

        public class RpcProtectedTestServiceProxy : IRpcProtectedTestService
        {
            private readonly IScabraRpcChannel _channel;

            public RpcProtectedTestServiceProxy(IScabraRpcChannel channel)
            {
                _channel = channel ?? throw new ArgumentNullException(nameof(channel));
            }

            public void DoJob()
            {
                _channel.InvokeMethod<IRpcProtectedTestService>(nameof(DoJob));
            }
        }
    }
}
