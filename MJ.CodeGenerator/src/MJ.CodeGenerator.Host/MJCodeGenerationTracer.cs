using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MJ.CodeGenerator.Host
{
    internal sealed class MJCodeGenerationTracer : IMJCodeGenerationTracer
    {
        private readonly ConcurrentBag<string> _generatedFiles = new();
        private readonly List<string> _logs = new();
        private readonly ConcurrentBag<Func<CancellationToken, Task<string>>> _generationTasks = new();

        private readonly ILogger _logger;

        public MJCodeGenerationTracer(ILogger logger)
        {
            _logger = logger;
        }

        public IReadOnlyList<string> GeneratedFiles => _generatedFiles.ToArray();

        public IReadOnlyList<string> Logs => _logs.ToArray();

        public void AddInfoLog(string info)
        {
            _logs.Add($"{LogHeader("Info")} {info}");
            _logger.LogInformation(info);
        }

        public void AddWarningLog(string warning)
        {
            _logs.Add($"{LogHeader("Warning")} {warning}");
            _logger.LogWarning(warning);
        }

        public void AddErrorLog(Exception exception, string? error = default)
        {
            var message = exception + (string.IsNullOrWhiteSpace(error) ? string.Empty : $"{Environment.NewLine}Additional info: {error}");

            _logs.Add($"{LogHeader("Error")} {message}");
            _logger.LogError(message);
        }

        public void AddGenerationTask(Func<CancellationToken, Task<string>> task)
        {
            _generationTasks.Add(task);
        }

        public async Task ExecuteGenerationTasks(CancellationToken cancellationToken = default)
        {
            try
            {
                await Parallel.ForEachAsync(_generationTasks, cancellationToken, async (task, token) =>
                {
                    await ExecuteGenerationTask(task, token);
                });
            }
            catch (Exception ex)
            {
                AddErrorLog(ex);
            }
        }

        private async Task ExecuteGenerationTask(
            Func<CancellationToken, Task<string>> task,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var filename = await task.Invoke(cancellationToken);
                if (string.IsNullOrWhiteSpace(filename))
                {
                    return;
                }

                _generatedFiles.Add(filename);
            }
            catch (Exception exp)
            {
                AddErrorLog(exp);
            }
        }

        private static string LogHeader(string type)
        {
            return $"[{type} {DateTime.Now:HH:mm:ss.fff}]";
        }
    }
}