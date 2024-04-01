using System.Net.Sockets;

namespace IPK_project1.Tests;

class TCPTester : TCP {
    public string SentMsg { get; set; } = "";

    public TCPTester(string host, ushort port) : base(host, port) { }

    public override void Send(string msg) {
        SentMsg = msg;
    }
}

public class TCPTests {
    [Fact]
    public void TestAuthFormat() {
        Args args = new(["-s", "anton5.fit.vutbr.cz", "-t", "tcp"]);
        TCPTester tcp = new(args.Host, args.Port);

        tcp.Auth("a", "b", "c");
        string res = "AUTH a AS c USING b\r\n";
        Assert.Equal(res, tcp.SentMsg);
    }

    [Fact]
    public void TestJoinFormat() {
        Args args = new(["-s", "anton5.fit.vutbr.cz", "-t", "tcp"]);
        TCPTester tcp = new(args.Host, args.Port);

        tcp.Join("a", "b");
        string res = "JOIN b AS a\r\n";
        Assert.Equal(res, tcp.SentMsg);
    }

    [Fact]
    public void TestMsgFormat() {
        Args args = new(["-s", "anton5.fit.vutbr.cz", "-t", "tcp"]);
        TCPTester tcp = new(args.Host, args.Port);

        tcp.Msg("a", "b");
        string res = "MSG FROM a IS b\r\n";
        Assert.Equal(res, tcp.SentMsg);
    }

    [Fact]
    public void TestErrFormat() {
        Args args = new(["-s", "anton5.fit.vutbr.cz", "-t", "tcp"]);
        TCPTester tcp = new(args.Host, args.Port);

        tcp.Err("a", "b");
        string res = "ERR FROM a IS b\r\n";
        Assert.Equal(res, tcp.SentMsg);
    }

    [Fact]
    public void TestByeFormat() {
        Args args = new(["-s", "anton5.fit.vutbr.cz", "-t", "tcp"]);
        TCPTester tcp = new(args.Host, args.Port);

        tcp.Bye();
        string res = "BYE\r\n";
        Assert.Equal(res, tcp.SentMsg);
    }
}
