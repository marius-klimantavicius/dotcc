using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class PostDefinitionAggregateAttributeExecutionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Packed_post_definition_keeps_nested_storage_offsets_and_array_stride(bool objects)
    {
        const string source = """
            #include <stddef.h>
            #include <stdio.h>
            #define packed_definition(definition) definition __attribute__((packed))
            packed_definition(struct Header { short a; short b; short c; short d; short e; });
            struct Outer { char before; struct Header header; char after; };
            _Static_assert(sizeof(struct Header)==10 && _Alignof(struct Header)==1, "header");
            _Static_assert(offsetof(struct Header,e)==8, "last field");
            _Static_assert(sizeof(struct Outer)==12 && offsetof(struct Outer,header)==1 && offsetof(struct Outer,after)==11, "nested");
            int main(void) {
                union Local { char byte; long word; } __attribute__((packed));
                struct Outer rows[2] = {{1,{2,3,4,5,6},7},{8,{9,10,11,12,13},14}};
                struct Header *p = &rows[1].header;
                unsigned char *bytes = (unsigned char *)rows;
                p->e = 42;
                printf("packed %lu %lu %lu %d %d %d %d\n", sizeof(struct Header), _Alignof(struct Header), sizeof(union Local),
                    (int)((char *)&rows[1]-(char *)&rows[0]), rows[0].after, rows[1].header.e, bytes[21]);
                return rows[0].after != 7 || p->e != 42 || bytes[21] != 42 || sizeof(union Local) != 8;
            }
            """;
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-post-aggregate-run-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "main.c");
            File.WriteAllText(path, source);
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "main.cs");
                File.WriteAllText(fragment, Compiler.EmitObject(path));
                emitted = Compiler.LinkObjects(new[] { fragment });
            }
            else emitted = Compiler.EmitCSharp(new[] { path });
            var result = FixtureRunner.CompileAndRunCapturingExit(emitted, Array.Empty<string>());
            result.exit.ShouldBe(0);
            result.stdout.Trim().ShouldBe("packed 10 1 8 12 7 42 42");
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
