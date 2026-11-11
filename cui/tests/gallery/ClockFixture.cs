using static Managed.Cui.Cui;

internal static unsafe class GalleryClockFixture
{
    internal static int ThemeIndex => ReadIndex("CUI_GALLERY_TEST_THEME", 2);
    internal static int AccentIndex => ReadIndex("CUI_GALLERY_TEST_ACCENT", 8);
    private static int ReadIndex(string name, int count)
    {
        string? value = Environment.GetEnvironmentVariable(name);
        if (value is null) return 0;
        if (!int.TryParse(value, out int index) || (uint)index >= count) throw new ArgumentException("Invalid gallery fixture: " + name);
        return index;
    }
    internal static void Validate()
    {
        string? expected = Environment.GetEnvironmentVariable("CUI_GALLERY_EXPECTED_EPOCH");
        if (expected is null) return;
        long epoch = long.Parse(expected, System.Globalization.CultureInfo.InvariantCulture);
        long observed = Managed.Cui.Cui.Libc.time(null);
        if (observed != epoch) throw new InvalidOperationException($"Wall-clock fixture did not reach translated CUI: expected {epoch}, observed {observed}");
        Console.WriteLine($"CLOCK_FIXTURE\t{observed}\t{TimeZoneInfo.Local.Id}");
    }
}
