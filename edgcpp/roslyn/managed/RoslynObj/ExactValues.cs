using System.Numerics;

namespace RoslynObj;

public enum FloatingPointClass { Finite, Infinity, NaN }

/// <summary>A target floating value in exact mathematical components, including signed zero.</summary>
public sealed record ExactFloatingPoint(
    FloatingPointClass Class, bool IsNegative, int Radix, BigInteger Significand,
    BigInteger Exponent, BigInteger? NaNPayload, bool? IsQuietNaN)
{
    public bool IsZero => Class == FloatingPointClass.Finite && Significand.IsZero;
}

public sealed record TargetProfile(
    string Abi, int AddressableUnitBits, string Endianness,
    BigInteger PointerBits, BigInteger PointerAlignment);

public sealed record BuildCapability(string Name, string? Value, bool IsRequired, bool? Enabled, SemanticKind State);

public sealed record ObjectManifest(
    int FormatMajor, int FormatMinor, string ArtifactIdentity, string Producer,
    string EdgRevision, string EdgVersion, string CaptureProfile, string Stage,
    TargetProfile Target);

/// <summary>A stable named discriminant from the format catalog; it is independent of EDG enum ordinals.</summary>
public sealed record SemanticKind
{
    internal SemanticKind(string family, int id, string name)
    {
        Family = family; Id = id; Name = name;
    }
    public string Family { get; }
    public string Name { get; }
    internal int Id { get; }
    public override string ToString() => Name;
}

public enum AvailabilityState { Available, Absent, NotApplicable, Dependent, NotRetained, Disabled, Unsupported }
public sealed record FactAvailability(string Property, AvailabilityState State, string? Reason);
