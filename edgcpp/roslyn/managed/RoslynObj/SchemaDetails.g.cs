// Generated from schema/roslynobj.sql by managed/tools/generate_bindings.py. Do not edit.
#nullable enable
using System.Numerics;
namespace RoslynObj;

public sealed class KindDispositionDetails : SemanticDetail
{
    internal KindDispositionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public string State => Data.OptionalText("state")!;
    public SemanticKind? CanonicalKind => Object.Link(Data.OptionalLong("canonical_kind_id"), value => Object.Kind(value));
}

public sealed class EntityDetails : SemanticDetail
{
    internal EntityDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public IReadOnlyList<DeclarationSequenceDetails> DeclarationSequenceRows => Object.List("SELECT * FROM declaration_sequence WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new DeclarationSequenceDetails(Object, r));
    public IReadOnlyList<SemanticReferenceDetails> SemanticReferenceRows => Object.List("SELECT * FROM semantic_reference WHERE owner_id=$owner ORDER BY role_id,ordinal", Data.Long("id"), r => new SemanticReferenceDetails(Object, r));
    public IReadOnlyList<AttributeDetails> AttributeRows => Object.List("SELECT * FROM attribute WHERE owner_id=$owner ORDER BY ordinal,id", Data.Long("id"), r => new AttributeDetails(Object, r), "attribute");
    public IReadOnlyList<LifetimeDetails> LifetimeRows => Object.List("SELECT * FROM lifetime WHERE owner_id=$owner ORDER BY id", Data.Long("id"), r => new LifetimeDetails(Object, r), "lifetime");
    public IReadOnlyList<TemplateDefinitionDetails> TemplateDefinitionRows => Object.List("SELECT * FROM template_definition WHERE owner_id=$owner ORDER BY id", Data.Long("id"), r => new TemplateDefinitionDetails(Object, r), "template_definition");
    public IReadOnlyList<ConstraintMetadataDetails> ConstraintMetadataRows => Object.List("SELECT * FROM constraint_metadata WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new ConstraintMetadataDetails(Object, r));
    public IReadOnlyList<SynthesizedOriginDetails> SynthesizedOriginRows => Object.List("SELECT * FROM synthesized_origin WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new SynthesizedOriginDetails(Object, r));
}

public sealed class ObjectHeaderDetails : SemanticDetail
{
    internal ObjectHeaderDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public long FormatMajor => Data.Long("format_major");
    public long FormatMinor => Data.Long("format_minor");
    public string ArtifactId => Data.OptionalText("artifact_id")!;
    public string Producer => Object.Text(Data.OptionalLong("producer_id"))!;
    public string Revision => Object.Text(Data.OptionalLong("revision_id"))!;
    public string EdgVersion => Object.Text(Data.OptionalLong("edg_version_id"))!;
    public SemanticKind CaptureProfile => Object.Kind(Data.Long("capture_profile_id"));
    public TargetProfileDetails TargetProfile => Object.Link(Data.OptionalLong("target_profile_id"), value => Object.Detail("target_profile", value, r => new TargetProfileDetails(Object, r)))!;
    public SourceFile? MainSourceFile => Object.Link(Data.OptionalLong("main_source_file_id"), value => Object.File(value));
    public TranslationUnit TranslationUnit => Object.Link(Data.OptionalLong("translation_unit_id"), value => Object.Unit(value))!;
    public bool Complete => Data.Bool("complete");
}

public sealed class TargetProfileDetails : SemanticDetail
{
    internal TargetProfileDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Abi => Object.Kind(Data.Long("abi_id"));
    public long AddressableUnitBits => Data.Long("addressable_unit_bits");
    public long LittleEndian => Data.Long("little_endian");
    public long PointerBits => Data.Long("pointer_bits");
    public BigInteger PointerAlignment => RequiredNumber(Data.OptionalText("pointer_alignment"));
    public string? Description => Object.Text(Data.OptionalLong("description_id"));
}

public sealed class TranslationUnitDetails : SemanticDetail
{
    internal TranslationUnitDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public Scope GlobalScope => Object.Link(Data.OptionalLong("global_scope_id"), value => Object.Scope(value))!;
    public SourceFile? PrimaryInput => Object.Link(Data.OptionalLong("primary_input_id"), value => Object.File(value));
    public string LanguageMode => Object.Text(Data.OptionalLong("language_mode_id"))!;
    public IReadOnlyList<StaticInitializationDetails> StaticInitializationRows => Object.List("SELECT * FROM static_initialization WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new StaticInitializationDetails(Object, r));
}

public sealed class BuildCapabilityDetails : SemanticDetail
{
    internal BuildCapabilityDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public string Name => Object.Text(Data.OptionalLong("name_id"))!;
    public SemanticKind State => Object.Kind(Data.Long("state_id"));
    public string? Value => Object.Text(Data.OptionalLong("value_id"));
    public bool Required => Data.Bool("required");
    public bool? Enabled => Data.OptionalBool("enabled");
}

public sealed class InvocationArgumentDetails : SemanticDetail
{
    internal InvocationArgumentDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public long Ordinal => Data.Long("ordinal");
    public string Text => Object.Text(Data.OptionalLong("text_id"))!;
}

public sealed class FactAvailabilityDetails : SemanticDetail
{
    internal FactAvailabilityDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticEntity Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Entity(value))!;
    public SemanticKind Fact => Object.Kind(Data.Long("fact_id"));
    public SemanticKind State => Object.Kind(Data.Long("state_id"));
    public string? Reason => Object.Text(Data.OptionalLong("reason_id"));
}

public sealed class NameDetails : SemanticDetail
{
    internal NameDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public string? Spelling => Object.Text(Data.OptionalLong("spelling_id"));
    public SemanticKind Category => Object.Kind(Data.Long("category_id"));
    public IReadOnlyList<QualifiedNamePartDetails> QualifiedNamePartRows => Object.List("SELECT * FROM qualified_name_part WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new QualifiedNamePartDetails(Object, r));
}

public sealed class QualifiedNamePartDetails : SemanticDetail
{
    internal QualifiedNamePartDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public NameDetails Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Detail("name", value, r => new NameDetails(Object, r)))!;
    public long Ordinal => Data.Long("ordinal");
    public NameDetails Name => Object.Link(Data.OptionalLong("name_id"), value => Object.Detail("name", value, r => new NameDetails(Object, r)))!;
}

public sealed class ScopeDetails : SemanticDetail
{
    internal ScopeDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public Scope? Parent => Object.Link(Data.OptionalLong("parent_id"), value => Object.Scope(value));
    public SemanticEntity? AssociatedEntity => Object.Link(Data.OptionalLong("associated_entity_id"), value => Object.Entity(value));
    public Lifetime? Lifetime => Object.Link(Data.OptionalLong("lifetime_id"), value => Object.Lifetime(value));
    public IReadOnlyList<ScopeMemberDetails> ScopeMemberRows => Object.List("SELECT * FROM scope_member WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new ScopeMemberDetails(Object, r));
}

public sealed class ScopeMemberDetails : SemanticDetail
{
    internal ScopeMemberDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public Scope Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Scope(value))!;
    public long Ordinal => Data.Long("ordinal");
    public SemanticEntity Member => Object.Link(Data.OptionalLong("member_id"), value => Object.Entity(value))!;
}

public sealed class SymbolDetails : SemanticDetail
{
    internal SymbolDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public NameDetails? Name => Object.Link(Data.OptionalLong("name_id"), value => Object.Detail("name", value, r => new NameDetails(Object, r)));
    public SemanticEntity? ContainingEntity => Object.Link(Data.OptionalLong("containing_entity_id"), value => Object.Entity(value));
    public CppType? Type => Object.Link(Data.OptionalLong("type_id"), value => Object.Type(value));
    public SourceInfo? SourceOrigin => Object.Link(Data.OptionalLong("source_origin_id"), value => Object.Source(value));
    public SemanticKind? Linkage => Object.Link(Data.OptionalLong("linkage_id"), value => Object.Kind(value));
    public SemanticKind? Storage => Object.Link(Data.OptionalLong("storage_id"), value => Object.Kind(value));
    public SemanticKind? Access => Object.Link(Data.OptionalLong("access_id"), value => Object.Kind(value));
    public bool CompilerGenerated => Data.Bool("compiler_generated");
}

public sealed class DeclarationDetails : SemanticDetail
{
    internal DeclarationDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public SemanticEntity Symbol => Object.Link(Data.OptionalLong("symbol_id"), value => Object.Entity(value))!;
    public Scope? LexicalScope => Object.Link(Data.OptionalLong("lexical_scope_id"), value => Object.Scope(value));
    public SourceInfo? SourceOrigin => Object.Link(Data.OptionalLong("source_origin_id"), value => Object.Source(value));
    public bool CompilerGenerated => Data.Bool("compiler_generated");
    public bool? Implicit => Data.OptionalBool("implicit");
}

public sealed class DeclarationSequenceDetails : SemanticDetail
{
    internal DeclarationSequenceDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticEntity Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Entity(value))!;
    public long Ordinal => Data.Long("ordinal");
    public Declaration Declaration => Object.Link(Data.OptionalLong("declaration_id"), value => Object.Declaration(value))!;
}

public sealed class NamespaceSymbolDetails : SemanticDetail
{
    internal NamespaceSymbolDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public NamespaceSymbol? AliasTarget => Object.Link(Data.OptionalLong("alias_target_id"), value => Object.Namespace(value));
    public bool IsInline => Data.Bool("is_inline");
    public bool IsAnonymous => Data.Bool("is_anonymous");
    public Scope? Scope => Object.Link(Data.OptionalLong("scope_id"), value => Object.Scope(value));
}

public sealed class RoutineSymbolDetails : SemanticDetail
{
    internal RoutineSymbolDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType RoutineType => Object.Link(Data.OptionalLong("routine_type_id"), value => Object.Type(value))!;
    public CppType? ContainingType => Object.Link(Data.OptionalLong("containing_type_id"), value => Object.Type(value));
    public SemanticNode? BodyRoot => Object.Link(Data.OptionalLong("body_root_id"), value => Object.Node(value));
    public Scope? BodyScope => Object.Link(Data.OptionalLong("body_scope_id"), value => Object.Scope(value));
    public SemanticKind BodyState => Object.Kind(Data.Long("body_state_id"));
    public SemanticKind? SpecialFunction => Object.Link(Data.OptionalLong("special_function_id"), value => Object.Kind(value));
    public SemanticKind? BuiltinKind => Object.Link(Data.OptionalLong("builtin_kind_id"), value => Object.Kind(value));
    public SemanticKind? RuntimeKind => Object.Link(Data.OptionalLong("runtime_kind_id"), value => Object.Kind(value));
    public TemplateSymbol? Template => Object.Link(Data.OptionalLong("template_id"), value => Object.Template(value));
    public TemplateInstantiation? Instance => Object.Link(Data.OptionalLong("instance_id"), value => Object.Instance(value));
    public FunctionSymbol? PrimaryEntry => Object.Link(Data.OptionalLong("primary_entry_id"), value => Object.Function(value));
    public bool IsVirtual => Data.Bool("is_virtual");
    public bool IsPure => Data.Bool("is_pure");
    public bool IsFinal => Data.Bool("is_final");
    public bool IsInline => Data.Bool("is_inline");
    public bool IsConstexpr => Data.Bool("is_constexpr");
    public bool IsConsteval => Data.Bool("is_consteval");
    public bool IsDeleted => Data.Bool("is_deleted");
    public bool IsDefaulted => Data.Bool("is_defaulted");
    public IReadOnlyList<RoutineParameterDetails> RoutineParameterRows => Object.List("SELECT * FROM routine_parameter WHERE owner_id=$owner ORDER BY role_id,ordinal", Data.Long("id"), r => new RoutineParameterDetails(Object, r));
    public IReadOnlyList<OverrideRelationDetails> OverrideRelationRows => Object.List("SELECT * FROM override_relation WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new OverrideRelationDetails(Object, r));
    public IReadOnlyList<AbiRoutineRelationshipDetails> AbiRoutineRelationshipRows => Object.List("SELECT * FROM abi_routine_relationship WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new AbiRoutineRelationshipDetails(Object, r));
}

public sealed class RoutineParameterDetails : SemanticDetail
{
    internal RoutineParameterDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public FunctionSymbol Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Function(value))!;
    public long Ordinal => Data.Long("ordinal");
    public VariableSymbol Parameter => Object.Link(Data.OptionalLong("parameter_id"), value => Object.Variable(value))!;
    public SemanticKind Role => Object.Kind(Data.Long("role_id"));
    public Initialization? DefaultInitialization => Object.Link(Data.OptionalLong("default_initialization_id"), value => Object.Initialization(value));
    public bool IsSourceParameter => Data.Bool("is_source_parameter");
}

public sealed class VariableSymbolDetails : SemanticDetail
{
    internal VariableSymbolDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType Type => Object.Link(Data.OptionalLong("type_id"), value => Object.Type(value))!;
    public Initialization? Initializer => Object.Link(Data.OptionalLong("initializer_id"), value => Object.Initialization(value));
    public Constant? ConstantInitializer => Object.Link(Data.OptionalLong("constant_initializer_id"), value => Object.Constant(value));
    public Lifetime? Lifetime => Object.Link(Data.OptionalLong("lifetime_id"), value => Object.Lifetime(value));
    public SemanticKind? StorageDuration => Object.Link(Data.OptionalLong("storage_duration_id"), value => Object.Kind(value));
    public bool IsThreadLocal => Data.Bool("is_thread_local");
    public bool IsInline => Data.Bool("is_inline");
    public bool IsParameter => Data.Bool("is_parameter");
    public long? ParameterTypeOrdinal => Data.OptionalLong("parameter_type_ordinal");
    public VariableSymbol? BindingContainer => Object.Link(Data.OptionalLong("binding_container_id"), value => Object.Variable(value));
    public IReadOnlyList<VariableBindingDetails> VariableBindingRows => Object.List("SELECT * FROM variable_binding WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new VariableBindingDetails(Object, r));
}

public sealed class VariableBindingDetails : SemanticDetail
{
    internal VariableBindingDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public VariableSymbol Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Variable(value))!;
    public long Ordinal => Data.Long("ordinal");
    public VariableSymbol Variable => Object.Link(Data.OptionalLong("variable_id"), value => Object.Variable(value))!;
    public ExpressionOperation? Expression => Object.Link(Data.OptionalLong("expression_id"), value => Object.Expression(value));
}

public sealed class FieldSymbolDetails : SemanticDetail
{
    internal FieldSymbolDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType RecordType => Object.Link(Data.OptionalLong("record_type_id"), value => Object.Type(value))!;
    public CppType Type => Object.Link(Data.OptionalLong("type_id"), value => Object.Type(value))!;
    public Initialization? DefaultInitialization => Object.Link(Data.OptionalLong("default_initialization_id"), value => Object.Initialization(value));
    public bool IsMutable => Data.Bool("is_mutable");
    public bool IsBitField => Data.Bool("is_bit_field");
    public bool IsGeneratedBase => Data.Bool("is_generated_base");
    public bool IsGeneratedVptr => Data.Bool("is_generated_vptr");
}

public sealed class LabelSymbolDetails : SemanticDetail
{
    internal LabelSymbolDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public StatementOperation? Statement => Object.Link(Data.OptionalLong("statement_id"), value => Object.Statement(value));
    public Lifetime? Lifetime => Object.Link(Data.OptionalLong("lifetime_id"), value => Object.Lifetime(value));
    public bool IsBreak => Data.Bool("is_break");
    public bool IsContinue => Data.Bool("is_continue");
    public bool IsLeave => Data.Bool("is_leave");
}

public sealed class EnumSymbolDetails : SemanticDetail
{
    internal EnumSymbolDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType UnderlyingType => Object.Link(Data.OptionalLong("underlying_type_id"), value => Object.Type(value))!;
    public bool IsScoped => Data.Bool("is_scoped");
    public IReadOnlyList<EnumeratorDetails> EnumeratorRows => Object.List("SELECT * FROM enumerator WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new EnumeratorDetails(Object, r));
}

public sealed class EnumeratorDetails : SemanticDetail
{
    internal EnumeratorDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public EnumSymbolDetails Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Detail("enum_symbol", value, r => new EnumSymbolDetails(Object, r)))!;
    public long Ordinal => Data.Long("ordinal");
    public Symbol Symbol => Object.Link(Data.OptionalLong("symbol_id"), value => Object.Symbol(value))!;
    public Constant Constant => Object.Link(Data.OptionalLong("constant_id"), value => Object.Constant(value))!;
}

public sealed class AliasSymbolDetails : SemanticDetail
{
    internal AliasSymbolDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticEntity? Target => Object.Link(Data.OptionalLong("target_id"), value => Object.Entity(value));
    public CppType? UnderlyingType => Object.Link(Data.OptionalLong("underlying_type_id"), value => Object.Type(value));
}

public sealed class UsingDeclarationDetails : SemanticDetail
{
    internal UsingDeclarationDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticEntity? Target => Object.Link(Data.OptionalLong("target_id"), value => Object.Entity(value));
    public Scope? LexicalScope => Object.Link(Data.OptionalLong("lexical_scope_id"), value => Object.Scope(value));
}

public sealed class TypeDetails : SemanticDetail
{
    internal TypeDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public NameDetails? Name => Object.Link(Data.OptionalLong("name_id"), value => Object.Detail("name", value, r => new NameDetails(Object, r)));
    public SourceInfo? SourceOrigin => Object.Link(Data.OptionalLong("source_origin_id"), value => Object.Source(value));
    public BigInteger Size => RequiredNumber(Data.OptionalText("size"));
    public BigInteger Alignment => RequiredNumber(Data.OptionalText("alignment"));
    public bool Incomplete => Data.Bool("incomplete");
    public CppType? CanonicalType => Object.Link(Data.OptionalLong("canonical_type_id"), value => Object.Type(value));
    public CppType? DeclaredType => Object.Link(Data.OptionalLong("declared_type_id"), value => Object.Type(value));
    public bool IsDependent => Data.Bool("is_dependent");
    public bool IsAtomic => Data.Bool("is_atomic");
    public IReadOnlyList<DirectBaseDetails> DirectBaseRows => Object.List("SELECT * FROM direct_base WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new DirectBaseDetails(Object, r));
    public IReadOnlyList<VirtualMemberDetails> VirtualMemberRows => Object.List("SELECT * FROM virtual_member WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new VirtualMemberDetails(Object, r));
    public IReadOnlyList<LoweredTypeRelationshipDetails> LoweredTypeRelationshipRows => Object.List("SELECT * FROM lowered_type_relationship WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new LoweredTypeRelationshipDetails(Object, r));
}

public sealed class ArithmeticTypeDetails : SemanticDetail
{
    internal ArithmeticTypeDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind ArithmeticKind => Object.Kind(Data.Long("arithmetic_kind_id"));
    public SemanticKind? EdgIntegerKind => Object.Link(Data.OptionalLong("edg_integer_kind_id"), value => Object.Kind(value));
    public SemanticKind? EdgFloatKind => Object.Link(Data.OptionalLong("edg_float_kind_id"), value => Object.Kind(value));
    public SemanticKind? EdgFixedPrecision => Object.Link(Data.OptionalLong("edg_fixed_precision_id"), value => Object.Kind(value));
    public long? Bits => Data.OptionalLong("bits");
    public bool? Signed => Data.OptionalBool("signed");
    public long? Precision => Data.OptionalLong("precision");
    public long? Radix => Data.OptionalLong("radix");
    public long? Scale => Data.OptionalLong("scale");
    public bool IsBool => Data.Bool("is_bool");
    public bool IsWchar => Data.Bool("is_wchar");
    public bool IsChar8 => Data.Bool("is_char8");
    public bool IsChar16 => Data.Bool("is_char16");
    public bool IsChar32 => Data.Bool("is_char32");
    public bool IsMicrosoftSizedInteger => Data.Bool("is_microsoft_sized_integer");
}

public sealed class QualifiedTypeDetails : SemanticDetail
{
    internal QualifiedTypeDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType UnderlyingType => Object.Link(Data.OptionalLong("underlying_type_id"), value => Object.Type(value))!;
    public bool IsConst => Data.Bool("is_const");
    public bool IsVolatile => Data.Bool("is_volatile");
    public bool IsRestrict => Data.Bool("is_restrict");
    public long? AddressSpace => Data.OptionalLong("address_space");
    public bool IsShared => Data.Bool("is_shared");
    public BigInteger? SharedBlockSize => Number(Data.OptionalText("shared_block_size"));
}

public sealed class PointerTypeDetails : SemanticDetail
{
    internal PointerTypeDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType PointeeType => Object.Link(Data.OptionalLong("pointee_type_id"), value => Object.Type(value))!;
    public bool IsReference => Data.Bool("is_reference");
    public bool IsRvalueReference => Data.Bool("is_rvalue_reference");
    public bool IsHandle => Data.Bool("is_handle");
    public bool IsTrackingReference => Data.Bool("is_tracking_reference");
    public bool IsInterior => Data.Bool("is_interior");
    public bool IsPinned => Data.Bool("is_pinned");
    public bool IsPtr32 => Data.Bool("is_ptr32");
    public bool IsPtr64 => Data.Bool("is_ptr64");
    public bool IsSptr => Data.Bool("is_sptr");
    public bool IsUptr => Data.Bool("is_uptr");
    public VariableSymbol? BasedVariable => Object.Link(Data.OptionalLong("based_variable_id"), value => Object.Variable(value));
}

public sealed class MemberPointerTypeDetails : SemanticDetail
{
    internal MemberPointerTypeDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType DeclaringType => Object.Link(Data.OptionalLong("declaring_type_id"), value => Object.Type(value))!;
    public CppType MemberType => Object.Link(Data.OptionalLong("member_type_id"), value => Object.Type(value))!;
}

public sealed class ArrayTypeDetails : SemanticDetail
{
    internal ArrayTypeDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType ElementType => Object.Link(Data.OptionalLong("element_type_id"), value => Object.Type(value))!;
    public SemanticKind BoundKind => Object.Kind(Data.Long("bound_kind_id"));
    public BigInteger? Bound => Number(Data.OptionalText("bound"));
    public ExpressionOperation? BoundExpression => Object.Link(Data.OptionalLong("bound_expression_id"), value => Object.Expression(value));
    public VlaDimensionDetails? VlaDimension => Object.Link(Data.OptionalLong("vla_dimension_id"), value => Object.Detail("vla_dimension", value, r => new VlaDimensionDetails(Object, r)));
    public bool BoundIsZero => Data.Bool("bound_is_zero");
}

public sealed class RoutineTypeDetails : SemanticDetail
{
    internal RoutineTypeDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType ResultType => Object.Link(Data.OptionalLong("result_type_id"), value => Object.Type(value))!;
    public CppType? ThisType => Object.Link(Data.OptionalLong("this_type_id"), value => Object.Type(value));
    public SemanticKind? CallingConvention => Object.Link(Data.OptionalLong("calling_convention_id"), value => Object.Kind(value));
    public SemanticKind? LanguageLinkage => Object.Link(Data.OptionalLong("language_linkage_id"), value => Object.Kind(value));
    public ExceptionSpecificationDetails? ExceptionSpecification => Object.Link(Data.OptionalLong("exception_specification_id"), value => Object.Detail("exception_specification", value, r => new ExceptionSpecificationDetails(Object, r)));
    public bool IsVariadic => Data.Bool("is_variadic");
    public bool IsConst => Data.Bool("is_const");
    public bool IsVolatile => Data.Bool("is_volatile");
    public SemanticKind? RefQualifier => Object.Link(Data.OptionalLong("ref_qualifier_id"), value => Object.Kind(value));
    public bool HasPrototype => Data.Bool("has_prototype");
    public IReadOnlyList<RoutineTypeParameterDetails> RoutineTypeParameterRows => Object.List("SELECT * FROM routine_type_parameter WHERE owner_id=$owner ORDER BY role_id,ordinal", Data.Long("id"), r => new RoutineTypeParameterDetails(Object, r));
}

public sealed class RoutineTypeParameterDetails : SemanticDetail
{
    internal RoutineTypeParameterDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public RoutineTypeDetails Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Detail("routine_type", value, r => new RoutineTypeDetails(Object, r)))!;
    public long Ordinal => Data.Long("ordinal");
    public CppType Type => Object.Link(Data.OptionalLong("type_id"), value => Object.Type(value))!;
    public SemanticKind Role => Object.Kind(Data.Long("role_id"));
    public ExpressionOperation? DefaultExpression => Object.Link(Data.OptionalLong("default_expression_id"), value => Object.Expression(value));
    public bool PassedIndirectly => Data.Bool("passed_indirectly");
}

public sealed class RecordTypeDetails : SemanticDetail
{
    internal RecordTypeDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public Scope? Scope => Object.Link(Data.OptionalLong("scope_id"), value => Object.Scope(value));
    public SemanticKind DefinitionState => Object.Kind(Data.Long("definition_state_id"));
    public TemplateInstantiation? Instance => Object.Link(Data.OptionalLong("instance_id"), value => Object.Instance(value));
    public CppType? SubobjectPartner => Object.Link(Data.OptionalLong("subobject_partner_id"), value => Object.Type(value));
    public bool IsLambda => Data.Bool("is_lambda");
    public bool IsEmpty => Data.Bool("is_empty");
    public bool IsAbstract => Data.Bool("is_abstract");
}

public sealed class TypeReferenceDetails : SemanticDetail
{
    internal TypeReferenceDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType UnderlyingType => Object.Link(Data.OptionalLong("underlying_type_id"), value => Object.Type(value))!;
    public SemanticKind ReferenceKind => Object.Kind(Data.Long("reference_kind_id"));
    public SemanticEntity? TypedefSymbol => Object.Link(Data.OptionalLong("typedef_symbol_id"), value => Object.Entity(value));
    public ExpressionOperation? DecltypeExpression => Object.Link(Data.OptionalLong("decltype_expression_id"), value => Object.Expression(value));
}

public sealed class VectorTypeDetails : SemanticDetail
{
    internal VectorTypeDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType ElementType => Object.Link(Data.OptionalLong("element_type_id"), value => Object.Type(value))!;
    public BigInteger? ElementCount => Number(Data.OptionalText("element_count"));
    public SemanticKind VectorKind => Object.Kind(Data.Long("vector_kind_id"));
    public CppType? ScalableCountType => Object.Link(Data.OptionalLong("scalable_count_type_id"), value => Object.Type(value));
    public Constant? SizeConstant => Object.Link(Data.OptionalLong("size_constant_id"), value => Object.Constant(value));
    public bool IsBoolean => Data.Bool("is_boolean");
    public long? TupleElements => Data.OptionalLong("tuple_elements");
    public long? LengthMultiplier => Data.OptionalLong("length_multiplier");
}

public sealed class DependentTypeDetails : SemanticDetail
{
    internal DependentTypeDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind? ParameterKind => Object.Link(Data.OptionalLong("parameter_kind_id"), value => Object.Kind(value));
    public long? ParameterNumber => Data.OptionalLong("parameter_number");
    public long? ParameterDepth => Data.OptionalLong("parameter_depth");
    public CppType? ParentType => Object.Link(Data.OptionalLong("parent_type_id"), value => Object.Type(value));
    public ExpressionOperation? Expression => Object.Link(Data.OptionalLong("expression_id"), value => Object.Expression(value));
    public ExpressionOperation? Constraint => Object.Link(Data.OptionalLong("constraint_id"), value => Object.Expression(value));
    public Constant? BitWidthConstant => Object.Link(Data.OptionalLong("bit_width_constant_id"), value => Object.Constant(value));
}

public sealed class ManagedTypeDetails : SemanticDetail
{
    internal ManagedTypeDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind ManagedKind => Object.Kind(Data.Long("managed_kind_id"));
    public CppType? UnderlyingType => Object.Link(Data.OptionalLong("underlying_type_id"), value => Object.Type(value));
    public long? Rank => Data.OptionalLong("rank");
    public string? AssemblyName => Object.Text(Data.OptionalLong("assembly_name_id"));
}

public sealed class RecordLayoutDetails : SemanticDetail
{
    internal RecordLayoutDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType Type => Object.Link(Data.OptionalLong("type_id"), value => Object.Type(value))!;
    public BigInteger? NonvirtualSize => Number(Data.OptionalText("nonvirtual_size"));
    public BigInteger? NonvirtualAlignment => Number(Data.OptionalText("nonvirtual_alignment"));
    public bool IsComplete => Data.Bool("is_complete");
    public IReadOnlyList<FieldLayoutDetails> FieldLayoutRows => Object.List("SELECT * FROM field_layout WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new FieldLayoutDetails(Object, r));
}

public sealed class FieldLayoutDetails : SemanticDetail
{
    internal FieldLayoutDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public RecordLayout Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Layout(value))!;
    public long Ordinal => Data.Long("ordinal");
    public FieldSymbol Field => Object.Link(Data.OptionalLong("field_id"), value => Object.Field(value))!;
    public BigInteger Offset => RequiredNumber(Data.OptionalText("offset"));
    public BigInteger? BitOffset => Number(Data.OptionalText("bit_offset"));
    public BigInteger? BitWidth => Number(Data.OptionalText("bit_width"));
    public CppType? ContainerType => Object.Link(Data.OptionalLong("container_type_id"), value => Object.Type(value));
}

public sealed class DirectBaseDetails : SemanticDetail
{
    internal DirectBaseDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Type(value))!;
    public long Ordinal => Data.Long("ordinal");
    public CppType BaseType => Object.Link(Data.OptionalLong("base_type_id"), value => Object.Type(value))!;
    public SemanticKind? Access => Object.Link(Data.OptionalLong("access_id"), value => Object.Kind(value));
    public bool IsVirtual => Data.Bool("is_virtual");
    public BaseSubobject? Subobject => Object.Link(Data.OptionalLong("subobject_id"), value => Object.Subobject(value));
}

public sealed class BaseSubobjectDetails : SemanticDetail
{
    internal BaseSubobjectDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType MostDerivedType => Object.Link(Data.OptionalLong("most_derived_type_id"), value => Object.Type(value))!;
    public CppType BaseType => Object.Link(Data.OptionalLong("base_type_id"), value => Object.Type(value))!;
    public BigInteger? Offset => Number(Data.OptionalText("offset"));
    public bool IsVirtual => Data.Bool("is_virtual");
    public bool IsDirect => Data.Bool("is_direct");
    public BaseSubobject? Parent => Object.Link(Data.OptionalLong("parent_id"), value => Object.Subobject(value));
    public FieldSymbol? BaseField => Object.Link(Data.OptionalLong("base_field_id"), value => Object.Field(value));
}

public sealed class BaseDerivationDetails : SemanticDetail
{
    internal BaseDerivationDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public BaseSubobject BaseSubobject => Object.Link(Data.OptionalLong("base_subobject_id"), value => Object.Subobject(value))!;
    public long Ordinal => Data.Long("ordinal");
    public bool IsDirect => Data.Bool("is_direct");
    public bool IsPreferred => Data.Bool("is_preferred");
    public SemanticKind? Access => Object.Link(Data.OptionalLong("access_id"), value => Object.Kind(value));
    public long PathTailOrdinal => Data.Long("path_tail_ordinal");
    public IReadOnlyList<BaseDerivationStepDetails> BaseDerivationStepRows => Object.List("SELECT * FROM base_derivation_step WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new BaseDerivationStepDetails(Object, r));
}

public sealed class BaseDerivationStepDetails : SemanticDetail
{
    internal BaseDerivationStepDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public BaseDerivation Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Derivation(value))!;
    public long Ordinal => Data.Long("ordinal");
    public BaseSubobject Subobject => Object.Link(Data.OptionalLong("subobject_id"), value => Object.Subobject(value))!;
}

public sealed class VirtualMemberDetails : SemanticDetail
{
    internal VirtualMemberDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Type(value))!;
    public long Ordinal => Data.Long("ordinal");
    public FunctionSymbol Routine => Object.Link(Data.OptionalLong("routine_id"), value => Object.Function(value))!;
    public bool IsInherited => Data.Bool("is_inherited");
    public bool IsPure => Data.Bool("is_pure");
    public bool IsFinal => Data.Bool("is_final");
    public BigInteger? Slot => Number(Data.OptionalText("slot"));
}

public sealed class OverrideRelationDetails : SemanticDetail
{
    internal OverrideRelationDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public FunctionSymbol Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Function(value))!;
    public long Ordinal => Data.Long("ordinal");
    public FunctionSymbol Overridden => Object.Link(Data.OptionalLong("overridden_id"), value => Object.Function(value))!;
    public BaseSubobject? Subobject => Object.Link(Data.OptionalLong("subobject_id"), value => Object.Subobject(value));
    public BaseSubobject? OverridingSubobject => Object.Link(Data.OptionalLong("overriding_subobject_id"), value => Object.Subobject(value));
    public BaseSubobject? ReturnAdjustmentSubobject => Object.Link(Data.OptionalLong("return_adjustment_subobject_id"), value => Object.Subobject(value));
    public BigInteger? Adjustment => Number(Data.OptionalText("adjustment"));
    public BigInteger? ReturnAdjustment => Number(Data.OptionalText("return_adjustment"));
}

public sealed class LoweredTypeRelationshipDetails : SemanticDetail
{
    internal LoweredTypeRelationshipDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Type(value))!;
    public long Ordinal => Data.Long("ordinal");
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public CppType Target => Object.Link(Data.OptionalLong("target_id"), value => Object.Type(value))!;
}

public sealed class AbiRoutineRelationshipDetails : SemanticDetail
{
    internal AbiRoutineRelationshipDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public FunctionSymbol Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Function(value))!;
    public long Ordinal => Data.Long("ordinal");
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public FunctionSymbol Target => Object.Link(Data.OptionalLong("target_id"), value => Object.Function(value))!;
    public BigInteger? Adjustment => Number(Data.OptionalText("adjustment"));
    public BigInteger? ReturnAdjustment => Number(Data.OptionalText("return_adjustment"));
}

