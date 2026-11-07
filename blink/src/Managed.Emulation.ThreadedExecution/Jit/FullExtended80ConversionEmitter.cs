using System;
using System.Buffers.Binary;

namespace Managed.Emulation.Execution;

// R129 SOURCE_READY_UNEXECUTED. Specialized baseline integer machine code.
// No managed numeric helper is an emitted target. See D/API.md for completion.
internal static class FullExtended80ConversionEmitter
{
    internal const int CodeCapacity = 16384;
    internal static bool TryEmit(FullExtended80Conversion operation, Span<byte> destination,
        out int length, bool? windowsAbi = null) =>
        Emit(operation, destination, out length, windowsAbi ?? OperatingSystem.IsWindows(), false);
    internal static bool TryEmitContinuation(FullExtended80Conversion operation,
        Span<byte> destination, out int length) => Emit(operation, destination, out length, false, true);

    private static bool Emit(FullExtended80Conversion op, Span<byte> bytes,
        out int length, bool windows, bool continuation)
    {
        length = 0;
        if ((uint)op > (uint)FullExtended80Conversion.StoreRaw80) return false;
        Span<int> positions = stackalloc int[128], pending = stackalloc int[128];
        positions.Fill(-1); pending.Fill(-1);
        var a = new Assembler(bytes[..Math.Min(bytes.Length, CodeCapacity)], positions, pending);
        a.Convert(op, windows, continuation);
        if (!a.Success) return false;
        length = a.Count; return true;
    }

    // R8 magnitude/significand; R9 signed exponent; RCX variable shift count.
    // Scratch: sign96/u32, RC100/u32, masked SW104/u32, metadata108/u32,
    // original significand112/u64 and exponent120/i32. Nothing past123 used.
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

        internal void Convert(FullExtended80Conversion op,bool windows,bool continuation)
        {
            int version=Label(),size=Label(),finish=Label(),exit=Label();
            Byte(0xf3);Byte(0x0f);Byte(0x1e);Byte(0xfa);
            if(!continuation)Move(11,windows?1:7);
            Small(0,0);Write(0,12,32);for(int i=48;i<88;i+=8)Write(0,i);
            Read(0,0,32);Op(7,0,1,false);Branch(version,5);
            Read(0,4,32);Op(7,0,384,false);Branch(size,5);
            Put(96,0);Put(104,0);Put(108,1);
            Read(0,8,16);Shift(5,0,10,false);Op(4,0,3,false);Write(0,100,32);
            if(((uint)op >= (uint)FullExtended80Conversion.TruncateInt16 && (uint)op <= (uint)FullExtended80Conversion.TruncateInt64))Put(100,3);
            if(op is FullExtended80Conversion.LoadRaw80 or FullExtended80Conversion.StoreRaw80)
            {Read(8,16);Read(9,24,16);Write(8,48);Write(9,56,16);Flag(108,4);}
            else if(op is FullExtended80Conversion.LoadBinary32 or FullExtended80Conversion.LoadBinary64)
                LoadIeee(op==FullExtended80Conversion.LoadBinary32?32:64,finish);
            else if(((uint)op >= (uint)FullExtended80Conversion.LoadInt16 && (uint)op <= (uint)FullExtended80Conversion.LoadInt64))
                LoadInteger(op==FullExtended80Conversion.LoadInt16?16:op==FullExtended80Conversion.LoadInt32?32:64,finish);
            else
            {
                int invalid=Label(),special=Label(),zero=Label(),numeric=Label();
                Decode(invalid,special,zero,numeric);
                Mark(numeric);
                if(op is FullExtended80Conversion.StoreBinary32 or FullExtended80Conversion.StoreBinary64)
                    StoreIeee(op==FullExtended80Conversion.StoreBinary32?32:64,finish);
                else StoreInteger(IntegerWidth(op),invalid,finish);
                Mark(zero);
                Small(8,0);
                if(op is FullExtended80Conversion.StoreBinary32 or FullExtended80Conversion.StoreBinary64)
                    PackIeee(op==FullExtended80Conversion.StoreBinary32?32:64);
                else Write(8,48);
                Branch(finish);
                Mark(special);
                if(op is FullExtended80Conversion.StoreBinary32 or FullExtended80Conversion.StoreBinary64)
                    StoreSpecial(op==FullExtended80Conversion.StoreBinary32?32:64,invalid,finish);
                else Branch(invalid);
                Mark(invalid);
                Put(104,1);Flag(108,8);
                if(op is FullExtended80Conversion.StoreBinary32 or FullExtended80Conversion.StoreBinary64)
                {Constant(8,op==FullExtended80Conversion.StoreBinary32?0xffc00000UL:0xfff8000000000000UL);Write(8,48);}
                else {Constant(8,1UL<<(IntegerWidth(op)-1));Write(8,48);}
            }
            Mark(finish);Completion(op);Branch(exit);
            Mark(version);Put(12,1);Branch(exit);
            Mark(size);Put(12,2);
            Mark(exit);Return(continuation);
        }
        private static int IntegerWidth(FullExtended80Conversion op)=>op is FullExtended80Conversion.StoreInt16 or FullExtended80Conversion.TruncateInt16?16:
            op is FullExtended80Conversion.StoreInt32 or FullExtended80Conversion.TruncateInt32?32:64;

