using System.Net;
using System.Net.Sockets;
using System.Text;

public class UDP : IComm {
    private UdpClient Client { get; set; } = new();
    private IPEndPoint EP;
    private Args Arg { get; set; }

    private ushort MsgId { get; set; } = 0;
    private MsgItem? LastMsg { get; set; }
    private Queue<QMsg> Msgs { get; set; } = new();

    /// <summary>
    /// Constructs new UDP communication wrapper
    /// </summary>
    /// <param name="arg"></param>
    public UDP(Args arg) {
        EP = new(GetIPv4(arg.Host), arg.Port);
        Arg = arg;
    }

    public void Auth(string name, string secret, string nick) {
        QMsg msg = new(Type.AUTH, [
            .. Encoding.ASCII.GetBytes(name),
            0,
            .. Encoding.ASCII.GetBytes(nick),
            0,
            .. Encoding.ASCII.GetBytes(secret),
            0,
        ]);
        Send(msg);
    }

    public void Join(string name, string channel) {
        QMsg msg = new(Type.JOIN, [
            .. Encoding.ASCII.GetBytes(channel),
            0,
            .. Encoding.ASCII.GetBytes(name),
            0,
        ]);
        Send(msg);
    }

    public void Msg(string from, string content) {
        QMsg msg = new(Type.MSG, [
            .. Encoding.ASCII.GetBytes(from),
            0,
            .. Encoding.ASCII.GetBytes(content),
            0,
        ]);
        Send(msg);
    }

    public void Err(string from, string content) {
        QMsg msg = new(Type.ERR, [
            .. Encoding.ASCII.GetBytes(from),
            0,
            .. Encoding.ASCII.GetBytes(content),
            0,
        ]);
        Send(msg);
    }

    public void Bye() {
        QMsg msg = new(Type.BYE, []);
        Send(msg);
    }

    public byte[] Recv() {
        Resend();
        if (Client.Available <= 0)
            return [];

        return Client.Receive(ref EP);
    }

    public Response ParseRecv(InputReader reader, byte[] res) {
        var recv = Response.None;
        switch (res[0]) {
            case (byte)Type.CONFIRM:
                ParseConfirm(res);
                return Response.None;
            case (byte)Type.REPLY:
                Confirm(res);
                return ParseReply(reader, res);
            case (byte)Type.MSG:
                Confirm(res);
                ParseMsg(reader, res);
                return Response.Msg;
            case (byte)Type.ERR:
                Confirm(res);
                ParseErr(reader, res);
                return Response.Err;
            case (byte)Type.BYE:
                Confirm(res);
                return Response.Bye;
        }

        return recv;
    }

    public void Close() {
        Client.Close();
    }

    public bool CanEnd() {
        return LastMsg is null && Msgs.Count == 0;
    }

    /// <summary>
    /// Sends given message
    /// </summary>
    /// <param name="msg">Queue message</param>
    public virtual void Send(QMsg msg) {
        if (LastMsg is not null) {
            Msgs.Enqueue(msg);
            return;
        }
        var val = msg.Get(MsgId);
        Client.Send(val, val.Length, EP);
        LastMsg = new(val);
        LastMsg.Next();
    }

    /// <summary>
    /// Resends last message when no confirmation was received
    /// </summary>
    private void Resend() {
        if (!NextMsg())
            return;

        var now = DateTime.Now;
        if (now - LastMsg!.Time <= TimeSpan.FromMilliseconds(Arg.Timeout))
            return;

        if (LastMsg.Retries > Arg.Retransmit) {
            LastMsg = null;
            MsgId++;
            return;
        }
        Client.Send(LastMsg.Msg, LastMsg.Msg.Length, EP);
        LastMsg.Next();
    }

    /// <summary>
    /// Sends confirm message
    /// </summary>
    /// <param name="res">Received message in bytes</param>
    private void Confirm(byte[] res) {
        var msgId = BitConverter.ToUInt16(res, 1);
        byte[] msg = [(byte)Type.CONFIRM, .. BitConverter.GetBytes(msgId)];
        Client.Send(msg, msg.Length, EP);
    }

    /// <summary>
    /// Gets next message from queue
    /// </summary>
    /// <returns>True when some message, else false</returns>
    private bool NextMsg() {
        if (LastMsg is not null)
            return true;

        if (Msgs.Count > 0) {
            var msg = Msgs.Dequeue();
            LastMsg = new(msg.Get(MsgId));
            return true;
        }
        return false;
    }

    /// <summary>
    /// Gets IPv4 address by host address
    /// </summary>
    /// <param name="host">Host address</param>
    /// <returns></returns>
    /// <exception cref="Exception">When can't get IPv4</exception>
    private IPAddress GetIPv4(string host) {
        IPAddress[] addresses = Dns.GetHostAddresses(host);
        foreach (IPAddress ip in addresses) {
            if (ip.AddressFamily == AddressFamily.InterNetwork) {
                return ip;
            }
        }

        throw new Exception("Cannot get IPv4 of hostname");
    }

    private void ParseConfirm(byte[] res) {
        var msgId = BitConverter.ToUInt16(res, 1);

        MsgId++;
        LastMsg = null;
        NextMsg();
    }

    private Response ParseReply(InputReader reader, byte[] res) {
        var result = "Failure";
        var recv = Response.ReplyNok;
        if (res[3] == 1) {
            result = "Success";
            recv = Response.ReplyOk;
        }

        ReadOnlySpan<byte> msgBytes = BytesTillNull(res[6..]);
        var msg = Encoding.ASCII.GetString(msgBytes);
        reader.PrintErr($"{result}: {msg}");
        return recv;
    }

    private void ParseMsg(InputReader reader, byte[] res) {
        ReadOnlySpan<byte> nameBytes = BytesTillNull(res[3..]);
        var name = Encoding.ASCII.GetString(nameBytes);

        var offset = 3 + nameBytes.Length + 1;
        ReadOnlySpan<byte> msgBytes = BytesTillNull(res[offset..]);
        var msg = Encoding.ASCII.GetString(msgBytes);
        reader.Print($"{name}: {msg}");
    }

    private void ParseErr(InputReader reader, byte[] res) {
        ReadOnlySpan<byte> nameBytes = BytesTillNull(res[3..]);
        var name = Encoding.ASCII.GetString(nameBytes);

        var offset = 3 + nameBytes.Length + 1;
        ReadOnlySpan<byte> msgBytes = BytesTillNull(res[offset..]);
        var msg = Encoding.ASCII.GetString(msgBytes);
        reader.PrintErr($"ERR FROM {name}: {msg}");
    }

    /// <summary>
    /// Gets all bytes until null byte
    /// </summary>
    /// <param name="bytes">Bytes to read from</param>
    /// <returns>Bytes span till null byte</returns>
    private ReadOnlySpan<byte> BytesTillNull(ReadOnlySpan<byte> bytes) {
        int index = bytes.IndexOf((byte)0);
        if (index == -1)
            return bytes;

        return bytes.Slice(0, index);
    }
}
