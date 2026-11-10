// Ported from glfw/src/internal.h -- GLFW 3.6 internal types
// GLFWbool -> bool, function pointers -> delegates, linked lists preserved
//
// GlfwContext is defined in context.cs (with unsafe delegate* unmanaged pointers).
// IGlfwPlatform is defined here; GlfwPlatformSelector lives in platform.cs.

using System;

namespace Glfw
{
    // -----------------------------------------------------------------------
    // Constants
    // -----------------------------------------------------------------------

    public static class GlfwConstants
    {
        public const int _GLFW_MESSAGE_SIZE = 1024;

        public const int _GLFW_INSERT_FIRST = 0;
        public const int _GLFW_INSERT_LAST = 1;

        public const int _GLFW_POLL_PRESENCE = 0;
        public const int _GLFW_POLL_AXES = 1;
        public const int _GLFW_POLL_BUTTONS = 2;
        public const int _GLFW_POLL_ALL = _GLFW_POLL_AXES | _GLFW_POLL_BUTTONS;
    }

    // -----------------------------------------------------------------------
    // GlfwVidMode  (was GLFWvidmode)
    // -----------------------------------------------------------------------

    public struct GlfwVidMode
    {
        public int Width;
        public int Height;
        public int RedBits;
        public int GreenBits;
        public int BlueBits;
        public int RefreshRate;
    }

    // -----------------------------------------------------------------------
    // GlfwImage  (was GLFWimage)
    // -----------------------------------------------------------------------

    public struct GlfwImage
    {
        public int Width;
        public int Height;
        public byte[]? Pixels;
    }

    // -----------------------------------------------------------------------
    // GlfwGammaRamp  (was GLFWgammaramp)
    // -----------------------------------------------------------------------

    public class GlfwGammaRamp
    {
        public ushort[]? Red;
        public ushort[]? Green;
        public ushort[]? Blue;
        public uint Size;
    }

    // -----------------------------------------------------------------------
    // GlfwError  (was _GLFWerror)
    // -----------------------------------------------------------------------

    public class GlfwError
    {
        public GlfwError? Next;
        public int Code;
        public string Description = string.Empty;
    }

    // -----------------------------------------------------------------------
    // GlfwInitConfig  (was _GLFWinitconfig)
    // -----------------------------------------------------------------------

    public class GlfwInitConfig
    {
        public bool HatButtons;
        public int AngleType;
        public int PlatformID;
        // vulkanLoader omitted -- no Vulkan interop in managed port

        public NsConfig Ns = new();
        public X11Config X11 = new();
        public WlConfig Wl = new();

        public class NsConfig
        {
            public bool Menubar;
            public bool Chdir;
        }

        public class X11Config
        {
            public bool XcbVulkanSurface;
        }

        public class WlConfig
        {
            public int LibdecorMode;
        }
    }

    // -----------------------------------------------------------------------
    // GlfwWndConfig  (was _GLFWwndconfig)
    // -----------------------------------------------------------------------

    public class GlfwWndConfig
    {
        public int Xpos;
        public int Ypos;
        public int Width;
        public int Height;
        public bool Resizable;
        public bool Visible;
        public bool Decorated;
        public bool Focused;
        public bool AutoIconify;
        public bool Floating;
        public bool Maximized;
        public bool CenterCursor;
        public bool FocusOnShow;
        public bool MousePassthrough;
        public bool ScaleToMonitor;
        public bool ScaleFramebuffer;

        public NsWndConfig Ns = new();
        public X11WndConfig X11 = new();
        public Win32WndConfig Win32 = new();
        public WlWndConfig Wl = new();

        public class NsWndConfig
        {
            public string FrameName = string.Empty;
        }

        public class X11WndConfig
        {
            public string ClassName = string.Empty;
            public string InstanceName = string.Empty;
        }

        public class Win32WndConfig
        {
            public bool Keymenu;
            public bool ShowDefault;
        }

        public class WlWndConfig
        {
            public string AppId = string.Empty;
        }

