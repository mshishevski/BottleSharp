using BottleSharp.Compiler.Binding;
using BottleSharp.Compiler.Diagnostics;
using BottleSharp.Compiler.Lexing;
using BottleSharp.Compiler.Parsing;
using BottleSharp.Compiler.Text;

namespace BottleSharp.Compiler;

public static class BottleSharpCompiler
{
    public static BoundCompilationResult Compile(SourceText sourceText)
    {
        var diagnostics = new DiagnosticBag();
        BoundBlockStatement? boundProgram = null;

        if (string.IsNullOrWhiteSpace(sourceText.ToString()))
        {
            diagnostics.Report(
                DiagnosticCodes.EmptySource,
                "Source file is empty.",
                sourceText,
                new TextSpan(0, 0));
        }

        var lexResult = new Lexer(sourceText).Lex();
        diagnostics.AddRange(lexResult.Diagnostics);

        var parseResult = new Parser(sourceText, lexResult.Tokens).ParseCompilationUnit();
        diagnostics.AddRange(parseResult.Diagnostics);

        if (!diagnostics.HasErrors)
        {
            var bindResult = new Binder(sourceText).Bind(parseResult.Root);
            diagnostics.AddRange(bindResult.Diagnostics);

            if (!diagnostics.HasErrors)
            {
                boundProgram = bindResult.Statement;
            }
        }

        return new BoundCompilationResult(diagnostics.ToImmutableArray(), boundProgram);
    }

    public static CompilationResult Check(SourceText sourceText)
    {
        var compileResult = Compile(sourceText);
        return new CompilationResult(compileResult.Diagnostics);
    }
}
