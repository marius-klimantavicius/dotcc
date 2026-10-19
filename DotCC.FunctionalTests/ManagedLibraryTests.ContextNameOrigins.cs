using System;
using System.IO;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed partial class ManagedLibraryTests
{
    [Theory]
    [InlineData(false, 0)]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    [InlineData(true, 1)]
    [InlineData(false, 2)]
    [InlineData(true, 2)]
    public void Anonymous_struct_union_public_consumer_keeps_static_context_and_instance_semantics(bool objectLink, int mode)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-public-origin-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "unit.c");
            var obj = Path.Combine(directory, "unit.o");
            File.WriteAllText(path, """
                typedef struct { int value; } Cell;
                typedef union { int value; unsigned bits; } Choice;
                int count = 5;
                int probe(Cell *p, Choice *q) { return p->value + q->value + count++; }
                """);
            var options = new CSharpOutputOptions(Runtime: RuntimeProfile.C,
                StateContext: mode == 1, InstanceMethods: mode == 2, LiteralPool: true);
            string generated;
            if (objectLink)
            {
                File.WriteAllText(obj, Compiler.EmitObject(path, outputOptions: new(InstanceMethods: mode == 2)));
                generated = Compiler.LinkObjects(new[] { obj }, emit: EmitMode.ManagedLib, className: "Api", outputOptions: options);
            }
            else generated = Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib, className: "Api", outputOptions: options);
            string enter = mode switch
            {
                1 => "using var owner = Api.__DotCcCreateContext(); using var binding = owner.Enter();",
                2 => "using var owner = new Api();",
                _ => ""
            };
            string receiver = mode == 2 ? "owner" : "Api";
            var host = $$"""
                public static unsafe class Consumer {
                    public static int Run() {
                        {{enter}}
                        Cell cell = new() { value = 31 };
                        Choice choice = new() { bits = 6 };
                        if (sizeof(Cell) != 4 || sizeof(Choice) != 4) return -1;
                        int first = {{receiver}}.probe(&cell, &choice);
                        System.GC.Collect();
                        return first == 42 && {{receiver}}.probe(&cell, &choice) == 43 ? 42 : -2;
                    }
                }
                """;
            var references = RuntimeReferences();
            var library = Compile("Origins_" + Guid.NewGuid().ToString("N"), generated, references);
            references.Add(MetadataReference.CreateFromImage(library));
            var consumer = Compile("OriginConsumer_" + Guid.NewGuid().ToString("N"), host, references);
            var context = new AssemblyLoadContext("origin-" + Guid.NewGuid(), isCollectible: false);
            context.LoadFromStream(new MemoryStream(library));
            var assembly = context.LoadFromStream(new MemoryStream(consumer));
            assembly.GetType("Consumer", true)!.GetMethod("Run")!.Invoke(null, null).ShouldBe(42);
        }
        finally { Directory.Delete(directory, true); }
    }
}
