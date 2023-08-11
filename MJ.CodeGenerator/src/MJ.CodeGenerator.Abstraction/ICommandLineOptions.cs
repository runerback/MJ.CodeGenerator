namespace MJ.CodeGenerator
{
    public static class CommandLineOptionNames
    {
        public const string BuildId = "--build-id";
        public const string ProjectDir = "--project-dir";
        public const string OutputPath = "--output-path";
        public const string RootNamespace = "--root-ns";
        public const string ExternalReferences = "--ext-refs";
        public const string Generators = "-generators";
    }

    public interface ICommandLineOptions
    {
        string? BuildId { get; }
        string? ProjectDir { get; }
        string? OutputPath { get; }
        string? RootNamespace { get; }
        string? ExternalReferences { get; }
        string? Generators { get; }
    }
}