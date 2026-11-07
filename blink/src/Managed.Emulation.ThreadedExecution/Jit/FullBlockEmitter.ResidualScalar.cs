namespace Managed.Emulation.Execution;

internal static partial class FullBlockEmitter
{
    internal static bool CanInlineResidualScalar(JitInstruction i) =>
        i.Operation == JitOperation.SystemState && i.Options is >= FullResidualScalarLowering.Salc and <= FullResidualScalarLowering.CacheWritebackFence;

    internal static bool ValidateResidualScalar(JitInstruction i)
    {
        if (!CanInlineResidualScalar(i)) return false;
        bool salc = i.Options == FullResidualScalarLowering.Salc;
        int width = salc ? 8 : i.Options == FullResidualScalarLowering.CacheWritebackFence ? 64 : 32;
        if (i.Length is < 1 or > 15 || i.Width != width || i.Destination != (salc ? new JitOperand(0, 0) : default) ||
            i.Source != default || i.Extra != default || i.Address != default || i.Target != 0 ||
            i.Condition != 0 || i.SourceWidth != 0)
            throw new ArgumentException("Native selected residual scalar canonical metadata");
        return true;
    }

    private ref partial struct Writer
    {
        private bool TryResidualScalarOperation(in JitInstruction i)
        {
            if (!CanInlineResidualScalar(i)) return false;
            ValidateResidualScalar(i);
            if (i.Options == FullResidualScalarLowering.ClearInterrupt)
            {
                Bytes(0x41, 0x81, 0x63, 12); U32(~(1u << 9)); // guest IF only
                return true;
            }
            if (i.Options == FullResidualScalarLowering.SetInterrupt)
            {
                Bytes(0x41, 0x81, 0x4b, 12); U32(1u << 9); // guest IF only
                return true;
            }
            if (i.Options == FullResidualScalarLowering.CacheWritebackFence)
            {
                Bytes(0x0f, 0xae, 0xf0); // selected OpMfence ordering, baseline SSE2
                return true;
            }
            // r8d = -(guest software CF &1); Store writes AL alone. Host flags
            // are scratch; guest lazy parity and every flag bit stay untouched.
            Bytes(0x45, 0x8b, 0x43, 12);
            IntegerConstant(4, 8, 1);
            Bytes(0x41, 0xf7, 0xd8);
            Store(i.Destination, 8);
            return true; // common scaffold owns issue/polls/one commit
        }
    }
}
