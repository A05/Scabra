namespace Scabra.Rpc
{
    public static class RpcErrorCodeExtensions
    {
        public static string GetDescription(this RpcErrorCode code)
        {
            // These descriptions were grabbed from gRPC.

            return code switch
            {
                RpcErrorCode.Unknown => "Unknown error.",
                RpcErrorCode.Ok => "Not an error; returned on success.",
                RpcErrorCode.Cancelled => "The operation was cancelled.",
                RpcErrorCode.InvalidArgument => "Client specified an invalid argument.",
                RpcErrorCode.DeadlineExceeded => "The deadline expired before the operation could complete.",
                RpcErrorCode.NotFound => "Some requested entity was not found.",
                RpcErrorCode.AlreadyExists => "Some entity that we attempted to create already exists.",
                RpcErrorCode.PermissionDenied => "The caller does not have permission to execute the specified operation.",
                RpcErrorCode.Unauthenticated => "The call does not have valid authentication credentials for the operation.",
                RpcErrorCode.Aborted => "The operation was aborted, typically due to a concurrency issue like sequencer check failures.",
                RpcErrorCode.OutOfRange => "Operation was attempted past the valid range.",
                RpcErrorCode.Unimplemented => "Operation is not implemented or not supported/enabled in this service.",
                RpcErrorCode.Internal => "Internal error.",
                RpcErrorCode.Unavailable => "The service is currently unavailable.",
                
                _ => throw new System.NotSupportedException()
            };
        }
    }
}
