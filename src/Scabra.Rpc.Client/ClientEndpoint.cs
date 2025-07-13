using Microsoft.Extensions.Logging;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Scabra.Rpc.Client
{
    internal class ClientEndpoint : Endpoint
    {
        private const int CallTimeoutInMs = 1000; // TODO: (U) Pass it from the client. See gRPC.
        private const int CallsCompletionTimeoutInMs = 10_000; // TODO: (U) Calculate it from CallTimeoutInMs-s.
        private const int AbortingCallsCompletionTimeoutInMs = 100;
        private const int RoutineCompletionTimeoutInMs = 500;

        private readonly ClientCallBuffer _calls;
        private readonly Task _endpointRoutine;
        
        public ClientEndpoint(string address, ILogger logger) : base(address, logger)
        {
            _calls = new ClientCallBuffer(this);
            _endpointRoutine = Task.Factory.StartNew(EndpointRoutine, TaskCreationOptions.LongRunning);

            EnsureStarted();
        }

        protected override void DisposeImpl()
        {
            if (!_calls.WaitForCompletion(CallsCompletionTimeoutInMs))
            {
                LogError("{Timeout} ms timeout on waiting for calls to complete.", CallsCompletionTimeoutInMs);

                _calls.Abort();

                if (!_calls.WaitForCompletion(AbortingCallsCompletionTimeoutInMs))
                    LogError("{Timeout} ms timeout on waiting for aborting calls to complete.", AbortingCallsCompletionTimeoutInMs);
            }

            CancellationTokenSource.Cancel();

            if (_endpointRoutine.Wait(RoutineCompletionTimeoutInMs))
                _endpointRoutine.Dispose();
            else
                LogError("{Timeout} ms timeout on waiting for endpoint routine to complete.", RoutineCompletionTimeoutInMs);
        }

        public byte[] DoRemoteProcedureCall(byte[] callData)
        {
            if (State == EndpointState.Disposing || State == EndpointState.Disposed)
                throw new ObjectDisposedException(nameof(ClientEndpoint));

            if (State == EndpointState.Failed)
                throw new RpcScabraException(RpcErrorCode.Internal, "Endpoint is in failed state.");

            ClientCall call = null;

            try
            {
                call = _calls.AddPending(callData, CallTimeoutInMs);

                if (call.Wait())
                {
                    if (call.IsAborted)
                        throw new RpcScabraException(RpcErrorCode.Aborted, "Remote procedure call has been aborted.");
                    
                    return call.ReplyData;
                }
                else
                    throw new RpcScabraException(RpcErrorCode.Aborted, "Remote procedure call has been timed out.");
            }
            finally
            {
                if (call != null)
                    _calls.Remove(call);
            }
        }

        private void EndpointRoutine()
        {
            State = EndpointState.Started;

            try
            {
                while (!CancellationToken.IsCancellationRequested)
                {
                    using var socket = SocketFactory.CreateDealerSocket();
                    socket.Connect(Address);

                    while (!CancellationToken.IsCancellationRequested)
                    {
                        SendCalls(socket);
                        ReceiveReplies(socket);

                        Thread.Sleep(0);
                    }
                }
            }
            catch (Exception ex)
            {
                State = EndpointState.Failed;
                LogError("Endpoint routine crashed.", ex);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SendCalls(NetMqDealerSocket socket)
        {
            if (_calls.TryGetPendingForExecuting(out var call))
            {
                var (indexBytes, sequenceNumberBytes) = call.Id.ToBytes();

                socket.SendMore(indexBytes);
                socket.SendMore(sequenceNumberBytes);
                socket.Send(call.CallData);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReceiveReplies(NetMqDealerSocket socket)
        {   
            int frameCount = 0;
            byte[] frame0 = null, frame1 = null, frame2 = null;

            while (socket.HasIn)
            {
                var frame = socket.Receive(out bool hasMore);

                if (frameCount++ < 3)
                    if (frame0 == null)
                        frame0 = frame;
                    else if (frame1 == null)
                        frame1 = frame;
                    else
                        frame2 = frame;

                if (!hasMore)
                {
                    if (frameCount == 3)
                    {
                        var id = new ClientCallId(indexBytes: frame0, sequenceNumberBytes: frame1);

                        if (!_calls.TrySetReply(id, replyData: frame2))
                            LogWarning("Failed to set reply: call ({Id}) not found.", id);
                    }
                    else
                        LogError("Incorrect number {FrameCount} of message frames.", frameCount);

                    frameCount = 0;
                    frame0 = frame1 = null;
                }
            }
        }
    }
}
