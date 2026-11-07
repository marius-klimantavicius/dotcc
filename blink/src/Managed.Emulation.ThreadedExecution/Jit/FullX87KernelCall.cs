namespace Managed.Emulation.Execution;

// Translation-time authenticated snapshot. Parent may construct ONLY while the
// binding's owner is validated under Authority.Gate. No lifetime is acquired by
// copying this value: the original TB/store epoch + lease + authority enrollment
// must span native entry, kernel, continuation and return. Never retain a managed
// FullX87KernelBinding/catalog reference in FullBlockInstruction or native IR.
internal readonly record struct FullX87KernelCall(byte Key, nint Entry, ulong OwnerCookie, int CodeBytes);
