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
    [InlineData(false, SourceSplit.Size)]
    [InlineData(true, SourceSplit.None)]
    [InlineData(true, SourceSplit.Function)]
    [InlineData(true, SourceSplit.Size)]
    public void Overrides_and_endian_intrinsic_execute_through_source_and_objects(bool link, SourceSplit split)
    {
        var dir = Path.Combine(Path.GetTempPath(), "dotcc-override-run-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var path = Path.Combine(dir, "probe.c");
            File.WriteAllText(path, """
                #define LITTLE (*(char *)&one == 1)
                #define BIG (*(char *)&one == 0)
                #define NATIVE (BIG ? 3 : 2)
                #define X(n) do_call(n, 5)
                #define S(n) n
                #define CAT(n) n
                #define V(n,...) n
                static int one = 1;
                static int do_call(int n, int x) { return n + x; }
                static int take(_Bool b, int n) { return b + n; }
                int little(void) { return LITTLE; }
                int big(void) { return BIG; }
                int native(void) { return NATIVE; }
                int operations(void) {
                    int n=3; int x=X(n++);
                    int suffix=11;
                    char *s=S(hello world);
                    return x * 1000 + n * 100 + CAT(suf) * 10 + V(2,3) + (s[5] == ' ');
                }
                int conversions(void) {
                    _Bool b=LITTLE;
                    _Bool *p=&b; *p=BIG;
                    return take(LITTLE, (unsigned char)LITTLE) + (int)b +
                        (LITTLE ? BIG : LITTLE) + (BIG ? LITTLE : BIG) +
                        (LITTLE && !BIG) + (LITTLE || BIG) + (LITTLE == !BIG) +
                        (LITTLE < 2) + (LITTLE << 2);
                }
                int polarity(int b) { return (b ? 1 : 0) + (!b ? 10 : 0); }
                """);
            using var report = new StringWriter();
            var config = new CPreprocessingOptions(new[] {
                new MacroOverride("LITTLE", "(__dotcc_is_little_endian())", Exact:"(*(char *)&one == 1)", RequireMatch:true),
                new MacroOverride("BIG", "(!__dotcc_is_little_endian())", Exact:"(*(char *)&one == 0)", RequireMatch:true),
                new MacroOverride("X", "do_call(${__dotcc_n}, ${num})", Pattern:@"do_call\(n, (?<num>\d+)\)"),
                new MacroOverride("S", "# ${__dotcc_n}"),
                new MacroOverride("CAT", "${__dotcc_n} ## fix"),
                new MacroOverride("V", "do_call(${__dotcc_n}, __VA_ARGS__)")
            }, report:report);
            if (link)
            {
                var obj=Path.Combine(dir,"probe.o");
                File.WriteAllText(obj,Compiler.EmitObject(path,preprocessing:config));
                path=obj;
            }
            var files = link
                ? Compiler.LinkObjectFiles(new[]{path}, emit:EmitMode.ManagedLib, className:"Api", namespaceName:"Probe.Endian", split:split, overrideReport:report)
                : Compiler.EmitCSharpFiles(new[]{path}, emit:EmitMode.ManagedLib, className:"Api", namespaceName:"Probe.Endian", split:split, preprocessing:config);
            var source=string.Join("\n",files.Values);
            source.ShouldContain("global::System.BitConverter.IsLittleEndian");
            source.ShouldNotContain("AsPointer(ref one)");
            var compilation=CSharpCompilation.Create("Overrides"+Guid.NewGuid().ToString("N"),
                files.Select(f=>ParseSource(f.Value,path:f.Key)), RuntimeReferences(),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,allowUnsafe:true));
            using var image=new MemoryStream();
            var result=compilation.Emit(image,cancellationToken:TestContext.Current.CancellationToken);
            result.Success.ShouldBeTrue(string.Join("\n",result.Diagnostics.Where(d=>d.Severity==DiagnosticSeverity.Error)));
            image.Position=0;
            var assembly=new AssemblyLoadContext("overrides"+Guid.NewGuid(),isCollectible:false).LoadFromStream(image);
            var api=assembly.GetType("Probe.Endian.Api")!;
            int Run(string method, params object[] args) => (int)api.GetMethod(method)!.Invoke(null,args)!;
            int little=BitConverter.IsLittleEndian ? 1 : 0;
            Run("little").ShouldBe(little);
            Run("big").ShouldBe(1-little);
            Run("native").ShouldBe(little == 1 ? 2 : 3);
            Run("operations").ShouldBe(8516);
            Run("conversions").ShouldBe(4+6*little);
            Run("polarity",0).ShouldBe(10);
            Run("polarity",1).ShouldBe(1);
            foreach(var name in new[]{"LITTLE","BIG","NATIVE"}) api.GetField(name).ShouldBeNull();
            if(link) report.ToString().ShouldContain(config.ProfileHash);
        }
        finally { Directory.Delete(dir,true); }
    }
}
