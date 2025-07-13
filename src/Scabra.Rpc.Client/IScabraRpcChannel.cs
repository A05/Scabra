using System;

namespace Scabra.Rpc.Client
{
    public interface IScabraRpcChannel : IDisposable
    {
        T InvokeMethod<S, T>(string methodName, params object[] args) where S : class;

        void InvokeMethod<S>(string methodName, params object[] args) where S : class;
    }
}
