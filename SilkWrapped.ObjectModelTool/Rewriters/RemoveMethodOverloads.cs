using Microsoft.CodeAnalysis.CSharp;

namespace SilkWrapped.ObjectModelTool.Rewriters;

internal class RemoveMethodOverloads : CSharpSyntaxRewriter
{
    public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        var methodGroups = node.Members.OfType<MethodDeclarationSyntax>().GroupBy(m => m.Identifier.Text).ToList();

        node = node.RemoveNodes(node.Members.OfType<MethodDeclarationSyntax>(), SyntaxRemoveOptions.KeepLeadingTrivia)!;


        var filteredMethods = new List<MethodDeclarationSyntax>();

        foreach (var methodGroup in methodGroups)
        {
            if (methodGroup.All(m => m.ParameterList.Parameters.Count == 1))
            {
                var stringMethod = methodGroup.FirstOrDefault(m => m.ParameterList.Parameters[0].Type?.ToString() == "string");

                if (stringMethod is not null)
                {
                    filteredMethods.Add(stringMethod);
                    continue;
                }

                var refParamMethod = methodGroup.FirstOrDefault(m => m.ParameterList.Parameters[0].Modifiers.Any(mod => mod.Text is "ref" or "in"));

                if (refParamMethod is not null)
                {
                    filteredMethods.Add(refParamMethod);
                    continue;
                }
            }

            filteredMethods.AddRange(methodGroup);
        }

        node = node.WithMembers(node.Members.AddRange(filteredMethods));

        return base.VisitClassDeclaration(node);
    }

    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        //Remove overloads that still have pointers
        if (node.ParameterList.Parameters.Any(p => p.Type is PointerTypeSyntax))
        {
            return null;
        }


        return base.VisitMethodDeclaration(node);
    }
}
