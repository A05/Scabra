using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Scabra.Rpc.Server
{
    internal class ServerMarshaller : Marshaller
    {
        private class MethodDescriptor
        {
            public Type[] ParameterTypes { get; set; }

            public Func<object[], object> Invoker { get; set; }
        }

        // TODO: (NU) It's better to use Trie here.
        private readonly Dictionary<string, Dictionary<string, MethodDescriptor>> _serviceDescriptors = new();
        private readonly ILogger _logger;

        public ServerMarshaller(IRpcPayloadSerializer payloadSerializer, IScabraSecurityHandler securityHandler, ILogger logger) : base(payloadSerializer, securityHandler)
        {
            _logger = logger;
        }

        public void RegisterService(Type serviceType, object service)
        {
            Debug.Assert(serviceType != null);
            Debug.Assert(service != null);

            if (!serviceType.IsAssignableFrom(service.GetType()))
                throw new RpcScabraException(RpcErrorCode.InvalidArgument, $"The provided service is not of {serviceType.FullName} type.");

            var serviceKey = serviceType.FullName;

            if (_serviceDescriptors.ContainsKey(serviceKey))
                throw new RpcScabraException(RpcErrorCode.AlreadyExists, $"Service {serviceKey} is already registered.");

            var mDescriptors = new Dictionary<string, MethodDescriptor>();
            var methodInfos = GetServiceMethods(serviceType);
            foreach (var methodInfo in methodInfos)
            {
                var parameterTypes = methodInfo.GetParameters().Select(p => p.ParameterType).ToArray();
                if (parameterTypes.Length > MaxArgsLength)
                    throw new RpcScabraException(RpcErrorCode.Unimplemented, $"Maximum number ({MaxArgsLength}) of arguments is exceeded.");

                mDescriptors[methodInfo.Name] = new MethodDescriptor
                {
                    ParameterTypes = parameterTypes,
                    Invoker = (args) => methodInfo.Invoke(service, args)
                };
            }

            _serviceDescriptors[serviceKey] = mDescriptors;
        }

        public void UnmarshalCall(byte[] data, out Func<object[], object> invoker, out object[] args)
        {
            Debug.Assert(data != null);

            using var stream = new MemoryStream(data);

            var header = CallHeader.Deserialize(stream);

            if (!_serviceDescriptors.TryGetValue(header.Service, out var mRecords))
                throw new RpcScabraException(RpcErrorCode.NotFound, $"Service {header.Service} is not registered.");

            if (!mRecords.TryGetValue(header.Method, out MethodDescriptor mRecord))
                throw new RpcScabraException(RpcErrorCode.NotFound, $"Method {header.Method} of {header.Service} service is not registered.");

            var secret = _securityHandler.DecodeSecret(header.Secret);

            try
            {
                _securityHandler.TakeSecurityMeasures(secret);
            }
            catch (Exception ex)
            {
                throw new RpcScabraException(RpcErrorCode.Unauthenticated, "Failed to authenticated a remote call.", ex);
            }

            invoker = mRecord.Invoker;

            try
            {
                args = _payloadSerializer.DeserializeArgs(stream, mRecord.ParameterTypes, header.ArgsIndices);
            }
            catch (Exception ex)
            {
                throw new RpcScabraException(RpcErrorCode.Unimplemented, "Failed to deserialize method arguments.", ex);
            }
        }

        public byte[] MarshalReply(object reply, Exception callException)
        {
            var header = new ReplyHeader() 
            {
                ErrorCode = RpcErrorCode.Ok,
                IsReplyNull = reply == null
            };

            using var stream = new MemoryStream();

            if (callException == null)
            {
                header.Serialize(stream);

                try
                {
                    _payloadSerializer.Serialize(stream, reply);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to serialize reply.");

                    header.ErrorCode = RpcErrorCode.Unimplemented;
                    header.ErrorDescription = "Failed to serialize reply.";

                    stream.SetLength(0);
                    header.Serialize(stream);
                }
            }
            else 
            {
                // 'callException' is already logged befor marshalling.

                if (callException is RpcScabraException rpcEx)
                {
                    header.ErrorCode = rpcEx.ErrorCode;
                    header.ErrorDescription = rpcEx.Message;
                }
                else
                {
                    // 'Unknown' code is used here because 'Internal' code
                    // means an error of Scabra itself, but ex is an error
                    // of executing the target method.

                    header.ErrorCode = RpcErrorCode.Unknown;
                    header.ErrorDescription = "Exception was thrown by handler.";
                }

                header.Serialize(stream);
            }

            return stream.ToArray();
        }
    }
}
