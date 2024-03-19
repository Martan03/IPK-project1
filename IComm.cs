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

    /// <summary>
    /// Joins given channel
    /// </summary>
    /// <param name="name">Display name</param>
    /// <param name="channel">Channel ID</param>
    /// <returns>Response string</returns>
    string Join(string name, string channel);

    /// <summary>
    /// Send MSG to the server
    /// </summary>
    /// <param name="from">Display name</param>
    /// <param name="msg">Message</param>
    /// <returns>Response message</returns>
    string Msg(string from, string msg);

    /// <summary>
    /// Terminates conversation
    /// </summary>
    void Bye();

    void Send(string msg);

    string Recv();

    void Close();
}
