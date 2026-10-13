using System.Numerics;
using System.Text;
namespace RoslynObj;

public sealed class Constant : SemanticEntity
{
    private readonly Lazy<IReadOnlyList<Symbol>> declarationSymbols;
    internal Constant(RoslynObject owner,long id,Row row) : base(owner,id)
    {
        Details=new(owner,row);
        declarationSymbols=new(() => Object.List("SELECT s.* FROM symbol s WHERE s.id IN (SELECT symbol_id FROM enumerator WHERE constant_id=$owner UNION SELECT id FROM template_parameter WHERE placeholder_constant_id=$owner) ORDER BY s.id",Id,r=>Object.Symbol(r.Long("id")),"symbol"));
    }
    public ConstantDetails Details { get; }
    public CppType? Type => Details.Type;
    public CppType? OriginalType => Details.OrigType;
    public string? Name => Object.Name(Object.Store.Required("constant",Id).OptionalLong("name_id"));
    public SemanticEntity? ContainingEntity => Details.ContainingEntity;
    public IReadOnlyList<Symbol> DeclarationSymbols => declarationSymbols.Value;
    public Symbol? DeclarationSymbol => DeclarationSymbols.Count==1 ? DeclarationSymbols[0] : null;
    public IReadOnlyList<TemplateParameter> DeclaredParameters => Object.List("SELECT * FROM template_parameter WHERE placeholder_constant_id=$owner ORDER BY id",Id,r=>Object.TemplateParameter(r.Long("id")),"template_parameter");
    public ConstantValue Value => Details.Value;
    public Operation? BackingExpression => Details.BackingExpression;
    public SourceInfo? Source => Details.SourceOrigin;
    public VariableSymbol? AssociatedStorageVariable => Details.AssociatedStorageVariable;
    public IReadOnlyList<ConstantComponentDetails> Components => Object.List("SELECT * FROM constant_component WHERE owner_id=$owner ORDER BY role_id,ordinal",Id,r=>new ConstantComponentDetails(Object,r));
    public IReadOnlyList<Constant> Elements => Object.List("SELECT c.* FROM constant_component r JOIN constant c ON c.id=r.constant_id JOIN kind k ON k.id=r.role_id WHERE r.owner_id=$owner AND k.name='element' ORDER BY r.ordinal",Id,r=>Object.Constant(r.Long("id")),"constant");
}

public class ConstantValue : SemanticEntity
{
    public SemanticKind Kind { get; }
    internal ConstantValue(RoslynObject owner,long id,Row row) : base(owner,id) { Kind=owner.Kind(row.Long("kind_id")); }
}

public class ConstantValue<TDetails> : ConstantValue where TDetails : SemanticDetail
{
    private readonly string table;
    private readonly Func<Row,TDetails> create;
    internal ConstantValue(RoslynObject owner,long id,Row row,string table,Func<Row,TDetails> create) : base(owner,id,row)
    { this.table=table; this.create=create; }
    public TDetails Payload => Object.Detail(table,Id,create);
}

public sealed class ReflectionValue : ConstantValue<ReflectionValueDetails>
{
    internal ReflectionValue(RoslynObject owner,long id,Row row) : base(owner,id,row,"reflection_value",r=>new(owner,r)) { }
    public ReflectionValueDetails Details => Payload;
    public SemanticKind EntityKind => Details.Kind;
    public SemanticEntity? Entity => Details.Entity;
    public BigInteger? LocalScopeNumber => Details.LocalScopeNumber;
}

public sealed class IntegerValue : ConstantValue
{
    internal IntegerValue(RoslynObject owner,long id,Row row) : base(owner,id,row) { }
    public IntegerValueDetails Details => Object.Detail("integer_value",Id,r=>new IntegerValueDetails(Object,r));
    public BigInteger Value => Details.Value;
}

