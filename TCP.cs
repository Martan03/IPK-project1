using System.Net.Sockets;
using System.Text;

public class TCP : IComm {
    public ComState State { get; set; } = ComState.Start;

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

    public string Auth(string name, string secret, string nick) {
        if (State != ComState.Start && State != ComState.Auth)
            return "";

        State = ComState.Auth;
        Send($"AUTH {name} AS {nick} USING {secret}\r\n");

        return RecvWait();
    }

    public string Join(string name, string channel) {
        if (State != ComState.Open)
            return "";

        Send($"JOIN {channel} AS {name}\r\n");
        return RecvWait();
    }

    public string Msg(string from, string msg) {
        if (State != ComState.Open)
            return "";

        Send($"MSG FROM {from} IS {msg}\r\n");
        return RecvWait();
    }

    public void Bye() {
        if (State != ComState.Start && State != ComState.End)
            Send("BYE");
        State = ComState.End;
    }

    public void Send(string msg) {
        byte[] data = Encoding.UTF8.GetBytes(msg);
        Stream.Write(data, 0, data.Length);
    }

    public string Recv() {
        List<byte> msg = new();
        while (Stream.DataAvailable) {
            int val = Stream.ReadByte();
            if (val == -1)
                break;

            msg.Add((byte)val);
        }

        return Encoding.UTF8.GetString(msg.ToArray());
    }

    /// <summary>
    /// Closes the connection
    /// </summary>
    public void Close() {
        Stream.Close();
        Client.Close();
    }

    string RecvWait() {
        List<byte> msg = new();
        while (Stream.DataAvailable) {
            int val = Stream.ReadByte();
            if (val == -1)
                break;

            msg.Add((byte)val);
        }

        return Encoding.UTF8.GetString(msg.ToArray());
    }
}