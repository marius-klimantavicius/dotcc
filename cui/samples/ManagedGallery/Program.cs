using System.Globalization;
using System.Security.Cryptography;
using Cui.Managed;

namespace Cui.ManagedGallery;

internal static class Program
{
    [STAThread]
    private static int Main(string[] arguments)
    {
        try
        {
            bool gallerySmoke = false, listPages = false, help = false, showFps = true;
            string? requestedPage = null, capture = null;
            for (int i = 0; i < arguments.Length; i++)
            {
                switch (arguments[i])
                {
                    case "--gallery-smoke": gallerySmoke = true; break;
                    case "--no-fps": showFps = false; break;
                    case "--list-pages": listPages = true; break;
                    case "--help": case "-h": help = true; break;
                    case "--page": requestedPage = Value(arguments, ref i); break;
                    case "--capture": capture = Value(arguments, ref i); break;
                    default: throw new ArgumentException($"Unknown argument: {arguments[i]}. Use --help.");
                }
            }
            if (help)
            {
                Console.WriteLine("CUI Gallery: authored safe C# component pages, themes and interactive controls.\n" +
                    "  --page INDEX|NAME  Open a registered page (zero-based index or full name).\n" +
                    "  --list-pages       Print page indices and names without opening a window.\n" +
                    "  --gallery-smoke    Render all 27 pages, four frames each, and check cleanup.\n" +
                    "  --capture PATH     Save raw RGBA (last smoke page, or interactive frame four).\n" +
                    "  --no-fps           Hide the frame-rate readout (also hidden in smoke/captures).\n" +
                    "  --help, -h         Show this help.");
                return 0;
            }
            if (listPages)
            {
                for (int i = 0; i < GalleryDemo.PageCount; i++) Console.WriteLine($"{i}\t{GalleryDemo.PageName(i)}");
                return 0;
            }
            return RunGallery(ResolvePage(requestedPage), gallerySmoke, capture, showFps);
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

    private static int ResolvePage(string? requested)
    {
        if (requested is null) return 0;
        if (int.TryParse(requested, NumberStyles.Integer, CultureInfo.InvariantCulture, out int page))
        {
            if ((uint)page < GalleryDemo.PageCount) return page;
            throw new ArgumentException($"Page index must be between 0 and {GalleryDemo.PageCount - 1}.");
        }
        for (page = 0; page < GalleryDemo.PageCount; page++)
            if (GalleryDemo.PageName(page).Equals(requested, StringComparison.OrdinalIgnoreCase)) return page;
        throw new ArgumentException($"Unknown gallery page '{requested}'. Use --list-pages.");
    }

    private static int RunGallery(int firstPage, bool smoke, string? capture, bool showFps)
    {
        using var app = new Application(new()
        {
            Title = "CUI Gallery", Width = 1240, Height = 820, MinWidth = 720, MinHeight = 480,
            Hidden = smoke, Continuous = smoke || capture is not null, Theme = ThemeMode.Light, Accent = Accent.Neutral
        });
        using var demo = new GalleryDemo { Page = firstPage };
        var frameRate = showFps && !smoke && capture is null ? new FrameRateCounter() : null;
        int frames = 0, visited = 0, pageFrames = 0;
        bool resetScroll = false;
        while (app.NextFrame())
        {
            if (resetScroll) { app.Ui.ScrollTo("main", 0); resetScroll = false; }
            demo.Frame(app.Ui, frameRate is null ? default : frameRate.NextFrame());
            frames++;
            pageFrames++;
            if (smoke && pageFrames == 4)
            {
                byte[] pixels = app.CapturePixels(out int width, out int height);
                int colored = DifferingMainPixels(pixels, width, height);
                if (width != 1240 || height != 820 || colored < 1000 || app.Ui.FrameCount() == 0)
                    throw new InvalidOperationException($"Gallery page {demo.Page} did not render: {width}x{height}, {colored} differing main-content pixels.");
                Console.WriteLine($"CUI managed gallery page {demo.Page}\t{GalleryDemo.PageName(demo.Page)}\t{width}x{height}, {colored} differing main-content pixels, SHA256 {Convert.ToHexString(SHA256.HashData(pixels))}");
                if (++visited == GalleryDemo.PageCount)
                {
                    if (capture is not null) File.WriteAllBytes(capture, pixels);
                    app.Quit();
                }
                else
                {
                    demo.Page = (firstPage + visited) % GalleryDemo.PageCount;
                    resetScroll = true;
                    pageFrames = 0;
                }
            }
            else if (!smoke && capture is not null && frames == 4)
                File.WriteAllBytes(capture, app.CapturePixels(out _, out _));
        }
        if (smoke)
        {
            if (visited != GalleryDemo.PageCount) throw new InvalidOperationException($"Gallery closed after {visited}/{GalleryDemo.PageCount} pages.");
            demo.Dispose();
            demo.Dispose();
            app.Dispose();
            app.Dispose();
            bool rejected = false;
            try { app.NextFrame(); }
            catch (ObjectDisposedException) { rejected = true; }
            if (!rejected) throw new InvalidOperationException("A disposed gallery application was accepted.");
            using var retry = new Application(new() { Title = "CUI gallery reinitialization", Hidden = true, Continuous = true });
            if (!retry.NextFrame()) throw new InvalidOperationException("Gallery cleanup did not permit reinitialization.");
            retry.Quit();
            Console.WriteLine($"CUI managed gallery: {visited} pages, {frames} frames; disposal and reinitialization passed.");
        }
        return 0;
    }

    private static int DifferingMainPixels(ReadOnlySpan<byte> pixels, int width, int height)
    {
        int count = 0;
        for (int y = 0; y < height; y++)
            for (int x = 261; x < width; x++)
                if (!pixels.Slice((y * width + x) * 4, 3).SequenceEqual(pixels.Slice((y * width + 260) * 4, 3))) count++;
        return count;
    }
}
