#nullable enable

using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace DotCC.Libc;

/// <summary>
/// C99 <c>&lt;setjmp.h&gt;</c> surface, implemented via .NET exceptions.
/// </summary>
/// <remarks>
/// <para>
/// Real C's <c>setjmp</c> "returns twice" — once normally (0) and a
/// second time after <c>longjmp</c> (the longjmp value). That's
/// not expressible in structured C# control flow. The dotcc lowering
/// recognises specific syntactic patterns and rewrites them into a
/// <c>try / catch when</c> block. <c>longjmp</c> throws a
/// <see cref="JumpBufferException"/> carrying a numeric identity + value; the
/// <c>catch when</c> filter matches the identity captured at the right setjmp.
/// </para>
/// <para>
/// Supported syntactic shapes (the emitter recognises these and
/// rewrites; other shapes throw <c>CompileException</c>):
/// <list type="bullet">
///   <item><c>if (setjmp(env)) { recovery } else { normal }</c></item>
///   <item><c>if (setjmp(env) == 0) { normal } else { recovery }</c></item>
///   <item><c>switch (setjmp(env)) { case 0: …; case N: …; }</c> — VALUE
///     capture: the switch dispatches on the actual longjmp value.</item>
///   <item><c>int r = setjmp(env); switch (r) { … }</c> / <c>if (r == N) …</c>,
///     and <c>r = setjmp(env);</c> into a pre-declared simple variable.</item>
/// </list>
/// The value-capture shapes lower via a goto-restart: the enclosing region
/// re-runs from a synthetic label with <c>r</c> holding the jump value each
/// time a matching <c>longjmp</c> is caught — faithful "returns twice".
/// One bonus over real C: finally blocks DO run during the longjmp
/// unwind (.NET exception semantics). That's strictly better than
/// real <c>longjmp</c>, which silently skips through cleanup code —
/// a famous footgun.
/// </para>
/// </remarks>
public static unsafe partial class Libc
{
    private static long _nextJumpBufferIdentity;

    /// <summary>Arm an unmanaged identity slot for a newly executed setjmp.
    /// The returned identity is captured by the emitted handler, so neither a
    /// side-effecting buffer expression nor a later rearm changes its target.</summary>
    public static ulong ArmJumpBuffer(ulong* buffer)
    {
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));
        long previous, next;
        do
        {
            previous = Volatile.Read(ref _nextJumpBufferIdentity);
            if (previous == long.MaxValue)
                throw new InvalidOperationException("Nonlocal jump identity space exhausted.");
            next = previous + 1;
        } while (Interlocked.CompareExchange(ref _nextJumpBufferIdentity, next, previous) != previous);
        *buffer = (ulong)next;
        return (ulong)next;
    }

    /// <summary>Managed exception transport for an unmanaged C jump buffer.
    /// Only the numeric identity and return value cross the unwind boundary.</summary>
    public sealed class JumpBufferException : Exception
    {
        public ulong Identity { get; }
        public int Value { get; }
        public JumpBufferException(ulong identity, int value)
            : base($"longjmp(value={value})")
        {
            Identity = identity;
            Value = value;
        }
    }

    /// <summary>The emitter handles setjmp's returns-twice semantics. A direct
    /// runtime call cannot establish a resumable handler and fails explicitly.</summary>
    public static int setjmp(ulong* buffer)
        => throw new InvalidOperationException("setjmp requires compiler control-flow lowering.");

    public static void longjmp(ulong* buffer, int value)
    {
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));
        ulong identity = *buffer;
        if (identity == 0)
            throw new InvalidOperationException("longjmp requires an armed jump buffer.");
        throw new JumpBufferException(identity, value == 0 ? 1 : value);
    }

    /// <summary>
    /// Legacy managed API token retained for existing direct callers. The C
    /// header now uses an unmanaged numeric slot and never stores this class
    /// in translated C memory.
    /// </summary>
    public sealed class LongJmpToken { }

    /// <summary>
    /// Exception carrying a non-local jump's target + value. Thrown
    /// by <see cref="longjmp(LongJmpToken, int)"/>; caught by the
    /// emitter-generated <c>catch when (__jmp.Token == env)</c>.
    /// </summary>
    public sealed class LongJmpException : Exception
    {
        public LongJmpToken Token { get; }
        public int Value { get; }
        public LongJmpException(LongJmpToken token, int value)
            : base($"longjmp(value={value})")
        {
            Token = token;
            Value = value;
        }
    }

    /// <summary>
    /// <c>setjmp(env)</c>. The function ALWAYS returns 0 on its
    /// direct call (it's the post-longjmp re-entry that returns the
    /// value, and that's handled by the emitter's try/catch
    /// rewrite). User code that pattern-matches the result against
    /// 0 vs non-zero gets the right behaviour after the rewrite.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int setjmp(LongJmpToken env) => 0;

    /// <summary>
    /// <c>longjmp(env, value)</c>. Throws a
    /// <see cref="LongJmpException"/> tagged with <paramref name="env"/>.
    /// The emitter-generated <c>catch when (__jmp.Token == env)</c>
    /// at the matching <c>setjmp</c> site catches it, exposes the
    /// value, and resumes execution in the recovery branch.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void longjmp(LongJmpToken env, int value)
        => throw new LongJmpException(env, value == 0 ? 1 : value);
}
