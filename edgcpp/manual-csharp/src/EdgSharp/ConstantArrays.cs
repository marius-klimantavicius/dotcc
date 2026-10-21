// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Handwritten interpret.c ck_string materialization, array decay and indexed
// scalar locations. Aggregate addresses never masquerade as integer values.
using System.Collections.ObjectModel;
using System.Numerics;

namespace EdgSharp;

public sealed class ConstantArrayObject
{
    public int Identity { get; }
    public ArrayType Type { get; }
    public Symbol? Symbol { get; }
    public StringLiteralExpression? Literal { get; }
    public ReadOnlyCollection<ConstantStorageObject> Elements { get; }
    internal ConstantArrayObject(ConstantArray array, Dictionary<ConstantCell, ConstantStorageObject> snapshots)
        => (Identity, Type, Symbol, Literal, Elements) =
            (array.Identity, array.Type, array.Symbol, array.Literal, System.Array.AsReadOnly(array.Elements.Select(cell => snapshots[cell]).ToArray()));
}
public sealed class ConstantPointerValue
{
    public ConstantArrayObject? Array { get; }
    public int Index { get; }
    public bool IsNull => Array == null;
    public bool IsOnePastEnd => Array != null && Index == Array.Type.Length;
    internal ConstantPointerValue(ConstantArrayObject? array, int index) => (Array, Index) = (array, index);
}
/// <summary>A transported reference location that designates no scalar object.</summary>
public sealed class ConstantNonObjectReferenceBinding
{
    public ReferenceBindingExpression Expression { get; }
    public ConstantPointerValue Address { get; }
    public int Invocation { get; }
    internal ConstantNonObjectReferenceBinding(ReferenceBindingExpression expression, ConstantPointerValue address, int invocation)
        => (Expression, Address, Invocation) = (expression, address, invocation);
}
internal sealed class ConstantArray(int identity, ArrayType type, Symbol? symbol, StringLiteralExpression? literal, ConstantCell[] elements)
{
    internal int Identity { get; } = identity;
    internal ArrayType Type { get; } = type;
    internal Symbol? Symbol { get; } = symbol;
    internal StringLiteralExpression? Literal { get; } = literal;
    internal ConstantCell[] Elements { get; } = elements;
}
internal sealed record ConstantArrayAddress(ConstantArray? Array, int Index);
internal sealed class ConstantPointerSlot(PointerType type, Symbol symbol, bool readable)
{
    internal PointerType Type { get; } = type;
    internal Symbol Symbol { get; } = symbol;
    internal bool Readable { get; } = readable;
    internal bool Initializing { get; set; }
    internal ConstantArrayAddress? Value { get; set; }
}
internal sealed class ConstantAggregateSlot
{
    internal ConstantArray? Array { get; init; }
    internal ConstantPointerSlot? Pointer { get; init; }
}

