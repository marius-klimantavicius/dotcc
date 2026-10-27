using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

// Deliberately a leaf on both ABIs: only RAX/RCX/RDX/R8/R9/R10/R11 are
// modified. No CALL, PUSH, POP, stack adjustment, SIMD or guest byte copying.
// Windows can unwind this shape by simulating RET; it needs no RUNTIME_FUNCTION.
internal static class X64BlockEmitter
{
    private const int MaximumInstructions = 32;
    private const int ExitCount = (int)JitExit.MemoryMiss + 1;
    // Conservative bounds: at most 512 bytes per instruction, plus the
    // 40-byte entry and five 28-byte exits. Fixup links live in their rel32
    // placeholders until the corresponding label is bound.
    internal const int CodeCapacity = MaximumInstructions * 512 + 192;
    internal const int ScratchBytes = CodeCapacity + (MaximumInstructions + ExitCount) * sizeof(int) * 2;

    public static byte[] Emit(ReadOnlySpan<JitInstruction> instructions, int maxInstructions = 32)
    {
        if (instructions.Length is < 1 or > MaximumInstructions) throw new ArgumentOutOfRangeException(nameof(instructions));
        if (maxInstructions is < 1 or > 32) throw new ArgumentOutOfRangeException(nameof(maxInstructions));
        // Cache a value rather than a throwing type initializer: the caller
        // must still receive PlatformNotSupportedException and interpret.
        if (!Layout.Supported)
            throw new PlatformNotSupportedException("Translated Machine/JitFrame layout differs from the native block ABI");
        Span<byte> code = stackalloc byte[CodeCapacity];
        Span<int> labels = stackalloc int[MaximumInstructions + ExitCount];
        Span<int> fixupHeads = stackalloc int[MaximumInstructions + ExitCount];
        var writer = new Writer(instructions, maxInstructions, code, labels, fixupHeads);
        return writer.Emit();
    }

    // Preserve the old nullable array/list API and its argument exceptions.
    // Neither common caller needs a copy; new compile paths pass spans directly.
    public static byte[] Emit(JitInstruction[] instructions, int maxInstructions = 32)
    {
        ArgumentNullException.ThrowIfNull(instructions);
        return Emit((ReadOnlySpan<JitInstruction>)instructions, maxInstructions);
    }

    public static byte[] Emit(IReadOnlyList<JitInstruction> instructions, int maxInstructions = 32)
    {
        ArgumentNullException.ThrowIfNull(instructions);
        if (instructions is JitInstruction[] array) return Emit(array, maxInstructions);
        if (instructions is List<JitInstruction> list) return Emit(CollectionsMarshal.AsSpan(list), maxInstructions);
        if (instructions.Count is < 1 or > MaximumInstructions) throw new ArgumentOutOfRangeException(nameof(instructions));
        if (maxInstructions is < 1 or > 32) throw new ArgumentOutOfRangeException(nameof(maxInstructions));
        Span<JitInstruction> copy = stackalloc JitInstruction[MaximumInstructions];
        int count = instructions.Count;
        for (int i = 0; i < count; ++i) copy[i] = instructions[i];
        return Emit(copy[..count], maxInstructions);
    }

    private static class Layout
    {
        internal static readonly bool Supported = HasExpectedLayout();
    }

