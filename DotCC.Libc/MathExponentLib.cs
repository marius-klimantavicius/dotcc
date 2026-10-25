using System;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    /// <summary>Unbiased binary exponent, including subnormals. Zero/NaN use
    /// FP_ILOGB0/FP_ILOGBNAN (INT_MIN); infinity uses INT_MAX. These exceptional
    /// integer results set EDOM. Floating exception flags are not modeled.</summary>
    public static int ilogb(double x)
    {
        if (x == 0 || double.IsNaN(x)) { errno = EDOM; return int.MinValue; }
        if (double.IsInfinity(x)) { errno = EDOM; return int.MaxValue; }
        return Math.ILogB(x);
    }

    /// <summary>Binary exponent as a double. Like the Linux libc oracle,
    /// logb(0) returns -infinity without changing errno. FE_DIVBYZERO is not modeled.</summary>
    public static double logb(double x)
    {
        if (double.IsNaN(x)) return x;
        if (double.IsInfinity(x)) return double.PositiveInfinity;
        if (x == 0) return double.NegativeInfinity;
        return Math.ILogB(x);
    }

    /// <summary>IEEE remainder x-n*y, with n the nearest integer quotient,
    /// ties to even. Finite x and infinite y return x; the sign of exact zero
    /// follows x. Infinite x or zero y set EDOM unless an operand is NaN.</summary>
    public static double remainder(double x, double y)
    {
        if (double.IsNaN(x) || double.IsNaN(y)) return x + y;
        if (double.IsInfinity(x) || y == 0) { errno = EDOM; return double.NaN; }
        return Math.IEEERemainder(x, y);
    }
}
