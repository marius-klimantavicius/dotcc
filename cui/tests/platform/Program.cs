using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;
using Managed.Forms;
using static Managed.Forms.Cui;

internal static unsafe class Program
{
    private static int assertions;
    private static readonly Encoding Utf8 = new UTF8Encoding(false, true);
    private static void Check(bool condition, string message)
    {
        assertions++;
        if (!condition) throw new Exception(message);
    }
    private static bool B(CBool value) => value != 0;
    private static byte[] Z(string value) => Utf8.GetBytes(value + '\0');
    private static string Text(byte* text) => Marshal.PtrToStringUTF8((nint)text)!;

    private static void Layouts(string work)
    {
        using var native = JsonDocument.Parse(File.ReadAllText(Path.Combine(work, "native-abi.json")));
        Check(sizeof(CBool) == 1, "C _Bool is one byte");
        Check(sizeof(cui_file_entry) == native.RootElement.GetProperty("entry")[0].GetInt32(), "entry native/emitted size");
        string[] entry = { "name", "dir", "size", "modified" };
        for (int i = 0; i < entry.Length; i++)
            Check((int)Marshal.OffsetOf<cui_file_entry>(entry[i]) == native.RootElement.GetProperty("entry")[i + 1].GetInt32(), "entry offset " + entry[i]);
        Check(sizeof(cui_file_dialog_opts) == native.RootElement.GetProperty("dialog")[0].GetInt32(), "dialog native/emitted size");
        string[] dialog = { "kind", "title", "filter_name", "filter", "default_name" };
        for (int i = 0; i < dialog.Length; i++)
            Check((int)Marshal.OffsetOf<cui_file_dialog_opts>(dialog[i]) == native.RootElement.GetProperty("dialog")[i + 1].GetInt32(), "dialog offset " + dialog[i]);
        Check((int)cui_file_dialog_kind.CUI_FILE_OPEN == 0 && (int)cui_file_dialog_kind.CUI_FILE_SAVE == 1 && (int)cui_file_dialog_kind.CUI_FILE_FOLDER == 2, "dialog enum ABI");
        // Windows SDK x64 layouts; checked independently of the current OS.
        NativeLayout("BrowseInfo", 64, new() { ["owner"] = 0, ["root"] = 8, ["displayName"] = 16, ["title"] = 24,
            ["flags"] = 32, ["callback"] = 40, ["parameter"] = 48, ["image"] = 56 });
        NativeLayout("OpenFileName", 152, new() { ["size"] = 0, ["owner"] = 8, ["instance"] = 16,
            ["filter"] = 24, ["customFilter"] = 32, ["maxCustomFilter"] = 40, ["filterIndex"] = 44,
            ["file"] = 48, ["maxFile"] = 56, ["fileTitle"] = 64, ["maxFileTitle"] = 72,
            ["initialDirectory"] = 80, ["title"] = 88, ["flags"] = 96, ["fileOffset"] = 100,
            ["fileExtension"] = 102, ["defaultExtension"] = 104, ["customData"] = 112, ["hook"] = 120,
            ["templateName"] = 128, ["reserved"] = 136, ["reserved2"] = 144, ["flagsEx"] = 148 });
    }
    private static void NativeLayout(string name, int size, Dictionary<string, int> offsets)
    {
        Type type = typeof(PlatformHost).GetNestedType(name, BindingFlags.NonPublic)!;
        Check(Marshal.SizeOf(type) == size, name + " x64 size");
        foreach (var pair in offsets) Check((int)Marshal.OffsetOf(type, pair.Key) == pair.Value, name + "." + pair.Key);
    }

