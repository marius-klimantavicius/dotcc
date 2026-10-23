using System;
using System.IO;
using DotCC;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class StaticCallbackOverrideTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Static_managed_overrides_retain_and_return_exact_C_callback_identity(bool objects)
    {
        var folder = Path.Combine(Path.GetTempPath(), "dotcc-static-callback-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var source = Path.Combine(folder, "main.c");
            File.WriteAllText(source, """
                #include <stdio.h>
                typedef int (*callback_t)(void *, int);
                void retain(callback_t callback, void *context);
                callback_t retrieve(void);
                int invoke(int value);
                struct Context { int bias; };
                static struct Context context = { 11 };
                static int callback(void *opaque, int value) { return ((struct Context *)opaque)->bias + value; }
                int main(void) {
                    retain(callback, &context);
                    int identity = retrieve() == &callback;
                    int result = invoke(7);
                    retain(0, 0);
                    printf("%d %d %d\n", identity, result, invoke(3));
                    return 0;
                }
                """);
            var profile = new CPreprocessingOptions(Array.Empty<MacroOverride>(), functionOverrides: new[] {
                new FunctionOverride("retain", new("void", new[] { "callback_t", "void *" }), new("managedMethod", "global::RetainedHost.Retain"), RequireMatch: true),
                new FunctionOverride("retrieve", new("callback_t", Array.Empty<string>()), new("managedMethod", "global::RetainedHost.Retrieve"), RequireMatch: true),
                new FunctionOverride("invoke", new("int", new[] { "int" }), new("managedMethod", "global::RetainedHost.Invoke"), RequireMatch: true),
            });
            string generated;
            if (objects)
            {
                var fragment = Path.Combine(folder, "main.o");
                File.WriteAllText(fragment, Compiler.EmitObject(source, preprocessing: profile));
                generated = Compiler.LinkObjects(new[] { fragment });
            }
            else generated = Compiler.EmitCSharp(new[] { source }, preprocessing: profile);
            generated += """

                public static unsafe class RetainedHost {
                    private static delegate*<void*, int, int> callback;
                    private static void* context;
                    public static void Retain(delegate*<void*, int, int> value, void* opaque) { callback = value; context = opaque; }
                    public static delegate*<void*, int, int> Retrieve() => callback;
                    public static int Invoke(int value) {
                        System.GC.Collect(); System.GC.WaitForPendingFinalizers(); System.GC.Collect();
                        return callback == null ? -1 : callback(context, value);
                    }
                }
                """;
            var result = FixtureRunner.CompileAndRunCapturingExit(generated, Array.Empty<string>());
            result.exit.ShouldBe(0);
            result.stdout.Trim().ShouldBe("1 18 -1");
        }
        finally { Directory.Delete(folder, true); }
    }
}
