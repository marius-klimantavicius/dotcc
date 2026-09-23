using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class InstanceTranslationTests
{
    private static readonly CSharpOutputOptions Instance = new(Runtime: RuntimeProfile.C, InstanceMethods: true);

    [Fact]
    public void Instance_objects_require_matching_explicit_link_convention()
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-instance-abi-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var source = Path.Combine(directory, "unit.c");
            File.WriteAllText(source, "int value; int step(void) { return ++value; }");
            var oldObject = Path.Combine(directory, "static.o");
            var newObject = Path.Combine(directory, "instance.o");
            File.WriteAllText(oldObject, Compiler.EmitObject(source));
            File.WriteAllText(newObject, Compiler.EmitObject(source, outputOptions: new(InstanceMethods: true)));
            File.ReadAllText(newObject).ShouldContain("calling-convention:instance-v1");
            Should.Throw<CompileException>(() => Compiler.LinkObjects(new[] { newObject }, emit: EmitMode.ManagedLib))
                .Message.ShouldContain("calling convention mismatch");
            Should.Throw<CompileException>(() => Compiler.LinkObjects(new[] { oldObject, newObject }, emit: EmitMode.ManagedLib, outputOptions: Instance))
                .Message.ShouldContain("calling convention mismatch");
            var result = Compiler.LinkObjects(new[] { newObject }, emit: EmitMode.ManagedLib, outputOptions: Instance);
            result.ShouldContain("sealed unsafe class DotCcLib");
            result.ShouldContain("delegate*<DotCcFunctions, int>");
            result.ShouldContain("public unsafe int step(");
            result.ShouldNotContain("public static unsafe int step(");
        }
        finally { Directory.Delete(directory, true); }
    }

    [Theory]
    [InlineData("extern int external(int (*cb)(int)); int twice(int n) { return n*2; } int test(void) { return external(twice); }")]
    [InlineData("extern int external(int (*cb)(int)); int (*address(void))(int (*)(int)) { return external; }")]
    [InlineData("struct Box { int (*cb)(int); }; extern int external(struct Box *); int test(struct Box *box) { return external(box); }")]
    [InlineData("struct Box { int (*cb)(int); }; extern int external(struct Box); int test(struct Box box) { return external(box); }")]
    [InlineData("typedef int (*Fn)(int); extern Fn external(void); int test(void) { return external()(1); }")]
    public void Unadapted_external_callback_boundaries_are_diagnosed(string source)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-instance-unsupported-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, source);
        try
        {
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib, outputOptions: Instance))
                .Message.ShouldContain("unsupported external callback boundary");
            // Merely declaring an unused callback boundary is allowed.
            File.WriteAllText(path, "extern int external(int (*cb)(int)); int value(void) { return 1; }");
            Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib, outputOptions: Instance).ShouldContain("int value(");
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Instance_mode_rejects_unwrapped_and_ambiguous_state_output()
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-instance-mode-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, "int main(void) { return 0; }");
        try
        {
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }, outputOptions: Instance));
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib,
                outputOptions: Instance with { StateContext = true }));
        }
        finally { File.Delete(path); }
    }
}
