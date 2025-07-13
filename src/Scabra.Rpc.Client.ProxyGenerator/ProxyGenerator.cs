using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Scabra.Rpc.Client
{
    [Generator]
    public class ProxyGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var attributeBuilder = new ProxyAttributeCodeBuilder();

            var proxyDescriptors = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    attributeBuilder.GetFullyQualifiedAttributeName(),
                    predicate: (node, _) => node is ClassDeclarationSyntax,
                    transform: GetProxyDescriptor)
                .Where(i => i != default);

            proxyDescriptors = proxyDescriptors.WithComparer(new ProxyDescriptorEqualityComparer());

            context.RegisterSourceOutput(proxyDescriptors, AddProxyClassSource);

            context.RegisterPostInitializationOutput(ctx =>
            {
                var sourceText = attributeBuilder.BuildSourceText();
                var sourceFileName = attributeBuilder.GetClassName() + ".g.cs";
                
                ctx.AddSource(sourceFileName, SourceText.From(sourceText, Encoding.UTF8));
            });
        }

        static ProxyDescriptor GetProxyDescriptor(GeneratorAttributeSyntaxContext context, CancellationToken _)
        {
            var classDeclarationSyntax = (ClassDeclarationSyntax)context.TargetNode;
            if (classDeclarationSyntax.Modifiers.Any(SyntaxKind.StaticKeyword))
                return default;

            if (!classDeclarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword))
                return default;

            if (classDeclarationSyntax.BaseList.Types.Count != 1) 
                return default;

            var baseTypeSyntax = classDeclarationSyntax.BaseList.Types[0];
            
            var identifierNameSyntax = baseTypeSyntax.Type as IdentifierNameSyntax;
            if (identifierNameSyntax == null) 
                return default;
            
            var identifierSymbolInfo = context.SemanticModel.GetSymbolInfo(identifierNameSyntax);
            if (identifierSymbolInfo.Symbol == null)
                return default;

            if (identifierSymbolInfo.Symbol is not INamedTypeSymbol serviceInterfaceTypeSymbol)
                return default;

            if (serviceInterfaceTypeSymbol.TypeKind != TypeKind.Interface)
                return default;

            var methodDescriptors = new List<MethodDescriptor>();
            var usings = new List<string>();

            foreach (var symbol in serviceInterfaceTypeSymbol.GetMembers())
            {
                if (symbol is not IMethodSymbol methodSymbol)
                    return default;

                var methodDescriptor = getMethodDescriptor(methodSymbol, usings);
                methodDescriptors.Add(methodDescriptor);
            }

            var @namespace = collectNamespacesAndUsings(classDeclarationSyntax.Parent, usings);
            if (@namespace == null)
                return default;

            return new ProxyDescriptor()
            {
                Usings = usings.Distinct().OrderBy(i => i).ToArray(),
                NamespaceName = @namespace,
                ClassName = classDeclarationSyntax.Identifier.ValueText,
                InterfaceName = serviceInterfaceTypeSymbol.Name,
                MethodDescriptors = methodDescriptors.ToArray()
            };

            static MethodDescriptor getMethodDescriptor(IMethodSymbol ms, List<string> usings)
            {
                usings.Add(ms.ReturnType.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));

                var md = new MethodDescriptor()
                {
                    Name = ms.Name,
                    ReturnTypeName = ms.ReturnType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)
                };

                md.ParameterDescriptors = new ParameterDescriptor[ms.Parameters.Length];

                for (var i = 0; i < ms.Parameters.Length; i++)
                {
                    var p = ms.Parameters[i];

                    md.ParameterDescriptors[i] = new ParameterDescriptor()
                    {
                        Name = p.Name,
                        TypeName = p.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)
                    };

                    usings.Add(p.Type.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                }

                return md;
            }

            static string collectNamespacesAndUsings(SyntaxNode node, List<string> usings)
            {
                if (node == null)
                    return string.Empty;

                if (node is CompilationUnitSyntax cus)
                {
                    var topLevelUsings = getUsings(node);
                    usings.AddRange(topLevelUsings);

                    return string.Empty;
                }
                else if (node is NamespaceDeclarationSyntax nds)
                {
                    var namespaceLevelUsings = getUsings(node);
                    usings.AddRange(namespaceLevelUsings);

                    var @namespace = collectNamespacesAndUsings(nds.Parent, usings);
                    if (@namespace != null)
                        if (@namespace == string.Empty)
                            return nds.Name.ToString();
                        else
                            return @namespace + "." + nds.Name.ToString();
                }

                return null;
            }

            static IEnumerable<string> getUsings(SyntaxNode node)
            {
                var lUsings = node.ChildNodes()
                    .OfType<UsingDirectiveSyntax>()
                    .Select(i => i.Name?.ToString())
                    .Where(i => i != null);

                return lUsings;
            }
        }

        static void AddProxyClassSource(SourceProductionContext context, ProxyDescriptor d)
        {
            var builder = new ProxyCodeBuilder();

            var sourceText = builder.BuildSourceText(d);
            var sourceFileName = d.ClassName + ".g.cs";

            context.AddSource(sourceFileName, SourceText.From(sourceText, Encoding.UTF8));
        }
    }
}
