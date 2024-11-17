using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilkWrapped.ObjectModelTool;


internal class GeneratorConfig
{
    public const string DefaultObjectModelNameFormatString = "{0}Wrapper";
    public const string DefaultConstructionMethodNamePattern = ".*(Create|Finish|Acquire).*";
    public const string DefaultDisposalMethodNamePattern = ".*(Release|Drop|Destroy).*";
    public const string DefaultHandleTypeNameExclusionPattern = "(Pfn).*|.*Descriptor";


    public string? ProjectFilePath { get; set; }
    public string? ApiTypeName { get; set; }
    public string? ApiOwnerTypeName { get; set; }
    public string ConstructionMethodNamePattern { get; set; } = DefaultConstructionMethodNamePattern;
    public string HandleTypeNameExclusionPattern { get; set; } = DefaultHandleTypeNameExclusionPattern;
    public string DisposalMethodNamePattern { get; set; } = DefaultDisposalMethodNamePattern;
    public string ObjectModelNameFormatString { get; set; } = DefaultObjectModelNameFormatString;
    public string? OutputPath { get; set; }
    public List<string> TransformGenerators { get; set; } = [];
}
