using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class NamedScalarPrefixCastExecutionTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Wide_named_scalar_cast_of_dereference_is_unambiguous(bool objects, bool zig)
    {
        var source = zig ? """
            fn multiply(a: *const u64, b: *const u64) u128 { return @as(u128, a.*) * b.*; }
            pub fn main() u8 {
                const a: u64 = 3;
                const b: u64 = 13;
                return @intCast(multiply(&a, &b));
            }
            """ : """
            typedef unsigned __int128 u128;
            static u128 multiply(const unsigned long long *a, const unsigned long long *b) {
                return (u128)*a * *b;
            }
            int main(void) {
                unsigned long long a = 3, b = 13;
                return (int)multiply(&a, &b);
            }
            """;
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-prefix-cast-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, zig ? "main.zig" : "main.c");
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
            result.exit.ShouldBe(39);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }
}
