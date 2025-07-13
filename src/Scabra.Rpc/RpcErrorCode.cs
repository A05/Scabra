namespace Scabra.Rpc
{
    // These error codes were grabbed from gRPC.

    public enum RpcErrorCode
    {
        Unknown = 0,
        Ok,
        Cancelled,
        InvalidArgument,
        DeadlineExceeded,
        NotFound,
        AlreadyExists,
        PermissionDenied,
        Unauthenticated,
        Aborted,
        OutOfRange,
        Unimplemented,
        Internal,
        Unavailable
    }
}
