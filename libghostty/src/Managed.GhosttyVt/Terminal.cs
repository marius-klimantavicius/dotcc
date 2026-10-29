using System.Runtime.InteropServices;
using System.Text;
using Core = Managed.GhosttyVt.Native.GhosttyVt;

namespace Managed.GhosttyVt;

public readonly record struct TerminalPosition(ushort Columns, ushort Rows, ushort X, ushort Y);

[Flags]
public enum TerminalTextStyle
{
    None = 0,
    Bold = Core.DOTCC_STYLE_BOLD,
    Italic = Core.DOTCC_STYLE_ITALIC,
    Faint = Core.DOTCC_STYLE_FAINT,
    Blink = Core.DOTCC_STYLE_BLINK,
    Inverse = Core.DOTCC_STYLE_INVERSE,
    Invisible = Core.DOTCC_STYLE_INVISIBLE,
    Strikethrough = Core.DOTCC_STYLE_STRIKETHROUGH,
    Overline = Core.DOTCC_STYLE_OVERLINE
}

public readonly record struct TerminalCell(uint Codepoint, TerminalTextStyle Style,
    int Underline, byte? ForegroundPalette, uint? ForegroundRgb);

/// <summary>Owns a translated libghostty-vt terminal. Entry is serialized per instance.
/// Input bytes are borrowed only for Write; observations are copied before returning.</summary>
public sealed unsafe class Terminal : IDisposable
{
    private readonly object gate = new();
    private readonly TerminalHandle handle;

    public Terminal(ushort columns = 80, ushort rows = 24)
    {
        ArgumentOutOfRangeException.ThrowIfZero(columns);
        ArgumentOutOfRangeException.ThrowIfZero(rows);
        void* value = null;
        Check(Core.dotcc_ghostty_new(&value, columns, rows));
        if (value == null) throw new InvalidOperationException("libghostty-vt returned an empty terminal.");
        handle = new TerminalHandle(value);
    }

    private void* Value
    {
        get
        {
            ObjectDisposedException.ThrowIf(handle.IsClosed, this);
            return (void*)handle.DangerousGetHandle();
        }
    }

    public void Write(ReadOnlySpan<byte> utf8)
    {
        lock (gate)
        {
            var value = Value;
            fixed (byte* bytes = utf8) Core.dotcc_ghostty_write(value, bytes, (ulong)utf8.Length);
        }
    }

    public void Write(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        Write(Encoding.UTF8.GetBytes(text));
    }

    public TerminalPosition Position
    {
        get
        {
            lock (gate)
            {
                ushort columns, rows, x, y;
                Check(Core.dotcc_ghostty_position(Value, &columns, &rows, &x, &y));
                return new(columns, rows, x, y);
            }
        }
    }

    public int ActiveScreen
    {
        get
        {
            lock (gate)
            {
                int screen;
                Check(Core.dotcc_ghostty_screen(Value, &screen));
                return screen;
            }
        }
    }

    /// <summary>Queries an ANSI mode or, by default, a DEC private mode.</summary>
    public bool GetMode(ushort number, bool ansi = false)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(number, (ushort)32767);
        lock (gate)
        {
            int enabled;
            Check(Core.dotcc_ghostty_mode(Value, number, ansi ? 1 : 0, &enabled));
            return enabled != 0;
        }
    }

    /// <summary>Copies the active cell's primary codepoint and text styling.</summary>
    public TerminalCell GetCell(ushort x, ushort y)
    {
        lock (gate)
        {
            uint codepoint, rgb;
            int flags, underline, palette;
            Check(Core.dotcc_ghostty_cell(Value, x, y, &codepoint, &flags, &underline, &palette, &rgb));
            return new(codepoint, (TerminalTextStyle)flags, underline,
                palette < 0 ? null : checked((byte)palette), rgb == uint.MaxValue ? null : rgb);
        }
    }

    /// <summary>Copies the current screen as trimmed plain UTF-8 text, unwrapping soft lines.</summary>
    public string Snapshot()
    {
        lock (gate)
        {
            var value = Value;
            ulong required = 0;
            var status = Core.dotcc_ghostty_snapshot(value, null, 0, &required);
            if (status != Core.dotcc_ghostty_out_of_space()) Check(status);
            if (required == 0) return string.Empty;
            var bytes = new byte[checked((int)required)];
            fixed (byte* destination = bytes)
                Check(Core.dotcc_ghostty_snapshot(value, destination, (ulong)bytes.Length, &required));
            return Encoding.UTF8.GetString(bytes.AsSpan(0, checked((int)required)));
        }
    }

    public void Resize(ushort columns, ushort rows)
    {
        ArgumentOutOfRangeException.ThrowIfZero(columns);
        ArgumentOutOfRangeException.ThrowIfZero(rows);
        lock (gate) Check(Core.dotcc_ghostty_resize(Value, columns, rows));
    }

    public void Reset()
    {
        lock (gate) Core.dotcc_ghostty_reset(Value);
    }

    public void Dispose()
    {
        lock (gate) handle.Dispose();
    }

    private static void Check(int status)
    {
        if (status != 0) throw new InvalidOperationException($"libghostty-vt returned status {status}.");
    }

    private sealed class TerminalHandle : SafeHandle
    {
        internal TerminalHandle(void* value) : base(IntPtr.Zero, ownsHandle: true) => SetHandle((nint)value);
        public override bool IsInvalid => handle == IntPtr.Zero;
        protected override bool ReleaseHandle()
        {
            Core.dotcc_ghostty_free((void*)handle);
            return true;
        }
    }
}
