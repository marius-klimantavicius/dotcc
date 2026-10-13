using System.Numerics;
namespace RoslynObj;

public class CppType : SemanticEntity
{
    private readonly Lazy<IReadOnlyList<Symbol>> declarationSymbols;
    private readonly Lazy<Row?> sizeAvailability;
    private readonly Lazy<Row?> alignmentAvailability;
    internal CppType(RoslynObject owner, long id, Row row) : base(owner, id)
    {
        Details = new(owner,row);
        sizeAvailability = new(()=>Object.Store.Fact(Id,5015));
        alignmentAvailability = new(()=>Object.Store.Fact(Id,5016));
        declarationSymbols = new(() => Object.List("SELECT s.* FROM symbol s WHERE s.id=(SELECT symbol_id FROM type WHERE id=$owner) OR s.id IN (SELECT id FROM template_parameter WHERE placeholder_type_id=$owner) ORDER BY s.id",Id,r=>Object.Symbol(r.Long("id")),"symbol"));
    }
    public TypeDetails Details { get; }
    public SemanticKind Kind => Details.Kind;
    public override string? Name => base.Name;
    public BigInteger? Size => sizeAvailability.Value is {} fact&&Object.Kind(fact.Long("state_id")).Name=="available"?EffectiveSize:null;
    /// <summary>The retained stock helper quantity; it may be a THREADS coefficient or a placeholder when size is unavailable.</summary>
    public BigInteger EffectiveSize => Details.Size;
    public BigInteger? ThreadsSizeMultiplier => Details.SizeKind.Name=="upc_threads"?EffectiveSize:null;
    public BigInteger? Alignment => alignmentAvailability.Value is {} fact&&Object.Kind(fact.Long("state_id")).Name=="available"?EffectiveAlignment:null;
    public BigInteger EffectiveAlignment => Details.Alignment;
    public BigInteger DescriptorSize => Details.DescriptorSize ?? EffectiveSize;
    public BigInteger DescriptorAlignment => Details.DescriptorAlignment ?? EffectiveAlignment;
    public bool IsIncomplete => Details.Incomplete;
    public bool? DescriptorIsIncomplete => Details.DescriptorIncomplete;
    public bool IsDependent => Details.IsDependent;
    public SourceInfo? Source => SourceCorrespondence?.Source;
    public IReadOnlyList<Symbol> DeclarationSymbols => declarationSymbols.Value;
    public Symbol? DeclarationSymbol => DeclarationSymbols.Count==1 ? DeclarationSymbols[0] : null;
    public IReadOnlyList<TemplateParameter> DeclaredParameters => Object.List("SELECT * FROM template_parameter WHERE placeholder_type_id=$owner ORDER BY id",Id,r=>Object.TemplateParameter(r.Long("id")),"template_parameter");
    public IReadOnlyList<Symbol> Members => Object.List("SELECT s.* FROM symbol s WHERE s.id IN (SELECT e.owner_id FROM entity_source_correspondence e JOIN source_correspondence c ON c.id=e.source_correspondence_id JOIN scope p ON p.id=c.parent_scope_id WHERE p.associated_entity_id=$owner) OR s.id IN (SELECT id FROM field_symbol WHERE record_type_id=$owner UNION SELECT id FROM routine_symbol WHERE containing_type_id=$owner) ORDER BY s.id", Id, r => Object.Symbol(r.Long("id")), "symbol");
    public IReadOnlyList<FieldSymbol> Fields => Object.List("SELECT q.* FROM (SELECT f.*,COALESCE(o.ordinal,9223372036854775807) AS field_ordinal FROM field_symbol f LEFT JOIN record_layout l ON l.type_id=f.record_type_id LEFT JOIN field_layout o ON o.owner_id=l.id AND o.field_id=f.id WHERE f.record_type_id=$owner) q ORDER BY q.field_ordinal,q.id", Id, r => Object.Field(r.Long("id")), "field_symbol");
    public IReadOnlyList<FunctionSymbol> Methods => Object.List("SELECT * FROM routine_symbol WHERE containing_type_id=$owner ORDER BY id", Id, r => Object.Function(r.Long("id")), "routine_symbol");
    public virtual TemplateInstantiation? Instantiation => null;
    public IReadOnlyList<TemplateArgument> TemplateArguments => Instantiation?.Arguments ?? [];
    public IReadOnlyList<CppType> TypeArguments => Instantiation?.TypeArguments ?? [];
    public TemplateDescriptor? Template => Instantiation?.OriginalDefinition;
    public RecordLayout? Layout => Object.Store.Query("SELECT id FROM record_layout WHERE type_id=$owner", Id).FirstOrDefault() is { } row ? Object.Layout(row.Long("id")) : null;
    public IReadOnlyList<DirectBaseDetails> Bases => Object.List("SELECT * FROM direct_base WHERE owner_id=$owner ORDER BY ordinal", Id, r => new DirectBaseDetails(Object, r));
    public IReadOnlyList<BaseSubobject> BaseSubobjects => Object.List("SELECT * FROM base_subobject WHERE most_derived_type_id=$owner ORDER BY id", Id, r => Object.Subobject(r.Long("id")), "base_subobject");
    public override string ToString() => Name ?? Kind.Name;
}

