using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace MJ.CodeGenerator.Analyzers
{
    internal static class SerializationAttributesHelper
    {
        public readonly record struct TypeAnalysis
        {
            public List<MemberDeclarationSyntax> UnannotatedMembers { get; init; }
            public List<MemberDeclarationSyntax> OrderedMembers { get; init; }
            public List<MemberDeclarationSyntax> DisorderedMembers { get; init; }
            public uint LastOrderedId { get; init; }
        }

        public static TypeAnalysis AnalyzeTypeDeclaration(TypeDeclarationSyntax declaration)
        {
            uint nextId = 0;
            var unannotatedSerializableMembers = new List<MemberDeclarationSyntax>();
            var orderedSerializableMembers = new List<MemberDeclarationSyntax>();
            var disorderedSerializableMembers = new List<MemberDeclarationSyntax>();

            foreach (var member in declaration.Members)
            {
                // Skip members with existing [Id(x)] attributes, but record the highest value of x so that newly added attributes can begin from that value.
                if (member.TryGetAttribute(Constants.IdAttributeName, out var attribute))
                {
                    var id = 0u;
                    var args = attribute.ArgumentList?.Arguments;
                    if (args.HasValue)
                    {
                        if (args.Value.Count > 0)
                        {
                            var idArg = args.Value[0];
                            if (
                                idArg.Expression is LiteralExpressionSyntax literalExpression &&
                                uint.TryParse(literalExpression.Token.ValueText, out var value))
                            {
                                id = value;
                            }
                        }
                    }

                    if (disorderedSerializableMembers.Count == 0 && id == nextId)
                    {
                        orderedSerializableMembers.Add(member);
                        nextId = id + 1;
                    }
                    else
                    {
                        disorderedSerializableMembers.Add(member);
                    }

                    continue;
                }

                if (member is ConstructorDeclarationSyntax constructorDeclaration && constructorDeclaration.HasAttribute(Constants.GenerateSerializerAttributeName))
                {
                    continue;
                }

                if (!member.IsInstanceMember() || !member.IsFieldOrAutoProperty() || member.HasAttribute(Constants.NonSerializedAttribute) || member.IsAbstract())
                {
                    // No need to add any attribute.
                    continue;
                }

                unannotatedSerializableMembers.Add(member);
            }

            return new TypeAnalysis
            {
                UnannotatedMembers = unannotatedSerializableMembers,
                OrderedMembers = orderedSerializableMembers,
                DisorderedMembers = disorderedSerializableMembers,
                LastOrderedId = nextId,
            };
        }
    }
}