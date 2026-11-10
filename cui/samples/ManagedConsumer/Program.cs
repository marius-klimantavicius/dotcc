using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Managed.Cui;
using static Managed.Cui.Cui;
using Gallery = Managed.Cui.Gallery.Gallery;

internal static unsafe class Program
{
    [STAThread]
    private static int Main(string[] arguments)
    {
        try
        {
            bool smoke = false, gallerySmoke = false, listPages = false, help = false;
            string? requestedPage = null, capture = null;
            for (int i = 0; i < arguments.Length; i++)
            {
                switch (arguments[i])
                {
                    case "--smoke": smoke = true; break;
                    case "--gallery-smoke": gallerySmoke = true; break;
                    case "--list-pages": listPages = true; break;
                    case "--help": case "-h": help = true; break;
                    case "--page": requestedPage = Value(arguments, ref i); break;
                    case "--capture": capture = Value(arguments, ref i); break;
                    default: throw new ArgumentException($"Unknown argument: {arguments[i]}. Use --help.");
                }
            }
            if (help)
            {
                Console.WriteLine("CUI Gallery: all upstream component pages, themes and interactive controls.\n" +
                    "  --page INDEX|NAME  Open a registered page (zero-based index or full name).\n" +
                    "  --list-pages       Print page indices and names without opening a window.\n" +
                    "  --gallery-smoke    Render every page, four frames each, then check cleanup.\n" +
                    "  --smoke            Run the original four-widget pixel/lifetime regression.\n" +
                    "  --capture PATH     Save raw RGBA (last smoke page, or interactive frame four).");
                return 0;
            }
            if (smoke && (gallerySmoke || requestedPage is not null || listPages))
                throw new ArgumentException("--smoke cannot be combined with gallery selection options.");
            if (smoke) return LegacySmoke.Run(arguments);
            int total = Gallery.gallery_page_total();
            if (total <= 0) throw new InvalidOperationException("The upstream gallery has no registered pages.");
            if (listPages)
            {
                for (int page = 0; page < total; page++) Console.WriteLine($"{page}\t{PageName(page)}");
                return 0;
            }
            int firstPage = ResolvePage(requestedPage, total);
            return RunGallery(firstPage, total, gallerySmoke, capture);
        }
        catch (Exception error) { Console.Error.WriteLine(error.Message); return 1; }
    }

    private static string Value(string[] arguments, ref int index)
    {
        string option = arguments[index];
        if (++index >= arguments.Length || arguments[index].StartsWith("--", StringComparison.Ordinal))
            throw new ArgumentException($"{option} requires a value.");
        return arguments[index];
    }

    private static string PageName(int page) => Marshal.PtrToStringUTF8((nint)Gallery.gallery_page_name(page))
        ?? throw new InvalidOperationException($"Gallery page {page} has no name.");

    private static int ResolvePage(string? requested, int total)
    {
        if (requested is null) return 0;
        if (int.TryParse(requested, NumberStyles.Integer, CultureInfo.InvariantCulture, out int page))
        {
            if (page >= 0 && page < total) return page;
            throw new ArgumentException($"Page index must be between 0 and {total - 1}.");
        }
        for (page = 0; page < total; page++)
            if (PageName(page).Equals(requested, StringComparison.OrdinalIgnoreCase)) return page;
        throw new ArgumentException($"Unknown gallery page '{requested}'. Use --list-pages.");
    }

    private static int RunGallery(int firstPage, int total, bool smoke, string? capture)
    {
        using var app = new DesktopApplication("CUI Gallery", 1240, 820,
            hidden: smoke, continuous: smoke, minWidth: 720, minHeight: 480,
            theme: cui_theme_mode.CUI_THEME_LIGHT, accent: cui_accent.CUI_ACCENT_NEUTRAL);
        Gallery.gallery state = default;
        Gallery.gallery_init(&state);
        state.page = firstPage;
        int frames = 0, visited = 0, pageFrames = 0;
        while (app.NextFrame())
        {
            Gallery.gallery_frame(app.Ui, &state);
            frames++;
            pageFrames++;
            if (smoke && pageFrames == 4)
            {
                byte[] pixels = app.CapturePixels(out int width, out int height);
                var draw = cui_get_draw_data(app.Ui);
                int colored = DifferingMainPixels(pixels, width, height);
                if (width != 1240 || height != 820 || colored < 1000 || draw->vertex_count <= 0 || draw->command_count <= 0)
                    throw new InvalidOperationException($"Gallery page {state.page} did not render: {width}x{height}, {colored} differing main-content pixels.");
                Console.WriteLine($"CUI gallery page {state.page}\t{PageName(state.page)}\t{width}x{height}, {draw->vertex_count} vertices, {colored} differing main-content pixels, SHA256 {Convert.ToHexString(SHA256.HashData(pixels))}");
                if (++visited == total)
                {
                    if (capture is not null) File.WriteAllBytes(capture, pixels);
                    app.Quit();
                }
                else
                {
                    state.page = (firstPage + visited) % total;
                    fixed (byte* main = "main"u8) cui_scroll_to(app.Ui, main, 0);
                    pageFrames = 0;
                }
            }
            else if (!smoke && capture is not null && frames == 4)
                File.WriteAllBytes(capture, app.CapturePixels(out _, out _));
        }
        if (smoke)
        {
            if (visited != total) throw new InvalidOperationException($"Gallery closed after {visited}/{total} pages.");
            app.Dispose();
            app.Dispose();
            bool rejected = false;
            try { app.NextFrame(); }
            catch (ObjectDisposedException) { rejected = true; }
            if (!rejected) throw new InvalidOperationException("A disposed gallery application was accepted.");
            using var retry = new DesktopApplication("CUI gallery reinitialization", hidden: true, continuous: true);
            if (!retry.NextFrame()) throw new InvalidOperationException("Gallery cleanup did not permit reinitialization.");
            retry.Quit();
            Console.WriteLine($"CUI gallery: {visited} pages, {frames} frames; disposal and reinitialization passed.");
        }
        return 0;
    }

    private static int DifferingMainPixels(byte[] pixels, int width, int height)
    {
        int count = 0;
        for (int y = 0; y < height; y++)
            for (int x = 261; x < width; x++)
                if (!pixels.AsSpan((y * width + x) * 4, 3).SequenceEqual(pixels.AsSpan((y * width + 260) * 4, 3))) count++;
        return count;
    }
}
