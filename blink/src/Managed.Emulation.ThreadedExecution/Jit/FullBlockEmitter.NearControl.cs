namespace Managed.Emulation.Execution;

internal static partial class FullBlockEmitter
{
    internal static bool IsNativeNearControl(JitInstruction i) => i.Operation is
        JitOperation.Jump or JitOperation.ConditionalJump or JitOperation.Call or JitOperation.IndirectJump or
        JitOperation.IndirectCall or JitOperation.Ret or JitOperation.ReturnAdjust or JitOperation.Loop or
        JitOperation.LoopEqual or JitOperation.LoopNotEqual or JitOperation.CounterZeroJump;

    internal static bool CanInlineNearControl(JitInstruction i)
    {
        if (!IsNativeNearControl(i) || i.Length is < 1 or > 15 || i.Destination != default ||
            i.Extra != default || i.Options != 0) return false;
        bool counter = IsNativeCounterBranch(i);
        bool returns = i.Operation is JitOperation.Ret or JitOperation.ReturnAdjust;
        if (i.Width != 64 && !(counter && i.Width == 32) && !(returns && i.Width == 16)) return false;
        if (i.Condition != 0 && (i.Operation != JitOperation.ConditionalJump || i.Condition is < 0 or > 15)) return false;
        if (counter || i.Operation is JitOperation.Jump or JitOperation.ConditionalJump)
        {
            if (i.Source != default || i.SourceWidth != 0 || i.Address != default) return false;
            ulong rel = unchecked(i.Target - (i.Ip + (uint)i.Length));
            if (counter) return i.Length >= (i.Width == 32 ? 3 : 2) &&
                (rel <= 127 || rel >= unchecked(0UL - 128));
            return i.Length >= 2 && (rel <= int.MaxValue || rel >= unchecked(0UL - 2147483648UL));
        }
        if (i.SourceWidth != 0 && i.SourceWidth != i.Width || i.Condition != 0) return false;
        if (i.Operation == JitOperation.Call)
        {
            ulong rel = unchecked(i.Target - (i.Ip + (uint)i.Length));
            return i.Length >= 5 && i.Source == default &&
                (i.Address == default || i.Address == new JitAddress(4, -1, 0, -8)) &&
                (rel <= int.MaxValue || rel >= unchecked(0UL - 2147483648UL));
        }
        if (i.Target != 0) return false;
        if (i.Operation is JitOperation.Ret or JitOperation.ReturnAdjust)
            return (i.Address == default || i.Address == new JitAddress(4, -1, 0, 0)) &&
                (i.Operation == JitOperation.Ret ? i.Source == default :
                i.Length >= 3 && i.Source == new JitOperand(0, i.Source.Immediate, IsImmediate: true) &&
                i.Source.Immediate <= ushort.MaxValue);
        if (!i.Source.IsMemory)
            return i.Length >= 2 && i.Source.Register is >= 0 and <= 15 &&
                i.Source == new JitOperand(i.Source.Register, 0) &&
                (i.Address == default || i.Address == new JitAddress(-1, -1, 0, 0));
        var a = i.Address;
        return i.Length >= 2 && (i.Operation != JitOperation.IndirectCall || i.SourceWidth == 64) &&
            i.Source == new JitOperand(0, 0, IsMemory: true) &&
            a.Base is >= -1 and <= 15 && a.Index is >= -1 and <= 15 && a.Scale is >= 0 and <= 3 &&
            a.AddressWidth is 32 or 64 && a.Segment is >= -1 and <= 5 &&
            (!a.RipRelative || a.Base == -1 && a.Index == -1);
    }

    internal static bool ValidateNearControl(JitInstruction i)
    {
        if (!IsNativeNearControl(i)) return false;
        if (!CanInlineNearControl(i)) throw new ArgumentException("Invalid native near-control IR");
        return true;
    }

