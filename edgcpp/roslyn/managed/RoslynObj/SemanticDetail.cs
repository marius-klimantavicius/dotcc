using System.Globalization;
using System.Numerics;

namespace RoslynObj;

/// <summary>Immutable typed details backed by normalized scalar and foreign-key columns.</summary>
public abstract class SemanticDetail
{
    internal RoslynObject Object { get; }
    private protected Row Data { get; }
    internal SemanticDetail(RoslynObject owner, Row row) { Object = owner; Data = row; }
    private protected static BigInteger RequiredNumber(string? value) => value is null
        ? throw new IncompatibleObjectException("A required exact scalar is missing.")
        : BigInteger.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
    private protected static BigInteger? Number(string? value) => value is null ? null
        : BigInteger.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
}

public sealed class SemanticRecord<TDetails> : SemanticEntity where TDetails : SemanticDetail
{
    public TDetails Details { get; }
    internal SemanticRecord(RoslynObject owner, long id, TDetails details) : base(owner, id) => Details = details;
}
