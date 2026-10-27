namespace Managed.Emulation.Execution;

// Preparation only: no guest memory reads/writes, page commitment, flags
// evaluation, counter changes or execution. Native guards remain authoritative.
internal sealed class JitAddressPlan
{
    internal const int MaximumInstructions = 32, MaximumValues = 512, MaximumDefinitions = 80;
    internal const int MaximumAffineVisits = 128, MaximumAffineDepth = 32;
    // Selected x64 layout: object <=64 bytes; five array headers <=24 each.
    // Explicit element sizes are 16/64/8/32/16. Charge includes full retained
    // value/effect IR AND its separately compacted preparation program.
    internal const long MaximumCharge = 64 + 5 * 24 + 2 * MaximumValues * 16 +
        MaximumInstructions * 64 + MaximumDefinitions * 8 + MaximumInstructions * 32; // 20,280
    internal const int MaximumEvaluationScratchBytes = MaximumValues * sizeof(ulong); // 4,096
    internal static readonly JitAddressPlan Empty = new([], [], [], [], [], false, 0);

    private readonly JitPlanValue[] values;
    private readonly JitPlanInstruction[] instructions;
    private readonly JitPlanDefinition[] definitions;
    private readonly JitPlanMemory[] memory;
    private readonly JitPlanValue[] evaluation;

    private JitAddressPlan(JitPlanValue[] values, JitPlanInstruction[] instructions,
        JitPlanDefinition[] definitions, JitPlanMemory[] memory, JitPlanValue[] evaluation, bool backedge, int fastCount)
    {
        this.values = values; this.instructions = instructions; this.definitions = definitions;
        this.memory = memory; this.evaluation = evaluation; HasInternalBackedge = backedge;
        FastCount = fastCount;
    }

    internal bool HasMemory => memory.Length != 0; // Includes unknown-derived addresses.
    internal bool HasInternalBackedge { get; }
    internal int EvaluationCount => evaluation.Length;
    internal int FastCount { get; }
    internal ReadOnlySpan<JitPlanValue> Values => values;
    internal ReadOnlySpan<JitPlanInstruction> Instructions => instructions;
    internal ReadOnlySpan<JitPlanDefinition> Definitions => definitions;
    internal ReadOnlySpan<JitPlanMemory> Memory => memory;
    internal long Charge => 64 + ArrayCharge(values.Length, 16) + ArrayCharge(instructions.Length, 64) +
        ArrayCharge(definitions.Length, 8) + ArrayCharge(memory.Length, 32) + ArrayCharge(evaluation.Length, 16);
    private static long ArrayCharge(int count, int size) => count == 0 ? 0 : 24L + (long)count * size;

