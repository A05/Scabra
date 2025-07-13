using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Scabra.Rpc.Client
{
    internal class ClientCallBuffer
    {
        private enum _List { Unknown = 0, Free, Pending, Executing }

        private class _Node
        {
            public readonly ClientCall Call;
            public int? PrevIdx, NextIdx;
            public _List BelongsTo;

            public _Node(ushort index)
            {
                Call = new ClientCall(index);
            }

            public override string ToString()
            {
                return 
                    "Prev = " + (PrevIdx == null ? "null" : PrevIdx) + ", " + 
                    "Next = " + (NextIdx == null ? "null" : NextIdx) + ", " + 
                    Call;
            }
        }

        private const int InitialWaitingTimeoutInMs = 1;

        private readonly _Node[] _nodes;
        private readonly SemaphoreSlim _semaphore;
        private readonly int _gainAccessTimeoutInMs;
        private readonly IEndpointLogger _logger;

        private int? _freeHeadIdx, _freeTailIdx;
        private int? _pendingHeadIdx, _pendingTailIdx;
        private int? _executingHeadIdx, _executingTailIdx;

        private ushort _lastSequenceNumber;

        private int _freeCount, _pendingCount, _executingCount;

        public ClientCallBuffer(IEndpointLogger logger) : this(40, 1000, logger) { }

        public ClientCallBuffer(int length, int gainAccessTimeoutInMs, IEndpointLogger logger)
        {
            Debug.Assert(length > 0);
            Debug.Assert(gainAccessTimeoutInMs > 0);

            _nodes = new _Node[length];
            _gainAccessTimeoutInMs = gainAccessTimeoutInMs;

            for (ushort i = 0; i < length; i++)
            {
                _nodes[i] = new _Node(i) { BelongsTo = _List.Free };

                if (i > 0)
                {
                    _nodes[i].PrevIdx = i - 1;
                    _nodes[i - 1].NextIdx = i;
                }
            }

            _freeHeadIdx = 0; 
            _freeTailIdx = length - 1;

            _freeCount = length;
            _pendingCount = 0;
            _executingCount = 0;

            _lastSequenceNumber = ushort.MaxValue;
            _semaphore = new SemaphoreSlim(1, 1);
            _logger = logger;

            AssertInvariats();
        }

        public ClientCall AddPending(byte[] callData, int timeoutInMs)
        {
            Debug.Assert(callData != null);
            Debug.Assert(timeoutInMs > 0);

            GetAccessForAddingPending();

            try
            {
                if (_freeCount == 0)
                    throw new InvalidOperationException("Buffer is full.");

                Debug.Assert(_freeHeadIdx != null);

                var movedNode = RemoveNode(_freeHeadIdx.Value, ref _freeHeadIdx, ref _freeTailIdx);
                Debug.Assert(movedNode != null);

                AddNodeToTail(movedNode, ref _pendingHeadIdx, ref _pendingTailIdx);

                movedNode.BelongsTo = _List.Pending;

                _freeCount--;
                _pendingCount++;

                var sequenceNumber = unchecked(++_lastSequenceNumber);
                movedNode.Call.Initialize(sequenceNumber, timeoutInMs, callData);

                AssertInvariats();

                return movedNode.Call;
            }
            finally
            {
                ReleaseAccess();
            }
        }

        public bool TryGetPendingForExecuting(out ClientCall call)
        {
            GetAccessForGettingPendingForExecuting();

            try
            {
                if (_pendingCount == 0)
                {
                    call = null;
                    return false;
                }

                Debug.Assert(_pendingHeadIdx != null);

                var movedNode = RemoveNode(_pendingHeadIdx.Value, ref _pendingHeadIdx, ref _pendingTailIdx);
                Debug.Assert(movedNode != null);

                AddNodeToTail(movedNode, ref _executingHeadIdx, ref _executingTailIdx);

                movedNode.BelongsTo = _List.Executing;

                _pendingCount--;
                _executingCount++;                

                AssertInvariats();

                call = movedNode.Call;
                return true;
            }
            finally
            {
                ReleaseAccess();
            }
        }

        public bool TrySetReply(ClientCallId id, byte[] replyData)
        {
            Debug.Assert(replyData != null);
            
            if (id.Index < 0 || id.Index >= _nodes.Length)
                return false;

            GetAccessForSettingReply();

            try
            {
                var node = _nodes[id.Index];
                if (node.BelongsTo != _List.Executing)
                    return false;

                var call = node.Call;
                Debug.Assert(call.Id.Index == id.Index);

                if (call.Id.SequenceNumber != id.SequenceNumber)
                    return false;

                call.SetReply(replyData);

                AssertInvariats();

                return true;
            }
            finally
            {
                ReleaseAccess();
            }
        }

        public void Remove(ClientCall call)
        {
            Debug.Assert(call != null);

            if (call.Id.Index < 0 || call.Id.Index >= _nodes.Length)
                throw new InvalidOperationException($"Call ({call}) is outside of the call buffer.");

            GetAccessForRemoving();

            try
            {
                var removingNodeIdx = call.Id.Index;
                var removingNode = _nodes[removingNodeIdx];
                Debug.Assert(removingNode.Call.Id.Index == call.Id.Index);

                if (!ReferenceEquals(call, removingNode.Call))
                    throw new InvalidOperationException($"Call ({call}) not found.");

                if (removingNode.BelongsTo == _List.Pending)
                {
                    var n = RemoveNode(removingNodeIdx, ref _pendingHeadIdx, ref _pendingTailIdx);
                    Debug.Assert(ReferenceEquals(n, removingNode));

                    AddNodeToTail(removingNode, ref _freeHeadIdx, ref _freeTailIdx);
                    removingNode.BelongsTo = _List.Free;

                    _pendingCount--;
                    _freeCount++;                    
                }
                else if (removingNode.BelongsTo == _List.Executing)
                {
                    var n = RemoveNode(removingNodeIdx, ref _executingHeadIdx, ref _executingTailIdx);
                    Debug.Assert(ReferenceEquals(n, removingNode));

                    AddNodeToTail(removingNode, ref _freeHeadIdx, ref _freeTailIdx);
                    removingNode.BelongsTo = _List.Free;

                    _executingCount--;
                    _freeCount++;
                }
                else
                    throw new InvalidOperationException($"Call ({call}) must be either pending or executing.");

                AssertInvariats();
            }
            finally
            {
                ReleaseAccess();
            }
        }

        public bool WaitForCompletion(int timeoutInMs)
        {
            var start = Environment.TickCount;

            while (true)
            {
                GetAccessForWaitingForCompletion();

                try
                {
                    if (_pendingCount + _executingCount == 0)
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

        public void Abort()
        {
            GetAccessForAborting();

            try
            {
                abortImpl(_pendingHeadIdx);
                abortImpl(_executingHeadIdx);
            }
            finally
            {
                ReleaseAccess();
            }

            void abortImpl(int? idx)
            {
                for (var i = idx; i != null;)
                {
                    var node = _nodes[i.Value];

                    node.Call.Abort();

                    i = node.NextIdx;
                }
            }
        }

        private _Node RemoveNode(int removingNodeIdx, ref int? srcHeadIdx, ref int? srcTailIdx)
        {
            var removingNode = _nodes[removingNodeIdx];

            if (removingNodeIdx == srcHeadIdx)
            {
                if (removingNode.NextIdx == null)
                {
                    srcHeadIdx = null;
                    srcTailIdx = null;
                }
                else
                {
                    srcHeadIdx = removingNode.NextIdx;
                    _nodes[srcHeadIdx.Value].PrevIdx = null;
                }
            }
            else if (removingNodeIdx == srcTailIdx)
            {
                if (removingNode.PrevIdx == null)
                {
                    srcHeadIdx = null;
                    srcTailIdx = null;
                }
                else
                {
                    srcTailIdx = removingNode.PrevIdx;
                    _nodes[srcTailIdx.Value].NextIdx = null;
                }
            }
            else
            {
                _nodes[removingNode.PrevIdx.Value].NextIdx = removingNode.NextIdx;
                _nodes[removingNode.NextIdx.Value].PrevIdx = removingNode.PrevIdx;
            }

            return removingNode;
        }

        private void AddNodeToTail(_Node node, ref int? dstHeadIdx, ref int? dstTailIdx)
        {
            var nodeIdx = node.Call.Id.Index;

            if (dstHeadIdx == null)
            {
                node.PrevIdx = null;
                node.NextIdx = null;

                dstHeadIdx = nodeIdx;
                dstTailIdx = nodeIdx;
            }
            else
            {
                var oldTailIdx = dstTailIdx;
                var oldTail = _nodes[oldTailIdx.Value];

                oldTail.NextIdx = nodeIdx;

                node.PrevIdx = oldTailIdx;
                node.NextIdx = null;

                dstTailIdx = nodeIdx;
            }
        }

        private void GetAccessForAddingPending()
        {
            string getWarningMessage() => "{Timeout} ms timeout on gaining access for adding pending.";
            string getExceptionMessage() => "{0} ms timeout on gaining access for adding pending.";

            GainAccess(InitialWaitingTimeoutInMs, CancellationToken.None, getWarningMessage, getExceptionMessage);
        }

        private void GetAccessForGettingPendingForExecuting()
        {
            string getWarningMessage() => "{Timeout} ms timeout on gaining for getting pending for executing.";
            string getExceptionMessage() => "{0} ms timeout on gaining access for getting pending for executing.";

            GainAccess(InitialWaitingTimeoutInMs, CancellationToken.None, getWarningMessage, getExceptionMessage);
        }

        private void GetAccessForSettingReply()
        {
            string getWarningMessage() => "{Timeout} ms timeout on gaining for setting reply.";
            string getExceptionMessage() => "{0} ms timeout on gaining access for setting reply.";

            GainAccess(InitialWaitingTimeoutInMs, CancellationToken.None, getWarningMessage, getExceptionMessage);
        }

        private void GetAccessForRemoving()
        {
            string getWarningMessage() => "{Timeout} ms timeout on gaining access for removing.";
            string getExceptionMessage() => "{0} ms timeout on gaining access for removing.";

            GainAccess(InitialWaitingTimeoutInMs, CancellationToken.None, getWarningMessage, getExceptionMessage);
        }

        private void GetAccessForWaitingForCompletion()
        {
            string getWarningMessage() => "{Timeout} ms timeout on gaining access for waiting for completion.";
            string getExceptionMessage() => "{0} ms timeout on gaining access for waiting for completion.";

            GainAccess(InitialWaitingTimeoutInMs, CancellationToken.None, getWarningMessage, getExceptionMessage);
        }

        private void GetAccessForAborting()
        {
            string getWarningMessage() => "{Timeout} ms timeout on gaining access for aborting.";
            string getExceptionMessage() => "{0} ms timeout on gaining access for aborting.";

            GainAccess(InitialWaitingTimeoutInMs, CancellationToken.None, getWarningMessage, getExceptionMessage);
        }

        private void GainAccess(int initialeTimeoutInMs, CancellationToken cToken, Func<string> getWarningMessage, Func<string> getExceptionMessage)
        {
            Debug.Assert(initialeTimeoutInMs < _gainAccessTimeoutInMs);

            int waitingTime = 0;
            string warningMessage = null;

            for (int timeout = initialeTimeoutInMs; true; timeout *= 2)
            {
                if (_semaphore.Wait(timeout, cToken))
                    return;

                warningMessage ??= getWarningMessage();
                _logger.LogWarning(warningMessage, timeout);

                if ((waitingTime += timeout) >= _gainAccessTimeoutInMs)
                {
                    var exceptionMessage = getExceptionMessage();
                    throw new TimeoutException(string.Format(exceptionMessage, timeout));
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReleaseAccess()
        {
            _semaphore.Release();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AssertInvariats()
        {
            Debug.Assert(
                _freeCount + _pendingCount + _executingCount == _nodes.Length,
                "Sum of counters of call nodes must be equal to node array lenght.");

            // Free

            Debug.Assert(
                (_freeHeadIdx == null && _freeTailIdx == null) || (_freeHeadIdx != null && _freeTailIdx != null),
                "Free head and tail must be both null or not null.");

            Debug.Assert(
                _freeHeadIdx == null || _nodes[_freeHeadIdx.Value].PrevIdx == null,
                "Free head prev must be null.");

            Debug.Assert(
                _freeTailIdx == null || _nodes[_freeTailIdx.Value].NextIdx == null,
                "Free tail next must be null.");

            // Pending

            Debug.Assert(
                (_pendingHeadIdx == null && _pendingTailIdx == null) || (_pendingHeadIdx != null && _pendingTailIdx != null),
                "Pending head and tail must be both null or not null.");

            Debug.Assert(
                _pendingHeadIdx == null || _nodes[_pendingHeadIdx.Value].PrevIdx == null,
                "Pending head prev must be null.");

            Debug.Assert(
                _pendingTailIdx == null || _nodes[_pendingTailIdx.Value].NextIdx == null,
                "Pending tail next must be null.");

            // Executing

            Debug.Assert(
                (_executingHeadIdx == null && _executingHeadIdx == null) || (_executingHeadIdx != null && _executingTailIdx != null),
                "Free head and tail must be both null or not null.");

            Debug.Assert(
                _executingHeadIdx == null || _nodes[_executingHeadIdx.Value].PrevIdx == null,
                "Executing head prev must be null.");

            Debug.Assert(
                _executingTailIdx == null || _nodes[_executingTailIdx.Value].NextIdx == null,
                "Executing tail next must be null.");
        }

#if DEBUG
        internal IEnumerable<int> FreeCalls => GetIndices(_freeHeadIdx);
        internal IEnumerable<int> PendingCalls => GetIndices(_pendingHeadIdx);
        internal IEnumerable<int> ExecutingCalls => GetIndices(_executingHeadIdx);
        
        private IEnumerable<int> GetIndices(int? head)
        {
            if (head == null)
                yield break;

            for (int? i = head; i != null; i = _nodes[i.Value].NextIdx)
                yield return i.Value;
        }
#endif
    }
}
