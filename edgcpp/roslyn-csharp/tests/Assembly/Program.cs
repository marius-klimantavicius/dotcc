using System.Text.Json;
using RoslynObj;
using RoslynObj.CSharp;

if (args.Length != 2) throw new ArgumentException("Usage: Assembly FIXTURE_OBJECT_DIRECTORY REPORT.json");
var directory = Path.GetFullPath(args[0]);
var assembler = new ProgramAssembler();
var options = new ProgramAssemblyOptions { MaximumOpenUnits = 2 };
using var a = Open("unit-a"); using var b = Open("unit-b");
var result = assembler.Assemble([a, b], options);
Require(result.Success, Explain(result));
var program = result.Program!;
var addDeclaration = a.TranslationUnit.Functions.Single(f => f.Name == "shared_add");
var addDefinition = b.TranslationUnit.Functions.Single(f => f.Name == "shared_add");
Require(program.Resolve(addDeclaration)?.Definition == addDefinition, "external function declaration must select the actual other-unit strong definition");
var renamed = a.TranslationUnit.Functions.Single(f => f.Name == "source_renamed");
Require(program.Resolve(renamed) is { Definition.Name: "differently_named", ExternalName.Origin: "explicit-assembly-name" },
    "different source names must bind through the actual recorded GNU assembly name");
Require(program.Resolve(a.TranslationUnit.Variables.Single(v => v.Name == "shared_value"))?.Definition
    == b.TranslationUnit.Variables.Single(v => v.Name == "shared_value"), "external data must share selected storage identity");
var localA = a.TranslationUnit.Functions.Single(f => f.Name == "local_helper");
var localB = b.TranslationUnit.Functions.Single(f => f.Name == "local_helper");
Require(program.Resolve(localA) != program.Resolve(localB), "identical internal function source names must remain distinct");
Require(program.Storage.Count(s => s.Definition.Name == "local_value") == 2, "identical internal data source names must remain distinct");
var comparison = new AbiCompatibility().CompareFunctions(addDeclaration, addDefinition);
Require(comparison.Compatible, "top-level parameter qualification must not become an ABI mismatch");
var pointerA = a.TranslationUnit.Variables.Single(v => v.Name == "shared_pointer");
var pointerB = b.TranslationUnit.Variables.Single(v => v.Name == "shared_pointer");
Require(new AbiCompatibility().CompareStorage(pointerA, pointerB).Compatible, "opaque-vs-complete pointee views must preserve pointer ABI compatibility");
Require(Pointee(pointerA.Type).IsIncomplete && !Pointee(pointerB.Type).IsIncomplete,
    "genuine C fixture must activate incomplete-vs-complete pointee declarations");
var repeated = assembler.Assemble([a, b], options);
Require(JsonSerializer.Serialize(result.Report, ProgramManifest.JsonOptions) == JsonSerializer.Serialize(repeated.Report, ProgramManifest.JsonOptions), "assembly report must repeat deterministically");
Require(!assembler.Assemble([a, b], options with { MaximumOpenUnits = 1 }).Success, "explicit open-unit bound must be honored");
Require(!assembler.Assemble([a, a], options).Success, "repeated artifact identities must fail by default");
a.Dispose(); b.Dispose(); // Later independent cases also keep at most two reader units open.
using (var da = Open("duplicate-a")) using (var db = Open("duplicate-b"))
    Expect(assembler.Assemble([da, db], options), "ROC2012", "duplicate strong definitions");
using (var ca = Open("category-a")) using (var cb = Open("category-b"))
    Expect(assembler.Assemble([ca, cb], options), "ROC2016", "function/data external-name collision");
using (var unresolved = Open("unresolved"))
    Expect(assembler.Assemble([unresolved], options), "ROC2013", "required unresolved function");
using (var abiA = Open("abi-a")) using (var abiB = Open("abi-b"))
    Expect(assembler.Assemble([abiA, abiB], options), "ROC2014", "selected parameter-width representation mismatch");
