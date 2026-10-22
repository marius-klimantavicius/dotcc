using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Managed.PGlite.Initialization;

/// <summary>Stable UTF-8 argv storage retained through translated cleanup.</summary>
internal sealed unsafe class InitializationArguments : IDisposable
{
    public int Count { get; }
    public byte** Pointer { get; private set; }

    public InitializationArguments(IReadOnlyList<string> arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        if (arguments.Count == 0) throw new ArgumentException("argv[0] is required.", nameof(arguments));
        Count = arguments.Count;
        Pointer = (byte**)NativeMemory.AllocZeroed(checked((nuint)(Count + 1)), (nuint)sizeof(byte*));
        try
        {
            for (int i = 0; i < Count; i++)
            {
                string value = arguments[i];
                ArgumentNullException.ThrowIfNull(value);
                if (value.Contains('\0')) throw new ArgumentException("NUL in initdb/backend argument.");
                int count = Encoding.UTF8.GetByteCount(value);
                Pointer[i] = (byte*)NativeMemory.Alloc(checked((nuint)count + 1));
                Encoding.UTF8.GetBytes(value, new Span<byte>(Pointer[i], count));
                Pointer[i][count] = 0;
            }
        }
        catch { Dispose(); throw; }
    }

    public void Dispose()
    {
        if (Pointer == null) return;
        for (int i = 0; i < Count; i++) NativeMemory.Free(Pointer[i]);
        NativeMemory.Free(Pointer);
        Pointer = null;
    }
}

/// <summary>Restore the environment variables the selected original frontend
/// and backend modify. A child process's setenv must not alter its initdb
/// parent, and initdb's PGDATA must not escape the serialized owning API.</summary>
internal sealed class InitializationEnvironment : IDisposable
{
    private static readonly string[] names = ["PGDATA", "PGCLIENTENCODING", "PGLOCALEDIR", "PGSYSCONFDIR",
        "LC_ALL", "LC_COLLATE", "LC_CTYPE", "LC_MESSAGES", "LC_MONETARY", "LC_NUMERIC", "LC_TIME"];
    private readonly string?[] values = new string?[names.Length];

    public InitializationEnvironment()
    {
        for (int i = 0; i < names.Length; i++) values[i] = Environment.GetEnvironmentVariable(names[i]);
    }

    public void Dispose()
    {
        for (int i = 0; i < names.Length; i++) Environment.SetEnvironmentVariable(names[i], values[i]);
    }
}
