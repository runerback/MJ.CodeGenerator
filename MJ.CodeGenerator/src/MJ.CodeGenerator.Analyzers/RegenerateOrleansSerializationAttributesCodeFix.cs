using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Simplification;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MJ.CodeGenerator.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public class RegenerateOrleansSerializationAttributesCodeFix : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(RegenerateSerializerAttributeAnalyzer.RuleId);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
            var declaration = root!.FindNode(context.Span).FirstAncestorOrSelf<TypeDeclarationSyntax>()!;
            foreach (var diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case RegenerateSerializerAttributeAnalyzer.RuleId:
                        context.RegisterCodeFix(
                            CodeAction.Create("Regenerate serialization attributes", cancellationToken => RefereshSerializationAttributes(declaration, context, cancellationToken), equivalenceKey: RegenerateSerializerAttributeAnalyzer.RuleId),
                            diagnostic);
                        break;
                    default: break;
                }
            }
        }

        private static async Task<Document> RefereshSerializationAttributes(TypeDeclarationSyntax declaration, CodeFixContext context, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(context.Document, cancellationToken).ConfigureAwait(false);
            var analysis = SerializationAttributesHelper.AnalyzeTypeDeclaration(declaration);

            var nextId = analysis.LastOrderedId;
            foreach (var member in analysis.DisorderedMembers)
            {
                // Referesh the disordered [Id(x)] attribute
                var attribute = Attribute(ParseName(Constants.IdAttributeFullyQualifiedName))
                    .AddArgumentListArguments(AttributeArgument(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal((int)nextId++))))
                    .WithAdditionalAnnotations(Simplifier.Annotation);
                editor.ReplaceNode(member, (d, g) =>
                {
                    var n = d;
                    if (d is MemberDeclarationSyntax m && m.TryGetAttribute(Constants.IdAttributeName, out var p))
                    {
                        n = g.ReplaceNode(n, p, attribute);
                    }
                    return n;
                });
            }

            return editor.GetChangedDocument();
        }
    }
}