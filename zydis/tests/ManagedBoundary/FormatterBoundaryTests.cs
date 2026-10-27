using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Managed.Zydis;
using Core = Managed.Zydis.Zydis;

internal static unsafe class FormatterBoundaryTests
{
    private const int Capacity = 128;
    private const ulong Address = 0x123456789ABCDEF0;
    private enum Behavior { Success, ReturnFailure, ThrowAndContain }

    public static void Intel() => Run(Core.ZydisFormatterStyle_.ZYDIS_FORMATTER_STYLE_INTEL, Behavior.Success);
    public static void Att() => Run(Core.ZydisFormatterStyle_.ZYDIS_FORMATTER_STYLE_ATT, Behavior.Success);
    public static void StatusFailure() => Run(Core.ZydisFormatterStyle_.ZYDIS_FORMATTER_STYLE_INTEL, Behavior.ReturnFailure);
    public static void ContainedException() => Run(Core.ZydisFormatterStyle_.ZYDIS_FORMATTER_STYLE_ATT, Behavior.ThrowAndContain);

    private static void Run(Core.ZydisFormatterStyle_ style, Behavior behavior)
    {
        // Exercise returns from a non-inlined frame before checking release, so
        // the JIT cannot keep one of its temporary state references alive here.
        WeakReference witness = Exercise(style, behavior);
        Check.CompactGc();
        Check.Require(!witness.IsAlive, "managed context is collectible after its GCHandle is released");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference Exercise(Core.ZydisFormatterStyle_ style, Behavior behavior)
    {
        var instruction = InstructionCodec.Decode([0x48, 0x89, 0xD8]); // mov rax, rbx
        var metadata = instruction.Metadata;
        var operands = instruction.Operands.ToArray();
        byte[] output = Check.GuardedBuffer(Capacity);
        Core.ZydisFormatter_ formatter = default;
        new ZydisStatus(Core.ZydisFormatterInit(&formatter, style)).ThrowIfFailed("boundary formatter init");

        void* originalMnemonic = Query(&formatter, Core.ZydisFormatterFunction_.ZYDIS_FORMATTER_FUNC_PRINT_MNEMONIC);
        void* originalRegister = Query(&formatter, Core.ZydisFormatterFunction_.ZYDIS_FORMATTER_FUNC_PRINT_REGISTER);
        Check.Require(originalMnemonic != null && originalRegister != null, "default callbacks are present");
        Check.Require(originalMnemonic == (void*)formatter.func_print_mnemonic
            && originalRegister == (void*)formatter.func_print_register, "query selects the actual emitted callback fields");

        delegate* managed<Core.ZydisFormatter_*, Core.ZydisFormatterBuffer_*, Core.ZydisFormatterContext_*, uint>
            mnemonicHook = &PrintMnemonic;
        delegate* managed<Core.ZydisFormatter_*, Core.ZydisFormatterBuffer_*, Core.ZydisFormatterContext_*, Core.ZydisRegister_, uint>
            registerHook = &PrintRegister;
        using var root = CreateRoot(operands, output, behavior);
        try
        {
            Check.Require(Exchange(&formatter, Core.ZydisFormatterFunction_.ZYDIS_FORMATTER_FUNC_PRINT_MNEMONIC,
                (void*)mnemonicHook) == originalMnemonic, "mnemonic installation returns the displaced callback");
            Check.Require(Exchange(&formatter, Core.ZydisFormatterFunction_.ZYDIS_FORMATTER_FUNC_PRINT_REGISTER,
                (void*)registerHook) == originalRegister, "register installation returns the displaced callback");
            Check.Require(Query(&formatter, Core.ZydisFormatterFunction_.ZYDIS_FORMATTER_FUNC_PRINT_MNEMONIC) == (void*)mnemonicHook
                && Query(&formatter, Core.ZydisFormatterFunction_.ZYDIS_FORMATTER_FUNC_PRINT_REGISTER) == (void*)registerHook,
                "null callback query preserves both installed identities");

            fixed (Core.ZydisDecodedOperand_* operandPointer = operands)
            fixed (byte* outputPointer = output.AsSpan(Check.GuardLength, Capacity))
            {
                Configure(root.Cookie, &formatter, &metadata, operandPointer, outputPointer,
                    originalMnemonic, originalRegister);
                // There is no strong state reference in this frame. The normal
                // GCHandle alone roots the state before translated code enters it.
                Check.CompactGc();
                Check.Require(root.Witness.IsAlive, "GCHandle roots context before callback entry");
                uint status = Core.ZydisFormatterFormatInstruction(&formatter, &metadata, operandPointer,
                    (byte)instruction.VisibleOperandCount, outputPointer, Capacity, Address, root.Cookie);
                VerifyOutcome(root.Cookie, status, style, behavior);
            }
            Check.Guards(output, Capacity);
        }
        finally
        {
            // Keep the handle valid until both hooks have been removed, including
            // all failure paths. No callback or borrowed buffer escapes this scope.
            try
            {
                Check.Require(Exchange(&formatter, Core.ZydisFormatterFunction_.ZYDIS_FORMATTER_FUNC_PRINT_MNEMONIC,
                    originalMnemonic) == (void*)mnemonicHook, "mnemonic restore returns the exact managed callback");
            }
            finally
            {
                Check.Require(Exchange(&formatter, Core.ZydisFormatterFunction_.ZYDIS_FORMATTER_FUNC_PRINT_REGISTER,
                    originalRegister) == (void*)registerHook, "register restore returns the exact managed callback");
            }
        }

        Check.Require(Query(&formatter, Core.ZydisFormatterFunction_.ZYDIS_FORMATTER_FUNC_PRINT_MNEMONIC) == originalMnemonic
            && Query(&formatter, Core.ZydisFormatterFunction_.ZYDIS_FORMATTER_FUNC_PRINT_REGISTER) == originalRegister,
            "both default callback identities are restored");
        root.Dispose();
        // New pins and no user data: an accidentally retained test hook would fail.
        output.AsSpan().Fill(Check.Canary);
        fixed (Core.ZydisDecodedOperand_* operandPointer = operands)
        fixed (byte* outputPointer = output.AsSpan(Check.GuardLength, Capacity))
        {
            Check.CompactGc();
            uint status = Core.ZydisFormatterFormatInstruction(&formatter, &metadata, operandPointer,
                (byte)instruction.VisibleOperandCount, outputPointer, Capacity, Address, null);
            Check.Require(status == Core.ZYAN_STATUS_SUCCESS, "format after restoration and context release succeeds");
        }
        string expected = style == Core.ZydisFormatterStyle_.ZYDIS_FORMATTER_STYLE_INTEL
            ? "mov rax, rbx" : "mov %rbx, %rax";
        Check.Require(Check.Text(output.AsSpan(Check.GuardLength, Capacity)) == expected,
            "restored formatter emits exact default text with no managed context");
        Check.Guards(output, Capacity);
        return root.Witness;
    }

    private static void* Query(Core.ZydisFormatter_* formatter, Core.ZydisFormatterFunction_ function)
        => Exchange(formatter, function, null);

    private static void* Exchange(Core.ZydisFormatter_* formatter, Core.ZydisFormatterFunction_ function, void* replacement)
    {
        new ZydisStatus(Core.ZydisFormatterSetHook(formatter, function, &replacement))
            .ThrowIfFailed("boundary hook exchange");
        return replacement;
    }

    // These are ordinary managed methods addressed with delegate* managed, as
    // required by the actual emitted API. No native thunk or unmanaged calli is used.
    private static uint PrintMnemonic(Core.ZydisFormatter_* formatter, Core.ZydisFormatterBuffer_* buffer,
        Core.ZydisFormatterContext_* context)
    {
        Check.CompactGc(); // collect before recovering the state from user_data
        var state = State(context->user_data);
        ++state.MnemonicCalls;
        try
        {
            ValidateBorrowedStorage(state, formatter, buffer, context);
            Check.Require(context->operand == null, "mnemonic callback has no current operand");
            if (state.Mode == Behavior.ReturnFailure) return Core.ZYAN_STATUS_OUT_OF_RANGE;
            if (state.Mode == Behavior.ThrowAndContain) throw state.ExpectedException!;

            uint status = state.OriginalMnemonic(formatter, buffer, context);
            if (status != Core.ZYAN_STATUS_SUCCESS) return status;
            Core.ZyanString_* text = null;
            status = Core.ZydisFormatterBufferGetString(buffer, &text);
            if (status != Core.ZYAN_STATUS_SUCCESS) return status;
            byte* data = (byte*)text->vector.data;
            Check.Require(text->vector.size == 4 && data[0] == (byte)'m' && data[1] == (byte)'o' && data[2] == (byte)'v',
                "saved default mnemonic callback actually emits mov");
            // A visible host customization, using the string returned by the
            // upstream buffer API, without retaining any pointer after this call.
            data[0] = (byte)'M';
            data[1] = (byte)'O';
            data[2] = (byte)'V';
            return status;
        }
        catch (Exception error)
        {
            state.CapturedException = error;
            return Core.ZYAN_STATUS_INVALID_OPERATION;
        }
    }

    private static uint PrintRegister(Core.ZydisFormatter_* formatter, Core.ZydisFormatterBuffer_* buffer,
        Core.ZydisFormatterContext_* context, Core.ZydisRegister_ register)
    {
        Check.CompactGc();
        var state = State(context->user_data);
        try
        {
            ValidateBorrowedStorage(state, formatter, buffer, context);
            Check.Require(context->operand == context->operands || context->operand == context->operands + 1,
                "register callback identifies one of the two borrowed operands");
            Check.Require(context->operand->type == Core.ZydisOperandType_.ZYDIS_OPERAND_TYPE_REGISTER
                && context->operand->reg.value == register, "extra register callback argument preserves the emitted enum value");
            state.Registers.Add(register);
            return state.OriginalRegister(formatter, buffer, context, register);
        }
        catch (Exception error)
        {
            state.CapturedException = error;
            return Core.ZYAN_STATUS_INVALID_OPERATION;
        }
    }

    private static void ValidateBorrowedStorage(CallbackState state, Core.ZydisFormatter_* formatter,
        Core.ZydisFormatterBuffer_* buffer, Core.ZydisFormatterContext_* context)
    {
        Check.Require((nint)context->user_data == state.Cookie, "user_data preserves the exact GCHandle cookie");
        Check.Require((nint)formatter == state.Formatter && (nint)context->instruction == state.Instruction,
            "callback receives the caller's formatter and instruction addresses");
        Check.Require(context->instruction->mnemonic == Core.ZydisMnemonic_.ZYDIS_MNEMONIC_MOV
            && context->runtime_address == Address, "instruction and full-width runtime address survive dispatch");
        fixed (Core.ZydisDecodedOperand_* liveOperands = state.Operands)
        fixed (byte* liveOutput = state.Output.AsSpan(Check.GuardLength, Capacity))
        {
            Check.Require(liveOperands == context->operands && (nint)liveOperands == state.OperandAddress,
                "borrowed operand array stays pinned across compacting GC");
            Check.Require((nint)liveOutput == state.OutputAddress && buffer->@string.vector.data == liveOutput,
                "borrowed output slice stays pinned across compacting GC");
            Check.Require(buffer->is_token_list == 0 && buffer->@string.vector.size <= Capacity,
                "callback receives a bounded text buffer");
        }
        ++state.ValidatedEntries;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static RootedContext CreateRoot(Core.ZydisDecodedOperand_[] operands, byte[] output, Behavior behavior)
        => new(new CallbackState(operands, output, behavior));

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Configure(void* cookie, Core.ZydisFormatter_* formatter, Core.ZydisDecodedInstruction_* instruction,
        Core.ZydisDecodedOperand_* operands, byte* output, void* originalMnemonic, void* originalRegister)
    {
        var state = State(cookie);
        state.Formatter = (nint)formatter;
        state.Instruction = (nint)instruction;
        state.OperandAddress = (nint)operands;
        state.OutputAddress = (nint)output;
        state.OriginalMnemonic = (delegate* managed<Core.ZydisFormatter_*, Core.ZydisFormatterBuffer_*, Core.ZydisFormatterContext_*, uint>)originalMnemonic;
        state.OriginalRegister = (delegate* managed<Core.ZydisFormatter_*, Core.ZydisFormatterBuffer_*, Core.ZydisFormatterContext_*, Core.ZydisRegister_, uint>)originalRegister;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void VerifyOutcome(void* cookie, uint status, Core.ZydisFormatterStyle_ style, Behavior behavior)
    {
        var state = State(cookie);
        if (behavior == Behavior.ThrowAndContain)
        {
            Check.Require(ReferenceEquals(state.CapturedException, state.ExpectedException),
                "callback contains and records the exact managed exception instance");
            Check.Require(status == Core.ZYAN_STATUS_INVALID_OPERATION, "contained exception maps to the exact chosen upstream failure");
        }
        else
        {
            if (state.CapturedException is not null)
                throw new InvalidOperationException("Unexpected exception in managed formatter callback.", state.CapturedException);
            Check.Require(status == (behavior == Behavior.Success ? Core.ZYAN_STATUS_SUCCESS : Core.ZYAN_STATUS_OUT_OF_RANGE),
                "translated formatter preserves the exact callback status");
        }
        Check.Require(state.MnemonicCalls == 1, "mnemonic callback invoked exactly once");
        if (behavior == Behavior.Success)
        {
            Core.ZydisRegister_[] expected = style == Core.ZydisFormatterStyle_.ZYDIS_FORMATTER_STYLE_INTEL
                ? [Core.ZydisRegister_.ZYDIS_REGISTER_RAX, Core.ZydisRegister_.ZYDIS_REGISTER_RBX]
                : [Core.ZydisRegister_.ZYDIS_REGISTER_RBX, Core.ZydisRegister_.ZYDIS_REGISTER_RAX];
            Check.Require(state.Registers.SequenceEqual(expected) && state.ValidatedEntries == 3,
                "both register callbacks run in syntax order after GC");
            string text = style == Core.ZydisFormatterStyle_.ZYDIS_FORMATTER_STYLE_INTEL
                ? "MOV rax, rbx" : "MOV %rbx, %rax";
            Check.Require(Check.Text(state.Output.AsSpan(Check.GuardLength, Capacity)) == text,
                "managed hook customization and saved default callbacks produce exact text");
        }
        else
        {
            Check.Require(state.Registers.Count == 0 && state.ValidatedEntries == 1,
                "callback failure stops formatting before any register callback");
        }
    }

    private static CallbackState State(void* cookie)
    {
        Check.Require(cookie != null, "managed callback requires its scoped context");
        return (CallbackState)(GCHandle.FromIntPtr((nint)cookie).Target
            ?? throw new InvalidOperationException("Callback context root is missing."));
    }

    private sealed class CallbackState(Core.ZydisDecodedOperand_[] operands, byte[] output, Behavior behavior)
    {
        public readonly Core.ZydisDecodedOperand_[] Operands = operands;
        public readonly byte[] Output = output;
        public readonly Behavior Mode = behavior;
        public readonly Exception? ExpectedException = behavior == Behavior.ThrowAndContain
            ? new InvalidOperationException("Deliberate contained callback failure.") : null;
        public readonly List<Core.ZydisRegister_> Registers = [];
        public nint Cookie, Formatter, Instruction, OperandAddress, OutputAddress;
        public int MnemonicCalls, ValidatedEntries;
        public Exception? CapturedException;
        public delegate* managed<Core.ZydisFormatter_*, Core.ZydisFormatterBuffer_*, Core.ZydisFormatterContext_*, uint> OriginalMnemonic;
        public delegate* managed<Core.ZydisFormatter_*, Core.ZydisFormatterBuffer_*, Core.ZydisFormatterContext_*, Core.ZydisRegister_, uint> OriginalRegister;
    }

    private sealed class RootedContext : IDisposable
    {
        private GCHandle handle;
        public WeakReference Witness { get; }
        public void* Cookie => (void*)GCHandle.ToIntPtr(handle);

        public RootedContext(CallbackState state)
        {
            handle = GCHandle.Alloc(state, GCHandleType.Normal);
            Witness = new WeakReference(state);
            state.Cookie = (nint)Cookie;
        }

        public void Dispose()
        {
            if (!handle.IsAllocated) return;
            handle.Free();
            handle = default;
        }
    }
}
