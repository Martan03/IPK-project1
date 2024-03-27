using System.Net;
using System.Net.Sockets;
using System.Text;

public class UDP
{
    public ComState State { get; set; } = ComState.Start;

    private UdpClient Client { get; set; } = new();
    private IPAddress IP { get; set; }
    private ushort Port { get; set; }
    private ushort MsgId { get; set; } = 1;


    public UDP(string host, ushort port) {
        IPAddress[] addresses = Dns.GetHostAddresses(host);
        IPAddress? hostIp = null;
        foreach (IPAddress ip in addresses) {
            if (ip.AddressFamily == AddressFamily.InterNetwork) {
                hostIp = ip;
                break;
            }
        }
        if (hostIp is null)
            throw new Exception("Cannot get IPv4 of hostname");

        IP = hostIp!;
        Console.WriteLine(IP.ToString());
        Port = port;
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
        List<byte> bytes = [(byte)Type.BYE, .. BitConverter.GetBytes(MsgId)];
        MsgId++;

        Send(bytes.ToArray());
    }

    public void Confirm(ushort id) {
        List<byte> msg = [(byte)Type.CONFIRM, .. BitConverter.GetBytes(id)];

        Send(msg.ToArray());
    }

    public void Send(byte[] msg) {
        Client.Send(msg, msg.Length, new IPEndPoint(IP, Port));
        State = ComState.ConfWait;
    }

    public string Recv() {
        if (Client.Available <= 0)
            return "";

        Console.WriteLine("test");
        IPEndPoint remoteEP = new(IP, 0);
        byte[] recv = Client.Receive(ref remoteEP);

        return Encoding.UTF8.GetString(recv);
    }

    public void Close() {
        Client.Close();
    }
}
