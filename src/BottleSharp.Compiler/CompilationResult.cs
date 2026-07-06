using System.Collections.Immutable;
using BottleSharp.Compiler.Diagnostics;

namespace BottleSharp.Compiler;

public sealed record CompilationResult(ImmutableArray<Diagnostic> Diagnostics)
{
    public bool HasErrors => Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
}