    private static void Files(string work)
    {
        string directory = Path.Combine(work, "files");
        Directory.CreateDirectory(directory);
        Directory.CreateDirectory(Path.Combine(directory, "folder"));
        File.WriteAllBytes(Path.Combine(directory, "žą😀.txt"), new byte[] { 1, 2, 3 });
        File.WriteAllText(Path.Combine(directory, ".hidden"), "hidden");
        var time = new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        File.SetLastWriteTimeUtc(Path.Combine(directory, "žą😀.txt"), time);
        byte[] path = Z(directory);
        cui_file_entry* entries = stackalloc cui_file_entry[8];
        byte* names = stackalloc byte[512];
        new Span<byte>(names, 512).Fill(0xA5);
        fixed (byte* p = path)
        {
            Check(platform_list(p, entries, 8, names, 511, false) == 2, "hidden files filtered");
            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            var actual = new Dictionary<string, int>();
            for (int i = 0; i < 2; i++)
            {
                Check(entries[i].name >= names && entries[i].name < names + 511, "names borrow only caller storage");
                actual[Text(entries[i].name)] = i;
            }
            Check(actual.Keys.Order().SequenceEqual(new[] { "folder", "žą😀.txt" }.Order()), "exact Unicode names");
            int file = actual["žą😀.txt"], folder = actual["folder"];
            Check(entries[file].dir == 0 && entries[file].size == 3 && entries[file].modified == new DateTimeOffset(time).ToUnixTimeSeconds(), "file metadata");
            Check(entries[folder].dir != 0 && entries[folder].size == 0, "directory metadata");
            Check(names[511] == 0xA5, "name buffer guard");
            Check(platform_list(p, entries, 8, names, 511, true) == 3, "hidden files included");
            Check(platform_list(p, entries, 1, names, 511, true) == 1, "entry capacity");
            Check(platform_list(p, null, 0, null, 0, false) == 0, "zero capacity");
            Check(platform_list(p, null, 1, names, 511, false) == -1, "invalid entry storage");
            Check(platform_list(p, entries, -1, names, 511, false) == -1, "invalid capacity");
            Check(platform_list(p, entries, 8, null, 1, false) == -1, "invalid name storage");
        }
        string one = Path.Combine(work, "one"); Directory.CreateDirectory(one);
        File.WriteAllText(Path.Combine(one, "é😀"), "x");
        fixed (byte* p = Z(one))
        {
            new Span<byte>(names, 12).Fill(0xA5);
            Check(platform_list(p, entries, 8, names, 6, false) == 0, "UTF-8 requires terminator space");
            Check(names[0] == 0xA5 && names[6] == 0xA5, "no partial UTF-8 writes");
            Check(platform_list(p, entries, 8, names, 7, false) == 1 && Text(entries[0].name) == "é😀", "UTF-8 exact byte capacity");
            Check(names[6] == 0 && names[7] == 0xA5, "exact terminator and guard");
        }
        fixed (byte* p = Z(directory + "/missing")) Check(platform_list(p, entries, 8, names, 512, false) == -1, "missing directory");
        byte* invalid = stackalloc byte[] { 0xFF, 0 };
        Check(platform_list(invalid, entries, 8, names, 512, false) == -1, "invalid path UTF-8");
        File.CreateSymbolicLink(Path.Combine(directory, "dangling"), Path.Combine(directory, "missing"));
        File.CreateSymbolicLink(Path.Combine(directory, "file-link"), Path.Combine(directory, "žą😀.txt"));
        Directory.CreateSymbolicLink(Path.Combine(directory, "dir-link"), Path.Combine(directory, "folder"));
        fixed (byte* p = path)
        {
            Check(platform_list(p, entries, 8, names, 512, true) == 6, "symbolic links remain listable");
            for (int i = 0; i < 6; i++)
            {
                string name = Text(entries[i].name);
                if (name == "dangling") Check(entries[i].dir == 0 && entries[i].size == 0 && entries[i].modified == 0, "dangling link has unknown stat metadata");
                if (name == "file-link") Check(entries[i].dir == 0 && entries[i].size == 3 && entries[i].modified == new DateTimeOffset(time).ToUnixTimeSeconds(), "file link follows target metadata");
                if (name == "dir-link") Check(entries[i].dir != 0 && entries[i].size == 0, "directory link follows target metadata");
            }
        }
    }

    private static void Algorithms()
    {
        Check(platform_algorithms() == 0, "translated paths, natural sorting, descending folders first, date/size and UTF-8 filtering");
        Check(platform_glfw_allocation_case(0) == 0, "GLFW initialization failure does not terminate an uninitialized library");
        Check(platform_glfw_allocation_case(1) == 0, "GLFW state allocation failure pairs initialization and termination");
        string? oldHome = Environment.GetEnvironmentVariable("HOME"), oldProfile = Environment.GetEnvironmentVariable("USERPROFILE");
        byte* home = stackalloc byte[1024]; byte* root = stackalloc byte[1024]; byte* desktop = stackalloc byte[1024];
        try
        {
            Environment.SetEnvironmentVariable("HOME", "");
            Environment.SetEnvironmentVariable("USERPROFILE", "Z:\\žą😀");
            Check(platform_places(1, home, root, desktop) == 5, "Windows home fallback count");
            Check(Text(home) == "Z:\\žą😀" && Text(root) == "C:\\" && Text(desktop) == "Z:\\žą😀\\Desktop", "Windows paths");
            Check(platform_places(0, home, root, desktop) == 1 && Text(root) == "/", "Linux root ignores Windows profile");
            Environment.SetEnvironmentVariable("HOME", "/home/žą😀");
            Check(platform_places(0, home, root, desktop) == 5 && Text(home) == "/home/žą😀" && Text(desktop) == "/home/žą😀/Desktop", "Linux home paths");
        }
        finally { Environment.SetEnvironmentVariable("HOME", oldHome); Environment.SetEnvironmentVariable("USERPROFILE", oldProfile); }
        string? oldRenderer = Environment.GetEnvironmentVariable("CUI_RENDERER");
        try
        {
            for (int i = 0; i <= 5; i++)
            {
                Environment.SetEnvironmentVariable("CUI_RENDERER", i == 1 ? "software" : null);
                Check(platform_app_case(i) == 0, "staged app selection/cleanup " + i);
            }
        }
        finally { Environment.SetEnvironmentVariable("CUI_RENDERER", oldRenderer); }
    }

