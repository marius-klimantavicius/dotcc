using System.Runtime.InteropServices;
using System.Text;
using Raw = Managed.Zydis.Tests.ZydisQualification;

internal static unsafe class ManagedHooks
{
    private sealed class State
    {
        internal int Calls;
        internal bool Throw;
        internal Exception? Caught;
    }

    private static uint Hook(Raw.ZydisFormatter_* formatter, Raw.ZydisFormatterBuffer_* buffer,
        Raw.ZydisFormatterContext_* context)
    {
        State? state = null;
        try
        {
            if (context == null || context->user_data == null) throw new InvalidOperationException("Missing callback context");
            state = (State?)GCHandle.FromIntPtr((nint)context->user_data).Target
                ?? throw new InvalidOperationException("Unrooted callback context");
            ++state.Calls;
            if (state.Throw) throw new InvalidOperationException("Deliberate contained formatter callback failure");
            if (formatter == null || buffer == null || context->instruction == null || context->runtime_address != 0x4000)
                throw new InvalidOperationException("Incorrect actual formatter callback ABI");
            return Raw.ZYAN_STATUS_SUCCESS;
        }
        catch (Exception error)
        {
            if (state != null) state.Caught = error;
            return Raw.ZYAN_STATUS_INVALID_OPERATION;
        }
    }

    private static void Success(uint status)
    {
        if ((status & 0x80000000u) != 0) throw new InvalidOperationException($"Formatter callback probe failed: {status:X8}");
    }

    internal static void Check()
    {
        Raw.ZydisDecoder_ decoder = default;
        Raw.ZydisDecodedInstruction_ insn = default;
        Raw.ZydisDecodedOperand_* operands = stackalloc Raw.ZydisDecodedOperand_[Raw.ZYDIS_MAX_OPERAND_COUNT];
        byte* input = stackalloc byte[] { 0x48, 0x89, 0xD8 };
        byte* output = stackalloc byte[256];
        Raw.ZydisFormatter_ formatter = default;
        Success(Raw.ZydisDecoderInit(&decoder, Raw.ZydisMachineMode_.ZYDIS_MACHINE_MODE_LONG_64, Raw.ZydisStackWidth_.ZYDIS_STACK_WIDTH_64));
        Success(Raw.ZydisDecoderDecodeFull(&decoder, input, 3, &insn, operands));
        Success(Raw.ZydisFormatterInit(&formatter, Raw.ZydisFormatterStyle_.ZYDIS_FORMATTER_STYLE_INTEL));
        delegate*<Raw.ZydisFormatter_*, Raw.ZydisFormatterBuffer_*, Raw.ZydisFormatterContext_*, uint> callback = &Hook;
        void* previous = (void*)callback;
        Success(Raw.ZydisFormatterSetHook(&formatter, Raw.ZydisFormatterFunction_.ZYDIS_FORMATTER_FUNC_PRE_INSTRUCTION, &previous));
        // The default pre-instruction hook is null. Querying preserves the hook.
        if (previous != null) throw new InvalidOperationException("Unexpected original pre-instruction hook");
        void* query = null;
        Success(Raw.ZydisFormatterSetHook(&formatter, Raw.ZydisFormatterFunction_.ZYDIS_FORMATTER_FUNC_PRE_INSTRUCTION, &query));
        if (query != (void*)callback) throw new InvalidOperationException("Managed callback function identity changed");
        var first = new State(); var second = new State();
        var one = GCHandle.Alloc(first); var two = GCHandle.Alloc(second);
        try
        {
            foreach (var handle in new[] { one, two })
            {
                GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
                Success(Raw.ZydisFormatterFormatInstruction(&formatter, &insn, operands,
                    insn.operand_count_visible, output, 256, 0x4000, (void*)GCHandle.ToIntPtr(handle)));
                if (Encoding.ASCII.GetString(new ReadOnlySpan<byte>(output, 12)) != "mov rax, rbx")
                    throw new InvalidOperationException("Formatter output differs after managed hook");
            }
            if (first.Calls != 1 || second.Calls != 1 || first.Caught != null || second.Caught != null)
                throw new InvalidOperationException("Rooted formatter contexts leaked between calls");
            first.Throw = true;
            uint failure = Raw.ZydisFormatterFormatInstruction(&formatter, &insn, operands,
                insn.operand_count_visible, output, 256, 0x4000, (void*)GCHandle.ToIntPtr(one));
            if (failure != Raw.ZYAN_STATUS_INVALID_OPERATION || first.Caught == null || first.Calls != 2)
                throw new InvalidOperationException("Callback exception was not contained at the C boundary");
            // Remove the hook through a fresh formatter before releasing roots.
            Success(Raw.ZydisFormatterInit(&formatter, Raw.ZydisFormatterStyle_.ZYDIS_FORMATTER_STYLE_INTEL));
        }
        finally { one.Free(); two.Free(); }
        Success(Raw.ZydisFormatterFormatInstruction(&formatter, &insn, operands,
            insn.operand_count_visible, output, 256, 0x4000, null));
        GC.KeepAlive(first); GC.KeepAlive(second);
    }
}
