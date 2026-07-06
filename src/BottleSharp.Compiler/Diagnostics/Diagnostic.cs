namespace BottleSharp.Compiler.Diagnostics;

public sealed record Diagnostic(
    string Code,
    string Message,
    DiagnosticSeverity Severity,
    TextLocation Location);
