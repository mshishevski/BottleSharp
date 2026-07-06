using System.Collections.Immutable;
using BottleSharp.Compiler.Diagnostics;
using BottleSharp.Compiler.Syntax;

namespace BottleSharp.Compiler.Lexing;

public sealed record LexResult(ImmutableArray<SyntaxToken> Tokens, ImmutableArray<Diagnostic> Diagnostics);
