#nullable enable
using System;
using System.IO;
using Microsoft.Win32.SafeHandles;

namespace Edg.Runtime;

public sealed unsafe partial class PosixContext
{
    private static bool AbsolutePath(string name) => name.StartsWith('/') ||
        OperatingSystem.IsWindows() && Path.IsPathRooted(name);

    private bool CheckDirectory(Descriptor directory)
    {
        if (directory.Closed) { SetError(9); return false; }
        if (directory.Device != 0 || directory.Socket != null ||
            (PosixPlatform.Fstat(directory.Handle, directory.Path).st_mode & 0xf000) != 0x4000)
        { SetError(20); return false; }
        return true;
    }

    public int OpenAt(int directory, sbyte* path, int flags, int mode = 438)
    {
        if (path == null) { SetError(14); return -1; }
        SafeFileHandle? handle = null;
        try
        {
            string name = DecodePath(path);
            if (directory == -100 || AbsolutePath(name)) return Open(path, flags, mode);
            Descriptor? parent = Find(directory);
            if (parent == null) return -1;
            string diagnosticPath;
            lock (parent.Gate)
            {
                if (!CheckDirectory(parent)) return -1;
                handle = PosixPlatform.OpenAt(parent.Handle, name, flags, mode);
                // Used for diagnostics/lock bookkeeping only, never relative resolution.
                diagnosticPath = Path.Combine(parent.Path, name);
            }
            return RegisterDescriptor(handle, diagnosticPath, flags);
        }
        catch (Exception error) when (PlatformFailure(error)) { handle?.Dispose(); return Fail(error); }
    }

    public int UnlinkAt(int directory, sbyte* path, int flags)
    {
        if ((flags & ~0x200) != 0) { SetError(22); return -1; }
        if (path == null) { SetError(14); return -1; }
        try
        {
            string name = DecodePath(path);
            if (directory == -100 || AbsolutePath(name))
                return flags == 0 ? Unlink(path) : Rmdir(path);
            Descriptor? parent = Find(directory);
            if (parent == null) return -1;
            lock (parent.Gate)
            {
                if (!CheckDirectory(parent)) return -1;
                PosixPlatform.UnlinkAt(parent.Handle, name, flags != 0);
                return 0;
            }
        }
        catch (Exception error) when (PlatformFailure(error)) { return Fail(error); }
    }

    public int Truncate(sbyte* path, long length)
    {
        if (path == null) { SetError(14); return -1; }
        if (length < 0) { SetError(22); return -1; }
        try
        {
            using var handle = PosixPlatform.Open(Resolve(path), 1, 0);
            RandomAccess.SetLength(handle, length);
            return 0;
        }
        catch (Exception error) when (PlatformFailure(error)) { return Fail(error); }
    }

    public int Link(sbyte* existing, sbyte* created)
    {
        if (existing == null || created == null) { SetError(14); return -1; }
        try { PosixPlatform.HardLink(Resolve(existing), Resolve(created)); return 0; }
        catch (Exception error) when (PlatformFailure(error)) { return Fail(error); }
    }

    public int Symlink(sbyte* target, sbyte* created)
    {
        if (target == null || created == null) { SetError(14); return -1; }
        try
        {
            string value = DecodePath(target), destination = Resolve(created);
            if (OperatingSystem.IsWindows())
            {
                string resolved = ResolveWindowsPath(value, Path.GetDirectoryName(destination)!);
                // Windows symlinks require a file/directory kind at creation.
                // A missing target cannot establish that kind: do not create a
                // file-only link that would silently break a future directory.
                if (!Path.Exists(resolved)) { SetError(95); return -1; }
                if (AbsolutePath(value)) value = resolved;
                if ((File.GetAttributes(resolved) & FileAttributes.Directory) != 0)
                    Directory.CreateSymbolicLink(destination, value);
                else File.CreateSymbolicLink(destination, value);
            }
            else File.CreateSymbolicLink(destination, value);
            return 0;
        }
        catch (Exception error) when (PlatformFailure(error)) { return Fail(error); }
    }

    public long Pathconf(sbyte* path, int name)
    {
        if (path == null) { SetError(14); return -1; }
        if (name != 4) { SetError(22); return -1; }
        try
        {
            _ = PosixPlatform.Stat(Resolve(path), true);
            return 4096; // Portable PATH_MAX, also enforced by Realpath/Getcwd.
        }
        catch (Exception error) when (PlatformFailure(error)) { return Fail(error); }
    }
}
