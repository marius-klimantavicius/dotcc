// Compiler-only differential fixture. The C declaration has only the ulong
// parameter; overload resolution must not select this diagnostic int method.
internal static class OverloadFormal
{
    public static ulong Choose(int value) => checked((ulong)value + 10000UL);
    public static ulong Choose(ulong value) => value + 1000UL;
}
