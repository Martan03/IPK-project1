using System.Text;

public class InputReader {
    private StringBuilder Text { get; set; }
    private int WindowWidth { get; set; } = 1;
    private int Pos { get; set; } = 0;

    /// <summary>
    /// Creates new InputParser
    /// </summary>
    public InputReader() {
        Console.Write("\u001b7");
        Text = new();
    }

    /// <summary>
    /// Reads available keys from input
    /// </summary>
    public string Read() {
        var width = Console.WindowWidth;
        if (width != WindowWidth) {
            WindowWidth = width;
            Redraw();
        }

        while (Console.KeyAvailable) {
            var key = Console.ReadKey();
            switch (key.Key) {
                case ConsoleKey.Enter:
                    return Enter();
                case ConsoleKey.Backspace:
                    Backspace();
                    break;
                case ConsoleKey.Delete:
                    Delete();
                    break;
                case ConsoleKey.Home:
                    MoveTo(0);
                    break;
                case ConsoleKey.End:
                    MoveTo(Text.Length);
                    break;
                case ConsoleKey.LeftArrow:
                    MoveTo(Pos - 1);
                    break;
                case ConsoleKey.RightArrow:
                    MoveTo(Pos + 1);
                    break;
                case ConsoleKey.UpArrow:
                    MoveTo(Pos - WindowWidth);
                    break;
                case ConsoleKey.DownArrow:
                    MoveTo(Pos + WindowWidth);
                    break;
                default:
                    Text.Insert(Pos, key.KeyChar);
                    Pos++;
                    var after = Text.ToString().Substring(Pos);
                    Console.Write(after);
                    MoveTo(Pos);
                    break;
            }
        }
        return "";
    }

    /// <summary>
    /// Prints text to stdin
    /// </summary>
    /// <param name="text">Text to be printed</param>
    public void Print(string text) {
        Console.WriteLine($"\u001b8\x1b[0J{text}");
        Console.Write($"\u001b7{Text}");
        MoveTo(Pos);
    }

    /// <summary>
    /// Prints text to stderr
    /// </summary>
    /// <param name="text">Text to be printed</param>
    public void PrintErr(string text) {
        Console.Error.WriteLine($"\u001b8\x1b[0J{text}");
        Console.Write($"\u001b7{Text}");
        MoveTo(Pos);
    }

    /// <summary>
    /// Resets print to current cursor position
    /// </summary>
    public void ResetPrint() {
        Console.Write($"\u001b7{Text}");
    }

    /// <summary>
    /// Clears screen and resets print
    /// </summary>
    public void ClearReset() {
        Console.Write($"\x1b[2J\x1b[H\u001b7{Text}");
    }

    /// <summary>
    /// Redraws current text
    /// </summary>
    private void Redraw() {
        Console.Write($"\u001b8\x1b[0J{Text}");
        MoveTo(Pos);
    }

    /// <summary>
    /// Handles enter press
    /// </summary>
    /// <returns>Inputed string</returns>
    private string Enter() {
        Console.WriteLine($"\u001b8{Text}");
        Console.Write("\u001b7");
        Pos = 0;

        var res = Text.ToString();
        Text.Clear();
        return res;
    }

    /// <summary>
    /// Handles backspace press
    /// </summary>
    private void Backspace() {
        if (Pos <= 0)
            return;

        Pos--;
        Text.Remove(Pos, 1);
        Console.CursorLeft--;

        var after = Text.ToString().Substring(Pos);
        Console.Write($"\x1b[0J{after}");
        MoveTo(Pos);
    }

    /// <summary>
    /// Handles delete press
    /// </summary>
    private void Delete() {
        if (Pos == Text.Length)
            return;

        Text.Remove(Pos, 1);
        var after = Text.ToString().Substring(Pos);
        Console.Write($"\x1b[0J{after}");
        MoveTo(Pos);
    }

    /// <summary>
    /// Moves cursor to given position
    /// </summary>
    /// <param name="pos">Position in text</param>
    private void MoveTo(int pos) {
        if (pos < 0 || pos > Text.Length)
            return;

        Pos = pos;
        Console.Write($"\u001b8");
        var top = pos / WindowWidth;
        if (top > 0)
            Console.Write($"\x1b[{top}B");

        var left = pos % WindowWidth;
        if (left > 0)
            Console.Write($"\x1b[{left}C");
    }
}
