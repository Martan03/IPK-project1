namespace IPK_project1;

class Program
{
    static void Main(string[] args) {
        try {
            Args arg = new(args);
            UDPClient client = new(arg);
            Console.WriteLine("Connected...");
            client.Start();
        } catch (Exception e) {
            Console.Error.WriteLine($"Error: {e.Message}");
        }
    }
}
