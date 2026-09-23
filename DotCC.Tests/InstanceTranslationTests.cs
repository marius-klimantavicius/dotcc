using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class InstanceTranslationTests
{
    private static readonly CSharpOutputOptions Instance = new(Runtime: RuntimeProfile.C, InstanceMethods: true);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Instance_canonical_pointer_signature_conflicts_still_fail(bool reverse)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-instance-conflict-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var one = Path.Combine(directory, "one.c"); var two = Path.Combine(directory, "two.c");
            File.WriteAllText(one, "extern int external(int); int (*first(void))(int) { return external; }");
            File.WriteAllText(two, "extern long external(int); long (*second(void))(int) { return external; }");
            var objects = new[] { one + ".o", two + ".o" };
            File.WriteAllText(objects[0], Compiler.EmitObject(one, outputOptions: new(InstanceMethods: true)));
            File.WriteAllText(objects[1], Compiler.EmitObject(two, outputOptions: new(InstanceMethods: true)));
            if (reverse) Array.Reverse(objects);
            Should.Throw<CompileException>(() => Compiler.LinkObjects(objects, emit: EmitMode.ManagedLib, outputOptions: Instance))
                .Message.ShouldContain("conflicting canonical function pointer declarations");
        }
        finally { Directory.Delete(directory, true); }
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Opaque_and_complete_callback_aggregates_merge_conservatively(bool adapted, bool reverse)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-instance-opaque-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var opaque = Path.Combine(directory, "opaque.c");
            var complete = Path.Combine(directory, "complete.c");
            File.WriteAllText(opaque, "struct Box; typedef int (*Boundary)(struct Box *); extern int external(struct Box *); Boundary first(void) { return external; }");
            File.WriteAllText(complete, "struct Box { int (*callback)(int); }; typedef int (*Boundary)(struct Box *); extern int external(struct Box *); Boundary second(void) { return external; }");
            var preprocessing = adapted ? new CPreprocessingOptions(Array.Empty<MacroOverride>(), functionOverrides: new[] {
                new FunctionOverride("external", new("int", new[] { "struct Box*" }),
                    new("managedMethod", "global::Boundary.Invoke", PassInstance: true), RequireMatch: true) }) : null;
            var objects = new[] { opaque + ".o", complete + ".o" };
            File.WriteAllText(objects[0], Compiler.EmitObject(opaque, preprocessing: preprocessing, outputOptions: new(InstanceMethods: true)));
            File.WriteAllText(objects[1], Compiler.EmitObject(complete, preprocessing: preprocessing, outputOptions: new(InstanceMethods: true)));
            if (reverse) Array.Reverse(objects);
            if (adapted)
            {
                var generated = Compiler.LinkObjects(objects, emit: EmitMode.ManagedLib, outputOptions: Instance);
                generated.ShouldContain("global::Boundary.Invoke(this,");
                generated.ShouldNotContain("/*__dotcc_callback_context__*/");
            }
            else Should.Throw<CompileException>(() => Compiler.LinkObjects(objects, emit: EmitMode.ManagedLib, outputOptions: Instance))
                .Message.ShouldContain("unsupported external callback boundary");
        }
        finally { Directory.Delete(directory, true); }
    }

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
