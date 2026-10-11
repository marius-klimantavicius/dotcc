namespace DotObj;

internal static class Program
{
    public static int Main(string[] args)
    {
        var objects = new List<ObjectFile>();
        try
        {
            if (args.Length == 0 || args[0] is not ("link" or "verify" or "catalog" or "catalog-maps" or "catalog-generate" or "inspect-edg-graph")) throw new ArgumentException("Usage: dotobj-link link --out DIRECTORY [--namespace NAME] [--class NAME] [--entry SYMBOL] [--exception-provider shared|external] [--provider LIBRARY.dotobj] OBJECT.dotobj... | verify OBJECT.dotobj... | catalog [--resolve] --out CATALOG.sqlite [--provider OBJECT.dotobj] OBJECT.dotobj... | catalog-maps --out DIRECTORY CATALOG.sqlite | catalog-generate --out DIRECTORY CATALOG.sqlite | inspect-edg-graph [--out FILE.jsonl] OBJECT.dotobj");
            string command = args[0], namespaceName = "Translated", className = "Program";
            string? output = null, entry = null;
            bool allowExterns = false, auditCatalogLifetimes = false, resolveCatalog = false;
            bool sharedExceptionProvider = true;
            var paths = new List<string>();
            var providerPaths = new HashSet<string>(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
            for (int index = 1; index < args.Length; index++)
            {
                string argument = args[index];
                if (argument == "--allow-externs") { allowExterns = true; continue; }
                if (argument == "--audit-object-lifetimes") { auditCatalogLifetimes = true; continue; }
                if (argument == "--resolve") { resolveCatalog = true; continue; }
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
                        case "--exception-provider":
                            sharedExceptionProvider = value switch { "shared" => true, "external" => false,
                                _ => throw new ArgumentException("--exception-provider must be shared or external") }; break;
                        case "--provider": providerPaths.Add(Path.GetFullPath(value)); paths.Add(value); break;
                        default: throw new ArgumentException("Unknown option " + argument);
                    }
                }
                else paths.Add(argument);
            }
            if (paths.Count == 0) throw new ArgumentException("No semantic objects supplied");
            if (auditCatalogLifetimes && command is not("catalog" or "catalog-generate")) throw new ArgumentException("--audit-object-lifetimes requires catalog or catalog-generate");
            if (resolveCatalog && command != "catalog") throw new ArgumentException("--resolve requires catalog");
            if (command == "verify" && providerPaths.Count != 0) throw new ArgumentException("--provider is a link option; verify checks complete objects");
            if (command == "link" && paths.All(path => providerPaths.Contains(Path.GetFullPath(path)))) throw new ArgumentException("At least one primary semantic object is required");
            if (command is "link" or "catalog" or "catalog-maps" or "catalog-generate" && output is null) throw new ArgumentException(command + " requires --out");
            _ = Names.Namespace(namespaceName); _ = Names.OptionIdentifier(className);
            if (command == "inspect-edg-graph")
            {
                if (paths.Count != 1 || providerPaths.Count != 0)
                    throw new ArgumentException("inspect-edg-graph requires exactly one semantic object");
                using var obj = new ObjectFile(paths[0]);
                using Stream stream = new BufferedStream(output is null ? Console.OpenStandardOutput()
                    : new FileStream(output, FileMode.CreateNew, FileAccess.Write, FileShare.Read), 65536);
                using var writer = new System.Text.Json.Utf8JsonWriter(stream);
                foreach (var field in obj.ReadEdgGraphFields())
                {
                    field.Write(writer); writer.Flush(); stream.WriteByte((byte)'\n'); writer.Reset(stream);
                }
            }
            else if(command=="catalog-generate")
            {
                if(paths.Count!=1 || providerPaths.Count!=0)throw new ArgumentException("catalog-generate requires exactly one resolved catalog");
                CatalogGenerator.Write(paths[0],Path.GetFullPath(output!),namespaceName,className,entry,sharedExceptionProvider,
                    auditCatalogLifetimes?reference=>{
                        GC.Collect();GC.WaitForPendingFinalizers();GC.Collect();
                        if(reference.IsAlive)throw new InvalidOperationException("A generated catalog unit is still strongly retained");
                    }:null);
                Console.WriteLine("Generated C# from the resolved catalog with bounded unit loading.");
            }
            else if (command == "catalog-maps")
            {
                if(paths.Count!=1 || providerPaths.Count!=0)throw new ArgumentException("catalog-maps requires exactly one catalog");
                using var units=new CatalogUnitReader(paths[0]);
                units.WriteMaps(Path.GetFullPath(output!));
                Console.WriteLine("Wrote retained source and macro maps; no C# was generated.");
            }
            else if (command == "catalog")
            {
                int released = 0;
                var timer = System.Diagnostics.Stopwatch.StartNew();
                CatalogImporter.Import(output!, paths.Select(path => new CatalogInput(path, providerPaths.Contains(Path.GetFullPath(path)))),
                    unit => Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(new {
                        status = "unit-imported", unit, elapsedMilliseconds = timer.ElapsedMilliseconds,
                        managedBytes = GC.GetTotalMemory(false), peakWorkingSet = System.Diagnostics.Process.GetCurrentProcess().PeakWorkingSet64,
                        auditedReleasedObjects = released })),
                    auditCatalogLifetimes ? reference => {
                        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
                        if (reference.IsAlive) throw new InvalidOperationException("A completed catalog unit is still strongly retained");
                        released++;
                    } : null);
                if (resolveCatalog)
                {
                    using var resolver = new CatalogResolver(output!);
                    resolver.Resolve(className,allowExterns,sharedExceptionProvider);
                }
                Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(new {
                    status = resolveCatalog ? "selected-not-generated" : "imported-not-resolved", units = paths.Count,
                    elapsedMilliseconds = timer.ElapsedMilliseconds,
                    peakWorkingSet = System.Diagnostics.Process.GetCurrentProcess().PeakWorkingSet64 }));
            }
            else if (command == "verify")
            {
                // Verification has no cross-unit state. Release each object's
                // metadata and SQLite snapshot before opening the next unit.
                foreach (string path in paths) VerifyObject(path);
                Console.WriteLine($"Verified {paths.Count} scalar-control-v65 semantic object(s).");
            }
            else
            {
                foreach (string path in paths) objects.Add(new ObjectFile(path));
                var providers = objects.Where(obj => providerPaths.Contains(Path.GetFullPath(obj.Path))).ToHashSet();
                var linker = new Linker(objects, className, allowExterns, providers, sharedExceptionProvider);
                linker.VerifyBodies();
                new Emitter(linker, namespaceName, className, sharedExceptionProvider).Write(Path.GetFullPath(output!), entry);
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

    private static void VerifyObject(string path)
    {
        using var obj = new ObjectFile(path);
        foreach (long function in obj.Functions.Keys) _ = obj.ReadBody(function);
    }
}
