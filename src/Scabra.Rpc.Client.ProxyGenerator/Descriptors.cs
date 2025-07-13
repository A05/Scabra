using System;
using System.Collections.Generic;
using System.Linq;

namespace Scabra.Rpc.Client
{
    internal sealed record ProxyDescriptor
    {
        public string[] Usings;
        public string NamespaceName;
        public string ClassName;
        public string InterfaceName;
        public MethodDescriptor[] MethodDescriptors;
    };

    internal sealed record MethodDescriptor
    {
        public string Name;
        public string ReturnTypeName;
        public ParameterDescriptor[] ParameterDescriptors;
    };

    internal sealed record ParameterDescriptor
    {
        public string Name;
        public string TypeName;
    };

    internal sealed class ProxyDescriptorEqualityComparer : IEqualityComparer<ProxyDescriptor>
    {
        public bool Equals(ProxyDescriptor x, ProxyDescriptor y)
        {
            if (x.NamespaceName != y.NamespaceName)
                return false;

            if (x.ClassName != y.ClassName)
                return false;

            if (x.InterfaceName != y.InterfaceName)
                return false;

            if (!x.Usings.SequenceEqual(y.Usings))
                return false;

            return x.MethodDescriptors.SequenceEqual(y.MethodDescriptors, new MethodDescriptorEqualityComparer());
        }

        public int GetHashCode(ProxyDescriptor obj)
        {
            throw new NotImplementedException();
        }
    }

    internal sealed class MethodDescriptorEqualityComparer : IEqualityComparer<MethodDescriptor>
    {
        public bool Equals(MethodDescriptor x, MethodDescriptor y)
        {
            if (x.Name != y.Name)
                return false;

            if (x.ReturnTypeName != y.ReturnTypeName)
                return false;

            return x.ParameterDescriptors.SequenceEqual(y.ParameterDescriptors);
        }

        public int GetHashCode(MethodDescriptor obj)
        {
            throw new NotImplementedException();
        }
    }
}