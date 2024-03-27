using System.Text.RegularExpressions;

public static class Validator {
    /// <summary>
    /// Checks whether Username is valid
    /// </summary>
    /// <param name="name">Username to be checked</param>
    /// <exception cref="ArgumentException">When invalid</exception>
    public static void Username(string name) {
        string pattern = @"^[a-zA-Z0-9\-]+$";
        if (name.Length > 20 || !Regex.IsMatch(name, pattern)) {
            throw new ArgumentException(
                "Username contains not allowed characters"
            );
        }
    }

    /// <summary>
    /// Checks whether DisplayName is valid
    /// </summary>
    /// <param name="name">DisplayName to be checked</param>
    /// <exception cref="ArgumentException">When invalid</exception>
    public static void DisplayName(string name) {
        string pattern = @"^[\x21-\x7E]+$";
        if (name.Length > 20 || !Regex.IsMatch(name, pattern)) {
            throw new ArgumentException(
                "Display name contains not allowed characters"
            );
        }
    }

    /// <summary>
    /// Checks whether Secret is valid
    /// </summary>
    /// <param name="secret">Secret to be checked</param>
    /// <exception cref="ArgumentException">When invalid</exception>
    public static void Secret(string secret) {
        string pattern = @"^[a-zA-Z0-9\-]+$";
        if (secret.Length > 128 || !Regex.IsMatch(secret, pattern)) {
            throw new ArgumentException(
                "Secret contains not allowed characters"
            );
        }
    }

    /// <summary>
    /// Checks whether ChannelID is valid
    /// </summary>
    /// <param name="channel">ChannelID to be checked</param>
    /// <exception cref="ArgumentException">When invalid</exception>
    public static void ChannelID(string channel) {
        // TODO: remove this line
        channel = channel.Replace(".", "");

        string pattern = @"^[a-zA-Z0-9\-]+$";
        if (channel.Length > 20 || !Regex.IsMatch(channel, pattern)) {
            throw new ArgumentException(
                "ChannelID contains not allowed characters"
            );
        }
    }

    /// <summary>
    /// Checkes whether MessageContent is valid
    /// </summary>
    /// <param name="content">MessageContent to be checked</param>
    /// <exception cref="ArgumentException">When invalid</exception>
    public static void MessageContent(string content) {
        string pattern = @"^[\x20-\x7E]+$";
        if (content.Length > 1400 || !Regex.IsMatch(content, pattern)) {
            throw new ArgumentException(
                "Message content contains not allowed characters"
            );
        }
    }
}