// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Handwritten scalar storage/address model following interpret.c constexpr
// addresses, allocation identity, reference_to/ref_indirect and live storage.
using System.Numerics;

namespace EdgSharp;

public enum ConstantStorageLifetime { Unknown, Static, Automatic, ReferenceVariableTemporary, FullExpressionTemporary }

/// <summary>
/// An immutable observation of one object in one evaluation. An unknown
/// referent has no identity, object symbol, lifetime or initialization/liveness
/// facts; UnknownReference identifies the reference through which it arrived.
/// </summary>
public sealed class ConstantStorageObject
{
    public int? Identity { get; }
    public bool IsKnownObject { get; }
    public int Invocation { get; }
    public IntegerType Type { get; }
    public Symbol? Symbol { get; }
    public Symbol? UnknownReference { get; }
    public Symbol? LifetimeOwner { get; }
    public ConstantStorageLifetime Lifetime { get; }
    public bool? IsAlive { get; }
    public bool? IsInitialized { get; }
    public bool IsConstantReadable { get; }
    public BigInteger? Value { get; }
    internal ConstantStorageObject(ConstantCell cell)
        => (Identity, IsKnownObject, Invocation, Type, Symbol, UnknownReference, LifetimeOwner, Lifetime, IsAlive, IsInitialized, IsConstantReadable, Value) =
            (cell.Known ? cell.Identity : null, cell.Known, cell.Invocation, cell.Type,
                cell.Known ? cell.Symbol : null, cell.Known ? null : cell.Symbol, cell.LifetimeOwner,
                cell.Lifetime, cell.Known ? cell.Alive : null, cell.Known ? cell.Initialized : null, cell.Readable, cell.Value);
}

/// <summary>A reference constant designates an object; it is not that object's integer value.</summary>
public sealed class ConstantReferenceValue
{
    public ConstantStorageObject Object { get; }
    internal ConstantReferenceValue(ConstantStorageObject storage) => Object = storage;
}

public sealed class ConstantReferenceBinding
{
    public ReferenceBindingExpression Expression { get; }
    public ConstantStorageObject Object { get; }
    public int Invocation { get; }
    internal ConstantReferenceBinding(ReferenceBindingExpression expression, ConstantStorageObject storage, int invocation)
        => (Expression, Object, Invocation) = (expression, storage, invocation);
}

internal sealed class ConstantCell(int identity, int invocation, IntegerType type, Symbol? symbol,
    Symbol? lifetimeOwner, ConstantStorageLifetime lifetime, bool readable)
{
    internal int Identity { get; } = identity;
    internal int Invocation { get; } = invocation;
    internal IntegerType Type { get; } = type;
    internal Symbol? Symbol { get; } = symbol;
    internal Symbol? LifetimeOwner { get; } = lifetimeOwner;
    internal ConstantStorageLifetime Lifetime { get; } = lifetime;
    internal bool Readable { get; } = readable;
    internal bool Alive { get; set; } = true;
    internal bool Initialized { get; set; }
    internal bool Known { get; init; } = true;
    internal bool Initializing { get; set; }
    internal VariableDeclaration? DeferredInitializer { get; set; }
    internal BigInteger? Value { get; set; }
}

internal readonly record struct ConstantRuntimeValue(BigInteger? Integer, ConstantCell? Location)
{
    internal static ConstantRuntimeValue Scalar(BigInteger value) => new(value, null);
    internal static ConstantRuntimeValue Address(ConstantCell location) => new(null, location);
}

internal sealed class ConstantSlot
{
    internal ConstantCell? Object { get; init; }
    internal ConstantCell? Referent { get; set; }
    internal bool Initializing { get; set; }
}

internal sealed partial class ConstantInterpreter
{
    private readonly Dictionary<Symbol, ConstantSlot> globals = [];
    private readonly List<ConstantCell> storage = [];
    private readonly List<(ReferenceBindingExpression Binding, ConstantCell Cell, int Invocation)> bindingObservations = [];
    private readonly Stack<List<ConstantCell>> fullExpressions = [];
    private int invocationSequence;