public sealed class FloatingValue : ConstantValue
{
    internal FloatingValue(RoslynObject owner,long id,Row row) : base(owner,id,row) { }
    public FloatingValueDetails Details => Object.Detail("floating_value",Id,r=>new FloatingValueDetails(Object,r));
    public ExactFloatingPoint Value => new(Details.Class.Name switch { "finite" or "zero" => FloatingPointClass.Finite,
        "infinity"=>FloatingPointClass.Infinity,"nan"=>FloatingPointClass.NaN,
        _=>throw new IncompatibleObjectException("The target floating classification is unsupported.")},
        Details.Negative,checked((int)Details.Radix),Details.Significand??BigInteger.Zero,Details.Exponent??0,Details.NanPayload,Details.NanQuiet);
}

public sealed class StringValue : ConstantValue
{
    internal StringValue(RoslynObject owner,long id,Row row) : base(owner,id,row) { }
    public StringValueDetails Details => Object.Detail("string_value",Id,r=>new StringValueDetails(Object,r));
    public BigInteger Length => Details.Length;
    public long ElementBits => Details.ElementBits;
    public SemanticKind Encoding => Details.Encoding;
    public string? Text => Details.Text;
    public SemanticKind UnitKind => Details.UnitKind;
    public IReadOnlyList<uint> CodeUnits => StorageUnits;
    public IReadOnlyList<byte> TargetBytes => UnitKind.Name == "target_bytes" ? new ProjectedList<uint,byte>(StorageUnits,unit=>checked((byte)unit)) : [];
    public IReadOnlyList<uint> StorageUnits
    {
        get
        {
            if(Text is not {} text)
                return Object.List("SELECT * FROM string_unit WHERE owner_id=$owner ORDER BY ordinal",Id,r=>checked((uint)r.Long("unit")));
            var units=Encoding.Name switch
            {
                "ascii" => text.Select(c=>checked((uint)c)).ToArray(),
                "utf8" => System.Text.Encoding.UTF8.GetBytes(text).Select(b=>(uint)b).ToArray(),
                "utf16" when UnitKind.Name == "target_bytes" => (Object.Manifest.Target.Endianness == "little" ? System.Text.Encoding.Unicode : System.Text.Encoding.BigEndianUnicode).GetBytes(text).Select(b=>(uint)b).ToArray(),
                "utf16" => text.Select(c=>(uint)c).ToArray(),
                "utf32" when UnitKind.Name == "target_bytes" => new UTF32Encoding(Object.Manifest.Target.Endianness == "big",false,true).GetBytes(text).Select(b=>(uint)b).ToArray(),
                "utf32" => text.EnumerateRunes().Select(r=>(uint)r.Value).ToArray(),
                _=>throw new IncompatibleObjectException("The string fast-path encoding is unsupported.")
            };
            return Array.AsReadOnly(units);
        }
    }
}

public sealed class AggregateValue : ConstantValue
{
    internal AggregateValue(RoslynObject owner,long id,Row row) : base(owner,id,row) { }
    public AggregateValueDetails Details => Object.Detail("aggregate_value",Id,r=>new AggregateValueDetails(Object,r));
    public IReadOnlyList<AggregateElementDetails> Elements => Object.List("SELECT * FROM aggregate_element WHERE owner_id=$owner ORDER BY ordinal",Id,r=>new AggregateElementDetails(Object,r));
}

public sealed class LabelDifferenceValue : ConstantValue
{
    internal LabelDifferenceValue(RoslynObject owner,long id,Row row) : base(owner,id,row) { }
    public LabelDifferenceValueDetails Details => Object.Detail("label_difference_value",Id,r=>new LabelDifferenceValueDetails(Object,r));
    public LabelSymbol? LeftLabel => Details.LeftLabel;
    public LabelSymbol? RightLabel => Details.RightLabel;
    public LabelSymbol? ToLabel => LeftLabel;
    public LabelSymbol? FromLabel => RightLabel;
    public ConstantValue FromValue => Details.FromValue;
    public ConstantValue ToValue => Details.ToValue;
    public BigInteger? Offset => Details.Offset;
}
