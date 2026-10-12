namespace DotObj.Relational;

internal static class Program
{
    public static int Main(string[] args)
    {
        try
        {
            if (args.Length == 0 || args[0] != "link")
                throw new ArgumentException("Usage: dotobj-relational link --out DIRECTORY [--namespace NAME] [--class NAME] [--entry SYMBOL] [--emission-selection native-roots|all-bodies] [--unresolved-providers required-partial] OBJECT.dotobj...");
            string? output = null, entry = null;
            string namespaceName = "Translated", className = "Program";
            bool requiredProviders = false;
            string emissionSelection = "native-roots";
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
                    case "--emission-selection":
                        emissionSelection=value is "native-roots" or "all-bodies"?value:
                            throw new ArgumentException("Unknown emission selection " + value);
                        break;
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
            program.SelectEmission(emissionSelection,entry);
            new Emitter(program, namespaceName, className, requiredProviders).Write(Path.GetFullPath(output), entry);
            Console.WriteLine($"Generated C# from {paths.Count} relational object(s).");
            Console.WriteLine($"Emission selection: {program.EmissionSelection}; selected functions {program.Functions.Count} of {program.CapturedFunctionDefinitions}; selected globals {program.Globals.Count} of {program.CapturedGlobalDefinitions}; conservative unknown roots {program.ConservativeEmissionRoots}");
            Console.WriteLine($"Body loading: peak rows {program.PeakFunctionBodyRows}; peak nodes {program.PeakFunctionBodyNodes}; functions loaded {program.FunctionBodyLoads}; active bodies {program.LoadedFunctionBodies}; peak bodies {program.PeakLoadedFunctionBodies}");
            Console.WriteLine($"Object catalog rows: types {program.Units.Sum(unit=>unit.Types.Count)}; symbols {program.Units.Sum(unit=>unit.Symbols.Count)}; virtual tables {program.Units.Sum(unit=>unit.VirtualTables.Count)}; virtual entries {program.Units.Sum(unit=>unit.VirtualEntries.Values.Sum(entries=>entries.Count))}; literal objects {program.Units.Sum(unit=>unit.Literals.Count)}; global nodes {program.Units.Sum(unit=>unit.GlobalExpressionNodes)}. Connections and remaining planning catalogs are not yet bounded; literal bytes are streamed.");
            Console.WriteLine($"Cached primary rows: types {program.Units.Sum(unit=>unit.Types.CachedRows)}; symbols {program.Units.Sum(unit=>unit.Symbols.CachedRows)}; fields {program.Units.Sum(unit=>unit.Fields.CachedRows)}; process cap 4096 per row family; enumeration does not populate caches");
            Console.WriteLine($"Object connections: open {ObjectDatabase.OpenConnections}; peak {ObjectDatabase.PeakConnections}; cap {ObjectDatabase.ConnectionLimit}; SQLite page cache cap 8192 KiB per connection");
            return 0;
        }
        catch (Exception error) when (error is ArgumentException or InvalidDataException or IOException or DllNotFoundException or KeyNotFoundException or InvalidOperationException)
        {
            Console.Error.WriteLine("dotobj-relational: " + error.Message);
            return 1;
        }
    }
}
