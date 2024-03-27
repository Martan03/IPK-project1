using System.Text;
using System.Text.RegularExpressions;

public class UDPClient {
    ComState State { get; set; } = ComState.Start;

    Args Arg { get; set; }
    UDP Com { get; set; }
    string? Name { get; set; }
    InputReader Reader { get; set; }

    public UDPClient(Args arg) {
        Arg = arg;
        Com = new UDP(Arg);
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

            Com.Resend();
            var recv = Com.Recv();
            if (recv.Length != 0)
                ParseRecv(recv);
        }

        Com.Close();
    }

    /// <summary>
    /// Parses the user input
    /// </summary>
    /// <param name="text">Text entered by user</param>
    private void ParseInput(string text) {
        try {
            if (text.StartsWith('/')) {
                ParseCmd(text);
                return;
            }

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

    private void ParseRecv(byte[] res) {
        var recv = Response.None;
        switch (res[0]) {
            case (byte)Type.CONFIRM:
                ParseConfirm(res);
                return;
            case (byte)Type.REPLY:
                Success(res);
                recv = ParseReply(res);
                break;
            case (byte)Type.MSG:
                Success(res);
                ParseErrMsg(res);
                recv = Response.Msg;
                break;
            case (byte)Type.ERR:
                Success(res);
                ParseErrMsg(res, "ERR FROM ");
                recv = Response.Err;
                break;
            case (byte)Type.BYE:
                Success(res);
                recv = Response.Bye;
                break;
        }

        NextState(recv);
    }

    private void Success(byte[] res) {
        var msgId = BitConverter.ToUInt16(res, 1);
        Com.Confirm(msgId);
    }

    private void ParseConfirm(byte[] res) {
        var msgId = BitConverter.ToUInt16(res, 1);
        //Com.ConfirmMsg(msgId);
    }

    private Response ParseReply(byte[] res) {
        var result = "Failure";
        var recv = Response.ReplyNok;
        if (res[3] == 1) {
            result = "Success";
            recv = Response.ReplyOk;
        }

        ReadOnlySpan<byte> msgBytes = BytesTillNull(res[6..]);
        var msg = Encoding.UTF8.GetString(msgBytes);
        Reader.PrintErr($"{result}: {msg}");
        return recv;
    }

    private void ParseErrMsg(byte[] res, string pre = "") {
        ReadOnlySpan<byte> nameBytes = BytesTillNull(res[3..]);
        var name = Encoding.UTF8.GetString(nameBytes);

        var offset = 3 + nameBytes.Length + 1;
        ReadOnlySpan<byte> msgBytes = BytesTillNull(res[offset..]);
        var msg = Encoding.UTF8.GetString(msgBytes);
        Reader.PrintErr($"{pre}{name}: {msg}");
    }

    /// <summary>
    /// Sets next state if possible
    /// </summary>
    /// <param name="res">Current server response</param>
    private void NextState(Response res) {
        switch (State) {
            case ComState.Auth:
                if (res == Response.ReplyOk)
                    State = ComState.Open;
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
                    State = ComState.End;
                break;
        }
    }

    private ReadOnlySpan<byte> BytesTillNull(ReadOnlySpan<byte> bytes) {
        int index = bytes.IndexOf((byte)0);

        if (index == -1)
            return bytes;

        return bytes.Slice(0, index);
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
