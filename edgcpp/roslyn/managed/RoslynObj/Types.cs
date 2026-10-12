using System.Numerics;
namespace RoslynObj;

public class CppType : SemanticEntity
{
    internal CppType(RoslynObject owner, long id, Row row) : base(owner, id) => Details = new(owner, row);
    public TypeDetails Details { get; }
    public SemanticKind Kind => Details.Kind;
    public string? Name => Object.Name(Object.Store.Required("type", Id).OptionalLong("name_id"));
    public BigInteger Size => Details.Size;
    public BigInteger Alignment => Details.Alignment;
    public bool IsIncomplete => Details.Incomplete;
    public bool IsDependent => Details.IsDependent;
    public SourceInfo? Source => Details.SourceOrigin;
    public CppType? CanonicalType => Details.CanonicalType;
    public CppType? DeclaredType => Details.DeclaredType;
    public IReadOnlyList<Symbol> Members => Object.List("SELECT s.* FROM symbol s WHERE s.containing_entity_id=$owner ORDER BY s.id", Id, r => Object.Symbol(r.Long("id")), "symbol");
    public IReadOnlyList<FieldSymbol> Fields => Object.List("SELECT * FROM field_symbol WHERE record_type_id=$owner ORDER BY id", Id, r => Object.Field(r.Long("id")), "field_symbol");
    public IReadOnlyList<FunctionSymbol> Methods => Object.List("SELECT * FROM routine_symbol WHERE containing_type_id=$owner ORDER BY id", Id, r => Object.Function(r.Long("id")), "routine_symbol");
    public RecordLayout? Layout => Object.Store.Query("SELECT id FROM record_layout WHERE type_id=$owner", Id).FirstOrDefault() is { } row ? Object.Layout(row.Long("id")) : null;
    public IReadOnlyList<DirectBaseDetails> Bases => Object.List("SELECT * FROM direct_base WHERE owner_id=$owner ORDER BY ordinal", Id, r => new DirectBaseDetails(Object, r));
    public IReadOnlyList<BaseSubobject> BaseSubobjects => Object.List("SELECT * FROM base_subobject WHERE most_derived_type_id=$owner ORDER BY id", Id, r => Object.Subobject(r.Long("id")), "base_subobject");
    public IReadOnlyList<VirtualMemberDetails> VirtualMembers => Object.List("SELECT * FROM virtual_member WHERE owner_id=$owner ORDER BY ordinal", Id, r => new VirtualMemberDetails(Object, r));
    public IReadOnlyList<LoweredTypeRelationshipDetails> LoweringRelationships => Object.List("SELECT * FROM lowered_type_relationship WHERE owner_id=$owner ORDER BY ordinal", Id, r => new LoweredTypeRelationshipDetails(Object, r));
    public CppType? OriginalType => LoweringRelationships.FirstOrDefault(r => r.Kind.Name == "retained_original")?.Target;
    public CppType? LoweringOrigin => LoweringRelationships.FirstOrDefault(r => r.Kind.Name == "declared")?.Target;
    public override string ToString() => Name ?? Kind.Name;
}

public sealed class ArithmeticType : CppType
{
    internal ArithmeticType(RoslynObject owner, long id, Row row) : base(owner, id, row) { }
    public ArithmeticTypeDetails Arithmetic => Object.Detail("arithmetic_type", Id, r => new ArithmeticTypeDetails(Object, r));
}

public sealed class QualifiedType : CppType
{
    internal QualifiedType(RoslynObject owner, long id, Row row) : base(owner, id, row) { }
    public QualifiedTypeDetails Qualification => Object.Detail("qualified_type", Id, r => new QualifiedTypeDetails(Object, r));
    public CppType UnderlyingType => Qualification.UnderlyingType;
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
    public BigInteger? Bound => Array.Bound;
    public Operation? BoundExpression => Array.BoundExpression;
}

