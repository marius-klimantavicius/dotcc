using System;
using System.IO;
using Xunit;

namespace DotCC.Tests;

public sealed class ValkeyDeclaratorTests
{
    [Theory]
    [InlineData("typedef struct list { int count; } list; void clear(list *list) { list->count=0; } list after;")]
    [InlineData("struct polygon { double (*points)[2]; }; int size(void) { struct polygon p; return sizeof(*p.points); }")]
    [InlineData("extern const char table[][4]; int value(void) { return table[1][3]; }")]
    [InlineData("typedef struct dict { int count; } dict; void run(dict *d, void(callback)(dict *)) { callback(d); }")]
    public void Standard_declarator_shapes_lower_without_parse_failure(string source)
    {
        var path = Path.Combine(Path.GetTempPath(), $"dotcc-valkey-declarator-{Guid.NewGuid():N}.c");
        File.WriteAllText(path, source);
        try { Assert.NotEmpty(Compiler.EmitObject(path)); }
        finally { File.Delete(path); }
    }
}
