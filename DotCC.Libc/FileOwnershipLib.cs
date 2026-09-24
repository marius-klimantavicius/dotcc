#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    // Keeping this table weak avoids making a runtime identity a process root.
    // RuntimeContext.Dispose releases native handles and backings deterministically.
    private static readonly ConditionalWeakTable<RuntimeContext, DescriptorState> descriptorStates = new();
    private static DescriptorState FileDescriptors => descriptorStates.GetValue(RuntimeState, static _ => new());

    private sealed class DescriptorState
    {
        internal readonly Lock Sync = new();
        internal readonly List<FileSlot?> Files = new()
        {
            new() { Kind = FileSlot.K.In },
            new() { Kind = FileSlot.K.Out, StatusFlags = 1 },
            new() { Kind = FileSlot.K.Err, StatusFlags = 1 },
        };
        internal readonly HashSet<nint> Handles = new();
        internal FILE* Stdin, Stdout, Stderr;
        internal byte* Tmpnam;
    }

    private static FILE* AllocateFileHandle(int fd)
    {
        var state = FileDescriptors;
        lock (state.Sync)
        {
            var handle = (FILE*)NativeMemory.Alloc((nuint)sizeof(FILE));
            handle->_slot = fd;
            state.Handles.Add((nint)handle);
            return handle;
        }
    }

    private static void DisposeFileDescriptors(RuntimeContext owner)
    {
        if (!descriptorStates.TryGetValue(owner, out var state)) return;
        lock (state.Sync)
        {
            // No runtime binding remains at disposal. Use this captured table,
            // never the ambient owner (which may be an entirely different server).
            foreach (var slot in state.Files)
            {
                if (slot is null) continue;
                try { slot.Writer?.Flush(); } catch (IOException) { }
                try { slot.Stream?.Dispose(); } catch (IOException) { }
                slot.Socket?.Dispose();
            }
            state.Files.Clear();
            foreach (nint handle in state.Handles) NativeMemory.Free((void*)handle);
            state.Handles.Clear();
            NativeMemory.Free(state.Tmpnam);
            state.Stdin = state.Stdout = state.Stderr = null;
            state.Tmpnam = null;
        }
        descriptorStates.Remove(owner);
    }
}
