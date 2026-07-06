using System.Collections.Immutable;
using BottleSharp.Compiler.Binding;
using BottleSharp.Compiler.Diagnostics;

namespace BottleSharp.Compiler;

public sealed record BoundCompilationResult(
    ImmutableArray<Diagnostic> Diagnostics,
    BoundBlockStatement? BoundProgram)
{
    public bool HasErrors => Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
}
