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
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Explicit_program_contexts_isolate_globals_tls_addresses_and_pthread_objects(bool objectLink, bool nested)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-context-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "context.c");
            File.WriteAllText(path, """
                #include <stdint.h>
                #include <errno.h>
                #include <pthread.h>
                int value = 7;
                int array[] = { 11, 13 };
                int *saved = &value;
                _Alignas(64) int aligned = 17;
                _Thread_local int local;
                _Thread_local char buffer[16];
                pthread_mutex_t gate = PTHREAD_MUTEX_INITIALIZER;
                int step(int expected) {
                    if (*saved != expected || array[0] != expected + 4 || aligned != expected + 10) return 1;
                    if ((uintptr_t)&aligned % 64) return 2;
                    if (pthread_mutex_lock(&gate)) return 3;
                    ++value; ++array[0]; ++aligned; ++local; ++buffer[0];
                    if (pthread_mutex_unlock(&gate)) return 4;
                    errno = value;
                    return 0;
                }
                int tls(void) { return local * 100 + buffer[0]; }
                int error(void) { return errno; }
                void *child(void *unused) { return (void*)(intptr_t)(value + tls()); }
                int spawn(void) {
                    pthread_t thread; void *answer;
                    if (pthread_create(&thread, 0, child, 0)) return -1;
                    if (pthread_join(thread, &answer)) return -2;
                    return (int)(intptr_t)answer;
                }
                """);
            if (objectLink)
            {
                var obj = Path.ChangeExtension(path, ".o");
                File.WriteAllText(obj, Compiler.EmitObject(path));
                path = obj;
            }
            var options = new CSharpOutputOptions(NestTypes: nested, Runtime: RuntimeProfile.C, StateContext: true, LiteralPool: true);
            var files = objectLink
                ? Compiler.LinkObjectFiles(new[] { path }, emit: EmitMode.ManagedLib, className: "Api", outputOptions: options, split: SourceSplit.Function)
                : Compiler.EmitCSharpFiles(new[] { path }, emit: EmitMode.ManagedLib, className: "Api", outputOptions: options);
            const string host = """
                public static class ContextProbe {
                    public static string Run() {
                        bool unbound = false;
                        try { Api.tls(); } catch (System.InvalidOperationException) { unbound = true; }
                        if (!unbound) return "unbound";
                        using var a = Api.__DotCcCreateContext();
                        using var b = Api.__DotCcCreateContext();
                        using (a.Enter()) {
                            if (Api.step(7) != 0 || Api.tls() != 101 || Api.error() != 8 || Api.spawn() != 8) return "a";
                            bool active = false;
                            try { a.Dispose(); } catch (System.InvalidOperationException) { active = true; }
                            if (!active) return "active disposal";
                            using (b.Enter()) {
                                if (Api.tls() != 0 || Api.error() != 0 || Api.step(7) != 0) return "b";
                            }
                            if (Api.tls() != 101 || Api.error() != 8) return "restore";
                        }
                        System.GC.Collect(); System.GC.WaitForPendingFinalizers(); System.GC.Collect();
                        string result = "ok";
                        var one = new System.Threading.Thread(() => {
                            using (a.Enter()) { if (Api.tls() != 0 || Api.step(8) != 0 || Api.spawn() != 9) result = "thread a"; }
                        });
                        var two = new System.Threading.Thread(() => {
                            using (b.Enter()) { if (Api.tls() != 0 || Api.step(8) != 0 || Api.spawn() != 9) result = "thread b"; }
                        });
                        one.Start(); two.Start(); one.Join(); two.Join();
                        using (a.Enter()) { if (Api.tls() != 101 || Api.step(9) != 0) return "tls retained"; }
                        a.Dispose();
                        bool retired = false;
                        try { a.Enter(); } catch (System.ObjectDisposedException) { retired = true; }
                        if (!retired) return "retired";
                        using var fresh = Api.__DotCcCreateContext();
                        using (fresh.Enter()) { if (Api.tls() != 0 || Api.step(7) != 0) return "fresh"; }
                        return result;
                    }
                }
                """;
            var compilation = CSharpCompilation.Create("Contexts_" + Guid.NewGuid().ToString("N"),
                files.Select(file => ParseSource(file.Value, path: file.Key)).Append(ParseSource(host)), RuntimeReferences(),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
            using var image = new MemoryStream();
            var result = compilation.Emit(image, cancellationToken: TestContext.Current.CancellationToken);
            result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
            image.Position = 0;
            var assembly = new AssemblyLoadContext("contexts-" + Guid.NewGuid(), isCollectible: false).LoadFromStream(image);
            ((string)assembly.GetType("ContextProbe")!.GetMethod("Run")!.Invoke(null, null)!).ShouldBe("ok");
        }
        finally { Directory.Delete(directory, true); }
    }
}
