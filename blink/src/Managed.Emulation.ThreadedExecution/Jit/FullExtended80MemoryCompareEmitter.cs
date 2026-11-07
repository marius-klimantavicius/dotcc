using System;
using System.Buffers.Binary;

namespace Managed.Emulation.Execution;

// R155 SOURCE_READY_UNEXECUTED. Own-project R134 comparison method fork.
// Direct raw memory lift, ALL NaNs invalid. PC ignored; no quieted FLD input.
internal static class FullExtended80MemoryCompareEmitter
{
    internal const int CodeCapacity = 16384;
    internal static bool TryEmit(FullExtended80MemoryArithmeticFormat format, bool pop,
        Span<byte> destination, out int length, bool? windowsAbi = null) =>
        Emit(format, pop, destination, out length, windowsAbi ?? OperatingSystem.IsWindows(), false);
    internal static bool TryEmitContinuation(FullExtended80MemoryArithmeticFormat format, bool pop,
        Span<byte> destination, out int length) => Emit(format, pop, destination, out length, false, true);
    private static bool Emit(FullExtended80MemoryArithmeticFormat format, bool pop, Span<byte> bytes,
        out int length, bool windows, bool continuation)
    {
        length = 0;
        if ((uint)format > 3) return false;
        Span<int> positions = stackalloc int[128], pending = stackalloc int[128];
        positions.Fill(-1); pending.Fill(-1);
        var a = new Assembler(bytes[..Math.Min(bytes.Length, CodeCapacity)], positions, pending);
        a.Control(pop ? FullExtended80Control.Fcomp : FullExtended80Control.Fcom, format, windows, continuation);
        if (!a.Success) return false;
        length = a.Count; return true;
    }
    // Independent new class. Common span/fixup/GPR encoders and round skeleton
    // reuse the authored R126/R129 family, not external emulator source.
    // NEW deltas: two classifies before DE, zero-aware magnitude comparison,
    // typed SW/EFLAGS masks, completing compare DE, exact integral repack.
    // Scratch96..151; decoded raw memory B128/136/140/148. Caller256..383 immutable.
    private ref struct Assembler
    {
        private Span<byte> bytes;
        private Span<int> positions, pending;
        private int count, labelCount;
        private bool bad;
        internal int Count => count;
        internal bool Success
        {
            get { if (bad) return false; for (int i=0;i<labelCount;i++) if(pending[i]>=0)return false; return true; }
        }
        internal Assembler(Span<byte> b, Span<int> p, Span<int> h)
        { bytes=b; positions=p; pending=h; count=labelCount=0; bad=false; }
        private void Byte(int v) { if(count>=bytes.Length)bad=true; else bytes[count++]=(byte)v; }
        private void Int(int v)
        {
            if(bytes.Length-count<4){bad=true;return;}
            BinaryPrimitives.WriteInt32LittleEndian(bytes[count..],v); count+=4;
        }
        private int Label() { if(labelCount==positions.Length){bad=true;return 0;} return labelCount++; }
        private void Mark(int l)
        {
            if(bad)return;
            if(positions[l]>=0){bad=true;return;}
            positions[l]=count;
            for(int p=pending[l];p>=0;)
            { int next=BinaryPrimitives.ReadInt32LittleEndian(bytes[p..]); BinaryPrimitives.WriteInt32LittleEndian(bytes[p..],count-p-4);p=next; }
            pending[l]=-1;
        }
        // Conditions: B2 AE3 E4 NE5 BE6 A7 S8 NS9 L12 GE13 LE14 G15.
        private void Branch(int l,int cc=-1)
        {
            if(bad)return;
            if(cc<0)Byte(0xe9);else{Byte(0x0f);Byte(0x80+cc);}
            int p=count; Int(positions[l]<0?pending[l]:positions[l]-p-4);
            if(!bad&&positions[l]<0)pending[l]=p;
        }
        private void Prefix(bool wide,int reg,int rm)
        { int rex=0x40|(wide?8:0)|(reg>=8?4:0)|(rm>=8?1:0);if(rex!=0x40)Byte(rex); }
        private void Reg(int opcode,int dst,int src,bool wide=true)
        { Prefix(wide,src,dst);Byte(opcode);Byte(0xc0|((src&7)<<3)|(dst&7)); }
        private void Move(int dst,int src,bool wide=true)=>Reg(0x89,dst,src,wide);
        private void Constant(int r,ulong v)
        { Prefix(true,0,r);Byte(0xb8|(r&7));Int(unchecked((int)v));Int(unchecked((int)(v>>32))); }
        private void Small(int r,int v)
        { Prefix(false,0,r);Byte(0xb8|(r&7));Int(v); }
        private void Address(int r,int off) {Byte(0x83|((r&7)<<3));Int(off);}
        private void Read(int r,int off,int width=64)
        { Prefix(width==64,r,11);if(width==16){Byte(0x0f);Byte(0xb7);}else Byte(0x8b);Address(r,off); }
        private void Write(int r,int off,int width=64)
        { if(width==16)Byte(0x66);Prefix(width==64,r,11);Byte(0x89);Address(r,off); }
        private void Put(int off,int v) {Small(0,v);Write(0,off,32);}
        private void Op(int group,int r,int v,bool wide=true)
        { Prefix(wide,0,r);Byte(0x81);Byte(0xc0|(group<<3)|(r&7));Int(v); }
        private void Flag(int off,int bits)
        {Prefix(false,0,11);Byte(0x81);Address(1,off);Int(bits);}
        private void Test(int r,bool wide=true)=>Reg(0x85,r,r,wide);
        private void Shift(int group,int r,int n=-1,bool wide=true)
        {Prefix(wide,0,r);Byte(n<0?0xd3:0xc1);Byte(0xc0|(group<<3)|(r&7));if(n>=0)Byte(n);}
        private void Negate(int r) {Prefix(true,0,r);Byte(0xf7);Byte(0xd8|(r&7));}
        private void Scan(int dst,int src)
        {Prefix(true,dst,src);Byte(0x0f);Byte(0xbd);Byte(0xc0|((dst&7)<<3)|(src&7));}
        private void Return(bool continuation)
        {Read(0,12,32);if(continuation){Byte(0x41);Byte(0xff);Byte(0xa3);Int(88);}else Byte(0xc3);}

        internal void Control(FullExtended80Control op,FullExtended80MemoryArithmeticFormat format,bool windows,bool continuation)
        {
            int version=Label(),size=Label(),finish=Label(),exit=Label();
            Byte(0xf3);Byte(0x0f);Byte(0x1e);Byte(0xfa);
            if(!continuation)Move(11,windows?1:7);
            Small(0,0);Write(0,12,32);for(int i=48;i<88;i+=8)Write(0,i);
            Read(0,0,32);Op(7,0,1,false);Branch(version,5);
            Read(0,4,32);Op(7,0,384,false);Branch(size,5);
            Put(104,0);Put(108,op==FullExtended80Control.RoundToIntegral?2:1);
            Read(0,8,16);Shift(5,0,10,false);Op(4,0,3,false);Write(0,100,32);
            Decode(16,112,120,96,144);
            DecodeMemory(format);
            Compare(op,finish);
            Mark(finish);Completion(op);Branch(exit);
            Mark(version);Put(12,1);Branch(exit);
            Mark(size);Put(12,2);
            Mark(exit);Return(continuation);
        }

        // Direct raw memory lift into decoded operand B. No input write or
        // preliminary conversion/quieting/rounding; original IEEE denorm class6
        // survives until Body has resolved all higher-priority classes and ZE.
        private void DecodeMemory(FullExtended80MemoryArithmeticFormat format)
        {
            if (format is FullExtended80MemoryArithmeticFormat.Int16 or FullExtended80MemoryArithmeticFormat.Int32)
            { DecodeInteger(format == FullExtended80MemoryArithmeticFormat.Int16 ? 16 : 32); return; }
            int fraction = format == FullExtended80MemoryArithmeticFormat.Binary32 ? 23 : 52;
            int bias = fraction == 23 ? 127 : 1023, emax = fraction == 23 ? 255 : 2047;
            int eZero = Label(), special = Label(), zero = Label(), finish = Label(), notInf = Label();
            Read(9, 32, fraction == 23 ? 32 : 64);
            Move(2, 9); Shift(5, 2, fraction == 23 ? 31 : 63); Write(2, 140, 32);
            Move(0, 9); Shift(5, 0, fraction); Op(4, 0, emax, false);
            Write(0, 136, 32);
            Constant(0, (1UL << fraction) - 1); Reg(0x21, 9, 0);
            Read(0, 136, 32); Test(0, false); Branch(eZero, 4);
            Op(7, 0, emax, false); Branch(special, 4);
            Op(5, 0, bias, false); Write(0, 136, 32);
            Constant(0, 1UL << fraction); Reg(0x09, 9, 0); Shift(4, 9, 63 - fraction);
            Write(9, 128); Put(148, 0); Branch(finish);
            Mark(eZero); Test(9); Branch(zero, 4);
            Scan(1, 9); Small(0, 1 - bias - fraction); Reg(0x01, 0, 1, false); Write(0, 136, 32);
            Small(2, 63); Reg(0x29, 2, 1, false); Move(1, 2, false); Shift(4, 9);
            Write(9, 128); Put(148, 6); Branch(finish);
            Mark(zero); Write(9, 128); Put(136, -16382); Put(148, 1); Branch(finish);
            Mark(special); Put(136, 16384);
            Shift(4, 9, 63 - fraction); Constant(0, 0x8000000000000000); Reg(0x09, 9, 0); Write(9, 128);

            Reg(0x39, 9, 0); Branch(notInf, 5); Put(148, 2); Branch(finish);
            Mark(notInf); Move(0, 9); Shift(5, 0, 62); Op(4, 0, 1, false);
            Small(2, 4); Reg(0x29, 2, 0, false); Write(2, 148, 32);
            Mark(finish);
        }

        private void DecodeInteger(int width)
        {
            int zero = Label(), positive = Label(), finish = Label();
            Read(9, 32, width);
            // Sign extension and absolute value are exact for both signed minima.
            Shift(4, 9, 64 - width); Shift(7, 9, 64 - width);
            Move(2, 9); Shift(5, 2, 63); Write(2, 140, 32);
            Test(9); Branch(zero, 4); Branch(positive, 9);
            Prefix(true, 0, 9); Byte(0xf7); Byte(0xd8 | (9 & 7));
            Mark(positive); Scan(1, 9); Write(1, 136, 32);
            Small(2, 63); Reg(0x29, 2, 1, false); Move(1, 2, false); Shift(4, 9);
            Write(9, 128); Put(148, 0); Branch(finish);
            Mark(zero); Write(9, 128); Put(136, -16382); Put(148, 1);
            Mark(finish);
        }

        // Exact binary80 decode, including true/pseudo denormals. Class is
        // 0 finite,1 zero,2 infinity,3 qNaN,4 sNaN,5 unsupported,6 denormal.
        // Zero is explicitly classified: its exponent is NOT its magnitude.
        // Decode itself does not set DE, so BOTH classes can outrank DE later.
        private void Decode(int input,int sig,int exp,int sign,int cls)
        {
            int exponentZero=Label(),zero=Label(),special=Label(),unsupported=Label(),
                normalized=Label(),store=Label(),quiet=Label();
            Read(8,input);Read(9,input+8,16);Move(0,9,false);Shift(5,0,15,false);Write(0,sign,32);
            Op(4,9,0x7fff,false);Put(cls,0);
            Test(9,false);Branch(exponentZero,4);
            Test(8);Branch(unsupported,9); // J=0 with nonzero exponent.
            Op(7,9,0x7fff,false);Branch(special,4);
            Op(5,9,16383,false);Branch(store);
            Mark(exponentZero);Test(8);Branch(zero,4);Put(cls,6);Small(9,-16382);
            Test(8);Branch(normalized,8); // pseudo: exponent-one, no shift.
            Scan(2,8);Small(1,63);Reg(0x29,1,2,false);Shift(4,8);Reg(0x29,9,1,false);
            Mark(normalized);Branch(store);
            Mark(zero);Put(cls,1);Small(9,-16382);Branch(store);
            Mark(special);Small(9,16384);Constant(10,0x8000000000000000);
            Reg(0x39,8,10);Branch(quiet,4); // infinity
            Constant(10,0x4000000000000000);Move(0,8);Reg(0x21,0,10);Test(0);Branch(quiet,5);
            Put(cls,4);Branch(store);
            Mark(quiet);Put(cls,2);Constant(10,0x8000000000000000);Reg(0x39,8,10);Branch(store,4);
            Put(cls,3);Branch(store);
            Mark(unsupported);Put(cls,5);
            Mark(store);Write(8,sig);Write(9,exp,32);
        }

        private static bool EFlags(FullExtended80Control op)=>
            op is FullExtended80Control.Fcomi or FullExtended80Control.Fcomip or
            FullExtended80Control.Fucomi or FullExtended80Control.Fucomip;
        private static bool Quiet(FullExtended80Control op)=>
            op is FullExtended80Control.Fucom or FullExtended80Control.Fucomp or FullExtended80Control.Fucompp or
            FullExtended80Control.Fucomi or FullExtended80Control.Fucomip;

        private static bool Pops(FullExtended80Control op)=>
            op is FullExtended80Control.Fcomp or FullExtended80Control.Fcompp or
            FullExtended80Control.Fucomp or FullExtended80Control.Fucompp or
            FullExtended80Control.Fcomip or FullExtended80Control.Fucomip;

        private void InvalidClass(int off,int invalid)
        {
            int next=Label();Read(0,off,32);Op(7,0,4,false);Branch(next,2);
            Op(7,0,5,false);Branch(invalid,6);Mark(next);
        }

        private void Compare(FullExtended80Control op,int finish)
        {
            int invalid=Label(),unordered=Label(),numeric=Label(),relation=Label(),
                less=Label(),equal=Label(),greater=Label(),magnitude=Label(),
                magLess=Label(),magGreater=Label(),leftNonzero=Label(),noDenormal=Label(),gate=Label();
            // Unsupported/sNaN first, then qNaN, then DE. Each operand may
            // otherwise contain a denormal: early exits MUST NOT leak DE.
            InvalidClass(144,invalid);InvalidClass(148,invalid);
            Read(0,144,32);Op(7,0,3,false);Branch(Quiet(op)?unordered:invalid,4);
            Read(0,148,32);Op(7,0,3,false);Branch(Quiet(op)?unordered:invalid,4);Branch(numeric);
            Mark(invalid);Put(104,1);Flag(108,8);Branch(unordered);
            Mark(unordered);Small(8,3);Branch(relation);
            Mark(numeric);Flag(108,4);
            Read(0,144,32);Op(7,0,6,false);Branch(gate,4);
            Read(0,148,32);Op(7,0,6,false);Branch(noDenormal,5);
            // AMD Vol5 lists DE for both families. Parent AMD control confirms
            // condition updates even with DM clear. No generic pre-op abort.
            Mark(gate);Put(104,2);Flag(108,16|1024);
            Mark(noDenormal);
            Read(8,112);Read(9,128);Test(8);Branch(leftNonzero,5);
            Test(9);Branch(equal,4); // both zero: signs ignored.
            Read(0,140,32);Test(0,false);Branch(greater,5);Branch(less);
            Mark(leftNonzero);Test(9);Branch(magnitude,5);
            Read(0,96,32);Test(0,false);Branch(less,5);Branch(greater);
            Mark(magnitude);Read(0,96,32);Read(2,140,32);Reg(0x39,0,2,false);
            int sameSign=Label();Branch(sameSign,4);Test(0,false);Branch(less,5);Branch(greater);
            Mark(sameSign);Read(0,120,32);Read(2,136,32);Reg(0x39,0,2,false);
            Branch(magLess,12);Branch(magGreater,15);
            Reg(0x39,8,9);Branch(magLess,2);Branch(magGreater,7);Branch(equal);
            Mark(magLess);Read(0,96,32);Test(0,false);Branch(greater,5);Branch(less);
            Mark(magGreater);Read(0,96,32);Test(0,false);Branch(less,5);Branch(greater);
            Mark(less);Small(8,1);Branch(relation);
            Mark(equal);Small(8,2);Branch(relation);
            Mark(greater);Small(8,0);
            Mark(relation);Write(8,48,32);
            // Relation-dependent bits: no arithmetic Classification reuse.
            int bitsLess=Label(),bitsEqual=Label(),bitsDone=Label();
            Test(8,false);Branch(bitsDone,4);Op(7,8,1,false);Branch(bitsLess,4);
            Op(7,8,2,false);Branch(bitsEqual,4);
            Small(9,EFlags(op)?0x45:0x4500);Branch(bitsDone);
            Mark(bitsLess);Small(9,EFlags(op)?1:0x100);Branch(bitsDone);
            Mark(bitsEqual);Small(9,EFlags(op)?0x40:0x4000);
            Mark(bitsDone);
            // Greater path must initialize zero rather than inherit R9 operand.
            int nonGreater=Label();Test(8,false);Branch(nonGreater,5);Small(9,0);Mark(nonGreater);
            if(EFlags(op))
            {Write(9,56,32);Small(0,0x200);Write(0,54,16);Put(60,0x8d5);}
            else {Write(9,52,16);Small(0,0x4700);Write(0,54,16);}
            Branch(finish);
        }

        private void Completion(FullExtended80Control op)
        {
            bool round=op==FullExtended80Control.RoundToIntegral;
            int noPrecision=Label(),noUp=Label(),events=Label(),done=Label(),invalid=Label(),denormal=Label(),precision=Label();
            Read(8,104,32);
            if(round)
            {
                Move(0,8,false);Op(4,0,32,false);Test(0,false);Branch(noPrecision,4);Flag(108,32);
                Move(0,8,false);Op(4,0,512,false);Test(0,false);Branch(noUp,4);Flag(108,64);Mark(noUp);Branch(events);
                Mark(noPrecision);Move(0,8,false);Op(4,0,1,false);Test(0,false);Branch(events,5);Flag(108,4);
            }
            else {Mark(noPrecision);Mark(noUp);}
            Mark(events);Write(8,80,16);Read(9,8,16);
            if(!round&&Pops(op))
            {
                int noDenormalPop=Label();Move(0,8,false);Op(4,0,2,false);Test(0,false);Branch(noDenormalPop,4);
                Move(0,9,false);Op(4,0,2,false);Test(0,false);Branch(noDenormalPop,5);
                Flag(108,2048);Mark(noDenormalPop);
            }
            Move(0,8,false);Op(4,0,1,false);Test(0,false);Branch(invalid,5);
            if(round)
            {Move(0,8,false);Op(4,0,2,false);Test(0,false);Branch(denormal,5);}
            Branch(precision);
            Mark(invalid);Move(0,9,false);Op(4,0,1,false);Test(0,false);Branch(done,5);
            Small(8,1);
            if(round)Flag(108,256);
            else
            {
                Flag(108,128);if(Pops(op))Flag(108,2048);
                if(EFlags(op))Put(60,0x890);
                else {Small(0,0x200);Write(0,54,16);}
            }
            Branch(done);
            Mark(denormal);
            if(round)
            {
                Move(0,9,false);Op(4,0,2,false);Test(0,false);Branch(precision,5);
                Small(8,2);Flag(108,256);Branch(done);
            }
            Mark(precision);
            if(round)
            {
                Move(0,8,false);Op(4,0,32,false);Test(0,false);Branch(done,4);
                Move(0,9,false);Op(4,0,32,false);Test(0,false);Branch(done,5);Flag(108,512);
            }
            Mark(done);Write(8,82,16);Read(0,108,32);Write(0,84,32);
        }
    }
}
