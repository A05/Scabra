using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Diagnostics;
using System.IO;

namespace Scabra.Observer.Publisher
{
    public sealed class ScabraObserverPublisher : IScabraObserverPublisher
    {
        private const string LOGGER_CATEGORY = nameof(Scabra);

        private readonly IPayloadSerializer _serializer;
        private readonly PublishEndpoint _endpoint;

        private readonly ILogger _logger;
        
        private bool _started;
        private bool _disposed;

        public ScabraObserverPublisher(PublisherOptions options) : this(options, NullLoggerFactory.Instance)
        {
        }

        public ScabraObserverPublisher(PublisherOptions options, ILoggerFactory loggerFactory)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (options.Address == null)
                throw new ArgumentException("Address must be specified.", nameof(options));

            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));

            _logger = loggerFactory.CreateLogger(LOGGER_CATEGORY);

            _serializer = new ProtoBufPayloadSerializer();
            _endpoint = new PublishEndpoint(options.Address, _logger);
        }

        public void Start()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ScabraObserverPublisher));

            if (_started)
                throw new InvalidOperationException("Scabra publisher has been already started.");

            try
            {
                _endpoint.Start();

                _started = true;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Failed to start scabra publisher.");
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            try
            {
                _endpoint.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to stop scabra publisher.");
            }
            finally
            {
                _disposed = true;
            }
        }

        void IScabraObserverPublisher.Publish<TMessage>(string topic, TMessage message)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ScabraObserverPublisher));

            if (!_started)
                throw new InvalidOperationException("Scabra publisher must be started.");

            Debug.Assert(topic != null);
            Debug.Assert(message != null);

            using var stream = new MemoryStream();
            _serializer.Serialize(stream, message);
            var data = stream.ToArray();

            _endpoint.Publish(topic, data);
        }
    }
}