public sealed class VirtualTableDetails : SemanticDetail
{
    internal VirtualTableDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public VariableSymbol Variable => Object.Link(Data.OptionalLong("variable_id"), value => Object.Variable(value))!;
    public CppType? RecordType => Object.Link(Data.OptionalLong("record_type_id"), value => Object.Type(value));
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public IReadOnlyList<VirtualTableEntryDetails> VirtualTableEntryRows => Object.List("SELECT * FROM virtual_table_entry WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new VirtualTableEntryDetails(Object, r));
}

public sealed class VirtualTableEntryDetails : SemanticDetail
{
    internal VirtualTableEntryDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public VirtualTableDetails Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Detail("virtual_table", value, r => new VirtualTableDetails(Object, r)))!;
    public long Ordinal => Data.Long("ordinal");
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public FunctionSymbol? Routine => Object.Link(Data.OptionalLong("routine_id"), value => Object.Function(value));
    public SemanticEntity? Entity => Object.Link(Data.OptionalLong("entity_id"), value => Object.Entity(value));
    public BigInteger? Adjustment => Number(Data.OptionalText("adjustment"));
    public Constant? Constant => Object.Link(Data.OptionalLong("constant_id"), value => Object.Constant(value));
}

public sealed class SemanticNodeDetails : SemanticDetail
{
    internal SemanticNodeDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public SourceInfo? SourceOrigin => Object.Link(Data.OptionalLong("source_origin_id"), value => Object.Source(value));
    public bool CompilerGenerated => Data.Bool("compiler_generated");
    public SemanticKind? SynthesisKind => Object.Link(Data.OptionalLong("synthesis_kind_id"), value => Object.Kind(value));
    public SemanticKind RepresentationRole => Object.Kind(Data.Long("representation_role_id"));
    public IReadOnlyList<StructuralChildDetails> StructuralChildRows => Object.List("SELECT * FROM structural_child WHERE owner_id=$owner ORDER BY role_id,ordinal", Data.Long("id"), r => new StructuralChildDetails(Object, r));
    public IReadOnlyList<LifetimeTransitionDetails> LifetimeTransitionRows => Object.List("SELECT * FROM lifetime_transition WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new LifetimeTransitionDetails(Object, r));
}

public sealed class StructuralChildDetails : SemanticDetail
{
    internal StructuralChildDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticNode Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Node(value))!;
    public SemanticKind Role => Object.Kind(Data.Long("role_id"));
    public long Ordinal => Data.Long("ordinal");
    public SemanticNode Child => Object.Link(Data.OptionalLong("child_id"), value => Object.Node(value))!;
}

