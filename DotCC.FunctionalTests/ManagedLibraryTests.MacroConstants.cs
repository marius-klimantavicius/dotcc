using System;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed partial class ManagedLibraryTests
{
    [Theory]
    [InlineData(false, false, SourceSplit.None, true)]
    [InlineData(false, true, SourceSplit.Function, false)]
    [InlineData(true, false, SourceSplit.Size, true)]
    [InlineData(true, true, SourceSplit.Size, false)]
    public void Macro_exports_compile_and_match_C_values(bool link, bool nested, SourceSplit split, bool selected)
    {
        var dir = Path.Combine(Path.GetTempPath(), "dotcc-selected-macros-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var first = Path.Combine(dir, "first.c");
            var second = Path.Combine(dir, "second.c");
            File.WriteAllText(first, """
                #include <errno.h>
                typedef unsigned int Status;
                typedef enum Mode { First = 1, Last = 7 } Mode;
                #define STATUS(x) ((Status)(x))
                #define QUIC_STATUS_PENDING STATUS(-2)
                #define QUIC_STATUS_INVALID_PARAMETER STATUS(EINVAL)
                #define API_INVALID ((void*)-1)
                #define API_MODE ((Mode)Last)
                #define API_ENUM_HIGH ((Mode)0x80000000)
                #define API_BOOL ((_Bool)256)
                #define API_FLOAT_BOOL ((_Bool)0.25)
                #define API_SHORT_CIRCUIT (0 && (1 / 0))
                #define API_FLOAT_CONDITION (0.25 ? 7 : (1 / 0))
                #define API_CONFLICT STATUS(1)
                #define API_SAME STATUS(42)
                #define MAKE(name) static int name(int x) { return x + 1; }
                MAKE(helper)
                MAKE(callback)
                int macro_check(void) { int (*fn)(int) = callback; return helper(20) + fn(20); }
                unsigned int pending(void) { return QUIC_STATUS_PENDING; }
                unsigned int invalid(void) { return QUIC_STATUS_INVALID_PARAMETER; }
                void *sentinel(void) { return API_INVALID; }
                int boolean(void) { return API_BOOL; }
                Mode negative(void) { return (Mode)-1; }
                """);
            File.WriteAllText(second, """
                typedef unsigned int Status;
                #define STATUS(x) ((Status)(x))
                #define API_CONFLICT STATUS(2)
                #define API_SAME STATUS(42)
                int other(void) { return 0; }
                """);
            var preprocessing = new CPreprocessingOptions(Array.Empty<MacroOverride>(), emitDefines: selected ? new[] { "QUIC_STATUS_*", "API_*" } : Array.Empty<string>());
            var paths = new[] { first, second };
            if (link)
                paths = paths.Select(p => { var obj = Path.ChangeExtension(p, ".o"); File.WriteAllText(obj, Compiler.EmitObject(p, preprocessing: preprocessing)); return obj; }).ToArray();
            var options = new CSharpOutputOptions(NestTypes: nested, Runtime: RuntimeProfile.C);
            var files = link
                ? Compiler.LinkObjectFiles(paths, emit: EmitMode.ManagedLib, className: "Api", namespaceName: "Example", split: split, outputOptions: options)
                : Compiler.EmitCSharpFiles(paths, emit: EmitMode.ManagedLib, className: "Api", namespaceName: "Example", split: split, preprocessing: preprocessing, outputOptions: options);
            const string consumer = """
                using Example;
                public static unsafe class Consumer {
                    public static bool Check() {
                        const uint pending = Api.QUIC_STATUS_PENDING;
                        const uint invalid = Api.QUIC_STATUS_INVALID_PARAMETER;
                        return pending == 4294967294U && pending == Api.pending()
                            && invalid == 22 && invalid == Api.invalid()
                            && Api.API_INVALID == (void*)-1 && Api.API_INVALID == Api.sentinel()
                            && (int)Api.API_MODE == 7 && (int)Api.API_BOOL == 1 && Api.boolean() == 1
                            && (int)Api.API_ENUM_HIGH == -2147483648 && (int)Api.negative() == -1
                            && (int)Api.API_FLOAT_BOOL == 1 && Api.API_SAME == 42 && Api.macro_check() == 42
                            && Api.API_SHORT_CIRCUIT == 0 && Api.API_FLOAT_CONDITION == 7;
                    }
                }
                """;
            var trees = files.Select(f => ParseSource(f.Value, path: f.Key))
                .Append(ParseSource(consumer));
            var compilation = CSharpCompilation.Create("SelectedMacros" + Guid.NewGuid().ToString("N"), trees,
                RuntimeReferences(), new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
            using var image = new MemoryStream();
            var result = compilation.Emit(image, cancellationToken: TestContext.Current.CancellationToken);
            result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
            image.Position = 0;
            var assembly = new AssemblyLoadContext("selected-macros" + Guid.NewGuid(), isCollectible: false).LoadFromStream(image);
            assembly.GetType("Consumer")!.GetMethod("Check")!.Invoke(null, null).ShouldBe(true);
            var api = assembly.GetType("Example.Api")!;
            api.GetField("API_INVALID")!.IsInitOnly.ShouldBeTrue();
            api.GetField("API_BOOL")!.IsInitOnly.ShouldBeTrue();
            api.GetField("API_MODE")!.IsLiteral.ShouldBeTrue();
            api.GetField("API_CONFLICT").ShouldBeNull();
            var pointers = assembly.GetType(nested ? "Example.Api+ApiFunctionPointers" : "Example.ApiFunctionPointers")!;
            pointers.GetProperties().Any(f => f.Name == "helper" || f.Name.StartsWith("helper__unit_", StringComparison.Ordinal)).ShouldBeFalse();
            pointers.GetProperties().Any(f => f.Name == "callback" || f.Name.StartsWith("callback__unit_", StringComparison.Ordinal)).ShouldBeTrue();
        }
        finally { Directory.Delete(dir, true); }
    }

    [Theory]
    [InlineData(false, SourceSplit.None)]
    [InlineData(false, SourceSplit.Function)]
    [InlineData(true, SourceSplit.Size)]
    public void Constant_macros_are_public_typed_fields_without_changing_C_code(bool link, SourceSplit split)
    {
        var dir = Path.Combine(Path.GetTempPath(), "dotcc-macros-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var first = Path.Combine(dir, "first.c");
            var second = Path.Combine(dir, "second.c");
            File.WriteAllText(first, """
                #define SQLITE_CHECKPOINT_TRUNCATE 3
                #define MESSAGE "hello\n" "SQLite \"C#\""
                #define OCTAL 077
                #define HEX 0xffffffffU
                #define WIDE 0xffffffffffffffffULL
                #define NEGATIVE (-42)
                #define WRAPPED ((0xffffffffU + 1U) == 0)
                #define DIVIDE_UNSIGNED (-1 / 2U)
                #define CONDITIONAL (WRAPPED ? 42 : -1)
                #define LINE __LINE__
                #define LINE_ALIAS LINE
                #define FILE_NAME __FILE__
                #define SHIFT ((1U << 31) | 7)
                #define ALIAS SQLITE_CHECKPOINT_TRUNCATE
                #define DECIMAL 1.25f
                #define CAST ((unsigned short)65537)
                #define COMPARE (7 > 3)
                #define EMPTY
                #define CALL(x) ((x)+1)
                #define RUNTIME missing()
                #define SELF SELF
                #define REMOVED 99
                #undef REMOVED
                #define CONFLICT 1
                #define SAME (1 + 2)
                #define BadInjection 2; int injected=9
                #define method 7
                #undef method
                int method(void) { return SQLITE_CHECKPOINT_TRUNCATE + ALIAS; }
                #define method 9
                """);
            File.WriteAllText(second, """
                #define CONFLICT 2
                #define SAME 3
                int other(void) { return 0; }
                """);
            var paths = new[] { first, second };
            if (link)
                paths = paths.Select(p => { var obj=Path.ChangeExtension(p,".cs"); File.WriteAllText(obj,Compiler.EmitObject(p)); return obj; }).ToArray();
            var files = link
                ? Compiler.LinkObjectFiles(paths, emit: EmitMode.ManagedLib, className: "Api", namespaceName: "Example", split: split)
                : Compiler.EmitCSharpFiles(paths, emit: EmitMode.ManagedLib, className: "Api", namespaceName: "Example", split: split);
            var compilation = CSharpCompilation.Create("Macros" + Guid.NewGuid().ToString("N"),
                files.Select(f => ParseSource(f.Value, path:f.Key)), RuntimeReferences(),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe:true));
            using var image=new MemoryStream();
            var result=compilation.Emit(image, cancellationToken:TestContext.Current.CancellationToken);
            result.Success.ShouldBeTrue(string.Join("\n",result.Diagnostics.Where(d=>d.Severity==DiagnosticSeverity.Error)));
            image.Position=0;
            var assembly=new AssemblyLoadContext("macros"+Guid.NewGuid(),isCollectible:false).LoadFromStream(image);
            var api=assembly.GetType("Example.Api")!;
            object Value(string name) { var f=api.GetField(name); f.ShouldNotBeNull(name); f.IsLiteral.ShouldBeTrue(); return f.GetRawConstantValue()!; }
            Value("SQLITE_CHECKPOINT_TRUNCATE").ShouldBe(3);
            Value("MESSAGE").ShouldBe("hello\nSQLite \"C#\"");
            Value("OCTAL").ShouldBe(63);
            Value("HEX").ShouldBe(uint.MaxValue);
            Value("WIDE").ShouldBe(ulong.MaxValue);
            Value("NEGATIVE").ShouldBe(-42);
            Value("WRAPPED").ShouldBe(1);
            Value("DIVIDE_UNSIGNED").ShouldBe(2147483647U);
            Value("CONDITIONAL").ShouldBe(42);
            Value("SHIFT").ShouldBe(0x80000007U);
            Value("ALIAS").ShouldBe(3);
            Value("DECIMAL").ShouldBe(1.25f);
            Value("CAST").ShouldBe((ushort)1);
            Value("COMPARE").ShouldBe(1);
            Value("SAME").ShouldBe(3);
            foreach(var name in new[]{"EMPTY","CALL","RUNTIME","SELF","REMOVED","CONFLICT","BadInjection","method","LINE","LINE_ALIAS","FILE_NAME"})
                api.GetField(name).ShouldBeNull(name);
            api.GetMethod("method")!.Invoke(null,null).ShouldBe(6);
        }
        finally { Directory.Delete(dir,true); }
    }
}
