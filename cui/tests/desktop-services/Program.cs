using System.Text;
using Managed.Forms;
using static Managed.Forms.Cui;

// This executable calls public translated entry points, never PlatformHost.
// The Python coordinator drives the real desktop tools on its own Xvfb.
internal static unsafe class Program
{
    private static readonly UTF8Encoding Utf8 = new(false, true);
    private static int assertions;
    private static string root = "";

    private static void Check(bool value, string description)
    {
        assertions++;
        if (!value) throw new InvalidOperationException(description);
    }

    private static void Event(string kind, params string[] fields)
    {
        Console.WriteLine("CUI_DESKTOP_SERVICE\t" + kind + "\t" + string.Join('\t', fields));
        Console.Out.Flush();
    }

    private static void Frame(DesktopApplication app)
    {
        Check(app.NextFrame(), "Active app pumps after desktop service");
        fixed (byte* text = "Real desktop services"u8) cui_label(app.Ui, text);
    }

    private static void Dialog(DesktopApplication app, string id, cui_file_dialog_kind kind,
        string path, bool success, int capacity = 4096)
    {
        string title = $"CUI-service-{Environment.ProcessId}-{id}";
        byte[] titleBytes = Utf8.GetBytes(title + '\0');
        byte[] defaultBytes = Utf8.GetBytes(path + '\0');
        byte[] output = Enumerable.Repeat((byte)0xa5, 8192).ToArray();
        bool actual;
        Event("DIALOG", id, title, path);
        fixed (byte* titlePointer = titleBytes)
        fixed (byte* name = defaultBytes)
        fixed (byte* filter = "txt"u8)
        fixed (byte* filterName = "Private fixture text"u8)
        fixed (byte* buffer = output)
        {
            var options = new cui_file_dialog_opts
            {
                kind = kind, title = titlePointer, default_name = name,
                filter = filter, filter_name = filterName
            };
            actual = cui_app_file_dialog(null, &options, buffer, (ulong)capacity) != 0;
        }
        Check(actual == success, id + " return value");
        Check(output.AsSpan(capacity).ToArray().All(b => b == 0xa5), id + " capacity guard");
        if (success)
        {
            byte[] expected = Utf8.GetBytes(path + '\0');
            Check(output.AsSpan(0, expected.Length).SequenceEqual(expected),
                id + " exact UTF-8 path and NUL; received=" + Utf8.GetString(output.AsSpan(0, Array.IndexOf(output, (byte)0))));
            Check(output.AsSpan(expected.Length, capacity - expected.Length).ToArray().All(b => b == 0xa5),
                id + " writes only path and terminator");
            Console.WriteLine($"PASS {id}: UTF8={Convert.ToHexString(expected)}");
        }
        else Check(output[0] == 0, id + " clears stale output");
        Check(cui_app_ui(null) == app.Ui, id + " retains active app");
        Check(app.Ui->window_focused != 0 && app.Ui->mouse_inside == 0,
            id + " translated focus/mouse cleanup");
        Frame(app);
        Event("DIALOG_DONE", id, actual ? "true" : "false");
    }

    private static bool Open(string value)
    {
        byte[] bytes = Utf8.GetBytes(value + '\0');
        fixed (byte* text = bytes) return cui_app_open_url(text) != 0;
    }