public sealed class SemanticReferenceDetails : SemanticDetail
{
    internal SemanticReferenceDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticEntity Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Entity(value))!;
    public SemanticKind Role => Object.Kind(Data.Long("role_id"));
    public long Ordinal => Data.Long("ordinal");
    public SemanticEntity Target => Object.Link(Data.OptionalLong("target_id"), value => Object.Entity(value))!;
}

public sealed class ExpressionDetails : SemanticDetail
{
    internal ExpressionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType Type => Object.Link(Data.OptionalLong("type_id"), value => Object.Type(value))!;
    public SemanticKind ValueCategory => Object.Kind(Data.Long("value_category_id"));
    public CppType? OriginalLvalueType => Object.Link(Data.OptionalLong("original_lvalue_type_id"), value => Object.Type(value));
    public bool ResultUnused => Data.Bool("result_unused");
    public bool InitializationGuard => Data.Bool("initialization_guard");
    public bool GeneratedDefaultArgument => Data.Bool("generated_default_argument");
    public bool VolatileFetch => Data.Bool("volatile_fetch");
    public bool IsPackExpansion => Data.Bool("is_pack_expansion");
    public bool IsParenthesized => Data.Bool("is_parenthesized");
    public bool IsStaticCast => Data.Bool("is_static_cast");
    public bool IsFunctionalCast => Data.Bool("is_functional_cast");
    public bool IsBraceCast => Data.Bool("is_brace_cast");
    public bool IsSafeCast => Data.Bool("is_safe_cast");
    public bool IsCliTypeid => Data.Bool("is_cli_typeid");
    public bool IsCliParamArrayElement => Data.Bool("is_cli_param_array_element");
    public bool IsObjectlessMemberReference => Data.Bool("is_objectless_member_reference");
    public bool IsGnuExtension => Data.Bool("is_gnu_extension");
    public bool TypeDefinitionNeeded => Data.Bool("type_definition_needed");
    public bool BooleanNotNormalized => Data.Bool("boolean_not_normalized");
    public IReadOnlyList<ExpressionOperandDetails> ExpressionOperandRows => Object.List("SELECT * FROM expression_operand WHERE owner_id=$owner ORDER BY role_id,ordinal", Data.Long("id"), r => new ExpressionOperandDetails(Object, r));
    public IReadOnlyList<ExpressionTemplateArgumentDetails> ExpressionTemplateArgumentRows => Object.List("SELECT * FROM expression_template_argument WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new ExpressionTemplateArgumentDetails(Object, r));
}

public sealed class StatementDetails : SemanticDetail
{
    internal StatementDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public ExpressionOperation? Expression => Object.Link(Data.OptionalLong("expression_id"), value => Object.Expression(value));
    public StatementOperation? Parent => Object.Link(Data.OptionalLong("parent_id"), value => Object.Statement(value));
    public Lifetime? Lifetime => Object.Link(Data.OptionalLong("lifetime_id"), value => Object.Lifetime(value));
    public bool IsGuard => Data.Bool("is_guard");
    public bool IsDeclaredConstexpr => Data.Bool("is_declared_constexpr");
    public bool LoweringGenerated => Data.Bool("lowering_generated");
}

public sealed class ExpressionOperandDetails : SemanticDetail
{
    internal ExpressionOperandDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public ExpressionOperation Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Expression(value))!;
    public SemanticKind Role => Object.Kind(Data.Long("role_id"));
    public long Ordinal => Data.Long("ordinal");
    public ExpressionOperation Expression => Object.Link(Data.OptionalLong("expression_id"), value => Object.Expression(value))!;
}

public sealed class OperatorExpressionDetails : SemanticDetail
{
    internal OperatorExpressionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind OperatorKind => Object.Kind(Data.Long("operator_kind_id"));
    public SemanticKind? OperationTypeKind => Object.Link(Data.OptionalLong("operation_type_kind_id"), value => Object.Kind(value));
    public bool EvalLeftToRight => Data.Bool("eval_left_to_right");
    public bool EvalRightToLeft => Data.Bool("eval_right_to_left");
    public bool PointerOperandIsSecond => Data.Bool("pointer_operand_is_second");
    public bool IsReinterpretLikeCast => Data.Bool("is_reinterpret_like_cast");
    public bool RequiresRuntimeCastCheck => Data.Bool("requires_runtime_cast_check");
    public bool ReturnsLvalue => Data.Bool("returns_lvalue");
    public bool IsReinterpretCast => Data.Bool("is_reinterpret_cast");
    public bool IsReferenceCast => Data.Bool("is_reference_cast");
    public bool IsRvalueReferenceCast => Data.Bool("is_rvalue_reference_cast");
    public bool IsConstCast => Data.Bool("is_const_cast");
    public bool IsTrackingReferenceCast => Data.Bool("is_tracking_reference_cast");
    public bool IsGnuTwoOperandConditional => Data.Bool("is_gnu_two_operand_conditional");
    public bool CallUsesOperatorSyntax => Data.Bool("call_uses_operator_syntax");
    public bool ImplicitMemberNaming => Data.Bool("implicit_member_naming");
    public bool ImplicitExplicitCastStep => Data.Bool("implicit_explicit_cast_step");
    public bool IsConversionCall => Data.Bool("is_conversion_call");
    public bool ArgumentDependentLookupSuppressed => Data.Bool("argument_dependent_lookup_suppressed");
    public bool QualifiedFunctionName => Data.Bool("qualified_function_name");
    public bool RetainedVirtualCall => Data.Bool("retained_virtual_call");
    public SemanticKind? RewrittenPropertyKind => Object.Link(Data.OptionalLong("rewritten_property_kind_id"), value => Object.Kind(value));
}

public sealed class CallExpressionDetails : SemanticDetail
{
    internal CallExpressionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public ExpressionOperation Callee => Object.Link(Data.OptionalLong("callee_id"), value => Object.Expression(value))!;
    public FunctionSymbol? TargetRoutine => Object.Link(Data.OptionalLong("target_routine_id"), value => Object.Function(value));
    public FunctionSymbol? SourceTargetRoutine => Object.Link(Data.OptionalLong("source_target_routine_id"), value => Object.Function(value));
    public ExpressionOperation? Receiver => Object.Link(Data.OptionalLong("receiver_id"), value => Object.Expression(value));
    public CppType? FunctionType => Object.Link(Data.OptionalLong("function_type_id"), value => Object.Type(value));
    public SemanticKind CallKind => Object.Kind(Data.Long("call_kind_id"));
    public SemanticKind? SourceDispatchKind => Object.Link(Data.OptionalLong("source_dispatch_kind_id"), value => Object.Kind(value));
    public ExpressionOperation? ReturnSlot => Object.Link(Data.OptionalLong("return_slot_id"), value => Object.Expression(value));
    public IReadOnlyList<CallArgumentDetails> CallArgumentRows => Object.List("SELECT * FROM call_argument WHERE owner_id=$owner ORDER BY role_id,ordinal", Data.Long("id"), r => new CallArgumentDetails(Object, r));
}

public sealed class CallArgumentDetails : SemanticDetail
{
    internal CallArgumentDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CallExpressionDetails Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Detail("call_expression", value, r => new CallExpressionDetails(Object, r)))!;
    public long Ordinal => Data.Long("ordinal");
    public ExpressionOperation Expression => Object.Link(Data.OptionalLong("expression_id"), value => Object.Expression(value))!;
    public VariableSymbol? Parameter => Object.Link(Data.OptionalLong("parameter_id"), value => Object.Variable(value));
    public SemanticKind? Role => Object.Link(Data.OptionalLong("role_id"), value => Object.Kind(value));
}

public sealed class ReferenceExpressionDetails : SemanticDetail
{
    internal ReferenceExpressionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticEntity? Target => Object.Link(Data.OptionalLong("target_id"), value => Object.Entity(value));
    public SemanticKind ReferenceKind => Object.Kind(Data.Long("reference_kind_id"));
    public NameReferenceDetails? NameReference => Object.Link(Data.OptionalLong("name_reference_id"), value => Object.Detail("name_reference", value, r => new NameReferenceDetails(Object, r)));
    public BaseSubobject? Subobject => Object.Link(Data.OptionalLong("subobject_id"), value => Object.Subobject(value));
}

public sealed class NameReferenceDetails : SemanticDetail
{
    internal NameReferenceDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind? Kind => Object.Link(Data.OptionalLong("kind_id"), value => Object.Kind(value));
    public SemanticKind? BuiltinKind => Object.Link(Data.OptionalLong("builtin_kind_id"), value => Object.Kind(value));
    public FieldSymbol? Property => Object.Link(Data.OptionalLong("property_id"), value => Object.Field(value));
    public CppType? DestructorType => Object.Link(Data.OptionalLong("destructor_type_id"), value => Object.Type(value));
}

public sealed class ConstantExpressionDetails : SemanticDetail
{
    internal ConstantExpressionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public Constant Constant => Object.Link(Data.OptionalLong("constant_id"), value => Object.Constant(value))!;
    public NameReferenceDetails? NameReference => Object.Link(Data.OptionalLong("name_reference_id"), value => Object.Detail("name_reference", value, r => new NameReferenceDetails(Object, r)));
}

public sealed class ConversionExpressionDetails : SemanticDetail
{
    internal ConversionExpressionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public ExpressionOperation Operand => Object.Link(Data.OptionalLong("operand_id"), value => Object.Expression(value))!;
    public CppType? SourceType => Object.Link(Data.OptionalLong("source_type_id"), value => Object.Type(value));
    public CppType TargetType => Object.Link(Data.OptionalLong("target_type_id"), value => Object.Type(value))!;
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public FunctionSymbol? Routine => Object.Link(Data.OptionalLong("routine_id"), value => Object.Function(value));
    public SemanticEntity? BasePath => Object.Link(Data.OptionalLong("base_path_id"), value => Object.Entity(value));
}

public sealed class BlockStatementDetails : SemanticDetail
{
    internal BlockStatementDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public Scope? Scope => Object.Link(Data.OptionalLong("scope_id"), value => Object.Scope(value));
    public Lifetime? Lifetime => Object.Link(Data.OptionalLong("lifetime_id"), value => Object.Lifetime(value));
    public bool EndReachable => Data.Bool("end_reachable");
    public bool IsStatementExpression => Data.Bool("is_statement_expression");
    public bool ImplicitScopeNotAllowed => Data.Bool("implicit_scope_not_allowed");
    public SemanticKind? UpcAccessMethod => Object.Link(Data.OptionalLong("upc_access_method_id"), value => Object.Kind(value));
    public IReadOnlyList<BlockItemDetails> BlockItemRows => Object.List("SELECT * FROM block_item WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new BlockItemDetails(Object, r));
}

public sealed class InitializationStatementDetails : SemanticDetail
{
    internal InitializationStatementDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public Initialization Initialization => Object.Link(Data.OptionalLong("initialization_id"), value => Object.Initialization(value))!;
}

public sealed class BlockItemDetails : SemanticDetail
{
    internal BlockItemDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public BlockStatementDetails Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Detail("block_statement", value, r => new BlockStatementDetails(Object, r)))!;
    public long Ordinal => Data.Long("ordinal");
    public StatementOperation Statement => Object.Link(Data.OptionalLong("statement_id"), value => Object.Statement(value))!;
}