    internal static JitAddressPlan Build(ReadOnlySpan<JitInstruction> source)
    {
        if (source.Length > MaximumInstructions) throw new ArgumentOutOfRangeException(nameof(source));
        if (source.IsEmpty) return Empty;
        // Stack-only compilation scratch. Final publication allocates at most
        // five exact-length arrays and this object, never a per-node object.
        Span<JitPlanValue> values = stackalloc JitPlanValue[MaximumValues];
        Span<JitPlanInstruction> steps = stackalloc JitPlanInstruction[MaximumInstructions];
        Span<JitPlanDefinition> definitions = stackalloc JitPlanDefinition[MaximumDefinitions];
        Span<JitPlanMemory> memory = stackalloc JitPlanMemory[MaximumInstructions];
        Span<ushort> registers = stackalloc ushort[16];
        var builder = new Builder(values, steps, definitions, memory, registers);
        int count = source.Length;
        for (int i = 0; i < count; ++i)
            if (source[i].Operation is JitOperation.Jump or JitOperation.Call or JitOperation.Ret)
            { count = i + 1; break; }
        source = source[..count]; // No recipes after an unconditional external exit.

        int loopStart = -1;
        if (source[^1].Operation is JitOperation.Jump or JitOperation.ConditionalJump)
            for (int i = 0; i < count; ++i)
                if (source[i].Ip == source[^1].Target) { loopStart = i; break; }
        uint loopWrites = 0;
        bool loopFlags = false;
        if (loopStart >= 0)
            for (int i = loopStart; i < count; ++i)
            {
                loopWrites |= WrittenRegisters(source[i]);
                loopFlags |= FlagsEffect(source[i].Operation) != JitPlanFlagsEffect.Preserve;
            }
        for (int i = 0; i < count; ++i)
            builder.Append(source[i], i, i == loopStart ? loopWrites : 0, i == loopStart && loopFlags,
                i == count - 1 ? loopStart : -1);

        // Full value/effect IR remains intact. Prove bounded direct forms before
        // appending only dependencies of remaining known memory recipes, in
        // recipe order. EvaluationEnd lets Fill stop without evaluating tail
        // recipes after it has authenticated its four descriptors.
        Span<JitPlanValue> evaluation = stackalloc JitPlanValue[MaximumValues];
        Span<ushort> map = stackalloc ushort[MaximumValues];
        Span<byte> needed = stackalloc byte[MaximumValues];
        Span<uint> affinePending = stackalloc uint[MaximumAffineDepth];
        map.Clear();
        int evaluationCount = 0, fastCount = 0;
        for (int m = 0; m < builder.MemoryCount; ++m)
        {
            ref var recipe = ref memory[m];
            if (values[recipe.Value].Known == 0) continue;
            if (TrySetAffineRecipe(values, ref recipe, affinePending)) { ++fastCount; continue; }
            if (evaluationCount == 0)
                evaluation[evaluationCount++] = new() { Kind = JitPlanValueKind.Constant, Width = 64, Known = 1 };
            needed.Clear(); needed[recipe.Value] = 1;
            for (int id = recipe.Value; id > 0; --id)
            {
                if (needed[id] == 0 || map[id] != 0) continue;
                var node = values[id];
                needed[node.Left] = 1; needed[node.Right] = 1;
            }
            for (int id = 1; id <= recipe.Value; ++id)
            {
                if (needed[id] == 0 || map[id] != 0) continue;
                var node = values[id];
                node.Left = map[node.Left]; node.Right = map[node.Right];
                map[id] = (ushort)evaluationCount;
                evaluation[evaluationCount++] = node;
            }
            recipe.EvaluationValue = map[recipe.Value];
            recipe.EvaluationEnd = (ushort)evaluationCount;
        }
        return new(values[..builder.ValueCount].ToArray(), steps[..count].ToArray(),
            definitions[..builder.DefinitionCount].ToArray(), memory[..builder.MemoryCount].ToArray(),
            evaluation[..evaluationCount].ToArray(), loopStart >= 0, fastCount);
    }

