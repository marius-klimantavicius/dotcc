using System;
using System.Buffers.Binary;

namespace Managed.Emulation.Execution;

// R152 standalone source handoff. This does not admit any guest instruction.
// Uses WorkFrame384/v1 with the operation-specific BCD protocol in D/API.md.
internal static class FullExtended80BcdEmitter
{
    internal const int CodeCapacity = 16384;
    internal static bool TryEmit(FullExtended80BcdOperation operation, Span<byte> destination,
        out int length, bool? windowsAbi = null) =>
        Emit(operation, destination, out length, windowsAbi ?? OperatingSystem.IsWindows(), false);
    internal static bool TryEmitContinuation(FullExtended80BcdOperation operation,
        Span<byte> destination, out int length) => Emit(operation, destination, out length, false, true);

    private static bool Emit(FullExtended80BcdOperation operation, Span<byte> destination,
        out int length, bool windows, bool continuation)
    {
        length = 0;
        if ((uint)operation > (uint)FullExtended80BcdOperation.StorePackedBcd) return false;
        Span<int> positions = stackalloc int[64], pending = stackalloc int[64];
        positions.Fill(-1); pending.Fill(-1);
        var a = new Assembler(destination[..Math.Min(destination.Length, CodeCapacity)], positions, pending);
        a.Convert(operation, windows, continuation);
        if (!a.Success) return false;
        length = a.Count; return true;
    }

    // R11 frame, R8 magnitude/significand, R9 exponent/decimal temporary.
    // Scratch96 sign/u32,100 RC/u32,104 masked status/u32,108 metadata/u32.
    // All frame addressing is disp32. No access beyond scratch111.
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
        { Prefix(width==64,r,11);if(width is 8 or 16){Byte(0x0f);Byte(width==8?0xb6:0xb7);}else Byte(0x8b);Address(r,off); }
        private void Write(int r,int off,int width=64)
        { if(width==16)Byte(0x66);Prefix(width==64,r,11);Byte(width==8?0x88:0x89);Address(r,off); }
        private void Put(int off,int v) {Small(0,v);Write(0,off,32);}
        private void Op(int group,int r,int v,bool wide=true)
        { Prefix(wide,0,r);Byte(0x81);Byte(0xc0|(group<<3)|(r&7));Int(v); }
        private void Flag(int off,int bits)
        {Prefix(false,0,11);Byte(0x81);Address(1,off);Int(bits);}
        private void Test(int r,bool wide=true)=>Reg(0x85,r,r,wide);
        private void Shift(int group,int r,int n=-1,bool wide=true)
        {Prefix(wide,0,r);Byte(n<0?0xd3:0xc1);Byte(0xc0|(group<<3)|(r&7));if(n>=0)Byte(n);}
        private void Scan(int dst,int src)
        {Prefix(true,dst,src);Byte(0x0f);Byte(0xbd);Byte(0xc0|((dst&7)<<3)|(src&7));}
        private void Return(bool continuation)
        {Read(0,12,32);if(continuation){Byte(0x41);Byte(0xff);Byte(0xa3);Int(88);}else Byte(0xc3);}

        internal void Convert(FullExtended80BcdOperation operation, bool windows, bool continuation)
        {
            int version=Label(),size=Label(),undefined=Label(),finish=Label(),exit=Label();
            Byte(0xf3);Byte(0x0f);Byte(0x1e);Byte(0xfa); // ENDBR64
            if(!continuation)Move(11,windows?1:7);
            Small(0,0);Write(0,12,32);for(int i=48;i<88;i+=8)Write(0,i);
            Read(0,0,32);Op(7,0,1,false);Branch(version,5);
            Read(0,4,32);Op(7,0,384,false);Branch(size,5);
            Put(96,0);Put(104,0);Put(108,1);
            Read(0,8,16);Shift(5,0,10,false);Op(4,0,3,false);Write(0,100,32);
            if(operation==FullExtended80BcdOperation.LoadPackedBcd)Load(undefined,finish);
            else Store(finish);
            Mark(finish);Completion();Branch(exit);
            Mark(undefined);Put(12,(int)FullExtended80BcdAdmission.UndefinedPackedBcdDigits);Branch(exit);
            Mark(version);Put(12,(int)FullExtended80BcdAdmission.BadVersion);Branch(exit);
            Mark(size);Put(12,(int)FullExtended80BcdAdmission.BadSize);
            Mark(exit);Return(continuation);
        }

