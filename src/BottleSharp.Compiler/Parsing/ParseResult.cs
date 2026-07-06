using System.Collections.Immutable;
using BottleSharp.Compiler.Diagnostics;
using BottleSharp.Compiler.Syntax;

namespace BottleSharp.Compiler.Parsing;

public sealed record ParseResult(CompilationUnitSyntax Root, ImmutableArray<Diagnostic> Diagnostics);
