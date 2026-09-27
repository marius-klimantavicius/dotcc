using System;
using System.IO;
using System.Text.RegularExpressions;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class StableFunctionPointerTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Only_address_uses_and_profile_selections_create_properties(bool objectLink)
    {
        WithDirectory(directory =>
        {
            var source = Path.Combine(directory, "callbacks.c");
            File.WriteAllText(source, """
                #include <stdlib.h>
                #define MAKE(name) static int name(int x) { return x + 2; }
                MAKE(macro_callback)
                int direct_only(int x) { return x; }
                int unused(int x) { return x; }
                int callback(int x) { return x + 1; }
                int event(int x) { return x + 3; }
                static inline int host_inline(int x) { return x + 4; }
                int (*table[])(int) = {callback, &callback};
                int run(void) { return direct_only(42); }
                """);
            var profile = Path.Combine(directory, "overrides.json");
            File.WriteAllText(profile, """
                {"version": 1, "stableFunctionPointers": ["macro_callback", "event", "host_inline", "abs", "in_another_unit"]}
                """);
            var preprocessing = CPreprocessingOptions.Load(profile);
            var options = new CSharpOutputOptions(DeduplicateInline: true);
            string emitted;
            if (objectLink)
            {
                var obj = Path.Combine(directory, "callbacks.o");
                File.WriteAllText(obj, Compiler.EmitObject(source, preprocessing: preprocessing));
                emitted = Compiler.LinkObjects(new[] { obj }, emit: EmitMode.ManagedLib, outputOptions: options);
            }
            else emitted = Compiler.EmitCSharp(new[] { source }, emit: EmitMode.ManagedLib,
                preprocessing: preprocessing, outputOptions: options);

            foreach (var name in new[] { "callback", "macro_callback", "event", "host_inline", "abs" })
                Regex.Matches(emitted, @"public static delegate\*<int, int> @?" + name + @"(?:__unit_[A-F0-9]+)?\s*\{").Count.ShouldBe(1, name);
            foreach (var name in new[] { "direct_only", "unused", "run", "in_another_unit" })
                Regex.IsMatch(emitted, @"public static delegate\*<[^\n]+> " + name + @"\s*\{").ShouldBeFalse(name);
            emitted.ShouldContain("int direct_only(int x)");
            emitted.ShouldContain("int unused(int x)");
        });
    }

    [Theory]
    [InlineData(EmitMode.File)]
    [InlineData(EmitMode.ManagedLib)]
    [InlineData(EmitMode.Object)]
    public void Direct_calls_do_not_create_pointer_container(EmitMode emit)
    {
        WithDirectory(directory =>
        {
            var source = Path.Combine(directory, "main.c");
            File.WriteAllText(source, "int add(int x) { return x + 1; } int main(void) { return add(41); }");
            var emitted = Compiler.EmitCSharp(new[] { source }, emit: emit);
            emitted.ShouldNotContain("class DotCcFunctionPointers");
            emitted.ShouldNotContain("public static delegate*");
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Host_selected_static_inline_functions_keep_distinct_addresses(bool objectLink)
    {
        WithDirectory(directory =>
        {
            var sources = new[] { Path.Combine(directory, "first.c"), Path.Combine(directory, "second.c") };
            var preprocessing = new CPreprocessingOptions(Array.Empty<MacroOverride>(), stableFunctionPointers: new[] { "helper" });
            for (var i = 0; i < sources.Length; i++)
            {
                File.WriteAllText(sources[i], "static inline int helper(int x) { return x; }");
                if (!objectLink) continue;
                var obj = Path.ChangeExtension(sources[i], ".o");
                File.WriteAllText(obj, Compiler.EmitObject(sources[i], preprocessing: preprocessing));
                sources[i] = obj;
            }
            var options = new CSharpOutputOptions(DeduplicateInline: true);
            var emitted = objectLink
                ? Compiler.LinkObjects(sources, emit: EmitMode.ManagedLib, outputOptions: options)
                : Compiler.EmitCSharp(sources, emit: EmitMode.ManagedLib, outputOptions: options, preprocessing: preprocessing);
            Regex.Matches(emitted, @"public static delegate\*<int, int> helper(?:__\w+)?\s*\{").Count.ShouldBe(2);
            Regex.Matches(emitted, @"int helper(?:__\w+)?\(int x\)").Count.ShouldBe(2);
        });
    }

    [Fact]
    public void Selection_is_immutable_hashed_and_preserved_without_report()
    {
        var names = new[] { "second", "first", "first" };
        var options = new CPreprocessingOptions(Array.Empty<MacroOverride>(), report: new StringWriter(), stableFunctionPointers: names);
        names[0] = "changed";
        options.StableFunctionPointers.ShouldBe(new[] { "first", "second" });
        options.HasOverrides.ShouldBeTrue();
        options.WithoutReport().Report.ShouldBeNull();
        options.WithoutReport().StableFunctionPointers.ShouldBe(options.StableFunctionPointers);
        options.WithoutReport().ProfileHash.ShouldBe(options.ProfileHash);
        options.ProfileHash.ShouldBe(new CPreprocessingOptions(Array.Empty<MacroOverride>(), stableFunctionPointers: new[] { "first", "second" }).ProfileHash);
        options.ProfileHash.ShouldNotBe(new CPreprocessingOptions(Array.Empty<MacroOverride>()).ProfileHash);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("\"callback\"")]
    [InlineData("[null]")]
    [InlineData("[42]")]
    [InlineData("[\"\"]")]
    [InlineData("[\"callback*\"]")]
    [InlineData("[\"Host.callback\"]")]
    public void Invalid_profile_selections_fail(string selection)
    {
        WithDirectory(directory =>
        {
            var profile = Path.Combine(directory, "overrides.json");
            File.WriteAllText(profile, "{\"version\":1,\"stableFunctionPointers\":" + selection + "}");
            Should.Throw<CompileException>(() => CPreprocessingOptions.Load(profile));
        });
    }

    private static void WithDirectory(Action<string> action)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-stable-pointers-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try { action(directory); }
        finally { Directory.Delete(directory, true); }
    }
}