    // Compile-only proof over the existing DAG. Each pending uint packs a value
    // ID, depth and accumulated shift. Revisited shared nodes consume the same
    // 128-inspection budget; there is no dictionary, recursion or allocation.
    // At most two distinct entry terms accumulate modulo-64 coefficients. The
    // final form needs one unscaled base and/or one power-of-two index.
    private static bool TrySetAffineRecipe(ReadOnlySpan<JitPlanValue> values,
        ref JitPlanMemory recipe, Span<uint> pending)
    {
        int count = 1, visits = 0, first = -1, second = -1;
        ulong firstCoefficient = 0, secondCoefficient = 0, displacement = 0;
        pending[0] = recipe.Value | 1u << 16;
        unchecked
        {
            while (count != 0)
            {
                if (visits == MaximumAffineVisits) return false;
                ++visits;
                uint item = pending[--count];
                int depth = (byte)(item >> 16), shift = (byte)(item >> 24);
                var node = values[(ushort)item];
                if (node.Known == 0 || node.Width != 64) return false;
                switch (node.Kind)
                {
                    case JitPlanValueKind.EntryRegister:
                        int register = (int)node.Immediate;
                        if (first == -1 || first == register)
                        { first = register; firstCoefficient += 1UL << shift; }
                        else if (second == -1 || second == register)
                        { second = register; secondCoefficient += 1UL << shift; }
                        else return false;
                        continue;
                    case JitPlanValueKind.Constant:
                        displacement += node.Immediate << shift;
                        continue;
                    case JitPlanValueKind.Sub:
                        if (visits == MaximumAffineVisits) return false;
                        ++visits; // The right constant is also an inspection.
                        var right = values[node.Right];
                        if (right.Kind != JitPlanValueKind.Constant || right.Width != 64 || right.Known == 0) return false;
                        displacement -= right.Immediate << shift;
                        break;
                    case JitPlanValueKind.Add:
                        if (depth == MaximumAffineDepth || count + 2 > pending.Length) return false;
                        pending[count++] = node.Right | (uint)(depth + 1) << 16 | (uint)shift << 24;
                        break;
                    case JitPlanValueKind.ShiftLeft:
                        // Larger combined shifts could be simplified, but must
                        // never accidentally use C#'s masked shift count.
                        if (node.Shift >= 64 || shift + node.Shift >= 64) return false;
                        shift += node.Shift;
                        break;
                    default: return false; // No Extract/Insert/SignExtend proof.
                }
                if (depth == MaximumAffineDepth || count == pending.Length) return false;
                pending[count++] = node.Left | (uint)(depth + 1) << 16 | (uint)shift << 24;
            }
        }
        if (firstCoefficient == 0) { first = second; firstCoefficient = secondCoefficient; secondCoefficient = 0; }
        if (firstCoefficient != 0 && !IsPowerOfTwo(firstCoefficient) ||
            secondCoefficient != 0 && !IsPowerOfTwo(secondCoefficient)) return false;
        if (secondCoefficient != 0 && firstCoefficient != 1 && secondCoefficient != 1) return false;
        int @base = -1, index = -1, scale = 0;
        if (firstCoefficient == 1)
        {
            @base = first;
            if (secondCoefficient != 0)
            { index = second; scale = System.Numerics.BitOperations.TrailingZeroCount(secondCoefficient); }
        }
        else
        {
            if (secondCoefficient == 1) @base = second;
            if (firstCoefficient != 0)
            { index = first; scale = System.Numerics.BitOperations.TrailingZeroCount(firstCoefficient); }
        }
        recipe.RecipeKind = JitPlanRecipeKind.Affine;
        recipe.Base = (sbyte)@base; recipe.Index = (sbyte)index;
        recipe.Scale = (byte)scale; recipe.Displacement = displacement;
        return true;
    }
    private static bool IsPowerOfTwo(ulong value) => (value & (value - 1)) == 0;

    // Same invocation, entry register span and scratch must be retained across
    // calls. Initialize evaluated=0 once. No recursion or allocation; each live
    // node evaluates at most once and unknown recipes evaluate no nodes.
    internal bool TryEvaluateAddress(int memoryIndex, ReadOnlySpan<ulong> entryRegisters,
        Span<ulong> scratch, ref int evaluated, out ulong address)
    {
        address = 0;
        ref readonly var recipe = ref memory[memoryIndex];
        if (recipe.RecipeKind == JitPlanRecipeKind.Affine)
        {
            if (entryRegisters.Length < 16 || scratch.Length < evaluation.Length || (uint)evaluated > (uint)evaluation.Length)
                throw new ArgumentException("Invalid address-plan evaluation scratch");
            unchecked
            {
                address = recipe.Displacement;
                if (recipe.Base >= 0) address += entryRegisters[recipe.Base];
                if (recipe.Index >= 0) address += entryRegisters[recipe.Index] << recipe.Scale;
            }
            return true;
        }
        if (recipe.EvaluationValue == 0) return false;
        if (entryRegisters.Length < 16 || scratch.Length < evaluation.Length || (uint)evaluated > (uint)evaluation.Length)
            throw new ArgumentException("Invalid address-plan evaluation scratch");
        if (evaluated == 0) scratch[0] = 0;
        while (evaluated < recipe.EvaluationEnd)
        {
            var node = evaluation[evaluated];
            scratch[evaluated] = node.Kind == JitPlanValueKind.EntryRegister ? entryRegisters[(int)node.Immediate] :
                Evaluate(node, scratch[node.Left], scratch[node.Right]);
            ++evaluated;
        }
        address = scratch[recipe.EvaluationValue];
        return true;
    }

