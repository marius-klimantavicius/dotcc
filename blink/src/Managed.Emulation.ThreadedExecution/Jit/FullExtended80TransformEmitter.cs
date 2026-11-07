using System;
using System.Buffers.Binary;

namespace Managed.Emulation.Execution;

// R136 SOURCE_READY_UNEXECUTED. No managed semantic execution path.
internal static class FullExtended80TransformEmitter
{
    internal const int CodeCapacity = 16384;
    internal const int SqrtDigitPairs = 128;
    internal static bool TryEmit(FullExtended80Transform operation, Span<byte> destination,
        out int length, bool? windowsAbi = null) =>
        Emit(operation, destination, out length, windowsAbi ?? OperatingSystem.IsWindows(), false);
    internal static bool TryEmitContinuation(FullExtended80Transform operation,
        Span<byte> destination, out int length) => Emit(operation, destination, out length, false, true);
    private static bool Emit(FullExtended80Transform op, Span<byte> bytes,
        out int length, bool windows, bool continuation)
    {
        length=0;
        if((uint)op>(uint)FullExtended80Transform.Extract)return false;
        Span<int> positions=stackalloc int[192],pending=stackalloc int[192];
        positions.Fill(-1);pending.Fill(-1);
        var a=new Assembler(bytes[..Math.Min(bytes.Length,CodeCapacity)],positions,pending);
        a.Transform(op,windows,continuation);
        if(!a.Success)return false;
        length=a.Count;return true;
    }
    // Span/fixup/GPR encoding conventions from authored R126/R129/R134.
    // NEW operation bodies. No runtime selector in emitted image.
    // Scratch: sign96,class100,e104,p108,maskedSW112,metadata116,
    // rootExponent120,iterationCount124,radicand128..159,remainder160..183,
    // trial184..207,root208..223,pair224..227. Bytes228..255 untouched.
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