    private static void Emoji()
    {
        string? windir = Environment.GetEnvironmentVariable("WINDIR");
        try
        {
            Environment.SetEnvironmentVariable("WINDIR", "Z:\\ž😀");
            Check(platform_emoji(1, 0) == 1 && platform_emoji_count() == 1 && platform_emoji_tried() == 1, "Windows emoji discovery");
            Check(Text(platform_emoji_path(0)) == "Z:\\ž😀\\Fonts\\seguiemj.ttf", "Unicode WINDIR font path");
            Environment.SetEnvironmentVariable("WINDIR", "");
            Check(platform_emoji(1, -1) == 0 && platform_emoji_count() == 1 && Text(platform_emoji_path(0)) == "C:\\Windows\\Fonts\\seguiemj.ttf", "Windows emoji fallback path");
            Environment.SetEnvironmentVariable("WINDIR", new string('x', 299));
            Check(platform_emoji(1, 0) == 0 && platform_emoji_count() == 0, "no truncated emoji file lookup");
            Check(platform_emoji(0, -1) == 0 && platform_emoji_count() == 9 && platform_emoji_tried() == 1, "Linux emoji discovery exhausted");
            Check(Text(platform_emoji_path(0)) == "/usr/share/fonts/truetype/noto/NotoColorEmoji.ttf" && Text(platform_emoji_path(8)) == "/system/fonts/NotoColorEmoji.ttf", "Linux ordered font search");
            Check(platform_emoji(0, 2) == 1 && platform_emoji_count() == 3, "emoji search stops at first successful load");
            Check(platform_emoji_null() == 0, "emoji null context");
        }
        finally { Environment.SetEnvironmentVariable("WINDIR", windir); }
    }

    private static void WindowsShim(string work)
    {
        nint shim = NativeLibrary.Load(Path.Combine(work, "windows-shim.so"));
        NativeLibrary.SetDllImportResolver(typeof(PlatformHost).Assembly, (name, assembly, path) =>
            name is "user32.dll" or "shell32.dll" or "ole32.dll" or "comdlg32.dll" ? shim : 0);
        var reset = (delegate* unmanaged[Cdecl]<int, void>)NativeLibrary.GetExport(shim, "platform_win_reset");
        var stat = (delegate* unmanaged[Cdecl]<int, int>)NativeLibrary.GetExport(shim, "platform_win_stat");
        MethodInfo method = typeof(PlatformHost).GetMethod("WindowsDialog", BindingFlags.Static | BindingFlags.NonPublic)!;
        string? Dialog(cui_file_dialog_opts* options) => (string?)method.Invoke(null, new[] { Pointer.Box(options, typeof(cui_file_dialog_opts*)) });
        fixed (byte* title = Z("Test ž😀"))
        fixed (byte* filterName = Z("Images"))
        fixed (byte* filter = Z("png;jpg"))
        fixed (byte* name = Z("ž😀.png"))
        {
            var options = new cui_file_dialog_opts { kind = cui_file_dialog_kind.CUI_FILE_FOLDER, title = title,
                filter_name = filterName, filter = filter, default_name = name };
            foreach (int mode in new[] { 0, 6 })
            {
                reset(mode);
                Check(Dialog(&options) == "Z:\\ž😀\\folder", "Windows folder native UTF-16 output");
                Check(stat(0) == 1 && stat(1) == 1 && stat(2) == 1 && stat(3) == 1 && stat(4) == 1 && stat(6) == 0, "Windows PIDL and COM paired, including S_FALSE");
            }
            reset(1);
            Check(Dialog(&options) == null && stat(0) == 1 && stat(1) == 0 && stat(4) == 0, "COM initialization failure");
            reset(2);
            Check(Dialog(&options) == null && stat(1) == 1 && stat(2) == 0 && stat(3) == 0 && stat(6) == 0, "folder cancellation ownership");
            reset(3);
            Check(Dialog(&options) == null && stat(1) == 1 && stat(2) == 1 && stat(3) == 1 && stat(6) == 0, "folder path failure frees PIDL");
            foreach (var kind in new[] { cui_file_dialog_kind.CUI_FILE_OPEN, cui_file_dialog_kind.CUI_FILE_SAVE })
            {
                options.kind = kind;
                reset(0);
                Check(Dialog(&options) == "Z:\\ž😀\\file.png" && stat(5) == 1 && stat(6) == 0, "Windows common dialog layout/filter/flags/output " + kind);
                reset(4);
                Check(Dialog(&options) == null && stat(5) == 1 && stat(6) == 0, "Windows common dialog cancellation " + kind);
            }
            fixed (byte* tooLong = Z(new string('x', 1040)))
            {
                options.default_name = tooLong; reset(0);
                Check(Dialog(&options) == null && stat(5) == 0, "Windows default name is not silently truncated");
            }
        }
        // P/Invoke stubs retain their library handles for the process lifetime.
    }

