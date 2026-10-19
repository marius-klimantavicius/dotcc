using Managed.CPython;

// A separate diagnostic consumer of the exact already-executed RAW PE.
// No product rebuild, source repair or alternative interpreter.
if (args.Length != 3) throw new ArgumentException("Expected actual stdlib, application directory and diagnostic script.");
try
{
    using var runtime = PythonRuntime.Initialize(new PythonOptions(args[0], args[1]));
    var result = runtime.Execute(File.ReadAllText(args[2]), args[2]);
    Console.Write(result.StandardOutput);
    Console.Error.Write(result.StandardError);
    return 0;
}
catch (PythonException error)
{
    Console.Write(error.StandardOutput);
    Console.Error.Write(error.StandardError);
    Console.Error.WriteLine(error.PythonType + ": " + error.Message);
    Console.Error.WriteLine(error.PythonTraceback);
    return 2;
}
