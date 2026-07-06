using BottleSharp.Compiler.Text;

namespace BottleSharp.Compiler.Tests;

public sealed class CompilerCheckTests
{
    [Fact]
    public void Check_ReportsParseErrorsFromPipeline()
    {
        var source = new SourceText("print(x + );", "bad_parse.bsharp");

        var result = BottleSharpCompiler.Check(source);

        Assert.True(result.HasErrors);
        Assert.Contains(result.Diagnostics, d => d.Code == "BSHARP102");
    }

    [Fact]
    public void Check_ReportsSemanticErrorsFromPipeline()
    {
        var source = new SourceText("let x = 1; x = \"oops\";", "bad_semantic.bsharp");

        var result = BottleSharpCompiler.Check(source);

        Assert.True(result.HasErrors);
        Assert.Contains(result.Diagnostics, d => d.Code == "BSHARP203");
    }
}
