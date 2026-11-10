using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using G = Glfw.Glfw;

internal enum InputKey { Left, Home, End, Backspace, Delete, Control, A, Enter }

internal abstract class OsInput : IDisposable
{
    internal static OsInput Create()
    {
        var window = G.glfwGetCurrentContext() ?? throw new InvalidOperationException("No desktop GL context");
        G.glfwGetWindowContentScale(window, out float scale, out _);
        return OperatingSystem.IsWindows()
            ? new WindowsInput(G.glfwGetWin32Window(window), scale)
            : new X11Input(G.glfwGetX11Display(), G.glfwGetX11Window(window), scale);
    }
    internal abstract void Focus();
    internal abstract void Move(float x, float y);
    internal abstract void Button(bool down);
    internal abstract void Key(InputKey key, bool down);
    internal abstract void Text(string value);
    internal abstract void Resize(int width, int height);
    internal abstract void Close();
    internal abstract void Refresh();
    internal virtual bool Idle => true;
    public virtual void Dispose() { }
}

internal sealed unsafe class X11Input(nint display, nuint window, float scale) : OsInput
{
    private Task? typing;
    [DllImport("libX11.so.6")] private static extern nuint XInternAtom(nint display, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, int onlyIfExists);
    [DllImport("libX11.so.6")] private static extern int XSendEvent(nint display, nuint window, int propagate, nint mask, void* ev);
    [DllImport("libX11.so.6")] private static extern int XSync(nint display, int discard);

    [StructLayout(LayoutKind.Sequential)]
    private struct XClientMessage
    {
        public int type;
        public nuint serial;
        public int sendEvent;
        public nint display;
        public nuint window, messageType;
        public int format;
        public nint data0, data1, data2, data3, data4;
    }
    [StructLayout(LayoutKind.Sequential)]
    private struct XExpose
    {
        public int type;
        public nuint serial;
        public int sendEvent;
        public nint display;
        public nuint window;
        public int x, y, width, height, count;
    }

    private string Id => window.ToString(CultureInfo.InvariantCulture);
    private static string Number(float value) => ((int)MathF.Round(value)).ToString(CultureInfo.InvariantCulture);

    private static void Xdotool(params string[] arguments)
    {
        var start = new ProcessStartInfo("xdotool")
        {
            UseShellExecute = false, RedirectStandardError = true, RedirectStandardOutput = true
        };
        foreach (string argument in arguments) start.ArgumentList.Add(argument);
        using var process = Process.Start(start) ?? throw new InvalidOperationException("Could not run xdotool");
        if (!process.WaitForExit(10000))
        {
            process.Kill(entireProcessTree: true);
            throw new TimeoutException("xdotool " + string.Join(' ', arguments));
        }
        string error = process.StandardError.ReadToEnd();
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"xdotool {string.Join(' ', arguments)}: exit {process.ExitCode}: {error}");
    }

    internal override void Focus() => Xdotool("windowraise", Id, "windowfocus", "--sync", Id);
    // No --window on button/key/type: XTEST delivers to the actual OS focus and
    // pointer target, so this does not use xdotool's XSendEvent input fallback.
    internal override void Move(float x, float y) => Xdotool("mousemove", "--sync", "--window", Id, Number(x * scale), Number(y * scale));
    internal override void Button(bool down) => Xdotool(down ? "mousedown" : "mouseup", "1");
    internal override void Key(InputKey key, bool down) => Xdotool(down ? "keydown" : "keyup", key switch
    {
        InputKey.Control => "Control_L", InputKey.Backspace => "BackSpace", InputKey.Enter => "Return",
        InputKey.A => "a", _ => key.ToString()
    });
    internal override void Text(string value)
    {
        if (!Idle) throw new InvalidOperationException("Previous OS typing is still active");
        // Keep the application pumping while xdotool temporarily maps Unicode
        // keysyms. Blocking until it exits can restore the keymap before GLFW
        // looks up the queued character events, silently losing Unicode text.
        typing = Task.Run(() => Xdotool("type", "--clearmodifiers", "--delay", "100", "--", value));
    }
    internal override bool Idle
    {
        get
        {
            if (typing is null) return true;
            if (!typing.IsCompleted) return false;
            typing.GetAwaiter().GetResult();
            typing = null;
            return true;
        }
    }
    public override void Dispose() => typing?.GetAwaiter().GetResult();
    internal override void Resize(int width, int height) => Xdotool("windowsize", "--sync", Id, width.ToString(CultureInfo.InvariantCulture), height.ToString(CultureInfo.InvariantCulture));

    internal override void Close()
    {
        // Send the same WM_PROTOCOLS client message as a window-manager close
        // button. XDestroyWindow would bypass GLFW/CUI's close callback.
        byte* ev = stackalloc byte[24 * 8];
        new Span<byte>(ev, 24 * 8).Clear();
        var message = (XClientMessage*)ev;
        message->type = 33;
        message->display = display;
        message->window = window;
        message->messageType = XInternAtom(display, "WM_PROTOCOLS", 0);
        message->format = 32;
        message->data0 = (nint)XInternAtom(display, "WM_DELETE_WINDOW", 0);
        Send(ev, 0);
    }

    internal override void Refresh()
    {
        byte* ev = stackalloc byte[24 * 8];
        new Span<byte>(ev, 24 * 8).Clear();
        var expose = (XExpose*)ev;
        expose->type = 12;
        expose->display = display;
        expose->window = window;
        expose->width = 640;
        expose->height = 480;
        Send(ev, 1 << 15);
    }

    private void Send(void* ev, nint mask)
    {
        if (XSendEvent(display, window, 0, mask, ev) == 0)
            throw new InvalidOperationException("XSendEvent failed");
        XSync(display, 0);
    }
}

