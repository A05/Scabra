using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using System;
using System.IO;
using System.Text;
using Scabra.Rpc.Client.Hosting;
using Scabra.Rpc.Server.Hosting;
using Scabra.Rpc.Server;
using Microsoft.Extensions.Hosting;

namespace Scabra.Rpc
{
    [TestFixture]
    public class RpcHostingTests
    {
        [Test]
        public void should_host_client()
        {
            var jsonConfiguration = @"
{
  ""Scabra"": {
    ""rpc"": {
      ""clients"": [
        {
          ""name"": ""server 1"",
          ""address"": ""tcp://127.0.0.1:3551""
        },
        {
          ""name"": ""server 2"",
          ""address"": ""tcp://127.0.0.1:3552""
        }
      ]
    }
  }
}";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonConfiguration));
            var configuration = new ConfigurationBuilder()
                .AddJsonStream(stream)
                .Build();

            var securityHandler = Mock.Of<IScabraSecurityHandler>();

            var services = new ServiceCollection();
            services.AddScabraRpcClient(configuration, securityHandler);

            var serviceProvider = services.BuildServiceProvider();

            var provider = serviceProvider.GetRequiredService<IScabraRpcChannelProvider>();
            var channel1 = provider.GetChannel("server 1");
            Assert.That(channel1, Is.Not.Null);
            var channel2 = provider.GetChannel("server 2");
            Assert.That(channel2, Is.Not.Null);

            Assert.Throws<ArgumentOutOfRangeException>(() => provider.GetChannel("server 3"));

            Assert.That(securityHandler, Is.SameAs(serviceProvider.GetRequiredService<IScabraSecurityHandler>()));
        }

        [Test]
        public void should_host_server()
        {
            var jsonConfiguration = @"
{  
  ""Scabra"": {
    ""rpc"": {
      ""server"": {
        ""address"": ""tcp://127.0.0.1:3551""
      }
    }
  }
}";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonConfiguration));
            var configuration = new ConfigurationBuilder()
                .AddJsonStream(stream)
                .Build();

            var securityHandler = Mock.Of<IScabraSecurityHandler>();

            var services = new ServiceCollection();
            services.AddScabraRpcServer(configuration, securityHandler);

            var serviceProvider = services.BuildServiceProvider();

            var rpcServer = serviceProvider.GetRequiredService<IScabraRpcServer>();            
            Assert.That(rpcServer, Is.Not.Null);

            var hostedService = serviceProvider.GetRequiredService<IHostedService>();
            Assert.That(hostedService, Is.Not.Null);
            Assert.That(hostedService, Is.TypeOf<ScabraRpcServerHostedService>());

            Assert.That(securityHandler, Is.SameAs(serviceProvider.GetRequiredService<IScabraSecurityHandler>()));
        }
    }
}
