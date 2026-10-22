using System;
using System.IO;
using System.Threading;
using DotCC;
using Xunit;
using L = DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class SignalJumpTests
{
    private static ulong* handlerTarget;
    private static int delivered;

    private static void JumpHandler(int signal)
    {
        ulong extra = 1UL << 9;
        Assert.Equal(0, L.sigprocmask(0, &extra, null));
        L.siglongjmp(handlerTarget, 0);
    }

    private static void CountingHandler(int signal) => delivered++;

    private static string Emit(string body)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-signal-jump-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, "#include <setjmp.h>\n" + body);
        try { return Compiler.EmitCSharp(new[] { path }); }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Direct_unlowered_and_unarmed_calls_fail_and_saving_requires_real_signals()
    {
        ulong slot = 0;
        ulong* pointer = &slot;
        Assert.Throws<InvalidOperationException>(() => L.sigsetjmp(pointer, 0));
        Assert.Throws<InvalidOperationException>(() => L.siglongjmp(pointer, 0));
        using var owner = new L.RuntimeContext();
        using var binding = owner.Enter();
        Assert.Throws<NotSupportedException>(() => L.ArmSignalJumpBuffer(pointer, 1));
        Assert.Equal(0UL, slot);
        using var activation = L.ArmSignalJumpBuffer(pointer, 0);
        var jump = Assert.Throws<L.JumpBufferException>(() => L.siglongjmp(pointer, 0));
        Assert.Equal(1, jump.Value);
        Assert.Equal(activation.Identity, jump.Identity);
        activation.Restore(jump);
    }

    [Theory]
    [InlineData(0, 1UL << 9)]
    [InlineData(1, 1UL << 11)]
    [InlineData(-1, 1UL << 11)]
    public void Snapshot_restores_only_when_requested_and_can_be_used_on_repeated_jumps(int save, ulong expected)
    {
        using var owner = new L.RuntimeContext();
        using var binding = owner.Enter();
        owner.EnablePgliteSignals();
        ulong mask = 1UL << 11, changed = 1UL << 9, observed = 0;
        Assert.Equal(0, L.sigprocmask(2, &mask, null));
        ulong slot = 0;
        ulong* pointer = &slot;
        using var activation = L.ArmSignalJumpBuffer(pointer, save);
        foreach (int value in new[] { 0, 7, -3 })
        {
            Assert.Equal(0, L.sigprocmask(2, &changed, null));
            var jump = Assert.Throws<L.JumpBufferException>(() => L.siglongjmp(pointer, value));
            activation.Restore(jump);
            Assert.Equal(value == 0 ? 1 : value, jump.Value);
            Assert.Equal(0, L.sigprocmask(2, null, &observed));
            Assert.Equal(expected, observed);
        }
    }

    [Fact]
    public void Nested_and_rearmed_buffers_use_the_captured_identity()
    {
        ulong* slots = stackalloc ulong[2];
        using var outer = L.ArmSignalJumpBuffer(slots, 0);
        using var inner = L.ArmSignalJumpBuffer(slots + 1, 0);
        using var replacement = L.ArmSignalJumpBuffer(slots, 0);
        var jump = Assert.Throws<L.JumpBufferException>(() => L.siglongjmp(slots, 7));
        Assert.NotEqual(outer.Identity, replacement.Identity);
        Assert.NotEqual(inner.Identity, replacement.Identity);
        Assert.Throws<InvalidOperationException>(() => outer.Restore(jump));
        Assert.Throws<InvalidOperationException>(() => inner.Restore(jump));
        replacement.Restore(jump);
    }

    [Fact]
    public void Expired_activation_rejects_recovery_without_dereferencing_retired_guest_storage()
    {
        ulong slot = 0;
        ulong* pointer = &slot;
        var activation = L.ArmSignalJumpBuffer(pointer, 0);
        var jump = Assert.Throws<L.JumpBufferException>(() => L.siglongjmp(pointer, 9));
        activation.Dispose(); activation.Dispose();
        Assert.Throws<InvalidOperationException>(() => activation.Restore(jump));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void Foreign_throw_is_rejected_even_after_its_binding_has_unwound(int save)
    {
        using var owner = new L.RuntimeContext();
        using var foreign = new L.RuntimeContext();
        using var binding = owner.Enter();
        owner.EnablePgliteSignals();
        ulong slot = 0;
        ulong* pointer = &slot;
        using var activation = L.ArmSignalJumpBuffer(pointer, save);
        L.JumpBufferException jump;
        using (foreign.Enter()) jump = Assert.Throws<L.JumpBufferException>(() => L.siglongjmp(pointer, 7));
        Assert.Same(owner, L.RuntimeContext.Current);
        Assert.Throws<InvalidOperationException>(() => activation.Restore(jump));
    }

    [Fact]
    public void Foreign_thread_and_terminal_owner_cannot_resume_an_activation()
    {
        using var owner = new L.RuntimeContext();
        using var binding = owner.Enter();
        ulong slot = 0;
        ulong* pointer = &slot;
        using var activation = L.ArmSignalJumpBuffer(pointer, 0);
        L.JumpBufferException? jump = null;
        var thread = new Thread(() =>
        {
            using var foreignBinding = owner.Enter();
            jump = Assert.Throws<L.JumpBufferException>(() => L.siglongjmp(pointer, 2));
        });
        thread.Start(); thread.Join();
        Assert.Throws<InvalidOperationException>(() => activation.Restore(jump!));
        var localJump = Assert.Throws<L.JumpBufferException>(() => L.siglongjmp(pointer, 3));
        Assert.Throws<L.RuntimeTerminationException>(() => L.exit(0));
        Assert.Throws<InvalidOperationException>(() => activation.Restore(localJump));
        Assert.Throws<InvalidOperationException>(() => L.ArmSignalJumpBuffer(pointer, 0));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void Escaping_a_real_cooperative_handler_preserves_or_restores_its_mask(int save)
    {
        using var owner = new L.RuntimeContext();
        using var binding = owner.Enter();
        owner.EnablePgliteSignals();
        L.PgliteSignalAction action = new() { Handler = (nint)(delegate*<int, void>)&JumpHandler };
        Assert.Equal(0, L.sigaction(14, &action, null));
        ulong slot = 0, observed = 0;
        handlerTarget = &slot;
        using var activation = L.ArmSignalJumpBuffer(handlerTarget, save);
        var jump = Assert.Throws<L.JumpBufferException>(() => L.raise(14));
        activation.Restore(jump);
        Assert.Equal(0, L.sigprocmask(2, null, &observed));
        Assert.Equal(save == 0 ? (1UL << 13) | (1UL << 9) : 0UL, observed);
        Assert.False(owner.PgliteSignals!.Delivering);
        handlerTarget = null;
    }

    [Fact]
    public void Restore_defers_pending_callbacks_until_the_next_safe_point()
    {
        using var owner = new L.RuntimeContext();
        using var binding = owner.Enter();
        owner.EnablePgliteSignals(); delivered = 0;
        L.PgliteSignalAction action = new() { Handler = (nint)(delegate*<int, void>)&CountingHandler };
        Assert.Equal(0, L.sigaction(14, &action, null));
        ulong slot = 0, block = 1UL << 13;
        ulong* pointer = &slot;
        using var activation = L.ArmSignalJumpBuffer(pointer, 1);
        Assert.Equal(0, L.sigprocmask(0, &block, null));
        Assert.Equal(0, L.raise(14));
        var jump = Assert.Throws<L.JumpBufferException>(() => L.siglongjmp(pointer, 1));
        activation.Restore(jump);
        Assert.Equal(0, delivered);
        owner.PumpPgliteSignals();
        Assert.Equal(1, delivered);
    }

    [Fact]
    public void Pending_and_wait_reject_unopted_owners_without_success_or_errno_loss()
    {
        ulong unbound = 0;
        int unboundSignal = 99;
        Assert.Equal(-1, L.sigpending(&unbound)); Assert.Equal(L.ENOTSUP, L.errno);
        Assert.Equal(L.ENOTSUP, L.sigwait(&unbound, &unboundSignal)); Assert.Equal(99, unboundSignal);
        using var owner = new L.RuntimeContext();
        using var binding = owner.Enter();
        ulong set = 1UL << 12, observed = 123;
        int selected = 99;
        Assert.Equal(-1, L.sigpending(&observed));
        Assert.Equal(L.ENOTSUP, L.errno);
        Assert.Equal(123UL, observed);
        L.errno = 777;
        Assert.Equal(L.ENOTSUP, L.sigwait(&set, &selected));
        Assert.Equal(777, L.errno);
        Assert.Equal(99, selected);
    }

    [Fact]
    public void Pending_queries_do_not_consume_and_wait_consumes_only_selected_blocked_bits()
    {
        using var owner = new L.RuntimeContext();
        using var binding = owner.Enter(); owner.EnablePgliteSignals(); delivered = 0;
        L.PgliteSignalAction action = new() { Handler = (nint)(delegate*<int, void>)&CountingHandler };
        Assert.Equal(0, L.sigaction(13, &action, null));
        Assert.Equal(0, L.sigaction(14, &action, null));
        ulong pipe = 1UL << 12, both = pipe | (1UL << 13), observed = 0;
        Assert.Equal(0, L.sigprocmask(0, &both, null));
        Assert.Equal(0, L.raise(13)); Assert.Equal(0, L.raise(13)); Assert.Equal(0, L.raise(14));
        Assert.Equal(0, L.sigpending(&observed)); Assert.Equal(both, observed);
        Assert.Equal(0, L.sigpending(&observed)); Assert.Equal(both, observed);
        using (var foreign = new L.RuntimeContext())
        using (foreign.Enter())
        {
            foreign.EnablePgliteSignals();
            ulong foreignPending = 99;
            int foreignSignal = 99;
            Assert.Equal(0, L.sigpending(&foreignPending)); Assert.Equal(0UL, foreignPending);
            Assert.Equal(0, L.sigprocmask(0, &pipe, null));
            Assert.Equal(L.ENOTSUP, L.sigwait(&pipe, &foreignSignal)); Assert.Equal(99, foreignSignal);
        }
        int selected = 99;
        L.errno = 777;
        Assert.Equal(0, L.sigwait(&pipe, &selected)); Assert.Equal(13, selected); Assert.Equal(777, L.errno);
        Assert.Equal(0, L.sigpending(&observed)); Assert.Equal(1UL << 13, observed);
        selected = 99;
        Assert.Equal(L.ENOTSUP, L.sigwait(&pipe, &selected)); Assert.Equal(99, selected);
        Assert.Equal(0, L.sigwait(&both, &selected)); Assert.Equal(14, selected);
        Assert.Equal(0, L.sigpending(&observed)); Assert.Equal(0UL, observed);
        Assert.Equal(0, L.sigprocmask(1, &both, null)); owner.PumpPgliteSignals();
        Assert.Equal(0, delivered);
    }

    [Fact]
    public void Wait_observes_and_consumes_owned_timer_and_pending_observes_expiry()
    {
        using var owner = new L.RuntimeContext();
        using var binding = owner.Enter(); owner.EnablePgliteSignals(); delivered = 0;
        L.PgliteSignalAction action = new() { Handler = (nint)(delegate*<int, void>)&CountingHandler };
        Assert.Equal(0, L.sigaction(14, &action, null));
        ulong alarm = 1UL << 13, observed = 99;
        Assert.Equal(0, L.sigprocmask(0, &alarm, null));
        L.PgliteIntervalTimer timer = new() { ValueMicroseconds = 2000 };
        Assert.Equal(0, L.PgliteSetitimer(0, &timer, null));
        int selected = 0;
        L.errno = 777;
        Assert.Equal(0, L.sigwait(&alarm, &selected)); Assert.Equal(14, selected); Assert.Equal(777, L.errno);
        Assert.Equal(0, L.sigpending(&observed)); Assert.Equal(0UL, observed);
        Assert.Equal(0, L.PgliteSetitimer(0, &timer, null));
        Thread.Sleep(15);
        Assert.Equal(0, L.sigpending(&observed)); Assert.Equal(alarm, observed);
        Assert.Equal(0, L.sigwait(&alarm, &selected)); Assert.Equal(14, selected);
        Assert.Equal(0, L.sigprocmask(1, &alarm, null)); owner.PumpPgliteSignals();
        Assert.Equal(0, delivered);
    }

    [Fact]
    public void Pending_and_wait_validate_pointers_masks_threads_and_terminal_lifetimes()
    {
        using var owner = new L.RuntimeContext();
        using var binding = owner.Enter(); owner.EnablePgliteSignals();
        ulong mask = 1UL << 12;
        int result = 99;
        Assert.Equal(-1, L.sigpending(null)); Assert.Equal(L.EFAULT, L.errno);
        L.errno = 777;
        Assert.Equal(L.EFAULT, L.sigwait(null, &result));
        Assert.Equal(L.EFAULT, L.sigwait(&mask, null));
        Assert.Equal(L.EINVAL, L.sigwait(&mask, &result)); // not blocked
        Assert.Equal(777, L.errno); Assert.Equal(99, result);
        mask = 0; Assert.Equal(L.EINVAL, L.sigwait(&mask, &result));
        mask = 1UL << 8; Assert.Equal(L.EINVAL, L.sigwait(&mask, &result));
        int threadError = 0;
        var thread = new Thread(() =>
        {
            using var foreignBinding = owner.Enter();
            ulong selection = 1UL << 12, pending = 0;
            int signal = 0;
            Assert.Equal(-1, L.sigpending(&pending));
            threadError = L.sigwait(&selection, &signal);
        });
        thread.Start(); thread.Join(); Assert.Equal(L.EINVAL, threadError);
        Assert.Throws<L.RuntimeTerminationException>(() => L.exit(0));
        ulong observed = 123;
        Assert.Equal(-1, L.sigpending(&observed)); Assert.Equal(123UL, observed);
        Assert.Equal(L.EINVAL, L.sigwait(&mask, &result)); Assert.Equal(99, result);
    }

    [Theory]
    [InlineData("if (sigsetjmp(e, 1)) return 1;")]
    [InlineData("if (sigsetjmp(e, 1) == 0) siglongjmp(e, 1);")]
    [InlineData("if (0 != sigsetjmp(e, 1)) return 1;")]
    [InlineData("if (!sigsetjmp(e, 1)) siglongjmp(e, 1);")]
    [InlineData("int r = sigsetjmp(e, 1); if (!r) siglongjmp(e, 1);")]
    [InlineData("int r; r = sigsetjmp(e, 1); if (!r) siglongjmp(e, 1);")]
    [InlineData("int r; if (!(r = sigsetjmp(e, 1))) siglongjmp(e, 1);")]
    [InlineData("switch (sigsetjmp(e, 1)) { case 0: siglongjmp(e, 1); }")]
    [InlineData("int enabled = 1; if (enabled && sigsetjmp(e, 1) != 0) return 1; siglongjmp(e, 1);")]
    [InlineData("int enabled = 1; if (enabled && !sigsetjmp(e, 1)) siglongjmp(e, 1);")]
    public void Existing_lexical_shapes_carry_the_signal_activation(string statements)
    {
        var output = Emit("int main(void) { sigjmp_buf e; " + statements + " return 0; }");
        Assert.Contains("Libc.ArmSignalJumpBuffer(", output);
        Assert.Contains(".Restore(__jmp);", output);
        Assert.DoesNotContain("__dotcc_sigsetjmp_environment(", output);
    }

    [Theory]
    [InlineData("sigsetjmp(e, 1);")]
    [InlineData("while (sigsetjmp(e, 1)) {}")]
    [InlineData("do {} while (sigsetjmp(e, 1));")]
    [InlineData("for (; sigsetjmp(e, 1);) {}")]
    [InlineData("int x = sigsetjmp(e, 1) + 1;")]
    [InlineData("int x = sigsetjmp(e, 1) ? 1 : 2;")]
    [InlineData("int a[2]; int i = 0; if (!(a[i++] = sigsetjmp(e, 1))) {}")]
    [InlineData("int enabled = 1; if (enabled || sigsetjmp(e, 1)) {}")]
    [InlineData("int enabled = 1; if (enabled && sigsetjmp(e, 1) == 7) {}")]
    [InlineData("int enabled = 1; if (sigsetjmp(e, 1) != 0 && enabled) {}")]
    [InlineData("int enabled = 1; if (enabled && (sigsetjmp(e, 1) + 1)) {}")]
    public void Unsupported_signal_jump_flows_keep_loud_diagnostics(string statements)
    {
        var error = Assert.Throws<CompileException>(() => Emit("int main(void) { sigjmp_buf e; " + statements + " return 0; }"));
        Assert.Contains("sigsetjmp", error.Message);
        Assert.Contains("not supported", error.Message);
    }

    [Fact]
    public void Function_pointer_shadow_is_an_ordinary_call_and_normal_setjmp_does_not_capture_signals()
    {
        var shadow = Emit("""
            int fake(sigjmp_buf e, int save) { return 7; }
            int main(void) { sigjmp_buf e; int (*sigsetjmp)(sigjmp_buf, int) = fake; return sigsetjmp(e, 1) + 1; }
            """);
        Assert.DoesNotContain("Libc.ArmSignalJumpBuffer(", shadow);
        var ordinary = Emit("int main(void) { jmp_buf e; if (!setjmp(e)) longjmp(e, 0); return 0; }");
        Assert.Contains("Libc.ArmJumpBuffer(", ordinary);
        Assert.DoesNotContain("Libc.ArmSignalJumpBuffer(", ordinary);
    }
}
