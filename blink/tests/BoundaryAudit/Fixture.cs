using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
namespace Managed.Emulation;
public static unsafe class BlinkCore
{
    static BlinkCore() => throw new InvalidOperationException("Audit must not execute initializers.");
    public static int Direct() => Libc.Native();
    public static int Indirect(delegate*<int> target) => target();
    [DllImport("root-import-must-not-run")] public static extern int RootImport();
    public static string Virtual(object value) => value.ToString()!;
    public static void RecursiveGeneric<T>() => RecursiveGeneric<List<T>>();
    public static int HostRead() => AuditHost.HostBox.Read();
    public static Task<int> AsyncRead() => AsyncPart();
    // The class cannot be unsafe across await. Helpers below are ordinary
    // methods called directly from roots; their state machines need expansion.
    private static Task<int> AsyncPart() => AsyncFixture.Read();
    public static IEnumerable<int> Iterate() => IteratorFixture.Read();
    public static class Libc
    {
        [DllImport("audit-must-never-load", EntryPoint="forbidden")]
        public static extern int Native();
    }
}
public static class ModuleStart
{
    [DllImport("module-initializer-must-not-run")] private static extern int Native();
    [ModuleInitializer] public static void Initialize() => Native();
}
public static class AsyncFixture
{
    public static async Task<int> Read() { await Task.Yield(); return AsyncBoundary.Native(); }
}
public static class IteratorFixture
{
    public static IEnumerable<int> Read() { yield return IteratorBoundary.Native(); }
}
public static class AsyncBoundary
{
    [DllImport("async-body-must-not-run")] public static extern int Native();
}
public static class IteratorBoundary
{
    [DllImport("iterator-body-must-not-run")] public static extern int Native();
}
