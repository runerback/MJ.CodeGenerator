using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.Documentation;
using ICSharpCode.Decompiler.TypeSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MJ.CodeGenerator.Demo.Generator
{
    public class DemoGenerator : IMJCodeGenerator
    {
        public Task GenerateAsync(
            IMJCodeGenerationTracer tracer,
            IMJCodeGeneratorConfiguration configuration,
            ICommandLineOptions options,
            CancellationToken cancellationToken = default)
        {
            var externalRefs = options.ExternalReferences?.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (externalRefs?.Length > 0)
            {
                var outputFolder = Path.Combine(options.ProjectDir, options.OutputPath, "CodeGen");
                if (!Directory.Exists(outputFolder))
                {
                    Directory.CreateDirectory(outputFolder);
                }

                var reference = externalRefs[0];

                var decompiler = new CSharpDecompiler(externalRefs[0], new DecompilerSettings
                {
                    ThrowOnAssemblyResolveErrors = false,
                    LoadInMemory = true,
                    ShowXmlDocumentation = true,
                });

                var referenceDocFile = Path.Combine(Path.GetDirectoryName(reference), Path.GetFileNameWithoutExtension(reference) + ".xml");
                if (File.Exists(referenceDocFile))
                {
                    decompiler.DocumentationProvider = new XmlDocumentationProvider(referenceDocFile);
                }

                foreach (var type in decompiler.TypeSystem.MainModule.TypeDefinitions)
                {
                    if (type.Kind == TypeKind.Class && type.Namespace.StartsWith("MJ.CodeGenerator.Demo"))
                    {
                        var interfaceBuilder = new StringBuilder(); // or use roslyn
                        var interfaceType = $"I{type.Name}";

                        var wrapperTypeBuilder = new StringBuilder();
                        var wrapperType = $"{type.Name}Wrapper";

                        foreach (var headerLine in HeaderLineIterator())
                        {
                            interfaceBuilder.AppendLine(headerLine);
                            wrapperTypeBuilder.AppendLine(headerLine);
                        }

                        interfaceBuilder.AppendLine($"namespace {options.RootNamespace}");
                        interfaceBuilder.AppendLine("{");

                        wrapperTypeBuilder.AppendLine($"namespace {options.RootNamespace}");
                        wrapperTypeBuilder.AppendLine("{");

                        foreach (var typeDocLine in DocLineIterator(decompiler.DocumentationProvider.GetDocumentation(type)))
                        {
                            interfaceBuilder.AppendLine($"    {typeDocLine}");
                        }

                        interfaceBuilder.AppendLine($"    internal interface {interfaceType}");
                        interfaceBuilder.AppendLine("    {");

                        wrapperTypeBuilder.AppendLine($"    internal sealed class {wrapperType} : {type.FullName}, {interfaceType}");
                        wrapperTypeBuilder.AppendLine("    {");

                        foreach (var property in type.Properties)
                        {
                            foreach (var propDocLine in DocLineIterator(decompiler.DocumentationProvider.GetDocumentation(property)))
                            {
                                interfaceBuilder.AppendLine($"        {propDocLine}");
                            }

                            interfaceBuilder.Append($"        {GetPrimitiveTypeNameForDemo(property.ReturnType.FullName)} {property.Name} {{ ");

                            if (property.Getter != null)
                            {
                                interfaceBuilder.Append("get; ");
                            }

                            if (property.Setter != null)
                            {
                                interfaceBuilder.Append("set; ");
                            }

                            interfaceBuilder.AppendLine("}");
                            interfaceBuilder.AppendLine();
                        }

                        foreach (var method in type.Methods)
                        {
                            if (method.IsConstructor)
                            {
                                wrapperTypeBuilder.Append($"        public {wrapperType}(");
                                wrapperTypeBuilder.Append(string.Join(", ", method.Parameters.Select(param => $"{GetPrimitiveTypeNameForDemo(param.Type.FullName)} {param.Name}")));
                                wrapperTypeBuilder.Append(") : base(");
                                wrapperTypeBuilder.Append(string.Join(", ", method.Parameters.Select(param => param.Name)));
                                wrapperTypeBuilder.AppendLine(")");
                                wrapperTypeBuilder.AppendLine("        {");
                                wrapperTypeBuilder.AppendLine();
                                wrapperTypeBuilder.AppendLine("        }");
                                wrapperTypeBuilder.AppendLine();
                            }
                            else
                            {
                                foreach (var methodDocLine in DocLineIterator(decompiler.DocumentationProvider.GetDocumentation(method)))
                                {
                                    interfaceBuilder.AppendLine($"        {methodDocLine}");
                                }

                                interfaceBuilder.Append($"        {GetPrimitiveTypeNameForDemo(method.ReturnType.FullName)} {method.Name}(");

                                interfaceBuilder.Append(string.Join(", ", method.Parameters.Select(param =>
                                {
                                    var paramBuilder = new StringBuilder();

                                    if (param.IsRef)
                                    {
                                        paramBuilder.Append("ref ");
                                    }
                                    else if (param.IsOut)
                                    {
                                        paramBuilder.Append("out ");
                                    }

                                    paramBuilder.Append($"{GetPrimitiveTypeNameForDemo(param.Type.FullName)} ");
                                    paramBuilder.Append(param.Name);

                                    return paramBuilder.ToString();
                                })));

                                interfaceBuilder.AppendLine(");");
                                interfaceBuilder.AppendLine();
                            }
                        }

                        interfaceBuilder.AppendLine("    }");
                        interfaceBuilder.AppendLine("}");

                        var interfaceCode = interfaceBuilder.ToString();
                        var interfaceFile = Path.Combine(outputFolder, $"{interfaceType}.g.cs");

                        tracer.AddGenerationTask(async cancellationToken =>
                        {
                            using (var output = File.Create(interfaceFile))
                            using (var writer = new StreamWriter(output))
                            {
                                cancellationToken.ThrowIfCancellationRequested();

                                await writer.WriteAsync(interfaceCode);
                            }

                            return interfaceFile;
                        });

                        wrapperTypeBuilder.AppendLine("    }");
                        wrapperTypeBuilder.AppendLine("}");

                        var wrapperCode = wrapperTypeBuilder.ToString();
                        var wrapperFile = Path.Combine(outputFolder, $"{wrapperType}.g.cs");

                        tracer.AddGenerationTask(async cancellationToken =>
                        {
                            using (var output = File.Create(wrapperFile))
                            using (var writer = new StreamWriter(output))
                            {
                                cancellationToken.ThrowIfCancellationRequested();

                                await writer.WriteAsync(wrapperCode);
                            }

                            return wrapperFile;
                        });
                    }
                }
            }

            return Task.CompletedTask;
        }

        public Task Cleanup() => Task.CompletedTask;

        private static IEnumerable<string> HeaderLineIterator()
        {
            yield return "#nullable enable";
            yield return "// ";
            yield return "// auto-generated";
            yield return "// ";
        }

        private static IEnumerable<string> DocLineIterator(string doc)
        {
            if (string.IsNullOrWhiteSpace(doc))
            {
                yield break;
            }

            foreach (var line in doc.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                yield return $"/// {line.Trim().Replace("cref=\"T:", "cref=\"")}";
            }
        }

        private static string GetPrimitiveTypeNameForDemo(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                throw new ArgumentNullException(nameof(fullName));
            }

            return fullName switch
            {
                "System.Int32" => "int",
                "System.String" => "string",
                _ => fullName,
            };
        }
    }
}