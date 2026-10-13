using System.Numerics;
namespace RoslynObj;

public sealed class IncludeOccurrence : SemanticEntity
{
    internal IncludeOccurrence(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public IncludeOccurrenceDetails Details { get; }
    public SourceFile File => Details.File;
    public IncludeOccurrence? Parent => Details.Parent;
    /// <summary>The pinned EDG source-file descriptor has no include directive position; this is null.</summary>
    public SourcePosition? Position => Details.Position;
    public string? DisplayName => Details.DisplayName;
    public string? NameAsWritten => Details.NameAsWritten;
    public long Ordinal => Details.Ordinal;
    public BigInteger? FirstSequence => Details.FirstSequence;
    public BigInteger? LastSequence => Details.LastSequence;
    public BigInteger? FirstLine => Details.FirstLine;
    public CppModule? Module => Details.Module;
    public bool IsSystem => Details.IsSystem;
    public bool UsesAngleBrackets => Details.UsesAngleBrackets;
    public bool IsIncludeFile => Details.IsIncludeFile;
    public bool IsPreinclude => Details.IsPreinclude;
    public bool IsMacrosOnly => Details.IsMacrosOnly;
    public bool IsTopLevel => Details.IsTopLevel;
    public bool IsTopLevelFromPch => Details.IsTopLevelFromPch;
    public bool IsImplicitInclude => Details.IsImplicitInclude;
    public IReadOnlyList<SemanticEntity> Children => Object.List("SELECT * FROM source_descriptor_child WHERE owner_id=$owner ORDER BY ordinal",Id,r=>Object.Entity(r.Long("child_id")));
}

/// <summary>A retained presumed-source mapping descriptor; directive token positions are not retained.</summary>
public sealed class LineDirective : SemanticEntity
{
    internal LineDirective(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public LineDirectiveDetails Details { get; }
    public string PresumedName => Details.PresumedName;
    public BigInteger FirstSequence => Details.FirstSequence;
    public BigInteger LastSequence => Details.LastSequence;
    public BigInteger FirstLine => Details.FirstLine;
    public IncludeOccurrence? Include => Details.Include;
    public LineDirective? Parent => Details.Parent;
    public IReadOnlyList<SemanticEntity> Children => Object.List("SELECT * FROM source_descriptor_child WHERE owner_id=$owner ORDER BY ordinal",Id,r=>Object.Entity(r.Long("child_id")));
}

public sealed class SourceMappingSegment : SemanticEntity
{
    internal SourceMappingSegment(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public SourceMappingSegmentDetails Details { get; }
    public BigInteger FirstSequence => Details.FirstSequence;
    public BigInteger LastSequence => Details.LastSequence;
    public SourceFile File => Details.File;
    public IncludeOccurrence? Include => Details.Include;
    public LineDirective? LineDirective => Details.LineDirective;
    public BigInteger LogicalLine => Details.LogicalLine;
    public BigInteger? PhysicalLine => Details.PhysicalLine;
    public string? PresumedName => Details.PresumedName;
    public bool EndsAtEndOfSource => Details.EndsAtEndOfSource;
}
