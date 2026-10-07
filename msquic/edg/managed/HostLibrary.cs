namespace Managed.Transport;

// Authored bridge compiled into the same assembly as the untouched translation.
// It exposes the source-named global to the host's drained-rundown cleanup.
public static unsafe partial class MsQuic
{
    public static ref QUIC_LIBRARY HostLibrary => ref MsQuicLib;
}
