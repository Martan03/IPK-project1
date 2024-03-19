
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

    /// <summary>
    /// Parses given arguments
    /// </summary>
    /// <param name="args">Span of arguments</param>
    /// <exception cref="ArgumentException">Invalid usage</exception>
    public Args(ReadOnlySpan<string> args) {
        bool type = false;
        bool host = false;
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
                    if (args.Length > 1)
                        throw new ArgumentException(
                            "Help cannot be combined with other flags"
                        );
                    Help();
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
            throw new Exception($"Flag '{flag}' expects value after it");

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
    private void Help() {
        Console.WriteLine("TODO");
    }
}