internal sealed partial class ConstantInterpreter
{
    private sealed record FinishSubscript(SubscriptExpression Expression) : Work;
    private sealed record FinishPointer(ConstantPointerSlot Slot, bool ProduceValue) : Work;
    private sealed record FinishPointerArithmetic(PointerArithmeticExpression Expression) : Work;
    private readonly List<ConstantArray> arrayObjects = [];
    private readonly List<(ReferenceBindingExpression Expression, ConstantArrayAddress Address, int Invocation)> nonObjectBindings = [];
    private readonly Dictionary<StringLiteralExpression, ConstantArray> literals = [];
    private readonly Dictionary<Symbol, ConstantAggregateSlot> aggregateGlobals = [];
    private readonly Dictionary<CallFrame, Dictionary<Symbol, ConstantAggregateSlot>> aggregateLocals = [];
    private Dictionary<Symbol, ConstantAggregateSlot> AggregateLocals(CallFrame frame)
    {
        if (!aggregateLocals.TryGetValue(frame, out var variables)) aggregateLocals.Add(frame, variables = []);
        return variables;
    }
    private ConstantArray MakeArray(ArrayType type, Symbol? symbol, StringLiteralExpression? literal, IReadOnlyList<uint>? units, CallFrame? frame, bool constantReadable = false)
    {
        if (type.Length <= 0 || !TypeTable.IsArithmetic(type.ElementType)) { Stop(ConstantEvaluationStatus.InvalidExpression, "Array storage requires a complete scalar element type."); throw new InvalidOperationException(); }
        if (type.Length > options.StringLiteralCodeUnitLimit || (long)steps + type.Length > options.ConstantEvaluationStepLimit)
            Stop(ConstantEvaluationStatus.ResourceLimit, "Array materialization exceeds the constant evaluation resource limit.");
        steps += type.Length;
        bool isStatic = frame == null;
        bool readable = literal != null || !isStatic || constantReadable;
        var cells = new ConstantCell[type.Length];
        for (int index = 0; index < cells.Length; ++index)
        {
            ConstantCell cell = Allocate(type.ElementType, symbol, symbol, isStatic ? ConstantStorageLifetime.Static : ConstantStorageLifetime.Automatic, readable, frame, beganInEvaluation: !isStatic);
            cell.Initialized = units != null || isStatic;
            if (cell.Initialized && readable) StoreScalar(cell, ConvertScalar(ConstantRuntimeValue.Scalar(units != null && index < units.Count ? units[index] : BigInteger.Zero), type.ElementType));
            cells[index] = cell;
        }
        var array = new ConstantArray(arrayObjects.Count + 1, type, symbol, literal, cells); arrayObjects.Add(array); return array;
    }
    private void EvaluateString(StringLiteralExpression expression)
    {
        if (!literals.TryGetValue(expression, out ConstantArray? array))
        {
            array = MakeArray(expression.Type, null, expression, expression.CodeUnits, null); literals.Add(expression, array);
        }
        values.Push(ConstantRuntimeValue.ArrayAddress(array));
    }
    private bool ResolveAggregateReference(ReferenceExpression expression)
    {
        if (expression.ReferencedSymbol.Type is not (ArrayType or PointerType)) return false;
        Symbol symbol = expression.ReferencedSymbol;
        ConstantAggregateSlot? slot = null;
        if (calls.TryPeek(out CallFrame? frame)) AggregateLocals(frame).TryGetValue(symbol, out slot);
        if (slot == null && !aggregateGlobals.TryGetValue(symbol, out slot))
        {
            if (symbol.Declarations.FirstOrDefault() is not VariableDeclaration variable || variable.IsParameter || symbol.Scope.Kind != ScopeKind.TranslationUnit)
            { Stop(ConstantEvaluationStatus.NotConstant, "Array or pointer storage is unavailable outside its invocation."); return true; }
            if (variable.Type is ArrayType arrayType)
            {
                IReadOnlyList<uint>? units = (variable.Initializer as ArrayInitializationExpression)?.Literal.CodeUnits;
                if (variable.Initializer != null && units == null) Stop(ConstantEvaluationStatus.InvalidExpression, "Erroneous array initializer.");
                slot = new() { Array = MakeArray(arrayType, symbol, null, units, null, variable.IsConstexpr) }; aggregateGlobals.Add(symbol, slot);
            }
            else
            {
                var pointer = new ConstantPointerSlot((PointerType)variable.Type, symbol, variable.IsConstexpr);
                slot = new() { Pointer = pointer }; aggregateGlobals.Add(symbol, slot);
                values.Push(ConstantRuntimeValue.PointerAddress(pointer)); return true;
            }
        }
        if (slot.Array is { } array) values.Push(ConstantRuntimeValue.ArrayAddress(array));
        else values.Push(ConstantRuntimeValue.PointerAddress(slot.Pointer!));
        return true;
    }
    private bool EvaluateAggregateConversion(ConversionExpression expression)
    {
        if (expression.Conversion == ConversionKind.ArrayToPointer)
        {
            ConstantArray? array = values.Pop().Array;
            if (array == null) Stop(ConstantEvaluationStatus.InvalidExpression, "Array decay requires an array location.");
            values.Push(ConstantRuntimeValue.PointerValue(new(array, 0))); return true;
        }
        if (expression.Type is PointerType)
        {
            ConstantRuntimeValue source = values.Pop();
            if (expression.Conversion == ConversionKind.LValueToRValue)
            {
                ConstantPointerSlot slot = source.PointerLocation ?? throw new EvaluationStopped(ConstantEvaluationStatus.InvalidExpression, "Pointer load requires a pointer object.");
                if (!slot.Readable) Stop(ConstantEvaluationStatus.NotConstant, "Reading an existing mutable pointer is not constant.");
                if (slot.Value == null)
                {
                    if (slot.Initializing) Stop(ConstantEvaluationStatus.UninitializedRead, "Pointer initializer reads its own object.");
                    if (slot.Symbol.Declarations.FirstOrDefault() is not VariableDeclaration { Initializer: { } initializer })
                        Stop(ConstantEvaluationStatus.UninitializedRead, "Read of an uninitialized pointer.");
                    slot.Initializing = true;
                    ScheduleFullExpression(((VariableDeclaration)slot.Symbol.Declarations[0]).Initializer!, new FinishPointer(slot, true)); return true;
                }
                values.Push(ConstantRuntimeValue.PointerValue(slot.Value)); return true;
            }
            if (expression.Conversion == ConversionKind.NullPointerConversion)
                values.Push(ConstantRuntimeValue.PointerValue(new(null, 0)));
            else
            {
                if (source.Pointer == null) Stop(ConstantEvaluationStatus.InvalidExpression, "Pointer conversion requires a pointer value.");
                values.Push(source);
            }
            return true;
        }
        if (expression.Conversion == ConversionKind.BooleanConversion && expression.Operand.Type is PointerType)
        {
            ConstantArrayAddress? pointer = values.Pop().Pointer;
            if (pointer == null) Stop(ConstantEvaluationStatus.InvalidExpression, "Pointer boolean conversion requires a value.");
            values.Push(ConstantRuntimeValue.Scalar(pointer!.Array == null ? BigInteger.Zero : BigInteger.One)); return true;
        }
        return false;
    }
    private void Subscript(SubscriptExpression expression)
    {
        ConstantRuntimeValue sourceRight = values.Pop(), sourceLeft = values.Pop();
        BigInteger index = Scalar(expression.PointerOperandIsSecond ? sourceLeft : sourceRight);
        ConstantArrayAddress? pointer = (expression.PointerOperandIsSecond ? sourceRight : sourceLeft).Pointer;
        ConstantArray array = pointer?.Array ?? throw new EvaluationStopped(ConstantEvaluationStatus.UndefinedBehavior, "A null pointer cannot designate an array element.");
        BigInteger offset = pointer!.Index + index;
        if (offset < 0 || offset > array.Elements.Length)
            Stop(ConstantEvaluationStatus.UndefinedBehavior, "Subscript does not designate an element within the array.");
        if (offset == array.Elements.Length)
        {
            if (array.Elements.Any(cell => !cell.Alive)) Stop(ConstantEvaluationStatus.ExpiredObject, "The designated array's lifetime has ended.");
            values.Push(ConstantRuntimeValue.NonObjectAddress(new(array, (int)offset))); return;
        }
        ConstantCell cell = array.Elements[(int)offset]; EnsureAlive(cell);
        values.Push(ConstantRuntimeValue.Address(cell));
    }
    private void PointerArithmetic(PointerArithmeticExpression expression)
    {
        BigInteger offset = Scalar(values.Pop()); ConstantArrayAddress? pointer = values.Pop().Pointer;
        ConstantArray array = pointer?.Array ?? throw new EvaluationStopped(ConstantEvaluationStatus.UndefinedBehavior, "Pointer arithmetic requires an array object.");
        if (expression.Operator == TokenKind.Minus) offset = -offset;
        offset += pointer!.Index;
        if (offset < 0 || offset > array.Elements.Length)
            Stop(ConstantEvaluationStatus.UndefinedBehavior, "Pointer arithmetic leaves the array including its one-past position.");
        if (array.Elements.Any(cell => !cell.Alive)) Stop(ConstantEvaluationStatus.ExpiredObject, "Pointer arithmetic designates expired array storage.");
        values.Push(ConstantRuntimeValue.PointerValue(new(array, (int)offset)));
    }
    private bool DeclareAggregate(VariableDeclaration variable, CallFrame frame)
    {
        if (variable.Type is ArrayType array)
        {
            if (variable.Initializer != null && variable.Initializer is not ArrayInitializationExpression)
                Stop(ConstantEvaluationStatus.InvalidExpression, "Erroneous array initializer.");
            var objectValue = MakeArray(array, variable.Symbol, null, (variable.Initializer as ArrayInitializationExpression)?.Literal.CodeUnits, frame);
            AggregateLocals(frame).Add(variable.Symbol, new() { Array = objectValue }); return true;
        }
        if (variable.Type is PointerType pointer)
        {
            var slot = new ConstantPointerSlot(pointer, variable.Symbol, true);
            AggregateLocals(frame).Add(variable.Symbol, new() { Pointer = slot });
            if (variable.Initializer != null) ScheduleFullExpression(variable.Initializer, new FinishPointer(slot, false));
            return true;
        }
        return false;
    }
    private bool BindAggregateParameter(VariableDeclaration parameter, ConstantRuntimeValue argument, CallFrame frame)
    {
        if (parameter.Type is not PointerType pointer) return false;
        if (argument.Pointer == null) Stop(ConstantEvaluationStatus.InvalidExpression, "A pointer parameter requires a pointer value.");
        var slot = new ConstantPointerSlot(pointer, parameter.Symbol, true) { Value = argument.Pointer };
        AggregateLocals(frame).Add(parameter.Symbol, new() { Pointer = slot }); return true;
    }
    private void FinishPointerValue(FinishPointer item)
    {
        ConstantArrayAddress? pointer = values.Pop().Pointer;
        if (pointer == null) Stop(ConstantEvaluationStatus.InvalidExpression, "Pointer initialization requires an address value.");
        item.Slot.Value = pointer; item.Slot.Initializing = false;
        if (item.ProduceValue) values.Push(ConstantRuntimeValue.PointerValue(pointer!));
    }
    private void ClearAggregateState()
    { arrayObjects.Clear(); literals.Clear(); aggregateGlobals.Clear(); aggregateLocals.Clear(); nonObjectBindings.Clear(); }
    private void LeaveAggregateScope(Scope scope, CallFrame frame)
    {
        if (!aggregateLocals.TryGetValue(frame, out var variables)) return;
        foreach (VariableDeclaration variable in scope.Declarations.OfType<VariableDeclaration>())
            if (!variable.IsParameter) variables.Remove(variable.Symbol);
    }
}
