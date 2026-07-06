using BottleSharp.Compiler.Lexing;
using BottleSharp.Compiler.Syntax;
using BottleSharp.Compiler.Text;

namespace BottleSharp.Compiler.Tests;

public sealed class LexerTests
{
    [Fact]
    public void Lex_TokenizesSimpleProgram()
    {
        var source = new SourceText("let x = 10; print(x);");
        var lexer = new Lexer(source);

        var result = lexer.Lex();

        var kinds = result.Tokens.Select(t => t.Kind).ToArray();

        Assert.Equal(
        [
            SyntaxKind.LetKeyword,
            SyntaxKind.IdentifierToken,
            SyntaxKind.EqualsToken,
            SyntaxKind.IntLiteralToken,
            SyntaxKind.SemicolonToken,
            SyntaxKind.PrintKeyword,
            SyntaxKind.OpenParenToken,
            SyntaxKind.IdentifierToken,
            SyntaxKind.CloseParenToken,
            SyntaxKind.SemicolonToken,
            SyntaxKind.EndOfFileToken,
        ],
        kinds);

        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void Lex_ReportsUnterminatedString()
    {
        var source = new SourceText("print(\"hello");
        var lexer = new Lexer(source);

        var result = lexer.Lex();

        Assert.Contains(result.Diagnostics, d => d.Code == "BSHARP002");
    }

    [Fact]
    public void Lex_ReportsUnexpectedSingleAmpersand()
    {
        var source = new SourceText("let ok = true & false;");
        var lexer = new Lexer(source);

        var result = lexer.Lex();

        Assert.Contains(result.Diagnostics, d => d.Code == "BSHARP001");
    }
}
