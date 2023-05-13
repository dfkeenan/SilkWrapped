using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SilkWrapped.SourceGenerator.Debug;
internal class TestAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
{
    public TestAnalyzerConfigOptionsProvider()
    {
        GlobalOptions = new TestAnalyzerConfigOptions();
    }

    public override TestAnalyzerConfigOptions GlobalOptions { get; }

    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
    {
        throw new NotImplementedException();
    }

    public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
    {
        throw new NotImplementedException();
    }
}

internal class TestAnalyzerConfigOptions : AnalyzerConfigOptions
{
    private readonly Dictionary<string, string> options;

    public TestAnalyzerConfigOptions()
    {
        this.options = new Dictionary<string, string>();
    }

    public string this[string key]
    {
        get { return options[key]; }
        set { options[$"build_property.{key}"] = value; }
    }

    public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
    {
        return options.TryGetValue(key, out value);
    }
}
