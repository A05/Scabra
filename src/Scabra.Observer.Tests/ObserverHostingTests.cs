using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Scabra.Observer.Publisher;
using Scabra.Observer.Publisher.Hosting;
using Scabra.Observer.Subscriber;
using Scabra.Observer.Subscriber.Hosting;
using System;
using System.IO;
using System.Text;

namespace Scabra.Observer
{
    [TestFixture]
    public class ObserverHostingTests
    {
        [Test]
        public void should_host_subscriber()
        {
            var jsonConfiguration = @"
{
  ""Scabra"": {
    ""observer"": {
      ""subscribers"": [
        {
          ""name"": ""publisher 1"",
          ""address"": ""tcp://127.0.0.1:3551""
        },
        {
          ""name"": ""publisher 2"",
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

            var services = new ServiceCollection();
            services.AddScabraObserverSubscriber(configuration);

            var serviceProvider = services.BuildServiceProvider();

            var provider = serviceProvider.GetRequiredService<IScabraObserverSubscriberProvider>();
            var publisher1 = provider.GetSubscriber("publisher 1");
            Assert.That(publisher1, Is.Not.Null);
            var publisher2 = provider.GetSubscriber("publisher 2");
            Assert.That(publisher2, Is.Not.Null);

            Assert.Throws<ArgumentOutOfRangeException>(() => provider.GetSubscriber("publisher 3"));
        }

        [Test]
        public void should_host_publisher()
        {
            var jsonConfiguration = @"
{
  ""Scabra"": {
    ""observer"": {
      ""publisher"": {
        ""address"": ""tcp://127.0.0.1:3555""
      }
    }
  }
}";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonConfiguration));
            var configuration = new ConfigurationBuilder()
                .AddJsonStream(stream)
                .Build();

            var services = new ServiceCollection();
            services.AddScabraObserverPublisher(configuration);

            var serviceProvider = services.BuildServiceProvider();

            var publisher = serviceProvider.GetRequiredService<IScabraObserverPublisher>();
            Assert.That(publisher, Is.Not.Null);

            var hostedService = serviceProvider.GetRequiredService<IHostedService>();
            Assert.That(hostedService, Is.Not.Null);
            Assert.That(hostedService, Is.TypeOf<ScabraObserverPublisherHostedService>());
        }
    }
}
