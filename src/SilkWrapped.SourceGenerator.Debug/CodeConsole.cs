using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.Text;

namespace SilkWrapped.SourceGenerator.Debug;

public class Range
{
    public ClassifiedSpan ClassifiedSpan { get; private set; }
    public string Text { get; private set; }

    public Range(string classification, TextSpan span, SourceText text) :
        this(classification, span, text.GetSubText(span).ToString())
    {
    }

    public Range(string classification, TextSpan span, string text) :
        this(new ClassifiedSpan(classification, span), text)
    {
    }

    public Range(ClassifiedSpan classifiedSpan, string text)
    {
        ClassifiedSpan = classifiedSpan;
        Text = text;
    }

    public string ClassificationType => ClassifiedSpan.ClassificationType;

    public TextSpan TextSpan => ClassifiedSpan.TextSpan;
}
internal static class CodeConsole
{
    public static async Task Write(string code)
    {
        AdhocWorkspace workspace = new();
        Solution solution = workspace.CurrentSolution;
        Project project = solution.AddProject("projectName", "assemblyName", LanguageNames.CSharp);
        Document document = project.AddDocument("name.cs", code);

        await Write(document);
    }

    public static async Task Write(Document document)
    {
        //document = await Formatter.FormatAsync(document);
        SourceText text = await document.GetTextAsync();

        IEnumerable<ClassifiedSpan> classifiedSpans = await Classifier.GetClassifiedSpansAsync(document, TextSpan.FromBounds(0, text.Length));
        Console.BackgroundColor = ConsoleColor.Black;

        IEnumerable<Range> ranges = classifiedSpans.Select(classifiedSpan =>
            new Range(classifiedSpan, text.GetSubText(classifiedSpan.TextSpan).ToString()));

        ranges = FillGaps(text, ranges);

        foreach (Range range in ranges)
        {
            switch (range.ClassificationType)
            {
                case "keyword":
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    break;
                case "class name":
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case "string":
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
            }

            Console.Write(range.Text);
        }

        Console.ResetColor();
        Console.WriteLine();
    }

    private static IEnumerable<Range> FillGaps(SourceText text, IEnumerable<Range> ranges)
    {
        const string WhitespaceClassification = "whitespace";
        int current = 0;
        Range? previous = null;

        foreach (Range range in ranges)
        {
            int start = range.TextSpan.Start;
            if (start > current)
            {
                yield return new Range(WhitespaceClassification, TextSpan.FromBounds(current, start), text);
            }

            if (previous == null || range.TextSpan != previous.TextSpan)
            {
                yield return range;
            }

            previous = range;
            current = range.TextSpan.End;
        }

        if (current < text.Length)
        {
            yield return new Range(WhitespaceClassification, TextSpan.FromBounds(current, text.Length), text);
        }
    }
}
