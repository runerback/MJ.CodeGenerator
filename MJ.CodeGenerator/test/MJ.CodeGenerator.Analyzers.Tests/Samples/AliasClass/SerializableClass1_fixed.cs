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
    [Alias("Your.Namespace.Here." + nameof(SerializableClass1))]
    public class SerializableClass1
    {
    }
}