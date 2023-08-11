using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MJ.CodeGenerator.Host
{
    internal sealed class MJCodeGenHostedService : IHostedService
    {
        private readonly IServiceProvider _services;
        private readonly ICommandLineOptions _options;
        private readonly ILogger _logger;

        private int? _exitcode;

        public MJCodeGenHostedService(
            IServiceProvider services,
            ICommandLineOptions options,
            ILogger<MJCodeGenHostedService> logger)
        {
            _services = services;
            _options = options;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Begin generating code . . .");

                var configuration = await ReadConfiguration() ?? new MJCodeGeneratorConfiguration();
                if (!configuration.Disabled)
                {
                    if (configuration.HostDebugging)
                    {
                        configuration.TryLaunchDebugger();
                    }

                    foreach (var generator in new MJCodeGeneratorResolver().Resolve(_options.Generators))
                    {
                        try
                        {
                            await ExecuteAsync(generator, configuration, cancellationToken);
                        }
                        catch (Exception exp)
                        {
                            _logger.LogError(exp, nameof(ExecuteAsync));
                        }
                        finally
                        {
                            try
                            {
                                await generator.Cleanup();
                            }
                            catch (Exception exp)
                            {
                                _logger.LogError(exp, nameof(generator.Cleanup));
                            }
                        }
                    }
                }

                _exitcode = 0;
                _logger.LogInformation("done");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Code generation failed");
            }
            finally
            {
                _services.GetRequiredService<IHostApplicationLifetime>().StopApplication();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Environment.ExitCode = _exitcode.GetValueOrDefault(-1);

            return Task.CompletedTask;
        }

        private async Task<MJCodeGeneratorConfiguration?> ReadConfiguration()
        {
            var projectDir = _options.ProjectDir!;
            if (string.IsNullOrWhiteSpace(projectDir) || !Directory.Exists(projectDir))
            {
                return null;
            }

            var configurationFile = Path.Combine(projectDir, "codegen.json");
            if (!File.Exists(configurationFile))
            {
                configurationFile = Path.Combine(projectDir, "appsettings.json");
                if (!File.Exists(configurationFile))
                {
                    return null;
                }
            }

            return await MJCodeGeneratorConfiguration.LoadFrom(configurationFile);
        }

        private async Task ExecuteAsync(
            IMJCodeGenerator generator,
            IMJCodeGeneratorConfiguration configuration,
            CancellationToken cancellationToken = default)
        {
            var buildId = _options.BuildId;
            if (string.IsNullOrWhiteSpace(buildId))
            {
                return;
            }

            var outputPath = Path.Join(_options.ProjectDir, _options.OutputPath);

            var tracer = new MJCodeGenerationTracer(_logger);

            await generator.GenerateAsync(tracer, configuration, _options, cancellationToken);

            await tracer.ExecuteGenerationTasks(cancellationToken);

            var generatedCodeFiles = tracer.GeneratedFiles
                .Where(it => it.StartsWith(outputPath))
                .Distinct()
                .ToArray();

            var generatedPlainFiles = new HashSet<string>();

            if (configuration.Logging)
            {
                var logFile = Path.Combine(outputPath, "MJTestCodeGenerationTask.log");

                using (var writer = new StreamWriter(logFile))
                {
                    await writer.WriteLineAsync($"{nameof(_options.ProjectDir)}: {_options.ProjectDir}");
                    await writer.WriteLineAsync($"{nameof(_options.OutputPath)}: {_options.OutputPath}");
                    await writer.WriteLineAsync($"{nameof(_options.RootNamespace)}: {_options.RootNamespace}");
                    await writer.WriteLineAsync($"{nameof(_options.ExternalReferences)}: {_options.ExternalReferences}");

                    var tracedLogs = tracer.Logs;
                    if (tracedLogs.Count > 0)
                    {
                        foreach (var log in tracedLogs)
                        {
                            await writer.WriteLineAsync(log);
                        }
                    }
                }

                generatedPlainFiles.Add(logFile);
            }

            var generatedPaths = Enumerable.Empty<string>()
                .Concat(generatedCodeFiles)
                .Concat(generatedPlainFiles)
                .Select(it => Path.GetDirectoryName(it)!)
                .Distinct()
                .Where(it => it != outputPath)
                .OrderByDescending(it => it)
                .ToArray();

            var result = new MJCodeGenerationResult(
                generatedCodeFiles,
                generatedPlainFiles.ToArray(),
                generatedPaths);

            if (result.IsEmpty)
            {
                return;
            }

            var builder = new StringBuilder();

            builder.AppendFormat(MJCodeGenerationResult.DataBegin, buildId);
            builder.Append(result.Serialize(buildId));
            builder.AppendFormat(MJCodeGenerationResult.DataFinish, buildId);

            _logger.LogInformation("{data}", builder);
        }
    }
}