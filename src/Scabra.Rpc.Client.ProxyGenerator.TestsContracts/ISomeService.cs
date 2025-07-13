using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Scabra.Researches.Codegen
{
    using System.Data;

    namespace Laguna
    {
        using System.Buffers;

        namespace Sun
        {
            public class WrapperChi
            {
                public interface ISomeChiService
                {
                    IEnumerable<Timer> GetTimers();
                }
            }
        }

        public interface ISomeService
        {
            int DoAddition(int operand1, int operand2);

            int DoAdditionWithArray(Array operand1, IPinnable p, int operand2);

            int DoAdditionWithFile(FileStream operand1, int operand2);

            void MethodWithVoidReturnTypeAndSomeParameters(SomeParameterType parameter1);

            void MethodWithVoidReturnTypeAndNoParameters();
        }
    }

    public class SomeParameterType
    {
        public string Name { get; set; }
    }
}