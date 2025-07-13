using System;

namespace Scabra.Rpc
{
    public class RpcScabraException : ScabraException
    {
        public RpcErrorCode ErrorCode { get; }

        public RpcScabraException(RpcErrorCode errorCode) : this(errorCode, errorCode.GetDescription())
        {
        }

        public RpcScabraException(RpcErrorCode errorCode, string message) : this(errorCode, message, null)
        {
        }

        public RpcScabraException(RpcErrorCode errorCode, string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