    private static ulong Mask(int width) => width == 64 ? ulong.MaxValue : (1UL << width) - 1;
    private static ulong Evaluate(JitPlanValue node, ulong left, ulong right)
    {
        unchecked
        {
            return node.Kind switch
            {
                JitPlanValueKind.Constant => node.Immediate,
                JitPlanValueKind.Add => (left + right) & Mask(node.Width),
                JitPlanValueKind.Sub => (left - right) & Mask(node.Width),
                JitPlanValueKind.And => (left & right) & Mask(node.Width),
                JitPlanValueKind.Or => (left | right) & Mask(node.Width),
                JitPlanValueKind.Xor => (left ^ right) & Mask(node.Width),
                JitPlanValueKind.Extract => (left >> node.Shift) & Mask(node.Width),
                JitPlanValueKind.Insert => (left & ~(Mask(node.Width) << node.Shift)) | ((right & Mask(node.Width)) << node.Shift),
                JitPlanValueKind.SignExtend => (ulong)((long)(left << (64 - node.Width)) >> (64 - node.Width)),
                JitPlanValueKind.ShiftLeft => left << node.Shift,
                _ => 0 // Unknown nodes are never part of the preparation program.
            };
        }
    }

    private static uint WrittenRegisters(JitInstruction instruction) => instruction.Operation switch
    {
        JitOperation.Mov or JitOperation.Movzx or JitOperation.Movsx or JitOperation.Lea or
        JitOperation.Add or JitOperation.Sub or JitOperation.And or JitOperation.Or or JitOperation.Xor or
        JitOperation.Inc or JitOperation.Dec or JitOperation.LoadMov or JitOperation.LoadMovzx => RegisterMask(instruction.Destination),
        JitOperation.Pop => RegisterMask(instruction.Destination) | 1u << 4,
        JitOperation.Push or JitOperation.Call or JitOperation.Ret => 1u << 4,
        _ => 0
    };
    private static uint RegisterMask(JitOperand operand) => operand.Register is >= 0 and < 16 && !operand.IsImmediate ?
        1u << operand.Register : throw new ArgumentException("Invalid address-plan destination");
    private static JitPlanFlagsEffect FlagsEffect(JitOperation operation) => operation switch
    {
        JitOperation.Add or JitOperation.Sub or JitOperation.Cmp or JitOperation.MemoryCmp or JitOperation.MemoryCmpReverse => JitPlanFlagsEffect.Arithmetic,
        JitOperation.And or JitOperation.Or or JitOperation.Xor or JitOperation.Test or JitOperation.MemoryTest => JitPlanFlagsEffect.Logical,
        JitOperation.Inc => JitPlanFlagsEffect.IncPreserveCarry,
        JitOperation.Dec => JitPlanFlagsEffect.DecPreserveCarry,
        _ => JitPlanFlagsEffect.Preserve
    };