public sealed class RoutineType : CppType
{
    internal RoutineType(RoslynObject owner, long id, Row row) : base(owner, id, row) { }
    public RoutineTypeDetails Routine => Object.Detail("routine_type", Id, r => new RoutineTypeDetails(Object, r));
    public CppType ReturnType => Routine.ResultType;
    public IReadOnlyList<RoutineTypeParameterDetails> Parameters => Object.List("SELECT * FROM routine_type_parameter WHERE owner_id=$owner ORDER BY ordinal", Id, r => new RoutineTypeParameterDetails(Object, r));
    public ExceptionSpecificationDetails? ExceptionSpecification => Routine.ExceptionSpecification;
}

public class RecordType : CppType
{
    internal RecordType(RoslynObject owner, long id, Row row) : base(owner, id, row) { }
    public RecordTypeDetails Record => Object.Detail("record_type", Id, r => new RecordTypeDetails(Object, r));
    public CppType? SubobjectPartner => Record.SubobjectPartner;
    public TemplateInstantiation? Instantiation => Record.Instance;
}

public sealed class ConstructedType : RecordType
{
    internal ConstructedType(RoslynObject owner, long id, Row row) : base(owner, id, row) { }
    public TemplateSymbol? OriginalDefinition => Instantiation?.OriginalDefinition;
    public TemplateSymbol? Template => OriginalDefinition;
    public IReadOnlyList<TemplateArgument> TemplateArguments => Instantiation?.Arguments ?? [];
    public IReadOnlyList<CppType> TypeArguments => Instantiation?.TypeArguments ?? [];
}

public sealed class TypeReference : CppType
{
    internal TypeReference(RoslynObject owner, long id, Row row) : base(owner, id, row) { }
    public TypeReferenceDetails Reference => Object.Detail("type_reference", Id, r => new TypeReferenceDetails(Object, r));
    public CppType UnderlyingType => Reference.UnderlyingType;
}

public sealed class VectorType : CppType
{
    internal VectorType(RoslynObject owner, long id, Row row) : base(owner, id, row) { }
    public VectorTypeDetails Vector => Object.Detail("vector_type", Id, r => new VectorTypeDetails(Object, r));
}

public sealed class MemberPointerType : CppType
{
    internal MemberPointerType(RoslynObject owner, long id, Row row) : base(owner, id, row) { }
    public MemberPointerTypeDetails MemberPointer => Object.Detail("member_pointer_type", Id, r => new MemberPointerTypeDetails(Object, r));
}

public sealed class DependentType : CppType
{
    internal DependentType(RoslynObject owner, long id, Row row) : base(owner, id, row) { }
    public DependentTypeDetails Dependency => Object.Detail("dependent_type", Id, r => new DependentTypeDetails(Object, r));
}

public sealed class RecordLayout : SemanticEntity
{
    internal RecordLayout(RoslynObject owner, long id, Row row) : base(owner, id) => Details = new(owner, row);
    public RecordLayoutDetails Details { get; }
    public CppType Type => Details.Type;
    public BigInteger Size => Type.Size;
    public BigInteger Alignment => Type.Alignment;
    public IReadOnlyList<FieldLayoutDetails> Fields => Object.List("SELECT * FROM field_layout WHERE owner_id=$owner ORDER BY ordinal", Id, r => new FieldLayoutDetails(Object, r));
}

public sealed class BaseSubobject : SemanticEntity
{
    internal BaseSubobject(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public BaseSubobjectDetails Details { get; }
    public CppType MostDerivedType => Details.MostDerivedType;
    public CppType BaseType => Details.BaseType;
    public BigInteger? Offset => Details.Offset;
    public BaseSubobject? Parent => Details.Parent;
    public IReadOnlyList<BaseDerivation> Derivations => Object.List("SELECT * FROM base_derivation WHERE base_subobject_id=$owner ORDER BY ordinal,id",Id,r=>Object.Derivation(r.Long("id")),"base_derivation");
}

public sealed class BaseDerivation : SemanticEntity
{
    internal BaseDerivation(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public BaseDerivationDetails Details { get; }
    public BaseSubobject BaseSubobject => Details.BaseSubobject;
    public IReadOnlyList<BaseDerivationStepDetails> Steps => Object.List("SELECT * FROM base_derivation_step WHERE owner_id=$owner ORDER BY ordinal",Id,r=>new BaseDerivationStepDetails(Object,r));
}
