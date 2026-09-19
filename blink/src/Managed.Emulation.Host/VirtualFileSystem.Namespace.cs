using System.Text;

namespace Managed.Emulation.Host;

public sealed partial class VirtualFileSystem
{
    public HostResult<int> MakeDirectory(string path, uint mode, string cwd = "/")
    {
        lock (sync)
        {
            if (disposed) return Fail<int>(GuestError.BadDescriptor);
            if ((mode & ~0x1ffU) != 0) return Fail<int>(GuestError.Unsupported);
            // mkdir permits a trailing separator on its new final component.
            var resolved = Resolve(path.Length > 1 ? path.TrimEnd('/') : path, cwd);
            if (!resolved.Succeeded) return Fail<int>(resolved.Error);
            string name = resolved.Value;
            if (files.ContainsKey(name) || directories.Contains(name)) return Fail<int>(GuestError.Exists);
            if (!directoryNodes.TryGetValue(Parent(name), out var parent)) return Fail<int>(GuestError.NoEntry);
            if (!CanAddName(name)) return Fail<int>(GuestError.NoSpace);
            var node = new Node([], false, nextInode, 0x4000 | mode);
            // Build replacement indices before publishing. Allocation failures
            // cannot create a directory in only one of the two indices.
            var nextDirectories = new HashSet<string>(directories, StringComparer.Ordinal) { name };
            var nextNodes = new Dictionary<string, Node>(directoryNodes, StringComparer.Ordinal) { [name] = node };
            directories = nextDirectories;
            directoryNodes = nextNodes;
            ++nextInode;
            pathBytes += Encoding.UTF8.GetByteCount(name);
            parent.ModifyTime = parent.ChangeTime = VirtualFileTime.UtcNow;
            return HostResult<int>.Success(0);
        }
    }

    private static bool Within(string path, string directory) => path == directory
        || directory == "/" || path.StartsWith(directory + "/", StringComparison.Ordinal);
    private static bool SpecialFinalComponent(string path)
        => path.TrimEnd('/').Split('/').LastOrDefault() is "." or "..";

    public HostResult<int> Unlink(string path, bool removeDirectory = false, string cwd = "/", string protectedCwd = "/")
    {
        lock (sync)
        {
            if (disposed) return Fail<int>(GuestError.BadDescriptor);
            if (SpecialFinalComponent(path)) return Fail<int>(GuestError.Invalid);
            var resolved = Resolve(path, cwd);
            if (!resolved.Succeeded) return Fail<int>(resolved.Error);
            string name = resolved.Value;
            if (name == "/" || name == protectedCwd) return Fail<int>(GuestError.Busy);
            bool isDirectory = directoryNodes.TryGetValue(name, out var node);
            if (!isDirectory && !files.TryGetValue(name, out node)) return Fail<int>(GuestError.NoEntry);
            if (removeDirectory && !isDirectory) return Fail<int>(GuestError.NotDirectory);
            if (!removeDirectory && isDirectory) return Fail<int>(GuestError.IsDirectory);
            if (node!.Immutable) return Fail<int>(GuestError.ReadOnly);
            if (isDirectory && files.Keys.Concat(directories).Any(child => child != name && Within(child, name)))
                return Fail<int>(GuestError.NotEmpty);
            bool keep = descriptors.Values.Any(file => ReferenceEquals(file.Node, node));
            var nextDetached = new HashSet<Node>(detachedNodes);
            if (keep) nextDetached.Add(node);
            var parent = directoryNodes[Parent(name)];
            var now = VirtualFileTime.UtcNow;
            // No allocations remain after mutation starts.
            if (isDirectory) { directoryNodes.Remove(name); directories.Remove(name); }
            else files.Remove(name);
            detachedNodes = nextDetached;
            node.Linked = false;
            if (!keep) writableBytes -= node.Bytes.Length;
            pathBytes -= Encoding.UTF8.GetByteCount(name);
            node.ChangeTime = now;
            parent.ModifyTime = parent.ChangeTime = node.ChangeTime;
            return HostResult<int>.Success(0);
        }
    }

