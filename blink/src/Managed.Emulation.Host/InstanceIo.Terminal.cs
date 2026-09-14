namespace Managed.Emulation.Host;

public sealed partial class InstanceIo
{
    /// <summary>Every descriptor in this model is explicitly nonterminal.
    /// This lookup neither borrows payload pointers nor invokes host ioctls.</summary>
    public HostResult<int> TerminalOperation(int descriptor)
    {
        lock (sync)
            return Fail<int>(Find(descriptor, out _) ? (GuestError)25 : GuestError.BadDescriptor);
    }
}
