using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp;

namespace SilkWrapped.ObjectModelTool.GeneratorTransforms;
internal class ExtractObjectModelTypes : GeneratorTransformBase
{
    public override Task TransformAsync(GeneratorTransformContext context, CancellationToken cancellationToken)
    {
        if (context.Decompiler is not Decompiler decompiler) return Task.CompletedTask;
        if (decompiler.GetSyntax(context.ApiTypeSymbol) is not CompilationUnitSyntax apiSyntax) return Task.CompletedTask;

        var apiMethods = apiSyntax.DescendantNodes()
                                  .OfType<MethodDeclarationSyntax>()
                                  .Select(m => m.WithBody(Block()));

        var handleTypeNameExclusionPattern =  new Regex(context.Config.HandleTypeNameExclusionPattern, RegexOptions.Compiled);

        var methodGroups = new Dictionary<string, List<MethodDeclarationSyntax>>();


        foreach (var method in apiMethods) 
        {
            if (cancellationToken.IsCancellationRequested) return Task.FromCanceled(cancellationToken);
            if (method is not  { ParameterList.Parameters: [var firstParameter, ..] }) continue;

            if(TypeName(firstParameter.Type) is string name)
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
            string className = string.Format(context.Config.ObjectModelNameFormatString!, name);
            var classDec = ClassDeclaration(className
                                , SyntaxKind.PublicKeyword, SyntaxKind.UnsafeKeyword, SyntaxKind.PartialKeyword)
                                .AddMembers(methods);

            var namespaceDec = FileScopedNamespaceDeclaration(ParseName(context.Project.DefaultNamespace!))
                .AddMembers(classDec);

            var fileName = Path.Combine(context.Config.OutputPath, $"{className}.cs");
            var document = context.Project.AddDocument(fileName, apiSyntax.WithMembers([namespaceDec]).NormalizeWhitespace().GetText());
            context.Project = document.Project;
        }

        return Task.CompletedTask;
    }
}
