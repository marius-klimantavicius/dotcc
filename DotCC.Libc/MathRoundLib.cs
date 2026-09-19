#nullable enable

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    /// <summary>Round to integral value in the runtime's default nearest,
    /// ties-to-even mode. Preserves signed zero, infinities and NaN classification.
    /// The runtime does not expose a mutable floating-point environment or
    /// floating-point exception flags; dynamic rounding modes are unsupported.</summary>
    public static double rint(double value) => global::System.Math.Round(value, global::System.MidpointRounding.ToEven);
    /// <inheritdoc cref="rint(double)"/>
    public static float rint(float value) => global::System.MathF.Round(value, global::System.MidpointRounding.ToEven);
    /// <inheritdoc cref="rint(double)"/>
    public static float rintf(float value) => rint(value);

    /// <summary>Nonzero exactly when either operand is NaN. No ordering
    /// comparison is performed; floating-point exception flags are not modeled.</summary>
    public static int isunordered(double left, double right) => double.IsNaN(left) || double.IsNaN(right) ? 1 : 0;
    /// <inheritdoc cref="isunordered(double,double)"/>
    public static int isunordered(float left, float right) => float.IsNaN(left) || float.IsNaN(right) ? 1 : 0;
}
