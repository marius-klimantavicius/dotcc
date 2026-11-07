using System;
using TX = Managed.Emulation.Execution.FullCanonicalX87TranscendentalOperation;

namespace Managed.Emulation.Execution;

internal static partial class FullBlockEmitter
{
    private ref partial struct Writer
    {
        // Independent response proof from RAW inputs, never kernel Decode data,
        // metadata assertions, or a helper CALL. Scratch288..319 belongs to this
        // continuation; R10=frame/R11=Machine/R9=Work are preserved throughout.
        private void CanonicalX87ValidateTranscendentalInteger(TX op,scoped Span<int> bad,ref int count)
        {
            bool pair=FullCanonicalX87TranscendentalLowering.Pushes(op);
            bool trig=op is TX.Fptan or TX.Fsincos or TX.Fsin or TX.Fcos;
            // Authenticate Work inputs against the still-uncommitted guest raw
            // registers: a response may not rewrite its own domain witness.
            CanonicalX87IntegerInputMatch(0,16,bad,ref count);
            if(FullCanonicalX87TranscendentalLowering.IsBinary(op))CanonicalX87IntegerInputMatch(1,32,bad,ref count);
            CanonicalX87WorkConstant(288,0);CanonicalX87WorkConstant(292,0);
            CanonicalX87WorkConstant(296,0);CanonicalX87WorkConstant(300,0);
            if(op==TX.Fyl2x)
            {
                CanonicalX87WorkLoad(24,1,16);IntegerConstant(7,1,1);bad[count++]=NearCondition(0x82);
                IntegerConstant(7,1,0x7fff);bad[count++]=NearCondition(0x83); // x positive normal
                IntegerImmediate(2,0x8000000000000000UL);CanonicalX87ControlMatch(16,2,64,bad,ref count);
                CanonicalX87IntegerZeroOrNormal(32,bad,ref count);
                CanonicalX87WorkLoad(40,0,16);IntegerConstant(4,0,0x7fff);int zeroY=NearCondition(0x84);
                CanonicalX87WorkLoad(24,0,16);IntegerConstant(7,0,0x3fff);int zeroLog=NearCondition(0x84);
                CanonicalX87WorkLoad(40,0,16);IntegerConstant(4,0,0x7fff);
                IntegerConstant(7,0,383);bad[count++]=NearCondition(0x82); // -16000 +16383
                IntegerConstant(7,0,32383);bad[count++]=NearCondition(0x87);
                CanonicalX87IntegerLogProduct();int productDone=IntegerJump();
                Patch(zeroY,position);Patch(zeroLog,position);
                CanonicalX87WorkLoad(24,0,16);IntegerConstant(7,0,0x3fff);
                Bytes(0x0f,0x92,0xc0,0x0f,0xb6,0xc0);IntegerShift(4,0,32,15);
                CanonicalX87WorkLoad(40,1,16);IntegerConstant(4,1,0x8000);IntegerRR(0x31,0,1,32);
                CanonicalX87WorkStore(296,0,32);Patch(productDone,position);
            }
            else if(op==TX.Fyl2xp1)
            {
                CanonicalX87IntegerZero(16,bad,ref count);CanonicalX87IntegerZeroOrNormal(32,bad,ref count);
                CanonicalX87WorkLoad(24,0,16);CanonicalX87WorkLoad(40,1,16);
                IntegerRR(0x31,0,1,32);IntegerConstant(4,0,0x8000);CanonicalX87WorkStore(296,0,32);
            }
            else if(op==TX.Fpatan)
            {
                CanonicalX87IntegerZero(32,bad,ref count);CanonicalX87IntegerZeroOrNormal(16,bad,ref count);
                CanonicalX87WorkLoad(24,0,16);IntegerConstant(4,0,0x8000);bad[count++]=NearCondition(0x85);
                CanonicalX87WorkLoad(40,0,16);CanonicalX87WorkStore(296,0,32);
            }
            else if(op==TX.F2xm1)
            {
                CanonicalX87WorkLoad(24,0,16);IntegerConstant(4,0,0x7fff);int zero=NearCondition(0x84);
                IntegerConstant(7,0,0x3fff);bad[count++]=NearCondition(0x85);
                IntegerImmediate(2,0x8000000000000000UL);CanonicalX87ControlMatch(16,2,64,bad,ref count);
                CanonicalX87WorkStore(288,2);CanonicalX87WorkLoad(24,0,16);IntegerConstant(4,0,0x8000);
                int minus=NearCondition(0x85);CanonicalX87WorkConstant(296,0x3fff);int endpoint=IntegerJump();
                Patch(minus,position);CanonicalX87WorkConstant(296,0xbffe);Patch(endpoint,position);int done=IntegerJump();
                Patch(zero,position);CanonicalX87ControlEqual(16,0,64,bad,ref count);
                CanonicalX87WorkLoad(24,0,16);CanonicalX87WorkStore(296,0,32);Patch(done,position);
            }
            else
            {
                CanonicalX87IntegerZero(16,bad,ref count);
                if(op==TX.Fcos)
                {IntegerImmediate(2,0x8000000000000000UL);CanonicalX87WorkStore(288,2);CanonicalX87WorkConstant(296,0x3fff);}
                else {CanonicalX87WorkLoad(24,0,16);CanonicalX87WorkStore(296,0,32);}
            }
            CanonicalX87WorkLoad(288,2);CanonicalX87ControlMatch(48,2,64,bad,ref count);
            CanonicalX87WorkLoad(296,2,16);CanonicalX87ControlMatch(56,2,16,bad,ref count);
            CanonicalX87ControlEqual(58,0,16,bad,ref count);CanonicalX87ControlEqual(60,0,32,bad,ref count);
            if(pair)
            {
                IntegerImmediate(2,0x8000000000000000UL);CanonicalX87ControlMatch(64,2,64,bad,ref count);
                CanonicalX87ControlEqual(72,0x3fff,16,bad,ref count);
                CanonicalX87ControlEqual(74,0,16,bad,ref count);CanonicalX87ControlEqual(76,0,32,bad,ref count);
            }
            else {CanonicalX87ControlEqual(64,0,64,bad,ref count);CanonicalX87ControlEqual(72,0,64,bad,ref count);}
            CanonicalX87WorkLoad(300,2,16);CanonicalX87ControlMatch(80,2,16,bad,ref count);
            CanonicalX87ControlMatch(82,2,16,bad,ref count);
            IntegerMov(1,2,32);IntegerConstant(4,1,0x200);CanonicalX87ControlMatch(232,1,16,bad,ref count);
            CanonicalX87ControlEqual(234,trig?0x600u:0x200u,16,bad,ref count);
            IntegerImmediate(8,(uint)op-0xa000|0x10100|(pair?0x200u:0),32);
            IntegerConstant(4,2,32);int exact=NearCondition(0x84);
            CanonicalX87WorkLoad(8,0,16);IntegerConstant(4,0,32);int masked=NearCondition(0x85);
            IntegerConstant(1,8,0x1000);Patch(exact,position);Patch(masked,position);
            // Exact equality excludes HostApproximation/CapturedActual, suppress,
            // range, undefined-profile, extraneous kind and every reserved bit.
            CanonicalX87ControlMatch(84,8,32,bad,ref count);
            for(int off=96;off<232;off+=8)CanonicalX87ControlEqual(off,0,64,bad,ref count);
            for(int off=236;off<256;off+=4)CanonicalX87ControlEqual(off,0,32,bad,ref count);
            for(int off=288;off<320;off+=4)CanonicalX87WorkConstant(off,0);
        }
        private void CanonicalX87IntegerInputMatch(int logical,int input,scoped Span<int> bad,ref int count)
        {
            CanonicalX87Address(logical);IntegerMov(8,0);
            Bytes(0x49,0x8b,0x10); // RDX=qword[R8], raw significand
            CanonicalX87ControlMatch(input,2,64,bad,ref count);
            Bytes(0x41,0x0f,0xb7,0x50,0x08); // EDX=word[R8+8], sign/exponent
            CanonicalX87ControlMatch(input+8,2,16,bad,ref count);
        }
        private void CanonicalX87IntegerZero(int offset,scoped Span<int> bad,ref int count)
        {
            CanonicalX87ControlEqual(offset,0,64,bad,ref count);
            CanonicalX87WorkLoad(offset+8,0,16);IntegerConstant(4,0,0x7fff);bad[count++]=NearCondition(0x85);
        }
        private void CanonicalX87IntegerZeroOrNormal(int offset,scoped Span<int> bad,ref int count)
        {
            CanonicalX87WorkLoad(offset+8,0,16);IntegerConstant(4,0,0x7fff);int zero=NearCondition(0x84);
            IntegerConstant(7,0,0x7fff);bad[count++]=NearCondition(0x83);
            CanonicalX87WorkLoad(offset,0);IntegerRR(0x85,0,0);bad[count++]=NearCondition(0x89);
            int normal=IntegerJump();Patch(zero,position);CanonicalX87ControlEqual(offset,0,64,bad,ref count);Patch(normal,position);
        }

        // Verify k*y with a separate 32+32-bit decomposition, NOT the kernel's
        // MUL r64/BSR/SHRD recipe. Product is accumulated exactly in RDX:RAX;
        // repeated one-bit division extracts guard/sticky before RC rounding.
        private void CanonicalX87IntegerLogProduct()
        {
            CanonicalX87WorkLoad(24,0,16);IntegerConstant(5,0,0x3fff);
            IntegerMov(1,0,32);IntegerShift(7,1,32,31);
            IntegerRR(0x31,0,1,32);IntegerRR(0x29,0,1,32);IntegerMov(8,0,32);
            IntegerConstant(4,1,0x8000);CanonicalX87WorkLoad(40,0,16);IntegerConstant(4,0,0x8000);
            IntegerRR(0x31,1,0,32);CanonicalX87WorkStore(304,1,32); // expected sign
            CanonicalX87WorkLoad(40,0,16);IntegerConstant(4,0,0x7fff);CanonicalX87WorkStore(308,0,32);
            CanonicalX87WorkLoad(32,0,32); // low32*y factor, fits 46 bits
            Bytes(0x49,0x0f,0xaf,0xc0); // imul rax,r8
            CanonicalX87WorkLoad(36,2,32);Bytes(0x49,0x0f,0xaf,0xd0); // imul rdx,r8
            IntegerMov(1,2);IntegerShift(5,2,64,32);IntegerShift(4,1,64,32);
            IntegerRR(0x01,0,1);IntegerConstant(2,2,0,64); // ADC high,0; carry before shift
            IntegerImmediate(8,0,32);CanonicalX87WorkConstant(312,0);
            int loop=position;
            IntegerRR(0x85,2,2);int normalized=NearCondition(0x84);
            // R8 keeps previous discarded guard/sticky as: bit1=guard, bit0=sticky.
            IntegerMov(1,8,32);IntegerShift(5,1,32,1);IntegerRR(0x09,8,1,32);IntegerConstant(4,8,1);
            IntegerMov(1,0,32);IntegerConstant(4,1,1);IntegerShift(4,1,32,1);IntegerRR(0x09,8,1,32);
            IntegerShift(5,2,64,1);IntegerShift(3,0,64,1); // SHR high then RCR low
            CanonicalX87WorkLoad(308,1,32);IntegerConstant(0,1,1);CanonicalX87WorkStore(308,1,32);
            int repeat=IntegerJump();Patch(repeat,loop);
            Patch(normalized,position);IntegerRR(0x85,8,8,32);int exact=NearCondition(0x84);
            CanonicalX87WorkConstant(300,0x20);
            CanonicalX87WorkLoad(8,1,16);IntegerShift(5,1,32,10);IntegerConstant(4,1,3);
            IntegerRR(0x85,1,1,32);int nearest=NearCondition(0x84);
            IntegerConstant(7,1,3);int truncate=NearCondition(0x84);
            CanonicalX87WorkLoad(304,2,32);IntegerShift(5,2,32,15);IntegerRR(0x01,1,2,32);
            IntegerConstant(7,1,2);int directedDown=NearCondition(0x85);int inc=IntegerJump();
            Patch(nearest,position);IntegerConstant(7,8,2);int belowHalf=NearCondition(0x82);int aboveHalf=NearCondition(0x87);
            IntegerMov(1,0,32);IntegerConstant(4,1,1);int even=NearCondition(0x84);
            Patch(inc,position);Patch(aboveHalf,position);
            IntegerConstant(0,0,1,64);int noCarry=NearCondition(0x85);
            IntegerImmediate(0,0x8000000000000000UL);CanonicalX87WorkLoad(308,1,32);IntegerConstant(0,1,1);CanonicalX87WorkStore(308,1,32);
            Patch(noCarry,position);CanonicalX87WorkConstant(300,0x220);
            Patch(truncate,position);Patch(directedDown,position);Patch(belowHalf,position);Patch(even,position);Patch(exact,position);
            CanonicalX87WorkStore(288,0);CanonicalX87WorkLoad(308,0,32);CanonicalX87WorkLoad(304,1,32);
            IntegerRR(0x09,0,1,32);CanonicalX87WorkStore(296,0,32);
        }
    }
}
