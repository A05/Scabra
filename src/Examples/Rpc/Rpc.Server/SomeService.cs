using System;

namespace Scabra.Examples.Rpc
{
    public class SomeService : ISomeService
    {
        public int DoAddition(int operand1, int operand2)
        {
            Console.WriteLine($"Accept call: operand1 = {operand1}, operand2 = {operand2}.");

            return operand1 + operand2;
        }
    }
}