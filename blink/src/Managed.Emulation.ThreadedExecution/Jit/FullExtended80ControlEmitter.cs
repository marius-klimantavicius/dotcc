using System;
using System.Buffers.Binary;

namespace Managed.Emulation.Execution;

// R134 SOURCE_READY_UNEXECUTED. Integer-only binary80 controls. D/API.md binds
// the typed output and AMD-scoped denormal completion; no guest activation.
internal static class FullExtended80ControlEmitter
{
    internal const int CodeCapacity = 16384;
    internal static bool TryEmit(FullExtended80Control operation, Span<byte> destination,
        out int length, bool? windowsAbi = null) =>
        Emit(operation, destination, out length, windowsAbi ?? OperatingSystem.IsWindows(), false);
    internal static bool TryEmitContinuation(FullExtended80Control operation,
        Span<byte> destination, out int length) => Emit(operation, destination, out length, false, true);
    private static bool Emit(FullExtended80Control op, Span<byte> bytes,
        out int length, bool windows, bool continuation)
    {
        length=0;
        if((uint)op>(uint)FullExtended80Control.RoundToIntegral)return false;
        Span<int> positions=stackalloc int[128],pending=stackalloc int[128];
        positions.Fill(-1);pending.Fill(-1);
        var a=new Assembler(bytes[..Math.Min(bytes.Length,CodeCapacity)],positions,pending);
        a.Control(op,windows,continuation);
        if(!a.Success)return false;
        length=a.Count;return true;
    }
    // Independent new class. Common span/fixup/GPR encoders and round skeleton
    // reuse the authored R126/R129 family, not external emulator source.
    // NEW deltas: two classifies before DE, zero-aware magnitude comparison,
    // typed SW/EFLAGS masks, completing compare DE, exact integral repack.
    // Scratch: sign96, RC100, maskedSW104, metadata108, leftSig112/leftExp120,
    // rightSig128/rightExp136/rightSign140, classes144/148. No bytes>=152 used.
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

        internal void Control(FullExtended80Control op,bool windows,bool continuation)
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
            if(op==FullExtended80Control.RoundToIntegral)Integral(finish);
            else
            {
                Decode(32,128,136,140,148);
                Compare(op,finish);
            }
            Mark(finish);Completion(op);Branch(exit);
            Mark(version);Put(12,1);Branch(exit);
            Mark(size);Put(12,2);
            Mark(exit);Return(continuation);
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

        private void Pack80()
        {
            Read(0,96,32);Shift(4,0,15,false);Reg(0x09,9,0,false);
            Write(8,48);Write(9,56,16);
        }
        private void Integral(int finish)
        {
            int invalid=Label(),nan=Label(),copy=Label(),numeric=Label(),pack=Label(),
                zero=Label(),large=Label(),noDenormal=Label();
            Read(0,144,32);Op(7,0,5,false);Branch(invalid,4);Op(7,0,4,false);Branch(nan,4);
            Op(7,0,6,false);Branch(numeric,4);Test(0,false);Branch(copy,5);Branch(numeric);
            Mark(invalid);Put(104,1);Flag(108,8);Constant(8,0xc000000000000000);Small(9,0xffff);
            Write(8,48);Write(9,56,16);Branch(finish);
            Mark(nan);Put(104,1);Flag(108,8);Read(8,16);Constant(10,0x4000000000000000);
            Reg(0x09,8,10);Read(9,24,16);Write(8,48);Write(9,56,16);Branch(finish);
            Mark(copy);Read(8,16);Read(9,24,16);Write(8,48);Write(9,56,16);Flag(108,4);Branch(finish);
            Mark(numeric);Read(0,144,32);Op(7,0,6,false);Branch(noDenormal,5);Put(104,2);Flag(108,16);
            Mark(noDenormal);Read(8,112);Read(9,120,32);
            Op(7,9,63,false);Branch(large,13); // all values with e>=63 already integral.
            Small(1,63);Reg(0x29,1,9,false);Round();Test(8);Branch(zero,4);
            Scan(9,8);Small(1,63);Reg(0x29,1,9,false);Shift(4,8);Op(0,9,16383,false);Branch(pack);
            Mark(large);Op(0,9,16383,false);Branch(pack);
            Mark(zero);Small(9,0);
            Mark(pack);Pack80();Branch(finish);
        }

        // Magnitude / 2^count. Count0/1..63/64/>64 separated; no modulo64
        // shifts. Halfway ties use parity, and C1 means magnitude increment.
        private void Round()
        {
            int done=Label(),large=Label(),greater64=Label(),discard=Label(),increment=Label(),nearest=Label(),down=Label();
            Test(1,false);Branch(done,4);Op(7,1,64,false);Branch(large,3);
            Move(2,8);Shift(5,8);Small(0,64);Reg(0x29,0,1,false);Move(1,0,false);Shift(4,2);Branch(discard);
            Mark(large);Move(2,8);Small(8,0);Op(7,1,64,false);Branch(greater64,7);Branch(discard);
            Mark(greater64);Small(2,1);
            Mark(discard);Test(2);Branch(done,4);Flag(104,32);
            Read(0,100,32);Test(0,false);Branch(nearest,4);Op(7,0,3,false);Branch(done,4);
            Op(7,0,1,false);Branch(down,4);
            Read(0,96,32);Test(0,false);Branch(increment,4);Branch(done);
            Mark(down);Read(0,96,32);Test(0,false);Branch(increment,5);Branch(done);
            Mark(nearest);Constant(10,0x8000000000000000);Reg(0x39,2,10);Branch(increment,7);Branch(done,2);
            Move(0,8);Op(4,0,1,false);Test(0,false);Branch(done,4);
            Mark(increment);Op(0,8,1);Flag(104,512);
            Mark(done);
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
