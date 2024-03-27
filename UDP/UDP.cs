using System.Net;
using System.Net.Sockets;
using System.Text;

public class UDP : IComm {
    private UdpClient Client { get; set; } = new();
    private IPEndPoint EP;
    private Args Arg { get; set; }

    private ushort MsgId { get; set; } = 0;
    private Dictionary<int, MsgItem> Msgs { get; set; } = new();

    /// <summary>
    /// Constructs new UDP communication wrapper
    /// </summary>
    /// <param name="arg"></param>
    public UDP(Args arg) {
        EP = new(GetIPv4(arg.Host), arg.Port);
        Arg = arg;
    }

    public void Auth(string name, string secret, string nick) {
        byte[] msg = [
            (byte)Type.AUTH,
            .. BitConverter.GetBytes(MsgId),
            .. Encoding.UTF8.GetBytes(name),
            0,
            .. Encoding.UTF8.GetBytes(nick),
            0,
            .. Encoding.UTF8.GetBytes(secret),
            0,
        ];
        Send(msg);
    }

    public void Join(string name, string channel) {
        byte[] msg = [
            (byte)Type.JOIN,
            .. BitConverter.GetBytes(MsgId),
            .. Encoding.UTF8.GetBytes(channel),
            0,
            .. Encoding.UTF8.GetBytes(name),
            0,
        ];
        Send(msg);
    }

    public void Msg(string from, string msg) {
        byte[] bytes = [
            (byte)Type.MSG,
            .. BitConverter.GetBytes(MsgId),
            .. Encoding.UTF8.GetBytes(from),
            0,
            .. Encoding.UTF8.GetBytes(msg),
            0,
        ];
        Send(bytes);
    }

    public void Err(string from, string msg) {
        List<byte> bytes =
        [
            (byte)Type.ERR,
            .. BitConverter.GetBytes(MsgId),
            .. Encoding.UTF8.GetBytes(from),
            0,
            .. Encoding.UTF8.GetBytes(msg),
            0,
        ];
        Send(bytes.ToArray());
    }

    public void Bye() {
        byte[] bytes = [(byte)Type.BYE, .. BitConverter.GetBytes(MsgId)];
        Send(bytes);
    }

    public void Send(byte[] msg) {
        Client.Send(msg, msg.Length, EP);

        Msgs[MsgId] = new(msg);
        MsgId++;
    }

    public byte[] Recv() {
        Resend();
        if (Client.Available <= 0)
            return [];

        return Client.Receive(ref EP);
    }

    public void Close() {
        Client.Close();
    }

    public void Resend() {
        var now = DateTime.Now;
        List<int> rem = new();
        foreach (var msg in Msgs) {
            var item = msg.Value;
            if (now - item.Time > TimeSpan.FromMilliseconds(Arg.Timeout)) {
                Client.Send(item.Msg, item.Msg.Length, EP);
                item.Next();

                if (item.Retries > Arg.Retransmit)
                    rem.Add(msg.Key);
            }
        }

        foreach (int key in rem) {
            Msgs.Remove(key);
        }
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
                ParseErrMsg(reader, res);
                return Response.Msg;
            case (byte)Type.ERR:
                Confirm(res);
                ParseErrMsg(reader, res, "ERR FROM ");
                return Response.Err;
            case (byte)Type.BYE:
                Confirm(res);
                return Response.Bye;
        }

        return recv;
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

    private void Confirm(byte[] res) {
        var msgId = BitConverter.ToUInt16(res, 1);
        byte[] msg = [(byte)Type.CONFIRM, .. BitConverter.GetBytes(msgId)];
        Client.Send(msg, msg.Length, EP);
    }

    private void ParseConfirm(byte[] res) {
        var msgId = BitConverter.ToUInt16(res, 1);
        Msgs.Remove(msgId);
    }

    private Response ParseReply(InputReader reader, byte[] res) {
        var result = "Failure";
        var recv = Response.ReplyNok;
        if (res[3] == 1) {
            result = "Success";
            recv = Response.ReplyOk;
        }

        ReadOnlySpan<byte> msgBytes = BytesTillNull(res[6..]);
        var msg = Encoding.UTF8.GetString(msgBytes);
        reader.PrintErr($"{result}: {msg}");
        return recv;
    }

    private void ParseErrMsg(InputReader reader, byte[] res, string pre = "") {
        ReadOnlySpan<byte> nameBytes = BytesTillNull(res[3..]);
        var name = Encoding.UTF8.GetString(nameBytes);

        var offset = 3 + nameBytes.Length + 1;
        ReadOnlySpan<byte> msgBytes = BytesTillNull(res[offset..]);
        var msg = Encoding.UTF8.GetString(msgBytes);
        reader.PrintErr($"{pre}{name}: {msg}");
    }

    private ReadOnlySpan<byte> BytesTillNull(ReadOnlySpan<byte> bytes) {
        int index = bytes.IndexOf((byte)0);

        if (index == -1)
            return bytes;

        return bytes.Slice(0, index);
    }
}
