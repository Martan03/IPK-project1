namespace IPK_project1.Tests;

public class ValidatorTests {
    /// <summary>
    /// Tests maximum length of each arguments
    /// </summary>
    [Fact]
    public void TestMaxLength() {
        string name = "TooLongNameToFit12345";
        Assert.Throws<ArgumentException>(
            () => Validator.Username(name)
        );
        Assert.Throws<ArgumentException>(
            () => Validator.DisplayName(name)
        );
        Assert.Throws<ArgumentException>(
            () => Validator.ChannelID(name)
        );
        Assert.Throws<ArgumentException>(
            () => Validator.Secret(new string('a', 129))
        );
        Assert.Throws<ArgumentException>(
            () => Validator.MessageContent(new string('a', 1401))
        );
    }
}
