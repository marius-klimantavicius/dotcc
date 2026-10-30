using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigModuleConfigurationExecutionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Same_dependency_alias_in_two_modules_uses_each_own_configuration(bool objects)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-zig-module-execution-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string Write(string name, string source)
        {
            var path = Path.Combine(directory, name);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, source);
            return path;
        }
        try
        {
            var root = Write("main.zig", "const left = @import(\"left\"); const right = @import(\"right\"); pub fn main() u8 { return left.answer() + right.answer(); }");
            var left = Write("left/api.zig", "const c = @import(\"config\"); pub fn answer() u8 { return c.value; }");
            var right = Write("right/api.zig", "const c = @import(\"config\"); pub fn answer() u8 { return c.value; }");
            var configA = Write("a.zig", "pub const value: u8 = 17;");
            var configB = Write("b.zig", "pub const value: u8 = 25;");
            var options = new ZigCompilationOptions("root", new[] {
                new ZigModuleDefinition("root", root, new[] {new ZigModuleDependency("left", "one"), new ZigModuleDependency("right", "two")}),
                new ZigModuleDefinition("one", left, new[] {new ZigModuleDependency("config", "a")}),
                new ZigModuleDefinition("two", right, new[] {new ZigModuleDependency("config", "b")}),
                new ZigModuleDefinition("a", configA, Array.Empty<ZigModuleDependency>()),
                new ZigModuleDefinition("b", configB, Array.Empty<ZigModuleDependency>()) });
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "main.object.cs");
                File.WriteAllText(fragment, Compiler.EmitObject(root, zigOptions: options));
                emitted = Compiler.LinkObjects(new[] {fragment}, emit: EmitMode.Csproj);
            }
            else emitted = Compiler.EmitCSharp(new[] {root}, emit: EmitMode.Csproj, zigOptions: options);
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, Array.Empty<string>());
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }
}
