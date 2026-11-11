#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using static Managed.Forms.Cui;

namespace Managed.Forms;

internal static unsafe class PlatformHost
{
    private static readonly Encoding Utf8 = new UTF8Encoding(false, true);

    public static int IsWindows() => OperatingSystem.IsWindows() ? 1 : OperatingSystem.IsLinux() ? 0 :
        throw new PlatformNotSupportedException("CUI supports Windows and Linux/X11.");

    private static string Text(byte* text) => text == null ? "" :
        Utf8.GetString(MemoryMarshal.CreateReadOnlySpanFromNullTerminated(text));

    public static int ListDirectory(void* user, byte* path, cui_file_entry* output, int capacity,
        byte* names, ulong namesCapacity, CBool hidden)
    {
        bool windows = IsWindows() != 0;
        if (capacity < 0 || (capacity != 0 && output == null) || (namesCapacity != 0 && names == null)) return -1;
        try
        {
            string directory = Text(path);
            if (directory.Length == 0) directory = ".";
            var entries = new DirectoryInfo(directory).EnumerateFileSystemInfos();
            int count = 0;
            ulong used = 0;
            foreach (var entry in entries)
            {
                if (hidden == 0 && (entry.Name.StartsWith('.') ||
                    (windows && (entry.Attributes & FileAttributes.Hidden) != 0))) continue;
                if (count >= capacity) break;
                byte[] bytes = Utf8.GetBytes(entry.Name);
                if ((ulong)bytes.Length + 1 > namesCapacity - used) break;
                // Like stat in the POSIX implementation, unavailable metadata does
                // not remove an entry (for example a dangling symbolic link).
                bool isDirectory = false;
                ulong size = 0;
                long modified = 0;
                try
                {
                    // FileInfo.Length describes the link itself on some .NET
                    // hosts. CUI uses stat, so follow links for metadata only.
                    FileSystemInfo? metadata = (entry.Attributes & FileAttributes.ReparsePoint) != 0
                        ? entry.ResolveLinkTarget(true) : entry;
                    if (metadata != null && metadata.Exists)
                    {
                        isDirectory = (metadata.Attributes & FileAttributes.Directory) != 0;
                        size = isDirectory ? 0 : (ulong)((FileInfo)metadata).Length;
                        modified = new DateTimeOffset(metadata.LastWriteTimeUtc).ToUnixTimeSeconds();
                    }
                }
                catch (Exception e) when (e is IOException or UnauthorizedAccessException)
                { isDirectory = false; size = 0; modified = 0; }
                var target = names + used;
                bytes.CopyTo(new Span<byte>(target, bytes.Length));
                target[bytes.Length] = 0;
                output[count++] = new cui_file_entry
                {
                    name = target, dir = isDirectory,
                    size = size, modified = modified
                };
                used += (ulong)bytes.Length + 1;
            }
            return count;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        { return -1; }
    }

    public static CBool OpenUrl(byte* url)
    {
        bool windows = IsWindows() != 0;
        if (url == null || *url == 0) return false;
        try
        {
            string value = Text(url);
            if (windows)
                return ShellExecuteW(0, "open", value, null, null, 1) > 32;
            var start = new ProcessStartInfo("xdg-open") { UseShellExecute = false };
            // xdg-open has no portable end-of-options marker. A relative file
            // whose name starts with '-' must not become a launcher option.
            start.ArgumentList.Add(value.StartsWith('-') ? Path.GetFullPath(value) : value);
            using var process = Process.Start(start);
            if (process == null) return false;
            // xdg-open reports missing handlers and desktop failures through its
            // exit status. Starting its process alone cannot establish success.
            process.WaitForExit();
            return process.ExitCode == 0;
        }
        catch (Exception e) when (e is System.ComponentModel.Win32Exception or InvalidOperationException or IOException or ArgumentException) { return false; }
    }

    public static CBool FileDialog(cui_app* app, cui_file_dialog_opts* options, byte* output, ulong capacity)
    {
        bool windows = IsWindows() != 0;
        if (output == null || capacity == 0) return false;
        output[0] = 0;
        cui_file_dialog_opts defaults = default;
        if (options == null) options = &defaults;
        if (options->kind != cui_file_dialog_kind.CUI_FILE_OPEN &&
            options->kind != cui_file_dialog_kind.CUI_FILE_SAVE &&
            options->kind != cui_file_dialog_kind.CUI_FILE_FOLDER) return false;
        try
        {
            string? path = windows ? WindowsDialog(options) : LinuxDialog(options);
            if (string.IsNullOrEmpty(path) || path.IndexOf('\0') >= 0) return false;
            byte[] bytes = Utf8.GetBytes(path);
            if ((ulong)bytes.Length + 1 > capacity) return false;
            bytes.CopyTo(new Span<byte>(output, bytes.Length));
            output[bytes.Length] = 0;
            return true;
        }
        catch (Exception e) when (e is System.ComponentModel.Win32Exception or IOException or InvalidOperationException or ArgumentException)
        { return false; }
    }

    private static string? LinuxDialog(cui_file_dialog_opts* options)
    {
        bool save = options->kind == cui_file_dialog_kind.CUI_FILE_SAVE;
        bool folder = options->kind == cui_file_dialog_kind.CUI_FILE_FOLDER;
        string title = options->title == null ? save ? "Save" : "Open" : Text(options->title);
        string filterName = options->filter_name == null ? "Files" : Text(options->filter_name);
        string filter = string.Join(" ", Text(options->filter).Split(';', StringSplitOptions.RemoveEmptyEntries).Select(s => "*." + s));
        foreach (string tool in new[] { "zenity", "kdialog" })
        {
            var start = new ProcessStartInfo(tool) { UseShellExecute = false, RedirectStandardOutput = true,
                RedirectStandardError = true, StandardOutputEncoding = Utf8 };
            if (tool == "zenity")
            {
                start.ArgumentList.Add("--file-selection");
                start.ArgumentList.Add("--title=" + title);
                if (save) { start.ArgumentList.Add("--save"); start.ArgumentList.Add("--confirm-overwrite"); }
                if (folder) start.ArgumentList.Add("--directory");
                if (save && options->default_name != null) start.ArgumentList.Add("--filename=" + Text(options->default_name));
                if (!folder && filter.Length != 0) start.ArgumentList.Add("--file-filter=" + filterName + " | " + filter);
            }
            else
            {
                start.ArgumentList.Add(save ? "--getsavefilename" : folder ? "--getexistingdirectory" : "--getopenfilename");
                start.ArgumentList.Add(options->default_name == null ? "." : Text(options->default_name));
                if (!folder && filter.Length != 0) start.ArgumentList.Add(filter + "|" + filterName);
                start.ArgumentList.Add("--title"); start.ArgumentList.Add(title);
            }
            Process? process;
            try { process = Process.Start(start); }
            catch (System.ComponentModel.Win32Exception e) when (e.NativeErrorCode == 2) { continue; }
            if (process == null) return null;
            using (process)
            {
                var errors = process.StandardError.ReadToEndAsync();
                string result = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                errors.GetAwaiter().GetResult();
                return process.ExitCode == 0 ? result.Split('\n')[0].TrimEnd('\r') : null;
            }
        }
        Console.Error.WriteLine("cui: install zenity or kdialog for file dialogs");
        return null;
    }

    private static string? WindowsDialog(cui_file_dialog_opts* options)
    {
        string title = options->title == null ? options->kind == cui_file_dialog_kind.CUI_FILE_SAVE ? "Save" : "Open" : Text(options->title);
        char* path = stackalloc char[1040];
        new Span<char>(path, 1040).Clear();
        if (options->kind == cui_file_dialog_kind.CUI_FILE_FOLDER)
        {
            int initialized = CoInitializeEx(0, 6);
            if (initialized < 0) return null;
            try
            {
                // SHBrowseForFolder writes the selected display name into a
                // caller-owned MAX_PATH buffer, separate from the output path.
                char* displayName = stackalloc char[260];
                new Span<char>(displayName, 260).Clear();
                fixed (char* titlePointer = title)
                {
                    var info = new BrowseInfo { owner = GetActiveWindow(), displayName = displayName,
                        title = titlePointer, flags = 0x41 };
                    nint item = SHBrowseForFolderW(&info);
                    if (item == 0) return null;
                    try { return SHGetPathFromIDListW(item, path) ? new string(path) : null; }
                    finally { CoTaskMemFree(item); }
                }
            }
            finally { CoUninitialize(); }
        }
        string extensions = string.Join(";", Text(options->filter).Split(';', StringSplitOptions.RemoveEmptyEntries).Select(s => "*." + s));
        string filter = (extensions.Length == 0 ? "" : (options->filter_name == null ? "Files" : Text(options->filter_name)) + '\0' + extensions + '\0') + "All files\0*.*\0\0";
        if (options->default_name != null)
        {
            string name = Text(options->default_name);
            if (name.Length >= 1040) return null;
            name.AsSpan().CopyTo(new Span<char>(path, 1039));
        }
        fixed (char* titlePointer = title)
        fixed (char* filterPointer = filter)
        {
            bool save = options->kind == cui_file_dialog_kind.CUI_FILE_SAVE;
            var info = new OpenFileName { size = (uint)sizeof(OpenFileName), owner = GetActiveWindow(),
                filter = filterPointer, file = path, maxFile = 1040, title = titlePointer,
                flags = 0x80000u | 8u | (save ? 2u : 0x1000u) };
            bool result = save ? GetSaveFileNameW(&info) : GetOpenFileNameW(&info);
            return result ? new string(path) : null;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BrowseInfo { public nint owner, root; public char* displayName; public char* title; public uint flags; public nint callback, parameter; public int image; }
    [StructLayout(LayoutKind.Sequential)]
    private struct OpenFileName
    {
        public uint size; public nint owner, instance; public char* filter; public char* customFilter;
        public uint maxCustomFilter, filterIndex; public char* file; public uint maxFile; public char* fileTitle;
        public uint maxFileTitle; public char* initialDirectory; public char* title; public uint flags;
        public ushort fileOffset, fileExtension; public char* defaultExtension; public nint customData, hook;
        public char* templateName; public nint reserved; public uint reserved2, flagsEx;
    }
    [DllImport("user32.dll")] private static extern nint GetActiveWindow();
    [DllImport("shell32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)] private static extern nint ShellExecuteW(nint owner, string operation, string file, string? parameters, string? directory, int show);
    [DllImport("shell32.dll", ExactSpelling = true)] private static extern nint SHBrowseForFolderW(BrowseInfo* info);
    [DllImport("shell32.dll", ExactSpelling = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool SHGetPathFromIDListW(nint item, char* path);
    [DllImport("ole32.dll")] private static extern int CoInitializeEx(nint reserved, uint flags);
    [DllImport("ole32.dll")] private static extern void CoUninitialize();
    [DllImport("ole32.dll")] private static extern void CoTaskMemFree(nint memory);
    [DllImport("comdlg32.dll", ExactSpelling = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetOpenFileNameW(OpenFileName* info);
    [DllImport("comdlg32.dll", ExactSpelling = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetSaveFileNameW(OpenFileName* info);
}