public sealed class IfStatementDetails : SemanticDetail
{
    internal IfStatementDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public StatementOperation? Then => Object.Link(Data.OptionalLong("then_id"), value => Object.Statement(value));
    public StatementOperation? Else => Object.Link(Data.OptionalLong("else_id"), value => Object.Statement(value));
    public ExpressionOperation? Condition => Object.Link(Data.OptionalLong("condition_id"), value => Object.Expression(value));
    public StatementOperation? Initialization => Object.Link(Data.OptionalLong("initialization_id"), value => Object.Statement(value));
    public bool? SelectionKnown => Data.OptionalBool("selection_known");
    public bool? SelectedThen => Data.OptionalBool("selected_then");
}

public sealed class LoopStatementDetails : SemanticDetail
{
    internal LoopStatementDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public StatementOperation? Body => Object.Link(Data.OptionalLong("body_id"), value => Object.Statement(value));
    public StatementOperation? Initialization => Object.Link(Data.OptionalLong("initialization_id"), value => Object.Statement(value));
    public ExpressionOperation? Condition => Object.Link(Data.OptionalLong("condition_id"), value => Object.Expression(value));
    public ExpressionOperation? Increment => Object.Link(Data.OptionalLong("increment_id"), value => Object.Expression(value));
    public Scope? InitScope => Object.Link(Data.OptionalLong("init_scope_id"), value => Object.Scope(value));
    public SourcePosition? TestPosition => Object.Link(Data.OptionalLong("test_position_id"), value => Object.Position(value));
    public ExpressionOperation? Affinity => Object.Link(Data.OptionalLong("affinity_id"), value => Object.Expression(value));
}

public sealed class RangeForDetails : SemanticDetail
{
    internal RangeForDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public StatementOperation? Body => Object.Link(Data.OptionalLong("body_id"), value => Object.Statement(value));
    public VariableSymbol? RangeVariable => Object.Link(Data.OptionalLong("range_variable_id"), value => Object.Variable(value));
    public VariableSymbol? BeginVariable => Object.Link(Data.OptionalLong("begin_variable_id"), value => Object.Variable(value));
    public VariableSymbol? EndVariable => Object.Link(Data.OptionalLong("end_variable_id"), value => Object.Variable(value));
    public VariableSymbol? IteratorVariable => Object.Link(Data.OptionalLong("iterator_variable_id"), value => Object.Variable(value));
    public ExpressionOperation? Comparison => Object.Link(Data.OptionalLong("comparison_id"), value => Object.Expression(value));
    public ExpressionOperation? Increment => Object.Link(Data.OptionalLong("increment_id"), value => Object.Expression(value));
    public StatementOperation? Initialization => Object.Link(Data.OptionalLong("initialization_id"), value => Object.Statement(value));
    public Scope? Scope => Object.Link(Data.OptionalLong("scope_id"), value => Object.Scope(value));
    public Scope? IteratorScope => Object.Link(Data.OptionalLong("iterator_scope_id"), value => Object.Scope(value));
    public bool UseAwait => Data.Bool("use_await");
}

public sealed class ForEachStatementDetails : SemanticDetail
{
    internal ForEachStatementDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public StatementOperation? Body => Object.Link(Data.OptionalLong("body_id"), value => Object.Statement(value));
    public ExpressionOperation? Collection => Object.Link(Data.OptionalLong("collection_id"), value => Object.Expression(value));
    public SemanticKind PatternKind => Object.Kind(Data.Long("pattern_kind_id"));
    public VariableSymbol? Iterator => Object.Link(Data.OptionalLong("iterator_id"), value => Object.Variable(value));
    public ExpressionOperation? Initialization => Object.Link(Data.OptionalLong("initialization_id"), value => Object.Expression(value));
    public ExpressionOperation? Test => Object.Link(Data.OptionalLong("test_id"), value => Object.Expression(value));
    public ExpressionOperation? Increment => Object.Link(Data.OptionalLong("increment_id"), value => Object.Expression(value));
}

public sealed class SwitchStatementDetails : SemanticDetail
{
    internal SwitchStatementDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public StatementOperation? Body => Object.Link(Data.OptionalLong("body_id"), value => Object.Statement(value));
    public ExpressionOperation? Selector => Object.Link(Data.OptionalLong("selector_id"), value => Object.Expression(value));
    public LabelSymbol? DefaultLabel => Object.Link(Data.OptionalLong("default_label_id"), value => Object.Label(value));
    public IReadOnlyList<SwitchCaseDetails> SwitchCaseRows => Object.List("SELECT * FROM switch_case WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new SwitchCaseDetails(Object, r));
}

public sealed class SwitchCaseDetails : SemanticDetail
{
    internal SwitchCaseDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SwitchStatementDetails Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Detail("switch_statement", value, r => new SwitchStatementDetails(Object, r)))!;
    public long Ordinal => Data.Long("ordinal");
    public StatementOperation? Statement => Object.Link(Data.OptionalLong("statement_id"), value => Object.Statement(value));
    public LabelSymbol? Label => Object.Link(Data.OptionalLong("label_id"), value => Object.Label(value));
    public Constant? LowConstant => Object.Link(Data.OptionalLong("low_constant_id"), value => Object.Constant(value));
    public Constant? HighConstant => Object.Link(Data.OptionalLong("high_constant_id"), value => Object.Constant(value));
    public bool IsDefault => Data.Bool("is_default");
}

public sealed class JumpStatementDetails : SemanticDetail
{
    internal JumpStatementDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public ExpressionOperation? Value => Object.Link(Data.OptionalLong("value_id"), value => Object.Expression(value));
    public LabelSymbol? Label => Object.Link(Data.OptionalLong("label_id"), value => Object.Label(value));
    public Initialization? Initialization => Object.Link(Data.OptionalLong("initialization_id"), value => Object.Initialization(value));
    public Lifetime? SourceLifetime => Object.Link(Data.OptionalLong("source_lifetime_id"), value => Object.Lifetime(value));
    public Lifetime? TargetLifetime => Object.Link(Data.OptionalLong("target_lifetime_id"), value => Object.Lifetime(value));
}

public sealed class LabelStatementDetails : SemanticDetail
{
    internal LabelStatementDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public LabelSymbol Label => Object.Link(Data.OptionalLong("label_id"), value => Object.Label(value))!;
    public StatementOperation? Statement => Object.Link(Data.OptionalLong("statement_id"), value => Object.Statement(value));
}

public sealed class TryStatementDetails : SemanticDetail
{
    internal TryStatementDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public StatementOperation Body => Object.Link(Data.OptionalLong("body_id"), value => Object.Statement(value))!;
    public bool IsFunctionTry => Data.Bool("is_function_try");
    public Lifetime? Lifetime => Object.Link(Data.OptionalLong("lifetime_id"), value => Object.Lifetime(value));
    public StatementOperation? FinallyStatement => Object.Link(Data.OptionalLong("finally_statement_id"), value => Object.Statement(value));
    public IReadOnlyList<HandlerDetails> HandlerRows => Object.List("SELECT * FROM handler WHERE try_id=$owner ORDER BY ordinal,id", Data.Long("id"), r => new HandlerDetails(Object, r), "handler");
}

public sealed class HandlerDetails : SemanticDetail
{
    internal HandlerDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public TryStatementDetails Try => Object.Link(Data.OptionalLong("try_id"), value => Object.Detail("try_statement", value, r => new TryStatementDetails(Object, r)))!;
    public long Ordinal => Data.Long("ordinal");
    public StatementOperation Body => Object.Link(Data.OptionalLong("body_id"), value => Object.Statement(value))!;
    public CppType? CaughtType => Object.Link(Data.OptionalLong("caught_type_id"), value => Object.Type(value));
    public VariableSymbol? Parameter => Object.Link(Data.OptionalLong("parameter_id"), value => Object.Variable(value));
    public Initialization? ParameterInitialization => Object.Link(Data.OptionalLong("parameter_initialization_id"), value => Object.Initialization(value));
    public VariableSymbol? Typeinfo => Object.Link(Data.OptionalLong("typeinfo_id"), value => Object.Variable(value));
    public Lifetime? Lifetime => Object.Link(Data.OptionalLong("lifetime_id"), value => Object.Lifetime(value));
    public SourcePosition? SourcePosition => Object.Link(Data.OptionalLong("source_position_id"), value => Object.Position(value));
    public IReadOnlyList<EhHandlerRegionDetails> EhHandlerRegionRows => Object.List("SELECT * FROM eh_handler_region WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new EhHandlerRegionDetails(Object, r));
}

public sealed class MicrosoftTryStatementDetails : SemanticDetail
{
    internal MicrosoftTryStatementDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public StatementOperation Body => Object.Link(Data.OptionalLong("body_id"), value => Object.Statement(value))!;
    public ExpressionOperation? Filter => Object.Link(Data.OptionalLong("filter_id"), value => Object.Expression(value));
    public StatementOperation? Handler => Object.Link(Data.OptionalLong("handler_id"), value => Object.Statement(value));
    public StatementOperation? Finally => Object.Link(Data.OptionalLong("finally_id"), value => Object.Statement(value));
    public bool IsFinally => Data.Bool("is_finally");
}

public sealed class ThrowExpressionDetails : SemanticDetail
{
    internal ThrowExpressionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public ExpressionOperation? Operand => Object.Link(Data.OptionalLong("operand_id"), value => Object.Expression(value));
    public CppType? ThrownType => Object.Link(Data.OptionalLong("thrown_type_id"), value => Object.Type(value));
    public Initialization? Initialization => Object.Link(Data.OptionalLong("initialization_id"), value => Object.Initialization(value));
    public FunctionSymbol? Destructor => Object.Link(Data.OptionalLong("destructor_id"), value => Object.Function(value));
    public bool IsRethrow => Data.Bool("is_rethrow");
    public IReadOnlyList<ThrowAccessibleBaseDetails> ThrowAccessibleBaseRows => Object.List("SELECT * FROM throw_accessible_base WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new ThrowAccessibleBaseDetails(Object, r));
}

public sealed class ThrowAccessibleBaseDetails : SemanticDetail
{
    internal ThrowAccessibleBaseDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public ThrowExpressionDetails Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Detail("throw_expression", value, r => new ThrowExpressionDetails(Object, r)))!;
    public long Ordinal => Data.Long("ordinal");
    public BaseSubobject Subobject => Object.Link(Data.OptionalLong("subobject_id"), value => Object.Subobject(value))!;
}

public sealed class TypeQueryExpressionDetails : SemanticDetail
{
    internal TypeQueryExpressionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind QueryKind => Object.Kind(Data.Long("query_kind_id"));
    public CppType? OperandType => Object.Link(Data.OptionalLong("operand_type_id"), value => Object.Type(value));
    public ExpressionOperation? OperandExpression => Object.Link(Data.OptionalLong("operand_expression_id"), value => Object.Expression(value));
    public bool IsDynamic => Data.Bool("is_dynamic");
    public bool IsStandardAlignof => Data.Bool("is_standard_alignof");
    public VlaDimensionDetails? VlaDimension => Object.Link(Data.OptionalLong("vla_dimension_id"), value => Object.Detail("vla_dimension", value, r => new VlaDimensionDetails(Object, r)));
}

public sealed class TemporaryExpressionDetails : SemanticDetail
{
    internal TemporaryExpressionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public Initialization? Initialization => Object.Link(Data.OptionalLong("initialization_id"), value => Object.Initialization(value));
    public TemporaryDetails? Temporary => Object.Link(Data.OptionalLong("temporary_id"), value => Object.Detail("temporary", value, r => new TemporaryDetails(Object, r)));
    public CppType? SourceType => Object.Link(Data.OptionalLong("source_type_id"), value => Object.Type(value));
}

public sealed class ConditionExpressionDetails : SemanticDetail
{
    internal ConditionExpressionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public ExpressionOperation? Expression => Object.Link(Data.OptionalLong("expression_id"), value => Object.Expression(value));
    public StatementOperation? Initialization => Object.Link(Data.OptionalLong("initialization_id"), value => Object.Statement(value));
    public Initialization? DynamicInitialization => Object.Link(Data.OptionalLong("dynamic_initialization_id"), value => Object.Initialization(value));
    public Scope? Scope => Object.Link(Data.OptionalLong("scope_id"), value => Object.Scope(value));
    public VariableSymbol? Variable => Object.Link(Data.OptionalLong("variable_id"), value => Object.Variable(value));
}

public sealed class LifetimeExpressionDetails : SemanticDetail
{
    internal LifetimeExpressionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public ExpressionOperation Expression => Object.Link(Data.OptionalLong("expression_id"), value => Object.Expression(value))!;
    public Lifetime? Lifetime => Object.Link(Data.OptionalLong("lifetime_id"), value => Object.Lifetime(value));
}

public sealed class StatementExpressionDetails : SemanticDetail
{
    internal StatementExpressionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public StatementOperation Statement => Object.Link(Data.OptionalLong("statement_id"), value => Object.Statement(value))!;
    public ExpressionOperation? Result => Object.Link(Data.OptionalLong("result_id"), value => Object.Expression(value));
}

public sealed class ReuseExpressionDetails : SemanticDetail
{
    internal ReuseExpressionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public Initialization Initialization => Object.Link(Data.OptionalLong("initialization_id"), value => Object.Initialization(value))!;
}

public sealed class BuiltinExpressionDetails : SemanticDetail
{
    internal BuiltinExpressionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public ExpressionOperation? SelectedExpression => Object.Link(Data.OptionalLong("selected_expression_id"), value => Object.Expression(value));
    public Constant? ResultConstant => Object.Link(Data.OptionalLong("result_constant_id"), value => Object.Constant(value));
}

public sealed class ParameterReferenceExpressionDetails : SemanticDetail
{
    internal ParameterReferenceExpressionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public long ParameterNumber => Data.Long("parameter_number");
    public long ParameterDepth => Data.Long("parameter_depth");
    public bool IsThis => Data.Bool("is_this");
}

public sealed class DependentExpressionDetails : SemanticDetail
{
    internal DependentExpressionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public ExpressionOperation? WrappedExpression => Object.Link(Data.OptionalLong("wrapped_expression_id"), value => Object.Expression(value));
    public TemplateSymbol? Template => Object.Link(Data.OptionalLong("template_id"), value => Object.Template(value));
    public Initialization? Initialization => Object.Link(Data.OptionalLong("initialization_id"), value => Object.Initialization(value));
    public Scope? ParameterScope => Object.Link(Data.OptionalLong("parameter_scope_id"), value => Object.Scope(value));
    public bool? NoexceptRequired => Data.OptionalBool("noexcept_required");
    public bool IsDependent => Data.Bool("is_dependent");
    public ExpressionOperation? PackIndex => Object.Link(Data.OptionalLong("pack_index_id"), value => Object.Expression(value));
    public CppType? PackType => Object.Link(Data.OptionalLong("pack_type_id"), value => Object.Type(value));
    public TemplateSymbol? PackTemplate => Object.Link(Data.OptionalLong("pack_template_id"), value => Object.Template(value));
    public SemanticKind? FoldOperator => Object.Link(Data.OptionalLong("fold_operator_id"), value => Object.Kind(value));
    public long? FoldLeftAssociative => Data.OptionalLong("fold_left_associative");
    public IReadOnlyList<RequirementParameterDetails> RequirementParameterRows => Object.List("SELECT * FROM requirement_parameter WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new RequirementParameterDetails(Object, r));
    public IReadOnlyList<DependentRequirementDetails> DependentRequirementRows => Object.List("SELECT * FROM dependent_requirement WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new DependentRequirementDetails(Object, r));
}

public sealed class RequirementParameterDetails : SemanticDetail
{
    internal RequirementParameterDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public DependentExpressionDetails Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Detail("dependent_expression", value, r => new DependentExpressionDetails(Object, r)))!;
    public long Ordinal => Data.Long("ordinal");
    public CppType Type => Object.Link(Data.OptionalLong("type_id"), value => Object.Type(value))!;
    public VariableSymbol? Parameter => Object.Link(Data.OptionalLong("parameter_id"), value => Object.Variable(value));
}

public sealed class ExpressionTemplateArgumentDetails : SemanticDetail
{
    internal ExpressionTemplateArgumentDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public ExpressionOperation Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Expression(value))!;
    public long Ordinal => Data.Long("ordinal");
    public TemplateArgument Argument => Object.Link(Data.OptionalLong("argument_id"), value => Object.Argument(value))!;
}

public sealed class DependentRequirementDetails : SemanticDetail
{
    internal DependentRequirementDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public DependentExpressionDetails Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Detail("dependent_expression", value, r => new DependentExpressionDetails(Object, r)))!;
    public long Ordinal => Data.Long("ordinal");
    public ExpressionOperation Expression => Object.Link(Data.OptionalLong("expression_id"), value => Object.Expression(value))!;
}

