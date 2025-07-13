using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;

namespace Scabra.Rpc.Server
{
    public sealed class ScabraRpcServer : IScabraRpcServer
    {
        private const string LOGGER_CATEGORY = nameof(Scabra);

        private readonly CallExecutor _callExecutor;
        private readonly ServerEndpoint _endpoint;
        private readonly ILogger _logger;

        private bool _started;
        private bool _disposed;

        public event ScabraRpcServerCrashedEventHandler Crashed;

        public ScabraRpcServer(ScabraRpcServerOptions options) : this(options, new NullScabraSecurityHandler(), NullLoggerFactory.Instance)
        {
        }

        public ScabraRpcServer(ScabraRpcServerOptions options, IScabraSecurityHandler securityHandler) : this(options, securityHandler, NullLoggerFactory.Instance)
        {
        }

        public ScabraRpcServer(ScabraRpcServerOptions options, ILoggerFactory loggerFactory) : this(options, new NullScabraSecurityHandler(), loggerFactory)
        {
        }

        public ScabraRpcServer(ScabraRpcServerOptions options, IScabraSecurityHandler securityHandler, ILoggerFactory loggerFactory)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (options.Address == null)
                throw new ArgumentException("Address must be specified.", nameof(options));

            if (securityHandler == null)
                throw new ArgumentNullException(nameof(securityHandler));

            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));

            _logger = loggerFactory.CreateLogger(LOGGER_CATEGORY);
                        
            _callExecutor = new CallExecutor(securityHandler, _logger);

            _endpoint = new ServerEndpoint(options.Address, _callExecutor, _logger);
            _endpoint.Crashed += (_, args) =>
            {
                var ex = new ScabraException($"Endpoint {_endpoint} crashed.", args.Exception);

                OnCrashed(new ScabraRpcServerCrashedEventArgs(ex));
            };
        }

        public void Start()
        {
            EnsureNotStartedAndNotDisposed();

            try
            {
                _endpoint.Start();

                _started = true;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Failed to start Scabra server.");
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
                _logger.LogError(ex, "Failed to stop Scabra server.");
            }
            finally
            {
                _disposed = true;
            }
        }

        void IScabraRpcServer.RegisterService<TService>(TService service)
        {
            EnsureNotStartedAndNotDisposed();

            (this as IScabraRpcServer).RegisterService(typeof(TService), service);
        }

        void IScabraRpcServer.RegisterService(Type serviceType, object service)
        {
            EnsureNotStartedAndNotDisposed();

            _callExecutor.RegisterService(serviceType, service);
        }

        private void EnsureNotStartedAndNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ScabraRpcServer));

            if (_started)
                throw new InvalidOperationException("Scabra server has been already started.");
        }

        private void OnCrashed(ScabraRpcServerCrashedEventArgs args)
        {
            Crashed?.Invoke(this, args);
        }
    }
}