public sealed class ArithmeticType : CppType
{
    internal ArithmeticType(RoslynObject owner, long id, Row row) : base(owner, id, row) { }
    public ArithmeticTypeDetails Arithmetic => Object.Detail("arithmetic_type", Id, r => new ArithmeticTypeDetails(Object, r));
}

/// <summary>A retained floating storage format with no EDG arithmetic subtype payload.</summary>
public sealed class StorageFloatingType : CppType
{
    internal StorageFloatingType(RoslynObject owner,long id,Row row) : base(owner,id,row) { }
}

/// <summary>The opaque target scalable-vector count type; it has no vector element payload.</summary>
public sealed class ScalableVectorCountType : CppType
{
    internal ScalableVectorCountType(RoslynObject owner,long id,Row row) : base(owner,id,row) { }
}

public sealed class QualifiedType : TypeReference
{
    internal QualifiedType(RoslynObject owner, long id, Row row) : base(owner, id, row) { }
    public QualifiedTypeDetails Qualification => Object.Detail("qualified_type", Id, r => new QualifiedTypeDetails(Object, r));
    public new CppType UnderlyingType => Qualification.UnderlyingType;
    public bool IsConst => Qualification.IsConst;
    public bool IsVolatile => Qualification.IsVolatile;
}

public sealed class PointerType : CppType
{
    internal PointerType(RoslynObject owner, long id, Row row) : base(owner, id, row) { }
    public PointerTypeDetails Pointer => Object.Detail("pointer_type", Id, r => new PointerTypeDetails(Object, r));
    public CppType PointeeType => Pointer.PointeeType;
    public bool IsReference => Pointer.IsReference;
    public bool IsRValueReference => Pointer.IsRvalueReference;
}

public sealed class ArrayType : CppType
{
    internal ArrayType(RoslynObject owner, long id, Row row) : base(owner, id, row) { }
    public ArrayTypeDetails Array => Object.Detail("array_type", Id, r => new ArrayTypeDetails(Object, r));
    public CppType ElementType => Array.ElementType;
    public BigInteger? Bound => Array.BoundKind.Name is "constant" or "zero"?Array.Bound:null;
    public BigInteger? ThreadsMultiplier => Array.BoundKind.Name=="upc_threads"?Array.Bound:null;
    public VlaDimension? VlaDimension => Array.VlaDimension;
    public Operation? BoundExpression => Array.BoundExpression;
}

public sealed partial class RoutineType : CppType
{
    internal RoutineType(RoslynObject owner, long id, Row row) : base(owner, id, row) { }
    public RoutineTypeDetails Routine => Object.Detail("routine_type", Id, r => new RoutineTypeDetails(Object, r));
    public CppType ReturnType => Routine.ResultType;
    public IReadOnlyList<SignatureParameter> Parameters => Object.List("SELECT p.*,t.owner_id,t.ordinal,t.parameter_type_id,t.role_id,t.base_subobject_id FROM routine_type_parameter t JOIN parameter_type p ON p.id=t.parameter_type_id WHERE t.owner_id=$owner ORDER BY t.ordinal", Id, r => new SignatureParameter(Object, r),"parameter_type");
    public ExceptionSpecification? ExceptionSpecification => Routine.ExceptionSpecification;
}

public partial class RecordType : CppType
{
    internal RecordType(RoslynObject owner, long id, Row row) : base(owner, id, row) { }
    public RecordTypeDetails Record => Object.Detail("record_type", Id, r => new RecordTypeDetails(Object, r));
    public CppType? SubobjectPartner => Record.SubobjectPartner;
    public override TemplateInstantiation? Instantiation => Record.Instance;
    public IReadOnlyList<RecordDeclaration> SourceDeclarations => Object.List("SELECT * FROM record_declaration WHERE record_type_id=$owner ORDER BY ordinal,id",Id,r=>Object.RecordDeclaration(r.Long("id")),"record_declaration");
}

