using System.Text;

public class InputReader {
    public StringBuilder Text { get; set; }

    /// <summary>
    /// Creates new InputParser
    /// </summary>
    public InputReader() {
        Text = new();
    }

    /// <summary>
    /// Reads available keys from input
    /// </summary>
    public string Read() {
        while (Console.KeyAvailable) {
            var key = Console.ReadKey();
            switch (key.Key) {
                case ConsoleKey.Enter:
                    var res = Text.ToString();
                    Text.Clear();
                    return res;
                case ConsoleKey.Backspace:
                    Text.Remove(Text.Length - 1, 1);
                    break;
            }
            Text.Append(key.KeyChar);
        }
        return "";
    }
}
