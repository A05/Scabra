using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Scabra
{
    public abstract class Endpoint : IEndpointLogger, IDisposable
    {
        private static readonly NetMqSocketFactory SocketFactoryInstance = new();

        protected readonly NetMqSocketFactory SocketFactory;
        protected readonly CancellationTokenSource CancellationTokenSource;
        protected readonly CancellationToken CancellationToken;
        protected EndpointState State;

        private readonly ILogger _logger;

        public string Address { get; }

        public EndpointCrashedEventHandler Crashed;

        protected Endpoint(string address, ILogger logger)
        {
            Address = address;

            SocketFactory = SocketFactoryInstance;

            CancellationTokenSource = new CancellationTokenSource();
            CancellationToken = CancellationTokenSource.Token;

            _logger = logger;
        }

        public virtual void Start() => throw new NotSupportedException();

        public virtual void Dispose()
        {
            if (State == EndpointState.Disposing || State == EndpointState.Disposed)
                return;

            State = EndpointState.Disposing;

            try
            {
                DisposeImpl();
            }
            catch (Exception ex)
            {
                LogError("Failed to dispose endpoint.", ex);
            }
            finally
            {
                State = EndpointState.Disposed;
            }
        }

        protected virtual void DisposeImpl() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void AssertNotStarted()
        {
            Debug.Assert(State == EndpointState.Unknown, $"Endpoint was already started: current state is {State}.");
        }

        protected void EnsureStarted(int timeoutInMs = 500)
        {
            var start = Environment.TickCount;

            while (State != EndpointState.Started)
            {
                if (Environment.TickCount - start >= timeoutInMs)
                    throw new ScabraException($"Endpoint starting timeout: {timeoutInMs} ms.");

                Thread.Sleep(0);
            }
        }

        #region IEndpointLogger

        void IEndpointLogger.LogInfo(string message, params object[] args) => LogInfo(message, args);
        void IEndpointLogger.LogWarning(string message, params object[] args) => LogWarning(message, args);
        void IEndpointLogger.LogError(string message, params object[] args) => LogError(message, args);
        void IEndpointLogger.LogError(Exception ex, string message, params object[] args) => LogError(message, ex, args);

        #endregion

        protected virtual void OnCrashed(EndpointCrashedEventArgs args)
        {
            Crashed?.Invoke(this, args);
        }

        protected void LogInfo(string message, params object[] args)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                var enrichedMessage = Enrich(message);
                var enrichedArgs = Enrich(args);

                _logger.LogInformation(enrichedMessage, enrichedArgs);
            }
        }

        protected void LogWarning(string message, params object[] args)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                var enrichedMessage = Enrich(message);
                var enrichedArgs = Enrich(args);

                _logger.LogWarning(enrichedMessage, enrichedArgs);
            }
        }

        protected void LogError(string message, params object[] args)
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                var enrichedMessage = Enrich(message);
                var enrichedArgs = Enrich(args);

                _logger.LogError(enrichedMessage, enrichedArgs);
            }
        }

        protected void LogError(string message, Exception ex = null, params object[] args)
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                var enrichedMessage = Enrich(message);
                var enrichedArgs = Enrich(args);

                if (ex == null)
                    _logger.LogError(enrichedMessage, enrichedArgs);
                else
                    _logger.LogError(ex, enrichedMessage, enrichedArgs);
            }
        }

        public override string ToString() => Address;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string Enrich(string message)
        {
            return "{Endpoint} | " + message;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private object[] Enrich(object[] args)
        {
            if (args == null)
                return new object[] { ToString() };

            var enrichedArgs = new object[args.Length + 1];            
            for (int i = 1; i < enrichedArgs.Length; i++)
                enrichedArgs[i] = args[i - 1];

            enrichedArgs[0] = ToString();

            return enrichedArgs;
        }
    }
}
