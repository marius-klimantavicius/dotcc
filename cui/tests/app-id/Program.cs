using System.Diagnostics;
using Cui.Managed;

if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("This test observes actual X11 WM_CLASS; Windows execution remains unrun.");
Environment.SetEnvironmentVariable("RESOURCE_NAME", null);
int assertions = 0;
void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
    assertions++;
}
string Command(string name, params string[] args)
{
    var start = new ProcessStartInfo(name) { RedirectStandardOutput = true, RedirectStandardError = true };
    foreach (string arg in args) start.ArgumentList.Add(arg);
    using var process = Process.Start(start)!;
    string output = process.StandardOutput.ReadToEnd();
    string error = process.StandardError.ReadToEnd();
    Check(process.WaitForExit(10000) && process.ExitCode == 0, name + ": " + error);
    return output.Trim();
}
try
{
    using var invalid = new Application(new() { AppId = "org.example\0truncated", Hidden = true });
    throw new InvalidOperationException("NUL application ID accepted");
}
catch (ArgumentException) { assertions++; }

string?[] ids = ["org.example.CuiSafe.First", "org.example.CuiSafe.Second", "", null];
for (int i = 0; i < ids.Length; i++)
{
    string title = "CUI safe application ID " + i;
    using var app = new Application(new() { Title = title, AppId = ids[i], Width = 320, Height = 200, Hidden = true });
    GC.Collect(2, GCCollectionMode.Forced, true, true);
    GC.WaitForPendingFinalizers();
    Check(app.NextFrame(), "App ID creation stays live after compacting GC");
    app.Ui.Text("Application ID lifecycle");
    app.Render();
    string window = Command("xdotool", "search", "--name", "^" + title + "$");
    Check(!window.Contains('\n'), "Exactly one app window");
    string value = Command("xprop", "-id", window, "WM_CLASS");
    string expected = string.IsNullOrEmpty(ids[i]) ? title : ids[i]!;
    Check(value.EndsWith("\"" + expected + "\", \"" + expected + "\"", StringComparison.Ordinal), "Safe application forwards/resets actual X11 WM_CLASS: " + value);
    Console.WriteLine("APP_ID\t" + i + "\t" + value);
    app.Dispose();
    app.Dispose();
    try { app.NextFrame(); throw new Exception("Disposed application reused"); }
    catch (ObjectDisposedException) { assertions++; }
}
Console.WriteLine($"PASS safe AppId lifecycle: {assertions} assertions; Windows unrun");
