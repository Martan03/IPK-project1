using System.Net.Sockets;
using CommandLine;

namespace IPK_project1;

class Options
{
    [Option('h', Required = true, HelpText = "Sets host to given address")]
    public string? Host { get; set; }
    [Option('p', Required = true, HelpText = "Sets port to given port value")]
    public ushort Port { get; set; }
    [Option('m', Required = true,
        HelpText = "Sets communication mode (valid values: udp/tcp)")]
    public string? Mode { get; set; }
}

class Program
{
    static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args).WithParsed(o => {
            switch (o.Mode) {
                case "udp":
                    Console.WriteLine($"UDP: host {o.Host} port {o.Port}");
                    break;
                case "tcp":
                    TcpMessage(o.Host!, o.Port);
                    break;
                default:
                    Console.WriteLine($"Invalid communication mode: {o.Mode}");
                    break;
            }
        });
    }

    static void TcpMessage(string host, ushort port) {
        TcpClient client = new TcpClient(host, port);
        NetworkStream stream = client.GetStream();

        StreamWriter writer = new StreamWriter(stream);
        StreamReader reader = new StreamReader(stream);

        writer.WriteLine("HELLO");
        writer.Flush();

        string? response = reader.ReadLine();
        if (response == "HELLO") {
            writer.WriteLine("SOLVE (+ 2 2)");
            writer.Flush();

            string? res = reader.ReadLine();
            Console.WriteLine($"Result: {res}");

            writer.WriteLine("BYE");
            writer.Flush();
        }

        reader.Close();
        writer.Close();
        stream.Close();
        client.Close();
    }
}