public sealed class ConstructedType : RecordType
{
    internal ConstructedType(RoslynObject owner, long id, Row row) : base(owner, id, row) { }
    public TemplateDescriptor? OriginalDefinition => Instantiation?.OriginalDefinition;

}

public class TypeReference : CppType
{
    internal TypeReference(RoslynObject owner, long id, Row row) : base(owner, id, row) { }
    public TypeReferenceDetails Reference => Object.Detail("type_reference", Id, r => new TypeReferenceDetails(Object, r));
    public CppType UnderlyingType => Reference.UnderlyingType;
    public ExpressionOperation? DecltypeExpression => Reference.DecltypeExpression;
    public IReadOnlyList<LocalExpressionReferenceDetails> ContextualExpressions => Object.List("SELECT * FROM local_expression_reference WHERE referrer_id=$owner ORDER BY owner_id,ordinal",Id,r=>new LocalExpressionReferenceDetails(Object,r));
    public override TemplateInstantiation? Instantiation => Reference.Instance;
    public IReadOnlyList<TemplateArgument> OriginalTemplateArguments => Object.List("SELECT a.* FROM typeref_original_template_argument r JOIN template_argument a ON a.id=r.argument_id WHERE r.owner_id=$owner ORDER BY r.ordinal",Id,r=>Object.Argument(r.Long("id")),"template_argument");
}

public sealed class VectorType : CppType
{
    internal VectorType(RoslynObject owner, long id, Row row) : base(owner, id, row) { }
    public VectorTypeDetails Vector => Object.Detail("vector_type", Id, r => new VectorTypeDetails(Object, r));
    public CppType ElementType => Vector.ElementType;
    /// <summary>The retained size expression constant, when supplied; this does not synthesize scalable storage size.</summary>
    public Constant? SizeConstant => Vector.SizeConstant;
}

public sealed class MemberPointerType : CppType
{
    internal MemberPointerType(RoslynObject owner, long id, Row row) : base(owner, id, row) { }
    public MemberPointerTypeDetails MemberPointer => Object.Detail("member_pointer_type", Id, r => new MemberPointerTypeDetails(Object, r));
    public CppType DeclaringType => MemberPointer.DeclaringType;
    public CppType? OriginalDeclaringType => MemberPointer.OriginalDeclaringType;
    public CppType MemberType => MemberPointer.MemberType;
}

public sealed class DependentType : CppType
{
    internal DependentType(RoslynObject owner, long id, Row row) : base(owner, id, row) { }
    public DependentTypeDetails Dependency => Object.Detail("dependent_type", Id, r => new DependentTypeDetails(Object, r));
    public ExpressionOperation? Constraint => Dependency.Constraint;
    public CppType? ProxyClass => Dependency.ProxyClass;
}

public sealed partial class RecordLayout : SemanticEntity
{
    internal RecordLayout(RoslynObject owner, long id, Row row) : base(owner, id) => Details = new(owner, row);
    public RecordLayoutDetails Details { get; }
    public CppType Type => Details.Type;
    public BigInteger? Size => Type.Size;
    public BigInteger? Alignment => Type.Alignment;
    public IReadOnlyList<FieldLayout> Fields => Object.List("SELECT * FROM field_layout WHERE owner_id=$owner ORDER BY ordinal", Id, r => new FieldLayout(Object, r));
}

public sealed partial class BaseSubobject : SemanticEntity
{
    internal BaseSubobject(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public BaseSubobjectDetails Details { get; }
    public CppType MostDerivedType => Details.MostDerivedType;
    public CppType BaseType => Details.BaseType;
    public BigInteger? Offset => Details.Offset;
    public long DirectBaseNumber => Details.DirectBaseNumber;
    public bool? IsCompleteSubobject => Details.CompleteSubobject;
    public IReadOnlyList<OverrideRelationDetails> Overrides => Object.List("SELECT * FROM override_relation WHERE subobject_id=$owner ORDER BY base_ordinal,owner_id,ordinal",Id,r=>new OverrideRelationDetails(Object,r));
    public IReadOnlyList<BaseDerivation> Derivations => Object.List("SELECT * FROM base_derivation WHERE base_subobject_id=$owner ORDER BY ordinal,id",Id,r=>Object.Derivation(r.Long("id")),"base_derivation");
}

public sealed class BaseDerivation : SemanticEntity
{
    internal BaseDerivation(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public BaseDerivationDetails Details { get; }
    public BaseSubobject BaseSubobject => Details.BaseSubobject;
    public IReadOnlyList<BaseDerivationStepDetails> Steps => Object.List("SELECT * FROM base_derivation_step WHERE owner_id=$owner ORDER BY ordinal",Id,r=>new BaseDerivationStepDetails(Object,r));
}
