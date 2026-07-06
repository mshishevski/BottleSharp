using BottleSharp.Compiler;
using BottleSharp.Compiler.Diagnostics;
using BottleSharp.Compiler.Emitting;
using BottleSharp.Compiler.Text;

namespace BottleSharp.Cli;

public static class CliApplication
{
    public static int Run(string[] args, TextWriter stdout, TextWriter stderr)
    {
        var options = CliOptions.Parse(args);

        if (options.Command == CliCommand.Help)
        {
            PrintUsage(stdout);
            return 0;
        }

        if (options.Command == CliCommand.Unknown)
        {
            stderr.WriteLine("error BSHARP904: Unknown command.");
            PrintUsage(stderr);
            return 1;
        }

        if (!options.HasValidArity || string.IsNullOrWhiteSpace(options.FilePath))
        {
            stderr.WriteLine("error BSHARP905: Invalid arguments for command.");
            PrintUsage(stderr);
            return 1;
        }

        if (options.Command == CliCommand.Build && string.IsNullOrWhiteSpace(options.OutputPath))
        {
            stderr.WriteLine("error BSHARP906: Build command requires an output file path via -o or --output.");
            PrintUsage(stderr);
            return 1;
        }

        if (!File.Exists(options.FilePath))
        {
            stderr.WriteLine($"error BSHARP901: File not found: {options.FilePath}");
            return 1;
        }

        if (!string.Equals(Path.GetExtension(options.FilePath), ".bsharp", StringComparison.OrdinalIgnoreCase))
        {
            stderr.WriteLine($"error BSHARP902: Expected a .bsharp file, got '{options.FilePath}'.");
            return 1;
        }

        var source = new SourceText(File.ReadAllText(options.FilePath), options.FilePath);
        var result = BottleSharpCompiler.Compile(source);

        if (result.Diagnostics.Length > 0)
        {
            stderr.WriteLine(DiagnosticRenderer.Render(result.Diagnostics));
        }

        if (result.HasErrors)
        {
            return 1;
        }

        if (options.Command == CliCommand.Check)
        {
            stdout.WriteLine("Check succeeded.");
            return 0;
        }

        if (options.Command == CliCommand.Build)
        {
            try
            {
                PersistedAssemblyEmitter.Save(
                    result.BoundProgram ?? throw new InvalidOperationException("Bound program is missing."),
                    options.OutputPath!);

                stdout.WriteLine($"Build succeeded: {Path.GetFullPath(options.OutputPath!)}");
                stdout.WriteLine("Note: the generated DLL currently lacks entry-point metadata for direct `dotnet <file.dll>` execution.");
                stdout.WriteLine("You can invoke Program.Main via reflection, or use `bottlesharp run` for direct execution.");
                return 0;
            }
            catch (Exception ex)
            {
                stderr.WriteLine($"error BSHARP907: Build failed: {ex.Message}");
                return 1;
            }
        }

        var originalOut = Console.Out;

        try
        {
            if (!ReferenceEquals(stdout, Console.Out))
            {
                Console.SetOut(stdout);
            }

            RuntimeEmitter.Execute(result.BoundProgram ?? throw new InvalidOperationException("Bound program is missing."));
            return 0;
        }
        catch (Exception ex)
        {
            stderr.WriteLine($"error BSHARP903: Runtime execution failed: {ex.Message}");
            return 1;
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    private static void PrintUsage(TextWriter writer)
    {
        writer.WriteLine("BottleSharp CLI");
        writer.WriteLine("Usage:");
        writer.WriteLine("  bottlesharp check <file.bsharp>");
        writer.WriteLine("  bottlesharp run <file.bsharp>");
        writer.WriteLine("  bottlesharp build <file.bsharp> -o <output.dll>");
        writer.WriteLine("  bottlesharp help");
    }
}
