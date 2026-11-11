namespace Cui.Managed;

[Flags]
public enum InputFlags { None = 0, Changed = 1, Submitted = 2, Focused = 4, Blurred = 8 }
[Flags]
public enum Mod { None = 0, Shift = 1, Ctrl = 2, Alt = 4, Super = 8, Primary = Ctrl }
[Flags]
public enum ComposerActions { None = 0, Send = 1, Stop = 2, Attach = 4, Remove = 8, Model = 16 }
[Flags]
public enum ResponseAction { None = 0, Copy = 1, Regenerate = 2, Up = 4, Down = 8, Share = 16 }
[Flags]
public enum PlayerActions { None = 0, Toggle = 1, Previous = 2, Next = 4, Seek = 8 }
public enum TourAction { None = 0, Next = 1, Back = 2, Done = 3, Skipped = 4 }

public partial struct Vec2
{
    public Vec2(float x, float y) { X = x; Y = y; }
}
public partial struct Rect
{
    public Rect(float x, float y, float width, float height) { X = x; Y = y; W = width; H = height; }
    public float Width { readonly get => W; set => W = value; }
    public float Height { readonly get => H; set => H = value; }
}
public partial struct Corners
{
    public Corners(float radius) { Tl = Tr = Br = Bl = radius; }
    public Corners(float topLeft, float topRight, float bottomRight, float bottomLeft) { Tl = topLeft; Tr = topRight; Br = bottomRight; Bl = bottomLeft; }
}
public partial struct Size
{
    public Size(SizeKind kind, float value = 0)
    {
        if (!float.IsFinite(value) || value < 0 || (uint)kind > 3) throw new ArgumentOutOfRangeException(nameof(value));
        Kind = kind; Value = value;
    }
    public static Size Fit => default;
    public static Size Fill => new Size(SizeKind.Grow, 1);
    public static Size Grow(float weight = 1) => new Size(SizeKind.Grow, weight);
    public static Size Px(float points) => new Size(SizeKind.Fixed, points);
    public static Size Percent(float percent) => new Size(SizeKind.Percent, percent);
}
public partial struct Color
{
    public Color(byte r, byte g, byte b, byte a = 255) { R = r; G = g; B = b; A = a; }
    public static Color Rgb(uint rgb) => new Color((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
    public static Color Rgba(uint rgba) => new Color((byte)(rgba >> 24), (byte)(rgba >> 16), (byte)(rgba >> 8), (byte)rgba);
    public static Color Transparent => default;
    public static Color Semantic(Token token)
    {
        if ((uint)token >= (uint)Token.Count) throw new ArgumentOutOfRangeException(nameof(token));
        return new Color((byte)token, 0xC5, 0x1D, 0);
    }
    public static Color Background => Semantic(Token.Background);
    public static Color Surface => Semantic(Token.Surface);
    public static Color SurfaceRaised => Semantic(Token.SurfaceRaised);
    public static Color SurfaceMuted => Semantic(Token.SurfaceMuted);
    public static Color Foreground => Semantic(Token.Foreground);
    public static Color TextSecondary => Semantic(Token.TextSecondary);
    public static Color TextMuted => Semantic(Token.TextMuted);
    public static Color Border => Semantic(Token.Border);
    public static Color BorderSubtle => Semantic(Token.BorderSubtle);
    public static Color BorderStrong => Semantic(Token.BorderStrong);
    public static Color Accent => Semantic(Token.Accent);
    public static Color AccentStrong => Semantic(Token.AccentStrong);
    public static Color AccentSubtle => Semantic(Token.AccentSubtle);
    public static Color AccentForeground => Semantic(Token.AccentForeground);
    public static Color Success => Semantic(Token.Success);
    public static Color Warning => Semantic(Token.Warning);
    public static Color Danger => Semantic(Token.Danger);
    public static Color Info => Semantic(Token.Info);
    public static Color Hover => Semantic(Token.Hover);
    public static Color Scrim => Semantic(Token.Scrim);
    public static Color Series(int index)
    {
        if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
        return Semantic(Token.Series1 + index % 6);
    }
}
public partial struct Date
{
    public Date(int year, int month, int day) { Year = year; Month = month; Day = day; }
    public Date(DateOnly date) : this(date.Year, date.Month, date.Day) { }
    public readonly DateOnly ToDateOnly() => new DateOnly(Year, Month, Day);
}
public partial struct DateRange
{
    public DateRange(Date start, Date end) { Start = start; End = end; }
}
public partial struct KeyCombo
{
    public KeyCombo(Mod mods, Key key) { Mods = (int)mods; Key = key; }
}
public static class ManagedCuiInfo
{
    public const string UpstreamVersion = "1.4.0";
    public const int FeatureFlagEnvironments = 8;
    public const int ReorderCapacity = 256;
    public const int Changed = 1, Submitted = 2, Focused = 4, Blurred = 8;
}