    private ref struct Builder
    {
        private readonly Span<JitPlanValue> values;
        private readonly Span<JitPlanInstruction> steps;
        private readonly Span<JitPlanDefinition> definitions;
        private readonly Span<JitPlanMemory> memory;
        private readonly Span<ushort> registers;
        private ushort flags;
        internal int ValueCount { get; private set; }
        internal int DefinitionCount { get; private set; }
        internal int MemoryCount { get; private set; }

        internal Builder(Span<JitPlanValue> values, Span<JitPlanInstruction> steps,
            Span<JitPlanDefinition> definitions, Span<JitPlanMemory> memory, Span<ushort> registers)
        {
            this.values = values; this.steps = steps; this.definitions = definitions;
            this.memory = memory; this.registers = registers;
            ValueCount = 1; DefinitionCount = MemoryCount = 0; flags = 0;
            values[0] = default;
            for (int r = 0; r < 16; ++r)
                registers[r] = Node(new() { Kind = JitPlanValueKind.EntryRegister, Width = 64, Immediate = (uint)r, Known = 1 });
        }

        private ushort Node(JitPlanValue node)
        {
            // Structural sharing preserves repeated slice identity (including
            // XOR eax,eax after a load) without conflating distinct reads or
            // loop-carried values. The scan is bounded by MaximumValues.
            if (node.Kind is not (JitPlanValueKind.MemoryRead or JitPlanValueKind.LoopInput))
                for (int i = 1; i < ValueCount; ++i)
                {
                    var old = values[i];
                    if (old.Kind == node.Kind && old.Immediate == node.Immediate && old.Left == node.Left &&
                        old.Right == node.Right && old.Width == node.Width && old.Shift == node.Shift && old.Known == node.Known)
                        return (ushort)i;
                }
            // Exhaustion degrades a value to unknown, never to an old recipe.
            // Current operations need <=8 nodes/instruction, plus entry/merge
            // values; 512 comfortably covers every admitted 32-instruction IR.
            if (ValueCount == MaximumValues) return 0;
            values[ValueCount] = node; return (ushort)ValueCount++;
        }
        private ushort Constant(ulong value) => Node(new() { Kind = JitPlanValueKind.Constant, Immediate = value, Width = 64, Known = 1 });
        private bool IsConstant(ushort id, ulong value) => values[id].Kind == JitPlanValueKind.Constant && values[id].Immediate == value;

        private ushort Unary(JitPlanValueKind kind, ushort left, int width = 64, int shift = 0)
        {
            if (kind == JitPlanValueKind.Extract)
            {
                if (width == 64 && shift == 0) return left;
                var input = values[left];
                if (input.Kind == JitPlanValueKind.Insert)
                {
                    if (shift >= input.Shift && shift + width <= input.Shift + input.Width)
                        return Unary(kind, input.Right, width, shift - input.Shift);
                    if (shift + width <= input.Shift || shift >= input.Shift + input.Width)
                        return Unary(kind, input.Left, width, shift);
                }
                if (input.Kind == JitPlanValueKind.Extract && shift + width <= input.Width)
                    return Unary(kind, input.Left, width, shift + input.Shift);
            }
            var node = new JitPlanValue { Kind = kind, Left = left, Width = (byte)width, Shift = (byte)shift, Known = values[left].Known };
            if (values[left].Kind == JitPlanValueKind.Constant) return Constant(Evaluate(node, values[left].Immediate, 0));
            return Node(node);
        }

        private ushort Binary(JitPlanValueKind kind, ushort left, ushort right, int width = 64, int shift = 0)
        {
            if (left != 0 && left == right && kind is JitPlanValueKind.Sub or JitPlanValueKind.Xor) return Constant(0);
            if (kind == JitPlanValueKind.And && (IsConstant(left, 0) || IsConstant(right, 0))) return Constant(0);
            if (kind is JitPlanValueKind.Add or JitPlanValueKind.Sub or JitPlanValueKind.Or or JitPlanValueKind.Xor && IsConstant(right, 0))
                return Unary(JitPlanValueKind.Extract, left, width);
            var node = new JitPlanValue { Kind = kind, Left = left, Right = right, Width = (byte)width,
                Shift = (byte)shift, Known = (byte)(values[left].Known & values[right].Known) };
            if (values[left].Kind == JitPlanValueKind.Constant && values[right].Kind == JitPlanValueKind.Constant)
                return Constant(Evaluate(node, values[left].Immediate, values[right].Immediate));
            return Node(node);
        }

        private ushort Read(JitOperand operand, int width)
        {
            if (operand.IsImmediate) return Constant(operand.Immediate & Mask(width));
            ValidateRegister(operand, width);
            return Unary(JitPlanValueKind.Extract, registers[operand.Register], width, operand.ByteOffset * 8);
        }
        private static void ValidateRegister(JitOperand operand, int width)
        {
            if (operand.IsImmediate || operand.Register is < 0 or > 15 || operand.ByteOffset is < 0 or > 1 ||
                operand.ByteOffset != 0 && (width != 8 || operand.Register > 3))
                throw new ArgumentException("Invalid address-plan register");
        }
        private void Define(int register, ushort value, bool merge = false)
        {
            definitions[DefinitionCount++] = new() { Register = (byte)register, Before = registers[register], After = value,
                IsLoopMerge = merge ? (byte)1 : (byte)0 };
            registers[register] = value;
        }
        private void Write(JitOperand operand, int width, ushort value)
        {
            ValidateRegister(operand, width);
            ushort whole = width >= 32 ? Unary(JitPlanValueKind.Extract, value, width) :
                Binary(JitPlanValueKind.Insert, registers[operand.Register], value, width, operand.ByteOffset * 8);
            Define(operand.Register, whole);
        }
        private ushort Address(JitInstruction instruction)
        {
            var address = instruction.Address;
            if (address.Base is < -1 or > 15 || address.Index is < -1 or > 15 || address.Scale is < 0 or > 3 ||
                address.RipRelative && (address.Base != -1 || address.Index != -1))
                throw new ArgumentException("Invalid address-plan address");
            ushort value = Constant(unchecked((ulong)address.Displacement +
                (address.RipRelative ? instruction.Ip + (uint)instruction.Length : 0)));
            if (address.Base >= 0) value = Binary(JitPlanValueKind.Add, registers[address.Base], value);
            if (address.Index >= 0)
            {
                ushort index = registers[address.Index];
                if (address.Scale != 0) index = Unary(JitPlanValueKind.ShiftLeft, index, shift: address.Scale);
                value = Binary(JitPlanValueKind.Add, value, index);
            }
            return value;
        }

        internal void Append(JitInstruction instruction, int index, uint loopWrites, bool mergeFlags, int internalTarget)
        {
            if (instruction.Length is < 1 or > 15 || instruction.Width is not (8 or 16 or 32 or 64))
                throw new ArgumentException("Invalid address-plan instruction");
            JitPlanInstruction step = new() { Ip = instruction.Ip, FallThrough = unchecked(instruction.Ip + (uint)instruction.Length),
                Target = instruction.Target, Operation = instruction.Operation, Width = (byte)instruction.Width,
                SourceWidth = (byte)instruction.SourceWidth, Condition = (byte)instruction.Condition,
                FirstDefinition = (ushort)DefinitionCount, InternalTarget = (short)internalTarget, MemoryIndex = -1,
                Effects = JitPlanEffects.Poll | JitPlanEffects.AtomicIssue | JitPlanEffects.MaterializedCommit | JitPlanEffects.AtomicComplete,
                FlagsEffect = FlagsEffect(instruction.Operation) };
            // Loop-carried writes cannot be expressed as entry values. Kill
            // them at the header, before ANY address recipe or state boundary.
            // Constants/invariant-derived overwrites within the loop recover
            // precision, but no recipe assumes the first iteration's values.
            for (int r = 0; r < 16; ++r)
                if ((loopWrites & (1u << r)) != 0)
                    Define(r, Node(new() { Kind = JitPlanValueKind.LoopInput, Width = 64 }), merge: true);
            step.MergeCount = (ushort)(DefinitionCount - step.FirstDefinition);
            if (mergeFlags) { flags = 33; step.MergesFlags = 1; }
            step.FlagsBefore = flags;

            var operation = instruction.Operation;
            bool read = operation is JitOperation.LoadMov or JitOperation.LoadMovzx or JitOperation.Pop or JitOperation.Ret or
                JitOperation.MemoryCmp or JitOperation.MemoryCmpReverse or JitOperation.MemoryTest;
            bool write = operation is JitOperation.StoreMov or JitOperation.Push or JitOperation.Call;
            if (read || write)
            {
                if (instruction.SourceWidth is not (8 or 16 or 32 or 64)) throw new ArgumentException("Invalid address-plan memory width");
                step.MemoryIndex = (short)MemoryCount;
                memory[MemoryCount++] = new() { Value = Address(instruction), Width = (byte)(instruction.SourceWidth / 8),
                    Access = write ? JitMemoryAccess.Write : JitMemoryAccess.Read, InstructionIndex = (byte)index };
                step.Effects |= JitPlanEffects.Guard | (read ? JitPlanEffects.MemoryRead : JitPlanEffects.MemoryWrite);
                if (read) step.MemoryValue = Node(new() { Kind = JitPlanValueKind.MemoryRead, Width = (byte)instruction.SourceWidth });
            }
            switch (operation)
            {
                case JitOperation.Mov:
                    step.Result = Read(instruction.Source, instruction.Width);
                    Write(instruction.Destination, instruction.Width, step.Result); break;
                case JitOperation.Lea:
                    step.Result = Address(instruction);
                    Write(instruction.Destination, instruction.Width, step.Result); break;
                case JitOperation.Movzx:
                case JitOperation.Movsx:
                    if (instruction.SourceWidth is not (8 or 16 or 32)) throw new ArgumentException("Invalid address-plan extension width");
                    step.Left = Read(instruction.Source, instruction.SourceWidth);
                    step.Result = operation == JitOperation.Movsx ? Unary(JitPlanValueKind.SignExtend, step.Left, instruction.SourceWidth) : step.Left;
                    Write(instruction.Destination, instruction.Width, step.Result); break;
                case JitOperation.LoadMov:
                case JitOperation.LoadMovzx:
                    step.Result = step.MemoryValue;
                    Write(instruction.Destination, instruction.Width, step.Result); break;
                case JitOperation.StoreMov:
                    step.MemoryValue = Read(instruction.Source, instruction.Width); break;
                case JitOperation.Push:
                case JitOperation.Call:
                    step.MemoryValue = operation == JitOperation.Call ? Constant(step.FallThrough) : Read(instruction.Source, 64);
                    step.Result = Binary(JitPlanValueKind.Sub, registers[4], Constant(8));
                    Define(4, step.Result); break;
                case JitOperation.Pop:
                case JitOperation.Ret:
                    step.Result = Binary(JitPlanValueKind.Add, registers[4], Constant(8));
                    Define(4, step.Result);
                    if (operation == JitOperation.Pop) Write(instruction.Destination, 64, step.MemoryValue);
                    else step.NextIpValue = step.MemoryValue;
                    break;
                case JitOperation.Add: case JitOperation.Sub: case JitOperation.Cmp:
                case JitOperation.And: case JitOperation.Or: case JitOperation.Xor: case JitOperation.Test:
                case JitOperation.Inc: case JitOperation.Dec:
                case JitOperation.MemoryCmp: case JitOperation.MemoryCmpReverse: case JitOperation.MemoryTest:
                    step.Left = operation is JitOperation.MemoryCmp or JitOperation.MemoryTest ? step.MemoryValue : Read(instruction.Destination, instruction.Width);
                    step.Right = operation is JitOperation.Inc or JitOperation.Dec ? Constant(1) :
                        operation == JitOperation.MemoryCmpReverse ? step.MemoryValue : Read(instruction.Source, instruction.Width);
                    JitPlanValueKind kind = operation switch
                    {
                        JitOperation.Add or JitOperation.Inc => JitPlanValueKind.Add,
                        JitOperation.Sub or JitOperation.Cmp or JitOperation.Dec or JitOperation.MemoryCmp or JitOperation.MemoryCmpReverse => JitPlanValueKind.Sub,
                        JitOperation.And or JitOperation.Test or JitOperation.MemoryTest => JitPlanValueKind.And,
                        JitOperation.Or => JitPlanValueKind.Or,
                        _ => JitPlanValueKind.Xor
                    };
                    step.Result = Binary(kind, step.Left, step.Right, instruction.Width);
                    if (operation is not (JitOperation.Cmp or JitOperation.Test or JitOperation.MemoryCmp or JitOperation.MemoryCmpReverse or JitOperation.MemoryTest))
                        Write(instruction.Destination, instruction.Width, step.Result);
                    break;
                case JitOperation.Jump: case JitOperation.ConditionalJump: case JitOperation.Nop: break;
                default: throw new ArgumentException("Unknown address-plan operation");
            }
            if (step.FlagsEffect != JitPlanFlagsEffect.Preserve) flags = (ushort)(index + 1);
            step.FlagsAfter = flags;
            step.DefinitionCount = (ushort)(DefinitionCount - step.FirstDefinition);
            steps[index] = step;
        }
    }
}
