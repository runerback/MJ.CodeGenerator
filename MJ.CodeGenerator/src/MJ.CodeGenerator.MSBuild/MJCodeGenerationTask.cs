using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace MJ.CodeGenerator.MsBuild
{
    public sealed class MJCodeGenerationTask : Microsoft.Build.Utilities.Task
    {
        private static readonly string AssemblyPath;

        static MJCodeGenerationTask()
        {
            AssemblyPath = Path.GetDirectoryName(typeof(MJCodeGenerationTask).Assembly.Location);
        }

        private readonly List<string> _generatedCodeFiles = new();
        private readonly List<string> _generatedPlainFiles = new();
        private readonly List<string> _generatedPaths = new();

        public string? ProjectDir { get; set; }

        public string? OutputPath { get; set; }

        public string? GeneratorOutputPath { get; set; }

        public string? RootNamespace { get; set; }

        public string? ExternalReferences { get; set; }

        public string? Debugging { get; set; }

        public string? Timeout { get; set; }

        public Microsoft.Build.Framework.ITaskItem[]? MJGenerators { get; set; }

        [Microsoft.Build.Framework.Output]
        public Microsoft.Build.Framework.ITaskItem[] GeneratedCodeFiles
            => _generatedCodeFiles.Select(it => new Microsoft.Build.Utilities.TaskItem(it)).ToArray();

        [Microsoft.Build.Framework.Output]
        public Microsoft.Build.Framework.ITaskItem[] GeneratedPlainFiles
            => _generatedPlainFiles.Select(it => new Microsoft.Build.Utilities.TaskItem(it)).ToArray();

        [Microsoft.Build.Framework.Output]
        public Microsoft.Build.Framework.ITaskItem[] GeneratedPaths
            => _generatedPaths.Select(it => new Microsoft.Build.Utilities.TaskItem(it)).ToArray();

        public override bool Execute()
        {
            try
            {
                LaunchDebugger();

                _generatedCodeFiles.Clear();
                _generatedPlainFiles.Clear();
                _generatedPaths.Clear();

                var host = Path.Combine(AssemblyPath, "MJ.CodeGenerator.Host.dll");
                if (!File.Exists(host))
                {
                    LogMessage("code generator host not found");
                    return true;
                }

                var generators = GeneratorIterator().Distinct().ToArray();
                if (generators.Length == 0)
                {
                    LogMessage("No valid code generator present");
                    return true;
                }

                var buildId = $"mjbuild_{Guid.NewGuid():N}";

                var argsBuilder = new StringBuilder();

                argsBuilder.Append($"{CommandLineOptionNames.BuildId} {buildId} ");
                argsBuilder.Append($"{CommandLineOptionNames.ProjectDir} {ProjectDir} ");
                argsBuilder.Append($"{CommandLineOptionNames.OutputPath} {OutputPath} ");
                argsBuilder.Append($"{CommandLineOptionNames.RootNamespace} {RootNamespace} ");
                argsBuilder.Append($"{CommandLineOptionNames.ExternalReferences} {ExternalReferences} ");
                argsBuilder.Append($"{CommandLineOptionNames.Generators} {string.Join(";", generators)} ");

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo("dotnet", $"{host} {argsBuilder}")
                    {
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                    },
                };

                var errorBuilder = new StringBuilder();
                var resultBuilder = new StringBuilder();

                process.ErrorDataReceived += (_, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e?.Data))
                    {
                        errorBuilder.AppendLine(e!.Data);
                    }
                };

                process.OutputDataReceived += (_, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e?.Data))
                    {
                        resultBuilder.AppendLine(e!.Data);
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                WaitForHost(process);

                if (errorBuilder.Length > 0)
                {
                    LogMessage(errorBuilder.ToString());
                    return false;
                }

                if (TryGetResult(resultBuilder.ToString(), buildId, out var result))
                {
                    _generatedCodeFiles.AddRange(result.GeneratedCodeFiles);
                    _generatedPlainFiles.AddRange(result.GeneratedPlainFiles);
                    _generatedPaths.AddRange(result.GeneratedPaths);
                }

                return true;
            }
            catch (Exception exp)
            {
                Log.LogErrorFromException(exp);
                return false;
            }
        }

        [DebuggerStepThrough]
        private void LaunchDebugger()
        {
            if (
                !string.IsNullOrWhiteSpace(Debugging) &&
                bool.TryParse(Debugging, out var debugging) &&
                debugging)
            {
                this.TryLaunchDebugger();
            }
        }

        private void WaitForHost(Process host)
        {
            var timeout = 30000;
            if (
                !string.IsNullOrWhiteSpace(Timeout) &&
                int.TryParse(Timeout, out var timeoutConfig) &&
                timeoutConfig > timeout)
            {
                timeout = timeoutConfig;
            }

            if (!host.WaitForExit(timeout))
            {
                var anyDebuggerAttachedToProcess = false;
                if (
                    CheckRemoteDebuggerPresent(host.Handle, ref anyDebuggerAttachedToProcess) &&
                    anyDebuggerAttachedToProcess)
                {
                    host.WaitForExit(); // debugging
                }
                else
                {
                    try
                    {
                        host.Kill();
                    }
                    catch { }
                }
            }
        }

        private IEnumerable<string> GeneratorIterator()
        {
            if (string.IsNullOrWhiteSpace(ProjectDir))
            {
                yield break;
            }

            if (!(MJGenerators?.Length > 0))
            {
                yield break;
            }

            var generatorOutputPath = Path.GetFullPath(Path.Combine(ProjectDir, GeneratorOutputPath, "generator"));

            foreach (var generatorItem in MJGenerators)
            {
                var generatorProject = generatorItem.ItemSpec;

                var generatorProjPath = Path.GetFullPath(Path.Combine(ProjectDir, generatorProject));
                if (!File.Exists(generatorProjPath))
                {
                    continue;
                }

                var targetFramework = generatorItem.GetMetadata("TargetFramework") ?? "netstandard2.0";
                var configuration = generatorItem.GetMetadata("Configuration") ?? "Debug";

                var outputFileName = generatorItem.GetMetadata("AssemblyName");
                if (string.IsNullOrWhiteSpace(outputFileName))
                {
                    outputFileName = Path.GetFileNameWithoutExtension(generatorProjPath);
                }

                var generatorPath = Path.Combine(generatorOutputPath, $"{outputFileName}.dll");
                if (!File.Exists(generatorPath))
                {
                    var dotnetCommandLine = $"build \"{generatorProjPath}\" --configuration {configuration} --framework {targetFramework} --output \"{generatorOutputPath}\"";
                    var dotnetBuild = new Process
                    {
                        StartInfo = new ProcessStartInfo("dotnet", dotnetCommandLine)
                        {
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                        },
                    };

                    var outputBuilder = new StringBuilder();
                    dotnetBuild.OutputDataReceived += (_, e) =>
                    {
                        outputBuilder.AppendLine(e.Data ?? "");
                    };
                    dotnetBuild.ErrorDataReceived += (_, e) =>
                    {
                        outputBuilder.AppendLine(e.Data ?? "");
                    };

                    LogMessage($"Begin build generator: {outputFileName}");
                    dotnetBuild.Start();
                    dotnetBuild.BeginOutputReadLine();
                    dotnetBuild.BeginErrorReadLine();

                    if (!dotnetBuild.WaitForExit(30000))
                    {
                        dotnetBuild.Kill();
                    }

                    if (!File.Exists(generatorPath))
                    {
                        LogMessage(outputBuilder.ToString());
                        LogMessage($"Generator build failed: {outputFileName}");
                        continue;
                    }

                    LogMessage($"Ready: {outputFileName}");
                }

                yield return generatorPath;
            }
        }

        private void LogMessage(string message)
        {
            Log.LogCriticalMessage("", "", "", "", 0, 0, 0, 0, message);
        }

        private bool TryGetResult(string output, string buildId, out MJCodeGenerationResult result)
        {
            result = default!;

            if (string.IsNullOrWhiteSpace(output))
            {
                return false;
            }

            var dataBlockPattern = Regex.Escape(string.Format(MJCodeGenerationResult.DataBegin, buildId)) +
                $@"(?<data>\w+)" +
                Regex.Escape(string.Format(MJCodeGenerationResult.DataFinish, buildId));

            var match = Regex.Match(output, dataBlockPattern);
            if (!match.Success)
            {
                return false;
            }

            var dataGroup = match.Groups["data"];
            if (!dataGroup.Success)
            {
                return false;
            }

            try
            {
                result = MJCodeGenerationResult.Deserialize(dataGroup.Value)!;

                return result != null && !result.IsEmpty;
            }
            catch (Exception exp)
            {
                Log.LogErrorFromException(exp);
                return false;
            }
        }

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, ref bool isDebuggerPresent);
    }
}