public sealed class AllocationExpressionDetails : SemanticDetail
{
    internal AllocationExpressionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType? AllocatedType => Object.Link(Data.OptionalLong("allocated_type_id"), value => Object.Type(value));
    public FunctionSymbol? Allocator => Object.Link(Data.OptionalLong("allocator_id"), value => Object.Function(value));
    public FunctionSymbol? Deallocator => Object.Link(Data.OptionalLong("deallocator_id"), value => Object.Function(value));
    public FunctionSymbol? Constructor => Object.Link(Data.OptionalLong("constructor_id"), value => Object.Function(value));
    public FunctionSymbol? Destructor => Object.Link(Data.OptionalLong("destructor_id"), value => Object.Function(value));
    public Initialization? Initialization => Object.Link(Data.OptionalLong("initialization_id"), value => Object.Initialization(value));
    public ExpressionOperation? ArrayCount => Object.Link(Data.OptionalLong("array_count_id"), value => Object.Expression(value));
    public ExpressionOperation? SizeExpression => Object.Link(Data.OptionalLong("size_expression_id"), value => Object.Expression(value));
    public ExpressionOperation? Operand => Object.Link(Data.OptionalLong("operand_id"), value => Object.Expression(value));
    public bool IsDelete => Data.Bool("is_delete");
    public bool IsArray => Data.Bool("is_array");
    public bool IsGlobal => Data.Bool("is_global");
    public bool IsGcnew => Data.Bool("is_gcnew");
}

public sealed class LambdaDefinitionDetails : SemanticDetail
{
    internal LambdaDefinitionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType ClosureType => Object.Link(Data.OptionalLong("closure_type_id"), value => Object.Type(value))!;
    public FunctionSymbol? CallOperator => Object.Link(Data.OptionalLong("call_operator_id"), value => Object.Function(value));
    public SourceInfo? SourceOrigin => Object.Link(Data.OptionalLong("source_origin_id"), value => Object.Source(value));
    public SourcePosition? CaptureEndPosition => Object.Link(Data.OptionalLong("capture_end_position_id"), value => Object.Position(value));
    public SourcePosition? MutablePosition => Object.Link(Data.OptionalLong("mutable_position_id"), value => Object.Position(value));
    public SourcePosition? ConstexprPosition => Object.Link(Data.OptionalLong("constexpr_position_id"), value => Object.Position(value));
    public bool IsGeneric => Data.Bool("is_generic");
    public bool IsMutable => Data.Bool("is_mutable");
    public bool ConstexprSpecified => Data.Bool("constexpr_specified");
    public bool ConstevalSpecified => Data.Bool("consteval_specified");
    public bool HasCaptureDefault => Data.Bool("has_capture_default");
    public bool DefaultIsReference => Data.Bool("default_is_reference");
    public bool ExplicitReturnType => Data.Bool("explicit_return_type");
    public bool HasParameterDeclaration => Data.Bool("has_parameter_declaration");
    public bool HasTemplateParameterList => Data.Bool("has_template_parameter_list");
    public IReadOnlyList<LambdaCaptureDetails> LambdaCaptureRows => Object.List("SELECT * FROM lambda_capture WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new LambdaCaptureDetails(Object, r));
}

public sealed class LambdaExpressionDetails : SemanticDetail
{
    internal LambdaExpressionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public LambdaDefinitionDetails Lambda => Object.Link(Data.OptionalLong("lambda_id"), value => Object.Detail("lambda_definition", value, r => new LambdaDefinitionDetails(Object, r)))!;
    public Initialization? Initialization => Object.Link(Data.OptionalLong("initialization_id"), value => Object.Initialization(value));
}

public sealed class LambdaCaptureDetails : SemanticDetail
{
    internal LambdaCaptureDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public LambdaDefinitionDetails Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Detail("lambda_definition", value, r => new LambdaDefinitionDetails(Object, r)))!;
    public long Ordinal => Data.Long("ordinal");
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public VariableSymbol? Variable => Object.Link(Data.OptionalLong("variable_id"), value => Object.Variable(value));
    public FieldSymbol? Field => Object.Link(Data.OptionalLong("field_id"), value => Object.Field(value));
    public Initialization? Initialization => Object.Link(Data.OptionalLong("initialization_id"), value => Object.Initialization(value));
    public FieldSymbol? SourceClosureField => Object.Link(Data.OptionalLong("source_closure_field_id"), value => Object.Field(value));
    public FieldSymbol? IndirectCaptureField => Object.Link(Data.OptionalLong("indirect_capture_field_id"), value => Object.Field(value));
    public SourceInfo? SourceOrigin => Object.Link(Data.OptionalLong("source_origin_id"), value => Object.Source(value));
    public bool IsPack => Data.Bool("is_pack");
    public bool IsInitCapture => Data.Bool("is_init_capture");
    public bool IsIndirectInitCapture => Data.Bool("is_indirect_init_capture");
    public bool IsParameterReference => Data.Bool("is_parameter_reference");
    public bool CaptureByReference => Data.Bool("capture_by_reference");
    public bool IsImplicit => Data.Bool("is_implicit");
    public bool IsPackElement => Data.Bool("is_pack_element");
    public bool DirectInitialization => Data.Bool("direct_initialization");
    public bool ParenthesizedInitialization => Data.Bool("parenthesized_initialization");
    public bool ConstCapture => Data.Bool("const_capture");
}

public sealed class CoroutineDetails : SemanticDetail
{
    internal CoroutineDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public FunctionSymbol Routine => Object.Link(Data.OptionalLong("routine_id"), value => Object.Function(value))!;
    public StatementOperation? Body => Object.Link(Data.OptionalLong("body_id"), value => Object.Statement(value));
    public VariableSymbol? Promise => Object.Link(Data.OptionalLong("promise_id"), value => Object.Variable(value));
    public VariableSymbol? Handle => Object.Link(Data.OptionalLong("handle_id"), value => Object.Variable(value));
    public ExpressionOperation? Allocation => Object.Link(Data.OptionalLong("allocation_id"), value => Object.Expression(value));
    public ExpressionOperation? Deallocation => Object.Link(Data.OptionalLong("deallocation_id"), value => Object.Expression(value));
    public ExpressionOperation? InitialSuspend => Object.Link(Data.OptionalLong("initial_suspend_id"), value => Object.Expression(value));
    public ExpressionOperation? FinalSuspend => Object.Link(Data.OptionalLong("final_suspend_id"), value => Object.Expression(value));
    public ExpressionOperation? ReturnObject => Object.Link(Data.OptionalLong("return_object_id"), value => Object.Expression(value));
    public ExpressionOperation? Exception => Object.Link(Data.OptionalLong("exception_id"), value => Object.Expression(value));
    public CppType? TraitsType => Object.Link(Data.OptionalLong("traits_type_id"), value => Object.Type(value));
    public VariableSymbol? InitialAwaitResume => Object.Link(Data.OptionalLong("initial_await_resume_id"), value => Object.Variable(value));
    public VariableSymbol? ThisParameterCopy => Object.Link(Data.OptionalLong("this_parameter_copy_id"), value => Object.Variable(value));
    public LabelSymbol? FinalSuspendLabel => Object.Link(Data.OptionalLong("final_suspend_label_id"), value => Object.Label(value));
    public ExpressionOperation? AllocationFailureCall => Object.Link(Data.OptionalLong("allocation_failure_call_id"), value => Object.Expression(value));
    public FunctionSymbol? Allocator => Object.Link(Data.OptionalLong("allocator_id"), value => Object.Function(value));
    public FunctionSymbol? Deallocator => Object.Link(Data.OptionalLong("deallocator_id"), value => Object.Function(value));
    public bool HasReturnVoid => Data.Bool("has_return_void");
    public bool BodyGenerated => Data.Bool("body_generated");
    public IReadOnlyList<CoroutineParameterCopyDetails> CoroutineParameterCopyRows => Object.List("SELECT * FROM coroutine_parameter_copy WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new CoroutineParameterCopyDetails(Object, r));
}

public sealed class CoroutineParameterCopyDetails : SemanticDetail
{
    internal CoroutineParameterCopyDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CoroutineDetails Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Detail("coroutine", value, r => new CoroutineDetails(Object, r)))!;
    public long Ordinal => Data.Long("ordinal");
    public VariableSymbol Parameter => Object.Link(Data.OptionalLong("parameter_id"), value => Object.Variable(value))!;
    public VariableSymbol Copy => Object.Link(Data.OptionalLong("copy_id"), value => Object.Variable(value))!;
    public Initialization? Initialization => Object.Link(Data.OptionalLong("initialization_id"), value => Object.Initialization(value));
}

public sealed class CoroutineOperationDetails : SemanticDetail
{
    internal CoroutineOperationDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public ExpressionOperation? Operand => Object.Link(Data.OptionalLong("operand_id"), value => Object.Expression(value));
    public ExpressionOperation? Ready => Object.Link(Data.OptionalLong("ready_id"), value => Object.Expression(value));
    public ExpressionOperation? Suspend => Object.Link(Data.OptionalLong("suspend_id"), value => Object.Expression(value));
    public ExpressionOperation? Resume => Object.Link(Data.OptionalLong("resume_id"), value => Object.Expression(value));
    public ExpressionOperation? ProtocolCall => Object.Link(Data.OptionalLong("protocol_call_id"), value => Object.Expression(value));
}

public sealed class VlaDimensionDetails : SemanticDetail
{
    internal VlaDimensionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType? Type => Object.Link(Data.OptionalLong("type_id"), value => Object.Type(value));
    public ExpressionOperation? Expression => Object.Link(Data.OptionalLong("expression_id"), value => Object.Expression(value));
    public VlaDimensionDetails? OriginalDimension => Object.Link(Data.OptionalLong("original_dimension_id"), value => Object.Detail("vla_dimension", value, r => new VlaDimensionDetails(Object, r)));
    public VariableSymbol? DimensionVariable => Object.Link(Data.OptionalLong("dimension_variable_id"), value => Object.Variable(value));
    public VariableSymbol? CountVariable => Object.Link(Data.OptionalLong("count_variable_id"), value => Object.Variable(value));
    public StatementOperation? EvaluationStatement => Object.Link(Data.OptionalLong("evaluation_statement_id"), value => Object.Statement(value));
}

public sealed class VlaOperationDetails : SemanticDetail
{
    internal VlaOperationDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public VariableSymbol? Variable => Object.Link(Data.OptionalLong("variable_id"), value => Object.Variable(value));
    public CppType? Type => Object.Link(Data.OptionalLong("type_id"), value => Object.Type(value));
    public VlaDimensionDetails? Dimension => Object.Link(Data.OptionalLong("dimension_id"), value => Object.Detail("vla_dimension", value, r => new VlaDimensionDetails(Object, r)));
}

public sealed class AsmStatementDetails : SemanticDetail
{
    internal AsmStatementDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public string Text => Object.Text(Data.OptionalLong("text_id"))!;
    public bool IsVolatile => Data.Bool("is_volatile");
    public bool IsGoto => Data.Bool("is_goto");
    public bool IsMicrosoft => Data.Bool("is_microsoft");
    public IReadOnlyList<AsmOperandDetails> AsmOperandRows => Object.List("SELECT * FROM asm_operand WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new AsmOperandDetails(Object, r));
    public IReadOnlyList<AsmClobberDetails> AsmClobberRows => Object.List("SELECT * FROM asm_clobber WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new AsmClobberDetails(Object, r));
    public IReadOnlyList<AsmLabelDetails> AsmLabelRows => Object.List("SELECT * FROM asm_label WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new AsmLabelDetails(Object, r));
}

public sealed class AsmOperandDetails : SemanticDetail
{
    internal AsmOperandDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public AsmStatementDetails Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Detail("asm_statement", value, r => new AsmStatementDetails(Object, r)))!;
    public long Ordinal => Data.Long("ordinal");
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public string? Name => Object.Text(Data.OptionalLong("name_id"));
    public string Constraint => Object.Text(Data.OptionalLong("constraint_id"))!;
    public ExpressionOperation Expression => Object.Link(Data.OptionalLong("expression_id"), value => Object.Expression(value))!;
}

public sealed class AsmClobberDetails : SemanticDetail
{
    internal AsmClobberDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public AsmStatementDetails Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Detail("asm_statement", value, r => new AsmStatementDetails(Object, r)))!;
    public long Ordinal => Data.Long("ordinal");
    public string Text => Object.Text(Data.OptionalLong("text_id"))!;
}

public sealed class AsmLabelDetails : SemanticDetail
{
    internal AsmLabelDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public AsmStatementDetails Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Detail("asm_statement", value, r => new AsmStatementDetails(Object, r)))!;
    public long Ordinal => Data.Long("ordinal");
    public LabelSymbol Label => Object.Link(Data.OptionalLong("label_id"), value => Object.Label(value))!;
}

public sealed class AttributeDetails : SemanticDetail
{
    internal AttributeDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticEntity Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Entity(value))!;
    public long Ordinal => Data.Long("ordinal");
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public SemanticKind? Family => Object.Link(Data.OptionalLong("family_id"), value => Object.Kind(value));
    public SemanticKind? Location => Object.Link(Data.OptionalLong("location_id"), value => Object.Kind(value));
    public string? Name => Object.Text(Data.OptionalLong("name_id"));
    public string? NamespaceName => Object.Text(Data.OptionalLong("namespace_name_id"));
    public AttributeGroupDetails? Group => Object.Link(Data.OptionalLong("group_id"), value => Object.Detail("attribute_group", value, r => new AttributeGroupDetails(Object, r)));
    public SourceInfo? SourceOrigin => Object.Link(Data.OptionalLong("source_origin_id"), value => Object.Source(value));
    public bool OnPrimaryDeclaration => Data.Bool("on_primary_declaration");
    public bool TransformsTypeSpecifier => Data.Bool("transforms_type_specifier");
    public bool IsPackExpansion => Data.Bool("is_pack_expansion");
    public bool IsStandardGcc => Data.Bool("is_standard_gcc");
    public bool IsImplicitAbiTag => Data.Bool("is_implicit_abi_tag");
    public bool NamespaceFromUsing => Data.Bool("namespace_from_using");
    public string? StringArgument => Object.Text(Data.OptionalLong("string_argument_id"));
    public BigInteger? IntegerArgument => Number(Data.OptionalText("integer_argument"));
    public CppType? TypeArgument => Object.Link(Data.OptionalLong("type_argument_id"), value => Object.Type(value));
    public ExpressionOperation? ExpressionArgument => Object.Link(Data.OptionalLong("expression_argument_id"), value => Object.Expression(value));
    public SemanticEntity? EntityArgument => Object.Link(Data.OptionalLong("entity_argument_id"), value => Object.Entity(value));
    public IReadOnlyList<AttributeArgumentDetails> AttributeArgumentRows => Object.List("SELECT * FROM attribute_argument WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new AttributeArgumentDetails(Object, r));
}

public sealed class AttributeGroupDetails : SemanticDetail
{
    internal AttributeGroupDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SourceInfo? SourceOrigin => Object.Link(Data.OptionalLong("source_origin_id"), value => Object.Source(value));
    public string? NamespaceName => Object.Text(Data.OptionalLong("namespace_name_id"));
}

