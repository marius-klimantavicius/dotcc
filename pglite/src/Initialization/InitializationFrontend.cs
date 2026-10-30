using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Frontend = Managed.PGlite.Initialization.Translated.Initdb;
using Runtime = Managed.PGlite.Initialization.Translated.Initdb.Libc;

namespace Managed.PGlite.Initialization;

/// <summary>Owns one invocation of the original translated initdb main.
/// Callback FILE handles stay in this frontend's runtime. Backend commands
/// cross only as argv and private byte files.</summary>
public sealed unsafe class InitializationFrontend : IInitdbFiles, IDisposable
{
    [ThreadStatic] private static InitializationFrontend? active;
    private readonly Frontend.__DotCcContext context;
    private readonly Runtime.RuntimeContext runtime;
    private readonly InitializationProcessTransport transport;
    private readonly int thread = Environment.CurrentManagedThreadId;
    private bool running, used, disposed;
    private Exception? callbackError;

    public InitializationFrontend(string backendExecutable, string privateDirectory,
        Func<BootstrapInvocation, int> invoke)
    {
        context = Frontend.__DotCcCreateContext();
        try
        {
            using var binding = context.Enter();
            runtime = Runtime.RuntimeContext.Current!;
            runtime.EnableGuestSignals();
            transport = new(backendExecutable, privateDirectory, this, invoke);
        }
        catch { context.Dispose(); throw; }
    }

    public Exception? ProcessError => callbackError ?? transport.LastError;

    private void RequireOwner()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (thread != Environment.CurrentManagedThreadId)
            throw new InvalidOperationException("Initdb must stay on its owning thread.");
    }

    public nint Open(string path, string mode)
    {
        RequireOwner();
        using var binding = context.Enter();
        byte[] name = Encoding.UTF8.GetBytes(path + "\0"), type = Encoding.UTF8.GetBytes(mode + "\0");
        fixed (byte* p = name) fixed (byte* m = type) return (nint)Runtime.fopen(p, m);
    }

    public int Flush(nint stream)
    {
        RequireOwner();
        using var binding = context.Enter();
        return Runtime.fflush((Runtime.FILE*)stream);
    }

    public int Close(nint stream)
    {
        RequireOwner();
        using var binding = context.Enter();
        return Runtime.fclose((Runtime.FILE*)stream);
    }

    public void SetErrno(int error)
    {
        RequireOwner();
        using var binding = context.Enter();
        Runtime.errno = error;
    }

    private static int CallbackFailure(Exception error)
    {
        var owner = active;
        if (owner == null) return -1;
        owner.callbackError = error;
        owner.SetErrno(Runtime.EIO);
        return -1;
    }

    private static long SystemCallback(byte* command)
    {
        try { return active!.transport.System(Marshal.PtrToStringUTF8((nint)command)!); }
        catch (Exception error) { return CallbackFailure(error); }
    }

    private static Runtime.FILE* OpenCallback(byte* command, byte* mode)
    {
        try { return (Runtime.FILE*)active!.transport.Open(Marshal.PtrToStringUTF8((nint)command)!, Marshal.PtrToStringUTF8((nint)mode)!); }
        catch (Exception error) { CallbackFailure(error); return null; }
    }

    private static int CloseCallback(Runtime.FILE* stream)
    {
        try { return active!.transport.Close((nint)stream); }
        catch (Exception error) { return CallbackFailure(error); }
    }

    private static void Redirect(string path, ReadOnlySpan<byte> mode, Runtime.FILE* stream)
    {
        byte[] name = Encoding.UTF8.GetBytes(Path.GetFullPath(path) + "\0");
        fixed (byte* p = name) fixed (byte* m = mode)
            if (Runtime.freopen(p, m, stream) == null)
                throw new IOException($"Cannot redirect translated initdb stream to {path} (errno {Runtime.errno}).");
    }

    /// <summary>Arguments include the real argv[0]. Returns the original
    /// main/exit status after draining its actual pglitec atexit callbacks.</summary>
    public int Run(IReadOnlyList<string> arguments, string inputPath, string outputPath, string errorPath)
    {
        RequireOwner();
        if (running || used || active != null)
            throw new InvalidOperationException("Each original initdb invocation requires a fresh owner.");
        using var argv = new InitializationArguments(arguments);
        using var environment = new InitializationEnvironment();
        used = running = true;
        active = this;
        try
        {
            using var binding = context.Enter();
            Redirect(inputPath, "rb\0"u8, Runtime.stdin);
            Redirect(outputPath, "wb\0"u8, Runtime.stdout);
            Redirect(errorPath, "wb\0"u8, Runtime.stderr);
            Frontend.dpg_initdb_install(&SystemCallback, &OpenCallback, &CloseCallback);
            try
            {
                try { return Frontend.dpg_initdb_start(&Frontend.main, argv.Count, argv.Pointer); }
                catch (Runtime.RuntimeTerminationException exit) when (ReferenceEquals(exit.Owner, runtime) &&
                    exit.Termination.Kind == Runtime.RuntimeTerminationKind.Exit)
                { return exit.Termination.Status; }
            }
            finally
            {
                // Cleanup can still use argv and the original process callbacks.
                try { Frontend.pgl_run_atexit_funcs(); }
                finally
                {
                    Frontend.dpg_initdb_uninstall();
                    if (Runtime.fflush(null) != 0)
                        throw new IOException($"Flushing translated initdb output failed (errno {Runtime.errno}).");
                }
            }
        }
        finally { active = null; running = false; }
    }

    public void Dispose()
    {
        if (disposed) return;
        RequireOwner();
        if (running) throw new InvalidOperationException("Cannot dispose an entering initdb owner.");
        // Close abandoned frontend FILEs while their actual owner is alive.
        try { transport.Dispose(); }
        finally { context.Dispose(); disposed = true; }
    }
}
