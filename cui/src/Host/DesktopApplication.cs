#nullable enable
using System;
using System.Text;
using static Managed.Forms.Cui;

namespace Managed.Forms;

/// <summary>Owns one translated CUI GLFW application on its creating thread.</summary>
/// <remarks>Dispose on the owner thread. The UI pointer is borrowed until disposal.
/// CUI permits one desktop application at a time; offscreen contexts are separate.</remarks>
public sealed unsafe class DesktopApplication : IDisposable
{
    private cui_app* app;
    private readonly int owner = Environment.CurrentManagedThreadId;

    public DesktopApplication(string title = "CUI", int width = 800, int height = 600,
        bool hidden = false, bool continuous = false, int minWidth = 0, int minHeight = 0,
        cui_theme_mode theme = cui_theme_mode.CUI_THEME_DARK,
        cui_accent accent = cui_accent.CUI_ACCENT_BLUE, string? appId = null)
    {
        ArgumentNullException.ThrowIfNull(title);
        if (title.Contains('\0')) throw new ArgumentException("A window title cannot contain NUL.", nameof(title));
        if (appId?.Contains('\0') == true) throw new ArgumentException("An application ID cannot contain NUL.", nameof(appId));
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        if (minWidth < 0) throw new ArgumentOutOfRangeException(nameof(minWidth));
        if (minHeight < 0) throw new ArgumentOutOfRangeException(nameof(minHeight));
        byte[] utf8 = Encoding.UTF8.GetBytes(title + '\0');
        byte[]? idUtf8 = string.IsNullOrEmpty(appId) ? null : Encoding.UTF8.GetBytes(appId + '\0');
        fixed (byte* text = utf8)
        fixed (byte* id = idUtf8)
        {
            var description = new cui_app_desc
            {
                title = text, app_id = id, width = width, height = height,
                min_width = minWidth, min_height = minHeight,
                hidden = hidden, continuous = continuous,
                renderer = cui_renderer_kind.CUI_RENDERER_OPENGL,
                theme = theme, accent = accent
            };
            // CUI reads app_id only during platform initialization. The GLFW
            // string-hint bridge decodes/copies it before this pin ends.
            app = cui_app_create(&description);
        }
        if (app == null)
            throw new InvalidOperationException("CUI could not create an OpenGL window. Check display/GPU availability and whether another CUI desktop app is active.");
    }

    private void Check()
    {
        if (Environment.CurrentManagedThreadId != owner)
            throw new InvalidOperationException("Use the CUI application on its creating thread.");
        ObjectDisposedException.ThrowIf(app == null, this);
    }

    public cui_ctx* Ui { get { Check(); return cui_app_ui(app); } }
    public bool NextFrame() { Check(); return cui_app_frame(app) != 0; }
    public void Quit() { Check(); cui_app_quit(app); }

    /// <summary>Finishes layout and presents the current frame on the owner thread.</summary>
    public void Render() { Check(); cui__app_render(app); }

    /// <summary>Finishes the current frame and reads actual OpenGL RGBA pixels.</summary>
    public byte[] CapturePixels(out int width, out int height)
    {
        Check();
        Render();
        var draw = cui_get_draw_data(cui_app_ui(app));
        width = draw->width_px;
        height = draw->height_px;
        if (width <= 0 || height <= 0) throw new InvalidOperationException("Build a frame before capturing pixels.");
        byte[] pixels = new byte[checked(width * height * 4)];
        fixed (byte* data = pixels)
            if (cui_app_screenshot(app, data, width, height) == 0)
                throw new InvalidOperationException("CUI could not read the window framebuffer.");
        return pixels;
    }

    public void Dispose()
    {
        if (Environment.CurrentManagedThreadId != owner)
            throw new InvalidOperationException("Dispose the CUI application on its creating thread.");
        if (app == null) return;
        var released = app;
        app = null;
        try { GlfwHost.ThrowCallbackError(); }
        finally { cui_app_destroy(released); }
    }
}
