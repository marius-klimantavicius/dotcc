namespace Managed.Emulation.Execution;

internal static partial class FullExtended80TranscendentalSoftwareEmitter
{
    private ref partial struct Writer
    {
        private void Counter(int first)=>Put(12,first);
        private void NextCounter(int loop,int last)
        {Bits(12,0,1);Load(0,12,32);Alu(7,0,last,false);J(loop,14);}
        private void LoadCounterDivisor(int multiplier,int add)
        {
            Load(10,12,32);
            if(multiplier==2)Shift(4,10,1,false);
            if(add!=0)Alu(0,10,add,false);
        }
        // Input D=u; output C=sum u^(2j+1)/(2j+1). Term A retains the
        // undivided power, so each denominator is applied exactly once.
        private void OddSeries(bool alternating)
        {
            Copy(A,DD);Copy(CC,DD);Counter(1);int loop=L();Bind(loop);
            Copy(BB,DD);Multiply(A,BB);Multiply(A,BB);
            if(alternating)Negate(A);
            Copy(BB,A);LoadCounterDivisor(2,1);DivideSmall(BB);Add(CC,BB);
            NextCounter(loop,160);
        }
        // x is bounded [-1,+1]. Factoring x*ln2 avoids the subtraction 2^x-1
        // at every exponent, including minimum binary80 and pseudo-denormals.
        private void Expm1Kernel()
        {
            FromRaw(A,16);Ln2(BB);Multiply(A,BB);Copy(DD,A);Copy(CC,A);
            Counter(2);int loop=L();Bind(loop);
            Copy(BB,DD);Multiply(A,BB);LoadCounterDivisor(1,0);DivideSmall(A);
            Copy(BB,A);Add(CC,BB);NextCounter(loop,128);
        }
        // For FYL2X normalize x=2^k*m, m in [1,2). For log1p use x/(2+x)
        // directly, so the small operand never disappears in rounded 1+x.
        private void LogKernel(bool plusOne)
        {
            FromRaw(A,16);
            if(plusOne)
            {Copy(CC,A);One(BB,1);Add(CC,BB);Copy(BB,CC);Divide();}
            else
            {
                // For x in [1/2,1), use x itself: log2(x)=2/ln2*atanh((x-1)/(x+1)).
                // This avoids subtracting a nearly-one logarithm from k=-1.
                // For all other binades use m in [1,2). k and log2(m) then
                // have the same sign, or their sum has magnitude >=1.
                int normalized=L(),haveMantissa=L();
                Load(0,A+32,32);Alu(7,0,-1,false);J(normalized,5);
                Put(60,0);J(haveMantissa);
                Bind(normalized);Store(0,60,32);Put(A+32,0);Bind(haveMantissa);
                Copy(CC,A);One(BB);Negate(BB);Add(A,BB);
                One(BB);Add(CC,BB);Copy(BB,CC);Divide();
            }
            Copy(DD,A);OddSeries(false);TwoOverLn2(BB);Multiply(CC,BB);
            if(!plusOne)
            {Load(0,60,32);FromSigned(A);Add(CC,A);}
            FromRaw(A,32);Multiply(CC,A);
        }
        // r in [0,1], alternating series after an optional (r-1)/(r+1)
        // transform. |argument|<=1/2; the transformed part is <=1/3.
        private void AtanKernel()
        {
            int swap=L(),ordered=L(),transform=L(),series=L(),noTransform=L(),noSwap=L(),positiveX=L();
            FromRaw(A,16);FromRaw(BB,32);Magnitude(A);Magnitude(BB);
            // Work60 bit0: ratio was reciprocated, bit1: transform selected.
            Put(60,0);CompareMagnitude(A,BB,swap,ordered,ordered);
            Bind(swap);
            for(int i=0;i<40;i+=8){Load(0,A+i);Load(1,BB+i);Store(1,A+i);Store(0,BB+i);}
            // A=max(|x|,|y|), B=min; after this unconditional swap below,
            // A=min and B=max. This branch corresponds |y|>|x|.
            Put(60,1);Bind(ordered);
            for(int i=0;i<40;i+=8){Load(0,A+i);Load(1,BB+i);Store(1,A+i);Store(0,BB+i);}
            Divide();
            Load(0,A+32,32);Alu(7,0,-1,false);J(series,12);J(transform,15);
            // Ratio==1/2 needs no transform; values above it do.
            Load(0,A+24);Imm(1,0x8000000000000000UL);RR(0x39,0,1);J(transform,7);
            Load(0,A);Load(1,A+8);RR(0x09,0,1);Load(1,A+16);RR(0x09,0,1);Test(0);J(series,4);
            Bind(transform);Bits(60,1,2);Copy(CC,A);One(BB);Negate(BB);Add(A,BB);
            One(BB);Add(CC,BB);Copy(BB,CC);Divide();
            Bind(series);Copy(DD,A);OddSeries(true);
            Load(0,60,32);Alu(4,0,2,false);J(noTransform,4);QuarterPi(BB);Add(CC,BB);Bind(noTransform);
            Load(0,60,32);Alu(4,0,1,false);J(noSwap,4);
            Negate(CC);HalfPi(BB);Add(CC,BB);Bind(noSwap);
            Load(0,24,16);Alu(4,0,0x8000,false);J(positiveX,4);
            Negate(CC);Pi(BB);Add(CC,BB);Bind(positiveX);
            Load(0,40,16);Shift(5,0,15,false);Store(0,CC+36,32);
        }
        // Range |x|<2^63, quotient n=nearest(|x|*2/pi) fits signed uint63.
        // Constants retain 256 bits. proof/trig-separation.json certifies ALL
        // 65 input binades, including half-quadrant boundaries, not samples:
        // distance >2^-71 dominates quotient error <2^-188. Thus n and its
        // quadrant are exact. Residual error is absolute, not relative to r.
        // Work84 retains n mod4; original sign stays in raw input24.
        private void ReduceTrig()
        {
            int small=L(),reduced=L();FromRaw(A,16);Magnitude(A);
            Load(0,A+32,32);Alu(7,0,-2,false);J(small,12);
            TwoOverPi(BB);Multiply(A,BB);
            // Round the wide positive quotient to an integer. e<=62; values
            // below 1/2 take n=0 and need no potentially negative shift count.
            Load(0,A+32,32);Alu(7,0,-1,false);int zeroN=L(),haveN=L();J(zeroN,12);
            C(1,63);RR(0x29,1,0,false); // discarded top-word bits, 1..64
            Alu(7,1,64,false);int lessOne=L();J(lessOne,4);
            Load(8,A+24);Mov(9,8);Shift(5,8);Alu(5,1,1,false);Shift(5,9);Alu(4,9,1);RR(0x01,8,9);J(haveN);
            Bind(lessOne);C(8,1);J(haveN);Bind(zeroN);C(8,0);Bind(haveN);
            Mov(0,8);Alu(4,0,3,false);Store(0,84,32);
            Mov(0,8);FromUnsigned(BB);HalfPi(CC);Multiply(BB,CC);
            FromRaw(A,16);Magnitude(A);Negate(BB);Add(A,BB);Copy(DD,A);J(reduced);
            Bind(small);Put(84,0);Copy(DD,A);Bind(reduced);
        }
        // D=reduced angle. Output C, preserving D. Sine/cosine share the same
        // 80-step factorial recurrence. Divisors max162*161<2^15.
        private void SinCosSeries(bool cosine)
        {
            if(cosine){One(A);One(CC);}else{Copy(A,DD);Copy(CC,DD);}
            Counter(1);int loop=L();Bind(loop);
            Copy(BB,DD);Multiply(A,BB);Multiply(A,BB);Negate(A);
            LoadCounterDivisor(2,cosine?-1:1);Load(8,12,32);Shift(4,8,1,false);
            // IMUL R10,R8, exact nonnegative bounded integer divisor.
            Rex(true,10,8);B(0x0f);B(0xaf);B(0xc0|((10&7)<<3)|(8&7));
            DivideSmall(A);Copy(BB,A);Add(CC,BB);NextCounter(loop,80);
        }
        private void TrigPart(bool cosine)
        {
            ReduceTrig();int sin=L(),evaluated=L(),signDone=L();
            Load(0,84,32);Alu(4,0,1,false);Alu(7,0,cosine?1:0,false);J(sin,4);
            SinCosSeries(true);J(evaluated);Bind(sin);SinCosSeries(false);Bind(evaluated);
            Load(0,84,32);
            if(cosine)Alu(0,0,1,false);
            Shift(5,0,1,false);Alu(4,0,1,false);
            if(!cosine)
            {Load(1,24,16);Shift(5,1,15,false);RR(0x31,0,1,false);}
            Test(0,false);J(signDone,4);Negate(CC);Bind(signDone);
        }
        // Output staging48..79 temporarily holds a FULL 256-bit sine. Its
        // exponent fits int16 for this trig domain; sign is staged82. This is
        // explicit charged output storage, not private Work or host storage.
        private void SaveSine()
        {
            for(int i=0;i<32;i+=8){Load(0,CC+i);Store(0,48+i);}
            Load(0,CC+32,32);Store(0,80,16);Load(0,CC+36,32);Store(0,82,16);
        }
        private void LoadSine()
        {
            for(int i=0;i<32;i+=8){Load(0,48+i);Store(0,A+i);}
            Load(0,80,16);Shift(4,0,16,false);Shift(7,0,16,false);Store(0,A+32,32);
            Load(0,82,16);Store(0,A+36,32);
        }
    }
}
