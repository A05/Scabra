namespace Scabra.Benchmarking.Rpc
{
    public class BenchmarkableRpcService : IBenchmarkableRpcService
    {
        public void NoParametersNoReturn()
        {
        }

        public int PrimitiveParametersPrimitiveReturn(int parameter1, int parameter2, int parameter3)
        {
            return 2548;
        }

        public ComplexEntity ComplexParametersComplexReturn(ComplexEntity parameter1, ComplexEntity parameter2, ComplexEntity parameter3)
        {
            return new ComplexEntity()
            {
                Id = parameter1.Id,
                Name = parameter2.Name,
                Marks = parameter3.Marks,
                Description = parameter3.Description,
            };
        }
    }
}