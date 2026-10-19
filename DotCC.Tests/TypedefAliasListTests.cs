using System;
using System.IO;
using Xunit;

namespace DotCC.Tests;

public sealed class TypedefAliasListTests
{
    [Theory]
    [InlineData("typedef struct Pair { unsigned long low, high; } First, Second; Second f(First x) { return x; }")]
    [InlineData("typedef struct { int value; } First, Second; Second f(First x) { return x; }")]
    [InlineData("typedef union Value { long wide; int narrow; } First, Second; Second f(First x) { return x; }")]
    [InlineData("typedef enum Kind { A=3 } First, Second; Second f(First x) { return x; }")]
    [InlineData("typedef int First, Second, *Pointer, Rows[2][3]; Rows rows; Second f(Pointer p) { return *p+sizeof(rows); }")]
    [InlineData("typedef int *Pointer, Scalar; Scalar f(Pointer p) { return *p; }")]
    [InlineData("typedef int *Pointer; typedef Pointer First, Second; int f(Second p) { return *p; }")]
    [InlineData("typedef int Scalar, Values[sizeof(Scalar)]; int f(void) { return sizeof(Values); }")]
    [InlineData("typedef int Outer; int f(void) { { typedef long Outer, Second; Second x=4; } Outer x=3; return x; }")]
    public void Every_alias_in_a_declaration_is_bound_to_its_own_type(string source)
    {
        var path=Path.Combine(Path.GetTempPath(), $"dotcc-typedef-alias-list-{Guid.NewGuid():N}.c");
        File.WriteAllText(path,source);
        try { Assert.NotEmpty(Compiler.EmitObject(path)); }
        finally { File.Delete(path); }
    }
}
