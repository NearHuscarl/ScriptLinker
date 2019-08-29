using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynExamples.Collector;
using System;
using System.IO;
using System.Linq;

namespace RoslynExamples
{
    static class SyntaxAnalysis
    {
        public static void ManualTraversal()
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HelloWorld.cs");
            var text = File.ReadAllText(filePath);

            var tree = CSharpSyntaxTree.ParseText(text);
            var root = (CompilationUnitSyntax)tree.GetRoot();
            var firstMember = root.Members[0];
            var helloWorldDeclaration = (NamespaceDeclarationSyntax)firstMember;
            var programDeclaration = (ClassDeclarationSyntax)helloWorldDeclaration.Members[0];
            var mainDeclaration = (MethodDeclarationSyntax)programDeclaration.Members[0];
            var argsParameter = mainDeclaration.ParameterList.Parameters[0];

            var className = programDeclaration.Identifier.ValueText;
            var methodName = mainDeclaration.Identifier.ValueText;
            var argsName = argsParameter.Identifier.ValueText;
            var argsType = argsParameter.Type.ToString();
            //var argsName = argsParameter.Type.;

            Console.WriteLine(text);
            Console.WriteLine();
            Console.WriteLine($"Class name: {className}");
            Console.WriteLine($"Method name: {methodName}");
            Console.WriteLine($"Arg name: {argsName}, Type: {argsType}");
            Console.ReadLine();
        }

        public static void QueryMethods()
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HelloWorld.cs");
            var text = File.ReadAllText(filePath);

            var tree = CSharpSyntaxTree.ParseText(text);
            var root = (CompilationUnitSyntax)tree.GetRoot();
            var firstParameters =
                from methodDeclaration in root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                where methodDeclaration.Identifier.ValueText == "Main"
                select methodDeclaration.ParameterList.Parameters.First();

            var argsParameter2 = firstParameters.Single();

            Console.WriteLine(text);
            Console.WriteLine();
            Console.WriteLine($"Arg: {argsParameter2}");
            Console.ReadLine();
        }

        public static void SyntaxWalkers()
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NamespaceTest.cs");
            var text = File.ReadAllText(filePath);
            var tree = CSharpSyntaxTree.ParseText(text);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var collector = new UsingCollector();
            collector.Visit(root.Members[0]);

            Console.WriteLine(text);
            Console.WriteLine();
            Console.WriteLine("Using:");
            foreach (var directive in collector.Results)
            {
                Console.WriteLine(directive.Name);
            }
            Console.ReadLine();
        }
    }
}
