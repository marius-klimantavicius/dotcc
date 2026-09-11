#nullable enable
using System;
using System.IO;
using DotCC;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class DesignatedMemberBraceTests
{
    [Theory]
    [InlineData("", "value = (struct hello){.extensions = {{65535}}};")]
    [InlineData("struct hello global = {.extensions = {{65535}}};", "value = global;")]
    [InlineData("", "static struct hello saved = {.extensions = {{65535}}}; value = saved;")]
    [InlineData("", "value = (struct hello){.pair = {.b = {7}}, .matrix = {{1}, {2,3}}};")]
    [InlineData("", "value = (struct hello){.extensions = {}, .pair = {}, .matrix = {}};")]
    public void Member_braces_bind_against_declared_array_and_aggregate_types(string globals, string body)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-designated-brace-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, $$"""
            struct extension { unsigned short type; void *data; };
            struct pair { int a; int b; };
            struct hello { struct extension extensions[3]; struct pair pair; int matrix[2][2]; };
            {{globals}}
            int main(void) { struct hello value; {{body}} return value.pair.a; }
            """);
        try { Compiler.EmitCSharp(new[] { path }, dialect: CDialect.Parse("c23")).ShouldContain("extensions"); }
        finally { File.Delete(path); }
    }
}
