using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Managed.Cui;
using static Managed.Cui.Cui;

internal static unsafe class Program
{
    [STAThread]
    private static int Main(string[] arguments)
    {
        try
        {
            bool smoke = arguments.Contains("--smoke");
            if (smoke)
            {
                var software = new cui_app_desc { renderer = cui_renderer_kind.CUI_RENDERER_SOFTWARE };
                if (cui_app_create(&software) != null)
                    throw new InvalidOperationException("Unsupported GLFW software window was accepted.");
            }
            string? capture = arguments.SkipWhile(a => a != "--capture").Skip(1).FirstOrDefault();
            using var app = new DesktopApplication("Translated CUI · Windows / X11", 640, 480,
                hidden: smoke, continuous: smoke);
            if (smoke)
            {
                bool rejected = false;
                try { using var second = new DesktopApplication(hidden: true); }
                catch (InvalidOperationException) { rejected = true; }
                if (!rejected) throw new InvalidOperationException("A second desktop application was accepted.");
                Exception? threadError = null;
                var otherThread = new Thread(() =>
                {
                    try { app.Quit(); }
                    catch (Exception error) { threadError = error; }
                });
                otherThread.Start(); otherThread.Join();
                if (threadError is not InvalidOperationException)
                    throw new InvalidOperationException("Cross-thread application access was accepted.");
            }
            int frames = 0, clicks = 0;
            CBool enabled = true;
            float value = 0.5f;
            byte[] input = new byte[128];
            while (app.NextFrame())
            {
                var ui = app.Ui;
                fixed (byte* heading = "Translated CUI: widgets, input and OpenGL"u8)
                fixed (byte* button = "Count click"u8)
                fixed (byte* check = "Enabled"u8)
                fixed (byte* slider = "Value"u8)
                fixed (byte* inputId = "message"u8)
                fixed (byte* buffer = input)
                {
                    cui_column_begin(ui, null);
                    cui_label(ui, heading);
                    if (cui_button_ex(ui, button, null) != 0) clicks++;
                    cui_checkbox_ex(ui, check, &enabled, null);
                    cui_slider_ex(ui, slider, &value, 0, 1, null);
                    cui_input_ex(ui, inputId, buffer, (ulong)input.Length, null);
                    cui_box_end(ui);
                }
                if (smoke && ++frames == 4)
                {
                    byte[] pixels = app.CapturePixels(out int width, out int height);
                    int colored = 0;
                    for (int i = 4; i < pixels.Length; i += 4)
                        if (!pixels.AsSpan(i, 3).SequenceEqual(pixels.AsSpan(0, 3))) colored++;
                    if (width != 640 || height != 480 || colored < 100)
                        throw new InvalidOperationException($"Unexpected framebuffer {width}x{height}, {colored} differing pixels.");
                    if (capture is not null) File.WriteAllBytes(capture, pixels);
                    Console.WriteLine($"CUI GLFW OpenGL {RuntimeInformation.OSDescription}: {frames} frames, {width}x{height}, {colored} differing pixels, SHA256 {Convert.ToHexString(SHA256.HashData(pixels))}, clicks {clicks}");
                    app.Quit();
                }
            }
            if (smoke)
            {
                app.Dispose(); app.Dispose();
                bool rejected = false;
                try { app.NextFrame(); }
                catch (ObjectDisposedException) { rejected = true; }
                if (!rejected) throw new InvalidOperationException("A disposed application was accepted.");
                using var retry = new DesktopApplication("CUI reinitialization", hidden: true, continuous: true);
                if (!retry.NextFrame()) throw new InvalidOperationException("Reinitialization did not start a frame.");
                retry.Quit();
                Console.WriteLine("CUI owning lifecycle: software rejection, second app rejection, owner thread, disposal and reinitialization passed.");
            }
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
