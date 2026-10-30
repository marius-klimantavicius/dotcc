using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigTypeBodyDestructureExecutionTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Reified_enum_slices_preserve_holes_sizes_and_exact_tag_width(bool objects, bool multipleHoles)
    {
        var fixture = FixtureDirectory();
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-typebody-destructure-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            File.Copy(Path.Combine(fixture, "builder.zig"), Path.Combine(directory, "builder.zig"));
            var path = Path.Combine(directory, "main.zig");
            File.Copy(Path.Combine(fixture, multipleHoles ? "main-multiple.zig" : "main.zig"), path);
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "main.object.cs");
                File.WriteAllText(fragment, Compiler.EmitObject(path));
                emitted = Compiler.LinkObjects(new[] { fragment }, emit: EmitMode.Csproj);
            }
            else emitted = Compiler.EmitCSharp(new[] { path }, emit: EmitMode.Csproj);
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, Array.Empty<string>());
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }

    private static string FixtureDirectory([CallerFilePath] string file = "")
        => Path.Combine(Path.GetDirectoryName(file)!, "Fixtures", "zig-typebody-destructure-r31");

    [Fact]
    public void Published_slices_share_backing_and_returned_methods_capture_comptime_constants()
    {
        const string source = """
            fn Build() type {
                var backing: [2]u8 = undefined;
                var value: u8 = 1;
                const view, const delta = pair: {
                    backing[0] = 10;
                    break :pair .{ backing[0..2], @as(u8, 2) };
                };
                backing[0] = 40;
                view[1] = delta;
                value += view[0] + view[1];
                value -= 1;
                const final_value = value;
                return struct {
                    pub fn answer() u8 { return final_value + delta - 2; }
                };
            }
            const Result = Build();
            pub fn main() u8 { return Result.answer(); }
            """;
        var path = Path.Combine(Path.GetTempPath(), "dotcc-typebody-mutations-" + Guid.NewGuid().ToString("N") + ".zig");
        File.WriteAllText(path, source);
        try
        {
            var result = FixtureRunner.CompileAndRunCapturingStreams(Compiler.EmitCSharp(new[] { path }), Array.Empty<string>());
            result.exit.ShouldBe(42);
        }
        finally { File.Delete(path); }
    }
    [Fact]
    public void Bounded_outer_loops_mutate_the_published_scalar_storage()
    {
        const string source = """
            fn Build() type {
                var total: u8 = 0;
                var index: usize = 0;
                while (index < 3) : (index += 1) {
                    if (index == 1) { total += 12; } else { total += 9; }
                }
                for (0..2) |i| {
                    if (i == 0) { total += 5; } else { total += 6; }
                }
                const final_total = total;
                return struct { pub fn answer() u8 { return final_total + 1; } };
            }
            const Result = Build();
            pub fn main() u8 { return Result.answer(); }
            """;
        var path = Path.Combine(Path.GetTempPath(), "dotcc-typebody-loops-" + Guid.NewGuid().ToString("N") + ".zig");
        File.WriteAllText(path, source);
        try
        {
            var result = FixtureRunner.CompileAndRunCapturingStreams(Compiler.EmitCSharp(new[] { path }), Array.Empty<string>());
            result.exit.ShouldBe(42);
        }
        finally { File.Delete(path); }
    }

}