public sealed class AttributeArgumentDetails : SemanticDetail
{
    internal AttributeArgumentDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public AttributeDetails Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Detail("attribute", value, r => new AttributeDetails(Object, r)))!;
    public long Ordinal => Data.Long("ordinal");
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public string? Text => Object.Text(Data.OptionalLong("text_id"));
    public BigInteger? IntegerValue => Number(Data.OptionalText("integer_value"));
    public CppType? Type => Object.Link(Data.OptionalLong("type_id"), value => Object.Type(value));
    public ExpressionOperation? Expression => Object.Link(Data.OptionalLong("expression_id"), value => Object.Expression(value));
    public SemanticEntity? Entity => Object.Link(Data.OptionalLong("entity_id"), value => Object.Entity(value));
    public Constant? Constant => Object.Link(Data.OptionalLong("constant_id"), value => Object.Constant(value));
    public string? TokenText => Object.Text(Data.OptionalLong("token_text_id"));
    public SemanticKind? TokenKind => Object.Link(Data.OptionalLong("token_kind_id"), value => Object.Kind(value));
    public bool IsPackExpansion => Data.Bool("is_pack_expansion");
    public SourceInfo? SourceOrigin => Object.Link(Data.OptionalLong("source_origin_id"), value => Object.Source(value));
}

public sealed class ConstantDetails : SemanticDetail
{
    internal ConstantDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType Type => Object.Link(Data.OptionalLong("type_id"), value => Object.Type(value))!;
    public ConstantValue Value => Object.Link(Data.OptionalLong("value_id"), value => Object.Value(value))!;
    public SourceInfo? SourceOrigin => Object.Link(Data.OptionalLong("source_origin_id"), value => Object.Source(value));
    public ExpressionOperation? BackingExpression => Object.Link(Data.OptionalLong("backing_expression_id"), value => Object.Expression(value));
    public bool IsPackExpansion => Data.Bool("is_pack_expansion");
    public bool IsLiteral => Data.Bool("is_literal");
}

public sealed class ConstantValueDetails : SemanticDetail
{
    internal ConstantValueDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
}

public sealed class IntegerValueDetails : SemanticDetail
{
    internal IntegerValueDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public BigInteger Value => RequiredNumber(Object.Text(Data.OptionalLong("value_id")));
}

public sealed class FloatingValueDetails : SemanticDetail
{
    internal FloatingValueDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Class => Object.Kind(Data.Long("class_id"));
    public bool Negative => Data.Bool("negative");
    public long Radix => Data.Long("radix");
    public BigInteger? Significand => Number(Object.Text(Data.OptionalLong("significand_id")));
    public long? Exponent => Data.OptionalLong("exponent");
    public BigInteger? NanPayload => Number(Object.Text(Data.OptionalLong("nan_payload_id")));
    public bool? NanQuiet => Data.OptionalBool("nan_quiet");
    public SemanticKind? RepresentationKind => Object.Link(Data.OptionalLong("representation_kind_id"), value => Object.Kind(value));
}

public sealed class FixedPointValueDetails : SemanticDetail
{
    internal FixedPointValueDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public BigInteger ScaledInteger => RequiredNumber(Object.Text(Data.OptionalLong("scaled_integer_id")));
    public long Scale => Data.Long("scale");
}

public sealed class ComplexValueDetails : SemanticDetail
{
    internal ComplexValueDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public ConstantValue? RealValue => Object.Link(Data.OptionalLong("real_value_id"), value => Object.Value(value));
    public ConstantValue ImaginaryValue => Object.Link(Data.OptionalLong("imaginary_value_id"), value => Object.Value(value))!;
}

public sealed class StringValueDetails : SemanticDetail
{
    internal StringValueDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public long ElementBits => Data.Long("element_bits");
    public BigInteger Length => RequiredNumber(Data.OptionalText("length"));
    public SemanticKind Encoding => Object.Kind(Data.Long("encoding_id"));
    public SemanticKind UnitKind => Object.Kind(Data.Long("unit_kind_id"));
    public string? Text => Object.Text(Data.OptionalLong("text_id"));
    public IReadOnlyList<StringUnitDetails> StringUnitRows => Object.List("SELECT * FROM string_unit WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new StringUnitDetails(Object, r));
}

public sealed class StringUnitDetails : SemanticDetail
{
    internal StringUnitDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public StringValueDetails Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Detail("string_value", value, r => new StringValueDetails(Object, r)))!;
    public long Ordinal => Data.Long("ordinal");
    public long Unit => Data.Long("unit");
}

public sealed class AddressValueDetails : SemanticDetail
{
    internal AddressValueDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind BaseKind => Object.Kind(Data.Long("base_kind_id"));
    public SemanticEntity? BaseEntity => Object.Link(Data.OptionalLong("base_entity_id"), value => Object.Entity(value));
    public BigInteger Offset => RequiredNumber(Data.OptionalText("offset"));
    public bool OnePastEnd => Data.Bool("one_past_end");
    public SemanticEntity? SubobjectPath => Object.Link(Data.OptionalLong("subobject_path_id"), value => Object.Entity(value));
}

public sealed class StackOffsetValueDetails : SemanticDetail
{
    internal StackOffsetValueDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public VariableSymbol Variable => Object.Link(Data.OptionalLong("variable_id"), value => Object.Variable(value))!;
    public BigInteger Offset => RequiredNumber(Data.OptionalText("offset"));
}

public sealed class MemberPointerValueDetails : SemanticDetail
{
    internal MemberPointerValueDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticEntity? Member => Object.Link(Data.OptionalLong("member_id"), value => Object.Entity(value));
    public CppType? ContainingType => Object.Link(Data.OptionalLong("containing_type_id"), value => Object.Type(value));
    public BigInteger? Adjustment => Number(Data.OptionalText("adjustment"));
    public BaseSubobject? Subobject => Object.Link(Data.OptionalLong("subobject_id"), value => Object.Subobject(value));
    public bool IsNull => Data.Bool("is_null");
}

public sealed class LabelDifferenceValueDetails : SemanticDetail
{
    internal LabelDifferenceValueDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public LabelSymbol LeftLabel => Object.Link(Data.OptionalLong("left_label_id"), value => Object.Label(value))!;
    public LabelSymbol RightLabel => Object.Link(Data.OptionalLong("right_label_id"), value => Object.Label(value))!;
    public BigInteger Offset => RequiredNumber(Data.OptionalText("offset"));
}

public sealed class AggregateValueDetails : SemanticDetail
{
    internal AggregateValueDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType Type => Object.Link(Data.OptionalLong("type_id"), value => Object.Type(value))!;
    public IReadOnlyList<AggregateElementDetails> AggregateElementRows => Object.List("SELECT * FROM aggregate_element WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new AggregateElementDetails(Object, r));
}

public sealed class AggregateElementDetails : SemanticDetail
{
    internal AggregateElementDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public AggregateValueDetails Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Detail("aggregate_value", value, r => new AggregateValueDetails(Object, r)))!;
    public long Ordinal => Data.Long("ordinal");
    public Constant Constant => Object.Link(Data.OptionalLong("constant_id"), value => Object.Constant(value))!;
    public FieldSymbol? Field => Object.Link(Data.OptionalLong("field_id"), value => Object.Field(value));
    public Initialization? Initialization => Object.Link(Data.OptionalLong("initialization_id"), value => Object.Initialization(value));
}

public sealed class RepeatValueDetails : SemanticDetail
{
    internal RepeatValueDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public Constant Constant => Object.Link(Data.OptionalLong("constant_id"), value => Object.Constant(value))!;
    public BigInteger Count => RequiredNumber(Data.OptionalText("count"));
}

public sealed class DesignatorValueDetails : SemanticDetail
{
    internal DesignatorValueDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public FieldSymbol? Field => Object.Link(Data.OptionalLong("field_id"), value => Object.Field(value));
    public BigInteger? IndexValue => Number(Data.OptionalText("index_value"));
    public BigInteger? RangeEnd => Number(Data.OptionalText("range_end"));
    public Constant? Constant => Object.Link(Data.OptionalLong("constant_id"), value => Object.Constant(value));
}

public sealed class DynamicInitializationValueDetails : SemanticDetail
{
    internal DynamicInitializationValueDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public Initialization Initialization => Object.Link(Data.OptionalLong("initialization_id"), value => Object.Initialization(value))!;
}

public sealed class DependentValueDetails : SemanticDetail
{
    internal DependentValueDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public long? ParameterNumber => Data.OptionalLong("parameter_number");
    public long? ParameterDepth => Data.OptionalLong("parameter_depth");
    public ExpressionOperation? Expression => Object.Link(Data.OptionalLong("expression_id"), value => Object.Expression(value));
    public CppType? Type => Object.Link(Data.OptionalLong("type_id"), value => Object.Type(value));
    public SemanticEntity? Entity => Object.Link(Data.OptionalLong("entity_id"), value => Object.Entity(value));
    public Constant? Constant => Object.Link(Data.OptionalLong("constant_id"), value => Object.Constant(value));
}

public sealed class ReflectionValueDetails : SemanticDetail
{
    internal ReflectionValueDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public SemanticEntity? Entity => Object.Link(Data.OptionalLong("entity_id"), value => Object.Entity(value));
    public FunctionSymbol? OwnerRoutine => Object.Link(Data.OptionalLong("owner_routine_id"), value => Object.Function(value));
}

public sealed class SubobjectPathDetails : SemanticDetail
{
    internal SubobjectPathDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public FieldSymbol? Field => Object.Link(Data.OptionalLong("field_id"), value => Object.Field(value));
    public BaseSubobject? BaseSubobject => Object.Link(Data.OptionalLong("base_subobject_id"), value => Object.Subobject(value));
    public BigInteger? ElementOffset => Number(Data.OptionalText("element_offset"));
    public SubobjectPathDetails? Next => Object.Link(Data.OptionalLong("next_id"), value => Object.Detail("subobject_path", value, r => new SubobjectPathDetails(Object, r)));
}

public sealed class InitializationDetails : SemanticDetail
{
    internal InitializationDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public SemanticEntity? Target => Object.Link(Data.OptionalLong("target_id"), value => Object.Entity(value));
    public CppType? Type => Object.Link(Data.OptionalLong("type_id"), value => Object.Type(value));
    public LambdaDefinitionDetails? Lambda => Object.Link(Data.OptionalLong("lambda_id"), value => Object.Detail("lambda_definition", value, r => new LambdaDefinitionDetails(Object, r)));
    public ExpressionOperation? Expression => Object.Link(Data.OptionalLong("expression_id"), value => Object.Expression(value));
    public Constant? Constant => Object.Link(Data.OptionalLong("constant_id"), value => Object.Constant(value));
    public FunctionSymbol? Constructor => Object.Link(Data.OptionalLong("constructor_id"), value => Object.Function(value));
    public FunctionSymbol? Destructor => Object.Link(Data.OptionalLong("destructor_id"), value => Object.Function(value));
    public Lifetime? Lifetime => Object.Link(Data.OptionalLong("lifetime_id"), value => Object.Lifetime(value));
    public Lifetime? InitExpressionLifetime => Object.Link(Data.OptionalLong("init_expression_lifetime_id"), value => Object.Lifetime(value));
    public Initialization? Master => Object.Link(Data.OptionalLong("master_id"), value => Object.Initialization(value));
    public Initialization? NextDestruction => Object.Link(Data.OptionalLong("next_destruction_id"), value => Object.Initialization(value));
    public Lifetime? OverlappingLifetime => Object.Link(Data.OptionalLong("overlapping_lifetime_id"), value => Object.Lifetime(value));
    public ExpressionOperation? Source => Object.Link(Data.OptionalLong("source_id"), value => Object.Expression(value));
    public VariableSymbol? ConditionVariable => Object.Link(Data.OptionalLong("condition_variable_id"), value => Object.Variable(value));
    public bool IsUnordered => Data.Bool("is_unordered");
    public bool IsConditional => Data.Bool("is_conditional");
    public bool IsOverlapping => Data.Bool("is_overlapping");
    public bool IsArray => Data.Bool("is_array");
    public bool IsElided => Data.Bool("is_elided");
    public bool IsClassResultMaster => Data.Bool("is_class_result_master");
    public bool IsStaticTemporary => Data.Bool("is_static_temporary");
    public bool IsPartialAggregate => Data.Bool("is_partial_aggregate");
    public bool IsPartialInitialization => Data.Bool("is_partial_initialization");
    public bool FollowsExecutableStatement => Data.Bool("follows_executable_statement");
    public bool HasTemporaryLifetime => Data.Bool("has_temporary_lifetime");
    public bool IsConstructorInitialization => Data.Bool("is_constructor_initialization");
    public bool IsConditionalGuard => Data.Bool("is_conditional_guard");
    public bool IsVlaDeallocation => Data.Bool("is_vla_deallocation");
    public bool CreatesInitializerList => Data.Bool("creates_initializer_list");
    public bool IsInitializerListArray => Data.Bool("is_initializer_list_array");
    public bool IsReusedValue => Data.Bool("is_reused_value");
    public bool IsQuestionResult => Data.Bool("is_question_result");
    public bool IsCommaResult => Data.Bool("is_comma_result");
    public bool ConstructorCopyImpliedSource => Data.Bool("constructor_copy_implied_source");
    public bool ConstructorImplicitCopy => Data.Bool("constructor_implicit_copy");
    public bool ConstructorValueInitialization => Data.Bool("constructor_value_initialization");
    public bool ConstructorSequencedArguments => Data.Bool("constructor_sequenced_arguments");
    public bool ConstructorArrayCopy => Data.Bool("constructor_array_copy");
    public bool ConstantNonconstant => Data.Bool("constant_nonconstant");
    public bool FreesStorage => Data.Bool("frees_storage");
    public bool FreesArray => Data.Bool("frees_array");
    public bool IsExplicitCast => Data.Bool("is_explicit_cast");
    public bool IsBraced => Data.Bool("is_braced");
    public bool IsCompoundLiteral => Data.Bool("is_compound_literal");
    public VariableSymbol? GuardVariable => Object.Link(Data.OptionalLong("guard_variable_id"), value => Object.Variable(value));
    public VlaDimensionDetails? VlaDimension => Object.Link(Data.OptionalLong("vla_dimension_id"), value => Object.Detail("vla_dimension", value, r => new VlaDimensionDetails(Object, r)));
    public IReadOnlyList<InitializationOperandDetails> InitializationOperandRows => Object.List("SELECT * FROM initialization_operand WHERE owner_id=$owner ORDER BY role_id,ordinal", Data.Long("id"), r => new InitializationOperandDetails(Object, r));
    public IReadOnlyList<ConstructionDependencyDetails> ConstructionDependencyRows => Object.List("SELECT * FROM construction_dependency WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new ConstructionDependencyDetails(Object, r));
}

public sealed class InitializationOperandDetails : SemanticDetail
{
    internal InitializationOperandDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public Initialization Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Initialization(value))!;
    public SemanticKind Role => Object.Kind(Data.Long("role_id"));
    public long Ordinal => Data.Long("ordinal");
    public Initialization? Initialization => Object.Link(Data.OptionalLong("initialization_id"), value => Object.Initialization(value));
    public ExpressionOperation? Expression => Object.Link(Data.OptionalLong("expression_id"), value => Object.Expression(value));
    public FieldSymbol? Field => Object.Link(Data.OptionalLong("field_id"), value => Object.Field(value));
    public BaseSubobject? BaseSubobject => Object.Link(Data.OptionalLong("base_subobject_id"), value => Object.Subobject(value));
}

public sealed class TemporaryDetails : SemanticDetail
{
    internal TemporaryDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType Type => Object.Link(Data.OptionalLong("type_id"), value => Object.Type(value))!;
    public VariableSymbol? Variable => Object.Link(Data.OptionalLong("variable_id"), value => Object.Variable(value));
    public Initialization? Initialization => Object.Link(Data.OptionalLong("initialization_id"), value => Object.Initialization(value));
    public Lifetime? Lifetime => Object.Link(Data.OptionalLong("lifetime_id"), value => Object.Lifetime(value));
    public Lifetime? ExtendedLifetime => Object.Link(Data.OptionalLong("extended_lifetime_id"), value => Object.Lifetime(value));
}

