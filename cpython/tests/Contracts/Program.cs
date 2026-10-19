using System.Text;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Managed.CPython;

int passed = 0;
void Check(bool condition) { if (!condition) throw new InvalidOperationException("Contract assertion failed"); passed++; }
void Throws<T>(Action action) where T : Exception
{
    try { action(); } catch (T) { passed++; return; }
    throw new InvalidOperationException("Expected " + typeof(T).Name);
}

if (args.SequenceEqual(new[] { "--faulted-owner" }))
{
    var uncertain = RuntimeOwnership.Claim();
    uncertain.BindExecutionThread();
    uncertain.MarkFaulted();
    Throws<InvalidOperationException>(() => uncertain.RequireOwner());
    Throws<InvalidOperationException>(() => uncertain.MarkFinalized());
    Throws<InvalidOperationException>(() => RuntimeOwnership.Claim());
    Console.WriteLine($"PASS {passed} uncertain-runtime reservation assertions; no CPython execution claimed");
    return;
}

foreach (string text in new[] { "", "ASCII", "λ🐍", "a\0b", "\ud800x\udfff" })
{
    Check(PythonText.FromCodePoints(PythonText.ToCodePoints(text, PythonSurrogatePolicy.Preserve), PythonSurrogatePolicy.Preserve) == text);
    if (!text.Contains('\ud800')) Check(PythonText.DecodeUtf8(PythonText.EncodeUtf8(text)) == text);
}
Check(PythonText.ToCodePoints("🐍").AsSpan().SequenceEqual(new uint[] { 0x1f40d }));
Check(PythonText.EncodeUtf8("a\0b").AsSpan().SequenceEqual(new byte[] { 97, 0, 98 }));
Throws<EncoderFallbackException>(() => PythonText.EncodeUtf8("\ud800"));
Throws<EncoderFallbackException>(() => PythonText.ToCodePoints("\udfff"));
Throws<DecoderFallbackException>(() => PythonText.DecodeUtf8(new byte[] { 0xc0, 0x80 }));
Throws<DecoderFallbackException>(() => PythonText.FromCodePoints(new uint[] { 0x110000 }));
Throws<DecoderFallbackException>(() => PythonText.FromCodePoints(new uint[] { 0xd800 }));
Throws<ArgumentException>(() => PythonText.RequireCString("a\0b", "path"));

var owner = RuntimeOwnership.Claim();
owner.BindExecutionThread();
Throws<InvalidOperationException>(() => RuntimeOwnership.Claim());
owner.AddValue();
Throws<InvalidOperationException>(() => owner.BeginStop());
Exception? foreignThreadError = null;
var thread = new Thread(() => { try { owner.RequireOwner(); } catch (Exception error) { foreignThreadError = error; } });
thread.Start(); Check(thread.Join(TimeSpan.FromSeconds(5))); Check(foreignThreadError is InvalidOperationException);
owner.ReleaseValue(); owner.BeginStop();
Throws<InvalidOperationException>(() => owner.AddValue());
owner.MarkFinalized();
Throws<ObjectDisposedException>(() => owner.RequireOwner());
var nextOwner = RuntimeOwnership.Claim(); nextOwner.BindExecutionThread(); nextOwner.BeginStop(); nextOwner.MarkFinalized(); passed++;
var cleanFailure = RuntimeOwnership.Claim(); cleanFailure.ReleaseCleanInitializationFailure();
Throws<ObjectDisposedException>(() => cleanFailure.RequireOwner());

