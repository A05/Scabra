using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Scabra.Rpc.Client
{
    internal class ClientMarshaller : Marshaller
    { 
        private readonly List<(Type serviceType, MethodInfo[] methodInfos)> _serviceDescriptors;

        public ClientMarshaller(IRpcPayloadSerializer payloadSerializer, IScabraSecurityHandler securityHandler) : base(payloadSerializer, securityHandler)
        {
            _serviceDescriptors = new List<(Type service, MethodInfo[] methodInfos)>(30);
        }

        public byte[] MarshalCall<S, T>(string methodName, object[] args)
        {
            Debug.Assert(methodName != null);
            Debug.Assert(args != null);

            if (args.Length > MaxArgsLength)
                throw new RpcScabraException(RpcErrorCode.InvalidArgument, $"Maximum number ({MaxArgsLength}) of arguments is exceeded.");

            var (serviceType, methodInfos) = _serviceDescriptors.FirstOrDefault(i => i.serviceType == typeof(S));

            if (serviceType == null)
            {
                List<MethodInfo> lMethodInfos = new();
                foreach (var iMethodInfo in GetServiceMethods(typeof(S))) 
                    lMethodInfos.Add(iMethodInfo);

                serviceType = typeof(S); methodInfos = lMethodInfos.ToArray();

                _serviceDescriptors.Add((serviceType, methodInfos));
            }

            var methodInfo = methodInfos.FirstOrDefault(i => i.Name == methodName);
            if (methodInfo == null)
                throw new RpcScabraException(RpcErrorCode.NotFound, $"Method {methodName} not found in {typeof(S).Name} type.");

            if (typeof(T) == typeof(Void) && methodInfo.ReturnType != typeof(void))
                throw new RpcScabraException(RpcErrorCode.InvalidArgument, $"Method {methodName} has return value.");

            if (typeof(T) != typeof(Void) && methodInfo.ReturnType == typeof(void))
                throw new RpcScabraException(RpcErrorCode.InvalidArgument, $"Method {methodName} does not have return value.");

            if (methodInfo.GetParameters().Length != args.Length)
                throw new RpcScabraException(RpcErrorCode.InvalidArgument, "Number of arguments in the call does not match the number of declared parameters.");

            return MarshalCallInternal(typeof(S).FullName, methodName, args);
        }

        internal byte[] MarshalCallInternal(string serviceName, string methodName, object[] args)
        {
            Debug.Assert(serviceName != null);
            Debug.Assert(methodName != null);
            Debug.Assert(args != null);

            var header = new CallHeader { Service = serviceName, Method = methodName };

            var creds = _securityHandler.GetSecret();
            header.Secret = _securityHandler.EncodeSecret(creds);

            header.ArgsIndices = _argsIndicesMarshaller.Marshal(args);

            using var stream = new MemoryStream();

            header.Serialize(stream);
            _payloadSerializer.SerializeArgs(stream, args);

            return stream.ToArray();
        }

        public T UnmarshalReply<T>(byte[] reply)
        {
            Debug.Assert(reply != null);

            using Stream stream = new MemoryStream(reply);
            
            var header = ReplyHeader.Deserialize(stream);

            if (header.ErrorCode != RpcErrorCode.Ok)
                throw new RpcScabraException(header.ErrorCode, header.ErrorDescription);

            if (header.IsReplyNull && !typeof(T).IsValueType)
                return default;

            if (typeof(T) == typeof(Void))
            {
                if (!header.IsReplyNull)
                    throw new RpcScabraException(RpcErrorCode.Internal, "Method handler returned some value but nothing is expected.");

                return default;
            }

            try
            {
                return (T)_payloadSerializer.Deserialize(typeof(T), stream);
            }
            catch (Exception ex)
            {
                throw new RpcScabraException(RpcErrorCode.Unimplemented, "Failed to deserialize the reply.", ex);
            }
        }
    }
}
