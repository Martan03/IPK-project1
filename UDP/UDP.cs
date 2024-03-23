using System.Net.Sockets;

public class UDP : IComm
{
    public ComState State { get; set; }

    private UdpClient Client { get; set; }

    public bool Auth(string name, string secret, string nick)
    {
        throw new NotImplementedException();
    }

    public bool Join(string name, string channel)
    {
        throw new NotImplementedException();
    }

    public bool Msg(string from, string msg)
    {
        throw new NotImplementedException();
    }

    public void Err(string from, string msg)
    {
        throw new NotImplementedException();
    }

    public void Bye()
    {
        throw new NotImplementedException();
    }

    public void Send(byte[] msg) {
        // Client.Send(msg, msg.Length, )
    }

    public string Recv()
    {
        throw new NotImplementedException();
    }

    public void Close()
    {
        throw new NotImplementedException();
    }
}
