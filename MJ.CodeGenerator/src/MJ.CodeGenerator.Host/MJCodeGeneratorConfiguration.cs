using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MJ.CodeGenerator.Host
{
    internal sealed class MJCodeGeneratorConfiguration : IMJCodeGeneratorConfiguration
    {
        public const string ConfigurationSectionName = "CodeGenerator";

        public bool Disabled { get; set; }

        public bool Debugging { get; set; }

        public bool Logging { get; set; }

        public object? Raw { get; internal set; }

        public static IMJCodeGeneratorConfiguration Empty() => new MJCodeGeneratorConfiguration() { Raw = new() };

        public static async Task<IMJCodeGeneratorConfiguration?> LoadFrom(string file)
        {
            string configuration;
            using (var reader = new StreamReader(file))
            {
                configuration = await reader.ReadToEndAsync();
            }

            var token = JToken.Parse(configuration);
            if (token.Type != JTokenType.Object)
            {
                return null;
            }

            var section = ((JObject)token).Property(ConfigurationSectionName, StringComparison.OrdinalIgnoreCase);
            if (section == null || !section.HasValues || section.Value is not JObject sectionValue)
            {
                return null;
            }

            try
            {
                var result = sectionValue.ToObject<MJCodeGeneratorConfiguration>();
                if (result != null)
                {
                    result.Raw = sectionValue.DeepClone();
                    return result;
                }
            }
            catch (Exception)
            {
            }

            return null;
        }
    }
}