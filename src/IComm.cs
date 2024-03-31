/// <summary>
/// Communication states
/// </summary>
public enum ComState {
    Start,
    Auth,
    Open,
    End,
}

public interface IComm {
    /// <summary>
    /// Sends Auth message and waits for response
    /// </summary>
    /// <param name="name">Username</param>
    /// <param name="secret">Secret/password</param>
    /// <param name="nick">Display name</param>
    /// <returns>False when Auth cannot be used in current state</returns>
    void Auth(string name, string secret, string nick);

    /// <summary>
    /// Joins given channel
    /// </summary>
    /// <param name="name">Display name</param>
    /// <param name="channel">Channel ID</param>
    /// <returns>Fale when Join cannot be used in current state</returns>
    void Join(string name, string channel);

    /// <summary>
    /// Send MSG to the server
    /// </summary>
    /// <param name="from">Display name</param>
    /// <param name="msg">Message</param>
    /// <returns>False when Msg cannot be used in current state</returns>
    void Msg(string from, string msg);

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

    /// <summary>
    /// Receives data
    /// </summary>
    /// <returns>byte array containing data</returns>
    byte[] Recv();

    /// <summary>
    /// Parses received data
    /// </summary>
    /// <param name="reader">Input reader</param>
    /// <param name="res">Received data</param>
    /// <returns></returns>
    Response ParseRecv(InputReader reader, byte[] res);

    /// <summary>
    /// Closes the connection
    /// </summary>
    void Close();

    /// <summary>
    /// Returns whether communication can end
    /// </summary>
    /// <returns></returns>
    bool CanEnd();
}
