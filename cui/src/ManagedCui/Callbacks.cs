using System.Runtime.InteropServices;
using System.Text;
using Raw = global::Managed.Forms.Cui;
using static global::Managed.Forms.Cui;

namespace Cui.Managed;

public delegate int FileListHandler(string path, Span<FileEntry> entries, bool hidden);

/// <summary>A reusable callback registration rooted by its context until context destruction.</summary>
public sealed class CanvasPainter : IDisposable
{
    internal readonly Ui Owner;
    internal Action<PaintContext>? Callback;
    internal readonly long Key;
    internal CanvasPainter(Ui ui, Action<PaintContext> callback, long key) { Owner = ui; Callback = callback; Key = key; }
    public void Dispose() { Owner.CheckOwnerThread(); Callback = null; }
}

/// <summary>A callback-scoped drawing capability; copies expire when the callback returns.</summary>
public readonly struct PaintContext
{
    private readonly Ui _ui;
    private readonly long _epoch;
    public Rect Rect { get; }
    internal PaintContext(Ui ui, Rect rect, long epoch) { this._ui = ui; Rect = rect; this._epoch = epoch; }
    private void Check() => _ui.ValidatePaint(_epoch);
    public Color ColorOf(Color color) { Check(); return _ui.PaintColor(color); }
    public void DrawCircle(Vec2 center, float radius, Color color) { Check(); _ui.DrawCircle(center, radius, color); }
    public void DrawRect(Rect rect, Corners corners, Color color) { Check(); _ui.DrawRect(rect, corners, color); }
    public void DrawRectBorder(Rect rect, Corners corners, Color fill, Color border, float width) { Check(); _ui.DrawRectBorder(rect, corners, fill, border, width); }
    public void DrawRectGradient(Rect rect, Corners corners, Color top, Color bottom) { Check(); _ui.DrawRectGradient(rect, corners, top, bottom); }
    public void DrawLine(Vec2 a, Vec2 b, float thickness, Color color) { Check(); _ui.DrawLine(a, b, thickness, color); }
    public void DrawPolyline(ReadOnlySpan<Vec2> points, float thickness, Color color, bool closed = false) { Check(); _ui.DrawPolyline(points, points.Length, thickness, color, closed); }
    public void DrawPolygon(ReadOnlySpan<Vec2> points, Color color) { Check(); _ui.DrawPolygon(points, points.Length, color); }
    public void DrawArc(Vec2 center, float radius, float start, float end, float thickness, Color color) { Check(); _ui.DrawArc(center, radius, start, end, thickness, color); }
    public void DrawText(Font font, float size, Vec2 position, Color color, string text) { Check(); _ui.DrawText(font, size, position, color, text, -1); }
    public void DrawText(Font font, float size, Vec2 position, Color color, ReadOnlySpan<byte> text) { Check(); _ui.DrawText(font, size, position, color, text, -1); }
    public void DrawText(Font font, float size, Vec2 position, Color color, TextBuffer text) { Check(); _ui.DrawText(font, size, position, color, text, -1); }
    public void DrawIcon(string icon, Rect rect, float size, Color color) { Check(); _ui.DrawIcon(icon, rect, size, color); }
    public void DrawIcon(ReadOnlySpan<byte> icon, Rect rect, float size, Color color) { Check(); _ui.DrawIcon(icon, rect, size, color); }
    public void DrawIcon(TextBuffer icon, Rect rect, float size, Color color) { Check(); _ui.DrawIcon(icon, rect, size, color); }
    public void DrawImage(Texture texture, Rect rect, Rect uv, Corners corners, Color tint) { Check(); _ui.DrawImage(texture, rect, uv, corners, tint); }
    public void DrawShadow(Rect rect, Corners corners, float blur, float spread, Vec2 offset, Color color) { Check(); _ui.DrawShadow(rect, corners, blur, spread, offset, color); }
}

