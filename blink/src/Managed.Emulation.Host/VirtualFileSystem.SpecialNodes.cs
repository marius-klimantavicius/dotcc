namespace Managed.Emulation.Host;

public sealed partial class VirtualFileSystem
{
    /// <summary>Guest creation of special nodes remains unsupported, including
    /// in the built-in device namespace. Validate without publishing a node.</summary>
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
