namespace Managed.Emulation.Execution;

// Worker-local, two-way negative admission hints. They can only select the
// interpreter; an expired or collided entry never authorizes native execution.
internal struct JitNegativeHint
{
    internal ulong Ip;
    internal long Until;
    internal int Delay;
    internal int HotCount;
}
