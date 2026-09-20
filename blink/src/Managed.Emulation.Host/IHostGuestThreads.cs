namespace Managed.Emulation.Host;

/// <summary>Private interpreter handoff to its authored C# lifecycle owner.
/// A successful Start transfers the existing Machine; failure retains caller
/// ownership. Exit callbacks must unwind, never return or free the Machine.</summary>
public interface IHostGuestThreads
{
    int Start(nint machine);
    void Exit(nint machine, int status, bool group);
    void StopOthers(nint system);
    int Signal(long thread, int signal);
}