    private static void Url(string id, string value, bool expected, bool recordsHandler, int handlerExit = 0,
        string? expectedArgument = null)
    {
        string log = Path.Combine(root, id + ".argv");
        Environment.SetEnvironmentVariable("CUI_DESKTOP_ARGV_LOG", log);
        Environment.SetEnvironmentVariable("CUI_DESKTOP_HANDLER_EXIT", handlerExit.ToString());
        Event("URL", id, value);
        bool actual = Open(value);
        Check(actual == expected, id + " launcher result");
        if (recordsHandler)
        {
            // gio may launch asynchronously: a successful launcher exit says
            // nothing about the handler's eventual exit. Wait for our receipt.
            var timer = System.Diagnostics.Stopwatch.StartNew();
            while (!File.Exists(log) || !File.Exists(Path.ChangeExtension(log, ".handler.json")))
            {
                if (timer.Elapsed > TimeSpan.FromSeconds(10)) throw new TimeoutException(id + " handler receipt");
                Thread.Sleep(10);
            }
            byte[] expectedBytes = Utf8.GetBytes("desktop-fixed\0" + (expectedArgument ?? value) + "\0");
            Check(File.ReadAllBytes(log).AsSpan().SequenceEqual(expectedBytes), id + " literal argv boundaries");
        }
        else Check(!File.Exists(log), id + " does not invoke fixture handler");
        Event("URL_DONE", id, actual ? "true" : "false");
    }

    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            Console.OutputEncoding = Utf8;
            if (!OperatingSystem.IsLinux())
            {
                Console.WriteLine("UNRUN: this real-tool coordinator qualifies Linux/X11 only; Windows requires its external interactive gate.");
                return 77;
            }
            if (args.Length != 2) throw new ArgumentException("Usage: DesktopServices.Tests fixture-root dialog-tool");
            root = Path.GetFullPath(args[0]);
            string tool = args[1];
            string fixture = Path.Combine(root, "fixture");
            string open = Path.Combine(fixture, "atverti λ Žąsis ' ; &.txt");
            string save = Path.Combine(fixture, "išsaugoti λ Žąsis.txt");
            string folder = Path.Combine(fixture, "aplankas λ Žąsis");
            string leading = Path.Combine(fixture, "-literal λ ' ; &.txt");
            using (var app = new DesktopApplication("CUI desktop services " + tool, 480, 320, continuous: true))
            {
                Frame(app);
                Check(cui_app_ui(null) == app.Ui && cui_app_native_window(null) != null, "Live owning app/current-app lookup");
                Dialog(app, "open-confirm", cui_file_dialog_kind.CUI_FILE_OPEN, open, true);
                Dialog(app, "open-cancel", cui_file_dialog_kind.CUI_FILE_OPEN, open, false);
                Dialog(app, "save-confirm", cui_file_dialog_kind.CUI_FILE_SAVE, save, true);
                Check(!File.Exists(save), "Save chooser returns a path without creating a file");
                Dialog(app, "folder-confirm", cui_file_dialog_kind.CUI_FILE_FOLDER, folder, true);
                Dialog(app, "small-capacity", cui_file_dialog_kind.CUI_FILE_OPEN, open, false, 4);

                string normalPath = Environment.GetEnvironmentVariable("PATH")!;
                try
                {
                    Environment.SetEnvironmentVariable("PATH", Path.Combine(root, "empty-bin"));
                    byte[] output = [0xa5, 0xa5];
                    fixed (byte* buffer = output)
                        Check(cui_app_file_dialog(null, null, buffer, 2) == 0 && buffer[0] == 0,
                            "Both dialog executables unavailable: false and cleared output");
                    Check(!Open("cuidsmissing://fixture"), "Launcher executable unavailable");
                }
                finally { Environment.SetEnvironmentVariable("PATH", normalPath); }
                Frame(app);

                string scheme = File.ReadAllText(Path.Combine(root, "scheme.txt")).Trim();
                // Shell-looking characters must remain one literal argument.
                string url = scheme + "://fixture/λ space?quote=\"'&semi=;$(touch SHOULD_NOT_EXIST)&percent=%25";
                Url("generic-success", url, true, true);
                Url("generic-handler-fails", url + "&failure=23", false, true, 23);
                Url("no-handler", scheme + "-unregistered://fixture", false, false);
                string oldDirectory = Environment.CurrentDirectory;
                try
                {
                    Environment.CurrentDirectory = fixture;
                    // PlatformHost must protect a leading '-' from xdg-open's
                    // option parser, then preserve the actual path as one arg.
                    Url("leading-dash", Path.GetFileName(leading), true, true, expectedArgument: leading);
                }
                finally { Environment.CurrentDirectory = oldDirectory; }
                fixed (byte* empty = ""u8)
                    Check(cui_app_open_url(null) == 0 && cui_app_open_url(empty) == 0, "Null/empty URL rejected");

                // A real GNOME/gio launch illustrates the asynchronous boundary.
                Environment.SetEnvironmentVariable("XDG_CURRENT_DESKTOP", "GNOME");
                try { Url("gio-handler-fails", scheme + "://fixture/encoded%20space?exit=23&literal=%CE%BB", true, true, 23); }
                finally { Environment.SetEnvironmentVariable("XDG_CURRENT_DESKTOP", "X-Generic"); }
                Frame(app);
                app.Quit();
                Check(!app.NextFrame(), "Desktop app quits after services");
            }
            Check(cui_app_ui(null) == null && cui_app_native_window(null) == null, "App/window released after services");
            using (var again = new DesktopApplication("CUI service cleanup", hidden: true, continuous: true)) Frame(again);
            Check(cui_app_ui(null) == null, "Reinitialized app also released");
            Check(!File.Exists(Path.Combine(root, "fixture", "SHOULD_NOT_EXIST")) &&
                !File.Exists(Path.Combine(root, "SHOULD_NOT_EXIST")), "URL did not execute shell content");
            Event("PASS", tool, assertions.ToString());
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
