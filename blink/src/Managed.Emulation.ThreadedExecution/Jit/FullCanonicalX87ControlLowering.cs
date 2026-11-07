using System;

namespace Managed.Emulation.Execution;

// Compile-time identities. No runtime selector, pop modifier or managed owner.
internal enum FullCanonicalX87ControlOperation : ushort
{
    Fcom = 0x9001, Fcomp, Fcompp, Fucom, Fucomp, Fucompp,
    Fcomi, Fcomip, Fucomi, Fucomip,
    Fcmovb, Fcmove, Fcmovbe, Fcmovu, Fcmovnb, Fcmovne, Fcmovnbe, Fcmovnu,
    Ftst, Fxam, Frndint, Fsqrt, Fxtract,
    Fscale, Fprem, Fprem1, Fldcw, Fnstcw, Fldenv, Fnstenv, Frstor, Fnsave, Fxsave, Fxrstor, Fnstsw
}

internal static class FullCanonicalX87ControlLowering
{
    internal static bool TryClassify(ulong ip, ReadOnlySpan<byte> bytes,
        out JitInstruction instruction, out string? reason)
    {
        if (FullCanonicalX87StackLowering.TryClassify(ip,bytes,out instruction,out reason) &&
            instruction.Options != (uint)FullCanonicalX87StackOperation.Fwait) return true;
        if (TryClassifyState(ip,bytes,out instruction,out reason,out bool handled)) return true;
        if (handled) return false;
        instruction = default; reason = "CanonicalX87ControlEncodingMISSING";
        if (bytes.Length is < 2 or > 15) return false;
        int p = 0;
        while (p < bytes.Length)
        {
            byte b = bytes[p];
            if (b == 0xf0) { reason = "CanonicalX87ControlLockInvalid"; return false; }
            if (b is 0xf2 or 0xf3) { reason = "CanonicalX87ControlRepPrefixPolicyMISSING"; return false; }
            if (b is 0x66 or 0x67 or 0x26 or 0x2e or 0x36 or 0x3e or 0x64 or 0x65 or >= 0x40 and <= 0x4f) ++p;
            else break;
        }
        if (p + 2 != bytes.Length) return false;
        byte opcode = bytes[p], m = bytes[p + 1];
        if (m < 0xc0) { reason = "CanonicalX87MemoryComparisonMISSING"; return false; }
        FullCanonicalX87ControlOperation op;
        int n = m & 7;
        if (opcode == 0xd8 && m is >= 0xd0 and <= 0xdf)
            op = m < 0xd8 ? FullCanonicalX87ControlOperation.Fcom : FullCanonicalX87ControlOperation.Fcomp;
        else if (opcode == 0xde && m == 0xd9) { op = FullCanonicalX87ControlOperation.Fcompp; n = 1; }
        else if (opcode == 0xdd && m is >= 0xe0 and <= 0xef)
            op = m < 0xe8 ? FullCanonicalX87ControlOperation.Fucom : FullCanonicalX87ControlOperation.Fucomp;
        else if (opcode == 0xda && m == 0xe9) { op = FullCanonicalX87ControlOperation.Fucompp; n = 1; }
        else if (opcode is 0xdb or 0xdf && m is >= 0xe8 and <= 0xf7)
            op = (opcode, m < 0xf0) switch
            {
                (0xdb, true) => FullCanonicalX87ControlOperation.Fucomi,
                (0xdb, false) => FullCanonicalX87ControlOperation.Fcomi,
                (0xdf, true) => FullCanonicalX87ControlOperation.Fucomip,
                _ => FullCanonicalX87ControlOperation.Fcomip
            };
        else if (opcode is 0xda or 0xdb && m <= 0xdf)
            op = (FullCanonicalX87ControlOperation)(0x900b + (m - 0xc0) / 8 + (opcode == 0xdb ? 4 : 0));
        else if (opcode == 0xd9)
        {
            n = 0;
            op = m switch
            {
                0xe4 => FullCanonicalX87ControlOperation.Ftst,
                0xe5 => FullCanonicalX87ControlOperation.Fxam,
                0xfc => FullCanonicalX87ControlOperation.Frndint,
                0xfa => FullCanonicalX87ControlOperation.Fsqrt,
                0xf4 => FullCanonicalX87ControlOperation.Fxtract,
                0xfd => FullCanonicalX87ControlOperation.Fscale,
                0xf8 => FullCanonicalX87ControlOperation.Fprem,
                0xf5 => FullCanonicalX87ControlOperation.Fprem1,
                _ => 0
            };
            if (op == 0)
            {
                reason = m switch
                {
                    0xf0 => "CanonicalX87F2xm1NativeAccuracyMISSING",
                    0xf1 => "CanonicalX87Fyl2xNativeAccuracyCompletionMISSING",
                    0xf2 => "CanonicalX87FptanNativeAccuracyPushMISSING",
                    0xf3 => "CanonicalX87FpatanNativeAccuracyCompletionMISSING",
                    0xf9 => "CanonicalX87Fyl2xp1NativeAccuracyCompletionMISSING",
                    0xfb => "CanonicalX87FsincosNativeAccuracyPushMISSING",
                    0xfe => "CanonicalX87FsinNativeAccuracyMISSING",
                    0xff => "CanonicalX87FcosNativeAccuracyMISSING",
                    _ => "CanonicalX87ControlEncodingMISSING"
                };
                return false;
            }
            if (op is FullCanonicalX87ControlOperation.Fscale or FullCanonicalX87ControlOperation.Fprem or FullCanonicalX87ControlOperation.Fprem1) n = 1;
        }
        else return false; // No DC/DE/DF historical undocumented aliases.
        instruction = new(ip, bytes.Length, JitOperation.X87, 80, default, new(n, 0),
            Extra: new(0, (ulong)((opcode & 7) << 8 | m), IsImmediate: true), Options: (uint)op);
        reason = null; return true;
    }

