using Managed.Interpreters;

if (args.Length != 3) throw new ArgumentException("Pass the pinned fixture directory, receipt.pint and callback-return.pint paths.");
var directory = Path.GetFullPath(args[0]);
var modules = Directory.GetFiles(directory, "*.pint").ToDictionary(path => Path.GetFileName(path), File.ReadAllBytes);
modules.Add("receipt.pint", File.ReadAllBytes(args[1]));
modules.Add("callback-return.pint", File.ReadAllBytes(args[2]));
var authoredDirectory = Path.GetDirectoryName(Path.GetFullPath(args[1]))!;
modules.Add("receipt-swapped.pint", File.ReadAllBytes(Path.Combine(authoredDirectory, "receipt-swapped.pint")));
modules.Add("invalid-opcode.pint", File.ReadAllBytes(Path.Combine(authoredDirectory, "invalid-opcode.pint")));
foreach (string dependency in new[] { "require-sub-v2.pint", "require-exports-sub-v2.pint" })
    modules.Add("..\\Marius.Pinta.Test.Files\\" + dependency, modules[dependency]);
Abi.Verify();
int passed = 0;
void Check(bool condition, string label)
{
    if (!condition) throw new InvalidOperationException(label);
    Console.WriteLine($"PASS {label}");
    passed++;
}
void Throws<T>(Action action, string label) where T : Exception
{
    try { action(); }
    catch (T) { Check(true, label); return; }
    throw new InvalidOperationException($"Expected {typeof(T).Name}: {label}");
}

using (var engine = new PintaEngine(modules!))
{
    GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
    var module = engine.LoadModule("unicode-string-v2.pint");
    engine.Execute(module);
    Check(engine.GetString(module, "WORD") == "Ąžuolas", "actual Unicode fixture after CLR GC");
    Throws<InvalidOperationException>(() => engine.Execute(module), "repeated execution explicitly rejected");
    engine.SetString(module, "WORD", "A\0Ą😀");
    GC.Collect();
    Check(engine.GetString(module, "WORD") == "A\0Ą😀", "UTF16 code units, NUL and surrogate pair");
    string saved = engine.GetString(module, "WORD")!;
    engine.SetInteger(module, "WORD", 42);
    Check(engine.GetString(module, "WORD") == "42", "integer global conversion");
    Check(saved == "A\0Ą😀", "copied global survives later allocation");
    engine.SetNull(module, "WORD");
    Check(engine.GetString(module, "WORD") == null, "null global");
    using var other = new PintaEngine(modules!);
    Throws<ArgumentException>(() => other.Execute(module), "cross-engine handle rejection");
    Throws<InvalidOperationException>(() => engine.LoadModule("missing.pint"), "missing module");
    engine.Dispose();
    Throws<ObjectDisposedException>(() => engine.Execute(module), "disposed handle rejection");
}
for (int iteration = 0; iteration < 8; iteration++)
{
    using var engine = new PintaEngine(modules!);
    var module = engine.LoadModule("unicode-string-v2.pint");
    engine.Execute(module);
    Check(engine.GetString(module, "WORD") == "Ąžuolas", $"creation/disposal {iteration}");
}
using (var engine = new PintaEngine(modules))
{
    var module = engine.LoadModule("receipt.pint");
    engine.SetString(module, "customer", "Ada");
    engine.SetInteger(module, "quantity", 3);
    engine.SetString(module, "unitPrice", "12.50");
    engine.Execute(module);
    const string expected = "Customer: Ada\nTotal: 37.5\n";
    Check(engine.GetString(module, "total") == "37.5", "receipt fixed-point total");
    Check(engine.CopyOutput().SequenceEqual(System.Text.Encoding.Unicode.GetBytes(expected)), "receipt exact output bytes");
    Check(engine.GetOutputString() == expected, "receipt decoded output");
}
using (var engine = new PintaEngine(modules))
{
    var order = new List<int>();
    engine.RegisterFunction(0, (ref PintaCallContext call) =>
    {
        order.Add(0);
        Check(call.ArgumentCount == 1 && call.GetInteger(0) == 6, "integer application callback");
        GC.Collect(); GC.WaitForPendingFinalizers();
        Check(call.GetInteger(0) == 6, "callback arguments survive CLR collection");
        Throws<InvalidOperationException>(() => engine.CopyOutput(), "callback engine reentry rejection");
        call.ReturnString(null);
    });
    engine.RegisterFunction(1, (ref PintaCallContext call) =>
    {
        order.Add(1);
        Check(call.ArgumentCount == 2 && call.GetString(0) == "Well, hello" && call.GetString(1) == "Hey there",
            "string application callback");
        call.ReturnString("Ą😀");
        call.Collect();
        Check(call.GetString(0) == "Well, hello", "callback argument roots during Pinta compaction");
    });
    engine.RegisterFunction(2, (ref PintaCallContext call) =>
    {
        order.Add(2);
        Check(call.ArgumentCount == 1 && call.GetString(0) == "314.15", "fixed-point callback conversion");
        call.ReturnInteger(42);
    });
    engine.Execute(engine.LoadModule("simple-internal-function.pint"));
    Check(order.SequenceEqual(new[] { 0, 1, 2 }), "application callback order");
}
using (var engine = new PintaEngine(modules))
{
    engine.RegisterFunction(0, (ref PintaCallContext call) => throw new InvalidOperationException("callback failure"));
    var module = engine.LoadModule("simple-internal-function.pint");
    try { engine.Execute(module); throw new InvalidOperationException("Callback error was lost."); }
    catch (PintaException exception) { Check(exception.Status == 11, "callback exception maps to Pinta engine status"); }
}
Parallel.For(0, 4, index =>
{
    using var engine = new PintaEngine(modules!);
    var module = engine.LoadModule("unicode-string-v2.pint");
    engine.Execute(module);
    if (engine.GetString(module, "WORD") != "Ąžuolas") throw new InvalidOperationException($"Independent engine {index}");
});
Check(true, "independent engines in parallel");
Throws<ArgumentOutOfRangeException>(() => new PintaEngine(modules!, 512), "invalid arena budget");
Throws<OutOfMemoryException>(() => new PintaEngine(modules, 2048, 1024, 128), "actual creation failure cleanup");
using (var engine = new PintaEngine(modules))
    Throws<InvalidOperationException>(() => engine.LoadModule("simple-receipt-script.pint"), "empty upstream fixture rejected");