    private ref partial struct Writer
    {
        private void NativeNearControl(JitInstruction i)
        {
            ulong next = unchecked(i.Ip + (uint)i.Length);
            if (IsNativeCounterBranch(i)) { NativeNearCounter(i); return; }
            if (i.Operation == JitOperation.ConditionalJump)
            {
                Condition(i.Condition); IntegerRR(0x85, 0, 0, 32);
                int taken = NearCondition(0x85);
                Commit(next); Dispatch(next, 0); Patch(taken, position);
                NativeNearStaticTarget(i.Target); return;
            }
            if (i.Operation == JitOperation.Jump) { NativeNearStaticTarget(i.Target); return; }
            if (i.Operation == JitOperation.IndirectJump)
            {
                if (i.Source.IsMemory)
                {
                    ScalarMiscCheckCanonical(i, 64);
                    NativeMemoryPrepare(i, FullMemoryAccess.Read, 64); NativeMemoryLoad(64);
                }
                else Load(i.Source, 64, 0);
                NativeNearCheckTarget();
                if (i.Source.IsMemory) NativeMemoryCount();
                NativeDynamicCommitAndDispatch(); return;
            }
            if (i.Operation is JitOperation.Ret or JitOperation.ReturnAdjust)
            {
                var read = i with { Address = new(4, -1, 0, 0, Segment: 2) };
                ScalarMiscCheckCanonical(read, i.Width);
                NativeMemoryPrepare(read, FullMemoryAccess.Read, i.Width); NativeMemoryLoad(i.Width);
                NativeNearCheckTarget(); // popped target proven BEFORE RSP adjustment
                IntegerFrameStore(208, 0);
                Bytes(0x49, 0x81, 0x43, 56); U32((uint)(i.Width / 8) + (i.Operation == JitOperation.ReturnAdjust ? (uint)i.Source.Immediate : 0));
                NativeMemoryCount(); IntegerFrameLoad(208, 0); NativeDynamicCommitAndDispatch(); return;
            }
            if (i.Operation == JitOperation.IndirectCall && i.Source.IsMemory)
            { NativeNearMemoryCall(i); return; }
            if (i.Operation == JitOperation.Call) IntegerImmediate(0, i.Target);
            else Load(i.Source, 64, 0); // CALL RSP reads the ORIGINAL RSP
            IntegerFrameStore(208, 0); // original target survives stack prechecks
            int stackBytes = i.Width / 8;
            var push = i with { Address = new(4, -1, 0, -stackBytes, Segment: 2) };
            ScalarMiscCheckCanonical(push, i.Width);
            NativeMemoryPrepare(push, FullMemoryAccess.Write, i.Width);
            IntegerFrameLoad(208, 0); NativeNearCheckTarget();
            IntegerImmediate(8, next); NativeMemoryStore(i.Width);
            Bytes(0x49, 0x83, 0x6b, 56, (byte)stackBytes); NativeMemoryCount();
            if (i.Operation == JitOperation.Call) { Commit(i.Target); Dispatch(i.Target, 1); }
            else { IntegerFrameLoad(208, 0); NativeDynamicCommitAndDispatch(); }
        }

        // RAX target is preserved. No host indirect branch is ever formed from
        // guest target bits. The existing authenticated cache consumes them only
        // after architectural commit. Fault leaves IP/oplen/registers untouched.
        private void NativeNearCheckTarget()
        {
            IntegerMov(1, 0); IntegerShift(4, 1, 64, 16); IntegerShift(7, 1, 64, 16);
            IntegerRR(0x39, 0, 1); int good = NearCondition(0x84);
            NativeNearTargetFault(); Patch(good, position);
        }
        private void NativeNearTargetFault()
        {
            Bytes(0x41, 0xc7, 0x82); U32(168); U32(13);
            Bytes(0x41, 0xc7, 0x82); U32(172); U32(128);
            Bytes(0x49, 0xc7, 0x82); U32(176); U32(0);
            JumpExit(FullBlockExit.GuestFault);
        }
        private void NativeNearStaticTarget(ulong target)
        {
            if (!FullControlFlagLowering.CanonicalTarget(target)) NativeNearTargetFault();
            else { Commit(target); Dispatch(target, 1); }
        }

        private void NativeNearCounter(JitInstruction i)
        {
            IntegerLoad(new(1, 0), i.Width, 8);
            bool zero = i.Operation == JitOperation.CounterZeroJump;
            if (!zero) IntegerConstant(5, 8, 1, i.Width);
            IntegerRR(0x85, 8, 8, i.Width);
            int fall = NearCondition(zero ? (byte)0x85 : (byte)0x84);
            int zfFall = -1;
            if (i.Operation is JitOperation.LoopEqual or JitOperation.LoopNotEqual)
            {
                Bytes(0x41, 0xf6, 0x43, 12, 0x40);
                zfFall = NearCondition(i.Operation == JitOperation.LoopEqual ? (byte)0x84 : (byte)0x85);
            }
            // Fault-class #GP restarts the operation: defer guest counter write
            // until the taken target is proven. SDM7.5/7.6; CPU rows unavailable.
            if (!FullControlFlagLowering.CanonicalTarget(i.Target)) NativeNearTargetFault();
            else
            {
                if (!zero) Store(new(1, 0), i.Width);
                Commit(i.Target); Dispatch(i.Target, 1);
            }
            Patch(fall, position); if (zfFall >= 0) Patch(zfFall, position);
            if (!zero) Store(new(1, 0), i.Width);
            ulong next = unchecked(i.Ip + (uint)i.Length);
            Commit(next); Dispatch(next, 0);
        }

        // The existing ordered pair resolves SOURCE then STACK before loading
        // target. Parent AMD matrix v2 proves stack permission/canonical faults
        // precede target #GP. No return write/RSP change until target is proven.
        private void NativeNearMemoryCall(JitInstruction i)
        {
            ScalarMiscCheckCanonical(i, 64);
            NativeStackMemoryPrepare(i);
            NativeStackMemoryAdopt(32); NativeMemoryLoad(64);
            NativeNearCheckTarget(); IntegerFrameStore(208, 0);
            NativeStackMemoryAdopt(80);
            IntegerImmediate(8, unchecked(i.Ip + (uint)i.Length)); NativeMemoryStore(64);
            IntegerFrameLoad(200, 0); Bytes(0x49, 0x89, 0x43, 56);
            NativeMemoryCount(); IntegerFrameLoad(208, 0); NativeDynamicCommitAndDispatch();
        }
    }
}
