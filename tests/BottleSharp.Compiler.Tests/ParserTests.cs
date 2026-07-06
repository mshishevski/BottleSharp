using BottleSharp.Compiler.Lexing;
using BottleSharp.Compiler.Parsing;
using BottleSharp.Compiler.Syntax;
using BottleSharp.Compiler.Text;

namespace BottleSharp.Compiler.Tests;

public sealed class ParserTests
{
    [Fact]
    public void Parse_BinaryPrecedence_BindsMultiplicationBeforeAddition()
    {
        var root = Parse("let result = 1 + 2 * 3;");

        var declaration = Assert.IsType<VariableDeclarationSyntax>(root.Statements[0]);
        var expression = Assert.IsType<BinaryExpressionSyntax>(declaration.Initializer);

        Assert.Equal(SyntaxKind.PlusToken, expression.OperatorToken.Kind);
        Assert.IsType<LiteralExpressionSyntax>(expression.Left);

        var right = Assert.IsType<BinaryExpressionSyntax>(expression.Right);
        Assert.Equal(SyntaxKind.StarToken, right.OperatorToken.Kind);
    }

    [Fact]
    public void Parse_ParsesIfElseAndWhileStatements()
    {
        var text = """
            if (true) {
                print(1);
            } else {
                print(2);
            }

            while (false) {
                print(3);
            }
            """;

        var root = Parse(text);

        Assert.IsType<IfStatementSyntax>(root.Statements[0]);
        Assert.IsType<WhileStatementSyntax>(root.Statements[1]);
    }

    [Fact]
    public void Parse_ReportsExpectedExpression()
    {
        var source = new SourceText("print(x + );", "hello.bsharp");
        var lex = new Lexer(source).Lex();
        var parse = new Parser(source, lex.Tokens).ParseCompilationUnit();

        Assert.Contains(parse.Diagnostics, d => d.Code == "BSHARP102");
    }

    [Fact]
    public void Parse_ReportsMissingSemicolon()
    {
        var source = new SourceText("let x = 10", "missing_semicolon.bsharp");
        var lex = new Lexer(source).Lex();
        var parse = new Parser(source, lex.Tokens).ParseCompilationUnit();

        Assert.Contains(parse.Diagnostics, d => d.Code == "BSHARP101");
    }

    [Fact]
    public void Parse_ParsesAssignmentStatement()
    {
        var root = Parse("x = 42;");

        var assignment = Assert.IsType<AssignmentStatementSyntax>(root.Statements[0]);
        Assert.Equal("x", assignment.IdentifierToken.Text);
        Assert.Equal(SyntaxKind.IntLiteralToken, Assert.IsType<LiteralExpressionSyntax>(assignment.Expression).LiteralToken.Kind);
    }

    private static CompilationUnitSyntax Parse(string text)
    {
        var source = new SourceText(text);
        var lex = new Lexer(source).Lex();
        var parse = new Parser(source, lex.Tokens).ParseCompilationUnit();

        Assert.Empty(parse.Diagnostics);
        return parse.Root;
    }
}
