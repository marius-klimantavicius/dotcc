using System.Buffers.Binary;
using X = Managed.Emulation.Execution.FullBlockEmitter.FullX87Operation;

namespace Managed.Emulation.Execution;

// R116. This parser consumes an EXACT authoritative decoder span. Opcodes and
// ModRM select compiled IR here; no emitted path interprets an opcode/FOP.
// Logical ST indices never become guest GPR operands. The retained R116
// snapshot and R117 partial report describe model and ISA discrepancies.
internal static class FullX87Lowering
{
    internal static bool TryClassify(ulong ip, ReadOnlySpan<byte> bytes,
        out JitInstruction instruction, out string? reason)
    {
        instruction = default; reason = "NativeX87EncodingMissing";
        if (bytes.Length is < 1 or > 15) return false;
        int p = 0, segment = -1; byte rex = 0;
        bool operand16 = false, address32 = false;
        while (p < bytes.Length)
        {
            byte b = bytes[p];
            if (b is >= 0x40 and <= 0x4f) { rex = b; ++p; continue; }
            if (b == 0x66) operand16 = true;
            else if (b == 0x67) address32 = true;
            else if (b is 0xf2 or 0xf3) { /* ignored legacy REP */ }
            else if (b is 0x26 or 0x2e or 0x36 or 0x3e or 0x64 or 0x65)
                segment = b switch { 0x26 => 0, 0x2e => 1, 0x36 => 2, 0x3e => 3, 0x64 => 4, _ => 5 };
            else if (b == 0xf0) { reason = "NativeX87LockUndefined"; return false; }
            else break;
            rex = 0; ++p;
        }
        if (p == bytes.Length) return false;
        byte opcode = bytes[p++];
        if (opcode == 0x9b)
        {
            if (p != bytes.Length) { reason = "NativeX87ExactWaitSpan"; return false; }
            instruction = new(ip, bytes.Length, JitOperation.X87, 64, default, default, Options: (uint)X.Wait);
            reason = null; return true;
        }
        if (opcode is < 0xd8 or > 0xdf || p == bytes.Length) return false;
        byte modrm = bytes[p++]; int group = modrm >> 3 & 7, rm = modrm & 7;
        bool memory = modrm < 0xc0, integer = false, env16 = false;
        int width = 0, pops = 0;
        X op = 0;
        JitOperand destination = default, source = default;
        JitAddress address = default;
        var st = new JitOperand(rm, 0);
        var mem = new JitOperand(0, 0, IsMemory: true);
        if (memory)
        {
            if (!Address(bytes, ref p, modrm, rex, address32, segment, out address))
            { reason = "NativeX87AddressMissing"; return false; }
            source = mem;
            if (opcode is 0xd8 or 0xda or 0xdc or 0xde)
            {
                width = opcode is 0xd8 or 0xda ? 32 : opcode == 0xdc ? 64 : 16;
                integer = opcode is 0xda or 0xde;
                op = Arithmetic(group); if (group == 3) pops = 1;
            }
            else
            {
                (op, width, pops) = (opcode, group) switch
                {
                    (0xd9, 0) => (X.Load, 32, 0), (0xd9, 2) => (X.Store, 32, 0), (0xd9, 3) => (X.Store, 32, 1),
                    (0xd9, 4) => (X.LoadEnvironment, operand16 ? 112 : 224, 0),
                    (0xd9, 5) => (X.LoadControl, 16, 0),
                    (0xd9, 6) => (X.StoreEnvironment, operand16 ? 112 : 224, 0), (0xd9, 7) => (X.StoreControl, 16, 0),
                    (0xdb, 0) => (X.LoadInteger, 32, 0), (0xdb, 1) => (X.StoreIntegerTruncate, 32, 1),
                    (0xdb, 2) => (X.StoreInteger, 32, 0), (0xdb, 3) => (X.StoreInteger, 32, 1),
                    (0xdb, 5) => (X.Load, 80, 0), (0xdb, 7) => (X.Store, 80, 1),
                    (0xdd, 0) => (X.Load, 64, 0), (0xdd, 1) => (X.StoreIntegerTruncate, 64, 1),
                    (0xdd, 2) => (X.Store, 64, 0), (0xdd, 3) => (X.Store, 64, 1),
                    (0xdd, 4) => (X.Restore, operand16 ? 752 : 864, 0),
                    (0xdd, 6) => (X.Save, operand16 ? 752 : 864, 0), (0xdd, 7) => (X.StoreStatus, 16, 0),
                    (0xdf, 0) => (X.LoadInteger, 16, 0), (0xdf, 1) => (X.StoreIntegerTruncate, 16, 1),
                    (0xdf, 2) => (X.StoreInteger, 16, 0), (0xdf, 3) => (X.StoreInteger, 16, 1),
                    (0xdf, 5) => (X.LoadInteger, 64, 0), (0xdf, 7) => (X.StoreInteger, 64, 1), _ => (0, 0, 0)
                };
                if (op is X.LoadEnvironment or X.StoreEnvironment or X.Save or X.Restore) env16 = operand16;
                if (IsMemoryWrite(op)) { destination = mem; source = default; }
            }
        }
        else if (opcode is 0xd8 or 0xdc or 0xde)
        {
            source = st; op = Arithmetic(group);
            if (group is 2 or 3)
            {
                if (opcode == 0xde) pops = group == 3 ? 2 : 1;
                else if (group == 3) pops = 1;
            }
            else if (opcode != 0xd8)
            {
                destination = st; source = default;
                // DC/DE /4,/6 are reverse arithmetic; /5,/7 are direct.
                op = group switch { 4 => X.SubtractReverse, 5 => X.Subtract, 6 => X.DivideReverse, 7 => X.Divide, _ => op };
                if (opcode == 0xde) pops = 1;
            }
        }
        else if (opcode == 0xd9)
        {
            source = st;
            op = group switch
            {
                0 => X.Load, 1 => X.Exchange, 2 => X.Nop, 3 => X.Store,
                4 => rm switch { 0 => X.ChangeSign, 1 => X.Absolute, 4 => X.Test, 5 => X.Examine, _ => 0 },
                5 => rm switch { 0 => X.One, 1 => X.Log2Ten, 2 => X.Log2E, 3 => X.Pi, 4 => X.Log10Two, 5 => X.LnTwo, 6 => X.Zero, _ => 0 },
                6 => rm switch { 4 => X.Extract, 5 => X.RemainderNearest, 6 => X.DecrementTop, 7 => X.IncrementTop, _ => 0 },
                7 => rm switch { 0 => X.Remainder, 2 => X.SquareRoot, 4 => X.Round, 5 => X.Scale, _ => 0 }, _ => 0
            };
            if (group == 3) { destination = st; source = default; pops = 1; }
            else if (group >= 2) source = default;
            if (group is 6 or 7 && op == 0)
            { reason = "NativeX87Complex" + ComplexName(opcode, modrm) + "Missing"; return false; }
        }
        else if (opcode is 0xda or 0xdb)
        {
            source = st;
            if (group <= 3) op = (X)((uint)(opcode == 0xda ? X.MoveBelow : X.MoveNotBelow) + (uint)group);
            else if (opcode == 0xda && modrm == 0xe9) { op = X.CompareUnordered; source = new(1, 0); pops = 2; }
            else if (opcode == 0xdb && group is 5 or 6) op = group == 5 ? X.CompareUnorderedFlags : X.CompareFlags;
            else if (opcode == 0xdb && group == 4)
            { op = rm == 2 ? X.ClearExceptions : rm == 3 ? X.Initialize : 0; source = default; }
        }
        else if (opcode == 0xdd)
        {
            source = st;
            op = group switch { 0 => X.Free, 1 => X.Exchange, 2 or 3 => X.Store, 4 => X.CompareUnordered, 5 => X.CompareUnordered, _ => 0 };
            if (group is 2 or 3) { destination = st; source = default; }
            if (group is 3 or 5) pops = 1;
        }
        else if (opcode == 0xdf)
        {
            source = st;
            op = group switch { 0 => X.Free, 1 => X.Exchange, 2 or 3 => X.Store, 4 => X.StoreStatusAx, 5 => X.CompareUnorderedFlags, 6 => X.CompareFlags, _ => 0 };
            if (group is 0 or 2 or 3 or 5 or 6) pops = 1;
            if (group is 2 or 3) { destination = st; source = default; }
            if (group == 4) source = default;
        }
        if (p != bytes.Length) { reason = "NativeX87EncodingLength"; return false; }
        if (op == 0)
        { reason = opcode == 0xdf && memory && group is 4 or 6 ? "NativeX87BcdSelectedDisabled" : "NativeX87SparseUndefined"; return false; }
        instruction = new(ip, bytes.Length, JitOperation.X87, 64, destination, source,
            SourceWidth: width, Address: address, Extra: new(0, (ulong)((opcode & 7) << 8 | modrm), true),
            Options: (uint)op | (uint)pops << 16 | (integer ? FullBlockEmitter.X87IntegerMemory : 0) |
                (env16 ? FullBlockEmitter.X87Environment16 : 0));
        reason = null; return true;
    }

