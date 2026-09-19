namespace Managed.Emulation.Host;

public sealed partial class VirtualFileSystem
{
    /// <summary>This namespace contains ordinary files and directories only.
    /// Validate the proposed destination without publishing a special node.</summary>
    public HostResult<int> RejectSpecialNode(string path, string cwd = "/")
    {
        lock (sync)
        {
            if (disposed) return Fail<int>(GuestError.BadDescriptor);
            var resolved = Resolve(path, cwd);
            if (!resolved.Succeeded) return Fail<int>(resolved.Error);
            if (files.ContainsKey(resolved.Value) || directoryNodes.ContainsKey(resolved.Value))
                return Fail<int>(GuestError.Exists);
            if (!directories.Contains(Parent(resolved.Value))) return Fail<int>(GuestError.NoEntry);
            return Fail<int>(GuestError.Unsupported);
        }
    }
}
