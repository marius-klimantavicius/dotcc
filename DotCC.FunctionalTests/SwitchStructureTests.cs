using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class SwitchStructureTests
{
    [Fact]
    public void A_nested_entry_keeps_unrelated_case_blocks_structured_and_compiles_within_a_bound()
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-switch-structure-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, StressSource(80));
        try
        {
            var emitted = Compiler.EmitCSharp(new[] { path });
            // Only dispatch and the region containing the nested entry need
            // labels. Flattening all unrelated branches caused Roslyn's
            // definite-assignment pass to spend minutes cloning flow states.
            Regex.Matches(emitted, @"__sw_flat_\w+:").Count.ShouldBeLessThan(200);
            using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            var (stdout, _, exit) = FixtureRunner.CompileAndRunCapturingStreams(emitted, Array.Empty<string>(), deadline.Token);
            // GCC -std=c17 -pedantic-errors, including the direct entry that
            // skips an initializer and the ordinary entry that executes it.
            stdout.ShouldBe("28320\n18 0\n18 2\n");
            exit.ShouldBe(0);
        }
        finally { File.Delete(path); }
    }

    private static string StressSource(int cases)
    {
        var source = new StringBuilder("#include <stdio.h>\nint effects;\nint run(int selected, int rounds) { int total = 0; for (int round = 0; round < rounds; ++round) { switch (selected) {\n");
        for (var section = 0; section < cases; ++section)
        {
            source.Append("case ").Append(section).Append(": {\n");
            for (var local = 0; local < 12; ++local)
                source.Append("int local").Append(local).Append(" = selected + round + ").Append(local + 1).Append(";\n");
            source.Append("int values[2] = {local0, local1};\nfor (int index = 0; index < 3; ++index) {\n");
            for (var local = 0; local < 12; ++local)
                source.Append("if ((index + local").Append(local).Append(") & 1) { total += local").Append(local)
                    .Append("; } else { total -= local").Append(local).Append("; }\n");
            source.Append("if (index == 1) continue; total += values[index & 1]; }\nif (round == 1) break; total += local11; break; }\n");
        }
        source.Append("case ").Append(cases).Append(": { int skipped = ++effects; case ").Append(cases + 1)
            .Append(": total += 9; break; }\ndefault: total = -1000; break;\n} } return total; }\n")
            .Append("int main(void) { int sum = 0; for (int i = 0; i < ").Append(cases)
            .Append("; ++i) sum += run(i, 3); printf(\"%d\\n\",sum); int direct = run(").Append(cases + 1)
            .Append(", 2); printf(\"%d %d\\n\",direct,effects); int ordinary = run(").Append(cases)
            .Append(", 2); printf(\"%d %d\\n\",ordinary,effects); return 0; }\n");
        return source.ToString();
    }
}
