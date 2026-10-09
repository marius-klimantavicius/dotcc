namespace DotObj;

internal static class Program
{
    public static int Main(string[] args)
    {
        var objects = new List<ObjectFile>();
        try
        {
            if (args.Length == 0 || args[0] is not ("link" or "verify")) throw new ArgumentException("Usage: dotobj-link link --out DIRECTORY [--namespace NAME] [--class NAME] [--entry SYMBOL] [--provider LIBRARY.dotobj] OBJECT.dotobj... | verify OBJECT.dotobj...");
            string command = args[0], namespaceName = "Translated", className = "Program";
            string? output = null, entry = null;
            bool allowExterns = false;
            var paths = new List<string>();
            var providerPaths = new HashSet<string>(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
            for (int index = 1; index < args.Length; index++)
            {
                string argument = args[index];
                if (argument == "--allow-externs") { allowExterns = true; continue; }
                if (argument.StartsWith("--", StringComparison.Ordinal))
                {
                    if (index + 1 == args.Length) throw new ArgumentException("Missing value for " + argument);
                    string value = args[++index];
                    switch (argument)
                    {
                        case "--out": output = value; break;
                        case "--namespace": namespaceName = value; break;
                        case "--class": className = value; break;
                        case "--entry": entry = value; break;
                        case "--provider": providerPaths.Add(Path.GetFullPath(value)); paths.Add(value); break;
                        default: throw new ArgumentException("Unknown option " + argument);
                    }
                }
                else paths.Add(argument);
            }
            if (paths.Count == 0) throw new ArgumentException("No semantic objects supplied");
            if (command == "verify" && providerPaths.Count != 0) throw new ArgumentException("--provider is a link option; verify checks complete objects");
            if (command == "link" && paths.All(path => providerPaths.Contains(Path.GetFullPath(path)))) throw new ArgumentException("At least one primary semantic object is required");
            if (command == "link" && output is null) throw new ArgumentException("link requires --out DIRECTORY");
            _ = Names.Namespace(namespaceName); _ = Names.OptionIdentifier(className);
            foreach (string path in paths) objects.Add(new ObjectFile(path));
            if (command == "verify")
            {
                foreach (ObjectFile obj in objects)
                    foreach (long function in obj.Functions.Keys) _ = obj.ReadBody(function);
                Console.WriteLine($"Verified {objects.Count} scalar-control-v30 semantic object(s).");
            }
            else
            {
                var providers = objects.Where(obj => providerPaths.Contains(Path.GetFullPath(obj.Path))).ToHashSet();
                var linker = new Linker(objects, className, allowExterns, providers);
                linker.VerifyBodies();
                new Emitter(linker, namespaceName, className).Write(Path.GetFullPath(output!), entry);
                linker.WriteSelection(Path.GetFullPath(output!));
                Console.WriteLine($"Linked {objects.Count} object(s), {linker.Definitions.Count} function definition(s) into {Path.GetFullPath(output!)}");
            }
            return 0;
        }
        catch (Exception error) when (error is ArgumentException or InvalidDataException or IOException or DllNotFoundException or KeyNotFoundException or InvalidOperationException)
        {
            Console.Error.WriteLine("dotobj-link: " + error.Message);
            return 1;
        }
        finally { foreach (ObjectFile obj in objects) obj.Dispose(); }
    }
}