    private static unsafe bool HasExpectedLayout()
    {
        Blink.Machine m = default;
        byte* p = (byte*)&m;
        // Fixed buffers promoted out of anonymous C unions retain their exact
        // translated field paths. Compile against the real producer layout.
        var r = &m.__anon___Anon37641D7941AF5F1FE58967576BC28A4841304ED904916BBE35E5A7FCC406FC9D
            .__anon___Anon5C063FEB9777807268733BCDB385F747165DC2D589C0D0C27891DEC268C9BC0E;
        return !((byte*)&m.ip - p != 0 || (byte*)&m.oplen - p != 8 || (byte*)&m.mode - p != 9 ||
            (byte*)&m.attention - p != 11 || (byte*)&m.flags - p != 12 ||
            r->__anon___Anon445E5DCBC7730421B53C86C8BE1EE5E07432263145DA093F727790ECADFE7DE4.ax - p != 24 ||
            r->__anon___AnonB069B6D4620B81265902ABA36EEBE5D5AC161682C44CDBB372A1A9982EF23E25.cx - p != 32 ||
            r->__anon___Anon83A5E445BF934F401681EA099D0AFF547485A839D028D7AD2B4DB983694F409C.dx - p != 40 ||
            r->__anon___AnonDA484378C5DE77BD535940C5F81948C8A2246D0E9C98BA1321C1909A06761000.bx - p != 48 ||
            r->sp - p != 56 || r->bp - p != 64 || r->si - p != 72 || r->di - p != 80 ||
            r->r8 - p != 88 || r->r9 - p != 96 || r->r10 - p != 104 || r->r11 - p != 112 ||
            r->r12 - p != 120 || r->r13 - p != 128 || r->r14 - p != 136 || r->r15 - p != 144 ||
            sizeof(JitFrame) != 96 || Marshal.OffsetOf<JitFrame>(nameof(JitFrame.NativeLimit)).ToInt32() != 60 || Marshal.OffsetOf<JitFrame>(nameof(JitFrame.NextIp)).ToInt32() != 72 ||
            Marshal.OffsetOf<JitFrame>(nameof(JitFrame.DataDescriptors)).ToInt32() != 80 ||
            Marshal.OffsetOf<JitFrame>(nameof(JitFrame.DataDescriptorCount)).ToInt32() != 88 ||
            Marshal.OffsetOf<JitFrame>(nameof(JitFrame.DataGeneration)).ToInt32() != 92 ||
            sizeof(JitDataDescriptor) != 48 ||
            Marshal.OffsetOf<JitDataDescriptor>(nameof(JitDataDescriptor.Page)).ToInt32() != 0 ||
            Marshal.OffsetOf<JitDataDescriptor>(nameof(JitDataDescriptor.Host)).ToInt32() != 8 ||
            Marshal.OffsetOf<JitDataDescriptor>(nameof(JitDataDescriptor.Identity)).ToInt32() != 16 ||
            Marshal.OffsetOf<JitDataDescriptor>(nameof(JitDataDescriptor.Generation)).ToInt32() != 24 ||
            Marshal.OffsetOf<JitDataDescriptor>(nameof(JitDataDescriptor.ValidStart)).ToInt32() != 32 ||
            Marshal.OffsetOf<JitDataDescriptor>(nameof(JitDataDescriptor.ValidEnd)).ToInt32() != 36 ||
            Marshal.OffsetOf<JitDataDescriptor>(nameof(JitDataDescriptor.Permissions)).ToInt32() != 40);
    }

