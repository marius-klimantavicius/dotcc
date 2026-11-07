namespace Managed.Emulation.Execution;

internal static partial class FullBlockEmitter
{
    private ref partial struct Writer
    {
        // A new dense operation may follow a sparse store in the same body.
        // Preserve an existing dense Ready request, but cancel a different tag
        // completely before any dense state/pointer can be considered. RDX is
        // scratch; neither guest state nor volatile SIMD inputs are touched.
        private void NativeNormalizeDenseDescriptor(int register)
        {
            IntegerPrefix(32,register);Bytes(0x81);Memory(7,register&7,0);U32(FullStackMemoryDescriptor.CurrentVersion);
            int dense=NearCondition(0x84);
            Bytes(0x41,0xf7,0x82);U32(324);U32((uint)FullMemoryFlags.BarrierTicket);
            ExitCondition(0x85,FullBlockExit.Invalidated); // never erase an owned tagged ticket
            IntegerImmediate(2,1,32); // Version1 and Count0 in one qword
            IntegerPrefix(64,register,2);Bytes(0x89);Memory(2,register&7,0);
            IntegerRR(0x31,2,2,32);
            for(int offset=8;offset<=96;offset+=8)
            {
                if(offset is 32 or 56 or 64 or 72)continue;
                IntegerPrefix(64,register,2);Bytes(0x89);Memory(2,register&7,offset);
            }
            Patch(dense,position);
        }
    }
}