public sealed class LifetimeDetails : SemanticDetail
{
    internal LifetimeDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public Lifetime? Parent => Object.Link(Data.OptionalLong("parent_id"), value => Object.Lifetime(value));
    public SemanticEntity? Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Entity(value));
    public Scope? Scope => Object.Link(Data.OptionalLong("scope_id"), value => Object.Scope(value));
    public Initialization? ParentCheckpoint => Object.Link(Data.OptionalLong("parent_checkpoint_id"), value => Object.Initialization(value));
    public SemanticNode? BeginNode => Object.Link(Data.OptionalLong("begin_node_id"), value => Object.Node(value));
    public SemanticNode? EndNode => Object.Link(Data.OptionalLong("end_node_id"), value => Object.Node(value));
    public IReadOnlyList<LifetimeCleanupDetails> LifetimeCleanupRows => Object.List("SELECT * FROM lifetime_cleanup WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new LifetimeCleanupDetails(Object, r));
}

public sealed class LifetimeCleanupDetails : SemanticDetail
{
    internal LifetimeCleanupDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public Lifetime Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Lifetime(value))!;
    public long Ordinal => Data.Long("ordinal");
    public Initialization Initialization => Object.Link(Data.OptionalLong("initialization_id"), value => Object.Initialization(value))!;
    public FunctionSymbol? Destructor => Object.Link(Data.OptionalLong("destructor_id"), value => Object.Function(value));
    public bool IsUnordered => Data.Bool("is_unordered");
    public bool IsConditional => Data.Bool("is_conditional");
}

public sealed class LifetimeTransitionDetails : SemanticDetail
{
    internal LifetimeTransitionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticNode Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Node(value))!;
    public long Ordinal => Data.Long("ordinal");
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public Lifetime? Source => Object.Link(Data.OptionalLong("source_id"), value => Object.Lifetime(value));
    public Lifetime? Target => Object.Link(Data.OptionalLong("target_id"), value => Object.Lifetime(value));
}

public sealed class ConstructionDependencyDetails : SemanticDetail
{
    internal ConstructionDependencyDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public Initialization Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Initialization(value))!;
    public long Ordinal => Data.Long("ordinal");
    public Initialization Dependency => Object.Link(Data.OptionalLong("dependency_id"), value => Object.Initialization(value))!;
    public VariableSymbol? Condition => Object.Link(Data.OptionalLong("condition_id"), value => Object.Variable(value));
}

public sealed class StaticInitializationDetails : SemanticDetail
{
    internal StaticInitializationDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public TranslationUnit Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Unit(value))!;
    public long Ordinal => Data.Long("ordinal");
    public VariableSymbol Variable => Object.Link(Data.OptionalLong("variable_id"), value => Object.Variable(value))!;
    public Initialization? Initialization => Object.Link(Data.OptionalLong("initialization_id"), value => Object.Initialization(value));
    public Constant? Constant => Object.Link(Data.OptionalLong("constant_id"), value => Object.Constant(value));
    public FunctionSymbol? InitRoutine => Object.Link(Data.OptionalLong("init_routine_id"), value => Object.Function(value));
    public FunctionSymbol? TerminationRoutine => Object.Link(Data.OptionalLong("termination_routine_id"), value => Object.Function(value));
    public bool IsThreadLocal => Data.Bool("is_thread_local");
}

public sealed class ExceptionSpecificationDetails : SemanticDetail
{
    internal ExceptionSpecificationDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public ExpressionOperation? NoexceptExpression => Object.Link(Data.OptionalLong("noexcept_expression_id"), value => Object.Expression(value));
    public bool ThrowAny => Data.Bool("throw_any");
    public bool? NoexceptEffective => Data.OptionalBool("noexcept_effective");
    public bool IsDependent => Data.Bool("is_dependent");
    public bool IsIndeterminate => Data.Bool("is_indeterminate");
    public IReadOnlyList<ExceptionSpecificationTypeDetails> ExceptionSpecificationTypeRows => Object.List("SELECT * FROM exception_specification_type WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new ExceptionSpecificationTypeDetails(Object, r));
}

public sealed class ExceptionSpecificationTypeDetails : SemanticDetail
{
    internal ExceptionSpecificationTypeDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public ExceptionSpecificationDetails Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Detail("exception_specification", value, r => new ExceptionSpecificationDetails(Object, r)))!;
    public long Ordinal => Data.Long("ordinal");
    public CppType Type => Object.Link(Data.OptionalLong("type_id"), value => Object.Type(value))!;
}

public sealed class FunctionEhDetails : SemanticDetail
{
    internal FunctionEhDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public FunctionSymbol Routine => Object.Link(Data.OptionalLong("routine_id"), value => Object.Function(value))!;
    public ExpressionOperation? Prologue => Object.Link(Data.OptionalLong("prologue_id"), value => Object.Expression(value));
    public ExpressionOperation? Epilogue => Object.Link(Data.OptionalLong("epilogue_id"), value => Object.Expression(value));
    public VariableSymbol? RegionTable => Object.Link(Data.OptionalLong("region_table_id"), value => Object.Variable(value));
    public VariableSymbol? ArrayTable => Object.Link(Data.OptionalLong("array_table_id"), value => Object.Variable(value));
    public BigInteger InitialState => RequiredNumber(Data.OptionalText("initial_state"));
    public BigInteger NullState => RequiredNumber(Data.OptionalText("null_state"));
    public IReadOnlyList<CleanupRegionDetails> CleanupRegionRows => Object.List("SELECT * FROM cleanup_region WHERE function_eh_id=$owner ORDER BY ordinal,id", Data.Long("id"), r => new CleanupRegionDetails(Object, r), "cleanup_region");
    public IReadOnlyList<CleanupArrayDetails> CleanupArrayRows => Object.List("SELECT * FROM cleanup_array WHERE function_eh_id=$owner ORDER BY ordinal,id", Data.Long("id"), r => new CleanupArrayDetails(Object, r), "cleanup_array");
    public IReadOnlyList<LoweredEhOperationDetails> LoweredEhOperationRows => Object.List("SELECT * FROM lowered_eh_operation WHERE function_eh_id=$owner ORDER BY id", Data.Long("id"), r => new LoweredEhOperationDetails(Object, r), "lowered_eh_operation");
}

public sealed class EhTableProjectionDetails : SemanticDetail
{
    internal EhTableProjectionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public VariableSymbol Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Variable(value))!;
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public FunctionEhDetails FunctionEh => Object.Link(Data.OptionalLong("function_eh_id"), value => Object.Detail("function_eh", value, r => new FunctionEhDetails(Object, r)))!;
}

public sealed class CleanupRegionDetails : SemanticDetail
{
    internal CleanupRegionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public FunctionEhDetails FunctionEh => Object.Link(Data.OptionalLong("function_eh_id"), value => Object.Detail("function_eh", value, r => new FunctionEhDetails(Object, r)))!;
    public long Ordinal => Data.Long("ordinal");
    public CleanupRegion? NextRegion => Object.Link(Data.OptionalLong("next_region_id"), value => Object.Region(value));
    public CleanupCondition? Condition => Object.Link(Data.OptionalLong("condition_id"), value => Object.Condition(value));
    public bool IsSupplement => Data.Bool("is_supplement");
    public CleanupRegion? SupplementOwner => Object.Link(Data.OptionalLong("supplement_owner_id"), value => Object.Region(value));
    public IReadOnlyList<CleanupActionDetails> CleanupActionRows => Object.List("SELECT * FROM cleanup_action WHERE region_id=$owner ORDER BY ordinal,id", Data.Long("id"), r => new CleanupActionDetails(Object, r), "cleanup_action");
    public IReadOnlyList<LoweredEhOperationDetails> LoweredEhOperationRows => Object.List("SELECT * FROM lowered_eh_operation WHERE region_id=$owner ORDER BY id", Data.Long("id"), r => new LoweredEhOperationDetails(Object, r), "lowered_eh_operation");
    public IReadOnlyList<CleanupStateOperationDetails> CleanupStateOperationRows => Object.List("SELECT * FROM cleanup_state_operation WHERE region_id=$owner ORDER BY id", Data.Long("id"), r => new CleanupStateOperationDetails(Object, r), "cleanup_state_operation");
    public IReadOnlyList<EhHandlerRegionDetails> EhHandlerRegionRows => Object.List("SELECT * FROM eh_handler_region WHERE region_id=$owner ORDER BY ordinal", Data.Long("id"), r => new EhHandlerRegionDetails(Object, r));
}

public sealed class CleanupActionDetails : SemanticDetail
{
    internal CleanupActionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CleanupRegion Region => Object.Link(Data.OptionalLong("region_id"), value => Object.Region(value))!;
    public long Ordinal => Data.Long("ordinal");
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public FunctionSymbol? Routine => Object.Link(Data.OptionalLong("routine_id"), value => Object.Function(value));
    public CleanupStorageLocation? Storage => Object.Link(Data.OptionalLong("storage_id"), value => Object.Storage(value));
    public CleanupArray? Array => Object.Link(Data.OptionalLong("array_id"), value => Object.Array(value));
    public CleanupAbiDetail? AbiDetail => Object.Link(Data.OptionalLong("abi_detail_id"), value => Object.AbiDetail(value));
    public bool IsArray => Data.Bool("is_array");
    public bool IsIndirect => Data.Bool("is_indirect");
    public bool IsConditional => Data.Bool("is_conditional");
    public bool IsUnordered => Data.Bool("is_unordered");
    public bool IsBaseSubobject => Data.Bool("is_base_subobject");
    public bool IsVla => Data.Bool("is_vla");
    public bool IsDeallocation => Data.Bool("is_deallocation");
    public bool IsLocalStaticGuard => Data.Bool("is_local_static_guard");
}

public sealed class CleanupStorageLocationDetails : SemanticDetail
{
    internal CleanupStorageLocationDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public VariableSymbol? Variable => Object.Link(Data.OptionalLong("variable_id"), value => Object.Variable(value));
    public SemanticEntity? BaseEntity => Object.Link(Data.OptionalLong("base_entity_id"), value => Object.Entity(value));
    public BigInteger Offset => RequiredNumber(Data.OptionalText("offset"));
}

public sealed class CleanupArrayDetails : SemanticDetail
{
    internal CleanupArrayDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public FunctionEhDetails FunctionEh => Object.Link(Data.OptionalLong("function_eh_id"), value => Object.Detail("function_eh", value, r => new FunctionEhDetails(Object, r)))!;
    public long Ordinal => Data.Long("ordinal");
    public CleanupStorageLocation Storage => Object.Link(Data.OptionalLong("storage_id"), value => Object.Storage(value))!;
    public BigInteger ElementSize => RequiredNumber(Data.OptionalText("element_size"));
    public BigInteger ElementCount => RequiredNumber(Data.OptionalText("element_count"));
    public CleanupStorageLocation? CountLocation => Object.Link(Data.OptionalLong("count_location_id"), value => Object.Storage(value));
    public bool IsNonarraySize => Data.Bool("is_nonarray_size");
}

public sealed class CleanupConditionDetails : SemanticDetail
{
    internal CleanupConditionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CleanupStorageLocation Storage => Object.Link(Data.OptionalLong("storage_id"), value => Object.Storage(value))!;
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public BigInteger? ComparisonValue => Number(Data.OptionalText("comparison_value"));
}

public sealed class CleanupAbiDetailDetails : SemanticDetail
{
    internal CleanupAbiDetailDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticEntity? VttEntity => Object.Link(Data.OptionalLong("vtt_entity_id"), value => Object.Entity(value));
    public BigInteger? VttOffset => Number(Data.OptionalText("vtt_offset"));
    public BaseSubobject? Subobject => Object.Link(Data.OptionalLong("subobject_id"), value => Object.Subobject(value));
    public bool IsDelegating => Data.Bool("is_delegating");
    public BigInteger? DelegationArgument => Number(Data.OptionalText("delegation_argument"));
    public CleanupStorageLocation? VlaCountLocation => Object.Link(Data.OptionalLong("vla_count_location_id"), value => Object.Storage(value));
}

public sealed class LoweredEhOperationDetails : SemanticDetail
{
    internal LoweredEhOperationDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public FunctionEhDetails? FunctionEh => Object.Link(Data.OptionalLong("function_eh_id"), value => Object.Detail("function_eh", value, r => new FunctionEhDetails(Object, r)));
    public CleanupRegion? Region => Object.Link(Data.OptionalLong("region_id"), value => Object.Region(value));
    public BigInteger? StateNumber => Number(Data.OptionalText("state_number"));
    public FunctionSymbol? Routine => Object.Link(Data.OptionalLong("routine_id"), value => Object.Function(value));
    public CppType? ObjectType => Object.Link(Data.OptionalLong("object_type_id"), value => Object.Type(value));
    public HandlerDetails? Handler => Object.Link(Data.OptionalLong("handler_id"), value => Object.Detail("handler", value, r => new HandlerDetails(Object, r)));
    public TryStatementDetails? TryBlock => Object.Link(Data.OptionalLong("try_block_id"), value => Object.Detail("try_statement", value, r => new TryStatementDetails(Object, r)));
    public ExpressionOperation? TryExpression => Object.Link(Data.OptionalLong("try_expression_id"), value => Object.Expression(value));
    public ExpressionOperation? CatchExpression => Object.Link(Data.OptionalLong("catch_expression_id"), value => Object.Expression(value));
    public Initialization? Initialization => Object.Link(Data.OptionalLong("initialization_id"), value => Object.Initialization(value));
}

public sealed class CleanupStateOperationDetails : SemanticDetail
{
    internal CleanupStateOperationDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CleanupRegion? Region => Object.Link(Data.OptionalLong("region_id"), value => Object.Region(value));
    public BigInteger StateNumber => RequiredNumber(Data.OptionalText("state_number"));
    public bool IsUnreachable => Data.Bool("is_unreachable");
}

public sealed class EhHandlerRegionDetails : SemanticDetail
{
    internal EhHandlerRegionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public HandlerDetails Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Detail("handler", value, r => new HandlerDetails(Object, r)))!;
    public long Ordinal => Data.Long("ordinal");
    public CleanupRegion Region => Object.Link(Data.OptionalLong("region_id"), value => Object.Region(value))!;
    public CleanupRegion? EndRegion => Object.Link(Data.OptionalLong("end_region_id"), value => Object.Region(value));
}

public sealed class TemplateDefinitionDetails : SemanticDetail
{
    internal TemplateDefinitionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public NameDetails? Name => Object.Link(Data.OptionalLong("name_id"), value => Object.Detail("name", value, r => new NameDetails(Object, r)));
    public SemanticEntity? Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Entity(value));
    public SemanticEntity? Pattern => Object.Link(Data.OptionalLong("pattern_id"), value => Object.Entity(value));
    public SourceInfo? SourceOrigin => Object.Link(Data.OptionalLong("source_origin_id"), value => Object.Source(value));
    public ExpressionOperation? Constraint => Object.Link(Data.OptionalLong("constraint_id"), value => Object.Expression(value));
    public TemplateSymbol? CanonicalTemplate => Object.Link(Data.OptionalLong("canonical_template_id"), value => Object.Template(value));
    public TemplateSymbol? DefinitionTemplate => Object.Link(Data.OptionalLong("definition_template_id"), value => Object.Template(value));
    public TemplateSymbol? PrototypeTemplate => Object.Link(Data.OptionalLong("prototype_template_id"), value => Object.Template(value));
    public bool IsExported => Data.Bool("is_exported");
    public bool IgnoreExport => Data.Bool("ignore_export");
    public bool IsPack => Data.Bool("is_pack");
    public bool IsFriendTemplate => Data.Bool("is_friend_template");
    public SourcePosition? ExportPosition => Object.Link(Data.OptionalLong("export_position_id"), value => Object.Position(value));
    public SourceRange? DefinitionRange => Object.Link(Data.OptionalLong("definition_range_id"), value => Object.Range(value));
    public IReadOnlyList<RoutineSymbolDetails> RoutineSymbolRows => Object.List("SELECT * FROM routine_symbol WHERE template_id=$owner ORDER BY id", Data.Long("id"), r => new RoutineSymbolDetails(Object, r), "routine_symbol");
    public IReadOnlyList<DependentExpressionDetails> DependentExpressionRows => Object.List("SELECT * FROM dependent_expression WHERE template_id=$owner ORDER BY id", Data.Long("id"), r => new DependentExpressionDetails(Object, r), "dependent_expression");
    public IReadOnlyList<TemplateHeaderDetails> TemplateHeaderRows => Object.List("SELECT * FROM template_header WHERE template_id=$owner ORDER BY id", Data.Long("id"), r => new TemplateHeaderDetails(Object, r), "template_header");
    public IReadOnlyList<TemplateParameterDetails> TemplateParameterRows => Object.List("SELECT * FROM template_parameter WHERE template_id=$owner ORDER BY depth,ordinal,id", Data.Long("id"), r => new TemplateParameterDetails(Object, r), "template_parameter");
    public IReadOnlyList<TemplateArgumentDetails> TemplateArgumentRows => Object.List("SELECT * FROM template_argument WHERE template_id=$owner ORDER BY id", Data.Long("id"), r => new TemplateArgumentDetails(Object, r), "template_argument");
    public IReadOnlyList<TemplateInstanceDetails> TemplateInstanceRows => Object.List("SELECT * FROM template_instance WHERE template_id=$owner ORDER BY id", Data.Long("id"), r => new TemplateInstanceDetails(Object, r), "template_instance");
}

