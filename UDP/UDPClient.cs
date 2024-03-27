using System.Text.RegularExpressions;

public class UDPClient {
    ComState State { get; set; } = ComState.Start;

    Args Arg { get; set; }
    UDP Com { get; set; }
    string? Name { get; set; }
    InputReader Reader { get; set; }

    public UDPClient(Args arg) {
        Arg = arg;
        Com = new UDP(Arg.Host, Arg.Port);
        Reader = new();

        Console.CancelKeyPress += delegate {
            Com.Bye();
            Com.Close();
        };
    }

    public void Start() {
        Reader.ResetPrint();
        while (State != ComState.End) {
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
        try {
            if (text.StartsWith('/'))
                ParseCmd(text);

            Msg(text);
        } catch (ArgumentException e) {
            Reader.PrintErr($"ERR: {e.Message}");
        } catch (InvalidOperationException e) {
            Reader.PrintErr($"ERR: {e.Message}");
        }
    }

    /// <summary>
    /// Parses command and executes it
    /// </summary>
    /// <param name="text">User input to be parsed</param>
    private void ParseCmd(string text) {
        string[] parts = text.Split();
        switch (parts[0]) {
            case "/auth":
                Auth(parts[1..]);
                break;
            case "/join":
                Join(parts[1..]);
                break;
            case "/rename":
                Rename(parts[1..]);
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


    private void Auth(ReadOnlySpan<string> args) {
        if (args.Length != 3)
            throw new ArgumentException("Auth: invalid number of arguments");

        if (State != ComState.Start && State != ComState.Auth) {
            throw new InvalidOperationException(
                "Cannot be authorize in this state"
            );
        }
        State = ComState.Auth;

        Validator.Username(args[0]);
        Validator.Secret(args[1]);
        Validator.DisplayName(args[2]);

        Com.Auth(args[0], args[1], args[2]);
        Name = args[2];
    }

    private void Join(ReadOnlySpan<string> args) {
        if (args.Length != 1)
            throw new ArgumentException("Join: invalid number of arguments");

        if (State != ComState.Open) {
            throw new InvalidOperationException(
                "Cannot join channel in this state"
            );
        }

        Validator.ChannelID(args[0]);
        Com.Join(Name!, args[0]);
    }

    private void Rename(ReadOnlySpan<string> args) {
        if (args.Length != 1)
            throw new ArgumentException("Rename: invalid number of arguments");

        Validator.DisplayName(args[0]);
        Name = args[0];
    }

    private void Msg(string text) {
        if (State != ComState.Open) {
            throw new InvalidOperationException(
                "Cannot send messages in this state"
            );
        }

        Validator.DisplayName(Name!);
        Validator.MessageContent(text);
        Com.Msg(Name!, text);
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
}
