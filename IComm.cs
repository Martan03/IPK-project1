/// <summary>
/// Communication states
/// </summary>
public enum ComState {
    Start,
    Auth,
    Open,
    Error,
    End,
    ConfWait,
}

public interface IComm {
    ComState State { get; set; }

    /// <summary>
    /// Sends Auth message and waits for response
    /// </summary>
    /// <param name="name">Username</param>
    /// <param name="secret">Secret/password</param>
    /// <param name="nick">Display name</param>
    /// <returns>False when Auth cannot be used in current state</returns>
    bool Auth(string name, string secret, string nick);

    /// <summary>
    /// Joins given channel
    /// </summary>
    /// <param name="name">Display name</param>
    /// <param name="channel">Channel ID</param>
    /// <returns>Fale when Join cannot be used in current state</returns>
    bool Join(string name, string channel);

    /// <summary>
    /// Send MSG to the server
    /// </summary>
    /// <param name="from">Display name</param>
    /// <param name="msg">Message</param>
    /// <returns>False when Msg cannot be used in current state</returns>
    bool Msg(string from, string msg);

    /// <summary>
    /// Sends ERR to the server
    /// </summary>
    /// <param name="from">Display name</param>
    /// <param name="msg">Message</param>
    void Err(string from, string msg);

    /// <summary>
    /// Terminates conversation
    /// </summary>
    void Bye();

    string Recv();

    void Close();
}