    /// <summary>Atomically replace namespace entries and return the rebased cwd.
    /// Open descriptions of replaced nodes retain their original node.</summary>
    public HostResult<string> Rename(string oldPath, string newPath, string oldCwd = "/", string newCwd = "/", string protectedCwd = "/")
    {
        lock (sync)
        {
            if (disposed) return Fail<string>(GuestError.BadDescriptor);
            if (SpecialFinalComponent(oldPath) || SpecialFinalComponent(newPath)) return Fail<string>(GuestError.Invalid);
            var source = Resolve(oldPath, oldCwd);
            if (!source.Succeeded) return Fail<string>(source.Error);
            string from = source.Value;
            bool isDirectory = directoryNodes.TryGetValue(from, out var node);
            if (!isDirectory && !files.TryGetValue(from, out node)) return Fail<string>(GuestError.NoEntry);
            // Like mkdir, a renamed directory may acquire a new name ending
            // in separators. A file still requires ordinary trailing-slash validation.
            var destination = Resolve(isDirectory && newPath.Length > 1 ? newPath.TrimEnd('/') : newPath, newCwd);
            if (!destination.Succeeded) return Fail<string>(destination.Error);
            string to = destination.Value;
            if (from == to) return HostResult<string>.Success(protectedCwd);
            if (from == "/" || to == "/" || to == protectedCwd) return Fail<string>(GuestError.Busy);
            if (node!.Immutable) return Fail<string>(GuestError.ReadOnly);
            if (isDirectory && Within(to, from)) return Fail<string>(GuestError.Invalid);
            bool targetDirectory = directoryNodes.TryGetValue(to, out var target);
            if (!targetDirectory) files.TryGetValue(to, out target);
            if (target != null)
            {
                if (target.Immutable) return Fail<string>(GuestError.ReadOnly);
                if (isDirectory != targetDirectory) return Fail<string>(isDirectory ? GuestError.NotDirectory : GuestError.IsDirectory);
                if (targetDirectory && files.Keys.Concat(directories).Any(child => child != to && Within(child, to)))
                    return Fail<string>(GuestError.NotEmpty);
            }
            if (!directoryNodes.ContainsKey(Parent(to))) return Fail<string>(GuestError.NoEntry);
            var movedFiles = files.Where(pair => pair.Key == from || isDirectory && Within(pair.Key, from)).ToArray();
            var movedDirs = directoryNodes.Where(pair => isDirectory && Within(pair.Key, from)).ToArray();
            if (movedFiles.Concat(movedDirs).Any(pair => pair.Value.Immutable)) return Fail<string>(GuestError.ReadOnly);
            string Rebase(string path) => to + path[from.Length..];
            var renamedFiles = movedFiles.Select(pair => new KeyValuePair<string, Node>(Rebase(pair.Key), pair.Value)).ToArray();
            var renamedDirs = movedDirs.Select(pair => new KeyValuePair<string, Node>(Rebase(pair.Key), pair.Value)).ToArray();
            if (renamedFiles.Concat(renamedDirs).Any(pair => Encoding.UTF8.GetByteCount(pair.Key) > 4096))
                return Fail<string>(GuestError.NameTooLong);
            long nextPathBytes = pathBytes - (target == null ? 0 : Encoding.UTF8.GetByteCount(to))
                - movedFiles.Concat(movedDirs).Sum(pair => (long)Encoding.UTF8.GetByteCount(pair.Key))
                + renamedFiles.Concat(renamedDirs).Sum(pair => (long)Encoding.UTF8.GetByteCount(pair.Key));
            if (nextPathBytes > pathBytesLimit) return Fail<string>(GuestError.NoSpace);
            var nextFiles = new Dictionary<string, Node>(files, StringComparer.Ordinal);
            var nextNodes = new Dictionary<string, Node>(directoryNodes, StringComparer.Ordinal);
            nextFiles.Remove(to); nextNodes.Remove(to);
            foreach (var pair in movedFiles) nextFiles.Remove(pair.Key);
            foreach (var pair in movedDirs) nextNodes.Remove(pair.Key);
            foreach (var pair in renamedFiles) nextFiles.Add(pair.Key, pair.Value);
            foreach (var pair in renamedDirs) nextNodes.Add(pair.Key, pair.Value);
            var nextDirectories = new HashSet<string>(nextNodes.Keys, StringComparer.Ordinal);
            bool keepTarget = target != null && descriptors.Values.Any(file => ReferenceEquals(file.Node, target));
            var nextDetached = new HashSet<Node>(detachedNodes);
            if (keepTarget) nextDetached.Add(target!);
            var changedDescriptions = descriptors.Values.Distinct()
                .Where(file => file.Node.Linked && file.DirectoryPath != null && Within(file.DirectoryPath, from))
                .Select(file => (File: file, Path: Rebase(file.DirectoryPath!))).ToArray();
            string nextCwd = isDirectory && Within(protectedCwd, from) ? Rebase(protectedCwd) : protectedCwd;
            var oldParent = directoryNodes[Parent(from)];
            var newParent = directoryNodes[Parent(to)];
            var now = VirtualFileTime.UtcNow;
            files = nextFiles; directoryNodes = nextNodes; directories = nextDirectories;
            detachedNodes = nextDetached; pathBytes = nextPathBytes;
            if (target != null)
            {
                target.Linked = false; target.ChangeTime = now;
                if (!keepTarget) writableBytes -= target.Bytes.Length;
            }
            foreach (var changed in changedDescriptions) changed.File.DirectoryPath = changed.Path;
            node.ChangeTime = now;
            oldParent.ModifyTime = oldParent.ChangeTime = newParent.ModifyTime = newParent.ChangeTime = now;
            return HostResult<string>.Success(nextCwd);
        }
    }
}
