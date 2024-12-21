using System.Text.RegularExpressions;
using SilkWrapped.ObjectModelTool.Rewriters;

namespace SilkWrapped.ObjectModelTool.GeneratorTransforms;
internal class ExtractObjectModelTypes : GeneratorTransformBase
{
    public List<CSharpSyntaxRewriter> Rewriters { get; set; } = [];

    public override async Task TransformAsync(GeneratorTransformContext context, CancellationToken cancellationToken)
    {
        if (context.Decompiler is not Decompiler decompiler) return;
        if (decompiler.GetSyntax(context.ApiTypeSymbol) is not CompilationUnitSyntax apiSyntax) return;

        var apiMethods = apiSyntax.DescendantNodes()
                                  .OfType<MethodDeclarationSyntax>()
                                  .Select(m => m.WithBody(Block()));

        var handleTypeNameExclusionPattern = new Regex(context.Generator.HandleTypeNameExclusionPattern, RegexOptions.Compiled);

        var methodGroups = new Dictionary<string, List<MethodDeclarationSyntax>>();


        foreach (var method in apiMethods)
        {
            if (cancellationToken.IsCancellationRequested) return;
            if (method is not { ParameterList.Parameters: [var firstParameter, ..] }) continue;

            if (TypeName(firstParameter.Type) is string name)
            {
                if (handleTypeNameExclusionPattern.IsMatch(name)) continue;

                if (methodGroups.TryGetValue(name, out var methodGroup))
                {
                    methodGroup.Add(method);
                }
                else
                {
                    methodGroups[name] = [method];
                }
            }
        }

        foreach (var (name, methods) in methodGroups)
        {
            string className = string.Format(context.Generator.ObjectModelNameFormatString!, name);
            var classDec = ClassDeclaration(className
                                , SyntaxKind.PublicKeyword, SyntaxKind.UnsafeKeyword, SyntaxKind.PartialKeyword)
                                .AddMembers(methods);

            var namespaceDec = FileScopedNamespaceDeclaration(ParseName(context.Project.DefaultNamespace!))
                .AddMembers(classDec);

            SyntaxNode typeSyntax = apiSyntax.WithMembers([namespaceDec]);

            foreach (var rewriter in Rewriters)
            {
                typeSyntax = rewriter switch
                {
                    ContextAwareCSharpSyntaxRewriter contextRewriter => contextRewriter.Visit(typeSyntax, context),
                    _ => rewriter.Visit(typeSyntax)
                };
            }

            var type = ParseTypeName(className);
            var sourceType = methods[0].ParameterList.Parameters[0].Type;

            await context.AddItem(null, className, typeSyntax, type, sourceType!, cancellationToken, true);
            context.SkipMarhsalling(className);
            context.SkipMarhsalling(name);

        }

    }
}
