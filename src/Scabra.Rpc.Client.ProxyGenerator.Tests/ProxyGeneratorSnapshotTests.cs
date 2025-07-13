using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using Scabra.Researches.Codegen.Laguna;
using System.Collections.Generic;
using System.Threading.Tasks;
using VerifyNUnit;

namespace Scabra.Rpc.Client
{
    [TestFixture]
    public class ProxyGeneratorSnapshotTests
    {
        [Test]
        public Task should_generate_all_required()
        {
            var source = @"
using Scabra.Researches.Codegen.Laguna;
using Scabra.Rpc.Client;

namespace Scabra.ProxyGenerator2.Tests
{
    [RpcClientProxy]
    public partial class SomeServiceScabraProxy2 : ISomeService
    {
    }
}";
            return Verify(source);
        }

        [Test]
        public Task should_ignore_proxy_class_access_modifier()
        {
            var source = @"
using Scabra.Researches.Codegen.Laguna;
using Scabra.Rpc.Client;

namespace Scabra.ProxyGenerator2.Tests
{
    [RpcClientProxy]
    internal partial class SomeServiceScabraProxy3 : ISomeService
    {
    }

    [RpcClientProxy]
    partial class SomeServiceScabraProxy4 : ISomeService
    {
    }
}";
            return Verify(source);
        }

        [Test]
        public Task should_not_generate_proxy_if_partial_keyword_is_missing()
        {
            var source = @"
using Scabra.Researches.Codegen.Laguna;
using Scabra.Rpc.Client;

namespace Scabra.ProxyGenerator2.Tests
{
    [RpcClientProxy]
    public class SomeServiceScabraProxy : ISomeService
    {
    }
}";
            return Verify(source);
        }

        [Test]
        public Task should_generate_sealed_proxy()
        {
            var source = @"
using Scabra.Researches.Codegen.Laguna;
using Scabra.Rpc.Client;

namespace Scabra.ProxyGenerator2.Tests
{
    [RpcClientProxy]
    sealed partial class SomeServiceScabraProxy : ISomeService
    {
    }
}";
            return Verify(source);
        }

        [Test]
        public Task should_generate_abstact_proxy()
        {
            var source = @"
using Scabra.Researches.Codegen.Laguna;
using Scabra.Rpc.Client;

namespace Scabra.ProxyGenerator2.Tests
{
    [RpcClientProxy]
    abstract partial class SomeServiceScabraProxy : ISomeService
    {
    }
}";
            return Verify(source);
        }

        [Test]
        public Task should_not_generate_static_proxy()
        {
            var source = @"
using Scabra.Researches.Codegen.Laguna;
using Scabra.Rpc.Client;

namespace Scabra.ProxyGenerator2.Tests
{
    [RpcClientProxy]
    static partial class SomeServiceScabraProxy : ISomeService
    {
    }
}";
            return Verify(source);
        }

        [Test]
        public Task should_not_generate_proxy_if_there_are_more_than_one_base_types()
        {
            var source = @"
using Scabra.Researches.Codegen.Laguna;
using Scabra.Rpc.Client;

namespace Scabra.ProxyGenerator2.Tests
{
    [RpcClientProxy]
    partial class SomeServiceScabraProxy : ISomeService, AnotherBaseType
    {
    }
}";
            return Verify(source);
        }

        [Test]
        public Task should_generate_proxy_if_interface_is_declared_locally()
        {
            var source = @"
using Scabra.Researches.Codegen.Laguna;
using Scabra.Rpc.Client;

namespace Scabra.ProxyGenerator2.Tests
{
    public interface ISomeOtherService
    {
        int JustDoIt();
    }

    [RpcClientProxy]
    partial class SomeServiceScabraProxy : ISomeOtherService
    {
    }
}";
            return Verify(source);
        }

        [Test]
        public Task should_not_generate_proxy_if_marker_attribute_is_missing()
        {
            var source = @"
using Scabra.Researches.Codegen.Laguna;
using Scabra.Rpc.Client;

namespace Scabra.ProxyGenerator2.Tests
{
    public interface ISomeOtherService
    {
        int JustDoIt();
    }

    partial class SomeServiceScabraProxy : ISomeOtherService
    {
    }
}";
            return Verify(source);
        }

        private static Task Verify(string source)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);

            IEnumerable<PortableExecutableReference> references = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ISomeService).Assembly.Location)
            };

            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName: "Tests",
                syntaxTrees: new[] { syntaxTree },
                references: references);

            var generator = new ProxyGenerator();

            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            driver = driver.RunGenerators(compilation);

            return Verifier.Verify(driver);
        }
    }
}
