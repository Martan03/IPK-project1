using System.Reflection;

namespace IPK_project1;

public class Options {
    // type of the communication (tcp/udp)
    public string? Type { get; set; }
    // Server IP or hostname
    public string? Host { get; set; }
    // Server port
    public ushort Port { get; set; } = 4567;
    // UDP confirmation timeout
    public ushort Timeout { get; set; } = 250;
    // Maximum number of UDP retransmissions
    public byte Retransmit { get; set; } = 3;
}
