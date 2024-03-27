using System.Net;
using System.Net.Sockets;
using System.Text;

public class MessageItem {
    public DateTime Time { get; set; }
    public byte[] Msg { get; set; }
    public byte Retries { get; set; }

    public MessageItem(byte[] msg) {
        Msg = msg;
        Time = DateTime.Now;
        Retries = 0;
    }

    public void Next() {
        Time = DateTime.Now;
        Retries++;
    }
}

public class UDP {
    private UdpClient Client { get; set; } = new();
    private IPEndPoint EP;
    private Args Arg { get; set; }

    private ushort MsgId { get; set; } = 0;
    private Dictionary<int, MessageItem> Msgs { get; set; } = new();

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

    public void Confirm(ushort id) {
        byte[] msg = [(byte)Type.CONFIRM, .. BitConverter.GetBytes(id)];
        Client.Send(msg, msg.Length, EP);
    }

    public void ConfirmMsg(ushort id) {
        Msgs.Remove(id);
    }

    public void Send(byte[] msg) {
        Client.Send(msg, msg.Length, EP);

        Msgs[MsgId] = new(msg);
        MsgId++;
    }

    public void Resend() {
        var now = DateTime.Now;
        foreach (var msg in Msgs) {
            var item = msg.Value;
            if (now - item.Time > TimeSpan.FromMilliseconds(Arg.Timeout)) {
                Send(item.Msg);
                item.Next();

                if (item.Retries > Arg.Retransmit)
                    Msgs.Remove(msg.Key);
            }
        }
    }

    public byte[] Recv() {
        if (Client.Available <= 0)
            return [];

        return Client.Receive(ref EP);
    }

    public void Close() {
        Client.Close();
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
}
