using System.Numerics;
namespace RoslynObj;

public sealed class ExceptionSpecification : SemanticEntity
{
    internal ExceptionSpecification(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public ExceptionSpecificationDetails Details { get; }
    public SemanticKind Kind => Details.Kind;
    public bool CopyFromPrototype => Details.CopyFromPrototype;
    public FunctionSymbol? CopyFromPrototypeRoutine => Details.CopyFromPrototypeRoutine;
    public bool ArgumentCached => Details.ArgumentCached;
    public Constant? NoexceptArgument => Details.NoexceptArgument;
    public bool? NoexceptEffective => Details.NoexceptEffective;
    public bool IsDependent => Details.IsDependent;
    public bool IsIndeterminate => Details.IsIndeterminate;
    public bool ThrowAny => Details.ThrowAny;
    public SourceInfo? Source => Details.SourceOrigin;
    public IReadOnlyList<CppType> Types => Object.List("SELECT t.* FROM exception_specification_type r JOIN type t ON t.id=r.type_id WHERE r.owner_id=$owner ORDER BY r.ordinal",Id,r=>Object.Type(r.Long("id")),"type");
}

public sealed class RequiresClause : SemanticEntity
{
    internal RequiresClause(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public RequiresClauseDetails Details { get; }
    public ExpressionOperation Constraint => Details.ConstraintExpression;
    public SourcePosition? RequiresPosition => Details.RequiresPosition;
}

public sealed class GnuRoutineSupplement : SemanticEntity
{
    internal GnuRoutineSupplement(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public GnuRoutineSupplementDetails Details { get; }
    public string? Section => Details.Section;
    public string? AssemblyName => Details.AssemblyName;
    /// <summary>The retained aliased routine; for an ifunc this is its resolver.</summary>
    public FunctionSymbol? AliasedRoutine => Details.AliasedRoutine;
    public FunctionSymbol? InlinePartner => Details.InlinePartner;
    public VariableSymbol? ResolverVariable => Details.ResolverVariable;
    public BigInteger? ConstructorPriority => Details.ConstructorPriority;
    public BigInteger? DestructorPriority => Details.DestructorPriority;
    public bool? IsRepresentative => Details.IsRepresentative;
    public bool? IsTargetSpecificVersion => Details.IsTargetSpecificVersion;
    public bool? ResolverRequired => Details.ResolverRequired;
    public FunctionSymbol? Representative => Details.Representative;
    public IReadOnlyList<FunctionSymbol> OwningRoutines => Object.List("SELECT * FROM routine_symbol WHERE gnu_supplement_id=$owner ORDER BY id",Id,r=>Object.Function(r.Long("id")),"routine_symbol");
    /// <summary>Recorded dispatch priority order, with the default routine first.</summary>
    public IReadOnlyList<FunctionSymbol> TargetedVersions => Object.List("SELECT r.* FROM routine_multiversion m JOIN routine_symbol r ON r.id=m.routine_id WHERE m.owner_id=$owner ORDER BY m.ordinal",Id,r=>Object.Function(r.Long("id")),"routine_symbol");
    public IReadOnlyList<SemanticKind> TargetArchitectures => Object.List("SELECT * FROM routine_target_architecture WHERE owner_id=$owner ORDER BY kind_id",Id,r=>Object.Kind(r.Long("kind_id")));
}

public sealed partial class FunctionSymbol
{
    public SemanticKind? SpecialFunctionKind => Details.SpecialFunction;
    public SemanticKind? OperatorName => Details.OperatorName;
    public SemanticKind? BuiltinKind => Details.BuiltinKind;
    public bool IsPureVirtual => Details.IsPure;
    public bool? IsPureFunction => Details.IsPureFunction;
    public bool IsVirtual => Details.IsVirtual;
    public bool IsOverride => Details.IsOverride;
    public bool IsFinal => Details.IsFinal;
    public bool IsTrivialDefaultConstructor => Details.IsTrivialDefaultConstructor;
    public bool IsTrivialCopyFunction => Details.IsTrivialCopyFunction;
    public bool IsTrivialDestructor => Details.IsTrivialDestructor;
    public bool IsExplicitConstructor => Details.IsExplicitConstructor;
    public bool IsExplicitConversionFunction => Details.IsExplicitConversionFunction;
    public bool IsInheritingConstructor => Details.IsInheritingCtor;
    public bool IsDelegatingConstructor => Details.IsDelegatingCtor;
    public bool IsLambdaBody => Details.IsLambdaBody;
    public bool IsCoroutine => Details.IsCoroutine;
    public SemanticKind? ConstructorOrDestructorKind => Details.CtorDtorKind;
    public CppType? DefinitionType => Details.DeclaredType;
    public SemanticKind? DefinitionStorageClass => Details.DeclaredStorageClass;
    public RequiresClause? TrailingRequiresClause => Details.TrailingRequiresClause;
    public FunctionSymbol? InheritedRoutine => Details.InheritedRoutine;
    public FunctionSymbol? LambdaCallOperator => Details.LambdaCallOperator;
    public TemplateDescriptor? DeductionGuideTemplate => Details.DeductionGuideTemplate;
    public FunctionSymbol? OverridingFunctionForWrapper => Details.OverridingFunction;
    public FunctionSymbol? OverriddenFunctionForWrapper => Details.OverriddenFunction;
    public BigInteger? ThisAdjustment => Details.ThisAdjustment;
    public BigInteger? ReturnAdjustment => Details.ReturnAdjustment;
    public BigInteger? VirtualCallIndex => Details.VcallIndex;
    public BigInteger? VirtualBaseIndex => Details.VbaseIndex;
    public BigInteger? VirtualFunctionNumber => Details.VirtualFunctionNumber;
    public long? ConstexprIntrinsic => Details.ConstexprIntrinsic;
    public GnuRoutineSupplement? GnuSupplement => Details.GnuSupplement;
    public FunctionSymbol? IfuncResolver => Details.IsIfunc==true?GnuSupplement?.AliasedRoutine:null;
    public IReadOnlyList<FunctionSymbol> AlternateEntryPoints => Object.List("SELECT * FROM routine_symbol WHERE primary_entry_id=$owner ORDER BY id",Id,r=>Object.Function(r.Long("id")),"routine_symbol");
    public IReadOnlyList<CppType> BefriendingClasses => Object.List("SELECT t.* FROM routine_befriending_class r JOIN type t ON t.id=r.type_id WHERE r.owner_id=$owner ORDER BY r.ordinal",Id,r=>Object.Type(r.Long("id")),"type");
    public IReadOnlyList<SemanticEntity> SelectedOverrides => Object.List("SELECT * FROM routine_selected_override WHERE owner_id=$owner ORDER BY ordinal",Id,r=>Object.Entity(r.Long("entity_id")));
}

public sealed partial class RoutineType
{
    public CppType? ThisClass => Routine.ThisType;
    public bool HasThisParameter => Routine.HasThisParam;
    public SemanticKind? CallingConvention => Routine.CallingConvention;
    public bool? IsCallingConventionExplicit => Routine.ExplicitCallingConvention;
    public SemanticKind? LanguageLinkage => Routine.LanguageLinkage;
    public bool IsLanguageLinkageExplicit => Routine.RoutineNameLinkageIsExplicit;
    public bool HasTrailingReturnType => Routine.HasTrailingReturnType;
    public bool ReturnsValueViaCopyConstructor => Routine.ValueReturnedByCctor;
    public bool? ReturnsValueAsParameter => Routine.ValueReturnedAsParameter;
    public bool? ReturnValueParameterFollowsThis => Routine.ReturnValueParameterFollowsThis;
    public bool DoesNotReturn => Routine.DoesNotReturn;
    public bool HasPrototype => Routine.HasPrototype;
    public bool IsVariadic => Routine.IsVariadic;
    public FunctionSymbol? AssociatedRoutine => Routine.AssociatedRoutine;
    public Scope? PrototypeScope => Routine.PrototypeScope;
    public RoutineQualifiers ObjectQualifiers => new(Routine,false);
    public RoutineQualifiers ThisQualifiers => new(Routine,true);
}

public sealed class RoutineQualifiers
{
    private readonly RoutineTypeDetails details;
    private readonly bool pointer;
    internal RoutineQualifiers(RoutineTypeDetails details,bool pointer) { this.details=details;this.pointer=pointer; }
    public bool IsConst => pointer?details.IsThisConst:details.IsConst;
    public bool IsVolatile => pointer?details.IsThisVolatile:details.IsVolatile;
    public bool IsRestrict => pointer?details.IsThisRestrict:details.IsRestrict;
    public bool IsAtomic => pointer?details.IsThisAtomic:details.IsAtomic;
    public bool IsNullable => pointer?details.IsThisNullable:details.IsNullable;
    public bool IsNonnull => pointer?details.IsThisNonnull:details.IsNonnull;
    public bool IsNullUnspecified => pointer?details.IsThisNullUnspecified:details.IsNullUnspecified;
    public bool IsUnaligned => pointer?details.IsThisUnaligned:details.IsUnaligned;
    public bool IsNear => pointer?details.IsThisNear:details.IsNear;
    public bool IsFar => pointer?details.IsThisFar:details.IsFar;
    public bool IsShared => pointer?details.IsThisShared:details.IsShared;
    public bool IsUpcStrict => pointer?details.IsThisUpcStrict:details.IsUpcStrict;
    public bool IsUpcRelaxed => pointer?details.IsThisUpcRelaxed:details.IsUpcRelaxed;
    public long? AddressSpace => pointer?details.ThisAddressSpace:details.AddressSpace;
}
