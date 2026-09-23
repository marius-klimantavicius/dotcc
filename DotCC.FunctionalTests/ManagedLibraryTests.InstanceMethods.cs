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
    public void Instance_methods_and_explicit_callbacks_own_globals_and_tls(bool objectLink, bool nested)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-instance-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "instance.c");
            File.WriteAllText(path, InstanceSource);
            var options = new CSharpOutputOptions(NestTypes: nested, Runtime: RuntimeProfile.C,
                InstanceMethods: true, LiteralPool: true, DeduplicateInline: true);
            if (objectLink)
            {
                var obj = Path.ChangeExtension(path, ".o");
                File.WriteAllText(obj, Compiler.EmitObject(path, outputOptions: new(InstanceMethods: true)));
                path = obj;
            }
            var files = objectLink
                ? Compiler.LinkObjectFiles(new[] { path }, emit: EmitMode.ManagedLib, className: "Api", outputOptions: options, split: SourceSplit.Function)
                : Compiler.EmitCSharpFiles(new[] { path }, emit: EmitMode.ManagedLib, className: "Api", outputOptions: options);
            var compilation = CSharpCompilation.Create("Instances_" + Guid.NewGuid().ToString("N"),
                files.Select(file => ParseSource(file.Value, path: file.Key)).Append(ParseSource(InstanceHost.Replace("POINTERS", nested ? "Api.ApiFunctionPointers" : "ApiFunctionPointers"))),
                RuntimeReferences(), new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
            using var image = new MemoryStream();
            var result = compilation.Emit(image, cancellationToken: TestContext.Current.CancellationToken);
            result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
            image.Position = 0;
            var assembly = new AssemblyLoadContext("instances-" + Guid.NewGuid(), isCollectible: false).LoadFromStream(image);
            ((string)assembly.GetType("InstanceProbe")!.GetMethod("Run")!.Invoke(null, null)!).ShouldBe("ok");
        }
        finally { Directory.Delete(directory, true); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Instance_callbacks_cross_objects_variadics_and_explicit_managed_boundaries(bool objectLink)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-instance-boundary-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var one = Path.Combine(directory, "one.c");
            var two = Path.Combine(directory, "two.c");
            File.WriteAllText(one, """
                #include <stdarg.h>
                typedef int (*Callback)(int);
                int bridge(Callback cb, int n);
                int managed(Callback cb, int n);
                int value = 5;
                int add(int n) { return value + n; }
                int sum(int first, ...) { va_list ap; va_start(ap,first); int n=va_arg(ap,int); va_end(ap); return first+n+value; }
                typedef int (*Variadic)(int, ...);
                Variadic variadic(void) { return sum; }
                int test(void) { return bridge(add,3) + managed(add,4) + variadic()(1,2); }
                """);
            File.WriteAllText(two, "typedef int (*Callback)(int); int bridge(Callback cb, int n) { return cb(n); }");
            var rule = new FunctionOverride("managed", new("int", new[] { "Callback", "int" }),
                new("managedMethod", "global::InstanceBoundary.Invoke", PassInstance: true), RequireMatch: true);
            var preprocessing = new CPreprocessingOptions(Array.Empty<MacroOverride>(), functionOverrides: new[] { rule });
            var options = new CSharpOutputOptions(Runtime: RuntimeProfile.C, InstanceMethods: true);
            string generated;
            if (objectLink)
            {
                var first = one + ".o"; var second = two + ".o";
                File.WriteAllText(first, Compiler.EmitObject(one, preprocessing: preprocessing, outputOptions: new(InstanceMethods: true)));
                File.WriteAllText(second, Compiler.EmitObject(two, outputOptions: new(InstanceMethods: true)));
                File.ReadAllText(first).ShouldContain("\"passInstance\":true");
                generated = Compiler.LinkObjects(new[] { first, second }, emit: EmitMode.ManagedLib, className: "Api", outputOptions: options);
            }
            else generated = Compiler.EmitCSharp(new[] { one, two }, emit: EmitMode.ManagedLib,
                className: "Api", preprocessing: preprocessing, outputOptions: options);
            const string host = """
                public static unsafe class InstanceBoundary {
                    public static int Invoke(Api instance, delegate*<Api,int,int> callback, int n) => callback(instance,n);
                    public static int Run() {
                        using var a = new Api(); using var b = new Api();
                        using (b.__DotCcEnter()) { b.Globals.value = 20; return a.test()*100 + b.test(); }
                    }
                }
                """;
            var compilation = CSharpCompilation.Create("InstanceBoundary_" + Guid.NewGuid().ToString("N"),
                new[] { ParseSource(generated), ParseSource(host) }, RuntimeReferences(),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
            using var image = new MemoryStream();
            var result = compilation.Emit(image, cancellationToken: TestContext.Current.CancellationToken);
            result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
            image.Position = 0;
            var assembly = new AssemblyLoadContext("instance-boundary-" + Guid.NewGuid(), isCollectible: false).LoadFromStream(image);
            ((int)assembly.GetType("InstanceBoundary")!.GetMethod("Run")!.Invoke(null, null)!).ShouldBe(2570);
        }
        finally { Directory.Delete(directory, true); }
    }

    internal const string InstanceSource = """
        #include <stdint.h>
        #include <stdlib.h>
        #include <pthread.h>
        typedef int (*Callback)(int);
        struct Bag { Callback callbacks[2]; int tail; };
        int value = 7;
        int array[] = { 11, 13 };
        int *saved = &value;
        _Alignas(64) int aligned = 17;
        _Thread_local int local;
        _Thread_local char buffer[16];
        int add(int n) { return value + n; }
        Callback table[] = { add, &add };
        Callback address(void) { return add; }
        int step(int expected) {
            static int times;
            if (*saved != expected || array[0] != expected + 4 || aligned != expected + 10) return 1;
            if ((uintptr_t)&aligned % 64) return 2;
            if (times != value - 7) return 3;
            ++value; ++array[0]; ++aligned; ++local; ++buffer[0]; ++times;
            return 0;
        }
        int tls(void) { return local * 100 + buffer[0]; }
        int special(void) { return aligned * 1000 + tls(); }
        int callbacks(Callback other) {
            struct Bag bag = {{add, other}, 17};
            Callback roundtrip = (Callback)(void*)add;
            int (*runtime)(int) = abs;
            if (sizeof(Callback) != sizeof(void*) || sizeof(bag.callbacks) != 2*sizeof(void*)) return -1;
            if (table[0] != add || table[1] != &add || roundtrip != add || !other) return -2;
            if (runtime(-2) != 2 || address()(3) != value + 3) return -3;
            return bag.callbacks[0](1) + bag.callbacks[1](2) + roundtrip(3);
        }
        int compare(const void *a, const void *b) { return (*(const int*)a - *(const int*)b) * (value > 0 ? 1 : -1); }
        int sort(void) {
            int data[] = { 4, 1, 3, 2 }; int key = 3;
            qsort(data, 4, sizeof(int), compare);
            int *found = bsearch(&key, data, 4, sizeof(int), compare);
            return data[0] == 1 && data[3] == 4 && found && *found == key;
        }
        void *child(void *unused) { return (void*)(intptr_t)(value + tls()); }
        int spawn(void) {
            pthread_t thread; void *answer;
            if (pthread_create(&thread, 0, child, 0)) return -1;
            if (pthread_join(thread, &answer)) return -2;
            return (int)(intptr_t)answer;
        }
        """;

    internal const string InstanceHost = """
        public static unsafe class InstanceProbe {
            public static string Run() {
                using var a = new Api(); using var b = new Api();
                using (a.__DotCcEnter()) {
                    if (a.step(7) != 0 || a.tls() != 101 || a.spawn() != 8 || a.sort() != 1) return "a";
                    var pointer = a.address();
                    if (pointer != b.address() || pointer != POINTERS.add) return "canonical";
                    if (a.callbacks(pointer) != 30 || b.callbacks(pointer) != 27) return "callbacks";
                    bool active = false;
                    try { a.Dispose(); } catch (System.InvalidOperationException) { active = true; }
                    if (!active) return "active disposal";
                    using (b.__DotCcEnter()) {
                        if (b.tls() != 0 || b.special() != 17000 || a.special() != 18101) return "instance special";
                        if (pointer(a, 1) != 9 || pointer(b, 1) != 8) return "explicit pointer";
                        if (b.step(7) != 0) return "b";
                        // Exception unwinding of callback binding restores B.
                    }
                    if (a.tls() != 101) return "restore";
                }
                var reservation = a.__DotCcRetain();
                bool retained = false;
                try { a.Dispose(); } catch (System.InvalidOperationException) { retained = true; }
                if (!retained) return "retained disposal";
                nint deferredAddress = (nint)a.address();
                int deferredResult = 0;
                var deferred = new System.Threading.Thread(() => {
                    try { deferredResult = ((delegate*<Api,int,int>)deferredAddress)(a,5); }
                    finally { reservation.Dispose(); }
                });
                deferred.Start(); deferred.Join();
                reservation.Dispose();
                if (deferredResult != 13) return "deferred origin";
                System.GC.Collect(); System.GC.WaitForPendingFinalizers(); System.GC.Collect();
                string result = "ok";
                var one = new System.Threading.Thread(() => {
                    using (a.__DotCcEnter()) { if (a.tls() != 0 || a.step(8) != 0 || a.spawn() != 9) result = "thread a"; }
                });
                var two = new System.Threading.Thread(() => {
                    using (b.__DotCcEnter()) { if (b.tls() != 0 || b.step(8) != 0 || b.spawn() != 9) result = "thread b"; }
                });
                one.Start(); two.Start(); one.Join(); two.Join();
                using (a.__DotCcEnter()) { if (a.tls() != 101 || a.step(9) != 0) return "tls retained"; }
                a.Dispose();
                bool retired = false;
                try { a.__DotCcEnter(); } catch (System.ObjectDisposedException) { retired = true; }
                if (!retired) return "retired";
                using var fresh = new Api();
                using (fresh.__DotCcEnter()) { if (fresh.tls() != 0 || fresh.step(7) != 0) return "fresh"; }
                return result;
            }
        }
        """;
}
