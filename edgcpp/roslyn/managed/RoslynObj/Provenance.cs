using System.Numerics;
namespace RoslynObj;

public sealed class SourceInfo : SemanticEntity
{
    internal SourceInfo(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public SourceOriginDetails Details { get; }
    public SourcePosition? Position => Details.Position;
    public SourceRange? Range => Details.Range;
    public MacroInvocation? MacroExpansion => Position?.MacroExpansion;
}

public sealed class SourcePosition : SemanticEntity
{
    internal SourcePosition(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public SourcePositionDetails Details { get; }
    public BigInteger Sequence => Details.Sequence;
    public long Column => Details.Column;
    public BigInteger? OriginalSequence => Details.OriginalSequence;
    public long? OriginalColumn => Details.OriginalColumn;
    public SourceFile? File => Details.File;
    public string? PresumedName => Details.PresumedName;
    public BigInteger? LogicalLine => Details.LogicalLine;
    public BigInteger? PhysicalLine => Details.PhysicalLine;
    public IncludeOccurrence? Include => Details.Include;
    public SourceMappingSegment? Mapping => Details.MappingSegment;
    public SourcePosition? SpellingPosition => Details.SpellingPosition;
    public bool AtEndOfSource => Details.AtEndOfSource;
    public MacroInvocation? MacroExpansion => Details.MacroInvocation;
    public override string ToString() => $"{PresumedName ?? File?.Name}:{LogicalLine}:{Column}";
}

public sealed class SourceRange : SemanticEntity
{
    internal SourceRange(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public SourceRangeDetails Details { get; }
    public SourcePosition Start => Details.Start;
    public SourcePosition End => Details.End;
    public SemanticKind EndpointKind => Details.EndpointKind;
}

public sealed class SourceFile : SemanticEntity
{
    internal SourceFile(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public SourceFileDetails Details { get; }
    public override string Name => IncludeOccurrences.FirstOrDefault()?.DisplayName ?? Path;
    public string Path => Details.Path;
    public IReadOnlyList<IncludeOccurrence> IncludeOccurrences => Object.List("SELECT * FROM include_occurrence WHERE file_id=$owner ORDER BY ordinal,id",Id,r=>Object.Include(r.Long("id")),"include_occurrence");
}

public sealed class MacroDefinition : SemanticEntity
{
    internal MacroDefinition(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public MacroDefinitionDetails Details { get; }
    public override string Name => base.Name!;
    public string Text => Details.Text;
    public SourcePosition? Position => SourceCorrespondence?.Source?.Position;
    public SourceRange? ReplacementRange => Details.ReplacementRange;
}

public sealed class MacroInvocation : SemanticEntity
{
    internal MacroInvocation(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public MacroInvocationDetails Details { get; }
    public MacroDefinition Definition => Details.Definition;
    public MacroInvocation? Parent => Details.Parent;
    public SourcePosition? Position => Details.Position;
    public string? ArgumentText => Details.ArgumentText;
    public SourcePosition? EndPosition => Details.EndPosition;
    public IReadOnlyList<MacroInvocation> Children => Object.List("SELECT * FROM macro_invocation WHERE parent_id=$owner ORDER BY ordinal",Id,r=>Object.MacroInvocation(r.Long("id")),"macro_invocation");
}

public sealed class MacroCollection
{
    private readonly RoslynObject owner;
    internal MacroCollection(RoslynObject owner)=>this.owner=owner;
    public IReadOnlyList<MacroDefinition> Definitions => owner.List("SELECT * FROM macro_definition ORDER BY id",0,r=>owner.MacroDefinition(r.Long("id")),"macro_definition");
    public IReadOnlyList<MacroInvocation> Invocations => owner.List("SELECT * FROM macro_invocation ORDER BY ordinal,id",0,r=>owner.MacroInvocation(r.Long("id")),"macro_invocation");
}
