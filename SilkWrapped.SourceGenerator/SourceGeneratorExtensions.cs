using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SilkWrapped.SourceGenerator;
internal static class SourceGeneratorExtensions
{

    public static IncrementalValueProvider<T> GetMSBuildProperties<T>(
        this IncrementalGeneratorInitializationContext context,
        Func<AnalyzerConfigOptions, T> selector)
    {
        return context.AnalyzerConfigOptionsProvider.Select((o,ct) => selector(o.GlobalOptions));
        
    }

    public static string GetMSBuildProperty(
        this AnalyzerConfigOptions analyzerConfigOptions,
        string name,
        string defaultValue = "")
    {
        return analyzerConfigOptions.TryGetValue($"build_property.{name}", out var value) 
            && !string.IsNullOrEmpty(value) ? value : defaultValue;

    }
}
