using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Scabra.Rpc.Client;
using Scabra.Rpc.Server;
using System;
using System.Threading.Tasks;

namespace Scabra.Rpc
{
    [TestFixture]
    public partial class RpcSecurityTests
    {
        private TestSecurityHandler _clientSecurityHandler;
        private IScabraRpcServer _rpcServer;
        private IScabraRpcChannel _rpcChannel;
        private RpcProtectedTestService _service;
        private IRpcProtectedTestService _proxy;

        [SetUp]
        public void SetUp()
        {
            var serverOptions = new ScabraRpcServerOptions()
            {
                Address = "inproc://rpc-security-test-server"
            };

            _rpcServer = new ScabraRpcServer(serverOptions, new TestSecurityHandler(), NullLoggerFactory.Instance);
            _rpcServer.RegisterService<IRpcProtectedTestService>(_service = new RpcProtectedTestService());
            _rpcServer.Start();

            var channelOptions = new ScabraRpcChannelOptions()
            {
                Address = "inproc://rpc-security-test-server"
            };

            _clientSecurityHandler = new TestSecurityHandler();
            _rpcChannel = new ScabraRpcChannel(channelOptions, _clientSecurityHandler, NullLoggerFactory.Instance);
            _proxy = new RpcProtectedTestServiceProxy(_rpcChannel);
        }

        [TearDown]
        public void TearDown()
        {
            _rpcServer?.Dispose();
            _rpcChannel?.Dispose();
        }

        [Test]
        public void should_make_protected_call()
        {
            Assert.That(_service.IsDoJobCalled, Is.False);

            _clientSecurityHandler.SetCorrectSecret();

            var t = Task.Run(() => _proxy.DoJob());
            Assert.That(t.Wait(1000), Is.True);
            Assert.That(t.IsFaulted, Is.False);
            
            Assert.That(_service.IsDoJobCalled, Is.True);
        }

        [Test]
        public void should_not_make_protected_call()
        {
            Assert.That(_service.IsDoJobCalled, Is.False);

            _clientSecurityHandler.SetInvalidSecret();

            var t = Task.Run(() => _proxy.DoJob());

            try
            {
                t.Wait(1000);

                Assert.Fail();
            }
            catch (AggregateException ex)
            {
                Assert.That(ex.InnerExceptions.Count, Is.EqualTo(1));
                Assert.That(ex.InnerExceptions[0], Is.TypeOf<RpcScabraException>());
            }
                       
            Assert.That(_service.IsDoJobCalled, Is.False);
        }
    }
}
