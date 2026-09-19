using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace AuditHost;
public static class HostBox
{
    private static readonly int Value = Native();
    [DllImport("host-initializer-must-not-run")] private static extern int Native();
    public static int Read() => Value;
}
public static class HostModule
{
    [DllImport("host-module-must-not-run")] private static extern int Native();
    [ModuleInitializer] public static void Initialize() => Native();
}
