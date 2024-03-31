using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

public class TCP : IComm {
    private TcpClient Client { get; set; }
    private NetworkStream Stream { get; set; }

    /// <summary>
    /// Constructs new TCP communication
    /// </summary>
    /// <param name="args">parsed arguments</param>
    public TCP(string host, ushort port) {
        Client = new TcpClient(host, port);
        Stream = Client.GetStream();
    }

    public void Auth(string name, string secret, string nick) {
        Send($"AUTH {name} AS {nick} USING {secret}\r\n");
    }

    public void Join(string name, string channel) {
        Send($"JOIN {channel} AS {name}\r\n");
    }

    public void Msg(string from, string msg) {
        Send($"MSG FROM {from} IS {msg}\r\n");
    }

    public void Err(string from, string msg) {
        Send($"ERROR FROM {from} IS {msg}\r\n");
    }

    public void Bye() {
        Send("BYE\r\n");
    }

    public byte[] Recv() {
        List<byte> msg = new();
        while (Stream.DataAvailable) {
            int val = Stream.ReadByte();
            if (val == -1)
                break;

            msg.Add((byte)val);
        }

        return msg.ToArray();
    }

    public Response ParseRecv(InputReader reader, byte[] res) {
        string recv = Encoding.ASCII.GetString(res);
        if (recv.StartsWith("ERR")) {
            return ParseErr(reader, recv);
        } else if (recv.StartsWith("REPLY OK")) {
            return ParseReplyOk(reader, recv);
        } else if (recv.StartsWith("REPLY NOK")) {
            return ParseReplyNok(reader, recv);
        } else if (recv.StartsWith("MSG")) {
            return ParseMsg(reader, recv);
        } else if (recv.Equals("BYE\r\n")) {
            return Response.Bye;
        }
        return Response.None;
    }

    public void Close() {
        Stream.Close();
        Client.Close();
    }

    public bool CanEnd() {
        return true;
    }

    /// <summary>
    /// Sends given message
    /// </summary>
    /// <param name="msg">Message</param>
    public virtual void Send(string msg) {
        byte[] data = Encoding.ASCII.GetBytes(msg);
        Stream.Write(data, 0, data.Length);
    }


    /// <summary>
    /// Function that prints parsed message
    /// </summary>
    /// <param name="match">Regex match containing matched groups</param>
    /// <returns>Response type</returns>
    private delegate Response ParsePrint(Match match);

    /// <summary>
    /// Generic function for parsing message from server
    /// </summary>
    /// <param name="msg">Message</param>
    /// <param name="pat">Regex pattern to be used</param>
    /// <param name="print">Print function</param>
    /// <returns>Reponse type</returns>
    private Response ParseMessage(string msg, string pat, ParsePrint print) {
        var match = Regex.Match(msg, pat);
        if (!match.Success)
            return Response.None;

        return print(match);
    }

    private Response ParseErr(InputReader reader, string msg) {
        string pattern = @"^ERR FROM ([\x21-\x7E]+) IS ([\x20-\x7E]+)\r\n";
        return ParseMessage(msg, pattern, match => {
            reader.PrintErr(
                $"ERR FROM {match.Groups[1].Value}: {match.Groups[2].Value}");
            return Response.Err;
        });
    }

    private Response ParseReplyOk(InputReader reader, string msg) {
        string pattern = @"^REPLY OK IS ([\x20-\x7E]+)\r\n";
        return ParseMessage(msg, pattern, match => {
            reader.PrintErr($"Success: {match.Groups[1].Value}");
            return Response.ReplyOk;
        });
    }

    private Response ParseReplyNok(InputReader reader, string msg) {
        string pattern = @"^REPLY NOK IS ([\x20-\x7E]+)\r\n";
        return ParseMessage(msg, pattern, match => {
            reader.PrintErr($"Failure: {match.Groups[1].Value}");
            return Response.ReplyNok;
        });
    }

    private Response ParseMsg(InputReader reader, string msg) {
        string pattern = @"MSG FROM ([\x21-\x7E]+) IS ([\x20-\x7E]+)\r\n";
        return ParseMessage(msg, pattern, match => {
            reader.Print($"{match.Groups[1].Value}: {match.Groups[2].Value}");
            return Response.Msg;
        });
    }
}