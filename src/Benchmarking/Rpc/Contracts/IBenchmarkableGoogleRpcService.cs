using System.ServiceModel;
using System.Threading.Tasks;

namespace Scabra.Benchmarking.Rpc
{
    [ServiceContract]
    public interface IBenchmarkableGoogleRpcService
    {
        [OperationContract]
        Task NoParametersNoReturn();

        [OperationContract]
        Task<PrimitiveReply> PrimitiveParametersPrimitiveReturn(PrimitiveCall call);

        [OperationContract]
        Task<ComplexEntity> ComplexParametersComplexReturn(ComplexCall call);
    }
}
