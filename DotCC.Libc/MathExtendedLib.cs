using System;
using System.Runtime.CompilerServices;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    // The shared C runtime supports the default nearest/ties-even mode, as
    // documented for rint/llrint; C long has the LP64 width in this backend.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long lrint(double value) => llrint(value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long lrintf(float value) => llrint(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int signbit(double value) => BitConverter.DoubleToInt64Bits(value) < 0 ? 1 : 0;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int signbit(float value) => BitConverter.SingleToInt32Bits(value) < 0 ? 1 : 0;

    /// <summary>exp(x)-1 without cancellation for small arguments.</summary>
    public static double expm1(double x)
    {
        // Preserve signed zero and subnormals. Below 2^-54 the correction
        // x*x/2 cannot change the rounded result.
        if (Math.Abs(x) < 5.551115123125783e-17) return x;
        if (Math.Abs(x) >= 0.5 || double.IsNaN(x)) return Math.Exp(x) - 1;
        // Compensated summation bounds rounding while the series converges
        // rapidly on this interval (successive terms shrink by at least 4).
        double term = x, sum = x, correction = 0;
        for (int n = 2; n <= 32; n++)
        {
            term *= x / n;
            double adjusted = term - correction;
            double next = sum + adjusted;
            correction = (next - sum) - adjusted;
            if (next == sum) break;
            sum = next;
        }
        return sum;
    }

    /// <summary>log(1+x) with compensation for rounding in the addition.</summary>
    public static double log1p(double x)
    {
        if (x == 0 || double.IsPositiveInfinity(x)) return x;
        if (x == -1) return double.NegativeInfinity;
        if (x < -1 || double.IsNaN(x)) return double.NaN;
        double sum = 1 + x;
        if (sum == 1) return x;
        return Math.Log(sum) - ((sum - 1) - x) / sum;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double hypot(double x, double y) => double.Hypot(x, y);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float expm1(float x) => (float)expm1((double)x);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float expm1f(float x) => expm1(x);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float log1p(float x) => (float)log1p((double)x);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float log1pf(float x) => log1p(x);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float hypot(float x, float y) => float.Hypot(x, y);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float hypotf(float x, float y) => hypot(x, y);
}
