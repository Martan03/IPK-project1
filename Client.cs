using System.Text.RegularExpressions;

public class Client {
    Args Arg { get; set; }
    IComm Com { get; set; }
    string? Name { get; set; }

    /// <summary>
    /// Creates new Client
    /// </summary>
    /// <param name="arg">Parsed arguments</param>
    public Client(Args arg) {
        Arg = arg;
        Com = Arg.Type switch {
            ComType.TCP => new TCP(Arg.Host, Arg.Port),
            _ => throw new NotImplementedException("UDP is not implemented"),
        };

        Console.CancelKeyPress += delegate {
            Com.Bye();
            Com.Close();
        };
    }

    /// <summary>
    /// Starts the Client loop
    /// </summary>
    public void Start() {
        InputReader reader = new();
        while (Com.State != ComState.End) {
            var read = reader.Read();
            if (read.Length != 0)
                ParseInput(read);

            var recv = Com.Recv();
            if (recv.Length != 0)
                Console.Write(recv);
        }

        Com.Close();
    }

    private void ParseInput(string text) {
        if (text.StartsWith('/'))
            ParseResponse(ParseCmd(text));
    }

    private string ParseCmd(string text) {
        string[] parts = text.Split();
        switch (parts[0]) {
            case "/auth":
                if (parts.Length != 4) {
                    Console.Error.WriteLine("Invalid number of arguments");
                    // Throw custom exception here
                    return "";
                }
                var res = Com.Auth(parts[1], parts[2], parts[3]);
                if (res.StartsWith("REPLY OK"))
                    Name = parts[1];
                return res;
            case "/join":
                if (parts.Length != 2) {
                    Console.Error.WriteLine("Invalid number of arguments");
                    return "";
                }

                return Com.Join(Name!, parts[1]);
            case "/rename":
                if (parts.Length != 2) {
                    Console.Error.WriteLine("Invalid number of arguments");
                    return "";
                }

                Name = parts[1];
                break;
            case "/help":
                Console.WriteLine("TODO");
                break;
            default:
                Console.Error.WriteLine($"Invalid command: {parts[0]}");
                break;
        }
        return "";
    }

    private void ParseResponse(string res) {
        if (res.StartsWith("ERR")) {
            string pattern =
                @"^ERR FROM ([a-zA-Z0-9\-]+) IS ([\x20-\x7E]+)\r\n";
            var match = Regex.Match(res, pattern);

            if (!match.Success)
                return;

            Console.Error.WriteLine(
                $"ERR FROM {match.Groups[1].Value}: {match.Groups[2].Value}"
            );
        } else if (res.StartsWith("REPLY OK")) {
            string pattern = @"^REPLY OK IS ([\x20-\x7E]+)\r\n";
            var match = Regex.Match(res, pattern);

            if (!match.Success)
                return;

            Console.WriteLine($"Success: {match.Groups[1].Value}");
        } else if (res.StartsWith("REPLY NOK")) {
            string pattern = @"^REPLY NOK IS ([\x20-\x7E]+)\r\n";
            var match = Regex.Match(res, pattern);

            if (!match.Success)
                return;

            Console.WriteLine($"Failure: {match.Groups[1].Value}");
        } else if (res.StartsWith("MSG")) {
            string pattern =
                @"MSG FROM ([a-zA-Z0-9\-]+) IS ([\x20-\x7E]+)\r\n";
            var match = Regex.Match(res, pattern);

            if (!match.Success)
                return;

            Console.WriteLine(
                $"{match.Groups[1].Value}: {match.Groups[2].Value}"
            );
        } else if (res.Equals("BYE\r\n")) {
            Com.State = ComState.End;
        }
    }

    private bool CheckName(string name, int max) {
        string pattern = @"^[a-zA-Z0-9\-]+$";
        return name.Length <= max && Regex.IsMatch(name, pattern);
    }

    private bool CheckNick(string name, int max) {
        string pattern = @"^[\x21-\x7E]+$";
        return name.Length <= max && Regex.IsMatch(name, pattern);
    }

    private bool CheckContent(string content, int max) {
        string pattern = @"^[\x20-\x7E]+$";
        return content.Length <= max && Regex.IsMatch(content, pattern);
    }
}