        public GlfwWndConfig Clone()
        {
            var c = new GlfwWndConfig
            {
                Xpos = Xpos, Ypos = Ypos, Width = Width, Height = Height,
                Resizable = Resizable, Visible = Visible, Decorated = Decorated,
                Focused = Focused, AutoIconify = AutoIconify, Floating = Floating,
                Maximized = Maximized, CenterCursor = CenterCursor,
                FocusOnShow = FocusOnShow, MousePassthrough = MousePassthrough,
                ScaleToMonitor = ScaleToMonitor, ScaleFramebuffer = ScaleFramebuffer,
            };
            c.Ns.FrameName = Ns.FrameName;
            c.X11.ClassName = X11.ClassName;
            c.X11.InstanceName = X11.InstanceName;
            c.Win32.Keymenu = Win32.Keymenu;
            c.Win32.ShowDefault = Win32.ShowDefault;
            c.Wl.AppId = Wl.AppId;
            return c;
        }
    }

    // -----------------------------------------------------------------------
    // GlfwCtxConfig  (was _GLFWctxconfig)
    // -----------------------------------------------------------------------

    public class GlfwCtxConfig
    {
        public int Client;
        public int Source;
        public int Major;
        public int Minor;
        public bool Forward;
        public bool Debug;
        public bool Noerror;
        public int Profile;
        public int Robustness;
        public int Release;
        public GlfwWindow? Share;

        public NsglCtxConfig Nsgl;

        public struct NsglCtxConfig
        {
            public bool Offline;
        }

        public GlfwCtxConfig Clone()
        {
            return new GlfwCtxConfig
            {
                Client = Client, Source = Source, Major = Major, Minor = Minor,
                Forward = Forward, Debug = Debug, Noerror = Noerror,
                Profile = Profile, Robustness = Robustness, Release = Release,
                Share = Share, Nsgl = Nsgl,
            };
        }
    }

    // -----------------------------------------------------------------------
    // GlfwFbConfig  (was _GLFWfbconfig)
    // -----------------------------------------------------------------------

    public class GlfwFbConfig
    {
        public int RedBits;
        public int GreenBits;
        public int BlueBits;
        public int AlphaBits;
        public int DepthBits;
        public int StencilBits;
        public int AccumRedBits;
        public int AccumGreenBits;
        public int AccumBlueBits;
        public int AccumAlphaBits;
        public int AuxBuffers;
        public bool Stereo;
        public int Samples;
        public bool SRGB;
        public bool Doublebuffer;
        public bool Transparent;
        public nuint Handle;

        public GlfwFbConfig Clone()
        {
            return new GlfwFbConfig
            {
                RedBits = RedBits, GreenBits = GreenBits, BlueBits = BlueBits,
                AlphaBits = AlphaBits, DepthBits = DepthBits, StencilBits = StencilBits,
                AccumRedBits = AccumRedBits, AccumGreenBits = AccumGreenBits,
                AccumBlueBits = AccumBlueBits, AccumAlphaBits = AccumAlphaBits,
                AuxBuffers = AuxBuffers, Stereo = Stereo, Samples = Samples,
                SRGB = SRGB, Doublebuffer = Doublebuffer, Transparent = Transparent,
                Handle = Handle,
            };
        }
    }

    // NOTE: GlfwContext is defined in context.cs with full unsafe delegate* members.

    // -----------------------------------------------------------------------
    // Callback delegates for window events (strongly typed)
    // These mirror the C GLFW callback typedefs with GlfwWindow instead of opaque ptr.
    // -----------------------------------------------------------------------