    private void ScheduleFullExpression(Expression expression, Work continuation)
    {
        work.Push(new EndFullExpression()); work.Push(continuation);
        work.Push(new EvaluateExpression(expression)); work.Push(new BeginFullExpression());
    }

    private ConstantCell Allocate(IntegerType type, Symbol? symbol, Symbol? owner, ConstantStorageLifetime lifetime,
        bool readable, CallFrame? frame)
    {
        var cell = new ConstantCell(storage.Count + 1, frame?.Identity ?? 0, type, symbol, owner, lifetime, readable);
        storage.Add(cell);
        if ((lifetime is ConstantStorageLifetime.Automatic or ConstantStorageLifetime.ReferenceVariableTemporary) && frame != null)
        {
            frame.Owned.Add(cell);
            if (owner != null)
            {
                if (!frame.LifetimeOwners.TryGetValue(owner, out List<ConstantCell>? owned)) frame.LifetimeOwners.Add(owner, owned = []);
                owned.Add(cell);
            }
        }
        if (lifetime == ConstantStorageLifetime.FullExpressionTemporary) fullExpressions.Peek().Add(cell);
        return cell;
    }

    private static void EnsureAlive(ConstantCell cell)
    {
        if (!cell.Alive) Stop(ConstantEvaluationStatus.ExpiredObject, "The designated object's lifetime has ended.");
    }

    private static BigInteger Scalar(ConstantRuntimeValue value)
    {
        if (!value.Integer.HasValue)
            Stop(ConstantEvaluationStatus.InvalidExpression, "An integer value was required without an implemented glvalue load.");
        return value.Integer!.Value;
    }

    private static ConstantCell Location(ConstantRuntimeValue value)
    {
        if (value.Location == null) Stop(ConstantEvaluationStatus.NotConstant, "Reference binding requires an available object location.");
        return value.Location!;
    }

    private static BigInteger Load(ConstantRuntimeValue value)
    {
        ConstantCell cell = Location(value);
        EnsureAlive(cell);
        if (!cell.Known) Stop(ConstantEvaluationStatus.NotConstant, "The reference designates an unknown runtime object whose value is unavailable.");
        if (!cell.Initialized) Stop(ConstantEvaluationStatus.UninitializedRead, "Read of an uninitialized object.");
        if (!cell.Readable) Stop(ConstantEvaluationStatus.NotConstant, "Reading this existing mutable or runtime object is not allowed in a constant expression.");
        return cell.Value!.Value;
    }

    private static void Store(ConstantSlot slot, ConstantRuntimeValue value)
    {
        if (slot.Object is { } cell)
        {
            cell.Value = Scalar(value); cell.Initialized = true;
        }
        else
        {
            ConstantCell referent = Location(value); EnsureAlive(referent); slot.Referent = referent;
        }
    }

    private void Materialize(MaterializedTemporaryExpression expression)
    {
        CallFrame? frame = calls.TryPeek(out CallFrame? active) ? active : null;
        bool extended = expression.Lifetime == ReferenceTemporaryLifetime.ReferenceVariable;
        bool isStatic = extended && expression.LifetimeOwner?.Scope.Kind == ScopeKind.TranslationUnit;
        ConstantStorageLifetime lifetime = isStatic ? ConstantStorageLifetime.Static : extended
            ? ConstantStorageLifetime.ReferenceVariableTemporary : ConstantStorageLifetime.FullExpressionTemporary;
        var cell = Allocate((IntegerType)expression.Type, null, expression.LifetimeOwner, lifetime,
            !isStatic || expression.Type.Qualifiers.HasFlag(TypeQualifiers.Const), frame);
        cell.Value = Scalar(values.Pop()); cell.Initialized = true;
        values.Push(ConstantRuntimeValue.Address(cell));
    }

