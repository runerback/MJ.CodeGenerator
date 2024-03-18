using Microsoft.CodeAnalysis;

namespace MJ.CodeGenerator.Analyzers
{
    internal readonly record struct AttributeArgumentBag<T>(T Value, Location Location);
}