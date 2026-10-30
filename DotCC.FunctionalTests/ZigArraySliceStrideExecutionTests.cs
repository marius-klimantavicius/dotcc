using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigArraySliceStrideExecutionTests
{
    [Theory]
    [InlineData(false, false, 132)]
    [InlineData(true, false, 132)]
    [InlineData(false, true, 120)]
    [InlineData(true, true, 120)]
    public void Slices_and_reslices_of_rows_scale_the_pointer_exactly_once(bool objects, bool reslice, int expected)
    {
        var source = """
            fn sumRows(rows: []const [3]u8) u32 {
                var sum: u32 = 0;
                for (rows) |row| sum += row[0] + row[1] * 2 + row[2] * 3;
                return sum;
            }
            fn bump(rows: [][3]u8) void {
                for (rows) |*row| row[1] += 1;
                rows[0][2] = 9;
            }
            pub fn main() u8 {
                var grid: [4][3]u8 = .{ .{ 1, 2, 3 }, .{ 4, 5, 6 }, .{ 7, 8, 9 }, .{ 0, 0, 1 } };
                const mid = grid[1..3];
                bump(mid);
                const row: [3]u8 = mid[1];
                return @intCast(sumRows(&grid) % 200 + row[1] + mid.len + grid[1][2]);
            }
            """;
        if (reslice)
            source = source.Replace("const mid = grid[1..3];", "const outer = grid[1..4]; const mid = outer[1..2];")
                .Replace("mid[1];", "mid[0];").Replace("grid[1][2]", "grid[2][2]");
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-zig-row-slice-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "main.zig");
            File.WriteAllText(path, source);
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "main.object.cs");
                File.WriteAllText(fragment, Compiler.EmitObject(path));
                emitted = Compiler.LinkObjects(new[] {fragment}, emit: EmitMode.Csproj);
            }
            else emitted = Compiler.EmitCSharp(new[] {path}, emit: EmitMode.Csproj);
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, Array.Empty<string>());
            result.exit.ShouldBe(expected);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }
}