    private ref struct Writer
    {
        private readonly ReadOnlySpan<JitInstruction> instructions;
        private readonly int maxInstructions;
        private readonly Span<byte> code;
        private readonly Span<int> labels;
        private readonly Span<int> fixupHeads;
        private int position;

        public Writer(ReadOnlySpan<JitInstruction> instructions, int maxInstructions,
            Span<byte> code, Span<int> labels, Span<int> fixupHeads)
        {
            this.instructions = instructions;
            this.maxInstructions = maxInstructions;
            this.code = code;
            this.labels = labels;
            this.fixupHeads = fixupHeads;
            position = 0;
            labels.Fill(-1);
            fixupHeads.Fill(-1);
            for (int index = 0; index < instructions.Length; ++index)
            {
                var instruction = instructions[index];
                for (int prior = 0; prior < index; ++prior)
                    if (instructions[prior].Ip == instruction.Ip) throw new ArgumentException("Duplicate instruction address");
                if (instruction.Length is < 1 or > 15 || instruction.Width is not (8 or 16 or 32 or 64) ||
                    (uint)instruction.Operation > (uint)JitOperation.Ret) throw new ArgumentException("Invalid native instruction IR");
                if (instruction.Operation == JitOperation.ConditionalJump && instruction.Condition is < 0 or > 15)
                    throw new ArgumentException("Invalid branch condition");
                if (instruction.Operation is JitOperation.Call or JitOperation.Ret)
                {
                    bool call = instruction.Operation == JitOperation.Call;
                    if (instruction.Ip != instructions[^1].Ip || instruction.Length != (call ? 5 : 1) ||
                        instruction.Width != 64 || instruction.SourceWidth != 64 ||
                        instruction.Address != new JitAddress(4, -1, 0, call ? -8 : 0))
                        throw new ArgumentException("Invalid terminal guest call/return");
                    continue;
                }
                if (instruction.Operation is not (JitOperation.Jump or JitOperation.ConditionalJump or JitOperation.Nop))
                {
                    bool memoryFlags = instruction.Operation is JitOperation.MemoryCmp or JitOperation.MemoryCmpReverse or JitOperation.MemoryTest;
                    bool store = instruction.Operation is JitOperation.StoreMov or JitOperation.Push;
                    bool stack = instruction.Operation is JitOperation.Push or JitOperation.Pop;
                    if (memoryFlags)
                        ValidateOperand(instruction.Operation == JitOperation.MemoryCmpReverse ? instruction.Destination : instruction.Source,
                            instruction.Width, destination: false);
                    else if (store)
                    {
                        if (instruction.Source.IsImmediate) throw new ArgumentException("Immediate memory source");
                        ValidateOperand(instruction.Source, instruction.Width, destination: false);
                    }
                    else ValidateOperand(instruction.Destination, instruction.Width, destination: true);
                    bool load = instruction.Operation is JitOperation.LoadMov or JitOperation.LoadMovzx;
                    if (instruction.Operation == JitOperation.Lea || load || store || stack || memoryFlags)
                    {
                        var address = instruction.Address;
                        if ((!memoryFlags && instruction.Width is not (32 or 64)) || address.Base is < -1 or > 15 ||
                            address.Index is < -1 or > 15 || address.Scale is < 0 or > 3 ||
                            (address.RipRelative && (address.Base != -1 || address.Index != -1)))
                            throw new ArgumentException("Invalid native address operand");
                        if (load && (instruction.Operation == JitOperation.LoadMov ?
                            instruction.SourceWidth != instruction.Width : instruction.SourceWidth is not (8 or 16)))
                            throw new ArgumentException("Invalid native load width");
                        if ((store || stack || memoryFlags) && instruction.SourceWidth != instruction.Width)
                            throw new ArgumentException("Invalid native memory width");
                        if (stack && (instruction.Width != 64 ||
                            address != new JitAddress(4, -1, 0, instruction.Operation == JitOperation.Push ? -8 : 0)))
                            throw new ArgumentException("Invalid native stack operand");
                    }
                    else if (instruction.Operation is JitOperation.Movzx or JitOperation.Movsx)
                    {
                        if (instruction.Width is not (16 or 32 or 64) || instruction.SourceWidth is not (8 or 16 or 32) ||
                            instruction.SourceWidth > instruction.Width ||
                            (instruction.SourceWidth == 32 && (instruction.Operation != JitOperation.Movsx || instruction.Width != 64)) ||
                            instruction.Source.IsImmediate)
                            throw new ArgumentException("Invalid native extension operand");
                        ValidateOperand(instruction.Source, instruction.SourceWidth, destination: false);
                    }
                    else if (instruction.Operation is not (JitOperation.Inc or JitOperation.Dec))
                        ValidateOperand(instruction.Source, instruction.Width, destination: false);
                }
            }
        }

        private static void ValidateOperand(JitOperand operand, int width, bool destination)
        {
            if (operand.IsImmediate)
            {
                if (destination) throw new ArgumentException("Immediate destination");
                return;
            }
            if (operand.Register is < 0 or > 15 || operand.ByteOffset is < 0 or > 1 ||
                (operand.ByteOffset != 0 && (width != 8 || operand.Register > 3)))
                throw new ArgumentException("Invalid native register operand");
        }

        public byte[] Emit()
        {
            Bytes(0xf3, 0x0f, 0x1e, 0xfa); // ENDBR64, including on IBT hosts
            Bytes(0x49, 0x89, OperatingSystem.IsWindows() ? (byte)0xca : (byte)0xfa); // r10 = RCX/RDI
            Bytes(0x4d, 0x8b, 0x5a, 0x08); // r11 = machine
            Bytes(0x41, 0xb9); U32((uint)maxInstructions); // r9d = dynamic limit
            Bytes(0x41, 0x8b, 0x42, 0x3c, 0x85, 0xc0); // eax=requested cap; zero retains compiled cap
            int defaultLimit = NearCondition(0x84);
            Bytes(0x41, 0x39, 0xc1, 0x44, 0x0f, 0x47, 0xc8); // r9d=min(r9d,eax), unsigned
            Patch(defaultLimit, position);
            Bytes(0x45, 0x89, 0x4a, 0x3c); // retain effective cap for every exit

            for (int index = 0; index < instructions.Length; ++index)
            {
                var instruction = instructions[index];
                ulong? following = index + 1 < instructions.Length ? instructions[index + 1].Ip : null;
                Bind(index);
                Poll();
                if (instruction.Operation is JitOperation.LoadMov or JitOperation.LoadMovzx or JitOperation.Pop or JitOperation.Ret or JitOperation.MemoryCmp or JitOperation.MemoryCmpReverse or JitOperation.MemoryTest)
                    GuardMemory(instruction, JitMemoryAccess.Read);
                else if (instruction.Operation is JitOperation.StoreMov or JitOperation.Push or JitOperation.Call)
                    GuardMemory(instruction, JitMemoryAccess.Write);
                Issue();
                if (instruction.Operation == JitOperation.ConditionalJump)
                {
                    Condition(instruction.Condition);
                    Bytes(0x85, 0xc0); // test eax,eax
                    int taken = NearCondition(0x85); // jne taken
                    Commit(unchecked(instruction.Ip + (uint)instruction.Length));
                    // The taken path is emitted next, so the untaken path
                    // must dispatch explicitly even if its guest IP follows.
                    Dispatch(unchecked(instruction.Ip + (uint)instruction.Length));
                    Patch(taken, position);
                    Commit(instruction.Target);
                    Dispatch(instruction.Target, following, forceExternal: index + 1 < instructions.Length);
                }
                else if (instruction.Operation is JitOperation.Call or JitOperation.Ret)
                {
                    Operation(instruction);
                    // RET has already loaded machine.ip from the checked guest
                    // stack. Neither form may dispatch to a native trace label.
                    Commit(instruction.Operation == JitOperation.Call ? instruction.Target : null);
                    Dispatch(instruction.Target, forceExternal: true);
                }
                else
                {
                    Operation(instruction);
                    ulong next = instruction.Operation == JitOperation.Jump ? instruction.Target : unchecked(instruction.Ip + (uint)instruction.Length);
                    Commit(next);
                    Dispatch(next, following);
                }
            }
            for (int exit = 0; exit < ExitCount; ++exit)
            {
                Bind(MaximumInstructions + exit);
                Bytes(0x41, 0x8b, 0x42, 0x3c); // eax=effective invocation cap
                Bytes(0x44, 0x29, 0xc8, 0x49, 0x89, 0x42, 0x30); // Executed = limit-r9d
                Bytes(0x49, 0x8b, 0x03, 0x49, 0x89, 0x42, 0x48); // NextIp = machine.ip
                Bytes(0xb8); U32((uint)exit);
                Bytes(0x41, 0x89, 0x42, 0x38, 0xc3); // frame.Exit=eax; ret
            }
            return code[..position].ToArray();
        }

        private void Poll()
        {
            Bytes(0x49, 0x8b, 0x42, 0x20, 0x83, 0x38, 0x00); // cmp dword [frame.Stop],0
            ExitCondition(0x85, JitExit.Stop);
            Bytes(0x41, 0x80, 0x7b, 0x0b, 0x00); // cmp byte [machine.attention],0
            ExitCondition(0x85, JitExit.Attention);
        }

        private void Issue()
        {
            Bytes(0x49, 0x8b, 0x42, 0x10, 0xba, 1, 0, 0, 0); // rax=issued; edx=1
            Bytes(0xf0, 0x48, 0x0f, 0xc1, 0x10); // lock xadd [rax],rdx (old issued)
            Bytes(0x49, 0x3b, 0x52, 0x28); // cmp rdx,frame.Budget
            ExitCondition(0x83, JitExit.Budget); // old >= budget, including rejected issue
        }

        private void GuardMemory(JitInstruction instruction, JitMemoryAccess access)
        {
            int width = instruction.SourceWidth / 8;
            Address(instruction, truncate: false); // guest address is always 64 bits
            Bytes(0x48, 0x89, 0xc1, 0x48, 0xc1, 0xe9, 47, 0x48, 0x85, 0xc9);
            ExitCondition(0x85, JitExit.MemoryMiss); // positive canonical EA < 2^47
            Bytes(0x89, 0xc2, 0x81, 0xe2); U32(4095); // edx = page offset
            Bytes(0x81, 0xfa); U32((uint)(4096 - width));
            ExitCondition(0x87, JitExit.MemoryMiss); // one page, nonwrapping extent
            Bytes(0x48, 0x25); U32(0xfffff000); // rax = guest page
            Bytes(0x41, 0x8b, 0x4a, 0x58, 0x83, 0xf9, JitDataDescriptor.MaximumCount);
            ExitCondition(0x87, JitExit.MemoryMiss); // bounded count before descriptor access
            Bytes(0x85, 0xc9);
            ExitCondition(0x84, JitExit.MemoryMiss);
            Bytes(0x4d, 0x8b, 0x42, 0x50, 0x4d, 0x85, 0xc0); // r8 = descriptor array
            ExitCondition(0x84, JitExit.MemoryMiss);
            int search = position;
            Bytes(0x49, 0x39, 0x00); // descriptor.Page == guest page
            int next = NearCondition(0x85);
            Bytes(0x41, 0x3b, 0x50, 0x20); // offset >= ValidStart
            ExitCondition(0x82, JitExit.MemoryMiss);
            Bytes(0x41, 0x8b, 0x48, 0x24, 0x81, 0xf9); U32(4096); // ValidEnd <= page
            ExitCondition(0x87, JitExit.MemoryMiss);
            Bytes(0x8d, 0x42, (byte)width, 0x39, 0xc8); // offset+width <= ValidEnd
            ExitCondition(0x87, JitExit.MemoryMiss);
            Bytes(0x41, 0xf6, 0x40, 0x28, (byte)access); // exact read/write permission bit
            ExitCondition(0x84, JitExit.MemoryMiss);
            Bytes(0x49, 0x83, 0x78, 0x10, 0); // nonzero identity
            ExitCondition(0x84, JitExit.MemoryMiss);
            Bytes(0x41, 0x8b, 0x4a, 0x5c, 0x85, 0xc9); // invocation mapping epoch
            ExitCondition(0x84, JitExit.MemoryMiss);
            Bytes(0x49, 0x39, 0x48, 0x18); // descriptor.Generation == invocation epoch
            ExitCondition(0x85, JitExit.MemoryMiss);
            Bytes(0x4d, 0x8b, 0x40, 0x08, 0x4d, 0x85, 0xc0); // r8 = host page
            ExitCondition(0x84, JitExit.MemoryMiss);
            Bytes(0x49, 0x01, 0xd0); // checked host address retained across issuance
            ExitCondition(0x82, JitExit.MemoryMiss); // host addition must not wrap
            Bytes(0xe9); int success = position; U32(0);
            Patch(next, position);
            Bytes(0x49, 0x83, 0xc0, 48, 0xff, 0xc9); // next descriptor, --count
            Patch(NearCondition(0x85), search);
            Bytes(0xe9); Reference(MaximumInstructions + (int)JitExit.MemoryMiss);
            Patch(success, position);
        }

        private void Operation(JitInstruction instruction)
        {
            JitOperation op = instruction.Operation;
            if (op is JitOperation.Jump or JitOperation.Nop) return;
            if (op == JitOperation.Call)
            {
                // Push the guest fall-through, never a host return address.
                Bytes(0x48, 0xb8); U64(unchecked(instruction.Ip + (uint)instruction.Length));
                Bytes(0x49, 0x89, 0x00); // [checked r8] = guest return IP
                Bytes(0x49, 0x83, 0x6b, 0x38, 8); // guest RSP -= 8
                return;
            }
            if (op == JitOperation.Ret)
            {
                Bytes(0x49, 0x8b, 0x00); // guest return IP = [checked r8]
                Bytes(0x49, 0x83, 0x43, 0x38, 8); // guest RSP += 8
                Bytes(0x49, 0x89, 0x03); // machine.ip = guest return IP
                return;
            }
            int width = instruction.Width;
            if (op is JitOperation.StoreMov or JitOperation.Push)
            {
                // R8 is the checked host address. Read the old source before
                // updating guest RSP, including PUSH RSP and MOV [RSP],RSP.
                Load(instruction.Source, width, 0);
                Bytes(width == 64 ? (byte)0x49 : (byte)0x41, 0x89, 0x00); // [r8] <- rax/eax, exactly 8/4 bytes
                if (op == JitOperation.Push) Bytes(0x49, 0x83, 0x6b, 0x38, 8); // guest RSP -= 8
                return;
            }
            if (op == JitOperation.Pop)
            {
                Bytes(0x49, 0x8b, 0x00); // rax = [checked r8], before any guest register change
                Bytes(0x49, 0x83, 0x43, 0x38, 8); // guest RSP += 8
                Bytes(0x49, 0x89, 0xc0);
                Store(instruction.Destination, 64); // POP RSP overwrites the increment
                return;
            }
            if (op is JitOperation.LoadMov or JitOperation.LoadMovzx)
            {
                // R8 survived the atomic issuance check. The caller's lease
                // authenticates the pointer until this synchronous return.
                if (instruction.SourceWidth is 8 or 16)
                    Bytes(0x41, 0x0f, instruction.SourceWidth == 8 ? (byte)0xb6 : (byte)0xb7, 0x00);
                else Bytes(instruction.SourceWidth == 64 ? (byte)0x49 : (byte)0x41, 0x8b, 0x00);
                Bytes(0x49, 0x89, 0xc0);
                Store(instruction.Destination, width);
                return;
            }
            if (op == JitOperation.Lea)
            {
                Address(instruction);
                Bytes(0x49, 0x89, 0xc0); // r8=rax
                Store(instruction.Destination, width);
                return;
            }
            if (op is JitOperation.Movzx or JitOperation.Movsx)
            {
                Load(instruction.Source, instruction.SourceWidth, 0);
                if (op == JitOperation.Movsx)
                {
                    if (instruction.SourceWidth == 32) Bytes(0x48, 0x63, 0xc0); // movsxd rax,eax
                    else Bytes(0x48, 0x0f, instruction.SourceWidth == 8 ? (byte)0xbe : (byte)0xbf, 0xc0);
                }
                // A 32-bit destination zeroes its high half even for MOVSX.
                if (width == 32) Bytes(0x89, 0xc0);
                Bytes(0x49, 0x89, 0xc0);
                Store(instruction.Destination, width);
                return;
            }
            if (op == JitOperation.Mov)
            {
                Load(instruction.Source, width, 0);
                Bytes(0x49, 0x89, 0xc0); // r8=rax
                Store(instruction.Destination, width);
                return;
            }
            bool memoryFlags = op is JitOperation.MemoryCmp or JitOperation.MemoryCmpReverse or JitOperation.MemoryTest;
            if (memoryFlags)
            {
                // The checked R8 address is consumed only after issuance. Read
                // exactly the guest width; CMP/TEST never write guest memory.
                int memoryRegister = op == JitOperation.MemoryCmpReverse ? 1 : 0;
                if (width is 8 or 16)
                    Bytes(0x41, 0x0f, width == 8 ? (byte)0xb6 : (byte)0xb7, (byte)(memoryRegister << 3));
                else Bytes(width == 64 ? (byte)0x49 : (byte)0x41, 0x8b, (byte)(memoryRegister << 3));
                if (memoryRegister == 1) Load(instruction.Destination, width, 0);
                else Load(instruction.Source, width, 1);
                op = op == JitOperation.MemoryTest ? JitOperation.Test : JitOperation.Cmp;
            }
            else Load(instruction.Destination, width, 0);
            bool bump = op is JitOperation.Inc or JitOperation.Dec;
            if (bump) { Bytes(0xb9, 1, 0, 0, 0); }
            else if (!memoryFlags) Load(instruction.Source, width, 1);
            int opcode = op switch
            {
                JitOperation.Add or JitOperation.Inc => 0x01,
                JitOperation.Sub or JitOperation.Cmp or JitOperation.Dec => 0x29,
                JitOperation.And or JitOperation.Test => 0x21,
                JitOperation.Or => 0x09,
                JitOperation.Xor => 0x31,
                _ => throw new ArgumentException("Unsupported native operation")
            };
            if (width == 16) Bytes(0x66);
            if (width == 64) Bytes(0x48);
            Bytes((byte)(width == 8 ? opcode - 1 : opcode), 0xc8); // alu rax,rcx
            Bytes(0x49, 0x89, 0xc0); // save result in r8 before LAHF overwrites AH
            Bytes(0x0f, 0x90, 0xc2, 0x9f); // seto dl; lahf
            Bytes(0xc1, 0xe8, 8); // eax = captured host status
            uint captureMask = op switch
            {
                JitOperation.Inc => 0xc0, // Blink passes y=0: INC clears AF
                JitOperation.Dec => 0xd0, // preserve guest CF
                JitOperation.And or JitOperation.Or or JitOperation.Xor or JitOperation.Test => 0xc0, // AF=0
                _ => 0xd1
            };
            Bytes(0x25); U32(captureMask);
            Bytes(0x0f, 0xb6, 0xd2, 0xc1, 0xe2, 11, 0x09, 0xd0); // merge OF
            Bytes(0x41, 0x0f, 0xb6, 0xc8, 0xc1, 0xe1, 24, 0x09, 0xc8); // merge lazy parity byte
            Bytes(0x41, 0x8b, 0x53, 0x0c, 0x81, 0xe2);
            U32(bump ? 0x00fff72fu : 0x00fff72eu); // keep old PF and all unaffected bits; bump keeps CF
            Bytes(0x09, 0xd0, 0x41, 0x89, 0x43, 0x0c); // store explicit Blink flags
            if (op is not (JitOperation.Cmp or JitOperation.Test)) Store(instruction.Destination, width);
        }

        private void Address(JitInstruction instruction, bool truncate = true)
        {
            var address = instruction.Address;
            ulong constant = unchecked((ulong)address.Displacement +
                (address.RipRelative ? instruction.Ip + (uint)instruction.Length : 0));
            Bytes(0x48, 0xb8); U64(constant); // rax = signed displacement / next RIP
            if (address.Base >= 0)
            {
                Load(new(address.Base, 0), 64, 1);
                Bytes(0x48, 0x01, 0xc8); // add rax,rcx
            }
            if (address.Index >= 0)
            {
                Load(new(address.Index, 0), 64, 1);
                if (address.Scale != 0) Bytes(0x48, 0xc1, 0xe1, (byte)address.Scale);
                Bytes(0x48, 0x01, 0xc8);
            }
            if (truncate && instruction.Width == 32) Bytes(0x89, 0xc0); // LEA modulo 2^32, zero extended
        }

        private void Load(JitOperand operand, int width, int register)
        {
            if (operand.IsImmediate)
            {
                if (width == 32) { Bytes((byte)(0xb8 + register)); U32((uint)operand.Immediate); }
                else { Bytes(0x48, (byte)(0xb8 + register)); U64(operand.Immediate); }
                return;
            }
            int offset = 24 + operand.Register * 8 + operand.ByteOffset;
            byte rex = width == 64 ? (byte)0x49 : (byte)0x41;
            Bytes(rex);
            if (width is 8 or 16) Bytes(0x0f, width == 8 ? (byte)0xb6 : (byte)0xb7);
            else Bytes(0x8b);
            Memory(register, 3, offset); // reg <- [r11+offset]
        }

        private void Store(JitOperand operand, int width)
        {
            int offset = 24 + operand.Register * 8 + operand.ByteOffset;
            if (width == 16) Bytes(0x66);
            Bytes(width is 32 or 64 ? (byte)0x4d : (byte)0x45, width == 8 ? (byte)0x88 : (byte)0x89);
            Memory(0, 3, offset); // [r11+offset] <- r8 (32-bit guest writes zero extend all 64 bits)
        }

        private void Memory(int reg, int baseLow, int offset)
        {
            if (offset < 128) Bytes((byte)(0x40 | reg << 3 | baseLow), (byte)offset);
            else { Bytes((byte)(0x80 | reg << 3 | baseLow)); U32((uint)offset); }
        }

        private void Condition(int condition)
        {
            Bytes(0x41, 0x8b, 0x43, 0x0c); // eax=flags, never install guest bits in RFLAGS
            switch (condition >> 1)
            {
                case 0: Bytes(0xc1, 0xe8, 11); break; // OF
                case 1: break; // CF
                case 2: Bytes(0xc1, 0xe8, 6); break; // ZF
                case 3: Bytes(0x89, 0xc1, 0xc1, 0xe9, 6, 0x09, 0xc8); break; // CF|ZF
                case 4: Bytes(0xc1, 0xe8, 7); break; // SF
                case 5:
                    Bytes(0xc1, 0xe8, 24, 0x84, 0xc0, 0x0f, 0x9a, 0xc0, 0x0f, 0xb6, 0xc0); // lazy parity
                    break;
                case 6: // SF != OF
                case 7: // (SF != OF) | ZF
                    if ((condition >> 1) == 7) Bytes(0x89, 0xc2, 0xc1, 0xea, 6, 0x83, 0xe2, 1);
                    Bytes(0x89, 0xc1, 0xc1, 0xe9, 4, 0x31, 0xc8, 0xc1, 0xe8, 7);
                    if ((condition >> 1) == 7) Bytes(0x09, 0xd0);
                    break;
            }
            Bytes(0x83, 0xe0, 1);
            if ((condition & 1) != 0) Bytes(0x83, 0xf0, 1);
        }

        private void Commit(ulong? ip)
        {
            if (ip.HasValue)
            {
                Bytes(0x48, 0xb8); U64(ip.Value);
                Bytes(0x49, 0x89, 0x03); // machine.ip
            }
            // Every exit materializes NextIp from machine.ip, including RET.
            Bytes(0x41, 0xc6, 0x43, 0x08, 0x00); // oplen=0 after successful op
            Bytes(0x49, 0x8b, 0x42, 0x18, 0xf0, 0x48, 0xff, 0x00); // lock inc *completed
            Bytes(0x41, 0xff, 0xc9); // --r9d
            ExitCondition(0x84, JitExit.Continue);
        }

        private void Dispatch(ulong ip, ulong? following = null, bool forceExternal = false)
        {
            if (!forceExternal && following == ip) return; // exact fall-through to the next poll/issue
            Bytes(0xe9);
            if (!forceExternal)
                for (int i = 0; i < instructions.Length; ++i)
                    if (instructions[i].Ip == ip) { Reference(i); return; }
            Reference(MaximumInstructions + (int)JitExit.Continue);
        }

        private int NearCondition(byte condition)
        { Bytes(0x0f, condition); int at = position; U32(0); return at; }
        private void ExitCondition(byte condition, JitExit exit)
        { Bytes(0x0f, condition); Reference(MaximumInstructions + (int)exit); }

        private void Reference(int label)
        {
            int at = position;
            // Negative one terminates the in-buffer forward-reference chain.
            // Bound labels can be patched immediately, including backedges.
            U32(unchecked((uint)fixupHeads[label]));
            if (labels[label] >= 0) Patch(at, labels[label]);
            else fixupHeads[label] = at;
        }

        private void Bind(int label)
        {
            int at = fixupHeads[label];
            while (at >= 0)
            {
                int next = BinaryPrimitives.ReadInt32LittleEndian(code.Slice(at, 4));
                Patch(at, position);
                at = next;
            }
            fixupHeads[label] = -1;
            labels[label] = position;
        }

        private void Patch(int at, int target)
        {
            int displacement = checked(target - (at + 4));
            BinaryPrimitives.WriteInt32LittleEndian(code.Slice(at, 4), displacement);
        }

        private Span<byte> Reserve(int count)
        {
            if (count > code.Length - position)
                throw new PlatformNotSupportedException("Native block exceeds bounded emitter scratch");
            Span<byte> bytes = code.Slice(position, count);
            position += count;
            return bytes;
        }

        // Fixed arities keep literal byte sequences allocation-free on both runtimes.
        private void Bytes(byte b0)
        {
            Span<byte> bytes = Reserve(1);
            bytes[0] = b0;
        }
        private void Bytes(byte b0, byte b1)
        {
            Span<byte> bytes = Reserve(2);
            bytes[0] = b0; bytes[1] = b1;
        }
        private void Bytes(byte b0, byte b1, byte b2)
        {
            Span<byte> bytes = Reserve(3);
            bytes[0] = b0; bytes[1] = b1; bytes[2] = b2;
        }
        private void Bytes(byte b0, byte b1, byte b2, byte b3)
        {
            Span<byte> bytes = Reserve(4);
            bytes[0] = b0; bytes[1] = b1; bytes[2] = b2; bytes[3] = b3;
        }
        private void Bytes(byte b0, byte b1, byte b2, byte b3, byte b4)
        {
            Span<byte> bytes = Reserve(5);
            bytes[0] = b0; bytes[1] = b1; bytes[2] = b2; bytes[3] = b3;
            bytes[4] = b4;
        }
        private void Bytes(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5)
        {
            Span<byte> bytes = Reserve(6);
            bytes[0] = b0; bytes[1] = b1; bytes[2] = b2; bytes[3] = b3;
            bytes[4] = b4; bytes[5] = b5;
        }
        private void Bytes(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6)
        {
            Span<byte> bytes = Reserve(7);
            bytes[0] = b0; bytes[1] = b1; bytes[2] = b2; bytes[3] = b3;
            bytes[4] = b4; bytes[5] = b5; bytes[6] = b6;
        }
        private void Bytes(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7)
        {
            Span<byte> bytes = Reserve(8);
            bytes[0] = b0; bytes[1] = b1; bytes[2] = b2; bytes[3] = b3;
            bytes[4] = b4; bytes[5] = b5; bytes[6] = b6; bytes[7] = b7;
        }
        private void Bytes(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7, byte b8)
        {
            Span<byte> bytes = Reserve(9);
            bytes[0] = b0; bytes[1] = b1; bytes[2] = b2; bytes[3] = b3;
            bytes[4] = b4; bytes[5] = b5; bytes[6] = b6; bytes[7] = b7;
            bytes[8] = b8;
        }
        private void Bytes(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7, byte b8, byte b9)
        {
            Span<byte> bytes = Reserve(10);
            bytes[0] = b0; bytes[1] = b1; bytes[2] = b2; bytes[3] = b3;
            bytes[4] = b4; bytes[5] = b5; bytes[6] = b6; bytes[7] = b7;
            bytes[8] = b8; bytes[9] = b9;
        }
        private void Bytes(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7, byte b8, byte b9, byte b10)
        {
            Span<byte> bytes = Reserve(11);
            bytes[0] = b0; bytes[1] = b1; bytes[2] = b2; bytes[3] = b3;
            bytes[4] = b4; bytes[5] = b5; bytes[6] = b6; bytes[7] = b7;
            bytes[8] = b8; bytes[9] = b9; bytes[10] = b10;
        }

        private void U32(uint value)
            => BinaryPrimitives.WriteUInt32LittleEndian(Reserve(4), value);
        private void U64(ulong value)
            => BinaryPrimitives.WriteUInt64LittleEndian(Reserve(8), value);
    }
}
