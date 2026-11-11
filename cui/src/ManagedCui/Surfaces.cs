using Managed.Forms;
using Raw = global::Managed.Forms.Cui;
using static global::Managed.Forms.Cui;

namespace Cui.Managed;

public sealed class ApplicationOptions
{
    public string Title { get; init; } = "CUI";
    /// <summary>Linux desktop-file ID without .desktop; ignored on Windows. Read during window creation.</summary>
    public string? AppId { get; init; }
    public int Width { get; init; } = 800;
    public int Height { get; init; } = 600;
    public int MinWidth { get; init; }
    public int MinHeight { get; init; }
    public bool Hidden { get; init; }
    public bool Continuous { get; init; }
    public ThemeMode Theme { get; init; } = ThemeMode.Dark;
    public Accent Accent { get; init; } = Accent.Blue;
    public int FrameArenaLimit { get; init; } = 16 * 1024 * 1024;
}

/// <summary>Owns the translated desktop application and its managed UI boundary.</summary>
public sealed unsafe class Application : IDisposable
{
    private global::Managed.Forms.DesktopApplication? _app;
    private readonly int _owner = Environment.CurrentManagedThreadId;
    private bool _pendingRender;
    public Ui Ui { get; }
    public Application(ApplicationOptions? options = null)
    {
        options ??= new ApplicationOptions();
        if (!Enum.IsDefined(options.Theme) || !Enum.IsDefined(options.Accent) || options.Accent == Accent.Count) throw new ArgumentOutOfRangeException(nameof(options));
        if (options.FrameArenaLimit < 4096) throw new ArgumentOutOfRangeException(nameof(options.FrameArenaLimit));
        _app = new DesktopApplication(options.Title, options.Width, options.Height, options.Hidden, options.Continuous,
            options.MinWidth, options.MinHeight, (cui_theme_mode)options.Theme, (cui_accent)options.Accent, options.AppId);
        try { Ui = new Ui(_app.Ui, options.FrameArenaLimit); }
        catch { _app.Dispose(); _app = null; throw; }
    }
    private void Check()
    {
        if (_owner != Environment.CurrentManagedThreadId) throw new InvalidOperationException("Use the application on its creating thread.");
        ObjectDisposedException.ThrowIf(_app == null, this);
    }
    public bool NextFrame()
    {
        Check(); Render(); Ui.PrepareFrame();
        if (!_app!.NextFrame()) return false;
        Ui.AdoptFrame(); _pendingRender = true; return true;
    }
    public void Render()
    {
        Check(); Ui.CheckLifecycle();
        if (!_pendingRender) return;
        try { Ui.EndFrame(); }
        finally { try { _app!.Render(); } finally { _pendingRender = false; } }
    }
    public void Run(Action<Ui> build)
    {
        Check(); ArgumentNullException.ThrowIfNull(build);
        while (NextFrame()) { try { build(Ui); } finally { Render(); } }
    }
    public void Quit() { Check(); _app!.Quit(); }
    public byte[] CapturePixels(out int width, out int height) { Check(); Render(); return _app!.CapturePixels(out width, out height); }
    public void Dispose()
    {
        if (_owner != Environment.CurrentManagedThreadId) throw new InvalidOperationException("Dispose on the application owner thread.");
        if (_app == null) return;
        Ui.CheckLifecycle();
        try { try { Render(); } finally { Ui.DestroyContext(_app.Dispose); } } finally { if (Ui.IsDisposed) _app = null; }
    }
}

/// <summary>Owns a translated software-rendered context and stable RGBA output.</summary>
public sealed unsafe class OffscreenSurface : IDisposable
{
    private cui_sw_renderer* _renderer;
    private cui_ctx* _context;
    private readonly int _owner = Environment.CurrentManagedThreadId;
    private byte[] _pixels;
    public int Width { get; private set; }
    public int Height { get; private set; }
    public float Scale { get; private set; }
    public Ui Ui { get; }
    public OffscreenSurface(int width, int height, float scale = 1, ThemeMode theme = ThemeMode.Dark, Accent accent = Accent.Blue, int frameArenaLimit = 16 * 1024 * 1024)
    {
        Validate(width, height, scale);
        if (!Enum.IsDefined(theme) || !Enum.IsDefined(accent) || accent == Accent.Count) throw new ArgumentOutOfRangeException(nameof(theme));
        if (frameArenaLimit < 4096) throw new ArgumentOutOfRangeException(nameof(frameArenaLimit));
        _pixels = GC.AllocateUninitializedArray<byte>(checked(width * height * 4), pinned: true);
        _pixels.AsSpan().Clear();
        _renderer = Raw.cui_sw_create();
        if (_renderer == null) throw new InvalidOperationException("CUI software renderer creation failed.");
        try
        {
            var description = new cui_ctx_desc { renderer = Raw.cui_sw_iface(_renderer), theme = (cui_theme_mode)theme, accent = (cui_accent)accent };
            _context = Raw.cui_create(&description);
            if (_context == null) throw new InvalidOperationException("CUI context creation failed.");
            Ui = new Ui(_context, frameArenaLimit);
            Raw.cui_sw_set_threads(_renderer, 1);
            Width = width; Height = height; Scale = scale;
            Raw.cui_set_viewport(_context, width / scale, height / scale, scale);
        }
        catch { if (_context != null) Raw.cui_destroy(_context); Raw.cui_sw_destroy(_renderer); _renderer = null; throw; }
    }
    private static void Validate(int width, int height, float scale)
    {
        if (width < 1 || height < 1 || !float.IsFinite(scale) || scale <= 0 || scale > 64 || !float.IsFinite(width / scale) || !float.IsFinite(height / scale)) throw new ArgumentOutOfRangeException(nameof(width));
        _ = checked(width * height * 4);
    }
    private void Check()
    {
        if (_owner != Environment.CurrentManagedThreadId) throw new InvalidOperationException("Use the surface on its creating thread.");
        ObjectDisposedException.ThrowIf(_context == null, this);
    }
    public UiFrame BeginFrame(float dt = 1f / 60) { Check(); return Ui.BeginFrame(dt); }
    public void Render()
    {
        Check(); Ui.EndFrame();
        var draw = Raw.cui_get_draw_data(_context);
        if (draw->width_px == 0 || draw->height_px == 0) return;
        fixed (byte* destination = _pixels)
            Raw.cui_sw_render(_renderer, draw, destination, Width, Height, checked(Width * 4), cui_sw_format.CUI_SW_RGBA);
    }
    public ReadOnlySpan<byte> Pixels { get { Check(); return _pixels; } }
    public void CopyPixelsTo(Span<byte> destination) { Check(); _pixels.AsSpan().CopyTo(destination); }
    public void Resize(int width, int height, float scale = 1)
    {
        Check(); Validate(width, height, scale); Render();
        var replacement = GC.AllocateUninitializedArray<byte>(checked(width * height * 4), pinned: true);
        replacement.AsSpan().Clear();
        Width = width; Height = height; Scale = scale; _pixels = replacement;
        Raw.cui_set_viewport(_context, width / scale, height / scale, scale);
    }
    public void Dispose()
    {
        if (_owner != Environment.CurrentManagedThreadId) throw new InvalidOperationException("Dispose on the surface owner thread.");
        if (_context == null) return;
        Ui.CheckLifecycle();
        try { Ui.DestroyContext(DestroyNative); }
        finally { if (Ui.IsDisposed) { _context = null; _renderer = null; _pixels = Array.Empty<byte>(); } }
    }
    private void DestroyNative() { try { Raw.cui_destroy(_context); } finally { Raw.cui_sw_destroy(_renderer); } }
}
