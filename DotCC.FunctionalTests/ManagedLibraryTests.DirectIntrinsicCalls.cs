using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed partial class ManagedLibraryTests
{
    [Theory]
    [InlineData(false, SourceSplit.None)]
    [InlineData(false, SourceSplit.Function)]
    [InlineData(true, SourceSplit.None)]
    [InlineData(true, SourceSplit.Function)]
    public void Direct_intrinsics_preserve_original_C_conversions_contexts_effects_and_addresses(bool objects, SourceSplit split)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-direct-intrinsic-run-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var fixture = Path.Combine(AppContext.BaseDirectory, "Fixtures", "function-intrinsic-calls");
            foreach (var name in new[] { "reference.c", "main.c", "operations.h" })
                File.Copy(Path.Combine(fixture, name), Path.Combine(directory, name));
            var selected = Path.Combine(directory, "selected.h");
            File.Copy(Path.Combine(directory, "operations.h"), selected);
            var paths = new[] { Path.Combine(directory, "reference.c"), Path.Combine(directory, "main.c") };
            var options = new CPreprocessingOptions(Array.Empty<MacroOverride>(), functionOverrides: DirectPrimitiveRules(selected));
            if (objects)
                paths = paths.Select(path =>
                {
                    var obj = Path.ChangeExtension(path, ".o");
                    File.WriteAllText(obj, Compiler.EmitObject(path, preprocessing: options));
                    return obj;
                }).ToArray();
            var files = objects
                ? Compiler.LinkObjectFiles(paths, emit: EmitMode.ManagedLib, className: "Api", split: split)
                : Compiler.EmitCSharpFiles(paths, emit: EmitMode.ManagedLib, className: "Api", split: split, preprocessing: options);
            var trees = files.Select(file => ParseSource(file.Value, path: file.Key)).ToArray();
            var methods = trees.SelectMany(tree => tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>()).ToArray();
            var memory = methods.Single(method => method.Identifier.ValueText == "check_memory").Body!.ToFullString();
            memory.ShouldContain("Unsafe.ReadUnaligned<ushort>");
            memory.ShouldContain("BinaryPrimitives.ReadUInt32BigEndian");
            memory.ShouldContain("BinaryPrimitives.ReadUInt64LittleEndian");
            memory.ShouldContain("Unsafe.WriteUnaligned<ulong>");
            foreach (var name in new[] { "check_memory", "check_conversions", "check_contexts" })
            {
                var calls = methods.Single(method => method.Identifier.ValueText == name).Body!.DescendantNodes()
                    .OfType<InvocationExpressionSyntax>().Select(call => call.Expression.ToString()).ToArray();
                foreach (var rule in options.FunctionOverrides)
                    calls.ShouldNotContain(rule.Name, "Actual direct caller must use the BCL intrinsic, not the selected C wrapper.");
            }
            var compilation = CSharpCompilation.Create("DirectPrimitives_" + Guid.NewGuid().ToString("N"), trees, RuntimeReferences(),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
            using var image = new MemoryStream();
            var emitted = compilation.Emit(image, cancellationToken: TestContext.Current.CancellationToken);
            emitted.Success.ShouldBeTrue(string.Join("\n", emitted.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d + "\n" + d.Location.SourceTree?.GetText().Lines[d.Location.GetLineSpan().StartLinePosition.Line].ToString())));
            image.Position = 0;
            var assembly = new AssemblyLoadContext("direct-primitives-" + Guid.NewGuid(), isCollectible: false).LoadFromStream(image);
            var api = assembly.GetType("Api", true)!;
            api.GetMethod("probe")!.Invoke(null, null).ShouldBe(0);
            api.GetMethod("check_shadowing")!.Invoke(null, null).ShouldBe(86);
            api.GetMethod("clz64")!.Invoke(null, new object[] { 1UL }).ShouldBe(63U);
            api.GetMethod("swap32")!.Invoke(null, new object[] { 0x12345678U }).ShouldBe(0x78563412U);
        }
        finally { Directory.Delete(directory, true); }
    }

    [Theory]
    [InlineData(SourceSplit.None)]
    [InlineData(SourceSplit.Function)]
    public void Equal_intrinsics_keep_distinct_TU_local_addresses_with_inline_deduplication(SourceSplit split)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-direct-intrinsic-identities-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var objects = new List<string>();
            foreach (var name in new[] { "first", "second" })
            {
                var source = Path.Combine(directory, name + ".c");
                File.WriteAllText(source, $$"""
                    #include <stdint.h>
                    static inline uint32_t local(const void *p) { return 0; }
                    uint32_t call_{{name}}(const void *p) { return local(p); }
                    uint32_t (*address_{{name}}(int unused))(const void *) { return local; }
                    """);
                var options = new CPreprocessingOptions(Array.Empty<MacroOverride>(), functionOverrides: new[]
                {
                    new FunctionOverride("local", new("uint32_t", new[] { "const void *" }),
                        new("intrinsic", "load.u32.be"), Linkage: "internal", RequireMatch: true)
                });
                var obj = Path.ChangeExtension(source, ".o");
                File.WriteAllText(obj, Compiler.EmitObject(source, preprocessing: options));
                objects.Add(obj);
            }
            var consumer = Path.Combine(directory, "consumer.c");
            File.WriteAllText(consumer, """
                #include <stdint.h>
                uint32_t (*address_first(int))(const void *);
                uint32_t (*address_second(int))(const void *);
                uint32_t call_first(const void *);
                uint32_t call_second(const void *);
                int probe(void) {
                    unsigned char b[4]={0x12,0x34,0x56,0x78};
                    if(address_first(0)!=address_first(0) || address_second(0)!=address_second(0)) return 1;
                    if(address_first(0)==address_second(0)) return 2;
                    if(call_first(b)!=0x12345678U || call_second(b)!=0x12345678U) return 3;
                    if(address_first(0)(b)!=0x12345678U || address_second(0)(b)!=0x12345678U) return 4;
                    return 0;
                }
                """);
            var consumerObject = Path.ChangeExtension(consumer, ".o");
            File.WriteAllText(consumerObject, Compiler.EmitObject(consumer)); objects.Add(consumerObject);
            var files = Compiler.LinkObjectFiles(objects, emit: EmitMode.ManagedLib, className: "Api", split: split,
                outputOptions: new(DeduplicateInline: true));
            var compilation = CSharpCompilation.Create("DirectIdentity_" + Guid.NewGuid().ToString("N"),
                files.Select(file => ParseSource(file.Value, path: file.Key)), RuntimeReferences(),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
            using var image = new MemoryStream();
            var emitted = compilation.Emit(image, cancellationToken: TestContext.Current.CancellationToken);
            emitted.Success.ShouldBeTrue(string.Join("\n", emitted.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
            image.Position = 0;
            var assembly = new AssemblyLoadContext("direct-identities-" + Guid.NewGuid(), isCollectible: false).LoadFromStream(image);
            assembly.GetType("Api", true)!.GetMethod("probe")!.Invoke(null, null).ShouldBe(0);
        }
        finally { Directory.Delete(directory, true); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Adapted_intrinsic_discards_preserve_user_underscore_variables_and_generated_name_collisions(bool objects)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-intrinsic-underscore-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var fixture = Path.Combine(AppContext.BaseDirectory, "Fixtures", "function-intrinsic-calls");
            var source = Path.Combine(directory, "underscore.c");
            File.Copy(Path.Combine(fixture, "underscore.c"), source);
            File.Copy(Path.Combine(fixture, "operations.h"), Path.Combine(directory, "operations.h"));
            var selected = Path.Combine(directory, "selected.h");
            File.Copy(Path.Combine(fixture, "operations.h"), selected);
            var options = new CPreprocessingOptions(Array.Empty<MacroOverride>(), functionOverrides: DirectPrimitiveRules(selected));
            string generated;
            if(objects)
            {
                var obj=Path.ChangeExtension(source,".o");
                File.WriteAllText(obj,Compiler.EmitObject(source,preprocessing:options));
                generated=Compiler.LinkObjects(new[] {obj},emit:EmitMode.ManagedLib,className:"Api");
            }
            else generated=Compiler.EmitCSharp(new[] {source},emit:EmitMode.ManagedLib,className:"Api",preprocessing:options);
            var image=Compile("IntrinsicUnderscore_"+Guid.NewGuid().ToString("N"),generated,RuntimeReferences());
            var assembly=new AssemblyLoadContext("intrinsic-underscore-"+Guid.NewGuid(),isCollectible:false).LoadFromStream(new MemoryStream(image));
            assembly.GetType("Api",true)!.GetMethod("underscore_probe")!.Invoke(null,null).ShouldBe(1);
        }
        finally { Directory.Delete(directory,true); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Instance_direct_intrinsics_preserve_owner_isolation_and_indirect_wrappers(bool objects)
    {
        var directory=Path.Combine(Path.GetTempPath(),"dotcc-instance-direct-intrinsic-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var source=Path.Combine(directory,"probe.c");
            File.WriteAllText(source,"""
                unsigned value=7;
                unsigned selected(const void* p) { return 0; }
                unsigned snapshot(void) { return selected(&value); }
                int probe(void) {
                    unsigned (*callback)(const void*)=selected;
                    return selected(&value)==callback(&value) && selected(&value)==value;
                }
                """);
            var preprocessing=new CPreprocessingOptions(Array.Empty<MacroOverride>(),functionOverrides:new[] {
                new FunctionOverride("selected",new("unsigned",new[] {"const void *"}),new("intrinsic","load.u32.native"),RequireMatch:true)
            });
            var options=new CSharpOutputOptions(Runtime:RuntimeProfile.C,InstanceMethods:true);
            string generated;
            if(objects)
            {
                var obj=source+".o";File.WriteAllText(obj,Compiler.EmitObject(source,preprocessing:preprocessing,outputOptions:new(InstanceMethods:true)));
                generated=Compiler.LinkObjects(new[] {obj},emit:EmitMode.ManagedLib,className:"Api",outputOptions:options);
            }
            else generated=Compiler.EmitCSharp(new[] {source},emit:EmitMode.ManagedLib,className:"Api",preprocessing:preprocessing,outputOptions:options);
            const string host="""
                public static class InstanceIntrinsicHost {
                    public static int Run() {
                        using var a=new Api();using var b=new Api();
                        a.Globals.value=41;b.Globals.value=73;
                        using(b.__DotCcEnter()) return a.snapshot()==41 && b.snapshot()==73 && a.probe()==1 && b.probe()==1 ? 1 : 0;
                    }
                }
                """;
            var compilation=CSharpCompilation.Create("InstanceIntrinsic_"+Guid.NewGuid().ToString("N"),
                new[] {ParseSource(generated),ParseSource(host)},RuntimeReferences(),new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,allowUnsafe:true));
            using var image=new MemoryStream();var emitted=compilation.Emit(image,cancellationToken:TestContext.Current.CancellationToken);
            emitted.Success.ShouldBeTrue(string.Join("\n",emitted.Diagnostics.Where(d=>d.Severity==DiagnosticSeverity.Error)));
            image.Position=0;var assembly=new AssemblyLoadContext("instance-intrinsic-"+Guid.NewGuid(),isCollectible:false).LoadFromStream(image);
            assembly.GetType("InstanceIntrinsicHost")!.GetMethod("Run")!.Invoke(null,null).ShouldBe(1);
        }
        finally {Directory.Delete(directory,true);}
    }

    private static FunctionOverride[] DirectPrimitiveRules(string selected)
    {
        FunctionOverride Rule(string name, string result, string[] parameters, string target) =>
            new(name, new(result, parameters), new("intrinsic", target), DeclarationFile: selected, RequireMatch: true);
        return new[]
        {
            Rule("read_native16", "uint16_t", new[] { "const void *" }, "load.u16.native"),
            Rule("read_be32", "uint32_t", new[] { "const void *" }, "load.u32.be"),
            Rule("read_le64", "uint64_t", new[] { "const void *" }, "load.u64.le"),
            Rule("write_le16", "void", new[] { "void *", "uint16_t" }, "store.u16.le"),
            Rule("write_be32", "void", new[] { "void *", "uint32_t" }, "store.u32.be"),
            Rule("write_native64", "void", new[] { "void *", "uint64_t" }, "store.u64.native"),
            Rule("swap32", "uint32_t", new[] { "uint32_t" }, "bswap.u32"),
            Rule("swap64", "uint64_t", new[] { "uint64_t" }, "bswap.u64"),
            Rule("clz32", "unsigned", new[] { "uint32_t" }, "clz.u32"),
            Rule("ctz32", "unsigned", new[] { "uint32_t" }, "ctz.u32"),
            Rule("clz64", "unsigned", new[] { "uint64_t" }, "clz.u64"),
            Rule("ctz64", "unsigned", new[] { "uint64_t" }, "ctz.u64"),
            Rule("highbit", "unsigned", new[] { "uint32_t" }, "highbit.u32"),
            Rule("commonbytes", "unsigned", new[] { "uint64_t" }, "commonbytes.u64"),
            Rule("little_unsigned", "unsigned", Array.Empty<string>(), "is-little-endian.u32"),
            Rule("little_signed", "int", Array.Empty<string>(), "is-little-endian.i32"),
        };
    }
}
