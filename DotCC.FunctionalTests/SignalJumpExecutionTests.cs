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
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Signal_jump_lexical_masks_operands_nested_owners_and_expired_regions_execute(bool objects, bool reverse)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-signal-execution-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var main = Path.Combine(directory, "main.c");
            var jump = Path.Combine(directory, "jump.c");
            File.WriteAllText(jump, "#include <setjmp.h>\nvoid send_jump(void *env, int value) { siglongjmp(env, value); }");
            File.WriteAllText(main, """
                #include <setjmp.h>
                #include <signal.h>
                #include <stddef.h>
                #include <errno.h>
                void send_jump(void *env, int value);
                static sigjmp_buf target, stale;
                static int selections, saves, old_value, recovered;
                static int prefix_calls;
                static void *choose(int old) { ++selections; old_value = old; return target; }
                static int choose_save(int old) { ++saves; return old == 42; }
                static int prefix(int enabled) { ++prefix_calls; return enabled; }
                int conditional(int enabled) {
                    selections = saves = prefix_calls = 0;
                    if (prefix(enabled) && sigsetjmp(choose(42), choose_save(42)) != 0)
                        return prefix_calls == 1 && selections == 1 && saves == 1 ? 42 : -1;
                    if (enabled) send_jump(target, 7);
                    return prefix_calls == 1 && selections == 0 && saves == 0 ? 3 : -2;
                }
                struct Prompt { void *jmpbuf; };
                int actual_conditional(int enabled) {
                    sigjmp_buf local;
                    struct Prompt prompt = { local };
                    struct Prompt *prompt_ctx = enabled ? &prompt : NULL;
                    if (prompt_ctx && sigsetjmp(*((sigjmp_buf*)prompt_ctx->jmpbuf), 1) != 0) return 42;
                    if (prompt_ctx) send_jump(local, 7);
                    return 3;
                }
                int operands(void) {
                    selections = saves = 0;
                    int rc = 42;
                    rc = sigsetjmp(choose(rc), choose_save(rc));
                    if (!rc) send_jump(target, 0);
                    return rc == 1 && selections == 1 && saves == 1 && old_value == 42 ? 0 : 1;
                }
                int masks(int save) {
                    sigjmp_buf local;
                    sigset_t empty, changed, observed;
                    sigemptyset(&empty); sigemptyset(&changed); sigaddset(&changed, SIGALRM);
                    sigprocmask(SIG_SETMASK, &empty, NULL);
                    if (sigsetjmp(local, save) == 0) {
                        sigprocmask(SIG_BLOCK, &changed, NULL);
                        send_jump(local, 7);
                    } else {
                        sigprocmask(SIG_SETMASK, NULL, &observed);
                        return sigismember(&observed, SIGALRM) == (save == 0) ? 0 : 2;
                    }
                    return 3;
                }
                int nested(void) {
                    sigjmp_buf outer, inner;
                    int a = sigsetjmp(outer, 1);
                    if (!a) {
                        int b = sigsetjmp(inner, 0);
                        if (!b) send_jump(inner, -2);
                        if (b == -2) send_jump(outer, 9);
                        return 1;
                    }
                    return a == 9 ? 0 : 2;
                }
                int patterns(void) {
                    sigjmp_buf e;
                    int rc;
                    if (!(rc = sigsetjmp(e, 1))) send_jump(e, 2);
                    else if (rc == 2) send_jump(e, 3);
                    if (rc != 3) return 1;
                    switch (sigsetjmp(e, 0)) { case 0: send_jump(e, 4); break; case 4: break; default: return 2; }
                    if (!sigsetjmp(e, 0)) send_jump(e, 1);
                    if (sigsetjmp(e, 0)) rc = 5; else send_jump(e, 1);
                    return rc == 5 ? 0 : 3;
                }
                static int fake(sigjmp_buf e, int save) { return save + 6; }
                int shadow(void) {
                    sigjmp_buf e;
                    int (*sigsetjmp)(sigjmp_buf, int) = fake;
                    return sigsetjmp(e, 1) == 7 ? 0 : 1;
                }
                void expire(void) { if (sigsetjmp(stale, 0) == 0) return; ++recovered; }
                void jump_stale(void) { send_jump(stale, 1); }
                void foreign(int save, void (*callback)(void *)) {
                    sigjmp_buf e;
                    if (sigsetjmp(e, save) == 0) callback(e);
                    else ++recovered;
                }
                int invalid_recoveries(void) { return recovered; }
                int ordinary(void) { jmp_buf e; int rc = setjmp(e); if (!rc) longjmp(e, 0); return rc == 1 ? 0 : 1; }
                int ordinary_masks(void) {
                    jmp_buf e;
                    sigset_t empty, alarm, observed;
                    sigemptyset(&empty); sigemptyset(&alarm); sigaddset(&alarm, SIGALRM);
                    sigprocmask(SIG_SETMASK, &empty, NULL);
                    if (setjmp(e) == 0) { sigprocmask(SIG_BLOCK, &alarm, NULL); longjmp(e, 1); }
                    sigprocmask(SIG_SETMASK, NULL, &observed);
                    sigprocmask(SIG_SETMASK, &empty, NULL);
                    return sigismember(&observed, SIGALRM) == 1 ? 0 : 1;
                }
                int unsupported_wait(void) {
                    sigset_t selection;
                    sigemptyset(&selection); sigaddset(&selection, SIGPIPE);
                    int result = 99;
                    errno = 777;
                    int error = sigwait(&selection, &result);
                    return error == ENOTSUP && result == 99 && errno == 777 ? 0 : 1;
                }
                int pending(void) {
                    sigset_t pipe, observed, empty;
                    sigemptyset(&empty); sigemptyset(&pipe); sigaddset(&pipe, SIGPIPE);
                    sigprocmask(SIG_BLOCK, &pipe, NULL);
                    raise(SIGPIPE); raise(SIGPIPE);
                    if (sigpending(&observed) || !sigismember(&observed, SIGPIPE)) return 1;
                    int result = 99;
                    if (sigwait(&pipe, &result) || result != SIGPIPE) return 2;
                    if (sigpending(&observed) || sigismember(&observed, SIGPIPE)) return 3;
                    sigprocmask(SIG_SETMASK, &empty, NULL);
                    return 0;
                }
                int pending_timer(void) {
                    sigset_t alarm, empty, observed;
                    sigemptyset(&empty); sigemptyset(&alarm); sigaddset(&alarm, SIGALRM);
                    sigprocmask(SIG_BLOCK, &alarm, NULL);
                    int result = 99;
                    if (sigwait(&alarm, &result) || result != SIGALRM) return 1;
                    if (sigpending(&observed) || sigismember(&observed, SIGALRM)) return 2;
                    sigprocmask(SIG_SETMASK, &empty, NULL);
                    return 0;
                }
                """);
            var sources = new[] { main, jump };
            if (reverse) Array.Reverse(sources);
            var options = new CSharpOutputOptions(NestTypes: true, Runtime: RuntimeProfile.C, StateContext: true);
            string generated;
            if (objects)
            {
                sources = sources.Select((source, i) =>
                {
                    var path = Path.Combine(directory, "unit" + i + ".o");
                    File.WriteAllText(path, Compiler.EmitObject(source));
                    return path;
                }).ToArray();
                generated = Compiler.LinkObjects(sources, emit: EmitMode.ManagedLib, className: "SignalApi", outputOptions: options);
            }
            else generated = Compiler.EmitCSharp(sources, emit: EmitMode.ManagedLib, className: "SignalApi", outputOptions: options);

            const string host = """
                using A = SignalApi;
                public static unsafe class SignalHost {
                    private static void Foreign(void *target) {
                        using var foreign = new A.Libc.RuntimeContext();
                        using var binding = foreign.Enter();
                        A.Libc.siglongjmp((ulong*)target, 1);
                    }
                    public static int Run() {
                        // Ordinary setjmp and signal save=0 need no signal opt-in.
                        if (A.ordinary() != 0 || A.unsupported_wait() != 0) return 10;
                        for (int life = 0; life < 2; life++) {
                            using var state = A.__DotCcCreateContext();
                            using var binding = state.Enter();
                            if (A.conditional(0) != 3 || A.actual_conditional(0) != 3) return 18;
                            A.Libc.RuntimeContext.Current.EnableGuestSignals();
                            if (A.conditional(1) != 42 || A.actual_conditional(1) != 42) return 19;
                            if (A.operands() != 0 || A.masks(0) != 0 || A.masks(1) != 0) return 11;
                            if (A.nested() != 0 || A.patterns() != 0 || A.shadow() != 0) return 12;
                            if (A.ordinary_masks() != 0 || A.pending() != 0) return 16;
                            A.Libc.GuestIntervalTimer timer = new() { ValueMicroseconds = 2000 };
                            if (A.Libc.GuestSetitimer(0, &timer, null) != 0 || A.pending_timer() != 0) return 17;
                            A.expire();
                            try { A.jump_stale(); return 13; } catch (A.Libc.JumpBufferException) { }
                            for (int save = 0; save <= 1; save++) {
                                try { A.foreign(save, &Foreign); return 14; } catch (System.InvalidOperationException) { }
                            }
                            if (A.invalid_recoveries() != 0) return 15;
                        }
                        return 0;
                    }
                }
                """;
            var compilation = CSharpCompilation.Create("SignalJumps_" + Guid.NewGuid().ToString("N"),
                new[] { ParseSource(generated), ParseSource(host) }, RuntimeReferences(),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true, optimizationLevel: OptimizationLevel.Release));
            using var image = new MemoryStream();
            var result = compilation.Emit(image, cancellationToken: TestContext.Current.CancellationToken);
            result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
            image.Position = 0;
            var load = new AssemblyLoadContext("signal-jumps-" + Guid.NewGuid().ToString("N"), isCollectible: false);
            var assembly = load.LoadFromStream(image);
            assembly.GetType("SignalHost", true)!.GetMethod("Run")!.Invoke(null, null).ShouldBe(0);
        }
        finally { Directory.Delete(directory, true); }
    }
}
