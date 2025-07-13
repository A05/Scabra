using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Scabra.Rpc.Server
{
    internal abstract class MutexableQueue<T> : Mutexable where T : class, new()
    {
        private readonly T[] _items;
        private readonly int _initialTimeoutInMs;

        private int? _head, _tail;

        public bool IsEmpty => _head == null && _tail == null;
        private bool IsFull => !IsEmpty && AddOne(_tail) == _head;

        protected MutexableQueue(int length, int initialTimeoutInMs, int gainAccessTimeoutInMs, IEndpointLogger logger) 
            : base(gainAccessTimeoutInMs, logger)
        {
            Debug.Assert(length > 0);
            Debug.Assert(gainAccessTimeoutInMs > 0);

            _items = new T[length];

            for (int i = 0; i < length; i++)
                _items[i] = new T();

            _initialTimeoutInMs = initialTimeoutInMs;

            AssertInvariats();
        }

        protected void Enqueue(Action<T> initialize)
        {
            GetAccessForEnqueueing();

            try
            {
                if (IsFull)
                    throw new InvalidOperationException("Queue is full.");

                if (IsEmpty)
                    _head = _tail = 0;
                else
                    _tail = AddOne(_tail);

                initialize(_items[_tail.Value]);

                AssertInvariats();
            }
            finally
            {
                ReleaseAccess();
            }
        }

        protected bool TryDequeue(Action<T> release)
        {
            GetAccessForDequeueing();

            try
            {
                if (IsEmpty)
                {
                    return false;
                }

                var item = _items[_head.Value];

                if (_head == _tail)
                    _head = _tail = null;
                else
                    _head = AddOne(_head);

                release(item);

                AssertInvariats();

                return true;
            }
            finally
            {
                ReleaseAccess();
            }
        }

        public bool WaitUntilEmpty(int timeoutInMs)
        {
            var start = Environment.TickCount;

            while (true)
            {
                GetAccessForWaitingUntilEmpty();

                try
                {
                    if (IsEmpty)
                        return true;
                    
                    if (Environment.TickCount - start >= timeoutInMs)
                        return false;                    
                }
                finally
                {
                    ReleaseAccess();
                }

                Thread.Sleep(1);
            }
        }

        protected IEnumerable<T> GetItems()
        {
            if (!IsEmpty)
            {
                for (int i = _head.Value; i != _tail.Value; i = AddOne(i))
                    yield return _items[i];

                yield return _items[_tail.Value];
            }
        }

        private int AddOne(int? i) => (i.Value + 1) % _items.Length;

        private void GetAccessForEnqueueing()
        {
            string getWarningMessage() => "{Timeout} ms timeout on gaining access for equeueing.";
            string getExceptionMessage() => "{0} ms timeout on gaining access for equeueing.";

            GainAccess(_initialTimeoutInMs, CancellationToken.None, getWarningMessage, getExceptionMessage);
        }

        private void GetAccessForDequeueing()
        {
            string getWarningMessage() => "{Timeout} ms timeout on gaining access for dequeueing.";
            string getExceptionMessage() => "{0} ms timeout on gaining access for dequeueing.";

            GainAccess(_initialTimeoutInMs, CancellationToken.None, getWarningMessage, getExceptionMessage);
        }

        private void GetAccessForWaitingUntilEmpty()
        {
            string getWarningMessage() => "{Timeout} ms timeout on gaining access for waiting until empty.";
            string getExceptionMessage() => "{0} ms timeout on gaining access for waiting until empty.";

            GainAccess(_initialTimeoutInMs, CancellationToken.None, getWarningMessage, getExceptionMessage);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AssertInvariats()
        {
            Debug.Assert(
                (_head == null && _tail == null) || (_head != null && _tail != null),
                $"Invalid head {_head} and tail {_tail} positions.");
        }

        public override string ToString() => 
            $"IsEmpty = {IsEmpty}, IsFull = {IsFull}, " +
            $"Head = {(_head.HasValue ? _head.ToString() : "null")}, " +
            $"Tail = {(_tail.HasValue ? _tail.ToString() : "null")}";
    }
}