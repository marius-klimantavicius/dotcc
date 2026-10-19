using Managed.CPython;
using VM = Managed.Interpreters.CPython;

if (args.Length != 2) throw new ArgumentException("Expected getcwd/frozen and explicit stdlib.");
if (args[0] == "getcwd")
{
    string original = Directory.GetCurrentDirectory();
    string temporary = Path.Combine(Path.GetTempPath(), "dotcc-b28-cwd-" + Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(temporary);
    try
    {
        Directory.SetCurrentDirectory(temporary);
        Directory.Delete(temporary);
        unsafe
        {
            byte* buffer = stackalloc byte[4096];
            VM.Libc.errno = 123;
            try
            {
                byte* result = VM.Libc.getcwd(buffer, 4096);
                Console.WriteLine($"deleted-cwd null={result == null} errno={VM.Libc.errno}");
                return result == null && VM.Libc.errno == 2 ? 0 : 26;
            }
            catch (Exception error)
            {
                Console.Error.WriteLine("ACTUAL getcwd escaped " + error.GetType().FullName + ": " + error.Message);
                return 27;
            }
        }
    }
    finally { Directory.SetCurrentDirectory(original); }
}
if (args[0] == "frozen")
{
    using var runtime = PythonRuntime.Initialize(new PythonOptions(args[1], AppContext.BaseDirectory));
    try
    {
        var result = runtime.Execute("""
            import sys, os, _imp, importlib.machinery, json
            _imp._override_frozen_modules_for_tests(1)
            spec = importlib.machinery.FrozenImporter.find_spec('__hello__')
            expected = os.path.join(sys.path[0], '__hello__.py')
            print(json.dumps({'stdlib_dir': sys._stdlib_dir, 'filename': spec.loader_state.filename, 'expected': expected, 'utf8_mode': sys.flags.utf8_mode}), flush=True)
            assert spec.loader_state.filename == expected
            """, "b28_frozen_config.py");
        Console.WriteLine(result.StandardOutput);
        return 0;
    }
    catch (PythonException error)
    {
        Console.WriteLine(error.StandardOutput);
        Console.Error.WriteLine(error.PythonTraceback);
        return 24;
    }
}
throw new ArgumentException("Unknown reduction.");
