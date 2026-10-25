using System.Buffers.Binary;
using System.Text;
using Managed.Emulation.Host;

namespace Managed.Emulation.Execution;

/// <summary>Only a PT_INTERP policy check. Upstream LoadProgram/LoadElf owns loading,
/// mapping, auxv and relocation handoff to the guest interpreter.</summary>
internal static class GuestElfPolicy
{
    internal static void Validate(InstanceIo io, string path, bool allowInterpreter)
    {
        var opened = io.OpenFile(path, FileAccessMode.Read);
        if (!opened.Succeeded)
            throw new InvalidOperationException($"Cannot open guest executable '{path}' in the guest filesystem ({opened.Error}); no guest instructions executed.");
        int fd = opened.Value;
        try
        {
            byte[] header = new byte[64];
            if (!Read(header, 0) || !header.AsSpan(0, 6).SequenceEqual(new byte[] { 127, 69, 76, 70, 2, 1 }) ||
                BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(18)) != 62) return;
            ulong offset = BinaryPrimitives.ReadUInt64LittleEndian(header.AsSpan(32));
            int stride = BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(54));
            int count = BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(56));
            if (stride < 56) return; // Leave unsupported formats to the upstream loader.
            byte[] phdr = new byte[56];
            for (int i = 0; i < count; ++i)
            {
                if (!Read(phdr, checked((long)offset + (long)i * stride))) return;
                if (BinaryPrimitives.ReadUInt32LittleEndian(phdr) != 3) continue;
                if (!allowInterpreter)
                    throw new InvalidOperationException("ELF PT_INTERP requires ExecutionOptions.AllowElfInterpreter=true; no guest instructions executed.");
                ulong length = BinaryPrimitives.ReadUInt64LittleEndian(phdr.AsSpan(32));
                if (length is < 2 or > 4096)
                    throw new InvalidOperationException("Guest ELF interpreter path exceeds the supported path bound; no guest instructions executed.");
                byte[] text = new byte[(int)length];
                if (!Read(text, checked((long)BinaryPrimitives.ReadUInt64LittleEndian(phdr.AsSpan(8)))) || text[^1] != 0)
                    throw new InvalidOperationException("Cannot read guest ELF interpreter path; no guest instructions executed.");
                string interpreter = new UTF8Encoding(false, true).GetString(text.AsSpan(0, text.Length - 1));
                var loader = io.OpenFile(interpreter, FileAccessMode.Read);
                if (!loader.Succeeded)
                    throw new InvalidOperationException($"ELF interpreter '{interpreter}' is unavailable in the guest filesystem ({loader.Error}); no guest instructions executed.");
                io.Close(loader.Value);
                return;
            }
        }
        finally { io.Close(fd); }

        bool Read(byte[] buffer, long offset)
        {
            int done = 0;
            while (done < buffer.Length)
            {
                var result = io.ReadAt(fd, buffer.AsSpan(done), checked(offset + done));
                if (!result.Succeeded || result.Value == 0) return false;
                done += result.Value;
            }
            return true;
        }
    }
}