// Runtime counterpart for an actual Windows x64 desktop. Compiling this code
// on Linux is not Windows runtime qualification.
internal sealed class WindowsInput(nint window, float scale) : OsInput
{
    [StructLayout(LayoutKind.Sequential)] private struct Point { public int x, y; }
    [StructLayout(LayoutKind.Sequential)] private struct Rect { public int left, top, right, bottom; }
    [StructLayout(LayoutKind.Sequential)] private struct Mouse
    {
        public int x, y;
        public uint data, flags, time;
        public nuint extra;
    }
    [StructLayout(LayoutKind.Sequential)] private struct Keyboard
    {
        public ushort key, scan;
        public uint flags, time;
        public nuint extra;
    }
    [StructLayout(LayoutKind.Explicit)] private struct InputUnion
    {
        [FieldOffset(0)] public Mouse mouse;
        [FieldOffset(0)] public Keyboard keyboard;
    }
    [StructLayout(LayoutKind.Sequential)] private struct Input { public uint type; public InputUnion value; }

    [DllImport("user32.dll", SetLastError = true)] private static extern uint SendInput(uint count, [In] Input[] inputs, int size);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool SetForegroundWindow(nint window);
    [DllImport("user32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool ClientToScreen(nint window, ref Point point);
    [DllImport("user32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetClientRect(nint window, out Rect rect);
    [DllImport("user32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetWindowRect(nint window, out Rect rect);
    [DllImport("user32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool SetWindowPos(nint window, nint insertAfter, int x, int y, int width, int height, uint flags);
    [DllImport("user32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool PostMessageW(nint window, uint message, nuint wparam, nint lparam);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool InvalidateRect(nint window, nint rect, [MarshalAs(UnmanagedType.Bool)] bool erase);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool UpdateWindow(nint window);

    private static void Require(bool result, string name)
    {
        if (!result) throw new InvalidOperationException($"{name} failed: {Marshal.GetLastPInvokeError()}");
    }
    private static void Send(Input input) => Require(SendInput(1, [input], Marshal.SizeOf<Input>()) == 1, "SendInput");
    internal override void Focus() => Require(SetForegroundWindow(window), "SetForegroundWindow");
    internal override void Move(float x, float y)
    {
        var point = new Point { x = (int)MathF.Round(x * scale), y = (int)MathF.Round(y * scale) };
        Require(ClientToScreen(window, ref point), "ClientToScreen");
        Require(SetCursorPos(point.x, point.y), "SetCursorPos");
    }
    internal override void Button(bool down) => Send(new Input
    {
        type = 0, value = new InputUnion { mouse = new Mouse { flags = down ? 0x0002u : 0x0004u } }
    });
    internal override void Key(InputKey key, bool down) => Send(new Input
    {
        type = 1, value = new InputUnion { keyboard = new Keyboard
        {
            key = key switch
            {
                InputKey.Left => 0x25, InputKey.Home => 0x24, InputKey.End => 0x23,
                InputKey.Backspace => 0x08, InputKey.Delete => 0x2e, InputKey.Control => 0x11,
                InputKey.A => 0x41, InputKey.Enter => 0x0d, _ => throw new ArgumentOutOfRangeException(nameof(key))
            },
            flags = (down ? 0u : 0x0002u) |
                (key is InputKey.Left or InputKey.Home or InputKey.End or InputKey.Delete ? 0x0001u : 0u)
        } }
    });
    internal override void Text(string value)
    {
        foreach (char character in value)
        {
            Send(new Input { type = 1, value = new InputUnion { keyboard = new Keyboard { scan = character, flags = 0x0004 } } });
            Send(new Input { type = 1, value = new InputUnion { keyboard = new Keyboard { scan = character, flags = 0x0006 } } });
        }
    }
    internal override void Resize(int width, int height)
    {
        Require(GetWindowRect(window, out var outer), "GetWindowRect");
        Require(GetClientRect(window, out var client), "GetClientRect");
        Require(SetWindowPos(window, 0, 0, 0, width + outer.right - outer.left - client.right,
            height + outer.bottom - outer.top - client.bottom, 0x0006), "SetWindowPos");
    }
    internal override void Close() => Require(PostMessageW(window, 0x0010, 0, 0), "WM_CLOSE");
    internal override void Refresh()
    {
        Require(InvalidateRect(window, 0, false), "InvalidateRect");
        Require(UpdateWindow(window), "UpdateWindow");
    }
}
