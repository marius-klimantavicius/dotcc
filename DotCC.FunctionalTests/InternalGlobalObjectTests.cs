using System;
using System.IO;
using System.Linq;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class InternalGlobalObjectTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Object_link_keeps_internal_storage_distinct_and_external_storage_shared(bool reverse, bool pool)
    {
        string[] sources = [
            """
            static int value = 7;
            static const unsigned char table[3];
            const unsigned char *first_table(void) { return table; }
            static const unsigned char table[3] = {1,2,3};
            static const int one[] = {4,5};
            static const int two[] = {4,5};
            int distinct_arrays(void) { return one != two && one[1] == two[1]; }
            int *first_value(void) { return &value; }
            int shared = 42;
            """,
            """
            static int value = 9;
            static const unsigned char table[3] = {1,2,3};
            const unsigned char *second_table(void) { return table; }
            int *second_value(void) { return &value; }
            extern int shared;
            int read_shared(void) { return shared; }
            """,
            """
            const unsigned char *first_table(void); const unsigned char *second_table(void);
            int *first_value(void); int *second_value(void);
            int read_shared(void);
            int distinct_arrays(void);
            int main(void) {
                if (first_table() == second_table()) return 1;
                if (first_table()[2] != 3 || second_table()[1] != 2) return 2;
                if (first_value() == second_value()) return 3;
                if (*first_value() != 7 || *second_value() != 9) return 4;
                *first_value() = 100;
                if (*second_value() != 9 || read_shared() != 42) return 5;
                if (!distinct_arrays()) return 6;
                return 0;
            }
            """
        ];
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-internal-globals-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var objects = sources.Select((source, index) =>
            {
                var path = Path.Combine(directory, "unit" + index + ".c");
                File.WriteAllText(path, source);
                File.WriteAllText(path + ".o.cs", Compiler.EmitObject(path));
                return path + ".o.cs";
            }).ToArray();
            if (reverse) Array.Reverse(objects);
            var generated = Compiler.LinkObjects(objects, emit: EmitMode.Csproj, outputOptions: new(LiteralPool: pool));
            FixtureRunner.CompileAndRunCapturingExit(generated, []).exit.ShouldBe(0);
        }
        finally { Directory.Delete(directory, true); }
    }
}
