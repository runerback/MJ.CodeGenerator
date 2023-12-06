using Orleans;
using System.Runtime.Serialization;

namespace Your.Namespace.Here
{
    /// <summary>
    /// your summary here
    /// <see cref="Class1" />
    /// </summary>
    [GenerateSerializer]
    [Serializable]
    [DataContract]
    [Immutable]
    public class Class1
    {
        /// <summary>
        /// <see cref="Field1" />
        /// </summary>
        [DataMember]
        [Id(0)]
        public int Field1 { get; set; }

        /// <summary>
        /// Field2
        /// </summary>
        [DataMember]
        [Id(1)]
        public string Field2 { get; set; }
    }
}