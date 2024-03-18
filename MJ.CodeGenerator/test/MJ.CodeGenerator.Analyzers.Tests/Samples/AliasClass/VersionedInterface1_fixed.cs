using Orleans;
using Orleans.CodeGeneration;
using System.Runtime.Serialization;

namespace Your.Namespace.Here
{
    [Version(1)]
    [Alias("Your.Namespace.Here." + nameof(VersionedInterface1))]
    public interface VersionedInterface1 : IGrainWithStringKey
    {
    }
}