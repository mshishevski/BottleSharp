using System.Text;

namespace BottleSharp.Compiler.Diagnostics;

public static class DiagnosticRenderer
{
    public static string Render(Diagnostic diagnostic)
    {
        var source = diagnostic.Location.Source;
        var lineIndex = diagnostic.Location.StartLine;
        var line = source.Lines[lineIndex];

        var lineText = source.ToString(BottleSharp.Compiler.Text.TextSpan.FromBounds(line.Start, line.End));
        var column = diagnostic.Location.StartColumn;
        var markerLength = Math.Max(1, diagnostic.Location.Span.Length);

        var builder = new StringBuilder();
        var fileName = string.IsNullOrWhiteSpace(source.FilePath) ? "<source>" : source.FilePath;

        builder.Append(fileName);
        builder.Append('(');
        builder.Append(lineIndex + 1);
        builder.Append(',');
        builder.Append(column + 1);
        builder.Append("): ");
        builder.Append(diagnostic.Severity.ToString().ToLowerInvariant());
        builder.Append(' ');
        builder.Append(diagnostic.Code);
        builder.Append(": ");
        builder.AppendLine(diagnostic.Message);
        builder.AppendLine();
        builder.AppendLine(lineText);
        builder.AppendLine(new string(' ', column) + new string('^', markerLength));

        return builder.ToString();
    }

    public static string Render(IEnumerable<Diagnostic> diagnostics)
    {
        return string.Join(Environment.NewLine, diagnostics.Select(Render));
    }
}
