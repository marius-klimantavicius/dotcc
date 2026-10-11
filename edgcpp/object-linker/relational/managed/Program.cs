namespace DotObj.Relational;

internal static class Program
{
    public static int Main(string[] args)
    {
        try
        {
            if (args.Length == 0 || args[0] != "link")
                throw new ArgumentException("Usage: dotobj-relational link --out DIRECTORY [--namespace NAME] [--class NAME] [--entry SYMBOL] [--unresolved-providers required-partial] OBJECT.dotobj...");
            string? output = null, entry = null;
            string namespaceName = "Translated", className = "Program";
            bool requiredProviders = false;
            var paths = new List<string>();
            for (int index = 1; index < args.Length; index++)
            {
                string argument = args[index];
                if (!argument.StartsWith("--", StringComparison.Ordinal)) { paths.Add(argument); continue; }
                if (++index == args.Length) throw new ArgumentException("Missing value for " + argument);
                string value = args[index];
                switch (argument)
                {
                    case "--out": output = value; break;
                    case "--namespace": namespaceName = value; break;
                    case "--class": className = value; break;
                    case "--entry": entry = value; break;
                    case "--unresolved-providers":
                        requiredProviders = value == "required-partial" ? true :
                            throw new ArgumentException("Unknown unresolved provider mode " + value);
                        break;
                    default: throw new ArgumentException("Unknown option " + argument);
                }
            }
            if (output is null || paths.Count == 0) throw new ArgumentException("An output directory and at least one object are required");
            namespaceName = Names.Namespace(namespaceName);
            className = Names.OptionIdentifier(className);
            using var program = new LinkedProgram(paths);
            new Emitter(program, namespaceName, className, requiredProviders).Write(Path.GetFullPath(output), entry);
            Console.WriteLine($"Generated C# from {paths.Count} relational object(s).");
            Console.WriteLine($"Body loading: peak rows {program.PeakFunctionBodyRows}; peak nodes {program.PeakFunctionBodyNodes}; functions loaded {program.FunctionBodyLoads}; active bodies {program.LoadedFunctionBodies}; peak bodies {program.PeakLoadedFunctionBodies}");
            Console.WriteLine($"Retained catalogs: types {program.Units.Sum(unit=>unit.Types.Count)}; symbols {program.Units.Sum(unit=>unit.Symbols.Count)}; virtual tables {program.Units.Sum(unit=>unit.VirtualTables.Count)}; virtual entries {program.Units.Sum(unit=>unit.VirtualEntries.Values.Sum(entries=>entries.Count))}; literal objects {program.Units.Sum(unit=>unit.Literals.Count)}; global nodes {program.Units.Sum(unit=>unit.GlobalExpressionNodes)}. Catalog loading is not yet bounded; literal bytes are streamed.");
            return 0;
        }
        catch (Exception error) when (error is ArgumentException or InvalidDataException or IOException or DllNotFoundException or KeyNotFoundException or InvalidOperationException)
        {
            Console.Error.WriteLine("dotobj-relational: " + error.Message);
            return 1;
        }
    }
}
