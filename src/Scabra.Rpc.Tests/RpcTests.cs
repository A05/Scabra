using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Scabra.Rpc.Client;
using Scabra.Rpc.Server;
using System;
using System.Threading.Tasks;

namespace Scabra.Rpc
{
    [TestFixture]
    public partial class RpcTests
    {
        private IScabraRpcServer _rpcServer;
        private IScabraRpcChannel _rpcChannel;
        private RpcTestService _service;
        private IRpcTestService _proxy;

        // TODO: (NU) Logging does not output to console.

        private readonly ILoggerFactory _loggerFactory = 
            LoggerFactory.Create(builder => builder
                .ClearProviders()
                .AddConsole()
                .SetMinimumLevel(LogLevel.Error));

        public IRpcTestService Proxy => _proxy;

        [SetUp]
        public void SetUp()
        {
            var serverOptions = new ScabraRpcServerOptions()
            {
                Address = "inproc://rpc-test-server"
            };

            _rpcServer = new ScabraRpcServer(serverOptions, _loggerFactory);
            _rpcServer.RegisterService<IRpcTestService>(_service = new RpcTestService());
            _rpcServer.Start();

            var channelOptions = new ScabraRpcChannelOptions()
            {
                Address = "inproc://rpc-test-server"
            };

            _rpcChannel = new ScabraRpcChannel(channelOptions, _loggerFactory);
            _proxy = new RpcTestServiceProxy(_rpcChannel);
        }

        [TearDown]
        public void TearDown()
        {
            _rpcServer?.Dispose();
            _rpcChannel?.Dispose();
        }

        [Test]
        public void should_make_void_call()
        {
            Assert.That(_service.IsDoVoidJobCalled, Is.False);

            var t = Task.Run(() => _proxy.DoVoidJob());
            Assert.That(t.Wait(1000), Is.True);
            
            Assert.That(_service.IsDoVoidJobCalled, Is.True);
        }

        [Test]
        public void should_make_void_call_several_times()
        {
            for (int i = 0; i < 10; i++)
            {
                var t = Task.Run(() => _proxy.DoVoidJob());
                Assert.That(t.Wait(1000), Is.True);
            }
        }

        [Test]
        public void should_not_make_void_call_if_none_void_invoke_method_used()
        {
            Assert.That(_service.IsDoVoidJobCalled, Is.False);

            var realProxy = (RpcTestServiceProxy)_proxy;
            Assert.Throws<RpcScabraException>(() => realProxy.DoVoidJob_WithNoneVoidInvokeMethod());
            
            Assert.That(_service.IsDoVoidJobCalled, Is.False);
        }

        [Test]
        public void should_make_primitive_call()
        {
            Assert.That(_service.IsDoPrimitiveJobCalled, Is.False);

            var t = Task.Run(() => _proxy.DoPrimitiveJob(25));
            Assert.That(t.Wait(1000), Is.True);

            Assert.That(t.Result, Is.EqualTo(26));
            Assert.That(_service.IsDoPrimitiveJobCalled, Is.True);            
        }

        [Test]
        public void should_not_make_primitive_call_if_void_invoke_method_used()
        {
            Assert.That(_service.IsDoPrimitiveJobCalled, Is.False);

            var realProxy = (RpcTestServiceProxy)_proxy;
            Assert.Throws<RpcScabraException>(() => realProxy.DoPrimitiveJob_WithVoidInvokeMethod(25));
            
            Assert.That(_service.IsDoPrimitiveJobCalled, Is.False);
        }

        [Test]
        public void should_make_complex_call()
        {
            Assert.That(_service.IsDoComplexJobCalled, Is.False);

            var entity = new ComplexEntity() {
                Id = 48,
                Name = "foo",
                Description = "bar",
                Marks = new[] { 25, 26, 27, 28 }
            };

            var t = Task.Run(() => _proxy.DoComplexJob(entity, null, 193, null));
            Assert.That(t.Wait(1000), Is.True);

            var reply = t.Result;

            Assert.That(reply, Is.Not.Null);
            Assert.That(reply.Id, Is.EqualTo(49));
            Assert.That(reply.Name, Is.EqualTo("foo_resname"));
            Assert.That(reply.Description, Is.EqualTo("bar_resdesc"));
            Assert.That(reply.Marks, Is.Not.Null);
            Assert.That(reply.Marks.Length, Is.EqualTo(4));
            Assert.That(reply.Marks[0], Is.EqualTo(25 * 2));
            Assert.That(reply.Marks[1], Is.EqualTo(26 * 2));
            Assert.That(reply.Marks[2], Is.EqualTo(27 * 2));
            Assert.That(reply.Marks[3], Is.EqualTo(28 * 2));
            Assert.That(_service.IsDoComplexJobCalled, Is.True);
            Assert.That(_service.DoesDoComplexJobHaveCorrectArguments, Is.True);
        }

        [Test]
        public void should_make_null_reference_return_call()
        {
            Assert.That(_service.IsDoNullReferenceReturnJobCalled, Is.False);

            var t = Task.Run(() => _proxy.DoNullReferenceReturnJob());
            Assert.That(t.Wait(1000), Is.True);

            Assert.That(t.Result, Is.Null);

            Assert.That(_service.IsDoNullReferenceReturnJobCalled, Is.True);
        }

        [Test]
        public void should_make_null_value_return_call()
        {
            Assert.That(_service.IsDoNullValueReturnJobCalled, Is.False);

            var t = Task.Run(() => _proxy.DoNullValueReturnJob());
            Assert.That(t.Wait(1000), Is.True);

            Assert.That(t.Result, Is.Null);
            Assert.That(_service.IsDoNullValueReturnJobCalled, Is.True);
        }
    }
}
