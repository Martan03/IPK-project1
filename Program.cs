namespace IPK_project1;

class Program
{
    static void Main(string[] args) {
        try {
            Args arg = new(args);
            if (arg.Help)
                return;

            Client client = new(arg);
            client.Start();
        } catch (Exception e) {
            Console.Error.WriteLine($"ERR: {e.Message}");
        }
    }
}
