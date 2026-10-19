using System.Text.Json;
using Managed.CPython;
using VM = Managed.Interpreters.CPython;

if (args.Length != 2) throw new ArgumentException("stdlib selected-native-builtin-names.json");
var owner = VM.Libc.CaptureRuntimeContext();
unsafe
{
    fixed (byte* name = "C.UTF-8\0"u8)
        if (VM.Libc.setlocale(0, name) == null)
            throw new InvalidOperationException("Consumer locale unavailable");
}
if (!ReferenceEquals(owner, VM.Libc.CaptureRuntimeContext()))
    throw new InvalidOperationException("Program owner changed before preinitialization");
using var runtime = PythonRuntime.Initialize(new PythonOptions(args[0], args[0])
{
    WriteBytecode = true,
    Utf8Mode = false,
});
runtime.Execute("import sys\nassert sys.flags.utf8_mode == 0 and not sys.dont_write_bytecode\nassert sys.flags.isolated and sys.flags.ignore_environment\n", "builtin_startup_r41.py");
using var names = JsonDocument.Parse(File.ReadAllText(args[1]));
int count = 0;
foreach (var element in names.RootElement.EnumerateArray())
{
    using var module = runtime.Import(element.GetString()!);
    count++;
}
if (count != 75) throw new InvalidOperationException("Incomplete selected builtin inventory");
Console.WriteLine($"PASS actual selected builtins={count} isolated=1 utf8_mode=0 bytecode=1");
