using Orleans;
using Orleans.CodeGeneration;
using System.Runtime.Serialization;

namespace Your.Namespace.Here
{
    [Version(1)]
    public interface VersionedInterface1 : IGrainWithStringKey
    {
    }
}