using System.Text;
using Managed.GhosttyVt;

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
