using BottleSharp.Compiler.Binding;
using BottleSharp.Compiler.Lexing;
using BottleSharp.Compiler.Parsing;
using BottleSharp.Compiler.Text;

namespace BottleSharp.Compiler.Tests;

public sealed class BinderTests
{
    [Fact]
    public void Bind_ReportsUndefinedVariable()
    {
        var diagnostics = BindDiagnostics("print(x);");

        Assert.Contains(diagnostics, d => d.Code == "BSHARP201");
    }

    [Fact]
    public void Bind_ReportsDuplicateVariableInSameScope()
    {
        var diagnostics = BindDiagnostics("let x = 1; let x = 2;");

        Assert.Contains(diagnostics, d => d.Code == "BSHARP202");
    }

    [Fact]
    public void Bind_ReportsInvalidAssignmentType()
    {
        var diagnostics = BindDiagnostics("let x = 1; x = \"hello\";");

        Assert.Contains(diagnostics, d => d.Code == "BSHARP203");
    }

    [Fact]
    public void Bind_ReportsInvalidBinaryOperator()
    {
        var diagnostics = BindDiagnostics("let x = \"hello\" + 5;");

        Assert.Contains(diagnostics, d => d.Code == "BSHARP205");
    }

    [Fact]
    public void Bind_ReportsConditionMustBeBool_ForIfAndWhile()
    {
        var diagnostics = BindDiagnostics("if (1) { print(1); } while (\"x\") { print(1); }");

        Assert.Equal(2, diagnostics.Count(d => d.Code == "BSHARP206"));
    }

    [Fact]
    public void Bind_AllowsShadowingAcrossNestedScopes()
    {
        var diagnostics = BindDiagnostics("let x = 1; { let x = 2; print(x); }");

        Assert.DoesNotContain(diagnostics, d => d.Code == "BSHARP202");
    }

    private static IReadOnlyList<BottleSharp.Compiler.Diagnostics.Diagnostic> BindDiagnostics(string text)
    {
        var source = new SourceText(text);
        var lex = new Lexer(source).Lex();
        Assert.Empty(lex.Diagnostics);

        var parse = new Parser(source, lex.Tokens).ParseCompilationUnit();
        Assert.Empty(parse.Diagnostics);

        var bind = new Binder(source).Bind(parse.Root);
        return bind.Diagnostics;
    }
}
