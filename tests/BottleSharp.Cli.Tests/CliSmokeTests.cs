using BottleSharp.Cli;
using System.Reflection;

namespace BottleSharp.Cli.Tests;

public sealed class CliSmokeTests
{
    [Fact]
    public void HelpCommand_ReturnsZeroAndPrintsUsage()
    {
        using var output = new StringWriter();
        using var error = new StringWriter();

        var code = CliApplication.Run(["help"], output, error);

        Assert.Equal(0, code);
        Assert.Contains("BottleSharp CLI", output.ToString());
        Assert.Equal(string.Empty, error.ToString());
    }

    [Fact]
    public void UnknownCommand_ReturnsError()
    {
        using var output = new StringWriter();
        using var error = new StringWriter();

        var code = CliApplication.Run(["wat"], output, error);

        Assert.Equal(1, code);
        Assert.Contains("BSHARP904", error.ToString());
    }

    [Fact]
    public void CheckCommand_SucceedsForValidProgram()
    {
        var filePath = CreateTempProgram("print(\"ok\");");

        try
        {
            using var output = new StringWriter();
            using var error = new StringWriter();

            var code = CliApplication.Run(["check", filePath], output, error);

            Assert.Equal(0, code);
            Assert.Contains("Check succeeded.", output.ToString());
            Assert.Equal(string.Empty, error.ToString());
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void CheckCommand_ReportsSemanticError()
    {
        var filePath = CreateTempProgram("let x = 1; x = \"oops\";");

        try
        {
            using var output = new StringWriter();
            using var error = new StringWriter();

            var code = CliApplication.Run(["check", filePath], output, error);

            Assert.Equal(1, code);
            Assert.Contains("BSHARP203", error.ToString());
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void RunCommand_ExecutesProgramAndPrintsOutput()
    {
        var filePath = CreateTempProgram("let x = 10; let y = 20; print(x + y);");

        try
        {
            using var output = new StringWriter();
            using var error = new StringWriter();

            var code = CliApplication.Run(["run", filePath], output, error);

            Assert.Equal(0, code);
            Assert.Contains("30", output.ToString());
            Assert.Equal(string.Empty, error.ToString());
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void BuildCommand_CreatesPersistedAssemblyArtifacts()
    {
        var sourcePath = CreateTempProgram("print(\"built\");");
        var outputDll = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.dll");
        var outputRuntimeConfig = Path.ChangeExtension(outputDll, ".runtimeconfig.json");

        try
        {
            using var output = new StringWriter();
            using var error = new StringWriter();

            var code = CliApplication.Run(["build", sourcePath, "-o", outputDll], output, error);

            Assert.Equal(0, code);
            Assert.True(File.Exists(outputDll));
            Assert.True(new FileInfo(outputDll).Length > 0);
            Assert.True(File.Exists(outputRuntimeConfig));
            Assert.Contains("Build succeeded", output.ToString());
            Assert.Equal(string.Empty, error.ToString());

            var assembly = Assembly.Load(File.ReadAllBytes(outputDll));
            var mainMethod = assembly.GetType("Program")?.GetMethod("Main", BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(mainMethod);

            var originalOut = Console.Out;
            using var programOutput = new StringWriter();
            try
            {
                Console.SetOut(programOutput);
                mainMethod!.Invoke(null, null);
            }
            finally
            {
                Console.SetOut(originalOut);
            }

            Assert.Contains("built", programOutput.ToString());
        }
        finally
        {
            File.Delete(sourcePath);
            if (File.Exists(outputDll))
            {
                File.Delete(outputDll);
            }

            if (File.Exists(outputRuntimeConfig))
            {
                File.Delete(outputRuntimeConfig);
            }
        }
    }

    private static string CreateTempProgram(string code)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.bsharp");
        File.WriteAllText(path, code);
        return path;
    }
}
