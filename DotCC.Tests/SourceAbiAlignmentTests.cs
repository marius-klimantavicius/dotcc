using System;
using System.IO;
using System.Linq;
using DotCC.Layout;
using Xunit;

namespace DotCC.Tests;

public sealed class SourceAbiAlignmentTests
{
    [Theory]
    [InlineData("typedef long double Value __attribute__((aligned(1)));", 1, 1, 17, 18)]
    [InlineData("typedef long double Value __attribute__((aligned(32)));", 32, 32, 48, 64)]
    [InlineData("typedef __attribute__((aligned(1))) long double Value;", 1, 1, 17, 18)]
    [InlineData("typedef __attribute__((aligned(32))) long double Value;", 32, 32, 48, 64)]
    [InlineData("typedef long double __attribute__((aligned(1))) Value;", 1, 1, 17, 18)]
    [InlineData("typedef long double __attribute__((aligned(32))) Value;", 32, 32, 48, 64)]
    [InlineData("typedef long double Base; typedef Base Value __attribute__((__aligned__(1)));", 1, 1, 17, 18)]
    [InlineData("typedef long double Base; typedef Base Value __attribute__((__aligned__(32)));", 32, 32, 48, 64)]
    public void X87_storage_typedef_alignment_preserves_native_scalar_and_record_layout(
        string declaration, int alignment, int valueOffset, int tailOffset, int recordSize)
    {
        // Expected values come from the GCC LP64 storage fixture. An over-aligned
        // scalar still occupies sixteen bytes; arrays of that typedef are invalid C.
        var source = $$"""
            #include <stddef.h>
            {{declaration}}
            typedef const Value Qualified;
            struct Record { char lead; Value value; char tail; };
            _Static_assert(sizeof(long double) == 16 && _Alignof(long double) == 16, "natural scalar");
            _Static_assert(sizeof(Value) == 16 && _Alignof(Value) == {{alignment}}, "aligned scalar");
            _Static_assert(sizeof(Qualified) == 16 && _Alignof(Qualified) == {{alignment}}, "qualified alias");
            _Static_assert(offsetof(struct Record, value) == {{valueOffset}}, "value offset");
            _Static_assert(offsetof(struct Record, tail) == {{tailOffset}}, "tail offset");
            _Static_assert(sizeof(struct Record) == {{recordSize}} && _Alignof(struct Record) == {{alignment}}, "record layout");
            int main(void) { struct Record record; return (int)sizeof(record); }
            """;
        var profile = new CPreprocessingOptions(Array.Empty<MacroOverride>(),
            sourceAbi: new(CLongDoubleRepresentation.X87Storage16));
        var path = Path.Combine(Path.GetTempPath(), "dotcc-source-abi-alignment-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, source);
        try
        {
            var emitted = Compiler.EmitObject(path, preprocessing: profile);
            var documents = OffsetDocument.ReadSource(emitted).ToArray();
            Assert.NotEmpty(documents);
            foreach (var document in documents)
            {
                var value = document.Aggregates["Record"].Fields.Single(field => field.Name == "value");
                Assert.Equal("p:16:" + alignment, value.Type);
                var model = new OffsetLayoutModel(name => document.Aggregates[name]);
                Assert.Equal(16, model.Type(value.Type).Size);
                Assert.Equal(alignment, model.Type(value.Type).Alignment);
                Assert.Equal(recordSize, model.Aggregate("Record").Size);
                Assert.Equal(alignment, model.Aggregate("Record").Alignment);
                Assert.Equal(0, model.Offset("Record", new[] { "lead" }));
                Assert.Equal(valueOffset, model.Offset("Record", new[] { "value" }));
                Assert.Equal(tailOffset, model.Offset("Record", new[] { "tail" }));
            }
        }
        finally { File.Delete(path); }
    }
}