    internal const uint StatePointers64 = 1u << 16, StateEnvironment16 = 1u << 17;
    internal static bool StateLoad(FullCanonicalX87ControlOperation op) =>
        op is FullCanonicalX87ControlOperation.Fldcw or FullCanonicalX87ControlOperation.Fldenv or
        FullCanonicalX87ControlOperation.Frstor or FullCanonicalX87ControlOperation.Fxrstor;
    internal static bool StateOperation(byte opcode,byte modrm,out FullCanonicalX87ControlOperation op)
    {
        op = 0; if (modrm >= 0xc0) return false;
        op = (opcode,modrm>>3&7) switch
        {
            (0xd9,5) => FullCanonicalX87ControlOperation.Fldcw,
            (0xd9,7) => FullCanonicalX87ControlOperation.Fnstcw,
            (0xd9,4) => FullCanonicalX87ControlOperation.Fldenv,
            (0xd9,6) => FullCanonicalX87ControlOperation.Fnstenv,
            (0xdd,4) => FullCanonicalX87ControlOperation.Frstor,
            (0xdd,6) => FullCanonicalX87ControlOperation.Fnsave,
            (0xdd,7) => FullCanonicalX87ControlOperation.Fnstsw,
            (0x0f,0) => FullCanonicalX87ControlOperation.Fxsave,
            (0x0f,1) => FullCanonicalX87ControlOperation.Fxrstor,
            _ => 0
        };
        return op != 0;
    }
    internal static int StateBytes(FullCanonicalX87ControlOperation op,bool small) => op switch
    {
        FullCanonicalX87ControlOperation.Fldcw or FullCanonicalX87ControlOperation.Fnstcw or
            FullCanonicalX87ControlOperation.Fnstsw => 2,
        FullCanonicalX87ControlOperation.Fldenv or FullCanonicalX87ControlOperation.Fnstenv => small?14:28,
        FullCanonicalX87ControlOperation.Frstor or FullCanonicalX87ControlOperation.Fnsave => small?94:108,
        // Architectural permission span is512. This width describes only the
        // defined416 data effects; a mandatory512-byte probe precedes mapping.
        FullCanonicalX87ControlOperation.Fxsave or FullCanonicalX87ControlOperation.Fxrstor => 416,
        _ => 0
    };
    private static bool TryClassifyState(ulong ip,ReadOnlySpan<byte> bytes,out JitInstruction i,out string? reason,out bool handled)
    {
        i = default; reason = null; handled = false;
        if (bytes.Length is < 2 or > 15) return false;
        int p = 0,segment = -1; bool as32 = false,small = false; byte rex = 0;
        while (p < bytes.Length)
        {
            byte b = bytes[p];
            if (b is >= 0x40 and <= 0x4f) { rex = b; ++p; continue; }
            // AMD24592 6.6.0.2 prescribes single F2/F3 as ignored. Multiple
            // same-group prefixes keep this decoder's finite scan policy only;
            // 3.5.1 makes those aliases undefined, not required ordinary forms.
            if (b is 0xf2 or 0xf3) { rex = 0; ++p; continue; }
            if (b == 0x66) small = true;
            else if (b == 0x67) as32 = true;
            else if (b is 0x26 or 0x2e or 0x36 or 0x3e or 0x64 or 0x65)
                segment = b switch { 0x26 => 0,0x2e => 1,0x36 => 2,0x3e => 3,0x64 => 4,_ => 5 };
            else break;
            rex = 0; ++p;
        }
        if (p+2 > bytes.Length) return false;
        byte opcode = bytes[p++]; bool fx = opcode == 0x0f;
        if (fx && bytes[p++] != 0xae) return false;
        if (p >= bytes.Length) return false;
        byte modrm = bytes[p++];
        // Documented register state controls share the existing raw stack
        // bodies; only their ignored-prefix decode gap is closed here.
        if (!fx && p == bytes.Length && (opcode,modrm) is (0xdb,0xe2) or (0xdb,0xe3) or (0xdf,0xe0))
        {
            var control = opcode == 0xdf ? FullCanonicalX87StackOperation.FnstswAx :
                modrm == 0xe2 ? FullCanonicalX87StackOperation.Fnclex : FullCanonicalX87StackOperation.Fninit;
            handled = true;
            i = new(ip,bytes.Length,JitOperation.X87,80,default,new(0,0),
                Extra:new(0,(uint)((opcode&7)<<8|modrm),IsImmediate:true),Options:(uint)control);
            return true;
        }
        if (!StateOperation(opcode,modrm,out var op)) return false;
        handled = true;
        // Effective REX.W selects the 28/108-byte format even after 66h.
        // A legacy prefix following REX already canceled that REX above.
        small &= (rex & 8) == 0;
        // Protected16 restore uses the authorized Linux x86-64 software
        // metadata profile: omitted FOP clears to0. Current AMD legacy-image
        // witness agrees; this is not an Intel prescription/undefined license.
        if (!FullMemoryLowering.Address(bytes,ref p,modrm,rex,as32,segment,out var address) || p != bytes.Length)
        { reason = "CanonicalX87ControlStateExactAddressMISSING"; return false; }
        bool smallEnv = small && op is FullCanonicalX87ControlOperation.Fldenv or FullCanonicalX87ControlOperation.Fnstenv or
            FullCanonicalX87ControlOperation.Frstor or FullCanonicalX87ControlOperation.Fnsave;
        uint options = (uint)op | (smallEnv?StateEnvironment16:0) | (fx && (rex&8) != 0?StatePointers64:0);
        var memory = new JitOperand(0,0,IsMemory:true); bool load = StateLoad(op);
        i = new(ip,bytes.Length,JitOperation.X87,80,load?default:memory,load?memory:default,
            SourceWidth:StateBytes(op,smallEnv)*8,Address:address,
            Extra:new(0,fx?modrm:(uint)((opcode&7)<<8|modrm),IsImmediate:true),Options:options);
        return true;
    }
}
