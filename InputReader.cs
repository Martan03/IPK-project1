using System.Text;

public class InputReader {
    private StringBuilder Text { get; set; }
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
                    Home();
                    break;
                case ConsoleKey.End:
                    End();
                    break;
                case ConsoleKey.LeftArrow:
                    CursorLeft();
                    break;
                case ConsoleKey.RightArrow:
                    CursorRight();
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

    private string Enter() {
        var res = Text.ToString();
        Text.Clear();

        Pos = 0;
        Console.CursorTop++;

        return res;
    }

    private void Backspace() {
        if (Pos <= 0)
            return;

        Pos--;
        Text.Remove(Pos, 1);
        Console.CursorLeft--;

        var after = Text.ToString().Substring(Pos);
        Console.Write($"\x1b[0J{after}");
        Console.CursorLeft -= after.Length;
    }

    private void Delete() {
        if (Pos == Text.Length)
            return;

        Text.Remove(Pos, 1);
        var after = Text.ToString().Substring(Pos);
        Console.Write($"\x1b[0J{after}");
        Console.CursorLeft -= after.Length;
    }

    private void Home() {
        Console.Write("\u001b8");
        Pos = 0;
    }

    private void End() {
        var width = Console.WindowWidth;
        Pos = Text.Length;
        Console.Write($"\u001b8\x1b[{Pos / width}B\x1b[{Pos % width}C");
    }

    private void CursorLeft() {
        if (Pos <= 0)
            return;

        Pos--;
        if (Console.CursorLeft == 0) {
            Console.SetCursorPosition(
                Console.WindowWidth - 1,
                Console.CursorTop - 1
            );
        } else {
            Console.CursorLeft--;
        }
    }

    private void CursorRight() {
        if (Pos >= Text.Length)
            return;

        Pos++;
        if (Console.CursorLeft == Console.WindowWidth - 1)
            Console.SetCursorPosition(0, Console.CursorTop + 1);
        else
            Console.CursorLeft++;
    }

    private void MoveTo(int pos) {
        var width = Console.WindowWidth;
        Console.Write($"\u001b8");

        var top = pos / width;
        if (top > 0)
            Console.Write($"\x1b[{top}B");

        var left = pos % width;
        if (left > 0)
            Console.Write($"\x1b[{left}C");
    }
}
