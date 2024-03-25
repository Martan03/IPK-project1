using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class UDP : IComm
{
    public ComState State { get; set; }

    private UdpClient Client { get; set; } = new();
    private IPAddress IP { get; set; }
    private ushort Port { get; set; }
    private ushort MsgId { get; set; } = 0;


    public UDP(string host, ushort port) {
        IPAddress[] addresses = Dns.GetHostAddresses(host);
        IP = addresses[0];
        Port = port;
    }

    public bool Auth(string name, string secret, string nick) {
        List<byte> msg = new();
        msg.Add((byte)Type.AUTH);
        msg.AddRange(BitConverter.GetBytes(MsgId));
        msg.AddRange(Encoding.UTF8.GetBytes(name));
        msg.Add(0);
        msg.AddRange(Encoding.UTF8.GetBytes(nick));
        msg.Add(0);
        msg.AddRange(Encoding.UTF8.GetBytes(secret));
        msg.Add(0);
        MsgId++;
        return true;
    }

    public bool Join(string name, string channel) {
        List<byte> msg = new();
        msg.Add((byte)Type.JOIN);
        msg.AddRange(BitConverter.GetBytes(MsgId));
        msg.AddRange(Encoding.UTF8.GetBytes(channel));
        msg.Add(0);
        msg.AddRange(Encoding.UTF8.GetBytes(name));
        msg.Add(0);

        MsgId++;
        Send(msg.ToArray());
        return true;
    }

    public bool Msg(string from, string msg) {
        List<byte> bytes = new();
        bytes.Add((byte)Type.MSG);
        bytes.AddRange(BitConverter.GetBytes(MsgId));
        bytes.AddRange(Encoding.UTF8.GetBytes(from));
        bytes.Add(0);
        bytes.AddRange(Encoding.UTF8.GetBytes(msg));
        bytes.Add(0);

        MsgId++;
        Send(bytes.ToArray());
        return true;
    }

    public void Err(string from, string msg) {
        List<byte> bytes = new();
        bytes.Add((byte)Type.ERR);
        bytes.AddRange(BitConverter.GetBytes(MsgId));
        bytes.AddRange(Encoding.UTF8.GetBytes(from));
        bytes.Add(0);
        bytes.AddRange(Encoding.UTF8.GetBytes(msg));
        bytes.Add(0);

        MsgId++;
        Send(bytes.ToArray());
    }

    public void Bye() {
        List<byte> bytes = new();
        bytes.Add((byte)Type.BYE);
        bytes.AddRange(BitConverter.GetBytes(MsgId));
        MsgId++;
    }

    public void Confirm(ushort id) {
        List<byte> msg = new();
        msg.Add((byte)Type.CONFIRM);
        msg.AddRange(BitConverter.GetBytes(id));

        Send(msg.ToArray());
    }

    public void Send(byte[] msg) {
        Client.Send(msg, msg.Length, IP.ToString(), Port);
    }

    public string Recv() {
        IPEndPoint? remoteEP = null;
        byte[] recv = Client.Receive(ref remoteEP);

        return Encoding.UTF8.GetString(recv);
    }

    public void Close() {
        Client.Close();
    }
}
