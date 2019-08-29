using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.IO;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RoslynExamples
{
    static class SyntaxTransformation
    {
        public static void ReplaceNamespace()
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HelloWorld.cs");
            var text = File.ReadAllText(filePath);

            NameSyntax name = IdentifierName("System");
            name = QualifiedName(name, IdentifierName("Collections"));
            name = QualifiedName(name, IdentifierName("Generic"));

            SyntaxTree tree = CSharpSyntaxTree.ParseText(text);

            var root = (CompilationUnitSyntax)tree.GetRoot();

            var oldUsing = root.Usings[1];
            var newUsing = oldUsing.WithName(name);

            root = root.ReplaceNode(oldUsing, newUsing);

            Console.WriteLine(text);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("New code:");
            Console.WriteLine();
            Console.WriteLine(root.ToFullString());
            Console.ReadLine();
        }

        public static void UseTypeInference()
        {
            var test = CreateTestCompilation();

            foreach (var sourceTree in test.SyntaxTrees)
            {
                var model = test.GetSemanticModel(sourceTree);
                var rewriter = new TypeInferenceRewriter(model);
                var newSource = rewriter.Visit(sourceTree.GetRoot());

                if (newSource != sourceTree.GetRoot())
                {
                    File.WriteAllText(sourceTree.FilePath, newSource.ToFullString());
                }
            }
        }

        private static Compilation CreateTestCompilation()
        {
            var programPath = @"..\..\Program.cs";
            var programText = File.ReadAllText(programPath);
            var programTree = CSharpSyntaxTree.ParseText(programText).WithFilePath(programPath);

            var rewriterPath = @"..\..\TypeInferenceRewriter.cs";
            var rewriterText = File.ReadAllText(rewriterPath);
            var rewriterTree = CSharpSyntaxTree.ParseText(rewriterText).WithFilePath(rewriterPath);

            var sourceTrees = new SyntaxTree[] { programTree, rewriterTree };

            MetadataReference mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            MetadataReference codeAnalysis = MetadataReference.CreateFromFile(typeof(SyntaxTree).Assembly.Location);
            MetadataReference csharpCodeAnalysis = MetadataReference.CreateFromFile(typeof(CSharpSyntaxTree).Assembly.Location);

            var references = new MetadataReference[] { mscorlib, codeAnalysis, csharpCodeAnalysis };

            return CSharpCompilation.Create("TransformationCS",
                sourceTrees,
                references,
                new CSharpCompilationOptions(OutputKind.ConsoleApplication));
        }
    }
}
