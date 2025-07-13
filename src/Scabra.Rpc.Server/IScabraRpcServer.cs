using System;

namespace Scabra.Rpc.Server
{
    public interface IScabraRpcServer : IDisposable
    {
        event ScabraRpcServerCrashedEventHandler Crashed;

        void Start();

        void RegisterService<TService>(TService service);

        void RegisterService(Type serviceType, object service);
    }
}
