using Orleans;
using Orleans.CodeGeneration;
using System.Runtime.Serialization;

namespace Your.Namespace.Here
{
    [Version(1)]
    [Alias("Your.Namespace.Here." + nameof(VersionedInterfaceTransactionMethod1))]
    public interface VersionedInterfaceTransactionMethod1 : IGrainWithStringKey
    {
        /// <summary>
        /// Do anything u want, nothing in fact.
        /// </summary>
        /// <returns></returns>
        [Transaction(TransactionOption.Suppress)]
        [Alias(nameof(DoThings))]
        Task DoThings();
    }
}