    [SupportedOSPlatform("linux")]
    private static void Tools(string work)
    {
        string directory = Path.Combine(work, "tools"); Directory.CreateDirectory(directory);
        string argsFile = Path.Combine(work, "tool-args");
        Environment.SetEnvironmentVariable("PLATFORM_ARGS", argsFile);
        Environment.SetEnvironmentVariable("PLATFORM_EXIT", "0");
        Environment.SetEnvironmentVariable("PLATFORM_RESULT", "/tmp/žą😀 ' ; $(literal).png");
        string script = "#!/bin/sh\nprintf '%s\\n' \"$@\" > \"$PLATFORM_ARGS\"\nprintf '%s\\n' \"$PLATFORM_RESULT\"\nexit \"$PLATFORM_EXIT\"\n";
        void Tool(string name, string body) { string file = Path.Combine(directory, name); File.WriteAllText(file, body); File.SetUnixFileMode(file, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute); }
        Tool("zenity", script); Tool("kdialog", script);
        string? oldPath = Environment.GetEnvironmentVariable("PATH");
        Environment.SetEnvironmentVariable("PATH", directory);
        byte* output = stackalloc byte[512];
        fixed (byte* title = Z("A title ' ; $(literal)"))
        fixed (byte* filter = Z("png;jpg"))
        fixed (byte* filterName = Z("Images '"))
        fixed (byte* name = Z("žą😀.png"))
        {
            var options = new cui_file_dialog_opts { kind = cui_file_dialog_kind.CUI_FILE_SAVE, title = title, filter = filter, filter_name = filterName, default_name = name };
            try
            {
                Check(B(platform_dialog(&options, output, 512)) && Text(output) == Environment.GetEnvironmentVariable("PLATFORM_RESULT"), "dialog Unicode path");
                string[] args = File.ReadAllLines(argsFile);
                Check(args.SequenceEqual(new[] { "--file-selection", "--title=A title ' ; $(literal)", "--save", "--confirm-overwrite", "--filename=žą😀.png", "--file-filter=Images ' | *.png *.jpg" }), "zenity argument boundaries");
                output[0] = 0xA5; output[1] = 0xA5;
                Check(!B(platform_dialog(&options, output, 2)) && output[0] == 0 && output[1] == 0xA5, "too-small dialog buffer clears without truncating");
                Environment.SetEnvironmentVariable("PLATFORM_RESULT", "ab\r\nignored");
                Check(B(platform_dialog(&options, output, 3)) && Text(output) == "ab", "first line CRLF and exact capacity");
                Check(B(platform_dialog(null, output, 512)), "default options");
                Environment.SetEnvironmentVariable("PLATFORM_EXIT", "1");
                Tool("kdialog", "#!/bin/sh\nexit 99\n");
                output[0] = 0xA5;
                Check(!B(platform_dialog(&options, output, 512)) && output[0] == 0, "cancellation does not use fallback");
                Check(File.ReadAllLines(argsFile)[0] == "--file-selection", "cancelled selected tool retained");
                Environment.SetEnvironmentVariable("PLATFORM_EXIT", "2");
                Check(!B(platform_dialog(&options, output, 512)), "dialog process failure");
                Environment.SetEnvironmentVariable("PLATFORM_EXIT", "0");
                File.Delete(Path.Combine(directory, "zenity")); Tool("kdialog", script);
                Check(B(platform_dialog(&options, output, 512)), "missing zenity falls back to kdialog");
                Check(File.ReadAllLines(argsFile).SequenceEqual(new[] { "--getsavefilename", "žą😀.png", "*.png *.jpg|Images '", "--title", "A title ' ; $(literal)" }), "kdialog save arguments");
                options.kind = cui_file_dialog_kind.CUI_FILE_FOLDER;
                Check(B(platform_dialog(&options, output, 512)), "folder dialog");
                Check(File.ReadAllLines(argsFile).SequenceEqual(new[] { "--getexistingdirectory", "žą😀.png", "--title", "A title ' ; $(literal)" }), "folder excludes filter");
                options.kind = (cui_file_dialog_kind)99; output[0] = 0xA5;
                Check(!B(platform_dialog(&options, output, 512)) && output[0] == 0, "invalid dialog kind");
                options.kind = cui_file_dialog_kind.CUI_FILE_OPEN;
                options.title = null;
                Tool("kdialog", "#!/bin/sh\nprintf '\\377'\n");
                Check(!B(platform_dialog(&options, output, 512)) && output[0] == 0, "invalid dialog output UTF-8");
                Tool("kdialog", "#!/bin/sh\nprintf '\\000path'\n");
                Check(!B(platform_dialog(&options, output, 512)) && output[0] == 0, "dialog output cannot contain embedded NUL");
                Tool("kdialog", script);
                Environment.SetEnvironmentVariable("PLATFORM_RESULT", "");
                Check(!B(platform_dialog(&options, output, 512)) && output[0] == 0, "empty result");
                output[0] = 0xA5;
                Check(!B(platform_dialog(&options, output, 0)) && output[0] == 0xA5, "zero output capacity");
                Check(!B(platform_dialog(&options, null, 1)), "null output");
                File.Delete(Path.Combine(directory, "kdialog"));
                Check(!B(platform_dialog(&options, output, 512)) && output[0] == 0, "no desktop dialog tool");
                Tool("xdg-open", "#!/bin/sh\nprintf '%s\\n' \"$@\" > \"$PLATFORM_ARGS\"\nexit \"$PLATFORM_EXIT\"\n");
                fixed (byte* url = Z("https://example.invalid/a ' ; $(literal)?q=žą😀"))
                {
                    Check(B(platform_url(url)), "URL successful launcher");
                    Check(File.ReadAllLines(argsFile).SequenceEqual(new[] { "https://example.invalid/a ' ; $(literal)?q=žą😀" }), "URL single literal argument");
                    Environment.SetEnvironmentVariable("PLATFORM_EXIT", "3");
                    Check(!B(platform_url(url)), "URL handler failure propagated");
                }
                Environment.SetEnvironmentVariable("PLATFORM_EXIT", "0");
                fixed (byte* file = Z("--help")) Check(B(platform_url(file)) && File.ReadAllLines(argsFile).SequenceEqual(new[] { Path.GetFullPath("--help") }), "file name cannot become launcher option");
                File.Delete(Path.Combine(directory, "xdg-open"));
                fixed (byte* url = Z("https://example.invalid")) Check(!B(platform_url(url)), "missing URL launcher");
                Check(!B(platform_url(null)), "null URL");
                fixed (byte* empty = Z("")) Check(!B(platform_url(empty)), "empty URL");
                byte* invalid = stackalloc byte[] { 0xFF, 0 };
                Check(!B(platform_url(invalid)), "invalid URL UTF-8");
            }
            finally { Environment.SetEnvironmentVariable("PATH", oldPath); }
        }
    }

    public static int Main(string[] args)
    {
        if (!OperatingSystem.IsLinux() || IntPtr.Size != 8) throw new PlatformNotSupportedException("Run this focused suite on Linux x64.");
        Check(PlatformHost.IsWindows() == 0, "runtime Linux selection");
        Layouts(args[0]); Files(args[0]); Algorithms(); Emoji(); WindowsShim(args[0]); Tools(args[0]);
        Console.WriteLine($"PASS {assertions} managed platform behavior/ABI assertions (actual emitted types and C algorithms)");
        Console.WriteLine("UNRUN Windows OS execution/dialog interaction, real desktop tools, font decoding and rendering. Windows native ABI shim is a test double.");
        return 0;
    }
}
