using System.Net.Sockets;

namespace IPK_project1;

class Program
{
    static void Main(string[] args) {
        Options? options = ParseArgs(args);
    }

    /// <summary>
    /// Parses input arguments
    /// </summary>
    /// <param name="args">array of arguments</param>
    static Options? ParseArgs(string[] args) {
        bool help = false;
        Options options = new();

        foreach (string arg in args) {
            switch (arg) {
                case "-t":
                    options.Type = arg;
                    break;
                case "-s":
                    options.Host = arg;
                    break;
                case "-p":
                    if (ushort.TryParse(arg, out ushort port)) {
                        options.Port = port;
                    } else {
                        Console.WriteLine("Invalid port");
                        return null;
                    }
                    break;
                case "-d":
                    if (ushort.TryParse(arg, out ushort timeout)) {
                        options.Timeout = timeout;
                    } else {
                        Console.WriteLine("Invalid timeout");
                        return null;
                    }
                    break;
                case "-r":
                    if (byte.TryParse(arg, out byte retransmit)) {
                        options.Retransmit = retransmit;
                    } else {
                        Console.WriteLine("Invalid retransmit");
                        return null;
                    }
                    break;
                case "-h":
                    help = true;
                    break;
            }
        }

        if (!help)
            return options;

        if (args.Length >= 2) {
            Console.WriteLine("Help must be used alone");
            return null;
        }

        Help();
        return null;
    }

    static void Help() {
        Console.WriteLine("TODO");
    }

    /*static void TcpMessage(string host, ushort port) {
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
    }*/
}
