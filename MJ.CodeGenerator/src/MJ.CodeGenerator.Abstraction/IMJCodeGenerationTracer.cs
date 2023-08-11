using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MJ.CodeGenerator
{
    public interface IMJCodeGenerationTracer
    {
        IReadOnlyList<string> GeneratedFiles { get; }

        IReadOnlyList<string> Logs { get; }

        void AddInfoLog(string info);

        void AddWarningLog(string warning);

        void AddErrorLog(Exception exception, string? error = default);

        void AddGenerationTask(Func<CancellationToken, Task<string>> task);
    }
}