namespace IPK_project1;

class Program
{
    static void Main(string[] args) {
        try {
            StartClient(args);
        } catch (Exception e) {
            Console.Error.WriteLine($"Error: {e.Message}");
        }
    }

    static void StartClient(string[] args) {
        Args arg = new(args);
        InputReader reader = new();

        IComm client = arg.Type switch {
            ComType.TCP => new TCP(arg),
            _ => throw new Exception("Err"),
        };

        Console.CancelKeyPress += delegate {
            client.Bye();
        };

        Console.WriteLine("Connected i guess..");

        while (client.State != ComState.End) {
            var read = reader.Read();
            if (read.Length != 0) {
                ParseInput(client, read);
            }

            var recv = client.Recv();
            if (recv.Length != 0) {
                Console.Write(recv);
            }
        }

        client.Close();
    }

    static string ParseInput(IComm client, string text) {
        string[] parts = text.Split();
        switch (parts[0]) {
            case "/auth":
                if (parts.Length != 4) {
                    Console.Error.WriteLine("Invalid number of arguments");
                    return "";
                }
                return client.Auth(parts[1], parts[2], parts[3]);
            case "/join":
                break;
            case "/rename":
                break;
            case "/help":
                break;
            default:
                Console.Error.WriteLine($"Invalid command: {parts[0]}");
                break;
        }
        return "";
    }

    static void ParseResp(string res) {

    }

    /*
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
    }*/
}
