using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace MJ.CodeGenerator.Host
{
    internal sealed class MJCodeGeneratorResolver
    {
        private static readonly Type GeneratorType = typeof(IMJCodeGenerator);

        public IEnumerable<IMJCodeGenerator> Resolve(string? generators)
        {
            if (string.IsNullOrWhiteSpace(generators))
            {
                yield break;
            }

            foreach (var reference in generators?.Split(new[] { ',', ';' }) ?? Array.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(reference) || !File.Exists(reference))
                {
                    continue;
                }

                Assembly assembly;

                try
                {
                    assembly = Assembly.LoadFrom(reference);
                }
                catch
                {
                    continue;
                }

                foreach (var type in assembly.GetExportedTypes())
                {
                    if (!type.IsClass || type.IsAbstract)
                    {
                        continue;
                    }

                    if (!GeneratorType.IsAssignableFrom(type))
                    {
                        continue;
                    }

                    IMJCodeGenerator generator;

                    try
                    {
                        var constructor = type.GetConstructor(Type.EmptyTypes);
                        if (constructor?.Invoke(null) is not IMJCodeGenerator instance)
                        {
                            continue;
                        }

                        generator = instance;
                    }
                    catch
                    {
                        continue;
                    }

                    yield return generator;
                }
            }
        }
    }
}