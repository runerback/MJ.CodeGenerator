using Orleans;
using System.Runtime.Serialization;

namespace Your.Namespace.Here
{
    /// <summary>
    /// your summary here
    /// <see cref="Class2" />
    /// </summary>
    [Serializable]
    [DataContract]
    [Immutable]
    [GenerateSerializer]
    public class Class2
    {
        /// <summary>
        /// <see cref="OperationTime" />
        /// </summary>
        [DataMember]
        [Id(0)]
        public string OperationTime { get; set; } = "";
        /// <summary>
        /// <see cref="RoomFrom" />
        /// </summary>
        [DataMember]
        [Id(3)]
        public string RoomFrom { get; set; } = "";
        /// <summary>
        /// <see cref="RoomFrom" />
        /// </summary>
        [DataMember]
        [Id(4)]
        public string DeskFrom { get; set; } = "";
    }
}