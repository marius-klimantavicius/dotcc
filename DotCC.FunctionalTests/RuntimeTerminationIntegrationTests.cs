using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class RuntimeTerminationIntegrationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Translated_instance_exit_and_pthread_failure_preserve_other_instances(bool objects)
    {
        string directory = Path.Combine(Path.GetTempPath(), "dotcc-owner-exit-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string path = Path.Combine(directory, "exit.c");
            File.WriteAllText(path, """
                #include <stdlib.h>
                #include <pthread.h>
                int continued;
                void request_exit(void) { exit(17); continued = 1; }
                static void *worker(void *arg) { abort(); continued = 2; return arg; }
                int run_worker(void) {
                    pthread_t thread;
                    if (pthread_create(&thread, 0, worker, 0)) return 1;
                    return pthread_join(thread, 0);
                }
                int healthy(void) { return 42 + continued; }
                """);
            var options = new CSharpOutputOptions(Runtime: RuntimeProfile.C, InstanceMethods: true);
            string generated;
            if (objects)
            {
                string fragment = path + ".o.cs";
                File.WriteAllText(fragment, Compiler.EmitObject(path, outputOptions: new(InstanceMethods: true)));
                generated = Compiler.LinkObjects([fragment], emit: EmitMode.ManagedLib, className: "ExitApi", outputOptions: options);
            }
            else generated = Compiler.EmitCSharp([path], emit: EmitMode.ManagedLib, className: "ExitApi", outputOptions: options);
            generated += """

                public static class ExitProbe {
                    public static int Main() {
                        using var first = new ExitApi();
                        using var second = new ExitApi();
                        using var third = new ExitApi();
                        bool caught = false;
                        using (second.__DotCcEnter()) {
                            try { using (first.__DotCcEnter()) first.request_exit(); return 1; }
                            catch (global::Libc.RuntimeTerminationException error) {
                                if (!object.ReferenceEquals(error.Owner, first.__DotCcRuntime)) return 2;
                                caught = error.Termination.Status == 17;
                            }
                            if (!object.ReferenceEquals(global::Libc.RuntimeContext.Current, second.__DotCcRuntime)) return 3;
                        }
                        if (!caught || first.Globals.continued != 0 || second.healthy() != 42) return 4;
                        using (third.__DotCcEnter())
                            if (third.run_worker() != 0 || third.Globals.continued != 0) return 5;
                        if (third.__DotCcRuntime.Termination?.Kind != global::Libc.RuntimeTerminationKind.Abort) return 6;
                        if (!third.__DotCcRuntime.TerminationTask.IsCompletedSuccessfully) return 7;
                        return second.__DotCcRuntime.Termination == null && second.healthy() == 42 ? 0 : 8;
                    }
                }
                """;
            FixtureRunner.CompileAndRunCapturingExit(generated, []).exit.ShouldBe(0);
        }
        finally { Directory.Delete(directory, true); }
    }
}
