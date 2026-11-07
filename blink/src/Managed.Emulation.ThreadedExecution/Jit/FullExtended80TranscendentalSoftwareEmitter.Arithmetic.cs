namespace Managed.Emulation.Execution;

internal static partial class FullExtended80TranscendentalSoftwareEmitter
{
    private ref partial struct Writer
    {
        // v=(-1)^s * (s0+2^64*s1+2^128*s2+2^192*s3)*2^(e-255).
        // Internal operations truncate to 256 significant bits. Final packing
        // separately implements RC; no PC-dependent intermediate operation.
        private void Copy(int dst,int src)
        {if(dst!=src)for(int i=0;i<40;i+=8){Load(0,src+i);Store(0,dst+i);}}
        private void Zero(int p)
        {C(0,0);for(int i=0;i<40;i+=8)Store(0,p+i);}
        private void Constant(int p,int e,ulong w0,ulong w1,ulong w2,ulong w3)
        {
            Imm(0,w0);Store(0,p);Imm(0,w1);Store(0,p+8);
            Imm(0,w2);Store(0,p+16);Imm(0,w3);Store(0,p+24);
            Put(p+32,e);Put(p+36,0);
        }
        private void One(int p,int exponent=0)
            =>Constant(p,exponent,0,0,0,0x8000000000000000UL);
        private void Negate(int p)=>Bits(p+36,6,1);
        private void Magnitude(int p)=>Put(p+36,0);
        private void AddExponent(int p,int value)=>Bits(p+32,0,value);
        private void MagnitudeTest(int p)
        {Load(0,p);for(int i=8;i<32;i+=8){Load(1,p+i);RR(0x09,0,1);}Test(0);}
        private void CompareSignificands(int a,int b,int less,int equal,int greater)
        {
            for(int i=24;i>=0;i-=8)
            {Load(0,a+i);Load(1,b+i);RR(0x39,0,1);J(less,2);J(greater,7);}
            J(equal);
        }
        private void CompareMagnitude(int a,int b,int less,int equal,int greater)
        {
            Load(0,a+32,32);Load(1,b+32,32);RR(0x39,0,1,false);
            J(less,12);J(greater,15);CompareSignificands(a,b,less,equal,greater);
        }
        private void Normalize(int p)
        {
            int done=L(),loop=L(),bits=L(),nonzero=L();
            MagnitudeTest(p);J(nonzero,5);Put(p+32,0);J(done);Bind(nonzero);
            Bind(loop);Load(0,p+24);Test(0);J(bits,5);
            // Each trip moves a nonzero limb toward the top. At most 3 trips.
            for(int i=24;i>0;i-=8){Load(0,p+i-8);Store(0,p+i);}
            C(0,0);Store(0,p);AddExponent(p,-64);J(loop);
            Bind(bits);Bsr(1,0);C(2,63);RR(0x29,2,1,false);Mov(1,2,false);
            Test(1,false);J(done,4);
            Load(0,p+32,32);RR(0x29,0,1,false);Store(0,p+32,32);
            for(int i=24;i>0;i-=8)
            {Load(0,p+i);Load(2,p+i-8);DoubleShift(false,0,2);Store(0,p+i);}
            Load(0,p);Shift(4,0);Store(0,p);Bind(done);
        }
        private void FromRaw(int p,int raw)
        {
            Zero(p);Load(0,raw);Store(0,p+24);
            Load(0,raw+8,16);Mov(1,0,false);Shift(5,1,15,false);Store(1,p+36,32);
            Alu(4,0,0x7fff,false);int normal=L(),ready=L();Test(0,false);J(normal,5);
            C(0,-16382);J(ready);Bind(normal);Alu(5,0,16383,false);Bind(ready);
            Store(0,p+32,32);Normalize(p);
        }
        // Input unsigned RAX. Every nonzero uint64 is represented exactly.
        private void FromUnsigned(int p)
        {Mov(8,0);Zero(p);Store(8,p+24);Put(p+32,63);Normalize(p);}
        // Input EAX; all exponents/k fit int32. Magnitude in R8 cannot overflow.
        private void FromSigned(int p)
        {
            Mov(9,0,false);Shift(7,9,31,false);RR(0x31,0,9,false);RR(0x29,0,9,false);
            Mov(8,0,false);Zero(p);Store(8,p+24);Put(p+32,63);
            Alu(4,9,1,false);Store(9,p+36,32);Normalize(p);
        }
        // ECX nonnegative count. Four whole-limb trips maximum; counts >=256
        // produce zero with bit0 jammed iff any source bit was nonzero.
        // ECX and all volatile GPRs except R11 may be clobbered.
        private void ShiftRightJam(int p)
        {
            int huge=L(),whole=L(),bits=L(),jam=L(),done=L();C(8,0);
            Alu(7,1,256,false);J(huge,13);
            Bind(whole);Alu(7,1,64,false);J(bits,12);
            Load(0,p);RR(0x09,8,0);
            for(int i=0;i<24;i+=8){Load(0,p+i+8);Store(0,p+i);}
            C(0,0);Store(0,p+24);Alu(5,1,64,false);J(whole);
            Bind(bits);Test(1,false);J(jam,4);
            Load(9,p);Imm(2,ulong.MaxValue);Shift(4,2);Alu(6,2,-1);
            RR(0x21,9,2);RR(0x09,8,9);
            for(int i=0;i<24;i+=8)
            {Load(0,p+i);Load(2,p+i+8);DoubleShift(true,0,2);Store(0,p+i);}
            Load(0,p+24);Shift(5,0);Store(0,p+24);J(jam);
            Bind(huge);MagnitudeTest(p);Mov(8,0);C(0,0);
            for(int i=0;i<32;i+=8)Store(0,p+i);
            Bind(jam);Test(8);J(done,4);Load(0,p);Alu(1,0,1);Store(0,p);Bind(done);
        }
        // a += b. Both slots may change. Other slots and Work header survive.
        // Add/sub alignment jams only discarded bits; <=2 internal ulps absolute
        // error at the larger operand exponent, even under cancellation.
        private void Add(int a,int b)
        {
            int done=L(),swap=L(),ordered=L(),same=L(),subtract=L(),noCarry=L();
            MagnitudeTest(a);int aNonzero=L();J(aNonzero,5);Copy(a,b);J(done);Bind(aNonzero);
            MagnitudeTest(b);J(done,4);
            CompareMagnitude(a,b,swap,ordered,ordered);Bind(swap);
            for(int i=0;i<40;i+=8){Load(0,a+i);Load(1,b+i);Store(1,a+i);Store(0,b+i);}
            Bind(ordered);Load(1,a+32,32);Load(0,b+32,32);RR(0x29,1,0,false);
            ShiftRightJam(b);Load(0,a+36,32);Load(1,b+36,32);RR(0x39,0,1,false);J(same,4);J(subtract);
            Bind(same);
            for(int i=0;i<32;i+=8)
            {Load(0,a+i);Load(1,b+i);RR(i==0?0x01:0x11,0,1);Store(0,a+i);}
            J(noCarry,3);
            // CF from the 256-bit sum is the 257th bit. MOV leaves it intact.
            Load(8,a);Mov(9,8); // preserve dropped bit without touching flags
            for(int i=24;i>=0;i-=8){Load(0,a+i);Shift(3,0,1);Store(0,a+i);}
            Alu(4,9,1);int noJam=L();J(noJam,4);Load(0,a);Alu(1,0,1);Store(0,a);Bind(noJam);
            AddExponent(a,1);Bind(noCarry);J(done);
            Bind(subtract);
            for(int i=0;i<32;i+=8)
            {Load(0,a+i);Load(1,b+i);RR(i==0?0x29:0x19,0,1);Store(0,a+i);}
            Normalize(a);Bind(done);
        }
        // a *= b; b retained, a!=b. Comba streaming needs exactly R8:R9:R10
        // plus MUL's RDX:RAX and RCX. At column4, a0 has already had its last
        // read (column3). Every later overwritten limb has the same proof.
        private void Multiply(int a,int b)
        {
            int zero=L(),done=L();
            Load(0,a+36,32);Load(1,b+36,32);RR(0x31,0,1,false);Store(0,a+36,32);
            MagnitudeTest(a);J(zero,4);MagnitudeTest(b);J(zero,4);
            Load(0,a+32,32);Load(1,b+32,32);RR(0x01,0,1,false);Alu(0,0,1,false);Store(0,a+32,32);
            C(8,0);C(9,0);C(10,0);
            for(int col=0;col<8;++col)
            {
                for(int i=0;i<4;++i)
                {
                    int j=col-i;if(j<0||j>=4)continue;
                    Load(0,a+8*i);Load(1,b+8*j);B(0x48);B(0xf7);B(0xe1); // MUL RCX
                    RR(0x01,8,0);RR(0x11,9,2);Alu(2,10,0);
                }
                if(col>=4)Store(8,a+8*(col-4));
                Mov(8,9);Mov(9,10);C(10,0);
            }
            Normalize(a);J(done);
            Bind(zero);C(0,0);for(int i=0;i<32;i+=8)Store(0,a+i);Put(a+32,0);Bind(done);
        }
        private void ShiftLeftOneCarry(int p)
        {
            for(int i=0;i<32;i+=8)
            {Load(0,p+i);RR(i==0?0x01:0x11,0,0);Store(0,p+i);}
            C(8,0);Set(8,2); // exact carry (MOV immediate does not change CF)
        }
        // a /= b; fixed A/BB operands, clobbers CC/DD. Divisor nonzero.
        // 256 quotient-bit iterations; remainder stays <divisor at each step.
        // The shifted remainder needs 257 bits; R8 holds its exact high bit.
        private void Divide()
        {
            int less=L(),start=L(),loop=L(),take=L(),skip=L(),done=L();
            Copy(CC,A);Zero(DD);
            Load(0,A+32,32);Load(1,BB+32,32);RR(0x29,0,1,false);Store(0,DD+32,32);
            Load(0,A+36,32);Load(1,BB+36,32);RR(0x31,0,1,false);Store(0,DD+36,32);
            MagnitudeTest(A);J(done,4);
            C(8,0);CompareSignificands(CC,BB,less,start,start);
            Bind(less);ShiftLeftOneCarry(CC);AddExponent(DD,-1); // ADD clobbers CF, saved R8 survives
            Bind(start);C(10,256);Bind(loop);C(9,0);Test(8);J(take,5);
            CompareSignificands(CC,BB,skip,take,take);
            Bind(take);
            for(int i=0;i<32;i+=8)
            {Load(0,CC+i);Load(1,BB+i);RR(i==0?0x29:0x19,0,1);Store(0,CC+i);}
            C(9,1);Bind(skip);
            for(int i=0;i<32;i+=8)
            {Load(0,DD+i);RR(i==0?0x01:0x11,0,0);Store(0,DD+i);}
            Load(0,DD);RR(0x09,0,9);Store(0,DD);
            ShiftLeftOneCarry(CC);Alu(5,10,1,false);J(loop,5);
            Bind(done);Copy(A,DD);
        }
        // R10 divisor in [1,26000]. Return the normalized 256-bit FLOOR of
        // the quotient, not a floor at the original (up to 15 bits coarser)
        // scale. RDX<divisor before each DIV, so every quotient fits uint64.
        private void DivideSmall(int p)
        {
            int done=L();MagnitudeTest(p);J(done,4);
            C(2,0);
            for(int i=24;i>=0;i-=8)
            {Load(0,p+i);B(0x49);B(0xf7);B(0xf2);Store(0,p+i);} // DIV R10
            // N=d*M+r. Normalize shifts M left t<=15. Append floor(r*2^t/d)
            // into those t zero bits. R8/R9/R10 survive Normalize; it uses only
            // RAX/RCX/RDX. r*2^t < 26000*32768 fits uint32. No new Work bytes.
            Mov(9,2);Load(8,p+32,32);Normalize(p);
            Load(1,p+32,32);RR(0x29,8,1,false);Mov(1,8,false);
            Mov(0,9);Shift(4,0);C(2,0);B(0x49);B(0xf7);B(0xf2);
            Load(1,p);RR(0x09,0,1);Store(0,p);Bind(done);
        }
    }
}
