namespace Managed.Emulation.Host;

public readonly record struct HostGuestSignalInfo(int ProcessId, uint UserId);

/// <summary>Private interpreter handoff to its authored C# lifecycle owner.
/// A successful Start transfers the existing Machine; failure retains caller
/// ownership. Exit callbacks must unwind, never return or free the Machine.</summary>
public interface IHostGuestThreads
{
    int Start(nint machine);
    void Exit(nint machine, int status, bool group);
    void StopOthers(nint system);
    int Signal(long thread, int signal);
    int WakeSignal(nint machine);
    void RunSignalActor(nint machine);
    void SignalCheckpoint(nint machine);
    void EnqueueSignalInfo(nint machine, int signal, int processId, uint userId);
    void DeliverThreadSignal(nint machine, int signal, int processId, uint userId);
    HostGuestSignalInfo? TakeSignalInfo(nint machine, int signal, bool discard);
}
