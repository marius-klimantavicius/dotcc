using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Blink = Managed.Emulation.BlinkCore;

unsafe partial class Program
{
    static void CheckNativeLimits(byte* machine, byte* data)
    {
        JitInstruction[] block = [
            new(Ip, 4, JitOperation.LoadMov, 32, new(15, 0), default,
                SourceWidth: 32, Address: new(-1, -1, 0, (long)Page + 32)),
            new(Ip + 4, 10, JitOperation.Mov, 64, new(0, 0), new(0, 42, true)),
            new(Ip + 14, 2, JitOperation.Jump, 64, default, default, Target: Ip) ];
        byte[] memoryCounts = JitMemoryCounts.Build(block);
        JitDataDescriptor descriptor = new() { Page = Page, Host = (nint)data,
            Identity = (ulong)data, Generation = 1, ValidEnd = 4096, Permissions = 1 };
        foreach (int compiledCap in new[] { 1, 2, 3, 31, 32 })
        {
            using var code = Create(block, compiledCap);
            foreach (uint requested in Enumerable.Range(0, 35).Select(x => (uint)x).Append(uint.MaxValue))
            for (ulong budget = 0; budget <= 34; ++budget)
            {
                Reset(machine);
                var expected = new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).ToArray();
                uint cap = requested == 0 ? (uint)compiledCap : Math.Min(requested, (uint)compiledCap);
                ulong count = Math.Min(budget, cap);
                if (count != 0)
                {
                    expected[8] = 0;
                    ulong next = count % 3 == 0 ? Ip : count % 3 == 1 ? Ip + 4 : Ip + 14;
                    BinaryPrimitives.WriteUInt64LittleEndian(expected, next);
                    BinaryPrimitives.WriteUInt64LittleEndian(expected.AsSpan(24 + 15 * 8),
                        BinaryPrimitives.ReadUInt32LittleEndian(new ReadOnlySpan<byte>(data + 32, 4)));
                    if (count >= 2) BinaryPrimitives.WriteUInt64LittleEndian(expected.AsSpan(24), 42);
                }
                long issued = 0, completed = 0; int stop = 0;
                JitFrame frame = new() { Machine = machine, Issued = &issued, Completed = &completed, Stop = &stop,
                    Budget = budget, NativeLimit = requested, DataGeneration = 1,
                    DataDescriptors = &descriptor, DataDescriptorCount = 1 };
                ((delegate* unmanaged<JitFrame*, int>)(void*)code.Entry)(&frame);
                bool rejected = budget < cap;
                Check(frame.NativeLimit == cap && frame.Executed == count && frame.Executed <= 32,
                    "effective native continuation limit");
                Check(frame.Exit == (rejected ? JitExit.Budget : JitExit.Continue) &&
                    issued == (long)count + (rejected ? 1 : 0) && completed == (long)count,
                    "requested native limit exact issue/completion");
                Check(memoryCounts[(int)count] == (count + 2) / 3, "remaining-cap memory prefix");
                Check(new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).SequenceEqual(expected),
                    "requested native limit complete Machine state");
            }
        }
    }
}
