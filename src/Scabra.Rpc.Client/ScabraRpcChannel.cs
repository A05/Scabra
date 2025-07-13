using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Diagnostics;

namespace Scabra.Rpc.Client
{
    public sealed partial class ScabraRpcChannel : IScabraRpcChannel
    {
        private const string LOGGER_CATEGORY = nameof(Scabra);

        private readonly ILogger _logger;
        private readonly ClientEndpoint _endpoint;
        private readonly ClientMarshaller _marshaller;

        private bool _disposed;

        public ScabraRpcChannel(ScabraRpcChannelOptions options) : this(options, new NullScabraSecurityHandler(), NullLoggerFactory.Instance)
        {
        }

        public ScabraRpcChannel(ScabraRpcChannelOptions options, IScabraSecurityHandler securityHandler) : this(options, securityHandler, NullLoggerFactory.Instance)
        {
        }

        public ScabraRpcChannel(ScabraRpcChannelOptions options, ILoggerFactory loggerFactory) : this(options, new NullScabraSecurityHandler(), loggerFactory)
        {
        }

        public ScabraRpcChannel(ScabraRpcChannelOptions options, IScabraSecurityHandler securityHandler, ILoggerFactory loggerFactory)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (options.Address == null)
                throw new ArgumentException($"{options.Address} must be specified.", nameof(options));

            if (securityHandler == null)
                throw new ArgumentNullException(nameof(securityHandler));

            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));

            _logger = loggerFactory.CreateLogger(LOGGER_CATEGORY);
                        
            var serializer = new RpcProtoBufPayloadSerializer(Marshaller.MaxArgsLength);
            _marshaller = new ClientMarshaller(serializer, securityHandler);

            _endpoint = new ClientEndpoint(options.Address, _logger);
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
                _logger.LogError(ex, "Failed to stop Scabra channel.");
            }
            finally
            {
                _disposed = true;
            }
        }

        public T InvokeMethod<S, T>(string methodName, params object[] args) where S : class
        {
            return InvokeMethodImpl<S, T>(methodName, args);
        }

        public void InvokeMethod<S>(string methodName, params object[] args) where S : class
        {
            InvokeMethodImpl<S, Marshaller.Void>(methodName, args);
        }

        private T InvokeMethodImpl<S, T>(string methodName, params object[] args) where S : class
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ScabraRpcChannel));

            if (methodName == null)
                throw new ArgumentNullException(nameof(methodName));

            if (args == null)
                throw new ArgumentNullException(nameof(args));

            try
            {
                byte[] call = _marshaller.MarshalCall<S, T>(methodName, args);

                byte[] reply = _endpoint.DoRemoteProcedureCall(call);
                Debug.Assert(reply != null);

                return _marshaller.UnmarshalReply<T>(reply);
            }
            catch (ObjectDisposedException ex) when (ex.ObjectName == nameof(ClientEndpoint))
            {
                throw new ObjectDisposedException(nameof(ScabraRpcChannel));
            }
            catch (RpcScabraException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new RpcScabraException(RpcErrorCode.Internal, $"Failed to invoke {methodName} method of {typeof(S).FullName} service.", ex);
            }
        }
    }
}