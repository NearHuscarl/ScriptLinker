namespace RoslynExamples
{
    /// <summary>
    /// https://github.com/dotnet/roslyn/wiki/Samples-and-Walkthroughs
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            //SyntaxAnalysis.ManualTraversal();
            //SyntaxAnalysis.QueryMethods();
            //SyntaxAnalysis.SyntaxWalkers();

            //SemanticAnalysis.SemanticModel();

            //SyntaxTransformation.ReplaceNamespace();
            SyntaxTransformation.UseTypeInference();

            //WorkspaceSample.Sample();
        }
    }
}