foreach (var name in new[] { "weak", "common", "alias", "ifunc" })
    using (var obj = Open(name))
    {
        if (name == "alias") Require(obj.TranslationUnit.Functions.Any(f => f.Details.IsGnuAlias == true && f.GnuSupplement?.AliasedRoutine is not null),
            "genuine alias fixture must retain the actual GNU alias target relationship");
        if (name == "ifunc") Require(obj.TranslationUnit.Functions.Any(f => f.Details.IsIfunc == true && f.IfuncResolver is not null),
            "genuine ifunc fixture must retain the actual resolver relationship");
        Expect(assembler.Assemble([obj], options), "ROC2010", "unsupported " + name + " selection");
    }
using (var comdat = Open("comdat"))
{
    var recorded = comdat.TranslationUnit.Functions.Where(f => f.BodyState.Name == "definition").ToArray();
    Require(recorded.Any(f => f.Details.IsUseComdat == true || f.Details.IsInline),
        "genuine concrete template fixture must activate retained COMDAT or inline selection");
    Expect(assembler.Assemble([comdat], options), "ROC2010", "unsupported COMDAT/inline template selection");
    Console.WriteLine("Concrete template selection activated: COMDAT=" + recorded.Any(f => f.Details.IsUseComdat == true)
        + ", inline=" + recorded.Any(f => f.Details.IsInline));
}
using var cppA = Open("names-a"); using var cppB = Open("names-b");
var cpp = assembler.Assemble([cppA, cppB], options);
Require(cpp.Success, Explain(cpp));
var overloads = cpp.Program!.Functions.Where(f => f.Definition.Name == "overloaded").ToArray();
Require(overloads.Length == 2 && overloads.Select(f => f.ExternalName!.Name).Distinct(StringComparer.Ordinal).Count() == 2,
    "EDG's two final overloaded names must select distinct functions without source lookup");
Require(overloads.All(f => f.ExternalName is { Origin: "captured-mangled-current-name", NameHasBeenMangled: true }),
    "C++ external identity must come from actual captured mangled CurrentName");
Require(cpp.Program.Functions.Single(f => f.Definition.Name == "assembly_c_bridge").ExternalName!.Name == "assembly_c_bridge",
    "extern C external identity must retain its actual unmangled name");
var receiverA = cppA.TranslationUnit.Functions.Single(f => f.Name == "read");
var receiverB = cppB.TranslationUnit.Functions.Single(f => f.Name == "read");
Require(new AbiCompatibility().CompareFunctions(receiverA, receiverB).Compatible,
    "selected hidden receiver pointer ABI must not require complete owning-class equivalence");
Require(cpp.Program.Resolve(receiverA)?.Definition == receiverB, "actual selected member declaration must bind to its captured-name definition");
Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(args[1]))!);
File.WriteAllText(args[1], JsonSerializer.Serialize(new { c = result.Report, cpp = cpp.Report }, ProgramManifest.JsonOptions) + "\n");
Console.WriteLine("Genuine cross-unit strong function/data, local identity, opaque-pointer, qualifier, C++ naming and link-diagnostic checks passed.");

RoslynObject Open(string name) => RoslynObject.Open(Path.Combine(directory, name + ".roslynobj"));
static string Explain(ProgramAssemblyResult result) => string.Join("\n", result.Diagnostics.Select(d => d.Code + " " + d.Locator + " " + d.Message));
static CppType Pointee(CppType type)
{
    while (type is TypeReference reference) type = reference.UnderlyingType;
    type = ((PointerType)type).PointeeType;
    while (type is TypeReference reference) type = reference.UnderlyingType;
    return type;
}
static void Expect(ProgramAssemblyResult result, string code, string reason)
    => Require(!result.Success && result.Program is null && result.Diagnostics.Any(d => d.Code == code), reason + " must fail explicitly: " + Explain(result));
static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
