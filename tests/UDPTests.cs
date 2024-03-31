namespace IPK_project1.Tests;

class UDPTester : UDP {
    public QMsg? SentMsg { get; set; }

    public UDPTester(Args arg) : base(arg) { }

    public override void Send(QMsg msg) {
        SentMsg = msg;
    }
}

public class UDPTests {
    /// <summary>
    /// Tests maximum length of each arguments
    /// </summary>
    [Fact]
    public void TestMsgs() {
        Args args = new(["-s", "anton5.fit.vutbr.cz", "-t", "udp"]);
        UDPTester udp = new(args);

        udp.Auth("name", "topsecret", "nick");

    }
}
