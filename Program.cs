using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace IPK_project1;

class Program
{
    static void Main(string[] args) {
        Args arg = new();
        try {
            arg.Parse(args);
        } catch (Exception e) {
            Console.Error.WriteLine($"Error: {e.Message}");
        }
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