public sealed class TemplateHeaderDetails : SemanticDetail
{
    internal TemplateHeaderDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public TemplateSymbol? Template => Object.Link(Data.OptionalLong("template_id"), value => Object.Template(value));
    public TemplateHeaderDetails? Parent => Object.Link(Data.OptionalLong("parent_id"), value => Object.Detail("template_header", value, r => new TemplateHeaderDetails(Object, r)));
    public Scope? Scope => Object.Link(Data.OptionalLong("scope_id"), value => Object.Scope(value));
    public SourceInfo? SourceOrigin => Object.Link(Data.OptionalLong("source_origin_id"), value => Object.Source(value));
    public bool IsGeneric => Data.Bool("is_generic");
    public ExpressionOperation? Constraint => Object.Link(Data.OptionalLong("constraint_id"), value => Object.Expression(value));
    public SourcePosition? RequiresPosition => Object.Link(Data.OptionalLong("requires_position_id"), value => Object.Position(value));
}

public sealed class TemplateParameterDetails : SemanticDetail
{
    internal TemplateParameterDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public TemplateSymbol? Template => Object.Link(Data.OptionalLong("template_id"), value => Object.Template(value));
    public TemplateHeaderDetails? Header => Object.Link(Data.OptionalLong("header_id"), value => Object.Detail("template_header", value, r => new TemplateHeaderDetails(Object, r)));
    public long Ordinal => Data.Long("ordinal");
    public long Depth => Data.Long("depth");
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public NameDetails? Name => Object.Link(Data.OptionalLong("name_id"), value => Object.Detail("name", value, r => new NameDetails(Object, r)));
    public bool IsPack => Data.Bool("is_pack");
    public bool IsAbbreviated => Data.Bool("is_abbreviated");
    public CppType? PlaceholderType => Object.Link(Data.OptionalLong("placeholder_type_id"), value => Object.Type(value));
    public Constant? PlaceholderConstant => Object.Link(Data.OptionalLong("placeholder_constant_id"), value => Object.Constant(value));
    public TemplateSymbol? PlaceholderTemplate => Object.Link(Data.OptionalLong("placeholder_template_id"), value => Object.Template(value));
    public TemplateArgument? DefaultArgument => Object.Link(Data.OptionalLong("default_argument_id"), value => Object.Argument(value));
    public ExpressionOperation? Constraint => Object.Link(Data.OptionalLong("constraint_id"), value => Object.Expression(value));
}

public sealed class TemplateGenericConstraintClauseDetails : SemanticDetail
{
    internal TemplateGenericConstraintClauseDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public TemplateHeaderDetails Header => Object.Link(Data.OptionalLong("header_id"), value => Object.Detail("template_header", value, r => new TemplateHeaderDetails(Object, r)))!;
    public long Ordinal => Data.Long("ordinal");
    public CppType ParameterType => Object.Link(Data.OptionalLong("parameter_type_id"), value => Object.Type(value))!;
    public SourcePosition? TypePosition => Object.Link(Data.OptionalLong("type_position_id"), value => Object.Position(value));
    public SourcePosition? WherePosition => Object.Link(Data.OptionalLong("where_position_id"), value => Object.Position(value));
    public SourcePosition? ColonPosition => Object.Link(Data.OptionalLong("colon_position_id"), value => Object.Position(value));
    public IReadOnlyList<TemplateGenericConstraintDetails> TemplateGenericConstraintRows => Object.List("SELECT * FROM template_generic_constraint WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new TemplateGenericConstraintDetails(Object, r));
}

public sealed class TemplateGenericConstraintDetails : SemanticDetail
{
    internal TemplateGenericConstraintDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public TemplateGenericConstraintClauseDetails Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Detail("template_generic_constraint_clause", value, r => new TemplateGenericConstraintClauseDetails(Object, r)))!;
    public long Ordinal => Data.Long("ordinal");
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public CppType? Type => Object.Link(Data.OptionalLong("type_id"), value => Object.Type(value));
    public bool IsImplicit => Data.Bool("is_implicit");
    public SourcePosition? SourcePosition => Object.Link(Data.OptionalLong("source_position_id"), value => Object.Position(value));
}

public sealed class TemplateArgumentDetails : SemanticDetail
{
    internal TemplateArgumentDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public CppType? Type => Object.Link(Data.OptionalLong("type_id"), value => Object.Type(value));
    public Constant? Constant => Object.Link(Data.OptionalLong("constant_id"), value => Object.Constant(value));
    public ExpressionOperation? Expression => Object.Link(Data.OptionalLong("expression_id"), value => Object.Expression(value));
    public TemplateSymbol? Template => Object.Link(Data.OptionalLong("template_id"), value => Object.Template(value));
    public bool IsPack => Data.Bool("is_pack");
    public bool IsExplicit => Data.Bool("is_explicit");
    public bool IsPackElement => Data.Bool("is_pack_element");
    public bool IsEllipsis => Data.Bool("is_ellipsis");
    public bool IsIntegerPack => Data.Bool("is_integer_pack");
    public bool IsInjectedName => Data.Bool("is_injected_name");
    public bool IsProvisional => Data.Bool("is_provisional");
    public bool IsError => Data.Bool("is_error");
    public bool IsUnknownArrayBound => Data.Bool("is_unknown_array_bound");
    public string? DeducedArrayBound => Data.OptionalText("deduced_array_bound");
    public IReadOnlyList<TemplateArgumentPackElementDetails> TemplateArgumentPackElementRows => Object.List("SELECT * FROM template_argument_pack_element WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new TemplateArgumentPackElementDetails(Object, r));
}

public sealed class TemplateArgumentPackElementDetails : SemanticDetail
{
    internal TemplateArgumentPackElementDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public TemplateArgument Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Argument(value))!;
    public long Ordinal => Data.Long("ordinal");
    public TemplateArgument Argument => Object.Link(Data.OptionalLong("argument_id"), value => Object.Argument(value))!;
}

public sealed class TemplateInstanceDetails : SemanticDetail
{
    internal TemplateInstanceDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticEntity Result => Object.Link(Data.OptionalLong("result_id"), value => Object.Entity(value))!;
    public TemplateSymbol? Template => Object.Link(Data.OptionalLong("template_id"), value => Object.Template(value));
    public TemplateSpecializationDetails? SelectedSpecialization => Object.Link(Data.OptionalLong("selected_specialization_id"), value => Object.Detail("template_specialization", value, r => new TemplateSpecializationDetails(Object, r)));
    public SemanticKind State => Object.Kind(Data.Long("state_id"));
    public SourcePosition? SourcePosition => Object.Link(Data.OptionalLong("source_position_id"), value => Object.Position(value));
    public bool IsDependent => Data.Bool("is_dependent");
    public bool IsPrototype => Data.Bool("is_prototype");
    public bool IsSuppressed => Data.Bool("is_suppressed");
    public bool IsExplicit => Data.Bool("is_explicit");
    public bool IsSpecialized => Data.Bool("is_specialized");
    public IReadOnlyList<TemplateInstanceArgumentDetails> TemplateInstanceArgumentRows => Object.List("SELECT * FROM template_instance_argument WHERE owner_id=$owner ORDER BY role_id,ordinal", Data.Long("id"), r => new TemplateInstanceArgumentDetails(Object, r));
}

public sealed class TemplateInstanceArgumentDetails : SemanticDetail
{
    internal TemplateInstanceArgumentDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public TemplateInstantiation Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Instance(value))!;
    public SemanticKind Role => Object.Kind(Data.Long("role_id"));
    public long Ordinal => Data.Long("ordinal");
    public TemplateArgument Argument => Object.Link(Data.OptionalLong("argument_id"), value => Object.Argument(value))!;
}

public sealed class TemplateSpecializationDetails : SemanticDetail
{
    internal TemplateSpecializationDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public TemplateSymbol PrimaryTemplate => Object.Link(Data.OptionalLong("primary_template_id"), value => Object.Template(value))!;
    public SemanticEntity Entity => Object.Link(Data.OptionalLong("entity_id"), value => Object.Entity(value))!;
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public ExpressionOperation? Constraint => Object.Link(Data.OptionalLong("constraint_id"), value => Object.Expression(value));
}

public sealed class ConstraintMetadataDetails : SemanticDetail
{
    internal ConstraintMetadataDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticEntity Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Entity(value))!;
    public long Ordinal => Data.Long("ordinal");
    public ExpressionOperation? Expression => Object.Link(Data.OptionalLong("expression_id"), value => Object.Expression(value));
    public bool? Result => Data.OptionalBool("result");
    public bool IsDependent => Data.Bool("is_dependent");
}

public sealed class SynthesizedOriginDetails : SemanticDetail
{
    internal SynthesizedOriginDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticEntity Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Entity(value))!;
    public long Ordinal => Data.Long("ordinal");
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public SemanticEntity CausingEntity => Object.Link(Data.OptionalLong("causing_entity_id"), value => Object.Entity(value))!;
}

public sealed class SourceFileDetails : SemanticDetail
{
    internal SourceFileDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public string? Path => Object.Text(Data.OptionalLong("path_id"));
    public string Name => Object.Text(Data.OptionalLong("name_id"))!;
    public SemanticKind OriginKind => Object.Kind(Data.Long("origin_kind_id"));
    public string? VerifiedDigest => Object.Text(Data.OptionalLong("verified_digest_id"));
}

public sealed class IncludeOccurrenceDetails : SemanticDetail
{
    internal IncludeOccurrenceDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SourceFile File => Object.Link(Data.OptionalLong("file_id"), value => Object.File(value))!;
    public IncludeOccurrenceDetails? Parent => Object.Link(Data.OptionalLong("parent_id"), value => Object.Detail("include_occurrence", value, r => new IncludeOccurrenceDetails(Object, r)));
    public SourcePosition? Position => Object.Link(Data.OptionalLong("position_id"), value => Object.Position(value));
    public long Ordinal => Data.Long("ordinal");
    public BigInteger? FirstSequence => Number(Data.OptionalText("first_sequence"));
    public BigInteger? LastSequence => Number(Data.OptionalText("last_sequence"));
    public bool IsSystem => Data.Bool("is_system");
}

public sealed class SourceMappingSegmentDetails : SemanticDetail
{
    internal SourceMappingSegmentDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public BigInteger FirstSequence => RequiredNumber(Data.OptionalText("first_sequence"));
    public BigInteger LastSequence => RequiredNumber(Data.OptionalText("last_sequence"));
    public SourceFile File => Object.Link(Data.OptionalLong("file_id"), value => Object.File(value))!;
    public IncludeOccurrenceDetails? Include => Object.Link(Data.OptionalLong("include_id"), value => Object.Detail("include_occurrence", value, r => new IncludeOccurrenceDetails(Object, r)));
    public BigInteger LogicalLine => RequiredNumber(Data.OptionalText("logical_line"));
    public BigInteger? PhysicalLine => Number(Data.OptionalText("physical_line"));
    public string? PresumedName => Object.Text(Data.OptionalLong("presumed_name_id"));
}

public sealed class SourcePositionDetails : SemanticDetail
{
    internal SourcePositionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public BigInteger Sequence => RequiredNumber(Data.OptionalText("sequence"));
    public long Column => Data.Long("column");
    public BigInteger? OriginalSequence => Number(Data.OptionalText("original_sequence"));
    public long? OriginalColumn => Data.OptionalLong("original_column");
    public MacroInvocation? MacroInvocation => Object.Link(Data.OptionalLong("macro_invocation_id"), value => Object.MacroInvocation(value));
    public SourceFile? File => Object.Link(Data.OptionalLong("file_id"), value => Object.File(value));
    public BigInteger? LogicalLine => Number(Data.OptionalText("logical_line"));
    public SemanticKind OriginKind => Object.Kind(Data.Long("origin_kind_id"));
}

public sealed class SourceRangeDetails : SemanticDetail
{
    internal SourceRangeDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SourcePosition Start => Object.Link(Data.OptionalLong("start_id"), value => Object.Position(value))!;
    public SourcePosition End => Object.Link(Data.OptionalLong("end_id"), value => Object.Position(value))!;
    public SemanticKind EndpointKind => Object.Kind(Data.Long("endpoint_kind_id"));
}

public sealed class SourceOriginDetails : SemanticDetail
{
    internal SourceOriginDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SourcePosition? Position => Object.Link(Data.OptionalLong("position_id"), value => Object.Position(value));
    public SourceRange? Range => Object.Link(Data.OptionalLong("range_id"), value => Object.Range(value));
    public SourceRange? SpellingRange => Object.Link(Data.OptionalLong("spelling_range_id"), value => Object.Range(value));
    public SourceRange? ExpansionRange => Object.Link(Data.OptionalLong("expansion_range_id"), value => Object.Range(value));
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
}

public sealed class MacroDefinitionDetails : SemanticDetail
{
    internal MacroDefinitionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public string Name => Object.Text(Data.OptionalLong("name_id"))!;
    public string Text => Object.Text(Data.OptionalLong("text_id"))!;
    public SourcePosition? Position => Object.Link(Data.OptionalLong("position_id"), value => Object.Position(value));
    public SourceRange? ReplacementRange => Object.Link(Data.OptionalLong("replacement_range_id"), value => Object.Range(value));
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public bool IsFunctionLike => Data.Bool("is_function_like");
    public bool IsPredefined => Data.Bool("is_predefined");
    public bool IsUndef => Data.Bool("is_undef");
}

public sealed class MacroInvocationDetails : SemanticDetail
{
    internal MacroInvocationDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public MacroDefinition Definition => Object.Link(Data.OptionalLong("definition_id"), value => Object.MacroDefinition(value))!;
    public MacroInvocation? Parent => Object.Link(Data.OptionalLong("parent_id"), value => Object.MacroInvocation(value));
    public SourcePosition? Position => Object.Link(Data.OptionalLong("position_id"), value => Object.Position(value));
    public SourcePosition? EndPosition => Object.Link(Data.OptionalLong("end_position_id"), value => Object.Position(value));
    public string? ArgumentText => Object.Text(Data.OptionalLong("argument_text_id"));
    public long Ordinal => Data.Long("ordinal");
}

public sealed class SourceAttachmentDetails : SemanticDetail
{
    internal SourceAttachmentDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SourceFile File => Object.Link(Data.OptionalLong("file_id"), value => Object.File(value))!;
    public string Uri => Object.Text(Data.OptionalLong("uri_id"))!;
    public string? Encoding => Object.Text(Data.OptionalLong("encoding_id"));
    public string? Digest => Object.Text(Data.OptionalLong("digest_id"));
    public SemanticKind AcquisitionKind => Object.Kind(Data.Long("acquisition_kind_id"));
}

