using Raw = global::Managed.Forms.Cui;
using static global::Managed.Forms.Cui;

namespace Cui.Managed;

public sealed class ApplicationOptions
{
    public string Title { get; init; } = "CUI";
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
    private global::Managed.Forms.DesktopApplication? app;
    private readonly int owner = Environment.CurrentManagedThreadId;
    private bool pendingRender;
    public Ui Ui { get; }
    public Application(ApplicationOptions? options = null)
    {
        options ??= new();
        if (!Enum.IsDefined(options.Theme) || !Enum.IsDefined(options.Accent) || options.Accent == Accent.Count) throw new ArgumentOutOfRangeException(nameof(options));
        if (options.FrameArenaLimit < 4096) throw new ArgumentOutOfRangeException(nameof(options.FrameArenaLimit));
        app = new(options.Title, options.Width, options.Height, options.Hidden, options.Continuous,
            options.MinWidth, options.MinHeight, (cui_theme_mode)options.Theme, (cui_accent)options.Accent);
        try { Ui = new Ui(app.Ui, options.FrameArenaLimit); }
        catch { app.Dispose(); app = null; throw; }
    }
    private void Check()
    {
        if (owner != Environment.CurrentManagedThreadId) throw new InvalidOperationException("Use the application on its creating thread.");
        ObjectDisposedException.ThrowIf(app == null, this);
    }
    public bool NextFrame()
    {
        Check(); Render(); Ui.PrepareFrame();
        if (!app!.NextFrame()) return false;
        Ui.AdoptFrame(); pendingRender = true; return true;
    }
    public void Render()
    {
        Check(); Ui.CheckLifecycle();
        if (!pendingRender) return;
        try { Ui.EndFrame(); }
        finally { try { app!.Render(); } finally { pendingRender = false; } }
    }
    public void Run(Action<Ui> build)
    {
        Check(); ArgumentNullException.ThrowIfNull(build);
        while (NextFrame()) { try { build(Ui); } finally { Render(); } }
    }
    public void Quit() { Check(); app!.Quit(); }
    public byte[] CapturePixels(out int width, out int height) { Check(); Render(); return app!.CapturePixels(out width, out height); }
    public void Dispose()
    {
        if (owner != Environment.CurrentManagedThreadId) throw new InvalidOperationException("Dispose on the application owner thread.");
        if (app == null) return;
        Ui.CheckLifecycle();
        try { try { Render(); } finally { Ui.DestroyContext(app.Dispose); } } finally { if (Ui.IsDisposed) app = null; }
    }
}

/// <summary>Owns a translated software-rendered context and stable RGBA output.</summary>
public sealed unsafe class OffscreenSurface : IDisposable
{
    private cui_sw_renderer* renderer;
    private cui_ctx* context;
    private readonly int owner = Environment.CurrentManagedThreadId;
    private byte[] pixels;
    public int Width { get; private set; }
    public int Height { get; private set; }
    public float Scale { get; private set; }
    public Ui Ui { get; }
    public OffscreenSurface(int width, int height, float scale = 1, ThemeMode theme = ThemeMode.Dark, Accent accent = Accent.Blue, int frameArenaLimit = 16 * 1024 * 1024)
    {
        Validate(width, height, scale);
        if (!Enum.IsDefined(theme) || !Enum.IsDefined(accent) || accent == Accent.Count) throw new ArgumentOutOfRangeException(nameof(theme));
        if (frameArenaLimit < 4096) throw new ArgumentOutOfRangeException(nameof(frameArenaLimit));
        pixels = GC.AllocateUninitializedArray<byte>(checked(width * height * 4), pinned: true);
        pixels.AsSpan().Clear();
        renderer = Raw.cui_sw_create();
        if (renderer == null) throw new InvalidOperationException("CUI software renderer creation failed.");
        try
        {
            var description = new cui_ctx_desc { renderer = Raw.cui_sw_iface(renderer), theme = (cui_theme_mode)theme, accent = (cui_accent)accent };
            context = Raw.cui_create(&description);
            if (context == null) throw new InvalidOperationException("CUI context creation failed.");
            Ui = new Ui(context, frameArenaLimit);
            Raw.cui_sw_set_threads(renderer, 1);
            Width = width; Height = height; Scale = scale;
            Raw.cui_set_viewport(context, width / scale, height / scale, scale);
        }
        catch { if (context != null) Raw.cui_destroy(context); Raw.cui_sw_destroy(renderer); renderer = null; throw; }
    }
    private static void Validate(int width, int height, float scale)
    {
        if (width < 1 || height < 1 || !float.IsFinite(scale) || scale <= 0 || scale > 64 || !float.IsFinite(width / scale) || !float.IsFinite(height / scale)) throw new ArgumentOutOfRangeException(nameof(width));
        _ = checked(width * height * 4);
    }
    private void Check()
    {
        if (owner != Environment.CurrentManagedThreadId) throw new InvalidOperationException("Use the surface on its creating thread.");
        ObjectDisposedException.ThrowIf(context == null, this);
    }
    public UiFrame BeginFrame(float dt = 1f / 60) { Check(); return Ui.BeginFrame(dt); }
    public void Render()
    {
        Check(); Ui.EndFrame();
        var draw = Raw.cui_get_draw_data(context);
        if (draw->width_px == 0 || draw->height_px == 0) return;
        fixed (byte* destination = pixels)
            Raw.cui_sw_render(renderer, draw, destination, Width, Height, checked(Width * 4), cui_sw_format.CUI_SW_RGBA);
    }
    public ReadOnlySpan<byte> Pixels { get { Check(); return pixels; } }
    public void CopyPixelsTo(Span<byte> destination) { Check(); pixels.AsSpan().CopyTo(destination); }
    public void Resize(int width, int height, float scale = 1)
    {
        Check(); Validate(width, height, scale); Render();
        var replacement = GC.AllocateUninitializedArray<byte>(checked(width * height * 4), pinned: true);
        replacement.AsSpan().Clear();
        Width = width; Height = height; Scale = scale; pixels = replacement;
        Raw.cui_set_viewport(context, width / scale, height / scale, scale);
    }
    public void Dispose()
    {
        if (owner != Environment.CurrentManagedThreadId) throw new InvalidOperationException("Dispose on the surface owner thread.");
        if (context == null) return;
        Ui.CheckLifecycle();
        try { Ui.DestroyContext(DestroyNative); }
        finally { if (Ui.IsDisposed) { context = null; renderer = null; pixels = Array.Empty<byte>(); } }
    }
    private void DestroyNative() { try { Raw.cui_destroy(context); } finally { Raw.cui_sw_destroy(renderer); } }
}
