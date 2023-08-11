using System.Threading;
using System.Threading.Tasks;

namespace MJ.CodeGenerator
{
    public interface IMJCodeGenerator
    {
        Task GenerateAsync(
            IMJCodeGenerationTracer tracer,
            IMJCodeGeneratorConfiguration configuration,
            ICommandLineOptions options,
            CancellationToken cancellationToken = default);

        Task Cleanup();
    }
}