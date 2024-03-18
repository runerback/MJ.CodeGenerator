using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Abstractions;

namespace MJ.CodeGenerator.Analyzers.Tests
{
    public class NamedAliasAttributeAnalyzerTest(ITestOutputHelper output)
    {
        private readonly ITestOutputHelper _output = output;

        [Theory]
        [InlineData(
            /*description*/ "AliasClass",
            /*code*/ "./Samples/AliasClass/SerializableClass1.cs",
            /*expectedFix*/ "./Samples/AliasClass/SerializableClass1_fixed.cs")]
        [InlineData(
            /*description*/ "AliasClass",
            /*code*/ "./Samples/AliasClass/VersionedInterface1.cs",
            /*expectedFix*/ "./Samples/AliasClass/VersionedInterface1_fixed.cs")]
        [InlineData(
            /*description*/ "AliasClass",
            /*code*/ "./Samples/AliasClass/VersionedInterfaceTransactionMethod1.cs",
            /*expectedFix*/ "./Samples/AliasClass/VersionedInterfaceTransactionMethod1_fixed.cs")]
        public async Task VerifyGeneratedDiagnostic(string description, string codeOrFile, string? expectedFixedCodeOrFile)
        {
            _output.WriteLine($"description: {description}");

            var code = codeOrFile;
            if (File.Exists(codeOrFile))
            {
                code = await File.ReadAllTextAsync(codeOrFile);
            }

            var expectedFixedCode = expectedFixedCodeOrFile;
            if (
                !string.IsNullOrWhiteSpace(expectedFixedCodeOrFile) &&
                File.Exists(expectedFixedCodeOrFile))
            {
                expectedFixedCode = await File.ReadAllTextAsync(expectedFixedCodeOrFile);
            }

            var context = new AnalyzerTestContext();
            var (diagnostics, sourceCode, project) = await context.GetDiagnosticsAsync<GenerateNamedAliasAttributesAnalyzer>(code);

            Assert.NotEmpty(diagnostics);
            Assert.Single(diagnostics);

            var diagnostic = diagnostics.First();
            Assert.Equal(GenerateNamedAliasAttributesAnalyzer.RuleId, diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Info, diagnostic.Severity);

            Assert.NotEmpty(project.Documents);
            var fixedCode = await context.ApplyCodeFix<GenerateNamedAliasAttributesCodeFix>(
                diagnostic,
                project.Documents.First());

            if (string.IsNullOrEmpty(expectedFixedCode))
            {
                Assert.Equal(sourceCode, fixedCode);
            }
            else
            {
                Assert.Equal(expectedFixedCode, fixedCode);
            }
        }
    }
}