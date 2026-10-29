using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class AggregateFactoryTests
{
    // A fixed ceiling catches parameter growth independently of exact helper
    // names, chunk size or whether the implementation uses a builder chain.
    private const int ParameterCeiling = 2;

    [Theory]
    [InlineData("unsigned short", "0x8001", 257)]
    [InlineData("unsigned", "0x80000001u", 257)]
    [InlineData("unsigned long long", "0x8000000000000001ULL", 1024)]
    [InlineData("short", "-123", 257)]
    [InlineData("long long", "(-9223372036854775807LL-1)", 257)]
    [InlineData("float", "-0.0f", 257)]
    [InlineData("double", "0x1p-1074", 257)]
    [InlineData("unsigned long long", "0", 8192)]
    public void Constant_expression_array_factories_have_bounded_signatures(string type, string value, int count)
    {
        string values = string.Join(",", Enumerable.Repeat(value, count));
        WithSource($"struct Value {{ {type} values[{count}]; }}; struct Value make(void) {{ return (struct Value){{{{{values}}}}}; }}", path =>
        {
            var emitted = Compiler.EmitObject(path, outputOptions: new(AggregateData: true));
            AssertBounded(emitted);
            if (value != "0") emitted.ShouldContain("// literal-bytes:");
            Compiler.EmitObject(path, outputOptions: new(AggregateData: true)).ShouldBe(emitted);
        });
    }

    [Fact]
    public void Dynamic_expression_fallback_does_not_grow_a_parameter_per_array_element()
    {
        const int count = 257;
        string values = string.Join(",", Enumerable.Range(0, count).Select(i => $"step({i})"));
        WithSource($"int step(int value); struct Value {{ int values[{count}]; }}; struct Value make(void) {{ return (struct Value){{{{{values}}}}}; }}", path =>
        {
            var emitted = Compiler.EmitObject(path, outputOptions: new(AggregateData: true));
            AssertBounded(emitted);
            // Calls stay typed executable expressions; the compiler must not
            // serialize them or silently replace unknown values with zero.
            Regex.Matches(emitted, @"\bstep\(\d+\)").Count.ShouldBe(count);
        });
    }

    [Fact]
    public void Proven_named_struct_and_bitfield_expression_arrays_have_bounded_factories()
    {
        WithSource("""
            struct RGB { unsigned char r,g,b; };
            struct Packed { unsigned char low:4; unsigned char high:4; };
            struct Value { struct RGB colors[512]; struct Packed packed[8192]; };
            struct Value make(void) {
                return (struct Value){{{1,2,3},[511]={254,253,252}},{{3,10},[8191]={7,12}}};
            }
            """, path => AssertBounded(Compiler.EmitObject(path, outputOptions: new(AggregateData: true))));
    }

    private static void AssertBounded(string emitted)
    {
        // Object fragments contain the authored helper declarations verbatim.
        // Primitive payload getters have no arguments and are not factories.
        var factories = Regex.Matches(emitted,
            @"(?m)^internal static (?:unsafe )?class __DotccAggregate[^\n]*\n\{(?<body>[\s\S]*?)^\}");
        factories.Count.ShouldBeGreaterThan(0, "Expected expression aggregate factory declarations");
        int methods = 0;
        foreach (Match factory in factories)
        {
            foreach (Match method in Regex.Matches(factory.Groups["body"].Value,
                @"(?m)^\s*(?:public|internal|private)\s+static\s+(?:unsafe\s+)?[^\n()]+\((?<parameters>[^\n]*)\)\s*$"))
            {
                ++methods;
                string parameters = method.Groups["parameters"].Value.Trim();
                int count = parameters.Length == 0 ? 0 : 1;
                int depth = 0;
                foreach (char c in parameters)
                {
                    if (c is '<' or '[' or '(') ++depth;
                    else if (c is '>' or ']' or ')') --depth;
                    else if (c == ',' && depth == 0) ++count;
                }
                count.ShouldBeLessThanOrEqualTo(ParameterCeiling, method.Value);
            }
        }
        methods.ShouldBeGreaterThan(0, "Expected factory/helper methods to inspect");
    }

    private static void WithSource(string source, Action<string> action)
    {
        string path = Path.Combine(Path.GetTempPath(), "dotcc-expression-factory-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, source);
        try { action(path); }
        finally { File.Delete(path); }
    }
}
