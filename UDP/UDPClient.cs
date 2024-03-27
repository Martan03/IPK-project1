using System.Text.RegularExpressions;

public class UDPClient {
    Args Arg { get; set; }
    UDP Com { get; set; }
    string? Name { get; set; }
    InputReader Reader { get; set; }

    public UDPClient(Args arg) {
        Arg = arg;
        Com = Arg.Type switch {
            ComType.UDP => new UDP(Arg.Host, Arg.Port),
            _ => throw new NotImplementedException("TCP is not implemented"),
        };
        Reader = new();

        Console.CancelKeyPress += delegate {
            Com.Bye();
            Com.Close();
        };
    }

    public void Start() {
        Reader.ResetPrint();
        while (Com.State != ComState.End) {
            var read = Reader.Read();
            if (read.Length != 0)
                ParseInput(read);

            var recv = Com.Recv();
            if (recv.Length != 0)
                Console.WriteLine(recv);
        }

        Com.Close();
    }

    /// <summary>
    /// Parses the user input
    /// </summary>
    /// <param name="text">Text entered by user</param>
    private void ParseInput(string text) {
        if (text.StartsWith('/'))
            ParseCmd(text);

        else if (!Com.Msg(Name!, text))
            Reader.PrintErr("ERR: Cannot send messages in this state");
    }

    /// <summary>
    /// Parses command and executes it
    /// </summary>
    /// <param name="text">User input to be parsed</param>
    private void ParseCmd(string text) {
        string[] parts = text.Split();
        switch (parts[0]) {
            case "/auth":
                if (parts.Length != 4 || !CheckName(parts[1], 20) ||
                    !CheckName(parts[2], 128) || !CheckNick(parts[3], 20)) {
                    Reader.PrintErr(
                        "ERR: Invalid usage. Type /help to show help"
                    );
                    return;
                }

                if (!Com.Auth(parts[1], parts[2], parts[3])) {
                    Reader.PrintErr("Cannot use Auth in this state");
                    return;
                }
                Name = parts[3];
                break;
            case "/join":
                if (parts.Length != 2 || !CheckChannel(parts[1])) {
                    Reader.PrintErr(
                        "ERR: Invalid usage. Type /help to show help"
                    );
                    return;
                }

                if (!Com.Join(Name!, parts[1])) {
                    Reader.PrintErr("ERR: Cannot use Join in this state");
                    return;
                }
                break;
            case "/rename":
                if (parts.Length != 2 || !CheckNick(parts[1], 20)) {
                    Reader.PrintErr(
                        "ERR: Invalid usage. Type /help to show help"
                    );
                    return;
                }

                Name = parts[1];
                break;
            case "/help":
                Help();
                break;
            default:
                Reader.PrintErr($"ERR: Unknown command: {parts[0]}");
                break;
        }
    }

    private void ParseRecv(string res) {
        if (res.StartsWith("ERR")) {
            string pattern =
                @"^ERR FROM ([a-zA-Z0-9\-]+) IS ([\x20-\x7E]+)\r\n";
            var match = Regex.Match(res, pattern);

            if (!match.Success)
                return;

            Reader.PrintErr(
                $"ERR FROM {match.Groups[1].Value}: {match.Groups[2].Value}"
            );
            NextState(Response.Err);
        } else if (res.StartsWith("REPLY OK")) {
            string pattern = @"^REPLY OK IS ([\x20-\x7E]+)\r\n";
            var match = Regex.Match(res, pattern);

            if (!match.Success)
                return;

            Reader.PrintErr($"Success: {match.Groups[1].Value}");
            NextState(Response.ReplyOk);
        } else if (res.StartsWith("REPLY NOK")) {
            string pattern = @"^REPLY NOK IS ([\x20-\x7E]+)\r\n";
            var match = Regex.Match(res, pattern);

            if (!match.Success)
                return;

            Reader.PrintErr($"Failure: {match.Groups[1].Value}");
            NextState(Response.ReplyNok);
        } else if (res.StartsWith("MSG")) {
            string pattern =
                @"MSG FROM ([a-zA-Z0-9\-]+) IS ([\x20-\x7E]+)\r\n";
            var match = Regex.Match(res, pattern);

            if (!match.Success)
                return;

            Reader.Print($"{match.Groups[1].Value}: {match.Groups[2].Value}");
            NextState(Response.Msg);
        } else if (res.Equals("BYE\r\n")) {
            Com.State = ComState.End;
        }
    }

    /// <summary>
    /// Sets next state if possible
    /// </summary>
    /// <param name="res">Current server response</param>
    private void NextState(Response res) {
        switch (Com.State) {
            case ComState.Auth:
                if (res == Response.ReplyOk)
                    Com.State = ComState.Open;
                else if (res == Response.Err)
                    Com.Bye();
                break;
            case ComState.Open:
                // Add error send
                if (res == Response.None)
                    Com.Bye();
                else if (res == Response.Err)
                    Com.Bye();
                else if (res == Response.Bye)
                    Com.State = ComState.End;
                break;
        }
    }

    /// <summary>
    /// Displays help
    /// </summary>
    private void Help() {
        Reader.Print(
            "Help for IPK Project 1\n" +
            "Commands:\n" +
            "/auth {Username} {Secret} {DisplayName}\n" +
            "  Tries to authorize user\n" +
            "/join {ChannelID}\n" +
            "  Joins channel with given Channel ID\n" +
            "/rename {DisplayName}\n" +
            "  Changes DisplayName to sent new messages with\n" +
            "/help\n" +
            "  Displays this help"
        );
    }

    private bool CheckName(string name, int max) {
        string pattern = @"^[a-zA-Z0-9\-]+$";
        return name.Length <= max && Regex.IsMatch(name, pattern);
    }

    private bool CheckChannel(string name) {
        name = name.Replace(".", "");
        return CheckName(name, 20);
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