using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Abstractions;

namespace MJ.CodeGenerator.Analyzers.Tests
{
    public class RegenerateSerializationAttributesAnalyzerTest(ITestOutputHelper output)
    {
        private readonly ITestOutputHelper _output = output;

        [Theory]
        [InlineData(
            /*description*/ "SerializableClass",
            /*code*/ @"[GenerateSerializer] public class D { [Id(0)] public int a; [Id(1)] public int b; [Id(1)] public int c; }",
            /*expectedFix*/ @"[GenerateSerializer] public class D { [Id(0)] public int a; [Id(1)] public int b; [Id(2)] public int c; }")]
        [InlineData(
            /*description*/ "SerializableClass",
            /*code*/ "./Samples/SerializableClass/Class1.cs",
            /*expectedFix*/ "./Samples/SerializableClass/Class1_fixed.cs")]
        [InlineData(
            /*description*/ "SerializableClass",
            /*code*/ "./Samples/SerializableClass/Class2.cs",
            /*expectedFix*/ "./Samples/SerializableClass/Class2_fixed.cs")]
        [InlineData(
            /*description*/ "SerializableClass",
            /*code*/ "./Samples/SerializableClass/Class3.cs",
            /*expectedFix*/ "./Samples/SerializableClass/Class3_fixed.cs")]
        [InlineData(
            /*description*/ "SerializableStruct",
            /*code*/ @"[GenerateSerializer] public struct D { [Id(0)] public int f; [Id(1)] public int b; [Id(1)] public int c; }",
            /*expectedFix*/ @"[GenerateSerializer] public struct D { [Id(0)] public int f; [Id(1)] public int b; [Id(2)] public int c; }")]
        [InlineData(
            /*description*/ "SerializableRecord",
            /*code*/ @"[GenerateSerializer] public record D { [Id(0)] public int f; [Id(1)] public int b; [Id(1)] public int c; }",
            /*expectedFix*/ @"[GenerateSerializer] public record D { [Id(0)] public int f; [Id(1)] public int b; [Id(2)] public int c; }")]
        [InlineData(
            /*description*/ "SerializableRecordStruct",
            /*code*/ @"[GenerateSerializer] public record struct D { [Id(0)] public int f; [Id(1)] public int b; [Id(1)] public int c;  }",
            /*expectedFix*/ @"[GenerateSerializer] public record struct D { [Id(0)] public int f; [Id(1)] public int b; [Id(2)] public int c;  }")]
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
            var (diagnostics, sourceCode, project) = await context.GetDiagnosticsAsync<RegenerateSerializerAttributeAnalyzer>(code);

            Assert.NotEmpty(diagnostics);
            Assert.Single(diagnostics);

            var diagnostic = diagnostics.First();
            Assert.Equal(RegenerateSerializerAttributeAnalyzer.RuleId, diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Info, diagnostic.Severity);

            Assert.NotEmpty(project.Documents);
            var fixedCode = await context.ApplyCodeFix<RegenerateOrleansSerializationAttributesCodeFix>(
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