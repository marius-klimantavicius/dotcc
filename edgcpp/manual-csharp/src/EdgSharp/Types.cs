// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Handwritten adaptation: types.c canonical type identity and exprutil.c
// determine_arithmetic_conversions_full/type_after_integral_promotion.
using System.Collections.ObjectModel;
using System.Numerics;

namespace EdgSharp;

/// <summary>Explicit initial target profile, independent of CLR integer widths and host ABI.</summary>
public sealed class TargetInfo
{
    public static TargetInfo Lp64 { get; } = new();
    public int CharBits => 8;
    public int ShortBits => 16;
    public int IntBits => 32;
    public int LongBits => 64;
    public int LongLongBits => 64;
    public int PointerBits => 64;
    public bool PlainCharIsSigned => true;
    private TargetInfo() { }
}

public enum LanguageProfile { Cpp20 }
public sealed record AnalysisOptions
{
    public LanguageProfile Language { get; init; } = LanguageProfile.Cpp20;
    public TargetInfo Target { get; init; } = TargetInfo.Lp64;
}

public enum TypeKind { Error, Void, Integer, Function }
public enum IntegerKind { Bool, Char, SignedChar, UnsignedChar, Short, UnsignedShort, Int, UnsignedInt, Long, UnsignedLong, LongLong, UnsignedLongLong }
[Flags] public enum TypeQualifiers { None = 0, Const = 1 }

/// <summary>Identity-bearing canonical type, owned by one compilation session.</summary>
public abstract class SemanticType
{
    public TypeKind Kind { get; }
    public string DisplayName { get; }
    public TypeQualifiers Qualifiers { get; }
    internal SemanticType(TypeKind kind, string name, TypeQualifiers qualifiers = TypeQualifiers.None)
        => (Kind, DisplayName, Qualifiers) = (kind, name, qualifiers);
    public override string ToString() => DisplayName;
}

public sealed class IntegerType : SemanticType
{
    public IntegerKind IntegerKind { get; }
    public int Width { get; }
    public int Rank { get; }
    public bool IsSigned { get; }
    public BigInteger Minimum => IsSigned ? -(BigInteger.One << (Width - 1)) : BigInteger.Zero;
    public BigInteger Maximum => IntegerKind == IntegerKind.Bool ? BigInteger.One :
        (BigInteger.One << (IsSigned ? Width - 1 : Width)) - 1;
    internal IntegerType(IntegerKind kind, string name, int width, int rank, bool signed, TypeQualifiers qualifiers = 0)
        : base(TypeKind.Integer, (qualifiers.HasFlag(TypeQualifiers.Const) ? "const " : "") + name, qualifiers)
        => (IntegerKind, Width, Rank, IsSigned) = (kind, width, rank, signed);
}

public sealed class FunctionType : SemanticType
{
    public SemanticType ReturnType { get; }
    public ReadOnlyCollection<SemanticType> ParameterTypes { get; }
    internal FunctionType(SemanticType result, SemanticType[] parameters)
        : base(TypeKind.Function, $"{result} ({string.Join(", ", parameters.Select(p => p.DisplayName))})")
        => (ReturnType, ParameterTypes) = (result, Array.AsReadOnly((SemanticType[])parameters.Clone()));
}

internal sealed class SpecialType(TypeKind kind, string name) : SemanticType(kind, name);

internal sealed class TypeTable
{
    private readonly Dictionary<(IntegerKind, TypeQualifiers), IntegerType> integers = [];
    private readonly List<FunctionType> functions = [];
    internal SemanticType Void { get; } = new SpecialType(TypeKind.Void, "void");
    internal SemanticType Error { get; } = new SpecialType(TypeKind.Error, "<error>");
    internal TypeTable(TargetInfo target)
    {
        Add(IntegerKind.Bool, "bool", target.CharBits, 0, false);
        Add(IntegerKind.Char, "char", target.CharBits, 1, target.PlainCharIsSigned);
        Add(IntegerKind.SignedChar, "signed char", target.CharBits, 1, true);
        Add(IntegerKind.UnsignedChar, "unsigned char", target.CharBits, 1, false);
        Add(IntegerKind.Short, "short", target.ShortBits, 2, true);
        Add(IntegerKind.UnsignedShort, "unsigned short", target.ShortBits, 2, false);
        Add(IntegerKind.Int, "int", target.IntBits, 3, true);
        Add(IntegerKind.UnsignedInt, "unsigned int", target.IntBits, 3, false);
        Add(IntegerKind.Long, "long", target.LongBits, 4, true);
        Add(IntegerKind.UnsignedLong, "unsigned long", target.LongBits, 4, false);
        Add(IntegerKind.LongLong, "long long", target.LongLongBits, 5, true);
        Add(IntegerKind.UnsignedLongLong, "unsigned long long", target.LongLongBits, 5, false);
    }
    private void Add(IntegerKind kind, string name, int width, int rank, bool signed)
    {
        integers.Add((kind, 0), new IntegerType(kind, name, width, rank, signed));
        integers.Add((kind, TypeQualifiers.Const), new IntegerType(kind, name, width, rank, signed, TypeQualifiers.Const));
    }
    internal IntegerType Integer(IntegerKind kind, TypeQualifiers qualifiers = 0) => integers[(kind, qualifiers)];
    internal SemanticType Unqualified(SemanticType type) => type is IntegerType i ? Integer(i.IntegerKind) : type;
    internal SemanticType Const(SemanticType type) => type is IntegerType i ? Integer(i.IntegerKind, TypeQualifiers.Const) : type;
    internal FunctionType Function(SemanticType result, IEnumerable<SemanticType> parameters)
    {
        SemanticType[] array = parameters.Select(Unqualified).ToArray();
        foreach (var existing in functions)
            if (ReferenceEquals(existing.ReturnType, result) && existing.ParameterTypes.SequenceEqual(array)) return existing;
        var created = new FunctionType(result, array);
        functions.Add(created);
        return created;
    }
    internal IntegerType Promote(IntegerType type) => type.Rank < Integer(IntegerKind.Int).Rank ? Integer(IntegerKind.Int) : Integer(type.IntegerKind);
    internal IntegerType UsualArithmeticConversions(IntegerType left, IntegerType right)
    {
        left = Promote(left);
        right = Promote(right);
        if (ReferenceEquals(left, right)) return left;
        if (left.IsSigned == right.IsSigned) return left.Rank > right.Rank ? left : right;
        IntegerType signed = left.IsSigned ? left : right;
        IntegerType unsigned = left.IsSigned ? right : left;
        if (unsigned.Rank >= signed.Rank) return unsigned;
        if (signed.Maximum >= unsigned.Maximum) return signed;
        return Integer(signed.IntegerKind switch
        {
            IntegerKind.Int => IntegerKind.UnsignedInt,
            IntegerKind.Long => IntegerKind.UnsignedLong,
            IntegerKind.LongLong => IntegerKind.UnsignedLongLong,
            _ => throw new InvalidOperationException("Integral promotions must precede arithmetic conversion.")
        });
    }
    internal static BigInteger ConvertInteger(BigInteger value, IntegerType target)
    {
        if (target.IntegerKind == IntegerKind.Bool) return value.IsZero ? BigInteger.Zero : BigInteger.One;
        BigInteger modulus = BigInteger.One << target.Width;
        BigInteger bits = ((value % modulus) + modulus) % modulus;
        if (target.IsSigned && bits > target.Maximum) bits -= modulus;
        return bits;
    }
}