        private void Load(int undefined, int finish)
        {
            // Horner over exactly18 nibbles, most significant first. Valid
            // magnitude <=10^18-1 fits60 bits, so all intermediate math is exact.
            Small(8,0);
            for(int digit=17;digit>=0;digit--)
            {
                Read(9,16+digit/2,8);
                if((digit&1)!=0)Shift(5,9,4,false);
                Op(4,9,15,false);Op(7,9,9,false);Branch(undefined,7);
                Move(0,8);Shift(4,8,3);Shift(4,0,1);Reg(0x01,8,0);Reg(0x01,8,9);
            }
            Read(0,25,8);Shift(5,0,7,false);Write(0,96,32);
            int zero=Label(),pack=Label();
            Test(8);Branch(zero,4);
            Scan(9,8);Small(1,63);Reg(0x29,1,9,false);Shift(4,8);
            Op(0,9,16383,false);Branch(pack);
            Mark(zero);Small(9,0);
            Mark(pack);Read(0,96,32);Shift(4,0,15,false);Reg(0x09,9,0,false);
            Write(8,48);Write(9,56,16);Branch(finish);
        }

        private void Store(int finish)
        {
            int invalid=Label(),expZero=Label(),numeric=Label(),pack=Label();
            Read(8,16);Read(9,24,16);Move(0,9,false);Shift(5,0,15,false);Write(0,96,32);
            Op(4,9,0x7fff,false);Test(9,false);Branch(expZero,4);
            Test(8);Branch(invalid,9); // J=0 at nonzero exponent is unsupported.
            Op(7,9,0x7fff,false);Branch(invalid,4); // Infinity and every NaN.
            Op(5,9,16383,false);Branch(numeric);
            Mark(expZero);Small(9,-16382); // canonical/pseudo denormal value; no DE.
            Mark(numeric);
            // No value with unbiased exponent>=60 can round into the18-digit
            // range. At exponent<=59 a rounded magnitude cannot overflow64 bits.
            Op(7,9,59,false);Branch(invalid,15);
            Small(1,63);Reg(0x29,1,9,false);Round();
            Constant(10,999999999999999999UL);Reg(0x39,8,10);Branch(invalid,7);
            // Pack two nibbles per byte. Every DIV high dividend is zero and
            // divisor10 is fixed/nonzero; even UINT64_MAX would be safe here.
            Small(10,10);
            for(int i=0;i<9;i++)
            {
                Divide10();Move(9,2);Divide10();Shift(4,2,4);Reg(0x09,9,2);Write(9,48+i,8);
            }
            Read(0,96,32);Shift(4,0,7,false);Write(0,57,8);Branch(pack);
            Mark(invalid);
            // IE replaces any intermediate PE/C1, including rounded overflow.
            Put(104,1);Flag(108,8);
            Constant(8,0xc000000000000000UL);Write(8,48);Small(8,0xffff);Write(8,56,16);
            Mark(pack);Branch(finish);
        }
        private void Divide10()
        {
            Move(0,8);Small(2,0);
            Prefix(true,0,10);Byte(0xf7);Byte(0xf2); // DIV R10 -> RDX:RAX
            Move(8,0);
        }
        // Round magnitude/2^CL once. RDX is discarded bits aligned to bit63;
        // count64 and >64 are separate, never x86's modulo64 shift behavior.
        private void Round()
        {
            int done=Label(),large=Label(),greater64=Label(),discard=Label(),increment=Label(),nearest=Label(),down=Label();
            Test(1,false);Branch(done,4);Op(7,1,64,false);Branch(large,3);
            Move(2,8);Shift(5,8);Small(0,64);Reg(0x29,0,1,false);Move(1,0,false);Shift(4,2);Branch(discard);
            Mark(large);Move(2,8);Small(8,0);Op(7,1,64,false);Branch(greater64,7);Branch(discard);
            Mark(greater64);Test(2);Branch(done,4);Small(2,1);
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
        private void Completion()
        {
            int events=Label(),noUp=Label(),invalid=Label(),done=Label();
            Read(8,104,32);Move(0,8,false);Op(4,0,32,false);Test(0,false);Branch(noUp,4);
            Flag(108,128);Move(0,8,false);Op(4,0,512,false);Test(0,false);Branch(events,4);
            Flag(108,256);Branch(events);
            Mark(noUp);Move(0,8,false);Op(4,0,1,false);Test(0,false);Branch(events,5);Flag(108,2);
            Mark(events);Write(8,80,16);Read(9,8,16);
            Move(0,8,false);Op(4,0,1,false);Test(0,false);Branch(invalid,5);
            Move(0,8,false);Op(4,0,32,false);Test(0,false);Branch(done,4);
            Move(0,9,false);Op(4,0,32,false);Test(0,false);Branch(done,5);
            Flag(108,1024);Branch(done);
            Mark(invalid);Move(0,9,false);Op(4,0,1,false);Test(0,false);Branch(done,5);
            Small(8,1);Flag(108,512);
            Mark(done);Write(8,82,16);Read(0,108,32);Write(0,84,32);
        }
    }
}