// Real BCL thread/dispatcher checks only. No export adapter or Python substitute.
var ambient = new AsyncLocal<int> { Value = 91 };
var originalContext = SynchronizationContext.Current;
SynchronizationContext.SetSynchronizationContext(new RejectingSynchronizationContext());
using (var execution = new PythonExecutionThread())
{
    int caller = Environment.CurrentManagedThreadId;
    int executor = execution.Invoke(() => Environment.CurrentManagedThreadId);
    Check(executor != caller);
    Check(execution.Invoke(() => Environment.CurrentManagedThreadId) == executor);
    Check(execution.Invoke(() => ambient.Value) == 0);
    Check(execution.Invoke(() => SynchronizationContext.Current) == null);
    if (OperatingSystem.IsLinux() && RuntimeInformation.ProcessArchitecture == Architecture.X64)
    {
        ulong physical = execution.Invoke(NativeStack.Size);
        Check(physical >= PythonExecutionThread.StackReservationBytes);
        Console.WriteLine($"Actual support executor Linux stack={physical}");
    }
    long ownerTokenForDrain = 0;
    long ownerToken = CallbackRoots.Add(value =>
    {
        Check(Environment.CurrentManagedThreadId == caller);
        Throws<InvalidOperationException>(() => execution.Invoke(() => 0));
        Throws<InvalidOperationException>(() => CallbackRoots.RemoveAfterDrain(ownerTokenForDrain));
        return value * 2;
    }, execution);
    ownerTokenForDrain = ownerToken;
    Check(execution.Invoke(() => CallbackRoots.Invoke(ownerToken, 21)) == new CallbackOutcome(true, 42, null));
    CallbackRoots.RemoveAfterDrain(ownerToken);
    var marker = new ApplicationException("exact exception identity");
    try { execution.Invoke<int>(() => ThrowMarker(marker)); }
    catch (ApplicationException error) { Check(ReferenceEquals(marker, error)); Check(error.StackTrace!.Contains(nameof(ThrowMarker))); }
    Check(execution.Invoke(() => 42) == 42);
    Exception? rejected = null;
    var foreign = new Thread(() => { try { execution.Invoke(() => 0); } catch (Exception error) { rejected = error; } });
    foreign.Start(); Check(foreign.Join(TimeSpan.FromSeconds(5))); Check(rejected is InvalidOperationException);
}
SynchronizationContext.SetSynchronizationContext(originalContext);
using (ExecutionContext.SuppressFlow())
using (var execution = new PythonExecutionThread()) Check(execution.Invoke(() => 7) == 7);
Throws<ApplicationException>(() => new PythonExecutionThread(() => throw new ApplicationException("startup failed")));

var (token, captured) = RootCapturedCallback();
GC.Collect(); GC.WaitForPendingFinalizers();
Check(captured.IsAlive);
Check(CallbackRoots.Invoke(token, 21) == new CallbackOutcome(true, 42, null));
CallbackRoots.RemoveAfterDrain(token);
Check(!CallbackRoots.Invoke(token, 21).Succeeded);
long failureToken = CallbackRoots.Add(_ => throw new InvalidOperationException("callback failure"));
CallbackOutcome failure = CallbackRoots.Invoke(failureToken, 1);
Check(!failure.Succeeded && failure.Error == "InvalidOperationException: callback failure");
CallbackRoots.RemoveAfterDrain(failureToken);
using var active = new ManualResetEventSlim();
using var release = new ManualResetEventSlim();
long blockedToken = CallbackRoots.Add(value => { active.Set(); if (!release.Wait(TimeSpan.FromSeconds(5))) throw new TimeoutException(); return value; });
CallbackOutcome workerResult = default;
var worker = new Thread(() => workerResult = CallbackRoots.Invoke(blockedToken, 7));
worker.Start(); Check(active.Wait(TimeSpan.FromSeconds(5)));
Throws<InvalidOperationException>(() => CallbackRoots.RemoveAfterDrain(blockedToken));
release.Set(); Check(worker.Join(TimeSpan.FromSeconds(5))); Check(workerResult.Value == 7 && workerResult.Succeeded);
CallbackRoots.RemoveAfterDrain(blockedToken);
Console.WriteLine($"PASS {passed} managed contract assertions; no CPython execution claimed");

[MethodImpl(MethodImplOptions.NoInlining)]
static (long, WeakReference) RootCapturedCallback()
{
    var multiplier = new CapturedMultiplier(2);
    return (CallbackRoots.Add(multiplier.Apply), new WeakReference(multiplier));
}

[MethodImpl(MethodImplOptions.NoInlining)]
static int ThrowMarker(Exception error) => throw error;

sealed class CapturedMultiplier(long factor)
{
    internal long Apply(long value) => checked(value * factor);
}

sealed class RejectingSynchronizationContext : SynchronizationContext
{
    public override void Send(SendOrPostCallback callback, object? state) => throw new InvalidOperationException("Unexpected context pump");
    public override void Post(SendOrPostCallback callback, object? state) => throw new InvalidOperationException("Unexpected context pump");
}

static unsafe class NativeStack
{
    [DllImport("libc", EntryPoint="pthread_self")] private static extern nuint Self();
    [DllImport("libc", EntryPoint="pthread_getattr_np")] private static extern int Attributes(nuint thread, void* attributes);
    [DllImport("libc", EntryPoint="pthread_attr_getstack")] private static extern int Get(void* attributes, void** address, nuint* size);
    [DllImport("libc", EntryPoint="pthread_attr_destroy")] private static extern int Destroy(void* attributes);
    internal static ulong Size()
    {
        byte* attributes=stackalloc byte[128];
        if(Attributes(Self(),attributes)!=0) throw new InvalidOperationException();
        try { void* address; nuint size; if(Get(attributes,&address,&size)!=0 || address==null) throw new InvalidOperationException(); return size; }
        finally { if(Destroy(attributes)!=0) throw new InvalidOperationException(); }
    }
}
