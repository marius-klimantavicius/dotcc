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
    private readonly Ui ui;
    private readonly long epoch;
    public Rect Rect { get; }
    internal PaintContext(Ui ui, Rect rect, long epoch) { this.ui = ui; Rect = rect; this.epoch = epoch; }
    private void Check() => ui.ValidatePaint(epoch);
    public Color ColorOf(Color color) { Check(); return ui.PaintColor(color); }
    public void DrawCircle(Vec2 center, float radius, Color color) { Check(); ui.DrawCircle(center, radius, color); }
    public void DrawRect(Rect rect, Corners corners, Color color) { Check(); ui.DrawRect(rect, corners, color); }
    public void DrawRectBorder(Rect rect, Corners corners, Color fill, Color border, float width) { Check(); ui.DrawRectBorder(rect, corners, fill, border, width); }
    public void DrawRectGradient(Rect rect, Corners corners, Color top, Color bottom) { Check(); ui.DrawRectGradient(rect, corners, top, bottom); }
    public void DrawLine(Vec2 a, Vec2 b, float thickness, Color color) { Check(); ui.DrawLine(a, b, thickness, color); }
    public void DrawPolyline(ReadOnlySpan<Vec2> points, float thickness, Color color, bool closed = false) { Check(); ui.DrawPolyline(points, points.Length, thickness, color, closed); }
    public void DrawPolygon(ReadOnlySpan<Vec2> points, Color color) { Check(); ui.DrawPolygon(points, points.Length, color); }
    public void DrawArc(Vec2 center, float radius, float start, float end, float thickness, Color color) { Check(); ui.DrawArc(center, radius, start, end, thickness, color); }
    public void DrawText(Font font, float size, Vec2 position, Color color, string text) { Check(); ui.DrawText(font, size, position, color, text, -1); }
    public void DrawIcon(string icon, Rect rect, float size, Color color) { Check(); ui.DrawIcon(icon, rect, size, color); }
    public void DrawImage(Texture texture, Rect rect, Rect uv, Corners corners, Color tint) { Check(); ui.DrawImage(texture, rect, uv, corners, tint); }
    public void DrawShadow(Rect rect, Corners corners, float blur, float spread, Vec2 offset, Color color) { Check(); ui.DrawShadow(rect, corners, blur, spread, offset, color); }
}

public sealed unsafe partial class Ui
{
    private sealed record FileRoot(Ui Ui, FileListHandler Handler, FileEntry[] Entries);
    private readonly Dictionary<FileListHandler, nint> fileRoots = new();
    private readonly Dictionary<long, CanvasPainter> painters = new();
    private long nextPainter;
    private long paintEpoch;
    private nint selfRoot;
    private void* RootFileList(FileListHandler handler)
    {
        if (fileRoots.TryGetValue(handler, out var pointer)) return (void*)pointer;
        if (callbackRoots.Count >= 1024) throw new InvalidOperationException("CUI callback root limit exceeded. Reuse callback registrations.");
        // Upstream default max_entries is 20000. The bridge rejects larger callbacks explicitly.
        var root = GCHandle.Alloc(new FileRoot(this, handler, new FileEntry[65536]));
        callbackRoots.Add(root); pointer = GCHandle.ToIntPtr(root); fileRoots.Add(handler, pointer); return (void*)pointer;
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
            int count; root.Ui.fileCallback = true;
            try { count = root.Handler(Decode(path) ?? "", entries, hidden != 0); }
            finally { root.Ui.fileCallback = false; }
            if (count == -1) return -1; // Upstream's ordinary listing failure.
            CheckLength(count, capacity, nameof(count));
            ulong offset = 0;
            for (int i = 0; i < count; i++)
            {
                var entry = entries[i]; string name = entry.Name ?? "";
                if (name.Contains('\0')) throw new ArgumentException("File names cannot contain NUL.");
                int length = Encoding.UTF8.GetByteCount(name);
                if ((ulong)length + 1 > namesCapacity - offset) return -1;
                byte* target = names + offset;
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
        if (painters.Count >= 1024) throw new InvalidOperationException("CUI painter limit exceeded. Reuse registrations.");
        var painter = new CanvasPainter(this, paint, ++nextPainter); painters.Add(painter.Key, painter); return painter;
    }
    public CanvasResult Canvas(string id, Size width, Size height, CanvasPainter painter)
    {
        CheckFrame(); ArgumentNullException.ThrowIfNull(painter);
        if (painter.Owner != this) throw new ArgumentException("Painter belongs to another context.");
        ObjectDisposedException.ThrowIf(painter.Callback == null, painter);
        if (selfRoot == 0)
        {
            var root = GCHandle.Alloc(this); callbackRoots.Add(root); selfRoot = GCHandle.ToIntPtr(root);
        }
        // CUI copies user bytes into its frame arena, retaining the stable root/key values.
        CanvasUser user = new() { Context = selfRoot, Key = painter.Key };
        return FromNative(Raw.cui_canvas(ctx, Utf8Required(id), ToNative(width), ToNative(height), &CanvasThunk, &user, (ulong)sizeof(CanvasUser)));
    }
    private struct CanvasUser { internal nint Context; internal long Key; }
    private static void CanvasThunk(cui_ctx* context, cui_rect rect, void* user)
    {
        Ui? ui = null;
        try
        {
            var data = *(CanvasUser*)user;
            ui = (Ui)GCHandle.FromIntPtr(data.Context).Target!; ui.Check();
            if (context != ui.ctx) throw new InvalidOperationException("Canvas callback context mismatch.");
            var painter = ui.painters[data.Key];
            if (painter.Callback == null) return;
            ui.painting = true; long epoch = ++ui.paintEpoch;
            painter.Callback(new PaintContext(ui, ui.FromNative(rect), epoch));
        }
        catch (Exception error) { ui?.Contain(error); }
        finally { if (ui != null) { ui.painting = false; ui.paintEpoch++; } }
    }
    internal void ValidatePaint(long epoch)
    {
        CheckDraw(); if (paintEpoch != epoch) throw new InvalidOperationException("PaintContext has expired.");
    }
    internal Color PaintColor(Color color) { CheckDraw(); return FromNative(Raw.cui_color_of(ctx, ToNative(color))); }
}
