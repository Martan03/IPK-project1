/// <summary>
/// Last msg class
/// </summary>
public class MsgItem {
    public DateTime Time { get; set; }
    public byte[] Msg { get; set; }
    public byte Retries { get; set; }

    public MsgItem(byte[] msg) {
        Msg = msg;
        Time = DateTime.Now;
        Retries = 0;
    }

    /// <summary>
    /// Updates time to current and adds one to retries
    /// </summary>
    public void Next() {
        Time = DateTime.Now;
        Retries++;
    }
}

/// <summary>
/// Class representing queued message
/// </summary>
public class QMsg {
    private Type Type { get; set; }
    private byte[] Msg { get; set; }

    public QMsg(Type type, byte[] msg) {
        Type = type;
        Msg = msg;
    }

    /// <summary>
    /// Gets message
    /// </summary>
    /// <param name="id">Message ID</param>
    /// <returns>Final message in bytes</returns>
    public byte[] Get(ushort id) {
        return [
            (byte)Type,
            .. BitConverter.GetBytes(id),
            .. Msg,
        ];
    }
}
