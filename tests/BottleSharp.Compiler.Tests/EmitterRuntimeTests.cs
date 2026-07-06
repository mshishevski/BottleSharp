using BottleSharp.Compiler.Emitting;
using BottleSharp.Compiler.Text;

namespace BottleSharp.Compiler.Tests;

public sealed class EmitterRuntimeTests
{
    [Fact]
    public void Execute_RunsArithmeticAndPrint()
    {
        var output = ExecuteAndCaptureOutput(
            """
            let x = 10;
            let y = 20;
            let result = x + y;
            print(result);
            """);

        Assert.Equal("30", output);
    }

    [Fact]
    public void Execute_RunsIfElse()
    {
        var output = ExecuteAndCaptureOutput(
            """
            let temperature = 32;
            if (temperature > 30) {
                print("hot");
            } else {
                print("normal");
            }
            """);

        Assert.Equal("hot", output);
    }

    [Fact]
    public void Execute_RunsWhileLoop()
    {
        var output = ExecuteAndCaptureOutput(
            """
            let i = 0;
            while (i < 3) {
                print(i);
                i = i + 1;
            }
            """);

        Assert.Equal("0\n1\n2", output);
    }

    [Fact]
    public void Execute_RunsBooleanOperations()
    {
        var output = ExecuteAndCaptureOutput(
            """
            print(true && false);
            print(true || false);
            print(!false);
            """);

        Assert.Equal("False\nTrue\nTrue", output);
    }

    private static string ExecuteAndCaptureOutput(string sourceText)
    {
        var source = new SourceText(sourceText, "runtime_test.bsharp");
        var compilation = BottleSharpCompiler.Compile(source);

        Assert.False(compilation.HasErrors);
        Assert.NotNull(compilation.BoundProgram);

        var originalOut = Console.Out;
        using var writer = new StringWriter();

        try
        {
            Console.SetOut(writer);
            RuntimeEmitter.Execute(compilation.BoundProgram!);
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        return writer.ToString().Replace("\r\n", "\n", StringComparison.Ordinal).TrimEnd('\n');
    }
}
