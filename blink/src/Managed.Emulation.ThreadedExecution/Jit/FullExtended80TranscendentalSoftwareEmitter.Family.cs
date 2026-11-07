namespace Managed.Emulation.Execution;

internal static partial class FullExtended80TranscendentalSoftwareEmitter
{
    private ref partial struct Writer
    {
        private static bool Binary(FullExtended80Transcendental op)=>op is FullExtended80Transcendental.Fyl2x or FullExtended80Transcendental.Fyl2xp1 or FullExtended80Transcendental.Fpatan;
        private static bool Pair(FullExtended80Transcendental op)=>op is FullExtended80Transcendental.Fptan or FullExtended80Transcendental.Fsincos;
        private static bool Trig(FullExtended80Transcendental op)=>op is FullExtended80Transcendental.Fptan or FullExtended80Transcendental.Fsincos or FullExtended80Transcendental.Fsin or FullExtended80Transcendental.Fcos;
        // R8: zero0, finite1, infinity2, qNaN3, sNaN4, unsupported5, DE6.
        private void Classify(int raw)
        {
            int e0=L(),special=L(),bad=L(),done=L(),notInf=L();
            Load(9,raw);Load(0,raw+8,16);Alu(4,0,0x7fff,false);Test(0,false);J(e0,4);
            Test(9);J(bad,9);Alu(7,0,0x7fff,false);J(special,4);C(8,1);J(done);
            Bind(e0);C(8,0);Test(9);J(done,4);C(8,6);J(done);
            Bind(special);Imm(0,0x8000000000000000UL);RR(0x39,9,0);J(notInf,5);C(8,2);J(done);
            Bind(notInf);Mov(0,9);Shift(5,0,62);Alu(4,0,1,false);C(8,4);RR(0x29,8,0,false);J(done);
            Bind(bad);C(8,5);Bind(done);
        }
        private void IsInfinity(int raw,int target)
        {Load(0,raw+8,16);Alu(4,0,0x7fff,false);Alu(7,0,0x7fff,false);J(target,4);}
        private void IsZero(int raw,int target)
        {Load(0,raw);Test(0);J(target,4);}
        private void OutputSign(int raw,int dest)
        {Load(0,raw+8,16);Alu(4,0,0x8000,false);Store(0,dest+8,16);}
        private void OutputXorSign(int first,int second,int dest,bool invert=false)
        {
            Load(0,first+8,16);Load(1,second+8,16);RR(0x31,0,1,false);
            if(invert)Alu(6,0,0x8000,false);Alu(4,0,0x8000,false);Store(0,dest+8,16);
        }
        private void InfinityOutput(bool negative=false)
        {Imm(0,0x8000000000000000UL);Store(0,48);Put(56,negative?0xffff:0x7fff);}
        private void SignedInfinity(int first,int second,bool invert)
        {OutputXorSign(first,second,48,invert);Load(0,56,16);Alu(1,0,0x7fff,false);Store(0,56,16);Imm(0,0x8000000000000000UL);Store(0,48);}
        private void CopyRawOutput(int raw,int output)
        {Load(0,raw);Store(0,output);C(0,0);Store(0,output+8);Load(0,raw+8,16);Store(0,output+8,16);}
        private void NaNOutput(FullExtended80Transcendental op,bool invalid,bool indefinite)
        {
            if(indefinite){Imm(0,0xc000000000000000UL);Store(0,48);Put(56,0xffff);}
            else
            {
                int useX=L(),useY=L(),chosen=L(),xQuiet=L(),sameClass=L();
                if(Binary(op))
                {
                    Load(0,80,32);Alu(7,0,3,false);J(xQuiet,4);
                    Load(0,84,32);Alu(7,0,3,false);J(useY,4);
                    Load(0,80,32);Alu(7,0,4,false);J(useY,5);
                    Load(0,84,32);Alu(7,0,4,false);J(useX,5);J(sameClass);
                    Bind(xQuiet);Load(0,84,32);Alu(7,0,3,false);J(useX,5);
                    Bind(sameClass);
                    // Same-class larger payload; equal payload selects left,
                    // retaining the prior integer arithmetic profile's sign tie.
                    Load(0,16);Load(1,32);RR(0x39,0,1);J(useY,2);J(useX);
                }
                else J(useX);
                Bind(useX);CopyRawOutput(16,48);J(chosen);Bind(useY);CopyRawOutput(32,48);Bind(chosen);
                Load(0,48);Imm(1,0xc000000000000000UL);RR(0x09,0,1);Store(0,48);
            }
            if(op==FullExtended80Transcendental.Fsincos)CopyRawOutput(48,64);
            else if(op==FullExtended80Transcendental.Fptan){Imm(0,0x8000000000000000UL);Store(0,64);Put(72,0x3fff);}
            Put(80,invalid?0x00010001:0);
        }
        private void Scrub()
        {C(0,0);for(int p=96;p<256;p+=8)Store(0,p);}
        private void Finish(FullExtended80Transcendental op,int exit,bool includeDenormal)
        {
            int noDE=L(),noDE2=L(),noInvalid=L(),usable=L(),complete=L();
            Put(12,0);
            if(includeDenormal)
            {
                Load(0,24,16);Alu(4,0,0x7fff,false);J(noDE,5);Load(0,16);Test(0);J(noDE,4);
                Bits(80,1,0x00020002);Bind(noDE);
                if(Binary(op))
                {
                    Load(0,40,16);Alu(4,0,0x7fff,false);J(noDE2,5);Load(0,32);Test(0);J(noDE2,4);
                    Bits(80,1,0x00020002);Bind(noDE2);
                }
            }
            Load(2,82,16);Mov(0,2,false);Alu(4,0,1,false);J(noInvalid,4);C(2,1);Put(80,0x00010001);Bind(noInvalid);
            Load(1,8,16);Alu(6,1,-1,false);RR(0x21,1,2,false);Alu(4,1,7,false);J(usable,4);
            // Lowest unmasked pre-event wins and suppresses both destinations.
            Mov(0,1,false);Alu(5,0,1,false);RR(0x21,0,1,false);RR(0x31,1,0,false);Store(1,82,16);
            C(0,0);for(int p=48;p<80;p+=8)Store(0,p);Put(84,(int)op+1|0x10400);Scrub();Put(232,0x02000000);J(exit);
            Bind(usable);C(8,(int)op+1|0x10100|(Pair(op)?0x200:0));
            Mov(0,2,false);Alu(4,0,32,false);J(complete,4);Load(0,8,16);Alu(4,0,32,false);J(complete,5);Alu(1,8,0x1000,false);
            Bind(complete);Store(8,84,32);Mov(8,2,false);Alu(4,8,0x200,false);
            Scrub();Store(8,232,16);C(0,Trig(op)?0x600:0x200);Store(0,234,16);J(exit);
        }
        internal void Family(FullExtended80Transcendental op,bool windows,bool continuation)
        {
            int badVersion=L(),badSize=L(),exit=L(),invalid=L(),snan=L(),qnan=L(),range=L(),undefined=L();
            int exact=L(),approx=L(),special=L();
            B(0xf3);B(0x0f);B(0x1e);B(0xfa);if(!continuation)Mov(11,windows?1:7);
            C(0,0);for(int p=48;p<88;p+=8)Store(0,p);Store(0,12,32);
            Load(0,0,32);Alu(7,0,1,false);J(badVersion,5);Load(0,4,32);Alu(7,0,384,false);J(badSize,5);
            Classify(16);Store(8,80,32);
            if(Binary(op)){Classify(32);Store(8,84,32);}else Put(84,0);
            foreach(int p in Binary(op)?new[]{80,84}:new[]{80}){Load(0,p,32);Alu(7,0,5,false);J(invalid,4);}
            foreach(int p in Binary(op)?new[]{80,84}:new[]{80}){Load(0,p,32);Alu(7,0,4,false);J(snan,4);}
            foreach(int p in Binary(op)?new[]{80,84}:new[]{80}){Load(0,p,32);Alu(7,0,3,false);J(qnan,4);}
            Put(80,0);Put(84,0);
            if(Trig(op))
            {
                IsInfinity(16,invalid);Load(0,24,16);Alu(4,0,0x7fff,false);Alu(7,0,0x403e,false);J(range,13);
                int nonzero=L();IsZero(16,special);J(nonzero);Bind(nonzero);
                if(op==FullExtended80Transcendental.Fsin){TrigPart(false);J(approx);}
                else if(op==FullExtended80Transcendental.Fcos){TrigPart(true);J(approx);}
                else
                {
                    TrigPart(false);SaveSine();TrigPart(true);LoadSine();Put(80,0);
                    if(op==FullExtended80Transcendental.Fptan)
                    {Copy(BB,CC);Divide();Copy(CC,A);PackPair(true);}
                    else PackPair(false);
                    Finish(op,exit,true);
                }
            }
            else if(op==FullExtended80Transcendental.F2xm1)
            {
                IsZero(16,special);IsInfinity(16,undefined);FromRaw(A,16);Load(0,A+32,32);Alu(7,0,0,false);J(undefined,15);
                int ordinary=L();J(ordinary,12);Load(0,A+24);Imm(1,0x8000000000000000UL);RR(0x39,0,1);J(undefined,5);
                // Exactly +/-1, handled without an approximation or PE.
                One(CC);Load(0,24,16);Alu(4,0,0x8000,false);int positive=L();J(positive,4);
                Put(CC+32,-1);Put(CC+36,1);Bind(positive);J(exact);
                Bind(ordinary);IsZero(16,special);Expm1Kernel();J(approx);
            }
            else if(op==FullExtended80Transcendental.Fyl2xp1)
            {
                IsZero(16,special);IsInfinity(16,undefined);FromRaw(A,16);Load(0,A+32,32);Alu(7,0,-2,false);J(undefined,15);
                int domain=L(),negative=L(),compare=L();J(domain,12);
                Load(0,A+36,32);Test(0,false);J(negative,5);Imm(1,0xd413cccfe7799211UL);J(compare);
                Bind(negative);Imm(1,0x95f619980c4336f7UL);Bind(compare);Load(0,A+24);RR(0x39,0,1);J(undefined,7);Bind(domain);
                IsZero(16,special);IsZero(32,special);IsInfinity(32,special);LogKernel(true);J(approx);
            }
            else if(op==FullExtended80Transcendental.Fyl2x)
            {
                int nonnegative=L();IsZero(16,special);Load(0,24,16);Alu(4,0,0x8000,false);J(invalid,5);Bind(nonnegative);
                IsInfinity(16,special);IsInfinity(32,special);IsZero(32,special);
                FromRaw(A,16);Load(0,A+24);Imm(1,0x8000000000000000UL);RR(0x39,0,1);int general=L();J(general,5);
                // General exact dyadic anchor: k*y, including subnormal inputs,
                // range wrapping and every y exponent. No artificial [-16000,16000] bound.
                Load(0,A+32,32);FromSigned(BB);FromRaw(CC,32);Multiply(CC,BB);J(exact);
                Bind(general);LogKernel(false);J(approx);
            }
            else
            {
                IsZero(16,special);IsZero(32,special);IsInfinity(16,special);IsInfinity(32,special);
                AtanKernel();J(approx);
            }
            Bind(exact);PackSingle(false);
            if(Pair(op)){Imm(0,0x8000000000000000UL);Store(0,64);Put(72,0x3fff);}
            Finish(op,exit,true);
            Bind(approx);PackSingle(true);Finish(op,exit,true);
            Bind(special);SpecialValues(op,invalid,exact,approx,exit);
            Bind(invalid);NaNOutput(op,true,true);Finish(op,exit,false);
            Bind(snan);NaNOutput(op,true,false);Finish(op,exit,false);
            Bind(qnan);NaNOutput(op,false,false);Finish(op,exit,false);
            Bind(range);Put(80,0);Put(84,(int)op+1|0x800);Scrub();Put(232,0x04000400);J(exit);
            Bind(undefined);CopyRawOutput(op==FullExtended80Transcendental.F2xm1?16:32,48);
            Put(80,0);Put(84,(int)op+1|0x8100);Scrub();Put(232,0x02000000);J(exit);
            Bind(badVersion);Put(12,1);J(exit);Bind(badSize);Put(12,2);J(exit);
            Bind(exit);Exit(continuation);
        }
        private void SpecialValues(FullExtended80Transcendental op,int invalid,int exact,int approx,int exit)
        {
            if(Trig(op))
            {
                if(op==FullExtended80Transcendental.Fcos)One(CC);else FromRaw(CC,16);
                J(exact);return;
            }
            if(op==FullExtended80Transcendental.F2xm1){FromRaw(CC,16);J(exact);return;}
            if(op==FullExtended80Transcendental.Fyl2xp1)
            {
                int infinity=L();IsInfinity(32,infinity);Zero(CC);
                Load(0,24,16);Load(1,40,16);RR(0x31,0,1,false);Shift(5,0,15,false);Store(0,CC+36,32);J(exact);
                Bind(infinity);IsZero(16,invalid);SignedInfinity(16,32,false);Put(80,0);Finish(op,exit,true);return;
            }
            if(op==FullExtended80Transcendental.Fyl2x)
            {
                int zeroX=L(),infinityX=L(),infinityY=L(),negativeLog=L(),signedZero=L(),positiveLog=L();
                IsZero(16,zeroX);IsInfinity(16,infinityX);IsInfinity(32,infinityY);
                // y=0, x positive finite; sign log(x) follows x<1.
                FromRaw(A,16);Load(0,A+32,32);Test(0,false);J(negativeLog,8);J(positiveLog);
                Bind(negativeLog);Zero(CC);Load(0,40,16);Shift(5,0,15,false);Alu(6,0,1,false);Store(0,CC+36,32);J(exact);
                Bind(positiveLog);Zero(CC);Load(0,40,16);Shift(5,0,15,false);Store(0,CC+36,32);J(exact);
                Bind(zeroX);IsZero(32,invalid);SignedInfinity(32,32,false); // reset sign below: log(0) negative
                Load(0,40,16);Alu(6,0,0x8000,false);Alu(4,0,0x8000,false);Alu(1,0,0x7fff,false);Store(0,56,16);
                Put(80,0x00040004);Finish(op,exit,true);
                Bind(infinityX);IsZero(32,invalid);InfinityOutput();OutputSign(32,48);Load(0,56,16);Alu(1,0,0x7fff,false);Store(0,56,16);
                Put(80,0);Finish(op,exit,true);
                Bind(infinityY);FromRaw(A,16);Load(0,A+32,32);Test(0,false);int minus=L(),plus=L();J(minus,8);J(plus,5);
                Load(0,A+24);Imm(1,0x8000000000000000UL);RR(0x39,0,1);J(invalid,4);J(plus);
                Bind(minus);InfinityOutput();Load(0,40,16);Alu(6,0,0x8000,false);Alu(4,0,0x8000,false);Alu(1,0,0x7fff,false);Store(0,56,16);J(signedZero);
                Bind(plus);InfinityOutput();Load(0,40,16);Alu(4,0,0x8000,false);Alu(1,0,0x7fff,false);Store(0,56,16);
                Bind(signedZero);Put(80,0);Finish(op,exit,true);return;
            }
            // FPATAN atan2: signed axes, +/-infinities, all four quadrants.
            int yZero=L(),xZero=L(),xInf=L(),yInf=L(),bothInf=L(),pi=L(),zero=L(),sign=L();
            IsZero(32,yZero);IsZero(16,xZero);IsInfinity(16,xInf);J(yInf);
            Bind(yZero);Load(0,24,16);Alu(4,0,0x8000,false);J(pi,5);J(zero);
            Bind(xZero);Bind(yInf);HalfPi(CC);J(sign);
            Bind(xInf);IsInfinity(32,bothInf);Load(0,24,16);Alu(4,0,0x8000,false);J(pi,5);J(zero);
            Bind(bothInf);QuarterPi(CC);Load(0,24,16);Alu(4,0,0x8000,false);int quarter=L();J(quarter,4);
            Negate(CC);Pi(BB);Add(CC,BB);Bind(quarter);J(sign);
            Bind(pi);Pi(CC);J(sign);
            Bind(zero);Zero(CC);Load(0,40,16);Shift(5,0,15,false);Store(0,CC+36,32);J(exact);
            Bind(sign);Load(0,40,16);Shift(5,0,15,false);Store(0,CC+36,32);J(approx);
        }
    }
}
