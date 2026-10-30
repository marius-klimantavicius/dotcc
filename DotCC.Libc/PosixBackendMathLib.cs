#nullable enable
using System;
using System.Runtime.InteropServices;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    // These are scalar C math services, not PostgreSQL numeric/SQL algorithms.
    // lgamma_r avoids changing libm's process-global signgam in the host.
    public static double lgamma(double value)
    {
        if (!OperatingSystem.IsLinux()) { errno = ENOTSUP; return double.NaN; }
        try
        {
            int sign;
            double result = BackendNativeLgamma(value, &sign);
            int error = Marshal.GetLastPInvokeError();
            if (error != 0) errno = error;
            return result;
        }
        catch (EntryPointNotFoundException) { errno = ENOSYS; return double.NaN; }
        catch (DllNotFoundException) { errno = ENOSYS; return double.NaN; }
    }

    public static double tgamma(double value)
    {
        if (!OperatingSystem.IsLinux()) { errno = ENOTSUP; return double.NaN; }
        try
        {
            double result = BackendNativeTgamma(value);
            int error = Marshal.GetLastPInvokeError();
            if (error != 0) errno = error;
            return result;
        }
        catch (EntryPointNotFoundException) { errno = ENOSYS; return double.NaN; }
        catch (DllNotFoundException) { errno = ENOSYS; return double.NaN; }
    }

    [DllImport("libm.so.6", EntryPoint = "lgamma_r", SetLastError = true)]
    private static extern double BackendNativeLgamma(double value, int* sign);
    [DllImport("libm.so.6", EntryPoint = "tgamma", SetLastError = true)]
    private static extern double BackendNativeTgamma(double value);
}