        private void Pack80()
        {
            Read(0,96,32);Shift(4,0,15,false);Reg(0x09,9,0,false);
            Write(8,48);Write(9,56,16);
        }
        private void PackIeee(int width)
        {Read(0,96,32);Shift(4,0,width-1);Reg(0x09,8,0);Write(8,48);}

        private void LoadIeee(int width,int finish)
        {
            int f=width==32?23:52,bias=width==32?127:1023,max=width==32?255:2047;
            int nonnormal=Label(),normal=Label(),zero=Label(),special=Label(),pack=Label(),quiet=Label();
            Read(8,16,width);Move(0,8);Shift(5,0,width-1);Write(0,96,32);
            Move(9,8);Shift(5,9,f);Op(4,9,max,false);
            Constant(10,(1UL<<f)-1);Reg(0x21,8,10);
            Test(9,false);Branch(nonnormal,4);Op(7,9,max,false);Branch(special,4);Branch(normal);
            Mark(nonnormal);Test(8);Branch(zero,4);
            // Exact subnormal normalization. BSR only sees nonzero operands.
            Scan(2,8);Small(1,63);Reg(0x29,1,2,false);Shift(4,8);
            Small(9,16383+1-bias-f);Reg(0x01,9,2,false);
            Put(104,2);Flag(108,16);Branch(pack);
            Mark(normal);Constant(10,1UL<<f);Reg(0x09,8,10);Shift(4,8,63-f);
            Op(0,9,16383-bias,false);Branch(pack);
            Mark(special);
            // Quieting precedes narrowing; no NaN payload is routed via double.
            Test(8);Branch(quiet,4);
            Constant(10,1UL<<(f-1));Move(0,8);Reg(0x21,0,10);Test(0);Branch(quiet,5);
            Put(104,1);Flag(108,8);Reg(0x09,8,10);
            Mark(quiet);Shift(4,8,63-f);Constant(10,0x8000000000000000);Reg(0x09,8,10);
            Small(9,0x7fff);Branch(pack);
            Mark(zero);Small(9,0);
            Mark(pack);Pack80();Branch(finish);
        }
        private void LoadInteger(int width,int finish)
        {
            int positive=Label(),zero=Label();
            Read(8,16,width==16?16:width);
            if(width<64){Shift(4,8,64-width);Shift(7,8,64-width);}
            Test(8);Branch(zero,4);Branch(positive,9);
            Put(96,1);Negate(8);
            Mark(positive);Scan(9,8);Small(1,63);Reg(0x29,1,9,false);Shift(4,8);
            Op(0,9,16383,false);Pack80();Branch(finish);
            Mark(zero);Small(9,0);Pack80();Branch(finish);
        }
        private void Decode(int invalid,int special,int zero,int numeric)
        {
            Read(8,16);Read(9,24,16);Move(0,9,false);Shift(5,0,15,false);Write(0,96,32);
            Op(4,9,0x7fff,false);int exponentZero=Label(),normalize=Label();
            Test(9,false);Branch(exponentZero,4);
            Test(8);Branch(invalid,9); // J bit must be 1 at any nonzero exponent.
            Op(7,9,0x7fff,false);Branch(special,4);
            Op(5,9,16383,false);Branch(numeric);
            Mark(exponentZero);Test(8);Branch(zero,4);Small(9,-16382);
            // Both canonical and pseudo denormals use exponent-one value. No DE
            // on FST/FIST; FLDm80 bypasses this entire numeric decode.
            Branch(normalize,8);Scan(2,8);Small(1,63);Reg(0x29,1,2,false);
            Shift(4,8);Reg(0x29,9,1,false);
            Mark(normalize);Branch(numeric);
        }

