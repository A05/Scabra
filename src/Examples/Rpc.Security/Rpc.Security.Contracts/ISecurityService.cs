namespace Scabra.Examples.Rpc.Security
{
    public interface ISecurityService
    {
        string GetAuthToken(string name, string password);
    }
}
