using Orleans;
using System.Runtime.Serialization;

namespace Your.Namespace.Here
{
    /// <summary>
    /// your summary here
    /// <see cref="Class3" />
    /// </summary>
    [Serializable]
    [DataContract]
    [Immutable]
    [GenerateSerializer]
    public class Class3
    {
        /// <summary>
        /// <see cref="OperationTime" />
        /// </summary>
        [DataMember]
        [Id(2)]
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