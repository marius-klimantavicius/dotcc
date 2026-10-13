namespace RoslynObj;

/// <summary>The actual retained correspondence shared by associated semantic entities.</summary>
public sealed class SourceCorrespondence : SemanticEntity
{
    internal SourceCorrespondence(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public SourceCorrespondenceDetails Details { get; }
    public string? SourceName => Details.UnnamedEntityGivenFabricatedName==true?null:Details.NameHasBeenMangled==true?Details.UnmangledNameOrMangledEncoding:Details.CurrentName;
    public override string? Name => SourceName;
    public override string? CurrentName => Details.CurrentName;
    public string? FabricatedName => Details.UnnamedEntityGivenFabricatedName==true?Details.NameHasBeenMangled==true?Details.UnmangledNameOrMangledEncoding:CurrentName:null;
    public string? MangledEncoding => Details.NameHasBeenMangled==false?Details.UnmangledNameOrMangledEncoding:null;
    public Scope? ParentScope => Details.ParentScope;
    public FunctionSymbol? EnclosingRoutine => Details.EnclosingRoutine;
    public SemanticEntity? ContainingEntity => ParentScope is {Kind.Name:"sck_class_struct_union" or "sck_class_reactivation" or "sck_namespace" or "sck_enum",AssociatedEntity: {} entity}?entity:EnclosingRoutine??(SemanticEntity?)ParentScope;
    public bool Referenced => Details.Referenced;
    public bool? Needed => Details.Needed;
    public bool IsLocalToFunction => Details.IsLocalToFunction;
    public bool IsClassMember => Details.IsClassMember;
    public bool? NameHasBeenMangled => Details.NameHasBeenMangled;
    public bool? UnnamedEntityGivenFabricatedName => Details.UnnamedEntityGivenFabricatedName;
    public bool? IsDeclAfterFirstInCommaList => Details.IsDeclAfterFirstInCommaList;
    public bool CopiedFromSecondaryTransUnit => Details.CopiedFromSecondaryTransUnit;
    public bool SameNameAsExternalEntityInSecondaryTransUnit => Details.SameNameAsExternalEntityInSecondaryTransUnit;
    public bool MemberOfUnknownBase => Details.MemberOfUnknownBase;
    public bool QualifiedUnknownBaseMember => Details.QualifiedUnknownBaseMember;
    public bool? MemberOfUnknownSuper => Details.MemberOfUnknownSuper;
    public bool? MicrosoftIdentifierUsed => Details.MicrosoftIdentifierUsed;
    public bool? MarkedAsGnuExtension => Details.MarkedAsGnuExtension;
    public bool IsDeprecatedOrUnavailable => Details.IsDeprecatedOrUnavailable;
    public bool Externalized => Details.Externalized;
    public bool MaybeUnused => Details.MaybeUnused;
    public bool IsImportedFromModule => Details.IsImportedFromModule;
    public SourceInfo? Source => Details.SourceOrigin;
    public SourceEntry? SourceSequenceEntry => Details.SourceSequenceEntry;
    public SemanticKind Access => Details.Access;
    public SemanticKind Linkage => Details.Linkage;
    public IReadOnlyList<SemanticEntity> Entities => Object.List("SELECT e.* FROM entity_source_correspondence c JOIN entity e ON e.id=c.owner_id WHERE c.source_correspondence_id=$owner ORDER BY e.id",Id,r=>Object.Entity(r.Long("id")),"entity");
}

public sealed class UsingDeclaration : SemanticEntity
{
    internal UsingDeclaration(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public UsingDeclarationDetails Details { get; }
    public SemanticEntity? Target => Details.Target;
    public Symbol? TargetSymbol => SourceSequenceTraversal.DeclaredSymbol(Target);
    public IReadOnlyList<Scope> LexicalScopes => Object.List("SELECT s.* FROM scope s WHERE s.id IN(SELECT owner_id FROM scope_member WHERE member_id=$owner) ORDER BY s.id",Id,r=>Object.Scope(r.Long("id")),"scope");
    public Scope? LexicalScope => Details.LexicalScope;
    public bool IsDirective => Details.IsDirective;
    public bool IsEnumerator => Details.IsEnumerator;
    public SemanticKind? AdjustedAccess => Details.AdjustedAccess;
    public CppType? QualifierType => Details.QualifierType;
    public NamespaceSymbol? QualifierNamespace => Details.QualifierNamespace;
    public bool? IsGlobalQualified => Details.IsGlobalQualified;
    public UsingDeclaration? NextInSet => (UsingDeclaration?)Details.NextInSet;
    public SourceInfo? Source => Details.SourceOrigin;
}
