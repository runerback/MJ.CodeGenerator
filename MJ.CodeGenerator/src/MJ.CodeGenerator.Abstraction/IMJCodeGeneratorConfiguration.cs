namespace MJ.CodeGenerator
{
    public interface IMJCodeGeneratorConfiguration
    {
        bool Disabled { get; }

        bool Debugging { get; }

        bool Logging { get; }

        /// <summary>
        /// `JObject`
        /// </summary>
        object? Raw { get; }
    }
}