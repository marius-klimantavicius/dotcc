using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

/// <summary>Compile the translated API and its consumer as separate assemblies.
/// The consumer explicitly registers a managed function pointer; no extension
/// discovery, native interop, delegates, or collectible code pointers are used.</summary>
public sealed class ManagedLibraryTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Separate_consumer_registers_managed_callback_and_survives_gc(bool objectLink)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-managed-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, """
            typedef struct Hook Hook;
            struct Hook { int (*callback)(void *, int); void *context; };
            enum Status { Ready = 7 };
            struct Bundle { Hook hooks[2]; enum Status status; };
            int api_version = 7;
            static int initial(void *context, int value) { return value + 1; }
            static Hook current = { initial, 0 };
            void install(Hook hook) { current = hook; }
            int invoke(int value) { return current.callback(current.context, value); }
            void clear(void) { current.callback = 0; current.context = 0; }
            int bundle_invoke(struct Bundle *bundle, int value) {
                return bundle->hooks[0].callback(bundle->hooks[0].context, value) + (int)bundle->status;
            }
            """);
        try
        {
            var mode = EmitMode.ManagedLib;
            string emitted;
            if (objectLink)
            {
                var fragment = Path.ChangeExtension(path, ".cs");
                try
                {
                    File.WriteAllText(fragment, Compiler.EmitObject(path));
                    emitted = Compiler.LinkObjects(new[] { fragment }, emit: mode);
                }
                finally { File.Delete(fragment); }
            }
            else emitted = Compiler.EmitCSharp(new[] { path }, emit: mode);
            var references = RuntimeReferences();
            var libraryName = "DotccManaged_" + Guid.NewGuid().ToString("N");
            var libraryImage = Compile(libraryName, emitted, references);
            references.Add(MetadataReference.CreateFromImage(libraryImage));
            var consumerImage = Compile("DotccConsumer_" + Guid.NewGuid().ToString("N"), """
                public static unsafe class Consumer
                {
                    private static int Callback(void* context, int value) => *(int*)context + value;
                    public static int Run()
                    {
                        if (DotCcLib.invoke(1) != 2) return -1;
                        int basis = 40;
                        Hook hook = new Hook { callback = &Callback, context = &basis };
                        Bundle bundle = default;
                        bundle.hooks[0] = hook;
                        bundle.status = Status.Ready;
                        if (DotCcGlobals.api_version != 7 || DotCcLib.bundle_invoke(&bundle, 2) != 49) return -2;
                        DotCcLib.install(hook);
                        try
                        {
                            System.GC.Collect();
                            System.GC.WaitForPendingFinalizers();
                            System.GC.Collect();
                            return DotCcLib.invoke(2);
                        }
                        finally { DotCcLib.clear(); }
                    }
                }
                """, references);

            // A managed function pointer is process-local code. Keep both code
            // images rooted in a noncollectible context for its whole lifetime.
            var context = new AssemblyLoadContext("dotcc-managed-" + Guid.NewGuid().ToString("N"), isCollectible: false);
            var library = context.LoadFromStream(new MemoryStream(libraryImage));
            var consumer = context.LoadFromStream(new MemoryStream(consumerImage));
            context.IsCollectible.ShouldBeFalse();
            library.GetType("DotCcLib", true)!.IsPublic.ShouldBeTrue();
            library.GetType("Hook", true)!.IsPublic.ShouldBeTrue();
            library.GetType("DotCcExports").ShouldBeNull();
            foreach (var method in library.GetType("DotCcLib", true)!.GetMethods(BindingFlags.Public | BindingFlags.Static))
                method.GetCustomAttributesData().ShouldNotContain(attribute => attribute.AttributeType.Name == "UnmanagedCallersOnlyAttribute");
            consumer.GetType("Consumer", true)!.GetMethod("Run")!.Invoke(null, null).ShouldBe(42);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Managed_project_is_a_library_with_explicit_generator_integration()
    {
        var project = Compiler.BuildGeneratedCsproj(assemblyName: "Example",
            offsetGeneratorAssembly: "/tmp/generator & tools/DotCC.OffsetGenerator.dll", managedLibrary: true);
        project.ShouldContain("<OutputType>Library</OutputType>");
        project.ShouldContain("DOTCC_OFFSET_GENERATOR");
        project.ShouldContain("generator &amp; tools/DotCC.OffsetGenerator.dll");
        project.ShouldNotContain("<NativeLib>");
        project.ShouldNotContain("<PublishAot>");
        Should.Throw<ArgumentException>(() => Compiler.BuildGeneratedCsproj(libraryMode: true, managedLibrary: true));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Managed_mode_rejects_explicit_native_bindings(bool archive)
    {
        var imports = new ImportOptions(archive ? Array.Empty<string>() : new[] { "example" },
            Array.Empty<string>(), archive ? new[] { "/tmp/libexample.a" } : Array.Empty<string>());
        Should.Throw<CompileException>(() => Compiler.EmitCSharp(Array.Empty<string>(), emit: EmitMode.ManagedLib, imports: imports))
            .Message.ShouldContain("does not support native import or archive bindings");
        Should.Throw<CompileException>(() => Compiler.LinkObjects(Array.Empty<string>(), emit: EmitMode.ManagedLib, imports: imports))
            .Message.ShouldContain("does not support native import or archive bindings");
    }

    private static List<MetadataReference> RuntimeReferences()
    {
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location)).Cast<MetadataReference>().ToList();
        FixtureRunner.AddReferenceByType(references, typeof(System.Diagnostics.Process));
        FixtureRunner.AddReferenceByType(references, typeof(System.Net.Sockets.Socket));
        FixtureRunner.AddReferenceByType(references, typeof(System.Net.IPAddress));
        FixtureRunner.AddReferenceByType(references, typeof(System.ComponentModel.Win32Exception));
        return references;
    }

    private static byte[] Compile(string name, string source, IReadOnlyList<MetadataReference> references)
    {
        var compilation = CSharpCompilation.Create(name,
            new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview)) },
            references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                allowUnsafe: true, optimizationLevel: OptimizationLevel.Release));
        using var output = new MemoryStream();
        var result = compilation.Emit(output);
        if (!result.Success)
            throw new InvalidOperationException("Separate managed library/consumer compilation failed:\n" +
                string.Join("\n", result.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)));
        return output.ToArray();
    }
}
