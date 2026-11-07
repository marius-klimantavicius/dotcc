using System;
using System.Buffers.Binary;

namespace Managed.Emulation.Execution;

// R157 retained UNQUALIFIED private host profile + R165 partial integer paths.
// Integer utilities derive from our own R154/R126 code. No QEMU/libm/helper.
// Work384 unchanged; R165 edits AUTHORED_UNRUN, c95 finite receipts retained.
internal static partial class FullExtended80TranscendentalEmitter
{
    internal const int CodeCapacity=16384;
    internal static bool TryEmit(FullExtended80Transcendental op,Span<byte> destination,out int length,bool? windowsAbi=null)
        => Emit(op,destination,out length,windowsAbi??OperatingSystem.IsWindows(),false);
    internal static bool TryEmitContinuation(FullExtended80Transcendental op,Span<byte> destination,out int length)
        => Emit(op,destination,out length,false,true);
    private static bool Emit(FullExtended80Transcendental op,Span<byte> destination,out int length,bool windows,bool continuation)
    {
        length=0; if ((uint)op>7) return false;
        Span<int> labels=stackalloc int[256],heads=stackalloc int[256];labels.Fill(-1);heads.Fill(-1);
        var w=new Writer(destination[..Math.Min(destination.Length,CodeCapacity)],labels,heads);
        w.Transcendental(op,windows,continuation);
        if(!w.Complete)return false;length=w.Length;return true;
    }
    private ref partial struct Writer
    {
        private Span<byte> code;
        private Span<int> labels, heads;
        private int next, n;
        private bool failed;
        internal int Length => n;
        internal bool Complete
        {
            get
            {
                if (failed) return false;
                for (int i = 0; i < next; ++i) if (heads[i] >= 0) return false;
                return true;
            }
        }
        internal Writer(Span<byte> bytes, Span<int> positions, Span<int> fixups)
        { code = bytes; labels = positions; heads = fixups; next = n = 0; failed = false; }
        private void B(int b)
        { if (n < code.Length) code[n++] = (byte)b; else failed = true; }
        private void D(int value)
        {
            if (code.Length - n < 4) { failed = true; return; }
            BinaryPrimitives.WriteInt32LittleEndian(code[n..], value); n += 4;
        }
        private void Q(ulong value) { D(unchecked((int)value)); D(unchecked((int)(value >> 32))); }
        private int L()
        { if (next >= labels.Length) { failed = true; return 0; } return next++; }
        private void Bind(int label)
        {
            if (failed) return;
            if (labels[label] >= 0) { failed = true; return; }
            labels[label] = n;
            int p = heads[label]; heads[label] = -1;
            while (p >= 0)
            {
                int previous = BinaryPrimitives.ReadInt32LittleEndian(code[p..]);
                BinaryPrimitives.WriteInt32LittleEndian(code[p..], n - p - 4); p = previous;
            }
        }
        // cc uses the low nibble of 0F 8x; -1 is unconditional E9.
        private void J(int label, int cc = -1)
        {
            if (failed) return;
            if (cc < 0) B(0xe9); else { B(0x0f); B(0x80 | cc); }
            int p = n; D(labels[label] >= 0 ? labels[label] - p - 4 : heads[label]);
            if (!failed && labels[label] < 0) heads[label] = p;
        }
        private void Rex(bool wide, int reg = 0, int rm = 0)
        { int r = 0x40 | (wide ? 8 : 0) | (reg >= 8 ? 4 : 0) | (rm >= 8 ? 1 : 0); if (r != 0x40) B(r); }
        private void RR(int opcode, int dst, int src, bool wide = true)
        { Rex(wide, src, dst); B(opcode); B(0xc0 | ((src & 7) << 3) | (dst & 7)); }
        private void Mov(int dst, int src, bool wide = true) => RR(0x89, dst, src, wide);
        private void Imm(int reg, ulong value, bool wide = true)
        { Rex(wide, 0, reg); B(0xb8 | (reg & 7)); if (wide) Q(value); else D(unchecked((int)value)); }
        private void C(int reg, int value) => Imm(reg, unchecked((uint)value), false);
        private void Mem(int reg, int offset)
        { B(0x80 | ((reg & 7) << 3) | 3); D(offset); } // [r11+disp32]
        private void Load(int reg, int offset, int width = 64)
        {
            Rex(width == 64, reg, 11);
            if (width == 16) { B(0x0f); B(0xb7); } else B(0x8b);
            Mem(reg, offset);
        }
        private void Store(int reg, int offset, int width = 64)
        { if (width == 16) B(0x66); Rex(width == 64, reg, 11); B(0x89); Mem(reg, offset); }
        private void Put(int offset, int value)
        { C(0, value); Store(0, offset, 32); }
        private void Alu(int group, int reg, int value, bool wide = true)
        { Rex(wide, 0, reg); B(0x81); B(0xc0 | (group << 3) | (reg & 7)); D(value); }
        private void Bits(int offset, int group, int value)
        { Rex(false, 0, 11); B(0x81); Mem(group, offset); D(value); }
        private void Test(int reg, bool wide = true) => RR(0x85, reg, reg, wide);
        private void Shift(int group, int reg, int count = -1, bool wide = true)
        { Rex(wide, 0, reg); B(count < 0 ? 0xd3 : 0xc1); B(0xc0 | (group << 3) | (reg & 7)); if (count >= 0) B(count); }
        private void DoubleShift(bool right, int dst, int src)
        { Rex(true, src, dst); B(0x0f); B(right ? 0xad : 0xa5); B(0xc0 | ((src & 7) << 3) | (dst & 7)); }
        private void Bsr(int dst, int src)
        { Rex(true, dst, src); B(0x0f); B(0xbd); B(0xc0 | ((dst & 7) << 3) | (src & 7)); }
        private void Set(int reg, int cc)
        { Rex(false, 0, reg); B(0x0f); B(0x90 | cc); B(0xc0 | (reg & 7)); }
        private void Exit(bool continuation)
        {
            Load(0, 12, 32);
            if (continuation) { B(0x41); B(0xff); B(0xa3); D(88); } else B(0xc3);
        }


        private static bool Binary(FullExtended80Transcendental op)=>op is FullExtended80Transcendental.Fyl2x or FullExtended80Transcendental.Fyl2xp1 or FullExtended80Transcendental.Fpatan;
        private static bool Pair(FullExtended80Transcendental op)=>op is FullExtended80Transcendental.Fptan or FullExtended80Transcendental.Fsincos;
        private static bool Trig(FullExtended80Transcendental op)=>op is FullExtended80Transcendental.Fptan or FullExtended80Transcendental.Fsincos or FullExtended80Transcendental.Fsin or FullExtended80Transcendental.Fcos;

        internal void Transcendental(FullExtended80Transcendental op,bool windows,bool continuation)
        {
            int badVersion=L(),badSize=L(),range=L(),compute=L(),finish=L(),exit=L(),undefined=L(),badHost=L();
            B(0xf3);B(0x0f);B(0x1e);B(0xfa);if(!continuation)Mov(11,windows?1:7);
            C(0,0);Store(0,12,32);for(int p=48;p<88;p+=8)Store(0,p);
            Load(0,0,32);Alu(7,0,1,false);J(badVersion,5);
            Load(0,4,32);Alu(7,0,384,false);J(badSize,5);
            Put(232,0x02000000);Put(236,0);Put(240,0);Put(244,0);Put(216,0);Put(248,0);Put(252,0);
            Decode(16,128,144,152,160,168);
            if(Binary(op))Decode(32,136,148,156,164,172);else Put(164,0);
            // Invalid/NaN classification precedes undefined/range tests and DE.
            // Private host computation uses only fully masked CW, even if the
            // guest requested a pre-operation exception or exponent wrapping.
            Load(0,160,32);Alu(7,0,3,false);J(compute,4);Alu(7,0,4,false);J(compute,4);Alu(7,0,5,false);J(compute,4);
            if(Binary(op)){Load(0,164,32);Alu(7,0,3,false);J(compute,4);Alu(7,0,4,false);J(compute,4);Alu(7,0,5,false);J(compute,4);}
            if(Trig(op))
            {
                // Infinities are invalid operands, not finite C2 range misses.
                Load(0,160,32);Alu(7,0,2,false);J(compute,4);
                Load(0,144,32);Alu(7,0,63,false);J(range,13);
            }
            if(op==FullExtended80Transcendental.F2xm1)
            {
                int domainOk=L();Load(0,160,32);Alu(7,0,2,false);C(8,1);J(undefined,4);
                Load(0,144,32);Alu(7,0,0,false);J(domainOk,12);J(undefined,15);
                Load(0,128);Imm(1,0x8000000000000000);RR(0x39,0,1);J(undefined,5);Bind(domainOk);
            }
            if(op==FullExtended80Transcendental.Fyl2xp1)
            {
                int domainOk=L();C(8,2);Load(0,160,32);Alu(7,0,2,false);J(undefined,4);
                Load(0,144,32);Alu(7,0,-2,false);J(domainOk,12);J(undefined,15);
                // Asymmetric domain: [-1+sqrt(2)/2, sqrt(2)-1].
                // Negative magnitude floor(2^65-sqrt(2^129))=95f619980c4336f7;
                // positive floor(sqrt(2^131)-2^65)=d413cccfe7799211.
                // Both endpoint floors have independent integer-square proofs.
                Load(0,152,32);Test(0,false);int negativeEndpoint=L(),compareEndpoint=L();J(negativeEndpoint,5);
                Imm(1,0xd413cccfe7799211);J(compareEndpoint);
                Bind(negativeEndpoint);Imm(1,0x95f619980c4336f7);Bind(compareEndpoint);
                Load(0,128);RR(0x39,0,1);J(undefined,7);Bind(domainOk);
            }
            Bind(compute);IntegerNative(op,exit);PrivateHost(op);
            // The private environment and exact RSP are already restored.
            // No exit/continuation can observe the temporary host state.
            NormalizeStatus(op,finish,badHost);
            Bind(finish);J(exit);
            Bind(range);Put(84,(int)op+1|0x800);Put(232,0x04000400);J(exit);
            Bind(undefined);
            // Explicit software profile for architecturally undefined inputs:
            // F2XM1 copies x; FYL2XP1 copies y and has its usual pop. No numeric
            // exceptions; clear C1, retain undefined C0/C2/C3. NaNs/unsupported
            // took the documented numeric-class path before this predicate.
            Load(0,op==FullExtended80Transcendental.F2xm1?16:32);Store(0,48);
            Load(0,op==FullExtended80Transcendental.F2xm1?24:40,16);Store(0,56,16);
            Put(84,(int)op+1|0x100|0x8000);J(exit);
            Bind(badHost);
            // A host violating the captured logical-TOP completion invariant
            // is outside the selected modern x64 native-NE host profile.
            Put(236,5);C(0,0);for(int p=48;p<88;p+=8)Store(0,p);Store(0,232,32);
            Put(12,257);J(exit);
            Bind(badVersion);Put(12,1);J(exit);Bind(badSize);Put(12,2);
            Bind(exit);Exit(continuation);
        }

        private void X87Memory(int opcode,int extension,int offset)
        {B(0x41);B(opcode);Mem(extension,offset);}
        private void PrivateHost(FullExtended80Transcendental op)
        {
            // Up to527 bytes below original RSP: align down15 then512. Neither
            // SysV red zone nor Win64 home space is used as persistent storage.
            // UNQUALIFIED: FX64 pointer save/restore may omit ES0 FIP/FDP/FOP;
            // abridged FTW also cannot prove preservation of forged full tags.
            // This retained numeric path does NOT clear HostStateBoundaryBlocker.
            // FXSAVE/FNINIT/FXRSTOR are nonwaiting; no original pending #MF is
            // consumed. FNINIT clears the private flags/tags before FLDCW.
            SaveHost();
            B(0xdb);B(0xe3); // fninit
            Load(0,8,16);Alu(4,0,0x0c00,false);Alu(1,0,0x037f,false);Store(0,208,16);
            X87Memory(0xd9,5,208); // fldcw: ALL exceptions masked, PC64, guest RC
            if(Binary(op))X87Memory(0xdb,5,32); // fld tbyte y, then x
            X87Memory(0xdb,5,16);
            SelectedOpcode(op);
            X87Memory(0xdd,7,240); // fnstsw m16 BEFORE FSTP changes C1
            if(Pair(op))X87Memory(0xdb,7,64); // newly pushed one/cosine
            X87Memory(0xdb,7,48); // old ST0 replacement or popped binary result
            // No branch/CALL/RET while private env active. Masked host stack has
            // two operands at most and enough room for the documented push.
            RestoreHost();
        }
        private void SelectedOpcode(FullExtended80Transcendental op)
        {
            B(0xd9);B(op switch
            {
                FullExtended80Transcendental.F2xm1=>0xf0,FullExtended80Transcendental.Fyl2x=>0xf1,
                FullExtended80Transcendental.Fptan=>0xf2,FullExtended80Transcendental.Fpatan=>0xf3,
                FullExtended80Transcendental.Fyl2xp1=>0xf9,FullExtended80Transcendental.Fsincos=>0xfb,
                FullExtended80Transcendental.Fsin=>0xfe,_=>0xff
            });
        }
        private void SaveHost()
        {
            Mov(0,4);Store(0,248);Alu(4,4,-16);Alu(5,4,512);
            B(0x48);B(0x0f);B(0xae);B(0x04);B(0x24); // fxsave64 [rsp]
        }
        private void RestoreHost()
        {
            B(0x48);B(0x0f);B(0xae);B(0x0c);B(0x24); // fxrstor64 [rsp]
            Load(0,248);Mov(4,0);
            Put(248,0);Put(252,0); // no host pointer escape
        }
        private void CaptureActual(FullExtended80Transcendental op,int badHost)
        {
            // Entry: integer IE/DE/ZE priority already handled. Normal masked
            // candidates/status are staged48/64/80 and remain safe across capture.
            SaveHost();B(0xdb);B(0xe3); // FNINIT: clean private tags/events
            Load(0,8,16);Alu(4,0,0xc00,false);Alu(1,0,0x037f,false);Store(0,208,16);
            X87Memory(0xd9,5,208);
            if(Binary(op))X87Memory(0xdb,5,32);X87Memory(0xdb,5,16);
            B(0xdb);B(0xe2); // FNCLEX: BEFORE installing any unmasked bit
            // Only guest OE/UE/PE masks may unmask. IE/DE/ZE stay masked because
            // integer priority was resolved above. PC64 and guest RC remain.
            Load(0,8,16);Alu(4,0,0xc38,false);Alu(1,0,0x0347,false);Store(0,208,16);
            X87Memory(0xd9,5,208);
            SelectedOpcode(op);
            X87Memory(0xdd,6,96); // IMMEDIATE literal FNSAVE108, NO WAIT/FSTP
            // FNSAVE both captures completed private values and clears pending
            // #MF. Only full FX64 restores the ORIGINAL host state.
            RestoreHost();
            Load(0,100,16);Mov(1,0,false);Shift(5,1,11,false);Alu(4,1,7,false);
            Store(0,216,32); // non-pointer captured private SW for validation
            Alu(4,0,0x023f,false);Mov(2,0,false);
            // Retain explicitly selected pseudo-denormal DE policy, but IE
            // always preempts it. Original diagnostic flags were staged80.
            Load(0,80,16);Alu(4,0,3,false);RR(0x09,2,0,false);
            Mov(0,2,false);Alu(4,0,1,false);int notInvalid=L();J(notInvalid,4);C(2,1);Bind(notInvalid);
            Store(2,82,16);Put(244,1);
            // FNSAVE stores logical ST0..7 at +28 in ten-byte dense images.
            // After completed binary pop ST0=result. Pair ST0=pushed one/cos,
            // ST1=old ST0 replacement. Capture copies use integer loads only.
            CopyCaptured(Pair(op)?134:124,48);
            if(Pair(op))CopyCaptured(124,64);
            // FNSAVE includes private FIP/FDP32 and raw empty register payloads
            // that FNINIT does not erase. Sanitize the ENTIRE108-byte image plus
            // padding BEFORE any refusal or exit; no host pointer/data escape.
            C(0,0);for(int p=96;p<208;p+=8)Store(0,p);
            Alu(7,1,Pair(op)?6:7,false);J(badHost,5);
            Bits(84,1,0x4000);
        }
        private void CopyCaptured(int input,int output)
        {Load(0,input);Store(0,output);Load(0,input+8,16);Store(0,output+8,16);}

        private void NormalizeStatus(FullExtended80Transcendental op,int finish,int badHost)
        {
            int invalid=L(),notInvalid=L(),nan=L(),noNan=L(),denormal=L(),noDenormal=L();
            int suppress=L(),usable=L(),noPrecisionTrap=L();
            Load(2,240,16);Alu(4,2,0x023f,false);
            // Unsupported and sNaN must raise IE even if a host profile differs;
            // special values are otherwise the selected host ISA profile.
            Load(0,160,32);Alu(7,0,4,false);J(invalid,4);Alu(7,0,5,false);J(invalid,4);
            if(Binary(op)){Load(0,164,32);Alu(7,0,4,false);J(invalid,4);Alu(7,0,5,false);J(invalid,4);}
            Mov(0,2,false);Alu(4,0,1,false);Test(0,false);J(notInvalid,4);
            Bind(invalid);C(2,1);J(noDenormal);
            Bind(notInvalid);
            Load(0,160,32);Alu(7,0,3,false);J(nan,4);
            if(Binary(op)){Load(0,164,32);Alu(7,0,3,false);J(nan,4);}
            J(noNan);Bind(nan);C(2,0);J(noDenormal);Bind(noNan);
            Load(0,160,32);Alu(7,0,6,false);J(denormal,4);
            if(Binary(op)){Load(0,164,32);Alu(7,0,6,false);J(denormal,4);}
            J(noDenormal);Bind(denormal);Alu(1,2,2,false);Bind(noDenormal);
            Store(2,80,16);
            Load(1,8,16);Mov(0,1,false);Alu(6,0,-1,false);RR(0x21,0,2,false);Alu(4,0,7,false);
            Test(0,false);J(usable,4);
            // Priority: IE, DE, ZE. The lowest set bit is the first event.
            Mov(1,0,false);Alu(5,1,1,false);RR(0x21,1,0,false);RR(0x31,0,1,false);
            Mov(2,0,false);J(suppress);
            Bind(usable);
            Store(2,82,16);Put(84,(int)op+1|0x100|0x2000|(Pair(op)?0x200:0));
            // Recompute for ANY unmasked post-operation event, not merely when
            // the masked host UE/OE flags happen to be set. This covers exact
            // tiny and rounded-to-min-normal boundaries without guessing.
            Load(0,8,16);Alu(4,0,0x38,false);Alu(7,0,0x38,false);int allPostMasked=L();J(allPostMasked,4);
            CaptureActual(op,badHost);Load(2,82,16);Bind(allPostMasked);
            Mov(0,2,false);Alu(4,0,32,false);Test(0,false);J(noPrecisionTrap,4);
            Load(0,8,16);Alu(4,0,32,false);Test(0,false);J(noPrecisionTrap,5);
            Bits(84,1,0x1000);Bind(noPrecisionTrap);
            Mov(0,2,false);Alu(4,0,0x200,false);Store(0,232,16);
            C(0,Trig(op)?0x600:0x200);Store(0,234,16);J(finish);
            Bind(suppress);Store(2,82,16);Put(84,(int)op+1|0x400|0x2000);
            C(0,0);for(int p=48;p<80;p+=8)Store(0,p);Put(232,0x02000000);J(finish);
        }
        private void Decode(int input, int sig, int exp, int sign, int cls, int rawSe)
        {
            int eZero = L(), special = L(), unsupported = L(), finish = L(), denormal = L();
            Load(9, input); Store(9, sig); Load(0, input + 8, 16); Store(0, rawSe, 32);
            Mov(2, 0, false); Shift(5, 2, 15, false); Store(2, sign, 32);
            Alu(4, 0, 0x7fff, false); Store(0, exp, 32);
            Test(0, false); J(eZero, 4); Alu(7, 0, 0x7fff, false); J(special, 4);
            Test(9); J(unsupported, 9); // JNS means explicit integer bit is zero.
            Alu(5, 0, 16383, false); Store(0, exp, 32); Put(cls, 1); J(finish);
            Bind(eZero); Test(9); J(denormal, 5); Put(cls, 0); Put(exp, -16382); J(finish);
            Bind(denormal);
            Bsr(1, 9); C(2, 63); RR(0x29, 2, 1, false); Mov(1, 2, false);
            Shift(4, 9); Store(9, sig); C(0, -16382); RR(0x29, 0, 2, false);
            Store(0, exp, 32); Put(cls, 6); J(finish);
            Bind(special); Test(9); J(unsupported, 9);
            Imm(0, 0x8000000000000000); RR(0x39, 9, 0); int notInf = L(); J(notInf, 5);
            Put(cls, 2); J(finish);
            Bind(notInf); Mov(0, 9); Shift(5, 0, 62); Alu(4, 0, 1, false);
            C(2, 4); RR(0x29, 2, 0, false); Store(2, cls, 32); J(finish);
            Bind(unsupported); Put(cls, 5); Bind(finish);
        }

    }
}
