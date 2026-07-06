namespace BottleSharp.Cli;

public sealed record CliOptions(CliCommand Command, string? FilePath, string? OutputPath, bool HasValidArity)
{
    public static CliOptions Parse(string[] args)
    {
        if (args.Length == 0)
        {
            return new CliOptions(CliCommand.Help, null, null, true);
        }

        var first = args[0].ToLowerInvariant();
        if (first is "help" or "--help" or "-h")
        {
            return new CliOptions(CliCommand.Help, null, null, true);
        }

        var command = first switch
        {
            "check" => CliCommand.Check,
            "run" => CliCommand.Run,
            "build" => CliCommand.Build,
            _ => CliCommand.Unknown,
        };

        if (command == CliCommand.Unknown)
        {
            return new CliOptions(CliCommand.Unknown, null, null, false);
        }

        if (command is CliCommand.Check or CliCommand.Run)
        {
            if (args.Length != 2)
            {
                return new CliOptions(command, null, null, false);
            }

            return new CliOptions(command, args[1], null, true);
        }

        if (command == CliCommand.Build)
        {
            if (args.Length != 4)
            {
                return new CliOptions(command, null, null, false);
            }

            var outputFlag = args[2].ToLowerInvariant();
            if (outputFlag is not ("-o" or "--output"))
            {
                return new CliOptions(command, null, null, false);
            }

            return new CliOptions(command, args[1], args[3], true);
        }

        return new CliOptions(CliCommand.Unknown, null, null, false);
    }
}
