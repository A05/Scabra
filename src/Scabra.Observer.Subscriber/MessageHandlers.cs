using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace Scabra.Observer.Subscriber
{
    internal class MessageHandlers
    {
        class Handler
        {
            public Type MessageType;
            public Action<object> Untyped;
            public object Target;
            public MethodInfo Method;
        }

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly Dictionary<string, List<Handler>> _topicHandlers = new(25);

        public bool Add<TMessage>(string topic, Action<TMessage> handler)
        {
            Debug.Assert(topic != null);
            Debug.Assert(handler != null);

            _lock.EnterWriteLock();

            try
            {
                if (!_topicHandlers.TryGetValue(topic, out var topicHandler))
                    _topicHandlers.Add(topic, topicHandler = new List<Handler>(10));

                var isFirstHandler = topicHandler.Count == 0;

                topicHandler.Add(new Handler()
                {
                    MessageType = typeof(TMessage),
                    Untyped = obj => handler((TMessage)obj),
                    Target = handler.Target,
                    Method = handler.Method
                });

                return isFirstHandler;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool Remove<TMessage>(string topic, Action<TMessage> handler)
        {
            _lock.EnterWriteLock();

            try
            {
                if (!_topicHandlers.TryGetValue(topic, out var topicHandler))
                    throw new InvalidOperationException($"Topic '{topic}' not found.");

                int k = -1;
                for (var i = 0; k == -1 && i < topicHandler.Count; i++)
                    if (ReferenceEquals(topicHandler[i].Target, handler.Target) &&
                        ReferenceEquals(topicHandler[i].Method, handler.Method))
                        k = i;

                if (k == -1)
                    throw new InvalidOperationException($"Handler not found in '{topic}' topic.");

                topicHandler.RemoveAt(k);

                var isLastHandler = topicHandler.Count == 0;

                if (isLastHandler)
                    _topicHandlers.Remove(topic);

                return isLastHandler;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public IEnumerable<(Type messageType, Action<object> handler)> Get(string topic)
        {
            _lock.EnterReadLock();

            try
            {
                foreach (var p in _topicHandlers)
                {
                    var iTopic = p.Key;
                    var iHandlers = p.Value;

                    if (topic.StartsWith(iTopic))
                        foreach (var iHandler in iHandlers)
                            yield return (iHandler.MessageType, iHandler.Untyped);
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }
}
