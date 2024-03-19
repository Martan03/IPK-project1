public enum ComState {
    Start,
    Auth,
    Open,
    Error,
    End,
}

public interface IComm {
    ComState State { get; set; }

    /// <summary>
    /// Sends Auth message and waits for response
    /// </summary>
    /// <param name="name">Username</param>
    /// <param name="secret">Secret/password</param>
    /// <param name="nick">Display name</param>
    /// <returns>Response message</returns>
    string Auth(string name, string secret, string nick);

    void Send(string msg);

    string Recv();

    void Close();

    void Bye();
}
