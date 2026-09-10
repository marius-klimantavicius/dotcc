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
                files.Select(f => CSharpSyntaxTree.ParseText(f.Value, path:f.Key)), RuntimeReferences(),
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