    public delegate void GlfwWindowPosFun(GlfwWindow window, int xpos, int ypos);
    public delegate void GlfwWindowSizeFun(GlfwWindow window, int width, int height);
    public delegate void GlfwWindowCloseFun(GlfwWindow window);
    public delegate void GlfwWindowRefreshFun(GlfwWindow window);
    public delegate void GlfwWindowFocusFun(GlfwWindow window, int focused);
    public delegate void GlfwWindowIconifyFun(GlfwWindow window, int iconified);
    public delegate void GlfwWindowMaximizeFun(GlfwWindow window, int maximized);
    public delegate void GlfwFramebufferSizeFun(GlfwWindow window, int width, int height);
    public delegate void GlfwWindowContentScaleFun(GlfwWindow window, float xscale, float yscale);
    public delegate void GlfwMouseButtonFun(GlfwWindow window, int button, int action, int mods);
    public delegate void GlfwCursorPosFun(GlfwWindow window, double xpos, double ypos);
    public delegate void GlfwCursorEnterFun(GlfwWindow window, int entered);
    public delegate void GlfwScrollFun(GlfwWindow window, double xoffset, double yoffset);
    public delegate void GlfwKeyFun(GlfwWindow window, int key, int scancode, int action, int mods);
    public delegate void GlfwCharFun(GlfwWindow window, uint codepoint);
    public delegate void GlfwCharModsFun(GlfwWindow window, uint codepoint, int mods);
    public delegate void GlfwDropFun(GlfwWindow window, int count, string[] paths);
    public delegate void GlfwMonitorFun(GlfwMonitor monitor, int eventType);
    public delegate void GlfwJoystickFun(int jid, int eventType);

    // -----------------------------------------------------------------------
    // Platform-specific window state (Null platform)
    // Win32 and X11 types are defined in their respective platform files.
    // -----------------------------------------------------------------------

    public class GlfwWindowNull
    {
        public int Xpos;
        public int Ypos;
        public int Width;
        public int Height;
        public bool Visible;
        public bool Iconified;
        public bool Maximized;
        public bool Resizable;
        public bool Decorated;
        public bool Floating;
        public bool Transparent;
        public float Opacity;
    }


    // -----------------------------------------------------------------------
    // Platform-specific monitor state (Null platform)
    // -----------------------------------------------------------------------

    public class GlfwMonitorNull
    {
        public GlfwGammaRamp Ramp = new();
    }


    // -----------------------------------------------------------------------
    // Platform-specific library state (Null platform)
    // -----------------------------------------------------------------------

    public class GlfwLibraryNull
    {
        public int Xcursor;
        public int Ycursor;
        public string? ClipboardString;
        public GlfwWindow? FocusedWindow;
        public ushort[] Keycodes = new ushort[121]; // GLFW_NULL_SC_LAST + 1
        public byte[] Scancodes = new byte[GLFW.GLFW_KEY_LAST + 1];
    }


    // -----------------------------------------------------------------------
    // GlfwWindow  (was _GLFWwindow)
    // -----------------------------------------------------------------------

    public class GlfwWindow
    {
        // Linked list
        public GlfwWindow? Next;

        // Window settings and state
        public bool Resizable;
        public bool Decorated;
        public bool AutoIconify;
        public bool Floating;
        public bool FocusOnShow;
        public bool MousePassthrough;
        public bool ShouldClose;
        public object? UserPointer;
        public bool doublebuffer;  // lowercase to match context.cs usage

        public GlfwVidMode VideoMode;
        public GlfwMonitor? Monitor;
        public GlfwCursor? Cursor;
        public string? Title;

        public int MinWidth, MinHeight;
        public int MaxWidth, MaxHeight;
        public int Numer, Denom;

        public bool StickyKeys;
        public bool StickyMouseButtons;
        public bool LockKeyMods;
        public bool DisableMouseButtonLimit;
        public int CursorMode;
        public byte[] MouseButtons = new byte[GLFW.GLFW_MOUSE_BUTTON_LAST + 1];
        public byte[] Keys = new byte[GLFW.GLFW_KEY_LAST + 1];

        // Virtual cursor position when cursor is disabled
        public double VirtualCursorPosX;
        public double VirtualCursorPosY;
        public bool RawMouseMotion;

        // Context (lowercase to match context.cs usage: window.context.client, etc.)
        public GlfwContext context = new();

        // Callbacks
        public WindowCallbacks Callbacks = new();

        public class WindowCallbacks
        {
            public GlfwWindowPosFun? Pos;
            public GlfwWindowSizeFun? Size;
            public GlfwWindowCloseFun? Close;
            public GlfwWindowRefreshFun? Refresh;
            public GlfwWindowFocusFun? Focus;
            public GlfwWindowIconifyFun? Iconify;
            public GlfwWindowMaximizeFun? Maximize;
            public GlfwFramebufferSizeFun? Fbsize;
            public GlfwWindowContentScaleFun? Scale;
            public GlfwMouseButtonFun? MouseButton;
            public GlfwCursorPosFun? CursorPos;
            public GlfwCursorEnterFun? CursorEnter;
            public GlfwScrollFun? Scroll;
            public GlfwKeyFun? Key;
            public GlfwCharFun? Character;
            public GlfwCharModsFun? Charmods;
            public GlfwDropFun? Drop;
        }

