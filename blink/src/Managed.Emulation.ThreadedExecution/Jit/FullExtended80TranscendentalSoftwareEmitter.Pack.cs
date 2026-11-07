namespace Managed.Emulation.Execution;

internal static partial class FullExtended80TranscendentalSoftwareEmitter
{
    private ref partial struct Writer
    {
        // Returns RAX rounded 64-bit significand, R10 signed unbounded exponent,
        // R9 delta PE/C1. Slot preserved. forcePrecision denotes a non-anchor
        // approximation, not a proof of the exact mathematical PE/C1 decision.
        private void Round64(int p,bool forcePrecision)
        {
            int done=L(),nearest=L(),increment=L(),truncate=L();
            Load(0,p+24);Load(10,p+32,32);C(9,forcePrecision?32:0);
            Load(8,p);Load(1,p+8);RR(0x09,8,1);Load(1,p+16);RR(0x09,8,1);
            Test(8);J(done,4);C(9,32);
            Load(1,8,16);Shift(5,1,10,false);Alu(4,1,3,false);Test(1,false);J(nearest,4);
            Alu(7,1,3,false);J(truncate,4);
            Load(2,p+36,32);RR(0x01,1,2,false);Alu(7,1,2,false);J(increment,4);J(truncate);
            Bind(nearest);Load(1,p+16);Imm(2,0x8000000000000000UL);RR(0x39,1,2);
            J(increment,7);J(truncate,2);
            Load(1,p);Load(2,p+8);RR(0x09,1,2);Test(1);J(increment,5);
            Mov(1,0);Alu(4,1,1);J(truncate,4);
            Bind(increment);Alu(0,0,1);C(9,0x220);J(truncate,5);
            Imm(0,0x8000000000000000UL);Alu(0,10,1,false);
            Bind(truncate);Bind(done);
        }
        // Actual-mask images occupy Normal/Second, matching the existing
        // captured-result protocol convention. NormalStatus remains the masked
        // delta; WrappedStatus is the actual completion delta. No Host bit.
        // Clobbers p only on masked subnormal conversion. Other slots retained.
        private void Pack(int p,int dest,bool actual,bool forcePrecision)
        {
            int zero=L(),overflow=L(),tiny=L(),write=L(),subnormal=L(),encoded=L();
            MagnitudeTest(p);J(zero,4);Round64(p,forcePrecision);
            Alu(7,10,16383,false);J(overflow,15);
            Alu(7,10,-16382,false);J(tiny,12);
            Alu(0,10,16383,false);J(write);
            Bind(overflow);
            if(actual)
            {
                Load(1,8,16);Alu(4,1,8,false);int masked=L();J(masked,5);
                Alu(0,10,16383-24576,false);Alu(1,9,8,false);J(write);Bind(masked);
            }
            C(9,0x28); // masked overflow necessarily loses finite precision
            Load(1,8,16);Shift(5,1,10,false);Alu(4,1,3,false);
            int infinity=L(),maximum=L();Test(1,false);J(infinity,4);
            Load(2,p+36,32);RR(0x01,1,2,false);Alu(7,1,2,false);J(infinity,4);J(maximum);
            Bind(infinity);Imm(0,0x8000000000000000UL);C(10,0x7fff);C(9,0x228);J(write);
            Bind(maximum);Imm(0,ulong.MaxValue);C(10,0x7ffe);J(write);
            Bind(tiny);
            if(actual)
            {
                Load(1,8,16);Alu(4,1,16,false);int masked=L();J(masked,5);
                Alu(0,10,16383+24576,false);Alu(1,9,16,false);J(write);Bind(masked);
            }
            // Re-round the ORIGINAL wide value at the subnormal unit, rather
            // than rescaling the already-rounded normal significand.
            C(1,-16382);Load(0,p+32,32);RR(0x29,1,0,false);ShiftRightJam(p);
            Put(p+32,-16382);Round64(p,forcePrecision);
            C(10,0);Test(0);J(subnormal,9); // top bit zero -> raw exponent0
            C(10,1);Bind(subnormal);
            Mov(1,9,false);Alu(4,1,32,false);J(write,4);Alu(1,9,16,false);J(write);
            Bind(zero);C(0,0);C(9,forcePrecision?32:0);C(10,0);
            Bind(write);Store(0,dest);C(1,0);Store(1,dest+8);
            Load(1,p+36,32);Shift(4,1,15,false);RR(0x09,10,1,false);Store(10,dest+8,16);
            // label reserved to keep all exits explicit in parser inspection.
            Bind(encoded);
        }
        private void PackSingle(bool approximate)
        {
            Copy(BB,CC);Pack(BB,48,false,approximate);Store(9,80,16);
            Copy(BB,CC);Pack(BB,48,true,approximate);Store(9,82,16);
            C(0,0);Store(0,64);Store(0,72);
        }
        private void PackPair(bool tangent)
        {
            if(tangent)
            {
                PackSingle(true);Imm(0,0x8000000000000000UL);Store(0,64);Put(72,0x3fff);
            }
            else
            {
                // A=full sine, C=full cosine. Both survive each BB conversion.
                // AMD APM Vol5 rev3.16 FSINCOS p298 assigns C1 to final
                // ST(1), the sine. Preserve sine C1; combine only exception
                // bits from cosine. Original candidate cosine policy was wrong.
                Copy(BB,A);Pack(BB,48,false,true);Store(9,80,16);
                Copy(BB,A);Pack(BB,48,true,true);Store(9,82,16);
                Copy(BB,CC);Pack(BB,64,false,true);Alu(4,9,~0x200,false);Load(0,80,16);RR(0x09,0,9,false);Store(0,80,16);
                Copy(BB,CC);Pack(BB,64,true,true);Alu(4,9,~0x200,false);Load(0,82,16);RR(0x09,0,9,false);Store(0,82,16);
            }
        }
    }
}
