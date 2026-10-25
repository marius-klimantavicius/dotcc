using System.Globalization;
using System.Text;
using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

public sealed unsafe partial class ThreadedGuestExecution
{
    // Anonymous-style identities intentionally omit backing filenames, offsets,
    // device and inode: upstream PTEs do not preserve reliable guest file
    // identity. Addresses and rwx come only from guest virtual page tables;
    // sharing comes from the mapping owner's software metadata, never /proc.
    private byte[] SnapshotProcessMaps()
    {
        const int maximumBytes = 1 << 20;
        var text = new StringBuilder();
        ulong pendingStart = 0, pendingEnd = 0;
        int pendingProtection = -1;
        bool pendingShared = false;
        void Flush()
        {
            if (pendingProtection < 0) return;
            string line = pendingStart.ToString("x", CultureInfo.InvariantCulture) + "-" +
                pendingEnd.ToString("x", CultureInfo.InvariantCulture) + " " +
                ((pendingProtection & 1) != 0 ? "r" : "-") +
                ((pendingProtection & 2) != 0 ? "w" : "-") +
                ((pendingProtection & 4) != 0 ? "x" : "-") +
                (pendingShared ? "s" : "p") + " 00000000 00:00 0\n";
            if (line.Length > maximumBytes - text.Length) throw new OutOfMemoryException("Guest maps snapshot exceeds 1 MiB.");
            text.Append(line);
        }
        void Walk(ulong table, int shift, ulong prefix)
        {
            byte* entries = program!.GetPageAddress(system, table, shift == 39);
            for (int index = 0; index < 512; ++index)
            {
                ulong entry = program.LoadPte(entries + index * 8);
                if ((entry & (ulong)Blink.PAGE_V) == 0) continue;
                ulong address = prefix | ((ulong)index << shift);
                if (shift == 39 && index >= 256) address |= 0xffff000000000000UL;
                if (shift > 12 && (entry & (ulong)Blink.PAGE_PS) == 0)
                {
                    Walk(entry, shift - 9, address);
                    continue;
                }
                int protection = program.GetProtection(entry);
                bool shared = false;
                // Reserved leaves are real mappings, including PROT_NONE.
                // File leaves already retain backing even before first use;
                // FindHostPage reads its registry without faulting a page in.
                if ((entry & (ulong)Blink.PAGE_HOST) != 0)
                    shared = program.BlinkHostMemoryFilePageShared(program.FindHostPage(entry)) != 0;
                ulong end = checked(address + (1UL << shift));
                if (pendingProtection == protection && pendingShared == shared && pendingEnd == address)
                    pendingEnd = end;
                else
                {
                    Flush();
                    pendingStart = address; pendingEnd = end;
                    pendingProtection = protection; pendingShared = shared;
                }
            }
        }
        if (system == null || program == null) throw new InvalidOperationException("Guest maps owner is unbound.");
        int error = Blink.Libc.pthread_mutex_lock(&system->mmap_lock);
        if (error != 0) throw new InvalidOperationException("Could not lock guest mappings: " + error);
        try
        {
            if (system->cr3 != 0) Walk(system->cr3, 39, 0);
            Flush();
        }
        finally { Blink.Libc.pthread_mutex_unlock(&system->mmap_lock); }
        return Encoding.ASCII.GetBytes(text.ToString());
    }
}