    internal static bool IsMemoryWrite(X op) => op is X.Store or X.StoreInteger or X.StoreIntegerTruncate or
        X.StoreControl or X.StoreStatus or X.StoreEnvironment or X.Save;

    internal static string ComplexName(byte opcode, byte modrm) => (opcode, modrm) switch
    {
        (0xd9, 0xf0) => "F2xm1", (0xd9, 0xf1) => "Fyl2x", (0xd9, 0xf2) => "Fptan", (0xd9, 0xf3) => "Fpatan",
        (0xd9, 0xf9) => "Fyl2xp1", (0xd9, 0xfb) => "Fsincos", (0xd9, 0xfe) => "Fsin", (0xd9, 0xff) => "Fcos", _ => "Undefined"
    };

    private static X Arithmetic(int group) => group switch
    { 0 => X.Add, 1 => X.Multiply, 2 or 3 => X.Compare, 4 => X.Subtract, 5 => X.SubtractReverse, 6 => X.Divide, _ => X.DivideReverse };

    private static bool Address(ReadOnlySpan<byte> bytes, ref int p, byte modrm, byte rex,
        bool address32, int segment, out JitAddress address)
    {
        address = default; int mod = modrm >> 6, rm = modrm & 7;
        int b = -1, index = -1, scale = 0, size = mod == 1 ? 1 : mod == 2 ? 4 : 0; bool rip = false;
        if (rm == 4)
        {
            if (p == bytes.Length) return false;
            byte sib = bytes[p++]; scale = sib >> 6; int sb = sib & 7, si = sib >> 3 & 7;
            if (mod == 0 && sb == 5) size = 4; else b = sb | ((rex & 1) << 3);
            if (si != 4 || (rex & 2) != 0) index = si | ((rex & 2) << 2);
        }
        else if (mod == 0 && rm == 5) { size = 4; rip = true; }
        else b = rm | ((rex & 1) << 3);
        if (size > bytes.Length - p) return false;
        long displacement = size == 1 ? (sbyte)bytes[p] : size == 4 ? BinaryPrimitives.ReadInt32LittleEndian(bytes[p..]) : 0;
        p += size; address = new(b, index, scale, displacement, rip, address32 ? 32 : 64, segment); return true;
    }
}
