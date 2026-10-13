using System.Numerics;
namespace RoslynObj;

public partial class RecordType
{
    public Scope? Scope => Record.Scope;
    public SemanticKind DefinitionState => Record.DefinitionState;
    public bool IsLambdaClosureClass => Record.IsLambda;
    public bool IsEmpty => Record.IsEmpty;
    public bool IsAbstract => Record.IsAbstract;
    public bool IsFinal => Record.IsFinal;
    public bool IsPacked => Record.IsPacked;
    public bool HasVirtualBases => Record.AnyVirtualBaseClasses;
    public bool HasVirtualFunctions => Record.AnyVirtualFunctionsIncludingBases;
    public bool HasPureVirtualFunctions => Record.AnyPureVirtualFunctions;
    public BigInteger? MaximumMemberAlignment => Record.MaximumMemberAlignment;
    public BaseSubobject? PrimaryBaseSubobject => Record.PrimaryBaseSubobject;
    public BaseSubobject? VirtualFunctionInfoBaseSubobject => Record.VirtualFunctionInfoBaseSubobject;
    public BigInteger? VirtualFunctionInfoOffset => Record.VirtualFunctionInfoOffset;
    public BigInteger? HighestVirtualFunctionNumber => Record.HighestVirtualFunctionNumber;
    public BigInteger? NextNegativeVirtualTableIndex => Record.NextNegativeVirtualTableIndex;
    public BigInteger? FirstVirtualCallOffsetIndex => Record.FirstVcallOffsetIndex;
    public VariableSymbol? VirtualFunctionTableVariable => Record.VirtualFunctionTableVariable;
    public VariableSymbol? VirtualTableTableVariable => Record.VirtualTableTableVariable;
    public VariableSymbol? UuidVariable => Record.UuidVariable;
    public string? Uuid => Record.UuidString;
    public SemanticKind? OriginalTagKind => Record.OriginalTagKind;
    public SemanticKind? InheritanceKind => Record.InheritanceKind;
    public CppType? ProxyOfType => Record.ProxyOfType;
    public SemanticEntity? LambdaParentEntity => Record.LambdaParentEntity;
    public FunctionSymbol? AllocationRoutine => Record.AllocationRoutine;
    public FunctionSymbol? DeallocationRoutine => Record.DeallocationRoutine;
    public IReadOnlyList<BaseSubobject> BasesInPostorder => Object.List("SELECT b.* FROM record_base_postorder o JOIN base_subobject b ON b.id=o.target_id WHERE o.owner_id=$owner ORDER BY o.ordinal",Id,r=>Object.Subobject(r.Long("id")),"base_subobject");
    public IReadOnlyList<BaseSubobject> BasesInPreorder => Object.List("SELECT b.* FROM record_base_preorder o JOIN base_subobject b ON b.id=o.target_id WHERE o.owner_id=$owner ORDER BY o.ordinal",Id,r=>Object.Subobject(r.Long("id")),"base_subobject");
    public IReadOnlyList<SemanticEntity> Friends => Object.List("SELECT * FROM record_friend WHERE owner_id=$owner ORDER BY ordinal",Id,r=>Object.Entity(r.Long("target_id")));
    public IReadOnlyList<CppType> BefriendingClasses => Object.List("SELECT t.* FROM record_befriending_class o JOIN type t ON t.id=o.target_id WHERE o.owner_id=$owner ORDER BY o.ordinal",Id,r=>Object.Type(r.Long("id")),"type");
    public IReadOnlyList<CppType> PromotedTypes => Object.List("SELECT t.* FROM record_promoted_type o JOIN type t ON t.id=o.target_id WHERE o.owner_id=$owner ORDER BY o.ordinal",Id,r=>Object.Type(r.Long("id")),"type");
    public IReadOnlyList<RecordEventInterfaceDetails> EventInterfaces => Object.List("SELECT * FROM record_event_interface WHERE owner_id=$owner ORDER BY ordinal",Id,r=>new RecordEventInterfaceDetails(Object,r));
    public IReadOnlyList<VirtualCallOffset> VirtualCallOffsets => Object.List("SELECT * FROM record_vcall_offset WHERE owner_id=$owner ORDER BY ordinal",Id,r=>new VirtualCallOffset(Object,r));
}

