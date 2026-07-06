using System.Collections.Immutable;
using BottleSharp.Compiler.Text;

namespace BottleSharp.Compiler.Diagnostics;

public sealed class DiagnosticBag
{
    private readonly List<Diagnostic> _diagnostics = [];

    public bool HasErrors => _diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);

    public void Report(
        string code,
        string message,
        TextLocation location,
        DiagnosticSeverity severity = DiagnosticSeverity.Error)
    {
        _diagnostics.Add(new Diagnostic(code, message, severity, location));
    }

    public void Report(
        string code,
        string message,
        SourceText source,
        TextSpan span,
        DiagnosticSeverity severity = DiagnosticSeverity.Error)
    {
        Report(code, message, new TextLocation(source, span), severity);
    }

    public void AddRange(IEnumerable<Diagnostic> diagnostics)
    {
        _diagnostics.AddRange(diagnostics);
    }

    public ImmutableArray<Diagnostic> ToImmutableArray() => _diagnostics.ToImmutableArray();
}
