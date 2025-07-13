namespace Scabra.Benchmarking.Rpc
{
    public interface IBenchmarkableRpcService
    {
        void NoParametersNoReturn();

        int PrimitiveParametersPrimitiveReturn(int parameter1, int parameter2, int parameter3);

        ComplexEntity ComplexParametersComplexReturn(ComplexEntity parameter1, ComplexEntity parameter2, ComplexEntity parameter3);
    }
}