        // Platform-specific state (nullable sub-objects)
        public GlfwWindowNull? Null;
        public GlfwWindowWin32? Win32;
        public GlfwWindowX11? X11;
    }

    // -----------------------------------------------------------------------
    // GlfwMonitor  (was _GLFWmonitor)
    // -----------------------------------------------------------------------

    public class GlfwMonitor
    {
        public string Name = string.Empty;
        public object? UserPointer;

        // Physical dimensions in millimeters
        public int WidthMM;
        public int HeightMM;

        // The window whose video mode is current on this monitor
        public GlfwWindow? Window;

        public GlfwVidMode[]? Modes;
        public int ModeCount;
        public GlfwVidMode CurrentMode;

        public GlfwGammaRamp OriginalRamp = new();
        public GlfwGammaRamp CurrentRamp = new();

        // Platform-specific state
        public GlfwMonitorNull? Null;
        public GlfwMonitorWin32? Win32;
        public GlfwMonitorX11? X11;
    }

    // -----------------------------------------------------------------------
    // GlfwCursor  (was _GLFWcursor)
    // -----------------------------------------------------------------------

    public class GlfwCursor
    {
        public GlfwCursor? Next;
        // Platform-specific state
        public GlfwCursorWin32? Win32;
        public GlfwCursorX11? X11;
    }

    // -----------------------------------------------------------------------
    // IGlfwPlatform  (was _GLFWplatform -- interface for platform backends)
    // -----------------------------------------------------------------------

    public interface IGlfwPlatform
    {
        int PlatformID { get; }

        // Init
        bool Init();
        void Terminate();

        // Input
        void GetCursorPos(GlfwWindow window, out double xpos, out double ypos);
        void SetCursorPos(GlfwWindow window, double xpos, double ypos);
        void SetCursorMode(GlfwWindow window, int mode);
        void SetRawMouseMotion(GlfwWindow window, bool enabled);
        bool RawMouseMotionSupported();
        bool CreateCursor(GlfwCursor cursor, in GlfwImage image, int xhot, int yhot);
        bool CreateStandardCursor(GlfwCursor cursor, int shape);
        void DestroyCursor(GlfwCursor cursor);
        void SetCursor(GlfwWindow window, GlfwCursor? cursor);
        string? GetScancodeName(int scancode);
        int GetKeyScancode(int key);
        void SetClipboardString(string value);
        string? GetClipboardString();

        // Joysticks
        bool InitJoysticks();
        void TerminateJoysticks();
        bool PollJoystick(GlfwJoystick joystick, int mode);
        string GetMappingName();
        string UpdateGamepadGUID(string guid);

        // Monitor
        void FreeMonitor(GlfwMonitor monitor);
        void GetMonitorPos(GlfwMonitor monitor, out int xpos, out int ypos);
        void GetMonitorContentScale(GlfwMonitor monitor, out float xscale, out float yscale);
        void GetMonitorWorkarea(GlfwMonitor monitor, out int xpos, out int ypos, out int width, out int height);
        GlfwVidMode[]? GetVideoModes(GlfwMonitor monitor, out int count);
        bool GetVideoMode(GlfwMonitor monitor, out GlfwVidMode mode);
        bool GetGammaRamp(GlfwMonitor monitor, GlfwGammaRamp ramp);
        void SetGammaRamp(GlfwMonitor monitor, GlfwGammaRamp ramp);

