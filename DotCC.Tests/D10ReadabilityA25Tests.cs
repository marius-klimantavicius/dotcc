using System;
using System.IO;
using System.Linq;
using DotCC;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class D10ReadabilityA25Tests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Small_rows_use_source_members_and_final_storage_aliases(bool link)
    {
        WithSource("""
            struct Nested { signed char tag; unsigned short code; };
            union Selected { long number; void *pointer; };
            struct Row { unsigned short length; unsigned short token; struct Nested nested; union Selected selected; struct Row *next; };
            struct Row rows[2] = {
                {.length=5, .token=65535, .nested={-17,32768}, .selected={.number=-9}, .next=&rows[1]},
                {.length=2, .token=32768, .nested={127,65535}, .selected={.pointer=&rows[0]}, .next=&rows[0]}
            };
            int *error[] = {0}; int *p[] = {0}; int *storage[] = {0};
            int next(int i) { static unsigned short local[] = {31,65535}; return local[i]++; }
            """, link, source =>
        {
            source.ShouldContain("Row* rows_destination");
            source.ShouldContain("rows_destination[0] = new()\n");
            source.ShouldContain("length = 5,\n");
            source.ShouldContain("token = 65535,\n");
            source.ShouldContain("nested = new()\n");
            source.ShouldContain("tag = -17,\n");
            source.ShouldContain("selected = new()\n");
            source.ShouldContain("number = ");
            source.ShouldContain("error_destination[0]");
            source.ShouldContain("p_destination[0]");
            source.ShouldContain("storage_destination[0]");
            source.ShouldNotContain("AsSpan(new ");
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Bitfields_arrays_and_large_containers_keep_in_place_stores(bool link)
    {
        WithSource("""
            struct Bits { unsigned int bit:3; int value; } bits[]={{5,-11},{7,23}};
            struct Buffer { unsigned int data[8201]; long tail; };
            struct Buffer root = {.data={[0]=3,[8200]=91},.tail=19};
            struct Buffer rows[] = {{.data={[0]=7,[8200]=33},.tail=5}};
            struct SmallBuffer { int value; int unused[4]; } small = {.value=1};
            struct SmallBuffer smallrows[] = {{.value=2}};
            struct SmallBits { int value; unsigned int unused:3; } smallbits = {.value=3};
            """, link, source =>
        {
            source.ShouldContain("fixed (Buffer* root_destination");
            source.ShouldContain("(*root_destination).data");
            source.ShouldContain("bits_destination[0].bit = ");
            source.ShouldNotContain("bits_destination[0] = new()");
            source.ShouldNotContain("rows_destination[0] = new()");
            source.ShouldNotContain("new Buffer {");
            source.ShouldContain(" + 8200, 1)");
            source.ShouldContain("fixed (SmallBuffer* small_destination");
            source.ShouldContain("smallrows_destination[0].value = 2");
            source.ShouldContain("fixed (SmallBits* smallbits_destination");
            source.ShouldNotContain("new SmallBuffer {");
            source.ShouldNotContain("new SmallBits {");
            source.ShouldNotContain("AsSpan(new ");
        });
    }

    [Fact]
    public void Narrow_signedness_boundary_and_enum_conversions_remain_explicit()
    {
        WithSource("""
            enum Tag { tag_zero=0, tag_one=1 };
            struct Row { unsigned short small; unsigned char boundary; enum Tag tag; };
            struct Row rows[] = {{65535,(unsigned char)-1,tag_one}};
            """, false, source =>
        {
            source.ShouldContain("small = 65535,");
            source.ShouldContain("boundary = ");
            source.ShouldNotContain("boundary = 255,");
            source.ShouldNotContain("tag = 1,");
        });
    }

    [Theory]
    [InlineData("unsigned char", "byte")]
    [InlineData("signed char", "sbyte")]
    [InlineData("unsigned short", "ushort")]
    [InlineData("short", "short")]
    [InlineData("unsigned int", "uint")]
    [InlineData("int", "int")]
    [InlineData("unsigned long", "ulong")]
    [InlineData("long", "long")]
    [InlineData("float", "float")]
    [InlineData("double", "double")]
    public void Primitive_payloads_use_named_nonallocating_getters(string cType, string csType)
    {
        WithSource(cType + " values[] = {0,1,42};", false, source =>
        {
            source.ShouldContain("ReadOnlySpan<" + csType + ">");
            source.ShouldContain("values_destination");
            source.ShouldNotContain("AsSpan(new ");
        });
    }

    [Fact]
    public void Primitive_properties_respect_4096_element_boundaries_and_zero_gap_offsets()
    {
        string initializers = string.Join(",", Enumerable.Range(1, 4097));
        WithSource("unsigned int dense[]={" + initializers + "}; unsigned int sparse[8201]={[0]=3,[8200]=91};", false, source =>
        {
            source.ShouldContain("dense_destination), 4096)");
            source.ShouldContain("dense_destination) + 4096, 1)");
            source.ShouldContain("sparse_destination) + 8200, 1)");
            source.ShouldNotContain("AsSpan(new ");
        });
    }

    [Theory]
    [InlineData("signed char", "-128", "127", "byte")]
    [InlineData("unsigned char", "0", "255", "byte")]
    [InlineData("short", "-32768", "32767", "ushort")]
    [InlineData("unsigned short", "0", "65535", "ushort")]
    [InlineData("int", "(-2147483647-1)", "2147483647", "uint")]
    [InlineData("unsigned int", "0", "4294967295U", "uint")]
    [InlineData("long", "(-9223372036854775807L-1)", "9223372036854775807L", "ulong")]
    [InlineData("unsigned long", "0", "9223372036854775807UL", "ulong")]
    public void Enum_views_retain_each_supported_underlying_width(string underlying, string minimum, string maximum, string backing)
    {
        WithSource("enum Value : " + underlying + " { value_min=" + minimum + ",value_max=" + maximum
            + " }; enum Value values[]={value_min,value_max};", true, source =>
        {
            source.ShouldContain("MemoryMarshal.Cast<" + backing + ", Value>");
            source.ShouldNotContain("AsSpan(new ");
        }, new CDialect(2023));
    }

    [Fact]
    public void Unsigned_64_bit_enum_storage_preserves_cast_values_above_the_enumerator_range()
    {
        WithSource("enum Value : unsigned long { value_zero=0 }; enum Value values[]={(enum Value)18446744073709551615UL};",
            true, source =>
            {
                source.ShouldContain("MemoryMarshal.Cast<ulong, Value>");
                source.ShouldContain("0xFFFFFFFFFFFFFFFFUL");
            }, new CDialect(2023));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Typed_payload_alias_exists_when_pointer_literal_pooling_is_disabled(bool link)
    {
        WithSource("int values[] = {1,-2};", link, source =>
        {
            source.ShouldContain("using DotCcLiterals = ");
            source.ShouldContain("ReadOnlySpan<uint>");
            source.ShouldNotContain("AsSpan(new ");
        }, pool: false);
    }

    [Theory]
    [InlineData("__int128", "global::System.Int128")]
    [InlineData("unsigned __int128", "global::System.UInt128")]
    [InlineData("_Float128", "Float128")]
    [InlineData("double _Complex", "global::System.Numerics.Complex")]
    public void Wider_representations_use_scalar_stores_without_narrowing_or_array_payload(string cType, string csType)
    {
        WithSource(cType + " wide[] = {1,2};", false, source =>
        {
            source.ShouldContain(csType + "* wide_destination");
            source.ShouldContain("wide_destination[0] = ");
            source.ShouldNotContain("ReadOnlySpan<" + csType + ">");
            source.ShouldNotContain("AsSpan(new ");
            source.ShouldNotContain("new " + csType + "[]");
        });
    }

    private static void WithSource(string input, bool link, Action<string> inspect, CDialect? dialect = null, bool pool = true)
    {
        string path = Path.Combine(Path.GetTempPath(), "dotcc-a25-" + Guid.NewGuid().ToString("N") + ".c");
        string obj = path + ".o";
        try
        {
            File.WriteAllText(path, input);
            var options = new CSharpOutputOptions(Runtime: RuntimeProfile.C, LiteralPool: pool);
            if (link) File.WriteAllText(obj, Compiler.EmitObject(path, dialect: dialect));
            inspect(link ? Compiler.LinkObjects([obj], emit: EmitMode.ManagedLib, outputOptions: options)
                : Compiler.EmitCSharp([path], emit: EmitMode.ManagedLib, outputOptions: options, dialect: dialect));
        }
        finally { File.Delete(path); File.Delete(obj); }
    }
}
