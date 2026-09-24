using System;
using System.IO;
using System.Linq;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class WeakFunctionLinkTests
{
    private const string Weak = """
        __attribute__((weak)) int choose(int value) { return value + 1; }
        typedef int (*Callback)(int);
        Callback weak_address(void) { return choose; }
        int from_weak(void) { return choose(10); }
        """;
    private const string Strong = """
        int offset = 40;
        int choose(int value) { return value + offset; }
        typedef int (*Callback)(int);
        Callback strong_address(void) { return choose; }
        """;
    private const string Main = """
        typedef int (*Callback)(int);
        Callback weak_address(void); Callback strong_address(void); int from_weak(void);
        int main(void) {
            return from_weak() == 50 && weak_address() == strong_address()
                && weak_address()(2) == 42 ? 0 : 1;
        }
        """;

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, false, false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, false, true)]
    [InlineData(true, true, true)]
    public void Strong_definition_replaces_weak_calls_and_addresses_in_both_orders(bool objects, bool reverse, bool instances)
    {
        WithSources([Weak, Strong, Main], paths =>
        {
            if (reverse) Array.Reverse(paths);
            string generated = Translate(paths, objects, instances);
            if (instances)
                generated += """

                    public static class WeakProbe {
                        public static int Main() {
                            using var first = new WeakApi(); using var second = new WeakApi();
                            using (first.__DotCcEnter()) {
                                second.Globals.offset = 80;
                                return first.main() == 0 && second.from_weak() == 90 && first.from_weak() == 50 ? 0 : 1;
                            }
                        }
                    }
                    """;
            FixtureRunner.CompileAndRunCapturingExit(generated, []).exit.ShouldBe(0);
        });
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Weak_only_definition_and_prototype_annotation_remain_callable(bool objects, bool prototype)
    {
        var declaration = prototype
            ? "int choose(int value) __attribute__((weak)); int choose(int value) { return value + 1; }"
            : "__attribute__((__weak__)) int choose(int value) { return value + 1; }";
        WithSources([declaration, "int choose(int); int main(void) { return choose(41) == 42 ? 0 : 1; }"], paths =>
            FixtureRunner.CompileAndRunCapturingExit(Translate(paths, objects, false), []).exit.ShouldBe(0));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Multiple_weak_definitions_choose_the_first_input(bool objects)
    {
        WithSources([
            "__attribute__((weak)) int value(void) { return 7; }",
            "__attribute__((weak)) int value(void) { return 9; }",
            "int value(void); int main(void) { return value() == 7 ? 0 : 1; }"], paths =>
            FixtureRunner.CompileAndRunCapturingExit(Translate(paths, objects, false), []).exit.ShouldBe(0));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_weak_definition_does_not_hide_two_strong_definitions(bool objects)
    {
        WithSources([
            "__attribute__((weak)) int value(void) { return 7; }",
            "int value(void) { return 8; }",
            "int value(void) { return 9; } int main(void) { return value(); }"], paths =>
            Should.Throw<CompileException>(() => Translate(paths, objects, false)).Message.ShouldContain("duplicate"));
    }

    [Fact]
    public void Weak_static_function_is_rejected()
    {
        WithSources(["__attribute__((weak)) static int value(void) { return 7; }"], paths =>
            Should.Throw<CompileException>(() => Compiler.EmitObject(paths[0])).Message.ShouldContain("external linkage"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Undefined_optional_weak_reference_is_rejected_explicitly(bool objects)
    {
        WithSources(["__attribute__((weak)) int optional(void); int main(void) { return optional ? optional() : 0; }"], paths =>
            Should.Throw<CompileException>(() => Translate(paths, objects, false)).Message.ShouldContain("undefined weak function reference"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Weak_prototype_in_another_unit_does_not_weaken_strong_definition(bool objects)
    {
        WithSources([
            "int value(void) { return 42; }",
            "int value(void) __attribute__((weak)); int main(void) { return value() == 42 ? 0 : 1; }"], paths =>
            FixtureRunner.CompileAndRunCapturingExit(Translate(paths, objects, false), []).exit.ShouldBe(0));
    }

    [Fact]
    public void Weak_object_is_rejected_instead_of_ignoring_linkage()
    {
        WithSources(["__attribute__((weak)) int value;"], paths =>
            Should.Throw<CompileException>(() => Compiler.EmitObject(paths[0])).Message.ShouldContain("non-function"));
    }

    private static string Translate(string[] paths, bool objects, bool instances)
    {
        var options = instances ? new CSharpOutputOptions(Runtime: RuntimeProfile.C, InstanceMethods: true) : null;
        var mode = instances ? EmitMode.ManagedLib : EmitMode.Csproj;
        if (!objects) return Compiler.EmitCSharp(paths, emit: mode, className: instances ? "WeakApi" : null, outputOptions: options);
        var fragments = paths.Select(path =>
        {
            var output = path + ".o.cs";
            File.WriteAllText(output, Compiler.EmitObject(path, outputOptions: instances ? new(InstanceMethods: true) : null));
            return output;
        }).ToArray();
        return Compiler.LinkObjects(fragments, emit: mode, className: instances ? "WeakApi" : null, outputOptions: options);
    }

    private static void WithSources(string[] sources, Action<string[]> action)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-weak-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var paths = sources.Select((source, index) =>
            {
                var path = Path.Combine(directory, "unit" + index + ".c");
                File.WriteAllText(path, source);
                return path;
            }).ToArray();
            action(paths);
        }
        finally { Directory.Delete(directory, true); }
    }
}