        // Window
        bool CreateWindow(GlfwWindow window, GlfwWndConfig wndconfig, GlfwCtxConfig ctxconfig, GlfwFbConfig fbconfig);
        void DestroyWindow(GlfwWindow window);
        void SetWindowTitle(GlfwWindow window, string title);
        void SetWindowIcon(GlfwWindow window, int count, GlfwImage[]? images);
        void GetWindowPos(GlfwWindow window, out int xpos, out int ypos);
        void SetWindowPos(GlfwWindow window, int xpos, int ypos);
        void GetWindowSize(GlfwWindow window, out int width, out int height);
        void SetWindowSize(GlfwWindow window, int width, int height);
        void SetWindowSizeLimits(GlfwWindow window, int minwidth, int minheight, int maxwidth, int maxheight);
        void SetWindowAspectRatio(GlfwWindow window, int numer, int denom);
        void GetFramebufferSize(GlfwWindow window, out int width, out int height);
        void GetWindowFrameSize(GlfwWindow window, out int left, out int top, out int right, out int bottom);
        void GetWindowContentScale(GlfwWindow window, out float xscale, out float yscale);
        void IconifyWindow(GlfwWindow window);
        void RestoreWindow(GlfwWindow window);
        void MaximizeWindow(GlfwWindow window);
        void ShowWindow(GlfwWindow window);
        void HideWindow(GlfwWindow window);
        void RequestWindowAttention(GlfwWindow window);
        void FocusWindow(GlfwWindow window);
        void SetWindowMonitor(GlfwWindow window, GlfwMonitor? monitor, int xpos, int ypos, int width, int height, int refreshRate);
        bool WindowFocused(GlfwWindow window);
        bool WindowIconified(GlfwWindow window);
        bool WindowVisible(GlfwWindow window);
        bool WindowMaximized(GlfwWindow window);
        bool WindowHovered(GlfwWindow window);
        bool FramebufferTransparent(GlfwWindow window);
        float GetWindowOpacity(GlfwWindow window);
        void SetWindowResizable(GlfwWindow window, bool enabled);
        void SetWindowDecorated(GlfwWindow window, bool enabled);
        void SetWindowFloating(GlfwWindow window, bool enabled);
        void SetWindowOpacity(GlfwWindow window, float opacity);
        void SetWindowMousePassthrough(GlfwWindow window, bool enabled);
        void PollEvents();
        void WaitEvents();
        void WaitEventsTimeout(double timeout);
        void PostEmptyEvent();
    }

    // -----------------------------------------------------------------------
    // GlfwLibrary  (was _GLFWlibrary -- global state, exposed as static class _glfw)
    // -----------------------------------------------------------------------

    public static partial class _glfw
    {
        public static bool initialized;
        public static IGlfwPlatform? platform;

        // Hints
        public static readonly HintsData hints = new();

        public class HintsData
        {
            public GlfwInitConfig Init = new();
            public GlfwFbConfig Framebuffer = new();
            public GlfwWndConfig Window = new();
            public GlfwCtxConfig Context = new();
            public int RefreshRate;
        }

        // Linked lists
        public static GlfwError? errorListHead;
        public static GlfwCursor? cursorListHead;
        public static GlfwWindow? windowListHead;

        // Monitors (array matching C++ _GLFWmonitor** pattern)
        public static GlfwMonitor[]? monitors;
        public static int monitorCount;

        // Timer
        public static readonly TimerData timer = new();

        public class TimerData
        {
            public ulong Offset;
        }

        // Thread-local storage (C# equivalent of _GLFWtls via [ThreadStatic])
        [ThreadStatic]
        public static GlfwError? errorSlot;

        // contextSlot: the thread-local _GLFWwindow* (managed as GlfwWindow?)
        // In C++: _glfwPlatformGetTls(&_glfw.contextSlot) / _glfwPlatformSetTls(...)
        internal static int tlsGeneration;
        [ThreadStatic] private static int contextGeneration;
        [ThreadStatic] private static GlfwWindow? threadContext;
        public static GlfwWindow? contextSlot
        {
            get => contextGeneration == tlsGeneration ? threadContext : null;
            set { threadContext = value; contextGeneration = tlsGeneration; }
        }

        // Mutex for error list (C# equivalent of _GLFWmutex)
        public static readonly object errorLock = new();

        // Library-level callbacks
        public static GlfwMonitorFun? monitorCallback;
        public static GlfwJoystickFun? joystickCallback;

        // Platform-specific library state (nullable sub-objects)
        public static GlfwLibraryNull? Null;
        public static GlfwLibraryWin32? Win32;
        public static GlfwLibraryX11? X11;
    }
}
