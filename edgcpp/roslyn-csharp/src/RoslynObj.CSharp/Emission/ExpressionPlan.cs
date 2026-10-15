namespace RoslynObj.CSharp;

[Flags]
internal enum ExpressionEffects { None = 0, ReadStorage = 1, WriteStorage = 2, Call = 4, MayThrow = 8, VolatileRead = 16, VolatileWrite = 32 }

/// <summary>A build-time C# type, including synthetic pointer temporaries and C# Boolean conditions.</summary>
internal sealed record CsType(CppType? Captured, int AddedPointerDepth = 0, string? Intrinsic = null)
{
    internal static CsType Value(CppType type) => new(type);
    internal static CsType Address(CppType type) => new(type, 1);
    internal static CsType Boolean { get; } = new(null, Intrinsic: "bool");
    internal static CsType Void { get; } = new(null, Intrinsic: "void");
    internal static CsType Int32 { get; } = new(null, Intrinsic: "int");
}

internal abstract record CsExpression(CsType Type);
internal sealed record CsBooleanLiteral(bool Value) : CsExpression(CsType.Boolean);
internal sealed record CsLeaf(CsType ValueType, string Text, bool Stable) : CsExpression(ValueType);
internal sealed record CsVariable(CsType ValueType, string Name) : CsExpression(ValueType);
internal sealed record CsUnary(CsType ValueType, string Operator, CsExpression Operand) : CsExpression(ValueType);
internal sealed record CsBinary(CsType ValueType, string Operator, CsExpression Left, CsExpression Right) : CsExpression(ValueType);
internal sealed record CsCast(CsType ValueType, CsExpression Operand) : CsExpression(ValueType);
internal sealed record CsChoice(CsType ValueType, CsExpression Condition, CsExpression Then, CsExpression Else) : CsExpression(ValueType);
internal sealed record CsLoad(LValuePlan Place, bool VolatileFetch = false) : CsExpression(CsType.Value(Place.Type));
internal sealed record CsAddress(CsType ValueType, LValuePlan Place) : CsExpression(ValueType);
internal sealed record CsPlannedCall(CsType ValueType, CallOperation Operation, CsExpression? Callee,
    IReadOnlyList<CsExpression> Arguments) : CsExpression(ValueType);
internal sealed record CsVariadicArgument(CsType ValueType, OperatorOperation Operation, CsExpression Cursor) : CsExpression(ValueType);
internal sealed record CsTargetShift(CsType ValueType, bool Right, CsExpression Left, CsExpression Count) : CsExpression(ValueType);
internal sealed record CsTruth(CsExpression Value, CppType SourceType) : CsExpression(CsType.Boolean);
internal sealed record CsSourceBoolean(CsType ValueType, CsExpression Condition) : CsExpression(ValueType);

internal enum LValueKind { Slot, Address, Field }
/// <summary>A storage location; a bitfield property has a receiver and no addressable field.</summary>
internal sealed record LValuePlan(CppType Type, LValueKind Kind, CsExpression Operand, FieldSymbol? Field = null, bool VolatileTarget = false,
    VariableSymbol? Variable = null)
{
    internal bool IsBitField => Field?.Details.IsBitField == true;
}

internal abstract record CsPlanStatement;
internal sealed record CsDeclare(CsVariable Variable, CsExpression? Initializer) : CsPlanStatement;
internal sealed record CsSetVariable(CsVariable Variable, CsExpression Value) : CsPlanStatement;
internal sealed record CsStore(LValuePlan Place, CsExpression Value) : CsPlanStatement;
internal sealed record CsCopy(LValuePlan Destination, LValuePlan Source, CppType StorageType) : CsPlanStatement;
internal sealed record CsModifyVariable(CsVariable Variable, string Operator, CsExpression? Right) : CsPlanStatement;
internal sealed record CsEvaluate(CsExpression Value) : CsPlanStatement;
internal sealed record CsIf(CsExpression Condition, IReadOnlyList<CsPlanStatement> Then,
    IReadOnlyList<CsPlanStatement> Else) : CsPlanStatement;

/// <summary>Prefix statements execute at this occurrence; branch prefixes remain inside their selected branch.</summary>
internal sealed record ExpressionPlan(CppType Type, SemanticKind ValueCategory,
    IReadOnlyList<CsPlanStatement> Prefix, CsExpression Value, ExpressionEffects Effects, LValuePlan? Place = null);
