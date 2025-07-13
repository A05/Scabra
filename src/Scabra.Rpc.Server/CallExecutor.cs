using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Scabra.Rpc.Server
{
    internal class CallExecutor
    {
        private readonly ServerMarshaller _marshaller;
        private readonly ILogger _logger;

        public CallExecutor(IScabraSecurityHandler securityHandler, ILogger logger) 
        {
            var serializer = new RpcProtoBufPayloadSerializer(Marshaller.MaxArgsLength);
            _marshaller = new ServerMarshaller(serializer, securityHandler, _logger);

            _logger = logger;
        }

        public void RegisterService(Type serviceType, object service)
        {
            _marshaller.RegisterService(serviceType, service);
        }

        public byte[] Execute(Call call)
        {
            Task<byte[]> execution = null;

            try
            {
                if (call.CancellationToken.IsCancellationRequested)
                    return CallCancelled();

                execution = Task.Run(() => Execute(call.CallData));

                if (!execution.Wait(call.TimeoutInMs, call.CancellationToken))
                    return CallTimedOut();
                
                if (execution.IsFaulted)
                {
                    _logger.LogError(execution.Exception, "Unexpected failure during call execution.");
                    return InternalError();
                }

                Debug.Assert(execution.Result != null);
                return execution.Result;
            }
            catch (OperationCanceledException)
            {
                return CallCancelled();
            }
            finally
            {
                try { execution?.Dispose(); } catch { }
            }
        }

        private byte[] Execute(byte[] callData)
        {
            object reply = null;
            Exception callException = null;

            var originalPrincipal = Thread.CurrentPrincipal;

            try
            {
                _marshaller.UnmarshalCall(callData, out var invoker, out object[] args);

                try
                {
                    reply = invoker.Invoke(args);

                    // 'reply' may be null if either the target method's
                    // return type is void or the target method returned null.
                }
                catch (Exception ex)
                {
                    callException = ex;

                    _logger.LogError(ex, "Failed to execute handler method.");
                }
            }
            catch (RpcScabraException ex)
            {
                callException = ex;

                _logger.LogError(ex, "Failed to execute handler method.");
            }
            finally
            {
                Thread.CurrentPrincipal = originalPrincipal;
            }

            return _marshaller.MarshalReply(reply, callException);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte[] InternalError()
        {
            var rpcException = new RpcScabraException(RpcErrorCode.Internal);
            return _marshaller.MarshalReply(reply: null, rpcException);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte[] CallTimedOut()
        {
            var rpcException = new RpcScabraException(RpcErrorCode.DeadlineExceeded, "Call has timed out.");
            return _marshaller.MarshalReply(reply: null, rpcException);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte[] CallCancelled()
        {
            var rpcException = new RpcScabraException(RpcErrorCode.Aborted, "Call has been cancelled.");
            return _marshaller.MarshalReply(reply: null, rpcException);
        }
    }
}
