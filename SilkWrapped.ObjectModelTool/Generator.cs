namespace SilkWrapped.ObjectModelTool;


internal class Generator
{
    public const string DefaultObjectModelNameFormatString = "{0}Wrapper";
    public const string DefaultConstructionMethodNamePattern = ".*(Create|Finish|Acquire).*";
    public const string DefaultDisposalMethodNamePattern = ".*(Release|Drop|Destroy).*";
    public const string DefaultHandleTypeNameExclusionPattern = "(Pfn).*|.*Descriptor";


    public required string ProjectFilePath { get; set; }
    public required string ApiTypeName { get; set; }
    public required string ApiOwnerTypeName { get; set; }
    public required string ConstructionMethodNamePattern { get; set; } = DefaultConstructionMethodNamePattern;
    public required string HandleTypeNameExclusionPattern { get; set; } = DefaultHandleTypeNameExclusionPattern;
    public required string DisposalMethodNamePattern { get; set; } = DefaultDisposalMethodNamePattern;
    public required string ObjectModelNameFormatString { get; set; } = DefaultObjectModelNameFormatString;
    public required string OutputPath { get; set; }
    public List<GeneratorTransformBase> TransformGenerators { get; set; } = [];
}