public sealed partial class RecordLayout
{
    public BigInteger? NonvirtualSize => Details.NonvirtualSize;
    public BigInteger? NonvirtualAlignment => Details.NonvirtualAlignment;
    public bool IsComplete => Details.IsComplete;
}

public sealed class FieldLayout
{
    internal FieldLayout(RoslynObject owner,Row row) => Details=new(owner,row);
    public FieldLayoutDetails Details { get; }
    public RecordLayout Owner => Details.Owner;
    public long Ordinal => Details.Ordinal;
    public FieldSymbol Field => Details.Field;
    /// <summary>Offset in target addressable units, as stored by EDG.</summary>
    public BigInteger? Offset => Details.Offset;
    public BigInteger? BitOffset => Details.BitOffset;
    public BigInteger? BitWidth => Details.BitWidth;
    /// <summary>Recorded displacement in target addressable units within a bit-field container, when retained.</summary>
    public BigInteger? ContainerOffset => Details.ContainerOffset;
}

public sealed class VirtualCallOffset
{
    internal VirtualCallOffset(RoslynObject owner,Row row) => Details=new(owner,row);
    public RecordVcallOffsetDetails Details { get; }
    public RecordType Owner => Details.Owner;
    public long Ordinal => Details.Ordinal;
    public FunctionSymbol? Routine => Details.Routine;
    public BaseSubobject? BaseSubobject => Details.BaseSubobject;
    public BigInteger VirtualTableIndex => Details.VtableIndex;
    public bool IsPrimary => Details.IsPrimary;
}

public sealed partial class BaseSubobject
{
    public bool IsVirtual => Details.IsVirtual;
    public bool IsDirect => Details.IsDirect;
    public bool IsAmbiguous => Details.Ambiguous;
    public bool IsOptimizedEmptyBase => Details.IsOptimizedEmptyBase;
    public bool IsPackExpansion => Details.IsPackExpansion;
    public bool SharesVirtualFunctionInfo => Details.SharesVirtualFunctionInfo;
    public bool HasPublicDerivation => Details.HasPublicDerivation;
    public CppType? OriginalType => Details.OriginalType;
    public BaseSubobject? PrimaryBaseSubobject => Details.PrimaryBaseSubobject;
    public BaseSubobject? DataSectionBaseSubobject => Details.DataSectionBaseSubobject;
    public BigInteger? VirtualBasePointerOffset => Details.VirtualBasePointerOffset;
    public BaseSubobject? VirtualBasePointerBaseSubobject => Details.VirtualBasePointerBaseSubobject;
    public VariableSymbol? VirtualFunctionTableVariable => Details.VirtualFunctionTableVariable;
    public BigInteger? VirtualFunctionTableIndex => Details.VirtualFunctionTableIndex;
    public BigInteger? ConstructionTableIndex => Details.ConstructionTableIndex;
    public BigInteger? ConstructionSubarrayIndex => Details.ConstructionSubarrayIndex;
    public BigInteger? VirtualBaseOffsetIndex => Details.VirtualBaseOffsetIndex;
}

public sealed partial class FieldSymbol
{
    public bool IsMutable => Details.IsMutable;
    public bool IsBitField => Details.IsBitField;
    public bool IsGeneratedBase => Details.IsGeneratedBase;
    public bool? IsGeneratedVirtualPointer => Details.IsGeneratedVptr;
    public Constant? BitSizeConstant => Details.BitSizeConstant;
    public ExpressionOperation? BitWidthExpression => Details.BitWidthExpression;
    public BigInteger? DeclaredBitSize => Details.DeclaredBitSize;
    public bool HasInitializer => Details.HasInitializer;
    public bool HasNoUniqueAddressAttribute => Details.HasNoUniqueAddressAttribute;
    public bool? IsPacked => Details.IsPacked;
    public BigInteger? ExplicitAlignment => Details.ExplicitAlignment;
    public IReadOnlyList<SemanticEntity> InitializerEntities => Object.List("SELECT * FROM field_initializer_entity WHERE owner_id=$owner ORDER BY ordinal",Id,r=>Object.Entity(r.Long("entity_id")));
}