public sealed unsafe partial class Ui
{
    private sealed record FileRoot(Ui Ui, FileListHandler Handler, FileEntry[] Entries);
    private readonly Dictionary<FileListHandler, nint> _fileRoots = new Dictionary<FileListHandler, IntPtr>();
    private readonly Dictionary<long, CanvasPainter> _painters = new Dictionary<long, CanvasPainter>();
    private long _nextPainter;
    private long _paintEpoch;
    private nint _selfRoot;
    private void* RootFileList(FileListHandler handler)
    {
        if (_fileRoots.TryGetValue(handler, out var pointer)) return (void*)pointer;
        if (_callbackRoots.Count >= 1024) throw new InvalidOperationException("CUI callback root limit exceeded. Reuse callback registrations.");
        // Upstream default max_entries is 20000. The bridge rejects larger callbacks explicitly.
        var root = GCHandle.Alloc(new FileRoot(this, handler, new FileEntry[65536]));
        _callbackRoots.Add(root); pointer = GCHandle.ToIntPtr(root); _fileRoots.Add(handler, pointer); return (void*)pointer;
    }
    private static int FileListThunk(void* user, byte* path, cui_file_entry* output, int capacity, byte* names, ulong namesCapacity, global::Managed.Forms.Cui.CBool hidden)
    {
        FileRoot? root = null;
        try
        {
            root = (FileRoot)GCHandle.FromIntPtr((nint)user).Target!;
            root.Ui.Check();
            if (capacity < 0 || capacity > root.Entries.Length) throw new ArgumentOutOfRangeException(nameof(capacity), "File callback capacity exceeds 65536.");
            var entries = root.Entries.AsSpan(0, capacity); entries.Clear();
            int count; root.Ui._fileCallback = true;
            try { count = root.Handler(Decode(path) ?? "", entries, hidden != 0); }
            finally { root.Ui._fileCallback = false; }
            if (count == -1) return -1; // Upstream's ordinary listing failure.
            CheckLength(count, capacity, nameof(count));
            ulong offset = 0;
            for (var i = 0; i < count; i++)
            {
                var entry = entries[i]; var name = entry.Name ?? "";
                if (name.Contains('\0')) throw new ArgumentException("File names cannot contain NUL.");
                var length = Encoding.UTF8.GetByteCount(name);
                if ((ulong)length + 1 > namesCapacity - offset) return -1;
                var target = names + offset;
                Encoding.UTF8.GetBytes(name, new Span<byte>(target, length)); target[length] = 0; offset += (ulong)length + 1;
                output[i] = new cui_file_entry { name = target, dir = entry.Dir, size = entry.Size, modified = entry.Modified };
            }
            return count;
        }
        catch (Exception error) { root?.Ui.Contain(error); return -1; }
    }
    public CanvasPainter CreateCanvasPainter(Action<PaintContext> paint)
    {
        Check(); ArgumentNullException.ThrowIfNull(paint);
        if (_painters.Count >= 1024) throw new InvalidOperationException("CUI painter limit exceeded. Reuse registrations.");
        var painter = new CanvasPainter(this, paint, ++_nextPainter); _painters.Add(painter.Key, painter); return painter;
    }
    public CanvasResult Canvas(string id, Size width, Size height, CanvasPainter painter)
    {
        CheckFrame(); ArgumentNullException.ThrowIfNull(painter);
        if (painter.Owner != this) throw new ArgumentException("Painter belongs to another context.");
        ObjectDisposedException.ThrowIf(painter.Callback == null, painter);
        if (_selfRoot == 0)
        {
            var root = GCHandle.Alloc(this); _callbackRoots.Add(root); _selfRoot = GCHandle.ToIntPtr(root);
        }
        // CUI copies user bytes into its frame arena, retaining the stable root/key values.
        CanvasUser user = new CanvasUser { Context = _selfRoot, Key = painter.Key };
        return FromNative(Raw.cui_canvas(_ctx, Utf8Required(id), ToNative(width), ToNative(height), &CanvasThunk, &user, (ulong)sizeof(CanvasUser)));
    }
    private struct CanvasUser { internal nint Context; internal long Key; }
    private static void CanvasThunk(cui_ctx* context, cui_rect rect, void* user)
    {
        Ui? ui = null;
        try
        {
            var data = *(CanvasUser*)user;
            ui = (Ui)GCHandle.FromIntPtr(data.Context).Target!; ui.Check();
            if (context != ui._ctx) throw new InvalidOperationException("Canvas callback context mismatch.");
            var painter = ui._painters[data.Key];
            if (painter.Callback == null) return;
            ui._painting = true; var epoch = ++ui._paintEpoch;
            painter.Callback(new PaintContext(ui, ui.FromNative(rect), epoch));
        }
        catch (Exception error) { ui?.Contain(error); }
        finally { if (ui != null) { ui._painting = false; ui._paintEpoch++; } }
    }
    internal void ValidatePaint(long epoch)
    {
        CheckDraw(); if (_paintEpoch != epoch) throw new InvalidOperationException("PaintContext has expired.");
    }
    internal Color PaintColor(Color color) { CheckDraw(); return FromNative(Raw.cui_color_of(_ctx, ToNative(color))); }
}
