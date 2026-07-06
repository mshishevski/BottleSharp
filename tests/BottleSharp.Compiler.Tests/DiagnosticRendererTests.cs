using BottleSharp.Compiler.Diagnostics;
using BottleSharp.Compiler.Text;

namespace BottleSharp.Compiler.Tests;

public sealed class DiagnosticRendererTests
{
    [Fact]
    public void Render_ProducesCodeMessageAndCaret()
    {
        var source = new SourceText("print(x + );", "hello.bsharp");
        var location = new TextLocation(source, new TextSpan(10, 1));
        var diagnostic = new Diagnostic("BSHARP001", "Expected expression after '+'.", DiagnosticSeverity.Error, location);

        var rendered = DiagnosticRenderer.Render(diagnostic);

        Assert.Contains("hello.bsharp(1,11): error BSHARP001: Expected expression after '+'.", rendered);
        Assert.Contains("print(x + );", rendered);
        Assert.Contains("          ^", rendered);
    }
}
