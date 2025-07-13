using System.Threading.Tasks;

namespace Scabra.Benchmarking.Rpc
{
    public class BenchmarkableGoogleRpcService : IBenchmarkableGoogleRpcService
    {
        public Task NoParametersNoReturn()
        {
            return Task.CompletedTask;
        }

        public Task<PrimitiveReply> PrimitiveParametersPrimitiveReturn(PrimitiveCall call)
        {
            return Task.FromResult(new PrimitiveReply() { Value = 2548 });
        }

        public Task<ComplexEntity> ComplexParametersComplexReturn(ComplexCall call)
        {
            return Task.FromResult(new ComplexEntity()
            {
                Id = call.Parameter1.Id,
                Name = call.Parameter2.Name,
                Marks = call.Parameter3.Marks,
                Description = call.Parameter3.Description,
            });
        }
    }
}