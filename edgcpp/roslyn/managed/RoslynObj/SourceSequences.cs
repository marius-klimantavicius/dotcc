namespace RoslynObj;

public sealed class SourceEntry : SemanticEntity
{
    private readonly Lazy<IReadOnlyList<Scope>> sequenceOwners;
    internal SourceEntry(RoslynObject owner,long id,Row row) : base(owner,id)
    {
        Details=new(owner,row);
        sequenceOwners=new(() => Object.List("SELECT s.* FROM scope s WHERE s.id IN (SELECT owner_id FROM scope_source_entry WHERE entry_id=$owner UNION SELECT h.owner_id FROM scope_source_sublist h JOIN source_sublist_entry e ON e.owner_id=h.sublist_id WHERE e.entry_id=$owner) ORDER BY s.id",Id,r=>Object.Scope(r.Long("id")),"scope"));
    }
    public SourceEntryDetails Details { get; }
    public SemanticKind Tag => Details.TagKind;
    public SemanticEntity? Target => Details.Target;
    public Declaration? Declaration => Details.Declaration;
    public Symbol? DeclaredSymbol => Declaration?.Symbol as Symbol ?? SourceSequenceTraversal.DeclaredSymbol(Target);
    public IReadOnlyList<Scope> SourceSequenceOwners => sequenceOwners.Value;
    public Scope? SourceSequenceOwner => SourceSequenceOwners.Count==1 ? SourceSequenceOwners[0] : null;
}

public sealed class SourceSublist : SemanticEntity
{
    internal SourceSublist(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public SourceSublistDetails Details { get; }
    public IReadOnlyList<SourceEntry> Entries => Object.List("SELECT e.* FROM source_sublist_entry r JOIN source_entry e ON e.id=r.entry_id WHERE r.owner_id=$owner ORDER BY r.ordinal",Id,r=>Object.SourceEntry(r.Long("id")),"source_entry");
}

public sealed class RecordDeclaration : SemanticEntity
{
    internal RecordDeclaration(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public RecordDeclarationDetails Details { get; }
    public CppType RecordType => Details.RecordType;
    public long Ordinal => Details.Ordinal;
    public SemanticKind Tag => Details.TagKind;
    public SemanticEntity? Target => Details.Target;
    public Declaration? Declaration => Details.Declaration;
    public Symbol? DeclaredSymbol => Declaration?.Symbol as Symbol ?? SourceSequenceTraversal.DeclaredSymbol(Target);
}

public sealed class SourceEndConstruct : SemanticEntity
{
    internal SourceEndConstruct(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public SourceEndConstructDetails Details { get; }
    public SemanticKind TargetTag => Details.TargetTagKind;
    public SemanticEntity? Target => Details.Target;
    public SourcePosition? Position => Details.Position;
}

public sealed class InstantiationDirective : SemanticEntity
{
    internal InstantiationDirective(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public InstantiationDirectiveDetails Details { get; }
    public SemanticEntity? Target => Details.Target;
    public CppType? DeclaredType => Details.DeclaredType;
    public SourceInfo? Source => Details.SourceOrigin;
    public bool DoNotInstantiate => Details.DoNotInstantiate;
    public DeclarationPositionsDetails? Positions => Object.Store.GetByOwner("declaration_positions",Id) is {} row ? new(Object,row) : null;
}

public sealed class LinkageSpecification : SemanticEntity
{
    internal LinkageSpecification(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public LinkageSpecificationDetails Details { get; }
    public Constant NameString => Details.NameString;
    public SemanticKind NameLinkage => Details.NameLinkage;
    public SourcePosition? Position => Details.Position;
    public SourcePosition? EndPosition => Details.EndPosition;
}

public sealed class MicrosoftIfExists : SemanticEntity
{
    internal MicrosoftIfExists(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public MicrosoftIfExistsDetails Details { get; }
    public SemanticEntity? Target => Details.Target;
    public NameReference? NameReference => Details.NameReference;
    public SourcePosition? Position => Details.Position;
    public bool? IsIfExists => Details.IsIfExists;
    public bool IsThis => Details.IsThis;
    public bool IsBegin => Target is not null || IsThis;
}

public sealed class PragmaDirective : SemanticEntity
{
    internal PragmaDirective(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public PragmaDetails Details { get; }
    public SemanticKind Kind => Details.Kind;
    public SemanticEntity? Target => Details.Target;
    public SourceInfo? Source => Details.SourceOrigin;
    public SourceEntry? SourceEntry => Details.SourceEntry;
    public string? Text => Details.Text;
    public bool IgnoreInBackEnd => Details.IgnoreInBackEnd;
    public bool IsMicrosoftOperator => Details.IsMicrosoftOperator;
    public PragmaStdcDetails? Standard => Optional("pragma_stdc",r=>new PragmaStdcDetails(Object,r));
    public PragmaUpcDetails? Upc => Optional("pragma_upc",r=>new PragmaUpcDetails(Object,r));
    public PragmaIdentDetails? Ident => Optional("pragma_ident",r=>new PragmaIdentDetails(Object,r));
    public PragmaCommentDetails? Comment => Optional("pragma_comment",r=>new PragmaCommentDetails(Object,r));
    public PragmaConformDetails? Conformance => Optional("pragma_conform",r=>new PragmaConformDetails(Object,r));
    public PragmaIncludeAliasDetails? IncludeAlias => Optional("pragma_include_alias",r=>new PragmaIncludeAliasDetails(Object,r));
    public PragmaGccDetails? Gcc => Optional("pragma_gcc",r=>new PragmaGccDetails(Object,r));
    private T? Optional<T>(string table,Func<Row,T> create) where T : SemanticDetail => Object.Store.Get(table,Id) is {} row ? create(row) : null;
}

internal static class SourceSequenceTraversal
{
    internal static Symbol? DeclaredSymbol(SemanticEntity? target) => target switch
    {
        Symbol symbol=>symbol,CppType type=>type.DeclarationSymbol,
        Constant constant=>constant.DeclarationSymbol,TemplateDescriptor template=>template.DeclarationSymbol,
        _=>null
    };
    internal static IEnumerable<SourceEntry> Flatten(IReadOnlyList<SourceEntry> entries)
    {
        var pending=new Stack<IEnumerator<SourceEntry>>();
        pending.Push(entries.GetEnumerator());
        try
        {
            while(pending.Count>0)
            {
                var current=pending.Peek();
                if(!current.MoveNext()) { current.Dispose();pending.Pop();continue; }
                var entry=current.Current;
                if(entry.Target is SourceSublist sublist)pending.Push(sublist.Entries.GetEnumerator());
                else yield return entry;
            }
        }
        finally { while(pending.Count>0)pending.Pop().Dispose(); }
    }
}
