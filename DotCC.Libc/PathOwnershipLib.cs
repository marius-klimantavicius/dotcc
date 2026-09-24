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
    private sealed class PathState
    {
        internal readonly Lock Sync = new();
        internal readonly Dictionary<nint, DirState> Directories = new();
    }

    private static readonly ConditionalWeakTable<RuntimeContext, PathState> pathStates = new();
    private static PathState OwnedPaths => pathStates.GetValue(RuntimeState, static _ => new());

    private static string CurrentWorkingDirectory
    {
        get
        {
            if (RuntimeContext.Current is not { } owner) return Directory.GetCurrentDirectory();
            lock (OwnedPaths.Sync) return owner.WorkingDirectory;
        }
    }

    // Empty paths retain their ordinary ENOENT/error behavior. A symbolic link's
    // target is data, not a path operand to resolve with the caller's cwd.
    private static string ResolvePath(byte* path) => ResolvePath(Str(path));
    private static string ResolvePath(string path)
    {
        if (path.Length == 0 || RuntimeContext.Current is null || global::System.IO.Path.IsPathFullyQualified(path)) return path;
        return global::System.IO.Path.Combine(CurrentWorkingDirectory, path);
    }

    private static string? PhysicalDirectory(string path)
    {
        string root = global::System.IO.Path.GetPathRoot(path)!;
        string current = root;
        // Resolve directory symlinks before consuming '..', as chdir does.
        foreach (string part in path[root.Length..].Split(new[] { global::System.IO.Path.DirectorySeparatorChar, global::System.IO.Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (part == ".") continue;
            if (part == "..") { current = global::System.IO.Path.GetDirectoryName(current) ?? root; continue; }
            string next = global::System.IO.Path.Combine(current, part);
            if (!Directory.Exists(next)) { errno = File.Exists(next) ? ENOTDIR : ENOENT; return null; }
            var directory = new DirectoryInfo(next);
            current = directory.LinkTarget is null ? next : directory.ResolveLinkTarget(true)!.FullName;
        }
        return current;
    }

    private static void DisposePathState(RuntimeContext owner)
    {
        if (!pathStates.TryGetValue(owner, out var state)) return;
        lock (state.Sync)
        {
            foreach (var directory in state.Directories)
            {
                NativeMemory.Free(directory.Value.Dirent);
                NativeMemory.Free((void*)directory.Key);
            }
            state.Directories.Clear();
        }
        pathStates.Remove(owner);
    }
}
