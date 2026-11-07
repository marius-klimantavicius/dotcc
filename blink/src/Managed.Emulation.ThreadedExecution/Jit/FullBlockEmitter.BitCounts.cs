namespace Managed.Emulation.Execution;

internal static partial class FullBlockEmitter
{
    internal static bool CanInlineBitCount(JitInstruction i)=>i.Operation is
        JitOperation.PopulationCount or JitOperation.LeadingZeroCount or JitOperation.TrailingZeroCount;

    internal static bool ValidateBitCount(JitInstruction i)
    {
        if(!CanInlineBitCount(i))return false;
        if(i.Length is <4 or >15 || i.Width is not(16 or 32 or 64) || i.SourceWidth!=i.Width ||
            i.Target!=0 || i.Condition!=0 || i.Options!=0 || i.Extra!=default ||
            !IntegerRegister(i.Destination,i.Width) || i.Destination.Immediate!=0 || i.Destination.ByteOffset!=0 ||
            (i.Source.IsMemory?i.Source!=new JitOperand(0,0,IsMemory:true):
                !IntegerRegister(i.Source,i.Width)||i.Source.Immediate!=0||i.Source.ByteOffset!=0||i.Address!=default))
            throw new ArgumentException("Native bit-count metadata/operands");
        if(i.Source.IsMemory)
        {
            var a=i.Address;
            if(a.Base is <-1 or >15 || a.Index is <-1 or >15 || a.Scale is <0 or >3 ||
                a.AddressWidth is not(32 or 64) || a.Segment is <-1 or >5 ||
                a.RipRelative&&(a.Base!=-1||a.Index!=-1))
                throw new ArgumentException("Native bit-count address");
        }
        return true;
    }

    private ref partial struct Writer
    {
        private bool TryBitCountOperation(in JitInstruction i)
        {
            if(!CanInlineBitCount(i))return false;
            if(i.Source.IsMemory)
            {
                ScalarMiscCheckCanonical(i,i.Width);
                NativeMemoryPrepare(i,FullMemoryAccess.Read,i.Width);NativeMemoryLoad(i.Width);
            }
            else IntegerLoad(i.Source,i.Width,0);
            IntegerMov(8,0);IntegerRR(0x85,0,0);
            Bytes(0x0f,0x94,0xc2,0x44,0x0f,0xb6,0xca); // r9d = original source==0
            if(i.Operation==JitOperation.PopulationCount)
            {
                // Fixed SWAR reduction, emitted as baseline integer operations.
                // No loop dispatch, helper, host POPCNT or guest flag mutation.
                IntegerMov(0,8);IntegerShift(5,0,64,1);
                IntegerImmediate(1,0x5555555555555555);IntegerRR(0x21,0,1);IntegerRR(0x29,8,0);
                IntegerMov(0,8);IntegerShift(5,0,64,2);
                IntegerImmediate(1,0x3333333333333333);IntegerRR(0x21,8,1);IntegerRR(0x21,0,1);IntegerRR(0x01,8,0);
                IntegerMov(0,8);IntegerShift(5,0,64,4);IntegerRR(0x01,8,0);
                IntegerImmediate(1,0x0f0f0f0f0f0f0f0f);IntegerRR(0x21,8,1);
                for(int shift=8;shift<=32;shift*=2)
                {IntegerMov(0,8);IntegerShift(5,0,64,shift);IntegerRR(0x01,8,0);}
                IntegerConstant(4,8,127,64);
                // POPCNT clears OF/SF/AF/CF/PF, ZF tests the original source.
                // Software parity input1 represents PF=0; raw PF also clears.
                IntegerMov(0,9,32);IntegerShift(4,0,32,6);IntegerConstant(1,0,1u<<24);
                IntegerMergeFlags(0xff0008d5);
            }
            else
            {
                IntegerImmediate(8,(uint)i.Width,32);int zero=NearCondition(0x84);
                IntegerPrefix(64,0,8);Bytes(0x0f,i.Operation==JitOperation.LeadingZeroCount?(byte)0xbd:(byte)0xbc,0xc0);
                if(i.Operation==JitOperation.LeadingZeroCount)IntegerConstant(6,8,(uint)(i.Width-1),64);
                Patch(zero,position);
                IntegerRR(0x85,8,8);Bytes(0x0f,0x94,0xc0,0x0f,0xb6,0xc0);
                IntegerShift(4,0,32,6);IntegerRR(0x09,0,9,32);IntegerMergeFlags(0x41);
            }
            Store(i.Destination,i.Width);
            if(i.Source.IsMemory)NativeMemoryCount();
            return true;
        }
    }
}
