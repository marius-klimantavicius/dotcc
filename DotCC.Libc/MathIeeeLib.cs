#nullable enable
using System;
using System.Runtime.InteropServices;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    public static double copysign(double value, double sign) => Math.CopySign(value, sign);

    /// <summary>Adjacent binary64 value toward target, including target's zero
    /// sign. Range errors follow the C errno contract; FP flags/fenv are not modeled.</summary>
    public static double nextafter(double value, double target)
    {
        if (double.IsNaN(value) || double.IsNaN(target)) return value + target;
        if (value == target) return target;
        double result = target > value ? Math.BitIncrement(value) : Math.BitDecrement(value);
        if (double.IsFinite(value) && (double.IsInfinity(result) || double.IsSubnormal(result)
            || result == 0)) errno = ERANGE;
        return result;
    }

    /// <summary>One correctly rounded fused operation under nearest/ties-even.
    /// Does not lower to a separately rounded multiplication and addition.</summary>
    public static double fma(double x, double y, double z)
    {
        double result = Math.FusedMultiplyAdd(x, y, z);
        if (double.IsNaN(result) && !double.IsNaN(x) && !double.IsNaN(y) && !double.IsNaN(z)) errno = EDOM;
        else if (double.IsInfinity(result) && double.IsFinite(x) && double.IsFinite(y) && double.IsFinite(z)) errno = ERANGE;
        return result;
    }

    public static double exp2(double value)
    {
        double result = double.Exp2(value);
        if (double.IsFinite(value) && (double.IsInfinity(result) || result == 0)) errno = ERANGE;
        return result;
    }

    // The BCL has no erf/erfc primitive. Use the host math service, as for other
    // narrowly bound POSIX services, rather than replacing CPython's algorithms
    // with a low-accuracy approximation. These signatures contain no wchar ABI.
    public static double erf(double value)
    {
        if (!OperatingSystem.IsLinux()) { errno = ENOTSUP; return double.NaN; }
        double result = PosixErf(value);
        int error = Marshal.GetLastPInvokeError();
        if (error != 0) errno = error;
        return result;
    }

    public static double erfc(double value)
    {
        if (!OperatingSystem.IsLinux()) { errno = ENOTSUP; return double.NaN; }
        double result = PosixErfc(value);
        int error = Marshal.GetLastPInvokeError();
        if (error != 0) errno = error;
        return result;
    }

    [DllImport("libm.so.6", EntryPoint = "erf", SetLastError = true)]
    private static extern double PosixErf(double value);
    [DllImport("libm.so.6", EntryPoint = "erfc", SetLastError = true)]
    private static extern double PosixErfc(double value);
}
