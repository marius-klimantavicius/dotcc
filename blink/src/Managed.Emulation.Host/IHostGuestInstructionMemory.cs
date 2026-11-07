namespace Managed.Emulation.Host;

/// <summary>Passive guest-memory readers defer anonymous page clearing/recycling
/// and physical reclamation without holding a mapping lock across semantics.</summary>
public interface IHostGuestInstructionMemory
{
    void PhysicalDataExposed(nint machine, nint page) { }
    ulong FindPageTableEntry(nint machine, ulong page);
    void RecycleAnonymousPage(nint system, nint page, bool clear);
}
