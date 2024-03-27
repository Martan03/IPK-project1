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
        Send("BYE");
    }

    public void Send(string msg) {
        byte[] data = Encoding.UTF8.GetBytes(msg);
        Stream.Write(data, 0, data.Length);
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

    /// <summary>
    /// Closes the connection
    /// </summary>
    public void Close() {
        Stream.Close();
        Client.Close();
    }

    public Response ParseRecv(InputReader reader, byte[] res) {
        string recv = Encoding.UTF8.GetString(res);
        if (recv.StartsWith("ERR")) {
            string pattern =
                @"^ERR FROM ([a-zA-Z0-9\-]+) IS ([\x20-\x7E]+)\r\n";
            var match = Regex.Match(recv, pattern);

            if (!match.Success)
                return Response.None;

            reader.PrintErr(
                $"ERR FROM {match.Groups[1].Value}: {match.Groups[2].Value}"
            );
            return Response.Err;
        } else if (recv.StartsWith("REPLY OK")) {
            string pattern = @"^REPLY OK IS ([\x20-\x7E]+)\r\n";
            var match = Regex.Match(recv, pattern);

            if (!match.Success)
                return Response.None;

            reader.PrintErr($"Success: {match.Groups[1].Value}");
            return Response.ReplyOk;
        } else if (recv.StartsWith("REPLY NOK")) {
            string pattern = @"^REPLY NOK IS ([\x20-\x7E]+)\r\n";
            var match = Regex.Match(recv, pattern);

            if (!match.Success)
                return Response.None;

            reader.PrintErr($"Failure: {match.Groups[1].Value}");
            return Response.ReplyNok;
        } else if (recv.StartsWith("MSG")) {
            string pattern =
                @"MSG FROM ([a-zA-Z0-9\-]+) IS ([\x20-\x7E]+)\r\n";
            var match = Regex.Match(recv, pattern);

            if (!match.Success)
                return Response.None;

            reader.Print($"{match.Groups[1].Value}: {match.Groups[2].Value}");
            return Response.Msg;
        } else if (recv.Equals("BYE\r\n")) {
            return Response.Bye;
        }
        return Response.None;
    }
}