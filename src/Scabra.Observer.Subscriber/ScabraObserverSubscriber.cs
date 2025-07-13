using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Diagnostics;

namespace Scabra.Observer.Subscriber
{
    public sealed class ScabraObserverSubscriber : IScabraObserverSubscriber
    {
        private const string LOGGER_CATEGORY = nameof(Scabra);

        private readonly SubscribeEndpoint _endpoint;
        private readonly MessageHandlers _handlers;
        private readonly MessageProcessor _messageProcessor;                
        private readonly ILogger _logger;        
        private bool _disposed;

        public ScabraObserverSubscriber(ScabraObserverSubscriberOptions options) : this(options, NullLoggerFactory.Instance)
        {
        }

        public ScabraObserverSubscriber(ScabraObserverSubscriberOptions options, ILoggerFactory loggerFactory)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (options.Address == null)
                throw new ArgumentException($"{options.Address} must be specified.", nameof(options));

            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));

            _logger = loggerFactory.CreateLogger(LOGGER_CATEGORY);

            _handlers = new();

            var serializer = new ProtoBufPayloadSerializer();
            _messageProcessor = new MessageProcessor(_handlers, serializer, _logger);
            
            _endpoint = new SubscribeEndpoint(options.Address, _logger);
            _endpoint.MessageReceived += _messageProcessor.ProcessMessage;
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
                _logger.LogError(ex, "Failed to stop Scabra subscriber.");
            }
            finally
            {
                _disposed = true;
            }
        }

        void IScabraObserverSubscriber.Subscribe<TMessage>(string topic, Action<TMessage> handler)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ScabraObserverSubscriber));

            Debug.Assert(topic != null);
            Debug.Assert(handler != null);

            var isFirstHandler = _handlers.Add(topic, handler);
            if (isFirstHandler)
                _endpoint.Subscribe(topic);
        }

        void IScabraObserverSubscriber.Unsubscribe<TMessage>(string topic, Action<TMessage> handler)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ScabraObserverSubscriber));

            var isLastHandler = _handlers.Remove(topic, handler);
            if (isLastHandler)
                _endpoint.Unsubscribe(topic);
        }
    }
}