    private void ResolveReference(ReferenceExpression expression)
    {
        Symbol symbol = expression.ReferencedSymbol;
        if (calls.TryPeek(out CallFrame? frame) && frame.Locals.TryGetValue(symbol, out ConstantSlot? local))
        {
            if (local.Object is { } cell) values.Push(ConstantRuntimeValue.Address(cell));
            else
            {
                if (local.Referent == null) Stop(ConstantEvaluationStatus.UninitializedRead, "Use of an uninitialized reference.");
                values.Push(ConstantRuntimeValue.Address(local.Referent!));
            }
            return;
        }
        if (globals.TryGetValue(symbol, out ConstantSlot? retained))
        {
            if (retained.Object == null && (retained.Initializing || retained.Referent == null))
                Stop(ConstantEvaluationStatus.UninitializedRead, "Use of a reference during its own initialization.");
            values.Push(ConstantRuntimeValue.Address(retained.Object ?? retained.Referent!)); return;
        }
        if (symbol.Declarations.FirstOrDefault() is not VariableDeclaration variable)
        { Stop(ConstantEvaluationStatus.NotConstant, "This designator has no object storage in constant evaluation."); return; }
        if (variable.Type is ReferenceType)
        {
            if (variable.IsParameter)
            {
                var unknown = new ConstantCell(storage.Count + 1, 0, (IntegerType)((ReferenceType)variable.Type).ReferredType,
                    symbol, null, ConstantStorageLifetime.Unknown, false) { Known = false };
                storage.Add(unknown); globals.Add(symbol, new ConstantSlot { Referent = unknown });
                values.Push(ConstantRuntimeValue.Address(unknown)); return;
            }
            if (symbol.Scope.Kind != ScopeKind.TranslationUnit || variable.Initializer == null)
            { Stop(ConstantEvaluationStatus.NotConstant, "A reference outside its invocation has no known binding."); return; }
            var slot = new ConstantSlot { Initializing = true }; globals.Add(symbol, slot);
            ScheduleFullExpression(variable.Initializer, new CompleteGlobalReference(slot));
            return;
        }
        if (variable.Type is not IntegerType integer)
        { Stop(ConstantEvaluationStatus.NotConstant, "Only scalar storage is implemented."); return; }
        bool constant = integer.Qualifiers.HasFlag(TypeQualifiers.Const) && variable.Initializer != null;
        ConstantCell storageCell = Allocate(integer, symbol, symbol,
            symbol.Scope.Kind == ScopeKind.TranslationUnit ? ConstantStorageLifetime.Static : ConstantStorageLifetime.Automatic, constant, null);
        var scalarSlot = new ConstantSlot { Object = storageCell };
        globals.Add(symbol, scalarSlot);
        if (constant)
        {
            // Optional declaration-time attempts are historical observations,
            // never storage. Replay the original typed initializer, including
            // its already-selected function identities, in this evaluation.
            // The in-progress cell detects cycles without recursive CLR calls.
            // Address formation does not evaluate initialization or fetch a
            // value. A subsequent real load requests this typed recipe.
            storageCell.DeferredInitializer = variable;
        }
        else
        {
            storageCell.Initialized = variable.Initializer != null || symbol.Scope.Kind == ScopeKind.TranslationUnit;
        }
        values.Push(ConstantRuntimeValue.Address(storageCell));
    }

    private ConstantEvaluationResult Result(ConstantEvaluationStatus status, ConstantRuntimeValue value, string message)
    {
        var snapshots = storage.ToDictionary(cell => cell, cell => new ConstantStorageObject(cell));
        return new(status, status == ConstantEvaluationStatus.Success ? value.Integer : null, steps, maximumDepth, message,
            status == ConstantEvaluationStatus.Success && value.Location is { } location ? new ConstantReferenceValue(snapshots[location]) : null,
            storage.Select(cell => snapshots[cell]).ToArray(),
            bindingObservations.Select(binding => new ConstantReferenceBinding(binding.Binding, snapshots[binding.Cell], binding.Invocation)).ToArray());
    }
}
