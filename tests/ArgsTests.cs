namespace IPK_project1.Tests;

public class ArgsTests {
    [Fact]
    public void TestMandatory() {
        Assert.Throws<ArgumentException>(
            () => new Args([])
        );
        Assert.Throws<ArgumentException>(
            () => new Args(["-t", "udp"])
        );
        Assert.Throws<ArgumentException>(
            () => new Args(["-s", "server-address"])
        );
    }

    [Fact]
    public void TestMandatoryValues() {
        Assert.Throws<ArgumentException>(
            () => new Args(["-t"])
        );
        Assert.Throws<ArgumentException>(
            () => new Args(["-s"])
        );
        Assert.Throws<ArgumentException>(
            () => new Args(["-t", "adsfasd"])
        );
        Assert.Throws<ArgumentException>(
            () => new Args(["-s", "test"])
        );
        try {
            new Args(["-s", "test", "-t", "udp"]);
            new Args(["-s", "test", "-t", "tcp"]);
            Assert.True(true);
        } catch (ArgumentException) {
            Assert.True(false);
        }
    }

    [Fact]
    public void TestCannotCombineWithHelp() {
        Assert.Throws<ArgumentException>(
            () => new Args(["-h", "-t", "test"])
        );
        Assert.Throws<ArgumentException>(
            () => new Args(["-h", "-s", "test"])
        );
        Assert.Throws<ArgumentException>(
            () => new Args(["-p", "123", "-h"])
        );
    }

    [Fact]
    public void TestOptionalArgs() {
        Assert.Throws<ArgumentException>(
            () => new Args(["-s", "test", "-t", "tcp", "-p", "-5"])
        );
        Assert.Throws<ArgumentException>(
            () => new Args(["-s", "test", "-t", "tcp", "-p", "test"])
        );
        Assert.Throws<ArgumentException>(
            () => new Args(["-s", "test", "-t", "tcp", "-p", "65536"])
        );

        Assert.Throws<ArgumentException>(
            () => new Args(["-s", "test", "-t", "tcp", "-d", "-5"])
        );
        Assert.Throws<ArgumentException>(
            () => new Args(["-s", "test", "-t", "tcp", "-d", "test"])
        );
        Assert.Throws<ArgumentException>(
            () => new Args(["-s", "test", "-t", "tcp", "-d", "65536"])
        );

        Assert.Throws<ArgumentException>(
            () => new Args(["-s", "test", "-t", "tcp", "-r", "-5"])
        );
        Assert.Throws<ArgumentException>(
            () => new Args(["-s", "test", "-t", "tcp", "-r", "test"])
        );
        Assert.Throws<ArgumentException>(
            () => new Args(["-s", "test", "-t", "tcp", "-r", "256"])
        );
    }
}
