using Managed.CPython;
using VM = Managed.Interpreters.CPython;

if (args.Length != 2) return 20;
// The same embedded Libc owner and nullable binding span all Python generations.
// Actual execution still goes through the production captured private executor.
using var owner = new VM.Libc.RuntimeContext();
using var binding = owner.Enter();
for (int cycle = 0; cycle < 6; cycle++)
{
    var options = cycle is 0 or 5 ? new PythonOptions(args[0], args[1]) :
        new PythonOptions(args[0], args[1])
        {
            WriteBytecode = (cycle - 1) / 2 != 0,
            Utf8Mode = (cycle - 1) % 2 != 0
        };
    using var runtime = PythonRuntime.Initialize(options);
    runtime.RegisterInt64Callback("host", "double", value => checked(value * 2));
    string script = $$"""
        import sys, os, importlib.util, host
        assert sys.flags.isolated == 1 and sys.flags.ignore_environment == 1
        assert sys.flags.utf8_mode == {{(options.Utf8Mode ? 1 : 0)}}
        assert sys.dont_write_bytecode is {{(options.WriteBytecode ? "False" : "True")}}
        assert host.double(21) == 42
        module = __import__('startup_fixture_{{cycle}}')
        assert module.VALUE == 37
        cache = importlib.util.cache_from_source(module.__file__)
        assert os.path.exists(cache) is {{(options.WriteBytecode ? "True" : "False")}}
        """;
    runtime.Execute(script, "owning_startup_options_r37.py");
    using var value = runtime.Evaluate("40 + 2");
    if (value.AsInt64() != 42) return 21;
    Console.WriteLine($"startup cycle={cycle} write={(options.WriteBytecode ? 1 : 0)} utf8={(options.Utf8Mode ? 1 : 0)} cache=verified");
}
Console.WriteLine("owning configured/default startup generations=6 callbacks=6 cache=verified");
return 0;
