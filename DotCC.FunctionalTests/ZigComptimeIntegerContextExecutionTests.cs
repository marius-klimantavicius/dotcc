using System;
using System.IO;
using DotCC.Frontends;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigComptimeIntegerContextExecutionTests
{
    private const string Program = """
        const offset = 4;
        fn fillValue() comptime_int { return 4294967295; }
        fn read(bytes: []const u8, i: usize) u8 { return bytes[i + offset]; }
        fn wide(value: u128, i: u64) u128 { return value + i; }
        pub fn main() u8 {
            var words: [3]u32 = undefined;
            @memset(&words, fillValue());
            var i: usize = 0;
            while (i < words.len) : (i += 1) {
                if (words[i] != 4294967295) return 1;
            }
            const bytes = [_]u8 { 0, 1, 2, 3, 40, 41, 42, 43 };
            if (read(&bytes, 2) != 42) return 2;
            var huge: [2]u128 = undefined;
            @memset(&huge, @as(u128, 340282366920938463463374607431768211455));
            if (huge[0] != 340282366920938463463374607431768211455) return 3;
            if (huge[1] != 340282366920938463463374607431768211455) return 4;
            if (wide(18446744073709551616, 42) != 18446744073709551658) return 5;
            var negative: [2]i128 = undefined;
            @memset(&negative, @as(i128, -170141183460469231731687303715884105728));
            if (negative[1] != -170141183460469231731687303715884105728) return 6;
            return 42;
        }
        """;

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Memset_and_index_contexts_preserve_fitting_and_full128_values(bool objects)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-comptime-context-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(directory, "lib", "std"));
        try
        {
            var path = Path.Combine(directory, "main.zig");
            File.WriteAllText(path, Program);
            File.WriteAllText(Path.Combine(directory, "lib", "std", "std.zig"), "");
            var options = new ZigCompilationOptions("root", [new("root", path, [])],
                zigLibDir: Path.Combine(directory, "lib"), stdlibTypes: ZigStdlibTypeModel.Source,
                functionPointerAbi: ZigFunctionPointerAbi.Managed);
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "main.csobj");
                File.WriteAllText(fragment, Compiler.EmitObject(path, zigOptions: options));
                emitted = Compiler.LinkObjects([fragment], emit: EmitMode.Csproj);
            }
            else emitted = Compiler.EmitCSharp([path], emit: EmitMode.Csproj, zigOptions: options);
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, [], TestContext.Current.CancellationToken);
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }
}
