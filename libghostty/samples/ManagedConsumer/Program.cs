using System.Text;
using Managed;

static void Equal<T>(T expected, T actual, string label)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
        throw new InvalidOperationException($"{label}: expected {expected}, observed {actual}");
}

using var terminal = new Terminal(20, 5);
using var independent = new Terminal(12, 3);
terminal.Write("Hello\r\n\u001b[31mworld\u001b[0m");
Equal("Hello\nworld", terminal.Snapshot(), "styled transcript");
Equal(new TerminalPosition(20, 5, 5, 1), terminal.Position, "cursor");
Equal((uint)'w', terminal.GetCell(0, 1).Codepoint, "cell codepoint");
Equal((byte?)1, terminal.GetCell(0, 1).ForegroundPalette, "cell ANSI foreground");
terminal.Write("\u001b[?25l");
Equal(false, terminal.GetMode(25), "cursor visibility mode");
terminal.Write("\u001b[?25h");
Equal(true, terminal.GetMode(25), "cursor visibility restored");
var unicode = Encoding.UTF8.GetBytes(" λ界");
foreach (var value in unicode) terminal.Write(new byte[] { value });
Equal("Hello\nworld λ界", terminal.Snapshot(), "split UTF-8");
independent.Write("independent");
Equal("independent", independent.Snapshot(), "second owner");
var primary = terminal.ActiveScreen;
terminal.Write("\u001b[?1049h\u001b[HALT");
Equal("ALT", terminal.Snapshot(), "alternate screen");
if (primary == terminal.ActiveScreen) throw new InvalidOperationException("alternate screen did not change");
terminal.Write("\u001b[?1049l");
Equal(primary, terminal.ActiveScreen, "primary screen restored");
Equal("Hello\nworld λ界", terminal.Snapshot(), "primary contents restored");
terminal.Resize(30, 6);
Equal((ushort)30, terminal.Position.Columns, "resized columns");
Equal((ushort)6, terminal.Position.Rows, "resized rows");
terminal.Reset();
Equal(string.Empty, terminal.Snapshot(), "reset content");
Equal(new TerminalPosition(30, 6, 0, 0), terminal.Position, "reset cursor and dimensions");
Equal("independent", independent.Snapshot(), "instance isolation after reset");
using (var styled = new Terminal(10, 3))
{
    Equal(string.Empty, styled.Snapshot(), "empty snapshot");
    styled.Write("\u001b[1;2;3;4:3;5;7;8;9;53;38;2;12;34;56mS\u001b[0mN");
    var allStyles = TerminalTextStyle.Bold | TerminalTextStyle.Italic | TerminalTextStyle.Faint
        | TerminalTextStyle.Blink | TerminalTextStyle.Inverse | TerminalTextStyle.Invisible
        | TerminalTextStyle.Strikethrough | TerminalTextStyle.Overline;
    Equal(new TerminalCell('S', allStyles, 3, null, 0x0c2238), styled.GetCell(0, 0), "combined style and RGB");
    Equal(new TerminalCell('N', TerminalTextStyle.None, 0, null, null), styled.GetCell(1, 0), "style reset");
    styled.Reset();
    styled.Write("abcdefghijklmn");
    Equal("abcdefghijklmn", styled.Snapshot(), "soft-line unwrap");
}
// Exercise the new 1.0 print-run/store paths around their batch boundaries.
using (var runs = new Terminal(65, 8))
{
    foreach (var length in new[] { 63, 64, 65, 127, 128, 129, 255, 256, 257, 513 })
    {
        var text = new string(Enumerable.Range(0, length).Select(i => (char)('A' + i % 26)).ToArray());
        foreach (var chunked in new[] { false, true })
        {
            runs.Reset();
            runs.Write("\u001b[1;38;2;12;34;56m");
            if (chunked)
                for (var offset = 0; offset < text.Length; offset += 64)
                    runs.Write(text.Substring(offset, Math.Min(64, text.Length - offset)));
            else
                runs.Write(text);
            Equal(text, runs.Snapshot(), "long ASCII batch and soft wrap");
            var last = length - 1;
            Equal(new TerminalCell((uint)text[last], TerminalTextStyle.Bold, 0, null, 0x0c2238),
                runs.GetCell((ushort)(last % 65), (ushort)(last / 65)), "last batch cell and style");
        }
    }
}
terminal.Dispose();
terminal.Dispose();
try
{
    terminal.Write("disposed");
    throw new InvalidOperationException("disposed owner accepted input");
}
catch (ObjectDisposedException) { }

using var rendezvous = new Barrier(2);
Task RunIndependent(string text) => Task.Factory.StartNew(() =>
{
    using var owner = new Terminal(20, 3);
    if (!rendezvous.SignalAndWait(TimeSpan.FromSeconds(15)))
        throw new InvalidOperationException("Independent terminal workers did not rendezvous.");
    owner.Write("\u001b[1;32m" + text + "\u001b[0m");
    Equal(text, owner.Snapshot(), "concurrent owner content");
    Equal((byte?)2, owner.GetCell(0, 0).ForegroundPalette, "concurrent owner style");
}, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
Task.WaitAll(RunIndependent("first λ"), RunIndependent("second 界"));
Console.WriteLine("PASS owning terminal transcript, UTF-8 chunking, screens, resize/reset, concurrent isolation and disposal");
