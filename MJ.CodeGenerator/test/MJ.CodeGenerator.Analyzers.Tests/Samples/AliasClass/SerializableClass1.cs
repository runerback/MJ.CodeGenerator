using Orleans;
using System.Runtime.Serialization;

namespace Your.Namespace.Here
{
    /// <summary>
    /// your summary here
    /// <see cref="SerializableClass1" />
    /// </summary>
    [GenerateSerializer]
    [Serializable]
    [DataContract]
    [Immutable]
    public class SerializableClass1
    {
    }
}