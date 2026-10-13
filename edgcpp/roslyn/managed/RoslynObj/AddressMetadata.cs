using System.Numerics;
namespace RoslynObj;

public sealed class AddressValue : ConstantValue<AddressValueDetails>
{
    internal AddressValue(RoslynObject owner,long id,Row row) : base(owner,id,row,"address_value",r=>new(owner,r)) { }
    public AddressValueDetails Details => Payload;
    public SemanticEntity? BaseEntity => Details.BaseEntity;
    public SemanticKind BaseKind => Details.BaseKind;
    /// <summary>Signed offset in target bytes, as recorded by EDG.</summary>
    public BigInteger ByteOffset => Details.Offset;
    public bool OnePastEnd => Details.OnePastEnd;
    public SubobjectPath? SubobjectPath => (SubobjectPath?)Details.SubobjectPath;
}

public sealed class SubobjectPath : SemanticEntity
{
    internal SubobjectPath(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public SubobjectPathDetails Details { get; }
    public SemanticKind Kind => Details.Kind;
    public FieldSymbol? Field => Details.Field;
    public BaseSubobject? BaseSubobject => Details.BaseSubobject;
    /// <summary>Signed array element count, not a byte displacement.</summary>
    public BigInteger? ElementOffset => Details.ElementOffset;
    public SubobjectPath? Next => Details.Next;
    public bool IsConverted => Details.IsConverted;
    public IEnumerable<SubobjectPath> Steps => EnumerateSteps();
    private IEnumerable<SubobjectPath> EnumerateSteps()
    {
        for(SubobjectPath? step=this;step is not null;step=step.Next)yield return step;
    }
}
