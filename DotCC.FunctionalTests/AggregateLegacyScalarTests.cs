using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class AggregateLegacyScalarTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), "dotcc-legacy-scalar-" + Guid.NewGuid().ToString("N"));
    private int nextFile;

    public AggregateLegacyScalarTests() => Directory.CreateDirectory(directory);
    public void Dispose() => Directory.Delete(directory, recursive: true);

    // Byte-for-byte outputs of the immutable 20261029-181517-e418fbb7 tool:
    // dotcc --emit=obj <source.c> -o <object.cs> (X87 additionally uses
    // {"version":1,"sourceAbi":{"longDouble":"x87-storage16"}}).
    // DotCC.Lib.dll SHA256: f7f85b7294c40b547489d2893ace67da1a78039b2300d1551cb202a711e715c5.
    // These fixtures were emitted by that compiler, not downgraded Graph4 objects.
    // Source: int legacy_value(void) { return 42; }
    private const string Scalar = """
        //!dotcc object 2 — link with `dotcc <objs> -o <out>`.
        //!!dotcc-obj aggregate-identities:1
        //!!dotcc-obj aggregate-linkage:3
        //!!dotcc-obj calling-convention:static-v1
        //!!dotcc-obj source-language:c
        //!!dotcc-obj override-profile:none
        //!!dotcc-obj source-abi:binary64
        //!!dotcc-obj foreign-abi:1
        //!!dotcc-obj function-contract:{"version":1,"name":"legacy_value","signature":"int()","kind":null,"target":null,"translationUnit":null,"declarationFile":null,"linkage":null}
        //!!dotcc-obj enum-constants:1
        //!!dotcc-obj inline-metadata:4
        //!!dotcc-obj namespace-neutral:1
        //!!dotcc-obj literal-pool:2
        //!!dotcc-obj globals-layout:2
        //!!dotcc-obj main:-1
        //!!dotcc-obj def:legacy_value
        //!!dotcc-obj section:aliases
        //!!dotcc-obj section:functions
        //!!dotcc-obj function:legacy_value
        static unsafe int legacy_value()
        {
            return 42;
        }

        """ + "\n";

    // Source: int foreign_count(int n); int legacy_value(void) { return foreign_count(7); }
    private const string OrdinaryImport = """
        //!dotcc object 2 — link with `dotcc <objs> -o <out>`.
        //!!dotcc-obj aggregate-identities:1
        //!!dotcc-obj aggregate-linkage:3
        //!!dotcc-obj calling-convention:static-v1
        //!!dotcc-obj source-language:c
        //!!dotcc-obj override-profile:none
        //!!dotcc-obj source-abi:binary64
        //!!dotcc-obj foreign-abi:1
        //!!dotcc-obj function-contract:{"version":1,"name":"legacy_value","signature":"int()","kind":null,"target":null,"translationUnit":null,"declarationFile":null,"linkage":null}
        //!!dotcc-obj enum-constants:1
        //!!dotcc-obj inline-metadata:4
        //!!dotcc-obj namespace-neutral:1
        //!!dotcc-obj literal-pool:2
        //!!dotcc-obj globals-layout:2
        //!!dotcc-obj main:-1
        //!!dotcc-obj import:foreign_count delegate* unmanaged[Cdecl]<int, int>
        //!!dotcc-obj def:legacy_value
        //!!dotcc-obj section:aliases
        //!!dotcc-obj section:functions
        //!!dotcc-obj function:legacy_value
        static unsafe int legacy_value()
        {
            return foreign_count(7);
        }

        """ + "\n";

    // Source: int ffs(int n); int legacy_value(void) { return ffs(8)+38; }
    private const string NativeRuntime = """
        //!dotcc object 2 — link with `dotcc <objs> -o <out>`.
        //!!dotcc-obj aggregate-identities:1
        //!!dotcc-obj aggregate-linkage:3
        //!!dotcc-obj calling-convention:static-v1
        //!!dotcc-obj source-language:c
        //!!dotcc-obj override-profile:none
        //!!dotcc-obj source-abi:binary64
        //!!dotcc-obj foreign-abi:1
        //!!dotcc-obj function-contract:{"version":1,"name":"legacy_value","signature":"int()","kind":null,"target":null,"translationUnit":null,"declarationFile":null,"linkage":null}
        //!!dotcc-obj enum-constants:1
        //!!dotcc-obj inline-metadata:4
        //!!dotcc-obj namespace-neutral:1
        //!!dotcc-obj literal-pool:2
        //!!dotcc-obj globals-layout:2
        //!!dotcc-obj main:-1
        //!!dotcc-obj import:ffs delegate* unmanaged[Cdecl]<int, int>
        //!!dotcc-obj def:legacy_value
        //!!dotcc-obj section:aliases
        //!!dotcc-obj section:functions
        //!!dotcc-obj function:legacy_value
        static unsafe int legacy_value()
        {
            return ffs(8) + 38;
        }

        """ + "\n";

    // Source: long double foreign_value(long double value); long double legacy_value(long double value) { return foreign_value(value); }
    private const string X87 = """
        //!dotcc object 2 — link with `dotcc <objs> -o <out>`.
        //!!dotcc-obj aggregate-identities:1
        //!!dotcc-obj aggregate-linkage:3
        //!!dotcc-obj calling-convention:static-v1
        //!!dotcc-obj source-language:c
        //!!dotcc-obj override-profile:1167c17c7947529ee2178ec192956091e3bc46a9eb3d252807a671d5555b69cc
        //!!dotcc-obj source-abi:x87-storage16
        //!!dotcc-obj foreign-abi:1
        //!!dotcc-obj foreign-abi-restriction:definition bGVnYWN5X3ZhbHVl bGVnYWN5X3ZhbHVl x87-storage16
        //!!dotcc-obj foreign-abi-restriction:import Zm9yZWlnbl92YWx1ZQ== Zm9yZWlnbl92YWx1ZQ== x87-storage16
        //!!dotcc-obj function-contract:{"version":1,"name":"legacy_value","signature":"LongDoubleStorage16(LongDoubleStorage16)","kind":null,"target":null,"translationUnit":null,"declarationFile":null,"linkage":null}
        //!!dotcc-obj enum-constants:1
        //!!dotcc-obj inline-metadata:4
        //!!dotcc-obj namespace-neutral:1
        //!!dotcc-obj literal-pool:2
        //!!dotcc-obj globals-layout:2
        //!!dotcc-obj main:-1
        //!!dotcc-obj import:foreign_value delegate* unmanaged[Cdecl]<LongDouble80Storage, LongDouble80Storage>
        //!!dotcc-obj def:legacy_value
        //!!dotcc-obj section:aliases
        //!!dotcc-obj section:functions
        //!!dotcc-obj function:legacy_value
        static unsafe LongDouble80Storage legacy_value(LongDouble80Storage value)
        {
            byte* __dotcc_aligned_value_bytes = stackalloc byte[sizeof(LongDouble80Storage) + 15];
            LongDouble80Storage* __dotcc_aligned_value = (LongDouble80Storage*)(((nuint)__dotcc_aligned_value_bytes + 15) & ~(nuint)15);
            *__dotcc_aligned_value = value;
            {
                return foreign_value((*__dotcc_aligned_value));
            }
        }

        """ + "\n";

    public static IEnumerable<object[]> GenuineObjects()
    {
        yield return [ Scalar, "155CB18F55D605550475FA6AC4DEC38089BFBC0EBED69200D425F234234039D1" ];
        yield return [ OrdinaryImport, "FC789E23A7644B3CCF34F3783FA842E5BBC78F86E09647B8BD47870288DEDA23" ];
        yield return [ NativeRuntime, "C0297BC60B1C942138671392FB0D589F1DCD963B62CFCFFC80168E2B03E2AD62" ];
        yield return [ X87, "4A11A2BC3977AC4E1B2AAD435B7823754E61E3F04A13CACE3D3546F69899A6C6" ];
    }

    [Theory]
    [MemberData(nameof(GenuineObjects))]
    public void Genuine_old_scalar_graphs_are_explicit_empty_linked_sets(string fragment, string hash)
    {
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(fragment))).ShouldBe(hash);
        var graph = AggregateIdentities.Read(fragment).ShouldBeOfType<AggregateIdentitySet>();
        graph.Nodes.ShouldBeEmpty();
        graph.Linkage.ShouldBeEmpty();
        graph.Declarations.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("//!!dotcc-obj aggregate-linkage-root:ZXh0ZXJuYWw= aW50 Missing ")]
    [InlineData("//!!dotcc-obj aggregate-declaration-root:ZXh0ZXJuYWw= aW50 Missing ")]
    [InlineData("//!!dotcc-obj aggregate-linkage-root:ZXh0ZXJuYWw= aW50  ")]
    [InlineData("//!!dotcc-obj aggregate-declaration-root:ZXh0ZXJuYWw= aW50  ")]
    [InlineData("//!!dotcc-obj aggregate-linkage-root:malformed")]
    [InlineData("//!!dotcc-obj aggregate-declaration-root:malformed")]
    [InlineData("//!!dotcc-obj aggregate:Missing 0 0 BAD")]
    [InlineData("//!!dotcc-obj aggregate-identity:Missing TWlzc2luZw== 1 0 ")]
    public void Empty_graph_cannot_hide_roots_unknown_references_or_malformed_metadata(string record)
        => Should.Throw<CompileException>(() => AggregateIdentities.Read(Scalar + record + "\n"))
            .Message.ShouldContain("regenerate");

    [Theory]
    [InlineData("struct State { int value; };")]
    [InlineData("union State { int value; long other; };")]
    [InlineData("enum State { VALUE=19 };")]
    [InlineData("struct State;")]
    public void Nonempty_graph_three_still_requires_regeneration(string source)
    {
        // Deliberately downgrade modern metadata only as a negative control.
        var fragment = Object(source).Replace(AggregateIdentities.LinkageVersion,
            "//!!dotcc-obj aggregate-linkage:3", StringComparison.Ordinal);
        Should.Throw<CompileException>(() => AggregateIdentities.Read(fragment))
            .Message.ShouldContain("regenerate older objects");
    }

    public static IEnumerable<object[]> ConflictingModernObjects()
    {
        foreach (bool reverse in new[] { false, true })
        {
            // Equal shapes with different unused function types distinguish
            // PlanLinked's declaration checks from the weaker PlanLegacy path.
            yield return [ "struct State { int value; }; int external(struct State *);",
                "struct State { int value; }; long external(struct State *);", "external", reverse ];
            yield return [ "enum Token { VALUE=19 }; int external(enum Token);",
                "enum Token { VALUE=19 }; long external(enum Token);", "external", reverse ];
            yield return [ "struct State { int value; }; int external(struct State *);",
                "struct State { long value; }; int external(struct State *);", "State", reverse ];
            yield return [ "enum Token { VALUE=19 }; int external(enum Token);",
                "enum Token { VALUE=23 }; int external(enum Token);", "Token", reverse ];
        }
    }

    [Theory]
    [MemberData(nameof(ConflictingModernObjects))]
    public void Old_scalar_does_not_disable_modern_struct_or_enum_contract_checks(
        string first, string second, string symbol, bool reverse)
    {
        var paths = new[] { Write(".obj.cs", Scalar), Write(".obj.cs", Object(first)), Write(".obj.cs", Object(second)) };
        if (reverse) Array.Reverse(paths);
        var error = Should.Throw<CompileException>(() => Compiler.LinkObjects(paths, emit: EmitMode.ManagedLib));
        error.Message.ShouldContain("conflicting");
        error.Message.ShouldContain("'" + symbol + "'");
    }

    [Fact]
    public void Ordinary_old_native_imports_still_select_dynamic_bindings()
    {
        var linked = Link(OrdinaryImport, imports: Imports(archive: false));
        linked.ShouldContain("static unsafe class DotCcImports");
        linked.ShouldContain("TryResolveExport(__libs, \"foreign_count\", out __p)");
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Explicit_old_x87_objects_keep_foreign_abi_guards(bool archive, bool missingProof)
    {
        // Removing proof records is a negative control; preserve the explicit ABI marker.
        var fragment = missingProof ? string.Join("\n", X87.Split('\n').Where(line =>
            !line.StartsWith("//!!dotcc-obj foreign-abi:", StringComparison.Ordinal)
            && !line.StartsWith("//!!dotcc-obj foreign-abi-restriction:", StringComparison.Ordinal))) : X87;
        fragment.ShouldContain("//!!dotcc-obj source-abi:x87-storage16");
        Link(fragment).ShouldNotBeNullOrEmpty();
        var error = Should.Throw<CompileException>(() => Link(fragment, imports: Imports(archive)));
        error.Message.ShouldContain(missingProof ? "lacks foreign ABI proof" : "storage/layout only");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Genuine_old_scalar_with_compatible_modern_types_executes_in_a_managed_library(bool nested)
    {
        const string header = "struct State { int value; }; enum Token { READY=1 }; ";
        var paths = new[] { Write(".obj.cs", Scalar),
            Write(".obj.cs", Object(header + "int legacy_value(void); int peer(struct State *, enum Token); " +
                "int run(void) { struct State state = {41}; return legacy_value() + peer(&state, READY) - 42; }")),
            Write(".obj.cs", Object(header + "int peer(struct State *p, enum Token token) { return p->value + (int)token; }")) };
        if (nested) Array.Reverse(paths);
        var linked = Compiler.LinkObjects(paths, emit: EmitMode.ManagedLib, className: "ScalarApi",
            namespaceName: "LegacyScalarCompat", outputOptions: new(NestTypes: nested, Runtime: RuntimeProfile.C));
        var syntax = CSharpSyntaxTree.ParseText(linked, new CSharpParseOptions(LanguageVersion.Preview));
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path));
        var compilation = CSharpCompilation.Create("LegacyScalar_" + Guid.NewGuid().ToString("N"), [syntax], references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
        using var image = new MemoryStream();
        var result = compilation.Emit(image, cancellationToken: TestContext.Current.CancellationToken);
        result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
        image.Position = 0;
        var assembly = AssemblyLoadContext.Default.LoadFromStream(image);
        assembly.GetType("LegacyScalarCompat.ScalarApi", true)!.GetMethod("run", BindingFlags.Public | BindingFlags.Static)!
            .Invoke(null, null).ShouldBe(42);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Genuine_old_scalar_and_native_objects_execute_with_compatible_modern_types(bool native)
    {
        // ffs is a real POSIX libc export, with no embedded-runtime namesake.
        if (native && !OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
            Assert.Skip("The ordinary native runtime control requires POSIX libc.");
        const string header = "struct State { int value; }; enum Token { READY=1 }; ";
        var paths = new[] { Write(".obj.cs", native ? NativeRuntime : Scalar),
            Write(".obj.cs", Object(header + "int legacy_value(void); int peer(struct State *, enum Token); " +
                "int main(void) { struct State state = {41}; return legacy_value() == 42 && peer(&state, READY) == 42 ? 0 : 1; }")),
            Write(".obj.cs", Object(header + "int peer(struct State *p, enum Token token) { return p->value + (int)token; }")) };
        if (native) Array.Reverse(paths);
        var imports = native ? new ImportOptions([OperatingSystem.IsMacOS() ? "/usr/lib/libSystem.B.dylib" : "libc.so.6"], [], []) : null;
        var linked = Compiler.LinkObjects(paths, imports: imports, outputOptions: new(Runtime: RuntimeProfile.C));
        if (native) linked.ShouldContain("TryResolveExport(__libs, \"ffs\", out __p)");
        FixtureRunner.CompileAndRunCapturingExit(linked, []).exit.ShouldBe(0);
    }

    private string Write(string extension, string text)
    {
        var path = Path.Combine(directory, "unit" + nextFile++ + extension);
        File.WriteAllText(path, text);
        return path;
    }

    private string Object(string source) => Compiler.EmitObject(Write(".c", source));
    private string Link(string fragment, ImportOptions? imports = null)
        => Compiler.LinkObjects([Write(".obj.cs", fragment), Write(".obj.cs", Object("int main(void) { return 0; }"))], imports: imports);
    private ImportOptions Imports(bool archive) => archive
        ? new([], [], [Path.Combine(directory, "libscalarcompat.a")])
        : new(["scalarcompat"], [], []);
}
