namespace Managed.Emulation.Execution;

internal static partial class FullBlockEmitter
{
    internal static bool CanInlineChecksum(JitInstruction i)=>i.Operation==JitOperation.ChecksumCrc32;
    internal static bool ValidateChecksum(JitInstruction i)
    {
        if(!CanInlineChecksum(i))return false;
        if(i.Width is not(32 or 64)||i.SourceWidth is not(8 or 16 or 32 or 64)||
            i.Width==64&&i.SourceWidth is not(8 or 64)||i.Width==32&&i.SourceWidth==64||
            i.Length is <5 or >15||i.Target!=0||i.Condition!=0||i.Options!=0||i.Extra!=default||
            !IntegerRegister(i.Destination,i.Width)||i.Destination.Immediate!=0||i.Destination.ByteOffset!=0||
            (i.Source.IsMemory?i.Source!=new JitOperand(0,0,IsMemory:true):
                !IntegerRegister(i.Source,i.SourceWidth)||i.Source.IsImmediate||i.Source.Immediate!=0))
            throw new ArgumentException("Native CRC32 operand/metadata");
        if(i.Source.IsMemory)
        {
            var a=i.Address;
            if(a.Base is <-1 or >15||a.Index is <-1 or >15||a.Scale is <0 or >3||a.AddressWidth is not(32 or 64)||
                a.Segment is <-1 or >5||a.RipRelative&&(a.Base!=-1||a.Index!=-1))throw new ArgumentException("Native CRC32 address");
        }
        else if(i.Address!=default)throw new ArgumentException("Native CRC32 unused address");
        return true;
    }
    private ref partial struct Writer
    {
        private bool TryChecksumOperation(in JitInstruction i)
        {
            if(!CanInlineChecksum(i))return false;
            if(i.Source.IsMemory)
            {
                ScalarMiscCheckCanonical(i,i.SourceWidth);
                NativeMemoryPrepare(i,FullMemoryAccess.Read,i.SourceWidth);NativeMemoryLoad(i.SourceWidth);
                IntegerMov(9,0);
            }
            else IntegerLoad(i.Source,i.SourceWidth,9);
            // Snapshot the source before reading the accumulator: high-byte,
            // source=destination and memory EA=destination aliases are legal.
            IntegerLoad(i.Destination,32,8);IntegerImmediate(2,(uint)i.SourceWidth,32);
            int loop=position;
            IntegerMov(0,8,32);IntegerRR(0x31,0,9,32);IntegerConstant(4,0,1);
            IntegerShift(5,8,32,1);IntegerShift(5,9,64,1);
            IntegerRR(0x85,0,0,32);int zero=NearCondition(0x84);
            IntegerConstant(6,8,0x82f63b78);Patch(zero,position);
            IntegerConstant(5,2,1);int again=NearCondition(0x85);Patch(again,loop);
            // Every iteration writes r8d: CRC32 always zeroes destination high32,
            // including its 64-bit destination form. Guest flags are untouched.
            Store(i.Destination,i.Width);if(i.Source.IsMemory)NativeMemoryCount();return true;
        }
    }
}
