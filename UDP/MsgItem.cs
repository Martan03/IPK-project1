public class MsgItem {
    public DateTime Time { get; set; }
    public byte[] Msg { get; set; }
    public byte Retries { get; set; }

    public MsgItem(byte[] msg) {
        Msg = msg;
        Time = DateTime.Now;
        Retries = 0;
    }

    public void Next() {
        Time = DateTime.Now;
        Retries++;
    }
}
