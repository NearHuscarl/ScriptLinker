using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace RoslynExamples.Collectors
{
    class UnusedUsingCollector : CSharpSyntaxVisitor
    {
        public SemanticModel SemanticModel { get; set; }
        public List<UsingDirectiveSyntax> Results = new List<UsingDirectiveSyntax>();

        public UnusedUsingCollector(SemanticModel semanticModel)
        {
            SemanticModel = semanticModel;
        }

        private IEnumerable<INamespaceSymbol> GetNamespace(SyntaxNode node)
        {
            // TODO: filter symbol https://stackoverflow.com/questions/43469603/roslyn-find-all-symbols

            var symbol = SemanticModel.GetSymbolInfo(node).Symbol;

            if (symbol != null)
            {
                if (symbol.ContainingNamespace != null && !symbol.ContainingNamespace.IsGlobalNamespace)
                    yield return symbol.ContainingNamespace;
            }

            var typeSymbol = SemanticModel.GetTypeInfo(node).Type;

            if (typeSymbol != null)
            {
                if (typeSymbol.ContainingNamespace != null && !typeSymbol.ContainingNamespace.IsGlobalNamespace)
                    yield return typeSymbol.ContainingNamespace;
            }
        }

        public override void Visit(SyntaxNode node = null)
        {
            Results.Clear();

            var searchFromRootNode = node == null;
            var root = SemanticModel.SyntaxTree.GetRoot();

            if (searchFromRootNode)
            {
                node = root;
            }

            var activeUsings = node.DescendantNodes()
                .SelectMany(n => GetNamespace(n))
                .Distinct().ToList();
            var allUsings = ((CompilationUnitSyntax)root).Usings;

            //var generic = root.DescendantNodes().OfType<GenericNameSyntax>().First();
            //var genericInfo = SemanticModel.GetTypeInfo(generic);
            //var genericSymbol = (INamedTypeSymbol)genericInfo.Type;

            foreach (var _using in allUsings)
            {
                var ns = string.Empty;

                if (_using.StaticKeyword.Kind() == SyntaxKind.StaticKeyword) // Get the namespace minus the static class name
                {
                    ns = _using.Name.ChildNodes().ToList().First().ToString();
                }
                else
                    ns = _using.Name.ToString();

                if (!activeUsings.Any(au => au.ToString() == ns))
                {
                    Results.Add(_using);
                }
            }
        }
    }
}
