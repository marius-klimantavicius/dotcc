namespace RoslynObj;

/// <summary>A retained static assertion and its recorded condition constant.</summary>
public sealed class StaticAssertion : SemanticEntity
{
    internal StaticAssertion(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public StaticAssertionDetails Details { get; }
    public Constant Condition => Details.Condition;
    /// <summary>The recorded message constant, absent for a terse assertion.</summary>
    public Constant? Message => Details.Message;
    public SourceInfo? Source => Details.SourceOrigin;
}
