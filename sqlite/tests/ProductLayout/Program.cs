internal static class Program
{
    private static void Main()
    {
        // LayoutStorageChecks runs as a module initializer before this point.
        ProductOffsetChecks.Check();
        System.Console.WriteLine("PASS actual product aggregate size, alignment, field offsets and inline-array storage");
    }
}
