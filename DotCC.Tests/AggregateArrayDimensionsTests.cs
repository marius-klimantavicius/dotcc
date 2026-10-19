using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class AggregateArrayDimensionsTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Aggregate_copy_expressions_keep_the_brace_elision_path(bool objects)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-aggregate-copies-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, "struct Entry { int first; int second; }; "
            + "int read(void) { struct Entry value = {7,8}; "
            + "struct Entry table[2][2] = {value,value,value}; return table[1][0].second; }");
        try
        {
            if (objects)
            {
                File.WriteAllText(path + ".o", Compiler.EmitObject(path));
                Compiler.LinkObjects([path + ".o"], emit: EmitMode.ManagedLib).ShouldContain("table");
            }
            else Compiler.EmitCSharp([path], emit: EmitMode.ManagedLib).ShouldContain("table");
        }
        finally { File.Delete(path); File.Delete(path + ".o"); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Nested_aggregate_arrays_keep_each_array_dimension(bool objects)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-aggregate-dimensions-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, "struct Entry { int first; int second; }; "
            + "const struct Entry table[2][3] = {{{1,2}},{{3,4},{5,6}}}; "
            + "int read(void) { return table[1][1].second; }");
        try
        {
            if (objects)
            {
                File.WriteAllText(path + ".o", Compiler.EmitObject(path));
                Compiler.LinkObjects([path + ".o"], emit: EmitMode.ManagedLib).ShouldContain("table");
            }
            else
            {
                Compiler.EmitCSharp([path], emit: EmitMode.ManagedLib).ShouldContain("table");
            }
        }
        finally { File.Delete(path); File.Delete(path + ".o"); }
    }

    [Theory]
    [InlineData("{{{1,2},{3,4},{5,6}}}")]
    [InlineData("{{{1,2}},{{3,4}},{{5,6}}}")]
    [InlineData("{{1},{2},{3},{4}}")]
    public void Nested_aggregate_array_checks_each_dimension(string initializer)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-aggregate-bounds-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, "struct Entry { int first; int second; }; "
            + "struct Entry table[2][2] = " + initializer + ";");
        try
        {
            Should.Throw<CompileException>(() => Compiler.EmitCSharp([path]))
                .Message.ShouldContain("too many initializers for aggregate array");
        }
        finally { File.Delete(path); }
    }
}
