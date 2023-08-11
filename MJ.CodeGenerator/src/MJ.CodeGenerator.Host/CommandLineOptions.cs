using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace MJ.CodeGenerator.Host
{
    internal sealed class CommandLineOptions : ICommandLineOptions
    {
        public static readonly Option<string?> BuildId = new(
            name: CommandLineOptionNames.BuildId,
            description: "build id")
        {
            IsRequired = true,
        };

        public static readonly Option<string?> ProjectDir = new(
            name: CommandLineOptionNames.ProjectDir,
            description: "project")
        {
            IsRequired = true,
        };

        public static readonly Option<string?> OutputPath = new(
            name: CommandLineOptionNames.OutputPath,
            description: "output folder under project dir")
        {
            IsRequired = true,
        };

        public static readonly Option<string?> RootNamespace = new(
            name: CommandLineOptionNames.RootNamespace,
            description: "root namespace")
        {
            IsRequired = false,
        };

        public static readonly Option<string?> ExternalReferences = new(
            name: CommandLineOptionNames.ExternalReferences,
            description: "project referenced assembly paths")
        {
            IsRequired = false,
        };

        public static readonly Option<string?> Generators = new(
            name: CommandLineOptionNames.Generators,
            description: "generator projects")
        {
            IsRequired = true,
        };

        private readonly InvocationContext _context;

        public CommandLineOptions(InvocationContext context)
        {
            _context = context;
        }

        public static IEnumerable<Option> Options
        {
            get
            {
                yield return BuildId;
                yield return ProjectDir;
                yield return OutputPath;
                yield return RootNamespace;
                yield return ExternalReferences;
                yield return Generators;
            }
        }

        string? ICommandLineOptions.BuildId => _context.ParseResult.GetValueForOption(BuildId);
        string? ICommandLineOptions.ProjectDir => _context.ParseResult.GetValueForOption(ProjectDir);
        string? ICommandLineOptions.OutputPath => _context.ParseResult.GetValueForOption(OutputPath);
        string? ICommandLineOptions.RootNamespace => _context.ParseResult.GetValueForOption(RootNamespace);
        string? ICommandLineOptions.ExternalReferences => _context.ParseResult.GetValueForOption(ExternalReferences);
        string? ICommandLineOptions.Generators => _context.ParseResult.GetValueForOption(Generators);
    }
}