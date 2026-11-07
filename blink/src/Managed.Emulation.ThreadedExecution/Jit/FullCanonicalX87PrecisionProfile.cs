namespace Managed.Emulation.Execution;

internal static class FullCanonicalX87PrecisionProfile
{
    internal const string Id = "LinuxSoftware.PC01IsPC64.RawCWPreserved.v1";
    internal static int ArithmeticBits(ushort rawControl) => (rawControl >> 8 & 3) switch
    { 0 => 24, 2 => 53, _ => 64 };
    // This describes a reserved encoding in our software profile, not a claim
    // about every Intel/AMD machine. Raw control is never normalized in Work,
    // Machine, saved images or clone/signal transports. RC and masks are raw.
}
