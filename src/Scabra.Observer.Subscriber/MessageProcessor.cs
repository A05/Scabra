using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;

namespace Scabra.Observer.Subscriber
{
    internal class MessageProcessor
    {
        private readonly MessageHandlers _handlers;
        private readonly IPayloadSerializer _serializer;
        private readonly ILogger _logger;

        public MessageProcessor(MessageHandlers handlers, IPayloadSerializer serializer, ILogger logger)
        {
            Debug.Assert(handlers != null);
            Debug.Assert(serializer != null);
            Debug.Assert(logger != null);

            _handlers = handlers;
            _serializer = serializer;
            _logger = logger;
        }

        public void ProcessMessage(string topic, byte[] data)
        {
            foreach (var (messageType, handler) in _handlers.Get(topic))
            {
                object message = null;

                try
                {
                    using var stream = new MemoryStream(data);
                    message = _serializer.Deserialize(messageType, stream);
                }
                catch (Exception ex)
                {
                    message = null;

                    _logger.LogError(ex, "Failed to deserialize a message of '{MessageType}' type in '{Topic}' topic.", messageType.Name, topic);
                }

                try 
                {
                    if (message != null)
                        handler(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to handle a message in '{Topic}' topic.", topic);
                }
            }
        }
    }
}