        internal void Transform(FullExtended80Transform op,bool windows,bool continuation)
        {
            int version=Label(),size=Label(),finish=Label(),exit=Label();
            Byte(0xf3);Byte(0x0f);Byte(0x1e);Byte(0xfa);
            if(!continuation)Move(11,windows?1:7);
            Small(0,0);Write(0,12,32);for(int i=48;i<88;i+=8)Write(0,i);
            Read(0,0,32);Op(7,0,1,false);Branch(version,5);
            Read(0,4,32);Op(7,0,384,false);Branch(size,5);
            if(op==FullExtended80Transform.Sqrt)
            {
                Read(0,8,16);Shift(5,0,8,false);Op(4,0,3,false);
                // PC01 is software PC64; leave Work.Control raw and immutable.
                int p24=Label(),p53=Label(),pDone=Label();
                Test(0,false);Branch(p24,4);Op(7,0,2,false);Branch(p53,4);
                Put(108,64);Branch(pDone);Mark(p24);Put(108,24);Branch(pDone);
                Mark(p53);Put(108,53);Mark(pDone);
            }
            Put(112,0);Put(116,op==FullExtended80Transform.Sqrt?1|4:2|4|8);
            Decode();
            if(op==FullExtended80Transform.Sqrt)SquareRoot(finish);else Extract(finish);
            Mark(finish);Completion(op);Branch(exit);
            Mark(version);Put(12,1);Branch(exit);
            Mark(size);Put(12,2);Branch(exit);
            Mark(exit);Return(continuation);
        }
        // Classes: 0 finite normal,1 zero,2 infinity,3 qNaN,4 sNaN,
        // 5 unsupported,6 true or pseudo denormal. No DE during decoding.
        private void Decode()
        {
            int exponentZero=Label(),zero=Label(),special=Label(),unsupported=Label(),
                normalized=Label(),store=Label(),quiet=Label();
            Read(8,16);Read(9,24,16);Move(0,9,false);Shift(5,0,15,false);Write(0,96,32);
            Op(4,9,0x7fff,false);Put(100,0);
            Test(9,false);Branch(exponentZero,4);
            Test(8);Branch(unsupported,9);
            Op(7,9,0x7fff,false);Branch(special,4);
            Op(5,9,16383,false);Branch(store);
            Mark(exponentZero);Test(8);Branch(zero,4);Put(100,6);Small(9,-16382);
            Test(8);Branch(normalized,8);
            Scan(2,8);Small(1,63);Reg(0x29,1,2,false);Shift(4,8);Reg(0x29,9,1,false);
            Mark(normalized);Branch(store);
            Mark(zero);Put(100,1);Small(9,-16382);Branch(store);
            Mark(special);Small(9,16384);Constant(10,0x8000000000000000);
            Reg(0x39,8,10);Branch(quiet,4);
            Constant(10,0x4000000000000000);Move(0,8);Reg(0x21,0,10);Test(0);Branch(quiet,5);
            Put(100,4);Branch(store);
            Mark(quiet);Put(100,2);Constant(10,0x8000000000000000);Reg(0x39,8,10);Branch(store,4);
            Put(100,3);Branch(store);
            Mark(unsupported);Put(100,5);
            Mark(store);Write(8,208);Write(9,104,32);
        }
        private void CopySource(int output)
        {Read(8,16);Read(9,24,16);Write(8,output);Write(9,output+8,16);}
        private void StorePair(bool extract)
        {Write(8,48);Write(9,56,16);if(extract){Write(8,64);Write(9,72,16);}}
        private void SquareRoot(int finish)
        {
            int invalid=Label(),snan=Label(),nan=Label(),copy=Label(),finite=Label(),noDE=Label();
            Read(0,100,32);Op(7,0,5,false);Branch(invalid,4);
            Op(7,0,4,false);Branch(snan,4);Op(7,0,3,false);Branch(nan,4);
            Op(7,0,1,false);Branch(copy,4); // -0 before the negative test.
            Read(0,96,32);Test(0,false);Branch(invalid,5);
            Read(0,100,32);Op(7,0,2,false);Branch(copy,4);Branch(finite);
            Mark(invalid);Put(112,1);Constant(8,0xc000000000000000);Small(9,0xffff);
            StorePair(false);Branch(finish);
            Mark(snan);Put(112,1);
            Mark(nan);Read(8,16);Constant(10,0x4000000000000000);Reg(0x09,8,10);
            Read(9,24,16);StorePair(false);Branch(finish);
            Mark(copy);CopySource(48);Branch(finish);
            Mark(finite);Read(0,100,32);Op(7,0,6,false);Branch(noDE,5);Put(112,2);Mark(noDE);
            Root128();RoundRoot();Branch(finish);
        }
        // Every iteration consumes exactly two bits of N, most significant first.
        // Q=floor(sqrt(prefix)), R=prefix-Q^2. T=4Q+1 has 130-bit storage.
        // The new Q is 2Q+(4R+pair>=T); no 128-bit carry is discarded.
        private void Root128()
        {
            int odd=Label(),ready=Label(),loop=Label(),subtract=Label(),keep=Label(),jamDone=Label();
            Read(8,208);Read(9,104,32);Move(0,9,false);Op(4,0,1,false);
            Shift(7,9,1,false);Write(9,120,32); // signed floor(e/2).
            Test(0,false);Branch(odd,5);
            Move(9,8);Shift(4,8,63);Shift(5,9,1);Write(8,144);Write(9,152);Branch(ready);
            Mark(odd);Write(8,152);Small(0,0);Write(0,144);
            Mark(ready);Small(0,0);Write(0,128);Write(0,136);
            for(int off=160;off<224;off+=8)Write(0,off);
            Put(124,0);
            Mark(loop);
            Read(10,152);Shift(5,10,62);Write(10,224,32);
            ShiftLimbs(128,4,2);ShiftLimbs(160,3,2);
            Read(8,160);Read(10,224,32);Reg(0x09,8,10);Write(8,160);
            Read(8,208);Read(9,216);Small(10,0);
            for(int i=0;i<2;i++){Reg(0x01,8,8);Reg(0x11,9,9);Reg(0x11,10,10);}
            Op(1,8,1);Write(8,184);Write(9,192);Write(10,200);
            Small(2,0);
            for(int limb=2;limb>=0;limb--)
            {
                Read(8,160+limb*8);Read(9,184+limb*8);Reg(0x39,8,9);
                Branch(subtract,7);Branch(keep,2);
            }
            Mark(subtract);
            for(int limb=0;limb<3;limb++)
            {Read(8,160+limb*8);Read(9,184+limb*8);Reg(limb==0?0x29:0x19,8,9);Write(8,160+limb*8);}
            Small(2,1);
            Mark(keep);Read(8,208);Read(9,216);Reg(0x01,8,8);Reg(0x11,9,9);
            Reg(0x09,8,2);Write(8,208);Write(9,216);
            Read(0,124,32);Op(0,0,1,false);Write(0,124,32);Op(7,0,SqrtDigitPairs,false);Branch(loop,2);
            Read(8,160);Read(9,168);Reg(0x09,8,9);Read(9,176);Reg(0x09,8,9);Test(8);Branch(jamDone,4);
            Read(8,208);Op(1,8,1);Write(8,208);Mark(jamDone);
        }
        private void ShiftLimbs(int off,int limbs,int steps)
        {
            for(int step=0;step<steps;step++)for(int limb=0;limb<limbs;limb++)
            {Read(8,off+limb*8);Reg(limb==0?0x01:0x11,8,8);Write(8,off+limb*8);}
        }
        private void RoundRoot()
        {
            int full=Label(),fraction=Label(),pack=Label(),increment=Label(),carry=Label(),
                rounded=Label(),noCarry=Label();
            Read(8,216);Read(10,208);Read(0,108,32);Op(7,0,64,false);Branch(full,4);
            Move(9,8);Move(1,0,false);Shift(4,9);Small(1,64);Reg(0x29,1,0,false);Shift(5,8);Branch(fraction);
            Mark(full);Move(9,10);Small(10,0);
            Mark(fraction);Move(0,9);Reg(0x09,0,10);Test(0);Branch(pack,4);
            Flag(112,32);Read(0,8,16);Shift(5,0,10,false);Op(4,0,3,false);
            Op(7,0,2,false);Branch(increment,4);Test(0,false);Branch(pack,5);
            Constant(0,0x8000000000000000);Reg(0x39,9,0);Branch(increment,7);Branch(pack,2);
            Test(10);Branch(increment,5);Move(0,8);Op(4,0,1);Test(0);Branch(pack,4);
            Mark(increment);Flag(112,0x200);Op(0,8,1);Branch(carry,2);
            Read(0,108,32);Op(7,0,64,false);Branch(rounded,4);
            Small(10,1);Move(1,0,false);Shift(4,10);Reg(0x39,8,10);Branch(rounded,5);
            Shift(5,8,1);Branch(noCarry);
            Mark(carry);Constant(8,0x8000000000000000);
            Mark(noCarry);Read(0,120,32);Op(0,0,1,false);Write(0,120,32);
            Mark(rounded);
            Mark(pack);Read(0,108,32);Small(1,64);Reg(0x29,1,0,false);Shift(4,8);
            Read(9,120,32);Op(0,9,16383,false);Write(8,48);Write(9,56,16);
        }
        private void Extract(int finish)
        {
            int invalid=Label(),snan=Label(),nan=Label(),infinity=Label(),zero=Label(),noDE=Label(),
                integerZero=Label(),positive=Label(),integerPack=Label();
            Read(0,100,32);Op(7,0,5,false);Branch(invalid,4);Op(7,0,4,false);Branch(snan,4);
            Op(7,0,3,false);Branch(nan,4);Op(7,0,2,false);Branch(infinity,4);Op(7,0,1,false);Branch(zero,4);
            Op(7,0,6,false);Branch(noDE,5);Put(112,2);Mark(noDE);
            Read(8,208);Read(9,96,32);Shift(4,9,15,false);Op(1,9,0x3fff,false);Write(8,48);Write(9,56,16);
            Read(8,104,32);Test(8,false);Branch(integerZero,4);
            Small(10,0);Test(8,false);Branch(positive,9);
            // NEG32 before zero extension: exponent magnitude at most16445.
            Prefix(false,0,8);Byte(0xf7);Byte(0xd8|(8&7));Small(10,0x8000);
            Mark(positive);Scan(9,8);Small(1,63);Reg(0x29,1,9,false);Shift(4,8);
            Op(0,9,16383,false);Reg(0x09,9,10,false);Branch(integerPack);
            Mark(integerZero);Small(8,0);Small(9,0);
            Mark(integerPack);Write(8,64);Write(9,72,16);Branch(finish);
            Mark(invalid);Put(112,1);Constant(8,0xc000000000000000);Small(9,0xffff);StorePair(true);Branch(finish);
            Mark(snan);Put(112,1);
            Mark(nan);Read(8,16);Constant(10,0x4000000000000000);Reg(0x09,8,10);Read(9,24,16);StorePair(true);Branch(finish);
            Mark(infinity);CopySource(48);Constant(8,0x8000000000000000);Small(9,0x7fff);
            Write(8,64);Write(9,72,16);Branch(finish);
            Mark(zero);Put(112,4);CopySource(48);Constant(8,0x8000000000000000);Small(9,0xffff);
            Write(8,64);Write(9,72,16);Branch(finish);
        }
        private void PreGate(int bit,int suppress)
        {
            int next=Label();Move(0,8,false);Op(4,0,bit,false);Test(0,false);Branch(next,4);
            Move(0,9,false);Op(4,0,bit,false);Test(0,false);Branch(suppress,4);Mark(next);
        }
        private void MetaEvent(int bit,int meta)
        {
            int next=Label();Move(0,8,false);Op(4,0,bit,false);Test(0,false);Branch(next,4);Flag(116,meta);Mark(next);
        }
        private void Completion(FullExtended80Transform op)
        {
            int suppressIE=Label(),suppressDE=Label(),suppressZE=Label(),suppressed=Label(),
                events=Label(),done=Label();
            Read(8,112,32);Write(8,80,16);Read(9,8,16);
            PreGate(1,suppressIE);PreGate(2,suppressDE);PreGate(4,suppressZE);
            if(op==FullExtended80Transform.Sqrt)Flag(116,8192);
            int notExact=Label(),exact=Label();Move(0,8,false);Op(4,0,33,false);Test(0,false);Branch(notExact,5);
            Read(0,100,32);Op(7,0,1,false);Branch(exact,6);Op(7,0,6,false);Branch(notExact,5);
            Mark(exact);Flag(116,16);Mark(notExact);
            if(op==FullExtended80Transform.Sqrt)
            {
                int noPE=Label();Move(0,8,false);Op(4,0,32,false);Test(0,false);Branch(noPE,4);
                Move(0,9,false);Op(4,0,32,false);Test(0,false);Branch(noPE,5);Flag(116,4096);Mark(noPE);
            }
            Branch(events);
            Mark(suppressIE);Small(8,1);Branch(suppressed);
            Mark(suppressDE);Small(8,2);Branch(suppressed);
            Mark(suppressZE);Small(8,4);
            Mark(suppressed);Small(0,0);for(int off=48;off<80;off+=8)Write(0,off);
            Put(116,(op==FullExtended80Transform.Sqrt?1:2|2048)|1024);
            Mark(events);Write(8,82,16);
            MetaEvent(1,32);MetaEvent(2,64);MetaEvent(4,128);MetaEvent(32,256);MetaEvent(512,512);
            Read(0,116,32);Write(0,84,32);Mark(done);
        }
    }
}
