using BottleSharp.Cli;

namespace BottleSharp.Cli.Tests;

public sealed class CliOptionsTests
{
    [Fact]
    public void Parse_DefaultsToHelpWhenNoArgs()
    {
        var options = CliOptions.Parse([]);

        Assert.Equal(CliCommand.Help, options.Command);
        Assert.True(options.HasValidArity);
        Assert.Null(options.FilePath);
        Assert.Null(options.OutputPath);
    }

    [Fact]
    public void Parse_RecognizesCheckCommand()
    {
        var options = CliOptions.Parse(["check", "hello.bsharp"]);

        Assert.Equal(CliCommand.Check, options.Command);
        Assert.Equal("hello.bsharp", options.FilePath);
        Assert.Null(options.OutputPath);
        Assert.True(options.HasValidArity);
    }

    [Fact]
    public void Parse_RecognizesRunCommand()
    {
        var options = CliOptions.Parse(["run", "hello.bsharp"]);

        Assert.Equal(CliCommand.Run, options.Command);
        Assert.Equal("hello.bsharp", options.FilePath);
        Assert.Null(options.OutputPath);
        Assert.True(options.HasValidArity);
    }

    [Fact]
    public void Parse_ReportsInvalidArityWhenFileIsMissing()
    {
        var options = CliOptions.Parse(["check"]);

        Assert.Equal(CliCommand.Check, options.Command);
        Assert.False(options.HasValidArity);
        Assert.Null(options.FilePath);
        Assert.Null(options.OutputPath);
    }

    [Fact]
    public void Parse_RecognizesBuildCommandWithOutput()
    {
        var options = CliOptions.Parse(["build", "hello.bsharp", "-o", "out.dll"]);

        Assert.Equal(CliCommand.Build, options.Command);
        Assert.Equal("hello.bsharp", options.FilePath);
        Assert.Equal("out.dll", options.OutputPath);
        Assert.True(options.HasValidArity);
    }

    [Fact]
    public void Parse_BuildCommandIsInvalidWithoutOutputFlag()
    {
        var options = CliOptions.Parse(["build", "hello.bsharp", "out.dll"]);

        Assert.Equal(CliCommand.Build, options.Command);
        Assert.False(options.HasValidArity);
        Assert.Null(options.FilePath);
        Assert.Null(options.OutputPath);
    }

    [Fact]
    public void Parse_RecognizesHelpAliases()
    {
        Assert.Equal(CliCommand.Help, CliOptions.Parse(["help"]).Command);
        Assert.Equal(CliCommand.Help, CliOptions.Parse(["--help"]).Command);
        Assert.Equal(CliCommand.Help, CliOptions.Parse(["-h"]).Command);
    }

    [Fact]
    public void Parse_UnknownCommandIsInvalid()
    {
        var options = CliOptions.Parse(["banana"]);

        Assert.Equal(CliCommand.Unknown, options.Command);
        Assert.False(options.HasValidArity);
    }
}