var provided = new Dictionary<string, byte[]> { ["copy.pint"] = (byte[])modules["unicode-string-v2.pint"].Clone() };
using (var engine = new PintaEngine(provided))
{
    Array.Clear(provided["copy.pint"]);
    var module = engine.LoadModule("copy.pint");
    engine.Execute(module);
    Check(engine.GetString(module, "WORD") == "Ąžuolas", "caller bytes copied before mutation");
}
using (var engine = new PintaEngine(modules, 512 * 1024, 16 * 1024, 16 * 1024))
{
    var module = engine.LoadModule("unicode-string-v2.pint");
    engine.Execute(module);
    string value = "";
    for (int index = 0; index < 512; index++)
    {
        value = $"{index}:Ą😀\0" + new string('x', 256);
        engine.SetString(module, "WORD", value);
        if (index % 16 == 0) { GC.Collect(); engine.Collect(); }
    }
    engine.Collect();
    Check(engine.GetString(module, "WORD") == value, "bounded heap and combined Pinta/CLR GC stress");
}
Check(ResolverProbes.Run(modules["unicode-string-v2.pint"], false), "raw resolver three-byte reads and close");
Check(ResolverProbes.Run(modules["unicode-string-v2.pint"], true), "raw resolver early EOF closes failed load");
using (var engine = new PintaEngine(modules))
{
    engine.RegisterFunction(1, (ref PintaCallContext call) =>
    {
        Check(call.ArgumentCount == 0, "return-value fixture callback arguments");
        call.ReturnString(new string('x', 64));
        call.ReturnString("Ą😀");
        call.Collect();
        GC.Collect();
    });
    var module = engine.LoadModule("callback-return.pint");
    engine.Execute(module);
    Check(engine.GetString(module, "answer") == "Ą😀", "callback return value survives compaction into module global");
}
Check(ValueExhaustionProbes.Run(0), "rooted heap exhaustion returns string OOM status");
Check(ValueExhaustionProbes.Run(1), "rooted heap exhaustion returns character OOM status");
Check(ValueExhaustionProbes.Run(2), "rooted heap exhaustion returns weak-reference OOM status");
// Additional malformed-module probes are authored in ModuleProbes.cs but remain
// outside this qualified consumer suite until their separate validation resumes.
var swappedBefore = (byte[])modules["receipt-swapped.pint"].Clone();
using (var engine = new PintaEngine(modules))
{
    var module = engine.LoadModule("receipt-swapped.pint");
    engine.SetString(module, "customer", "Ada");
    engine.SetInteger(module, "quantity", 3);
    engine.SetString(module, "unitPrice", "12.50");
    engine.Execute(module);
    Check(engine.GetString(module, "total") == "37.5" && engine.GetOutputString() == "Customer: Ada\nTotal: 37.5\n",
        "swapped-endian module executes with exact receipt result");
    Check(modules["receipt-swapped.pint"].SequenceEqual(swappedBefore), "swapped module source bytes remain immutable");
}
foreach (string name in new[] { "require-v2.pint", "require-exports-v2.pint" })
{
    using var engine = new PintaEngine(modules);
    int assertions = 0;
    engine.RegisterFunction(0, (ref PintaCallContext call) =>
    {
        if (call.ArgumentCount < 2 || call.GetString(0) != call.GetString(1))
            throw new PintaException((uint)Pinta.PintaException.PINTA_EXCEPTION_ENGINE);
        assertions++;
    });
    engine.Execute(engine.LoadModule(name));
    Check(assertions > 0, $"owning module resolver executes {name} assertions");
}
using (var engine = new PintaEngine(new Dictionary<string, byte[]> { ["main"] = modules["require-v2.pint"] }))
{
    try { engine.Execute(engine.LoadModule("main")); throw new InvalidOperationException("Missing import was accepted."); }
    catch (PintaException error)
    { Check(error.Status == (uint)Pinta.PintaException.PINTA_EXCEPTION_FILE_NOT_FOUND, "missing imported module preserves native status"); }
}
Console.WriteLine($"PASS total={passed}");
