using System.Collections.Immutable;
using BottleSharp.Compiler.Diagnostics;

namespace BottleSharp.Compiler.Binding;

public sealed record BindingResult(BoundBlockStatement Statement, ImmutableArray<Diagnostic> Diagnostics);
