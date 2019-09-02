using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.IO;
using System.Linq;

namespace RoslynExamples.Examples
{
    static class SemanticAnalysis
    {
        public static void SemanticModel()
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", "HelloWorld.cs");
            var text = File.ReadAllText(filePath);

            var tree = CSharpSyntaxTree.ParseText(text);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var compilation = CSharpCompilation
                .Create("HelloWorld")
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                .AddSyntaxTrees(tree);

            var model = compilation.GetSemanticModel(tree);
            var nameInfo = model.GetSymbolInfo(root.Usings[0].Name);
            var systemSymbol = (INamespaceSymbol)nameInfo.Symbol;

            Console.WriteLine("List of sub-namespace of the System namespace:");

            foreach (var ns in systemSymbol.GetNamespaceMembers())
            {
                Console.WriteLine(ns.Name);
            }

            var helloWorldString = root
                .DescendantNodes()
                .OfType<LiteralExpressionSyntax>()
                .First();

            var literalInfo = model.GetTypeInfo(helloWorldString);
            var stringTypeSymbol = (INamedTypeSymbol)literalInfo.Type;

            Console.WriteLine();
            Console.WriteLine("List of public methods in System.String:");

            foreach (var name in (from method in stringTypeSymbol.GetMembers().OfType<IMethodSymbol>()
                                  where method.ReturnType.Equals(stringTypeSymbol) &&
                                        method.DeclaredAccessibility == Accessibility.Public
                                  select method.Name).Distinct())
            {
                Console.WriteLine(name);
            }
            Console.ReadLine();
        }
    }
}
