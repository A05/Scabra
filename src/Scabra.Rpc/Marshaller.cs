using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Scabra.Rpc
{
    public abstract class Marshaller
    {
        public readonly struct Void
        {
            public static readonly Void Instance = new();
        }

        protected internal class ArgsIndicesMarshaller
        {
            public ushort Marshal(object[] args)
            {
                ushort indices = 0;

                if (args != null)
                    for (ushort i = 0, r = 1; i < args.Length && i < MaxArgsLength; i++, r <<= 1)
                        if (args[i] != null)
                            indices |= r;

                return indices;
            }
        }

        public const ushort MaxArgsLength = 16;

        protected readonly ArgsIndicesMarshaller _argsIndicesMarshaller;
        protected readonly IRpcPayloadSerializer _payloadSerializer;
        protected readonly IScabraSecurityHandler _securityHandler;

        protected Marshaller(IRpcPayloadSerializer payloadSerializer, IScabraSecurityHandler securityHandler)
        {
            Debug.Assert(payloadSerializer != null);
            Debug.Assert(securityHandler != null);

            _payloadSerializer = payloadSerializer;
            _securityHandler = securityHandler;

            _argsIndicesMarshaller = new ArgsIndicesMarshaller();
        }

        protected IEnumerable<MethodInfo> GetServiceMethods(Type serviceType)
        {
            if (!serviceType.IsInterface)
                throw new RpcScabraException(RpcErrorCode.Unimplemented, "A service contract must be defined by an interface.");

            List<string> existingMethods = new(); // TODO: (NU) It's better to use Trie here.

            foreach (var interfaceType in serviceType.GetInterfaces().Concat(new[] { serviceType }))
            {
                foreach (var methodInfo in interfaceType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (existingMethods.Contains(methodInfo.Name))
                        throw new RpcScabraException(RpcErrorCode.Unimplemented, "Method overloading is not supported yet.");

                    existingMethods.Add(methodInfo.Name);

                    yield return methodInfo;
                }
            }
        }
    }
}
