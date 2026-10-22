using System;
using System.IO;
using System.Text;
using Backend = Managed.PGlite.Translated.PGlite;
using Runtime = Managed.PGlite.Translated.PGlite.Libc;

namespace Managed.PGlite.Initialization;

/// <summary>Runs original PostgreSQL process entrypoints against a fresh
/// generated backend state for every initdb command. No session yields are
/// intercepted and no FILE pointer is imported from the frontend.</summary>
public sealed unsafe partial class InitializationBackend
{
    private readonly string modulePath;

    public InitializationBackend(string bootstrapPrefix)
    {
        modulePath = Path.Combine(Path.GetFullPath(bootstrapPrefix), "lib/postgresql/plpgsql");
        if (!File.Exists(modulePath)) throw new FileNotFoundException("Translated PL/pgSQL manifest is required.", modulePath);
    }

    private static void Redirect(string path, ReadOnlySpan<byte> mode, int stream)
    {
        byte[] name = Encoding.UTF8.GetBytes(Path.GetFullPath(path) + "\0");
        fixed (byte* p = name) fixed (byte* m = mode)
            if (Backend.pgl_freopen(p, m, stream) == null)
                throw new IOException($"Cannot redirect translated backend stream to {path} (errno {Runtime.errno}).");
    }

    public int Invoke(BootstrapInvocation request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (thread != Environment.CurrentManagedThreadId)
            throw new InvalidOperationException("Initdb backend commands must stay on their owning thread.");
        lock (gate)
        {
            if (active != null) throw new InvalidOperationException("A bootstrap backend is already entering.");
            active = this;
        }
        try
        {
            using var context = Backend.__DotCcCreateContext();
            using var argv = new InitializationArguments(request.Arguments);
            using var environment = new InitializationEnvironment();
            using var binding = context.Enter();
            runtime = Runtime.RuntimeContext.Current!;
            runtime.EnablePgliteSignals();
            Redirect(request.InputPath, "rb\0"u8, 0);
            Redirect(request.OutputPath, "wb\0"u8, 1);
            if (request.MergeStandardError)
            {
                if (Runtime.dup2(1, 2) != 2)
                    throw new IOException($"Cannot merge translated backend stderr (errno {Runtime.errno}).");
            }
            else Redirect(request.ErrorPath, "wb\0"u8, 2);
            byte[] path = Encoding.UTF8.GetBytes(modulePath + "\0");
            fixed (byte* p = path)
                if (Backend.dpg_static_register(p, Backend.dpg_plpgsql_module()) != 0)
                    throw new IOException($"Cannot register original translated PL/pgSQL (errno {Runtime.errno}).");
            int status;
            try
            {
                try { status = Backend.main(argv.Count, argv.Pointer); }
                catch (Runtime.RuntimeTerminationException exit) when (ReferenceEquals(exit.Owner, runtime) &&
                    exit.Termination.Kind == Runtime.RuntimeTerminationKind.Exit)
                { status = exit.Termination.Status; }
            }
            finally
            {
                // Keep context, argv and static export storage alive through
                // original fork cleanup, then release all runtime descriptors.
                try { Backend.pgl_run_atexit_funcs(); }
                finally
                {
                    if (Runtime.fflush(null) != 0)
                        throw new IOException($"Flushing translated backend output failed (errno {Runtime.errno}).");
                }
            }
            if ((uint)status > 255 || status is 99 or 100)
                throw new InvalidOperationException($"Unexpected bootstrap backend exit {status}.");
            return status;
        }
        finally { runtime = null; lock (gate) { active = null; } }
    }
}
