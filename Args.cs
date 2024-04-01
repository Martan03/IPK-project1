
/// <summary>
/// Type of communication
/// </summary>
public enum ComType {
    TCP,
    UDP
}

public class Args {
    public ComType Type { get; private set; }
    public string Host { get; private set; } = "";
    public ushort Port { get; private set; } = 4567;
    public ushort Timeout { get; private set; } = 250;
    public byte Retransmit { get; private set; } = 3;
    public bool Help { get; private set; } = false;

    /// <summary>
    /// Parses given arguments
    /// </summary>
    /// <param name="args">Span of arguments</param>
    /// <exception cref="ArgumentException">Invalid usage</exception>
    public Args(ReadOnlySpan<string> args) {
        bool type = false;
        bool host = false;
        int len = args.Length;
        while (!args.IsEmpty) {
            switch (args[0]) {
                case "-t":
                    args = GetNext(args);
                    Type = args[0] switch {
                        "tcp" => ComType.TCP,
                        "udp" => ComType.UDP,
                        _ => throw new ArgumentException(
                            "Invalid type given: " + args[0]
                        ),
                    };
                    type = true;
                    break;
                case "-s":
                    args = GetNext(args);
                    Host = args[0];
                    host = true;
                    break;
                case "-p":
                    args = GetNext(args);
                    Port = ParseArg<ushort>(args[0]);
                    break;
                case "-d":
                    args = GetNext(args);
                    Timeout = ParseArg<ushort>(args[0]);
                    break;
                case "-r":
                    args = GetNext(args);
                    Retransmit = ParseArg<byte>(args[0]);
                    break;
                case "-h":
                    if (len > 1)
                        throw new ArgumentException(
                            "Help cannot be combined with other flags"
                        );
                    ShowHelp();
                    Help = true;
                    return;
                default:
                    throw new ArgumentException(
                        "Invalid argument: " + args[0]
                    );
            }
            args = args[1..];
        }

        if (!type)
            throw new ArgumentException("Type must be specified");
        if (!host)
            throw new ArgumentException("Host must be specified");
    }

    /// <summary>
    /// Move span to next argument
    /// </summary>
    /// <param name="args">Span with arguments</param>
    /// <returns>Moved span to next argument</returns>
    /// <exception cref="Exception">When no next argument</exception>
    private ReadOnlySpan<string> GetNext(ReadOnlySpan<string> args) {
        string flag = args[0];
        args = args[1..];
        if (args.IsEmpty)
            throw new ArgumentException(
                $"Flag '{flag}' expects value after it"
            );

        return args;
    }

    /// <summary>
    /// Parses given argument
    /// </summary>
    /// <typeparam name="T">Type of the argument to parse to</typeparam>
    /// <param name="arg">Argument to be parsed</param>
    /// <returns>Parsed argument</returns>
    /// <exception cref="ArgumentException">Error parsing</exception>
    private T ParseArg<T>(string arg) where T: IParsable<T> {
        if (T.TryParse(arg, null, out T? value)) {
            return value;
        }
        throw new ArgumentException("Invalid argument type");
    }

    /// <summary>
    /// Prints Help
    /// </summary>
    private void ShowHelp() {
        Console.WriteLine(
            "Help for IPK Project 1\n\n" +
            "Mandatory arguments:\n" +
            "  -t {tcp|udp}\n    Transport protocol used for connection\n" +
            "  -s <IP/hostname>\n    Server IP or hostname\n\n" +
            "Optional arguments:\n" +
            "  -p <Port>\n    Server port\n" +
            "  -d <Timeout>\n    UDP confirmation timeout\n" +
            "  -r <Retransmits>\n    Maximum number of UDP retransmissions\n" +
            "  -h\n    Prints this help (cannot be combined with other args)\n"
        );
    }
}