        // Round magnitude/2^CL once. RDX is discarded bits aligned to bit63;
        // count64 and >64 are separate, never x86's modulo64 shift behavior.
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
        private void StoreInteger(int width,int invalid,int finish)
        {
            int range=Label(),negative=Label(),valid=Label(),positive=Label();
            Op(7,9,63,false);Branch(invalid,15);Branch(range,4);
            Small(1,63);Reg(0x29,1,9,false);Round();
            Mark(range);Read(0,96,32);Test(0,false);Branch(negative,5);
            Constant(10,1UL<<(width-1));Reg(0x39,8,10);Branch(invalid,3);Branch(valid);
            Mark(negative);Constant(10,1UL<<(width-1));Reg(0x39,8,10);Branch(invalid,7);
            Mark(valid);Read(0,96,32);Test(0,false);Branch(positive,4);Negate(8);
            Mark(positive);if(width<64){Constant(10,(1UL<<width)-1);Reg(0x21,8,10);}
            Write(8,48);Branch(finish);
        }
        private void StoreSpecial(int width,int invalid,int finish)
        {
            int f=width==32?23:52,quiet=Label(),infinity=Label();
            Constant(10,0x8000000000000000);Reg(0x39,8,10);Branch(infinity,4);
            Move(0,8);Constant(10,0x4000000000000000);Reg(0x21,0,10);Test(0);Branch(quiet,5);
            Put(104,1);Flag(108,8);
            Mark(quiet);Shift(5,8,63-f);Constant(10,(1UL<<f)-1);Reg(0x21,8,10);
            Constant(10,((width==32?255UL:2047UL)<<f)|(1UL<<(f-1)));Reg(0x09,8,10);PackIeee(width);Branch(finish);
            Mark(infinity);Constant(8,(width==32?255UL:2047UL)<<f);PackIeee(width);Branch(finish);
        }
        private void StoreIeee(int width,int finish)
        {
            int p=width==32?24:53,f=p-1,bias=width==32?127:1023,emin=1-bias,emax=bias,cut=64-p;
            int carryDone=Label(),tiny=Label(),overflow=Label(),pack=Label(),tinyPack=Label(),infinity=Label();
            Write(8,112);Write(9,120,32);
            Small(1,cut);Round();Constant(10,1UL<<p);Reg(0x39,8,10);Branch(carryDone,5);
            Shift(5,8,1);Op(0,9,1,false);
            Mark(carryDone);Op(7,9,emax,false);Branch(overflow,15);
            Op(7,9,emin,false);Branch(tiny,12);
            // Normal encoding (drop the implicit leading one).
            Op(0,9,bias,false);Shift(4,9,f);Constant(10,(1UL<<f)-1);Reg(0x21,8,10);Reg(0x09,8,9);Branch(pack);
            Mark(tiny);Flag(108,32);Put(104,0);Read(8,112);Read(9,120,32);
            Small(1,cut+emin);Reg(0x29,1,9,false);Round();
            // Integer q in units of destination minimum subnormal; carry to
            // minimum normal naturally has encoding 1<<f, no second rounding.
            Read(0,104,32);Op(4,0,32,false);Test(0,false);Branch(tinyPack,4);Flag(104,16);
            Mark(tinyPack);Branch(pack);
            Mark(overflow);Put(104,40);Flag(108,64);
            Read(0,100,32);Test(0,false);Branch(infinity,4);Op(7,0,3,false);int maximum=Label();Branch(maximum,4);
            Read(2,96,32);Op(7,0,1,false);int towardPositive=Label();Branch(towardPositive,5);
            Test(2,false);Branch(infinity,5);Branch(maximum);
            Mark(towardPositive);Test(2,false);Branch(infinity,4);
            Mark(maximum);Constant(8,(((1UL<<(width==32?8:11))-2)<<f)|((1UL<<f)-1));Branch(pack);
            Mark(infinity);Constant(8,((1UL<<(width==32?8:11))-1)<<f);Flag(104,512);
            Mark(pack);PackIeee(width);Branch(finish);
        }
        private void Completion(FullExtended80Conversion op)
        {
            int inexact=Label(),noUp=Label(),events=Label(),done=Label(),invalid=Label(),precision=Label();
            Read(8,104,32);Move(0,8,false);Op(4,0,32,false);Test(0,false);Branch(inexact,5);
            Move(0,8,false);Op(4,0,1,false);Test(0,false);Branch(events,5);Flag(108,2);Branch(events);
            Mark(inexact);Flag(108,128);Move(0,8,false);Op(4,0,512,false);Test(0,false);Branch(noUp,4);Flag(108,256);
            Mark(noUp);
            Mark(events);Write(8,80,16);Read(9,8,16);
            Move(0,8,false);Op(4,0,1,false);Test(0,false);Branch(invalid,5);
            if(op is FullExtended80Conversion.LoadBinary32 or FullExtended80Conversion.LoadBinary64)
            {
                Move(0,8,false);Op(4,0,2,false);Test(0,false);Branch(precision,4);
                Move(0,9,false);Op(4,0,2,false);Test(0,false);Branch(precision,5);
                Flag(108,2048);Branch(done);
            }
            if(op is FullExtended80Conversion.StoreBinary32 or FullExtended80Conversion.StoreBinary64)
            {
                int noOverflow=Label(),noTiny=Label();
                Move(0,8,false);Op(4,0,8,false);Test(0,false);Branch(noOverflow,4);
                Move(0,9,false);Op(4,0,8,false);Test(0,false);Branch(noOverflow,5);
                Small(8,8);Flag(108,512);Branch(done);
                Mark(noOverflow);Read(0,108,32);Op(4,0,32,false);Test(0,false);Branch(noTiny,4);
                Move(0,9,false);Op(4,0,16,false);Test(0,false);Branch(noTiny,5);
                Small(8,16);Flag(108,512);Branch(done);
                Mark(noTiny);
            }
            Branch(precision);
            Mark(invalid);Move(0,9,false);Op(4,0,1,false);Test(0,false);Branch(done,5);
            Small(8,1);Flag(108,512);Branch(done);
            Mark(precision);Move(0,8,false);Op(4,0,32,false);Test(0,false);Branch(done,4);
            Move(0,9,false);Op(4,0,32,false);Test(0,false);Branch(done,5);Flag(108,1024);
            Mark(done);Write(8,82,16);Read(0,108,32);Write(0,84,32);
        }
    }
}
