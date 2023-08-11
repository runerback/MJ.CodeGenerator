using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;

namespace MJ.CodeGenerator
{
    public sealed class MJCodeGenerationResult
    {
        public const string DataBegin = "+++++++++++++{0}++++++++++++++";
        public const string DataFinish = "-------------{0}---------------";

        public MJCodeGenerationResult(
            string[]? generatedCodeFiles = default,
            string[]? generatedPlainFiles = default,
            string[]? generatedPaths = default)
        {
            GeneratedCodeFiles = generatedCodeFiles ?? Array.Empty<string>();
            GeneratedPlainFiles = generatedPlainFiles ?? Array.Empty<string>(); ;
            GeneratedPaths = generatedPaths ?? Array.Empty<string>(); ;
        }

        public string[] GeneratedCodeFiles { get; }

        public string[] GeneratedPlainFiles { get; }

        public string[] GeneratedPaths { get; }

        public bool IsEmpty => GeneratedCodeFiles.Length == 0 &&
            GeneratedPlainFiles.Length == 0 &&
            GeneratedPaths.Length == 0;

        public string Serialize(string rootName)
        {
            var root = new XDocument(new XElement(rootName,
                new XElement(nameof(GeneratedCodeFiles),
                    GeneratedCodeFiles.Select(it => new XElement("item", it)).ToArray()),
                new XElement(nameof(GeneratedPlainFiles),
                    GeneratedPlainFiles.Select(it => new XElement("item", it)).ToArray()),
                new XElement(nameof(GeneratedPaths),
                    GeneratedPaths.Select(it => new XElement("item", it)).ToArray())));

            using (var xmlStream = new MemoryStream())
            using (var compressed = new MemoryStream())
            using (var compressor = new GZipStream(compressed, CompressionLevel.Fastest))
            {
                root.Save(xmlStream, SaveOptions.OmitDuplicateNamespaces | SaveOptions.DisableFormatting);
                xmlStream.Flush();
                xmlStream.Seek(0, SeekOrigin.Begin);
                xmlStream.CopyTo(compressor);
                compressor.Flush();

                return new string(compressed.ToArray().SelectMany(it => it.ToString("x2")).ToArray());
            }
        }

        public static MJCodeGenerationResult? Deserialize(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return null;
            }

            try
            {
                var buffer = Enumerable.Range(0, data.Length / 2)
                    .Select(i => Convert.ToByte($"{data[i * 2]}{data[i * 2 + 1]}", 16))
                    .ToArray();

                string xml;
                using (var input = new MemoryStream(buffer))
                using (var decompressor = new GZipStream(input, CompressionMode.Decompress))
                using (var reader = new StreamReader(decompressor))
                {
                    xml = reader.ReadToEnd();
                }

                if (string.IsNullOrWhiteSpace(xml))
                {
                    return null;
                }

                var root = XDocument.Parse(xml)?.Root;
                if (root == null)
                {
                    return null;
                }

                string[]? generatedCodeFiles = default;
                string[]? generatedPlainFiles = default;
                string[]? generatedPaths = default;

                foreach (var element in root.Elements())
                {
                    switch (element.Name.LocalName)
                    {
                        case nameof(GeneratedCodeFiles):
                            generatedCodeFiles = element.Elements("item").Select(it => (string)it).ToArray();
                            break;

                        case nameof(GeneratedPlainFiles):
                            generatedPlainFiles = element.Elements("item").Select(it => (string)it).ToArray();
                            break;

                        case nameof(GeneratedPaths):
                            generatedPaths = element.Elements("item").Select(it => (string)it).ToArray();
                            break;

                        default:
                            break;
                    }
                }

                return new(generatedCodeFiles, generatedPlainFiles, generatedPaths);
            }
            catch
            {
                return null;
            }
        }
    }
}