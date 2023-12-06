using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using Xunit;

namespace MJ.CodeGenerator.Analyzers.Tests
{
    internal sealed class AnalyzerTestContext
    {
        private static readonly string[] Usings = [
            "System",
            "System.Threading.Tasks",
            "Orleans",
            "MJ.CodeGenerator.Analyzers",
        ];

        public async Task<(Diagnostic[] diagnostics, string sourceCode, Project project)> GetDiagnosticsAsync<TDiagnosticAnalyzer>(
            string source,
            params string[] extraUsings)
            where TDiagnosticAnalyzer : DiagnosticAnalyzer, new()
        {
            var builder = new StringBuilder();
            BuildUsings(builder, extraUsings);
            builder.AppendLine(source);

            var sourceText = builder.ToString();

            var (diagnostics, project) = await GetDiagnosticsFullSourceAsync(sourceText, new TDiagnosticAnalyzer());

            return (diagnostics, sourceText, project);
        }

        public async Task<string> ApplyCodeFix<TCodeFixProvider>(
            Diagnostic diagnostic,
            Document document,
            params string[] extraUsings)
            where TCodeFixProvider : CodeFixProvider, new()
        {
            var codeFixProvider = new TCodeFixProvider();
            var actions = new List<CodeAction>();
            var context = new CodeFixContext(document,
                diagnostic,
                (a, _) => actions.Add(a),
                CancellationToken.None);
            await codeFixProvider.RegisterCodeFixesAsync(context);
            var changedSolution = document.Project.Solution;
            foreach (var codeAction in actions)
            {
                var operations = await codeAction.GetOperationsAsync(CancellationToken.None);
                if (operations.IsDefaultOrEmpty)
                {
                    continue;
                }

                foreach (var operation in operations.OfType<ApplyChangesOperation>())
                {
                    changedSolution = operation.ChangedSolution;
                    document.Project.Solution.Workspace.TryApplyChanges(changedSolution);
                }
            }

            var sourceText = (await changedSolution.Projects.First().Documents.First().GetTextAsync()).ToString();
            var usingsBuilder = new StringBuilder();
            BuildUsings(usingsBuilder, extraUsings);
            return sourceText.TrimEnd(['\r', '\n']).Replace(usingsBuilder.ToString(), "");
        }

        private async Task<(Diagnostic[] diagnostics, Project project)> GetDiagnosticsFullSourceAsync(string source, DiagnosticAnalyzer analyzer)
        {
            var project = CreateProject(source);
            var compilation = await project.GetCompilationAsync();
            var errors = compilation!.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error);

            Assert.Empty(errors);

            var compilationWithAnalyzers = compilation
                .WithOptions(
                    compilation.Options.WithSpecificDiagnosticOptions(
                        analyzer.SupportedDiagnostics.ToDictionary(d => d.Id, d => ReportDiagnostic.Default)))
                .WithAnalyzers(ImmutableArray.Create(analyzer));

            var diagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

            return (diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray(), project);
        }

        private static void BuildUsings(StringBuilder builder, params string[] extraUsings)
        {
            foreach (var @using in Usings.Concat(extraUsings))
            {
                builder.AppendLine($"using {@using};");
            }
        }

        private static Project CreateProject(string source)
        {
            const string fileName = "Test.cs";

            var projectId = ProjectId.CreateNewId(debugName: "TestProject");
            var documentId = DocumentId.CreateNewId(projectId, fileName);

            var assemblies = new[]
            {
                typeof(Task).Assembly,
                typeof(Orleans.IGrain).Assembly,
                typeof(Orleans.Grain).Assembly,
                typeof(Attribute).Assembly,
                typeof(int).Assembly,
                typeof(object).Assembly,
                typeof(RegenerateSerializerAttributeAnalyzer).Assembly,
            };

            var metadataReferences = assemblies
                .SelectMany(x => x.GetReferencedAssemblies().Select(Assembly.Load))
                .Concat(assemblies)
                .Distinct()
                .Select(x => MetadataReference.CreateFromFile(x.Location))
                .Cast<MetadataReference>()
                .ToList();

            var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
            metadataReferences.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "mscorlib.dll")));
            metadataReferences.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.dll")));
            metadataReferences.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Core.dll")));
            metadataReferences.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")));

            var solution = new AdhocWorkspace()
                .CurrentSolution
                .AddProject(projectId, "TestProject", "TestProject", LanguageNames.CSharp)
                .AddMetadataReferences(projectId, metadataReferences)
                .AddDocument(documentId, fileName, SourceText.From(source));

            return solution.GetProject(projectId)!
                .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        }
    }
}