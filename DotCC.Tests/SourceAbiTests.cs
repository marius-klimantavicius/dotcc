using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotCC.Ir;
using Xunit;

namespace DotCC.Tests;

public sealed class SourceAbiTests
{
    private static CPreprocessingOptions StorageProfile() => new(Array.Empty<MacroOverride>(),
        sourceAbi: new(CLongDoubleRepresentation.X87Storage16));

    [Fact]
    public void Unknown_public_representation_value_is_rejected()
    {
        var error = Assert.Throws<CompileException>(() => new CPreprocessingOptions(Array.Empty<MacroOverride>(),
            sourceAbi: new((CLongDoubleRepresentation)999)));
        Assert.Contains("unsupported source ABI", error.Message);
    }

    [Fact]
    public void Explicit_profile_changes_layout_without_changing_the_default()
    {
        Emit("_Static_assert(sizeof(long double)==8, \"default\");", null);
        Emit("_Static_assert(sizeof(long double)==16 && _Alignof(long double)==16, \"storage\");", StorageProfile());
        Emit("_Static_assert(sizeof(long double)==8, \"still default\");", null);
        Assert.Equal(8, CType.LongDouble.SizeOf);
    }

    [Fact]
    public async Task Concurrent_requests_keep_independent_data_models()
    {
        await Task.WhenAll(Enumerable.Range(0, 12).Select(index => Task.Run(() =>
            Emit($"_Static_assert(sizeof(long double)=={(index % 2 == 0 ? 8 : 16)}, \"request\");",
                index % 2 == 0 ? null : StorageProfile()), TestContext.Current.CancellationToken)));
        Assert.Equal(8, CType.LongDouble.SizeOf);
    }

    [Fact]
    public void Profile_json_and_reportless_copy_retain_abi_and_identity()
    {
        var path = Path.Combine(Path.GetTempPath(), $"dotcc-source-abi-{Guid.NewGuid():N}.json");
        File.WriteAllText(path, """{"version":1,"sourceAbi":{"longDouble":"x87-storage16"}}""");
        try
        {
            var profile = CPreprocessingOptions.Load(path);
            Assert.Equal(CLongDoubleRepresentation.X87Storage16, profile.SourceAbi.LongDouble);
            Assert.Equal(profile.SourceAbi, profile.WithoutReport().SourceAbi);
            Assert.Equal(profile.ProfileHash, profile.WithoutReport().ProfileHash);
            Assert.NotEqual(new CPreprocessingOptions(Array.Empty<MacroOverride>()).ProfileHash, profile.ProfileHash);
            Emit("_Static_assert(sizeof(long double)==16, \"JSON profile\");", profile);
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData("long double value; return (int)value;")]
    [InlineData("long double value; return value + value;")]
    [InlineData("long double value; return value == value;")]
    [InlineData("long double value; value++;")]
    [InlineData("long double value; value = 1;")]
    [InlineData("long double value = 1.0;")]
    [InlineData("long double value; if (value) return 1;")]
    [InlineData("long double value; return !value;")]
    [InlineData("long double value; return value ? 1 : 0;")]
    [InlineData("long double value; while (value) return 1;")]
    [InlineData("long double value; for (;value;) return 1;")]
    [InlineData("long double value; value += value;")]
    [InlineData("long double value; accept(value + value);")]
    [InlineData("long double value; double narrowed = value;")]
    [InlineData("long double value = 1.0L;")]
    [InlineData("accept(1);")]
    [InlineData("long double _Complex value;")]
    public void Numeric_uses_of_storage_only_long_double_fail_explicitly(string body)
    {
        var error = Assert.ThrowsAny<CompileException>(() => Emit(
            "void accept(long double value) { } int exercise(void) { " + body + " return 0; }", StorageProfile()));
        Assert.Contains("storage/layout only", error.Message);
    }

    [Theory]
    [InlineData("long double exercise(void *h) { long double (*fn)(void)=(long double (*)(void))dlsym(h,\"value\"); return fn(); }")]
    [InlineData("void exercise(void *h, long double value) { void (*fn)(long double)=(void (*)(long double))dlsym(h,\"consume\"); fn(value); }")]
    [InlineData("long double exercise(void *h) { return ((long double (*)(void))dlsym(h,\"value\"))(); }")]
    public void Native_storage_scalar_calls_reject_the_foreign_floating_abi(string source)
    {
        var error = Assert.ThrowsAny<CompileException>(() => Emit("#include <dlfcn.h>\n" + source, StorageProfile()));
        Assert.Contains("native x87 scalar calling ABI", error.Message);
    }

    [Theory]
    [InlineData("#include <stdio.h>\nint exercise(long double value) { return printf(\"%Lf\",value); }")]
    [InlineData("void sink(int tag, ...) { } void exercise(long double value) { sink(0,value); }")]
    [InlineData("void sink(int tag, ...) { } void exercise(long double value) { void (*fn)(int, ...)=sink; fn(0,value); }")]
    [InlineData("void sink(); void exercise(long double value) { sink(value); }")]
    [InlineData("void exercise(long double value) { unknown_sink(value); }")]
    public void Variadic_or_unprototyped_storage_values_fail_before_generated_compilation(string source)
    {
        var error = Assert.ThrowsAny<CompileException>(() => Emit(source, StorageProfile()));
        Assert.Contains("variadic or unprototyped scalar arguments", error.Message);
    }

    [Fact]
    public void Va_arg_cannot_materialize_a_storage_scalar_as_a_number()
    {
        var error = Assert.ThrowsAny<CompileException>(() => Emit("""
            #include <stdarg.h>
            void exercise(int tag, ...) {
                va_list ap;
                va_start(ap,tag);
                long double value=va_arg(ap,long double);
                va_end(ap);
            }
            """, StorageProfile()));
        Assert.Contains("storage/layout only", error.Message);
    }

    [Theory]
    [InlineData("#include <dlfcn.h>\nlong double *exercise(void *h, long double *p) { long double *(*fn)(long double *)=(long double *(*)(long double *))dlsym(h,\"storage\"); return fn(p); }")]
    [InlineData("static long double identity(long double value) { return value; } long double exercise(long double value) { long double (*fn)(long double)=identity; return fn(identity(value)); }")]
    [InlineData("#include <dlfcn.h>\nvoid exercise(void *h) { long double (*fn)(void)=(long double (*)(void))dlsym(h,\"value\"); (void)fn; }")]
    [InlineData("void sink(long double fixed, ...) { } void exercise(long double value) { sink(value,1); }")]
    public void Raw_pointer_and_known_managed_storage_copy_boundaries_remain_available(string source)
        => Emit(source, StorageProfile());

    private static void Emit(string text, CPreprocessingOptions? profile)
    {
        var path = Path.Combine(Path.GetTempPath(), $"dotcc-source-abi-{Guid.NewGuid():N}.c");
        File.WriteAllText(path, text);
        try { Assert.NotEmpty(Compiler.EmitObject(path, preprocessing: profile)); }
        finally { File.Delete(path); }
    }
}
