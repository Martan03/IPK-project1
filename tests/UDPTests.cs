namespace IPK_project1.Tests;

class UDPTester : UDP {
    public QMsg? SentMsg { get; set; }

    public UDPTester(Args arg) : base(arg) { }

    public override void Send(QMsg msg) {
        SentMsg = msg;
    }
}

public class UDPTests {
    [Fact]
    public void TestAuthFormat() {
        Args args = new(["-s", "anton5.fit.vutbr.cz", "-t", "udp"]);
        UDPTester udp = new(args);

        udp.Auth("a", "b", "c");
        byte[] res = [
            0x02,
            0x01,
            0,
            0x61,
            0,
            0x63,
            0,
            0x62,
            0
        ];
        Assert.Equal(res, udp.SentMsg!.Get(0x01));
    }

    [Fact]
    public void TestJoinFormat() {
        Args args = new(["-s", "anton5.fit.vutbr.cz", "-t", "udp"]);
        UDPTester udp = new(args);

        udp.Join("a", "b");
        byte[] res = [
            0x03,
            0x05,
            0,
            0x62,
            0,
            0x61,
            0
        ];
        Assert.Equal(res, udp.SentMsg!.Get(0x05));
    }

    [Fact]
    public void TestMsgFormat() {
        Args args = new(["-s", "anton5.fit.vutbr.cz", "-t", "udp"]);
        UDPTester udp = new(args);

        udp.Msg("a", "b");
        byte[] res = [
            0x04,
            0x1f,
            0,
            0x61,
            0,
            0x62,
            0
        ];
        Assert.Equal(res, udp.SentMsg!.Get(0x1f));
    }

    [Fact]
    public void TestErrFormat() {
        Args args = new(["-s", "anton5.fit.vutbr.cz", "-t", "udp"]);
        UDPTester udp = new(args);

        udp.Err("a", "b");
        byte[] res = [
            0xFE,
            0xff,
            0x52,
            0x61,
            0,
            0x62,
            0
        ];
        Assert.Equal(res, udp.SentMsg!.Get(0x52ff));
    }

    [Fact]
    public void TestByeFormat() {
        Args args = new(["-s", "anton5.fit.vutbr.cz", "-t", "udp"]);
        UDPTester udp = new(args);

        udp.Bye();
        byte[] res = [
            0xFF,
            0xff,
            0x52,
        ];
        Assert.Equal(res, udp.SentMsg!.Get(0x52ff));
    }
}
