// Ported from glfw/src/x11_platform.h -- GLFW 3.6 X11 platform types
//
// Copyright (c) 2002-2006 Marcus Geelnard
// Copyright (c) 2006-2019 Camilla Loewy <elmindreda@glfw.org>
//
// This software is provided 'as-is', without any express or implied
// warranty. In no event will the authors be held liable for any damages
// arising from the use of this software.
//
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it
// freely, subject to the following restrictions:
//
// 1. The origin of this software must not be misrepresented; you must not
//    claim that you wrote the original software. If you use this software
//    in a product, an acknowledgment in the product documentation would
//    be appreciated but is not required.
//
// 2. Altered source versions must be plainly marked as such, and must not
//    be misrepresented as being the original software.
//
// 3. This notice may not be removed or altered from any source
//    distribution.
//
// NOTE: GLX constants, GlfwContextGLX, and GlfwLibraryGLX are defined
//       in glx/glx_native.cs -- not duplicated here.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Glfw
{
    // =======================================================================
    //  X11 interop structs (matching C ABI layouts for p/invoke)
    // =======================================================================

    // XEvent is a union padded to 24 native longs (192 bytes on LP64).
    // the event subtypes we actually read at FieldOffset(0).
    [StructLayout(LayoutKind.Explicit)]
    public struct XEvent
    {
        [FieldOffset(0)] private XEventPadding padding;
        [FieldOffset(0)] public int type;

        // Overlay the subtypes we access at offset 0 (they all start with type)
        [FieldOffset(0)] public XKeyEvent xkey;
        [FieldOffset(0)] public XButtonEvent xbutton;
        [FieldOffset(0)] public XMotionEvent xmotion;
        [FieldOffset(0)] public XCrossingEvent xcrossing;
        [FieldOffset(0)] public XFocusChangeEvent xfocus;
        [FieldOffset(0)] public XExposeEvent xexpose;
        [FieldOffset(0)] public XGraphicsExposeEvent xgraphicsexpose;
        [FieldOffset(0)] public XNoExposeEvent xnoexpose;
        [FieldOffset(0)] public XMapEvent xmap;
        [FieldOffset(0)] public XUnmapEvent xunmap;
        [FieldOffset(0)] public XMapRequestEvent xmaprequest;
        [FieldOffset(0)] public XReparentEvent xreparent;
        [FieldOffset(0)] public XConfigureEvent xconfigure;
        [FieldOffset(0)] public XConfigureRequestEvent xconfigurerequest;
        [FieldOffset(0)] public XDestroyWindowEvent xdestroywindow;
        [FieldOffset(0)] public XPropertyEvent xproperty;
        [FieldOffset(0)] public XSelectionClearEvent xselectionclear;
        [FieldOffset(0)] public XSelectionRequestEvent xselectionrequest;
        [FieldOffset(0)] public XSelectionEvent xselection;
        [FieldOffset(0)] public XClientMessageEvent xclient;
        [FieldOffset(0)] public XMappingEvent xmapping;
        [FieldOffset(0)] public XGenericEventCookie xcookie;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct XEventPadding
    {
        internal nint A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XErrorEvent
    {
        public int type;
        public nint display;
        public nuint resourceid, serial;
        public byte error_code, request_code, minor_code;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct XRenderDirectFormat
    {
        public short red, redMask, green, greenMask, blue, blueMask, alpha, alphaMask;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct XRenderPictFormat
    {
        public nuint id;
        public int type, depth;
        public XRenderDirectFormat direct;
        public nuint colormap;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XkbStateNotifyEvent
    {
        public int type;
        public nuint serial;
        public int send_event;
        public nint display;
        public nuint time;
        public int xkb_type, device;
        public uint changed;
        public int group, base_group, latched_group, locked_group;
        public uint mods, base_mods, latched_mods, locked_mods;
        public int compat_state;
        public byte grab_mods, compat_grab_mods, lookup_mods, compat_lookup_mods;
        public int ptr_buttons;
        public byte keycode, event_type, req_major, req_minor;
    }

    // -------------------------------------------------------------------
    //  X11 event subtype structs
    // -------------------------------------------------------------------

    [StructLayout(LayoutKind.Sequential)]
    public struct XKeyEvent
    {
        public int type;
        public nuint serial;
        public int send_event;        // Bool
        public nint display;           // Display*
        public nuint window;           // Window
        public nuint root;             // Window
        public nuint subwindow;        // Window
        public nuint time;             // Time
        public int x, y;
        public int x_root, y_root;
        public uint state;
        public uint keycode;
        public int same_screen;        // Bool
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XButtonEvent
    {
        public int type;
        public nuint serial;
        public int send_event;
        public nint display;
        public nuint window;
        public nuint root;
        public nuint subwindow;
        public nuint time;
        public int x, y;
        public int x_root, y_root;
        public uint state;
        public uint button;
        public int same_screen;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XMotionEvent
    {
        public int type;
        public nuint serial;
        public int send_event;
        public nint display;
        public nuint window;
        public nuint root;
        public nuint subwindow;
        public nuint time;
        public int x, y;
        public int x_root, y_root;
        public uint state;
        public byte is_hint;
        public int same_screen;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XCrossingEvent
    {
        public int type;
        public nuint serial;
        public int send_event;
        public nint display;
        public nuint window;
        public nuint root;
        public nuint subwindow;
        public nuint time;
        public int x, y;
        public int x_root, y_root;
        public int mode;
        public int detail;
        public int same_screen;
        public int focus;
        public uint state;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XFocusChangeEvent
    {
        public int type;
        public nuint serial;
        public int send_event;
        public nint display;
        public nuint window;
        public int mode;
        public int detail;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XExposeEvent
    {
        public int type;
        public nuint serial;
        public int send_event;
        public nint display;
        public nuint window;
        public int x, y;
        public int width, height;
        public int count;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XGraphicsExposeEvent
    {
        public int type;
        public nuint serial;
        public int send_event;
        public nint display;
        public nuint drawable;      // Drawable
        public int x, y;
        public int width, height;
        public int count;
        public int major_code;
        public int minor_code;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XNoExposeEvent
    {
        public int type;
        public nuint serial;
        public int send_event;
        public nint display;
        public nuint drawable;
        public int major_code;
        public int minor_code;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XMapEvent
    {
        public int type;
        public nuint serial;
        public int send_event;
        public nint display;
        public nuint @event;        // Window
        public nuint window;
        public int override_redirect;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XUnmapEvent
    {
        public int type;
        public nuint serial;
        public int send_event;
        public nint display;
        public nuint @event;
        public nuint window;
        public int from_configure;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XMapRequestEvent
    {
        public int type;
        public nuint serial;
        public int send_event;
        public nint display;
        public nuint parent;
        public nuint window;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XReparentEvent
    {
        public int type;
        public nuint serial;
        public int send_event;
        public nint display;
        public nuint @event;
        public nuint window;
        public nuint parent;
        public int x, y;
        public int override_redirect;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XConfigureEvent
    {
        public int type;
        public nuint serial;
        public int send_event;
        public nint display;
        public nuint @event;
        public nuint window;
        public int x, y;
        public int width, height;
        public int border_width;
        public nuint above;         // Window
        public int override_redirect;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XConfigureRequestEvent
    {
        public int type;
        public nuint serial;
        public int send_event;
        public nint display;
        public nuint parent;
        public nuint window;
        public int x, y;
        public int width, height;
        public int border_width;
        public nuint above;
        public int detail;
        public nuint value_mask;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XDestroyWindowEvent
    {
        public int type;
        public nuint serial;
        public int send_event;
        public nint display;
        public nuint @event;
        public nuint window;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XPropertyEvent
    {
        public int type;
        public nuint serial;
        public int send_event;
        public nint display;
        public nuint window;
        public nuint atom;           // Atom
        public nuint time;           // Time
        public int state;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XSelectionClearEvent
    {
        public int type;
        public nuint serial;
        public int send_event;
        public nint display;
        public nuint window;
        public nuint selection;      // Atom
        public nuint time;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XSelectionRequestEvent
    {
        public int type;
        public nuint serial;
        public int send_event;
        public nint display;
        public nuint owner;          // Window
        public nuint requestor;      // Window
        public nuint selection;      // Atom
        public nuint target;         // Atom
        public nuint property;       // Atom
        public nuint time;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XSelectionEvent
    {
        public int type;
        public nuint serial;
        public int send_event;
        public nint display;
        public nuint requestor;
        public nuint selection;
        public nuint target;
        public nuint property;
        public nuint time;
    }

    // XClientMessageEvent.data is a union of 20 bytes / 10 shorts / 5 longs
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct XClientMessageEvent
    {
        public int type;
        public nuint serial;
        public int send_event;
        public nint display;
        public nuint window;
        public nuint message_type;    // Atom
        public int format;
        // data: union { char b[20]; short s[10]; long l[5]; }
        // On 64-bit Linux, long = 8 bytes, so l[5] = 40 bytes (largest member)
        public XClientMessageData l;
    }

    [InlineArray(5)]
    public struct XClientMessageData { private nint element; }

    [StructLayout(LayoutKind.Sequential)]
    public struct XMappingEvent
    {
        public int type;
        public nuint serial;
        public int send_event;
        public nint display;
        public nuint window;
        public int request;
        public int first_keycode;
        public int count;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XGenericEventCookie
    {
        public int type;
        public nuint serial;
        public int send_event;
        public nint display;
        public int extension;
        public int evtype;
        public uint cookie;
        public nint data;              // void*
    }

    // -------------------------------------------------------------------
    //  X11 structs used in Xlib calls
    // -------------------------------------------------------------------

    [StructLayout(LayoutKind.Sequential)]
    public struct XSetWindowAttributes
    {
        public nint background_pixmap;       // Pixmap
        public nuint background_pixel;
        public nint border_pixmap;           // Pixmap
        public nuint border_pixel;
        public int bit_gravity;
        public int win_gravity;
        public int backing_store;
        public nuint backing_planes;
        public nuint backing_pixel;
        public int save_under;               // Bool
        public nint event_mask;              // long
        public nint do_not_propagate_mask;   // long
        public int override_redirect;        // Bool
        public nuint colormap;               // Colormap
        public nuint cursor;                 // Cursor
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XWindowAttributes
    {
        public int x, y;
        public int width, height;
        public int border_width;
        public int depth;
        public nint visual;                  // Visual*
        public nuint root;                   // Window
        public int c_class;                  // int (class is reserved in C#)
        public int bit_gravity;
        public int win_gravity;
        public int backing_store;
        public nuint backing_planes;
        public nuint backing_pixel;
        public int save_under;               // Bool
        public nuint colormap;               // Colormap
        public int map_installed;            // Bool
        public int map_state;
        public nint all_event_masks;         // long
        public nint your_event_mask;         // long
        public nint do_not_propagate_mask;   // long
        public int override_redirect;        // Bool
        public nint screen;                  // Screen*
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XVisualInfo
    {
        public nint visual;                  // Visual*
        public nuint visualid;               // VisualID
        public int screen;
        public int depth;
        public int c_class;
        public nuint red_mask;
        public nuint green_mask;
        public nuint blue_mask;
        public int colormap_size;
        public int bits_per_rgb;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XClassHint
    {
        public nint res_name;                // char*
        public nint res_class;               // char*
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XSizeHints
    {
        public nint flags;                   // long
        public int x, y;
        public int width, height;
        public int min_width, min_height;
        public int max_width, max_height;
        public int width_inc, height_inc;
        public int min_aspect_x, min_aspect_y;
        public int max_aspect_x, max_aspect_y;
        public int base_width, base_height;
        public int win_gravity;
    }

    // XSizeHints flag constants
    public static class XSizeHintsFlags
    {
        public const nint USPosition   = 1 << 0;
        public const nint USSize       = 1 << 1;
        public const nint PPosition    = 1 << 2;
        public const nint PSize        = 1 << 3;
        public const nint PMinSize     = 1 << 4;
        public const nint PMaxSize     = 1 << 5;
        public const nint PResizeInc   = 1 << 6;
        public const nint PAspect      = 1 << 7;
        public const nint PBaseSize    = 1 << 8;
        public const nint PWinGravity  = 1 << 9;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XWMHints
    {
        public nint flags;                   // long
        public int input;                    // Bool
        public int initial_state;
        public nint icon_pixmap;             // Pixmap
        public nuint icon_window;            // Window
        public int icon_x, icon_y;
        public nint icon_mask;               // Pixmap
        public nuint window_group;           // XID
    }

    // XWMHints flag constants
    public static class XWMHintsFlags
    {
        public const nint InputHint        = 1 << 0;
        public const nint StateHint        = 1 << 1;
        public const nint IconPixmapHint   = 1 << 2;
        public const nint IconWindowHint   = 1 << 3;
        public const nint IconPositionHint = 1 << 4;
        public const nint IconMaskHint     = 1 << 5;
        public const nint WindowGroupHint  = 1 << 6;
    }

    // XWMHints states
    public static class XWMHintsState
    {
        public const int WithdrawnState = 0;
        public const int NormalState    = 1;
        public const int IconicState    = 3;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XIMStyles
    {
        public ushort count_styles;
        public nint supported_styles;        // XIMStyle* (array)
    }

    // XrmValue for Xrm resource queries
    [StructLayout(LayoutKind.Sequential)]
    public struct XrmValue
    {
        public uint size;
        public nint addr;                    // XPointer (char*)
    }

    // XcursorImage for Xcursor
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct XcursorImage
    {
        public uint version;
        public uint size;
        public uint width;
        public uint height;
        public uint xhot;
        public uint yhot;
        public uint delay;
        public uint* pixels;                 // XcursorPixel* (uint32_t*)
    }

    // XIEventMask for XInput2
    [StructLayout(LayoutKind.Sequential)]
    public struct XIEventMask
    {
        public int deviceid;
        public int mask_len;
        public nint mask;                    // unsigned char*
    }

    // XkbStateRec for XkbGetState
    [StructLayout(LayoutKind.Sequential)]
    public struct XkbStateRec
    {
        public byte group;
        public byte locked_group;
        public ushort base_group;
        public ushort latched_group;
        public byte mods;
        public byte base_mods;
        public byte latched_mods;
        public byte locked_mods;
        public byte compat_state;
        public byte grab_mods;
        public byte compat_grab_mods;
        public byte lookup_mods;
        public byte compat_lookup_mods;
        public ushort ptr_buttons;
    }

    // -------------------------------------------------------------------
    //  _GLFWwindowX11 -> GlfwWindowX11
    //  Partial class -- base declaration (with handle) is in
    //  glx/glx_context.cs. This part adds all remaining fields.
    // -------------------------------------------------------------------

    public partial class GlfwWindowX11
    {
        public nuint colormap;               // Colormap
        public nuint handle;                 // Window
        public nuint parent;                 // Window
        public nint ic;                      // XIC
        internal nint icHandle;              // Managed window retained by XIM callback.

        public bool overrideRedirect;
        public bool iconified;
        public bool maximized;

        // Whether the visual supports framebuffer transparency
        public bool transparent;

        // Cached position and size used to filter out duplicate events
        public int width, height;
        public int xpos, ypos;

        // The last received cursor position, regardless of source
        public int lastCursorPosX, lastCursorPosY;
        // The last position the cursor was warped to by GLFW
        public int warpCursorPosX, warpCursorPosY;

        // The time of the last KeyPress event per keycode, for discarding
        // duplicate key events generated for some keys by ibus
        public nuint[] keyPressTimes = new nuint[256];   // Time[256]
    }

    // -------------------------------------------------------------------
    //  _GLFWlibraryX11 -> GlfwLibraryX11
    //  Partial class -- display, screen, errorCode are defined in
    //  glx/glx_context.cs. This part adds all remaining fields.
    // -------------------------------------------------------------------

    public unsafe partial class GlfwLibraryX11
    {
        public nint display;                 // Display*
        public int screen;
        public nuint root;                   // Window

        // System content scale
        public float contentScaleX, contentScaleY;
        // Helper window for IPC
        public nuint helperWindowHandle;     // Window
        // Invisible cursor for hidden cursor mode
        public nuint hiddenCursorHandle;     // Cursor
        // Context for mapping window XIDs to GlfwWindow references
        public int context;                  // XContext (int)
        // XIM input method
        public nint im;                      // XIM
        // The previous X error handler, to be restored later
        public nint errorHandler;            // XErrorHandler (function pointer)
        // Most recent error code received by X error handler
        public int errorCode;
        // Primary selection string
        public string? primarySelectionString;
        // Clipboard string
        public string? clipboardString;
        // Key name strings:  C is char keynames[GLFW_KEY_LAST+1][5]
        // In C# we store as string[] for convenience
        public string[] keynames = new string[GLFW.GLFW_KEY_LAST + 1];
        // X11 keycode to GLFW key LUT
        public short[] keycodes = new short[256];
        // GLFW key to X11 keycode LUT
        public short[] scancodes = new short[GLFW.GLFW_KEY_LAST + 1];
        // Where to place the cursor when re-enabled
        public double restoreCursorPosX, restoreCursorPosY;
        // The window whose disabled cursor mode is active
        public GlfwWindow? disabledCursorWindow;
        // Pipe for posting empty events
        public int emptyEventPipeRead;
        public int emptyEventPipeWrite;

        // ------------------------------------------------------------------
        //  Window manager atoms
        // ------------------------------------------------------------------
        public nuint NET_SUPPORTED;
        public nuint NET_SUPPORTING_WM_CHECK;
        public nuint WM_PROTOCOLS;
        public nuint WM_STATE;
        public nuint WM_DELETE_WINDOW;
        public nuint NET_WM_NAME;
        public nuint NET_WM_ICON_NAME;
        public nuint NET_WM_ICON;
        public nuint NET_WM_PID;
        public nuint NET_WM_PING;
        public nuint NET_WM_WINDOW_TYPE;
        public nuint NET_WM_WINDOW_TYPE_NORMAL;
        public nuint NET_WM_STATE;
        public nuint NET_WM_STATE_ABOVE;
        public nuint NET_WM_STATE_FULLSCREEN;
        public nuint NET_WM_STATE_MAXIMIZED_VERT;
        public nuint NET_WM_STATE_MAXIMIZED_HORZ;
        public nuint NET_WM_STATE_DEMANDS_ATTENTION;
        public nuint NET_WM_BYPASS_COMPOSITOR;
        public nuint NET_WM_FULLSCREEN_MONITORS;
        public nuint NET_WM_WINDOW_OPACITY;
        public nuint NET_WM_CM_Sx;
        public nuint NET_WORKAREA;
        public nuint NET_CURRENT_DESKTOP;
        public nuint NET_ACTIVE_WINDOW;
        public nuint NET_FRAME_EXTENTS;
        public nuint NET_REQUEST_FRAME_EXTENTS;
        public nuint MOTIF_WM_HINTS;

        // Xdnd (drag and drop) atoms
        public nuint XdndAware;
        public nuint XdndEnter;
        public nuint XdndPosition;
        public nuint XdndStatus;
        public nuint XdndActionCopy;
        public nuint XdndDrop;
        public nuint XdndFinished;
        public nuint XdndSelection;
        public nuint XdndTypeList;
        public nuint text_uri_list;

        // Selection (clipboard) atoms
        public nuint TARGETS;
        public nuint MULTIPLE;
        public nuint INCR;
        public nuint CLIPBOARD;
        public nuint PRIMARY;
        public nuint CLIPBOARD_MANAGER;
        public nuint SAVE_TARGETS;
        public nuint NULL_;
        public nuint UTF8_STRING;
        public nuint COMPOUND_STRING;
        public nuint ATOM_PAIR;
        public nuint GLFW_SELECTION;

        // ------------------------------------------------------------------
        //  xlib -- dynamically loaded libX11 functions
        // ------------------------------------------------------------------
        public XlibFunctions xlib = new();

        public unsafe class XlibFunctions
        {
            public nint handle;              // dlopen handle for libX11.so.6
            public bool utf8;                // Whether Xutf8* functions are available

            public delegate* unmanaged[Cdecl]<nint> AllocClassHint;                             // XClassHint*
            public delegate* unmanaged[Cdecl]<nint> AllocSizeHints;                             // XSizeHints*
            public delegate* unmanaged[Cdecl]<nint> AllocWMHints;                               // XWMHints*
            public delegate* unmanaged[Cdecl]<nint, nuint, nuint, nuint, int, int, byte*, int, int> ChangeProperty;
            public delegate* unmanaged[Cdecl]<nint, nuint, nuint, XSetWindowAttributes*, int> ChangeWindowAttributes;
            public delegate* unmanaged[Cdecl]<nint, XEvent*, nint, nint, int> CheckIfEvent;     // Bool(*predicate), XPointer
            public delegate* unmanaged[Cdecl]<nint, nuint, int, XEvent*, int> CheckTypedWindowEvent;
            public delegate* unmanaged[Cdecl]<nint, int> CloseDisplay;
            public delegate* unmanaged[Cdecl]<nint, int> CloseIM;                               // Status
            public delegate* unmanaged[Cdecl]<nint, nuint, nuint, nuint, nuint, nuint, int> ConvertSelection;
            public delegate* unmanaged[Cdecl]<nint, nuint, nint, int, nuint> CreateColormap;    // Colormap
            public delegate* unmanaged[Cdecl]<nint, uint, nuint> CreateFontCursor;              // Cursor
            // XCreateIC has varargs -- will be called through a managed wrapper
            public nint CreateIC_ptr;
            public delegate* unmanaged[Cdecl]<nint> CreateRegion;                               // Region
            public delegate* unmanaged[Cdecl]<nint, nuint, int, int, uint, uint, uint, int, uint, nint, nuint, XSetWindowAttributes*, nuint> CreateWindow;
            public delegate* unmanaged[Cdecl]<nint, nuint, nuint, int> DefineCursor;
            public delegate* unmanaged[Cdecl]<nint, nuint, int, int> DeleteContext;
            public delegate* unmanaged[Cdecl]<nint, nuint, nuint, int> DeleteProperty;
            public delegate* unmanaged[Cdecl]<nint, void> DestroyIC;
            public delegate* unmanaged[Cdecl]<nint, int> DestroyRegion;
            public delegate* unmanaged[Cdecl]<nint, nuint, int> DestroyWindow;
            public delegate* unmanaged[Cdecl]<nint, int*, int*, int> DisplayKeycodes;
            public delegate* unmanaged[Cdecl]<nint, int, int> EventsQueued;
            public delegate* unmanaged[Cdecl]<XEvent*, nuint, int> FilterEvent;                 // Bool
            public delegate* unmanaged[Cdecl]<nint, nuint, int, nint*, int> FindContext;
            public delegate* unmanaged[Cdecl]<nint, int> Flush;
            public delegate* unmanaged[Cdecl]<nint, int> Free;                                  // void* -> int
            public delegate* unmanaged[Cdecl]<nint, nuint, int> FreeColormap;
            public delegate* unmanaged[Cdecl]<nint, nuint, int> FreeCursor;
            public delegate* unmanaged[Cdecl]<nint, XGenericEventCookie*, void> FreeEventData;
            public delegate* unmanaged[Cdecl]<nint, int, byte*, int, int> GetErrorText;
            public delegate* unmanaged[Cdecl]<nint, XGenericEventCookie*, int> GetEventData;    // Bool
            // XGetICValues / XGetIMValues have varargs -- stored as raw pointers
            public nint GetICValues_ptr;
            public nint GetIMValues_ptr;
            public delegate* unmanaged[Cdecl]<nint, nuint*, int*, int> GetInputFocus;
            public delegate* unmanaged[Cdecl]<nint, byte, int, int*, nint> GetKeyboardMapping;  // KeySym*
            public delegate* unmanaged[Cdecl]<nint, int*, int*, int*, int*, int> GetScreenSaver;
            public delegate* unmanaged[Cdecl]<nint, nuint, nuint> GetSelectionOwner;            // Window
            public delegate* unmanaged[Cdecl]<nint, nint, XVisualInfo*, int*, nint> GetVisualInfo; // XVisualInfo*
            public delegate* unmanaged[Cdecl]<nint, nuint, XSizeHints*, nint*, int> GetWMNormalHints; // Status
            public delegate* unmanaged[Cdecl]<nint, nuint, XWindowAttributes*, int> GetWindowAttributes; // Status
            public delegate* unmanaged[Cdecl]<nint, nuint, nuint, nint, nint, int, nuint, nuint*, int*, nuint*, nuint*, byte**, int> GetWindowProperty;
            public delegate* unmanaged[Cdecl]<nint, nuint, int, uint, int, int, nuint, nuint, nuint, int> GrabPointer;
            public delegate* unmanaged[Cdecl]<nint, nuint, int, int> IconifyWindow;             // Status
            // XInitThreads
            public delegate* unmanaged[Cdecl]<int> InitThreads;                                 // Status
            public delegate* unmanaged[Cdecl]<nint, byte*, int, nuint> InternAtom;              // Atom
            public delegate* unmanaged[Cdecl]<XKeyEvent*, byte*, int, nuint*, nint, int> LookupString;
            public delegate* unmanaged[Cdecl]<nint, nuint, int> MapRaised;
            public delegate* unmanaged[Cdecl]<nint, nuint, int> MapWindow;
            public delegate* unmanaged[Cdecl]<nint, nuint, int, int, uint, uint, int> MoveResizeWindow;
            public delegate* unmanaged[Cdecl]<nint, nuint, int, int, int> MoveWindow;
            public delegate* unmanaged[Cdecl]<nint, XEvent*, int> NextEvent;
            public delegate* unmanaged[Cdecl]<nint, nint, byte*, byte*, nint> OpenIM;           // XIM
            public delegate* unmanaged[Cdecl]<nint, XEvent*, int> PeekEvent;
            public delegate* unmanaged[Cdecl]<nint, int> Pending;
            public delegate* unmanaged[Cdecl]<nint, byte*, int*, int*, int*, int> QueryExtension; // Bool
            public delegate* unmanaged[Cdecl]<nint, nuint, nuint*, nuint*, int*, int*, int*, int*, uint*, int> QueryPointer; // Bool
            public delegate* unmanaged[Cdecl]<nint, nuint, int> RaiseWindow;
            public delegate* unmanaged[Cdecl]<nint, nint, byte*, byte*, nint, nint, int> RegisterIMInstantiateCallback; // Bool
            public delegate* unmanaged[Cdecl]<nint, nuint, uint, uint, int> ResizeWindow;
            public delegate* unmanaged[Cdecl]<nint, nint> ResourceManagerString;                // char*
            public delegate* unmanaged[Cdecl]<nint, nuint, int, byte*, int> SaveContext;
            public delegate* unmanaged[Cdecl]<nint, nuint, nint, int> SelectInput;
            public delegate* unmanaged[Cdecl]<nint, nuint, int, nint, XEvent*, int> SendEvent;  // Status
            public delegate* unmanaged[Cdecl]<nint, nuint, XClassHint*, int> SetClassHint;
            public delegate* unmanaged[Cdecl]<nint, nint> SetErrorHandler;                      // XErrorHandler -> XErrorHandler
            public delegate* unmanaged[Cdecl]<nint, void> SetICFocus;
            // XSetIMValues has varargs
            public nint SetIMValues_ptr;
            public delegate* unmanaged[Cdecl]<nint, nuint, int, nuint, int> SetInputFocus;
            public delegate* unmanaged[Cdecl]<byte*, nint> SetLocaleModifiers;                  // char*
            public delegate* unmanaged[Cdecl]<nint, int, int, int, int, int> SetScreenSaver;
            public delegate* unmanaged[Cdecl]<nint, nuint, nuint, nuint, int> SetSelectionOwner;
            public delegate* unmanaged[Cdecl]<nint, nuint, XWMHints*, int> SetWMHints;
            public delegate* unmanaged[Cdecl]<nint, nuint, XSizeHints*, void> SetWMNormalHints;
            public delegate* unmanaged[Cdecl]<nint, nuint, nuint*, int, int> SetWMProtocols;    // Status
            public delegate* unmanaged[Cdecl]<int> SupportsLocale;                              // Bool
            public delegate* unmanaged[Cdecl]<nint, int, int> Sync;
            public delegate* unmanaged[Cdecl]<nint, nuint, nuint, int, int, int*, int*, nuint*, int> TranslateCoordinates; // Bool
            public delegate* unmanaged[Cdecl]<nint, nuint, int> UndefineCursor;
            public delegate* unmanaged[Cdecl]<nint, nuint, int> UngrabPointer;
            public delegate* unmanaged[Cdecl]<nint, nuint, int> UnmapWindow;
            public delegate* unmanaged[Cdecl]<nint, void> UnsetICFocus;
            public delegate* unmanaged[Cdecl]<nint, nuint> VisualIDFromVisual;                  // VisualID
            public delegate* unmanaged[Cdecl]<nint, nuint, nuint, int, int, uint, uint, int, int, int> WarpPointer;
            public delegate* unmanaged[Cdecl]<nint, nint, byte*, byte*, nint, nint, int> UnregisterIMInstantiateCallback; // Bool
            public delegate* unmanaged[Cdecl]<nint, XKeyEvent*, byte*, int, nuint*, int*, int> utf8LookupString;
            public delegate* unmanaged[Cdecl]<nint, nuint, byte*, byte*, byte**, int, XSizeHints*, XWMHints*, XClassHint*, void> utf8SetWMProperties;

            // --- Display accessor functions (replacing DllImport macros) ---
            public delegate* unmanaged[Cdecl]<byte*, nint> OpenDisplay;                             // Display*
            public delegate* unmanaged[Cdecl]<nint, int> DefaultScreen;
            public delegate* unmanaged[Cdecl]<nint, int, nuint> RootWindow;                         // Window
            public delegate* unmanaged[Cdecl]<nint, int, nint> DefaultVisual;                       // Visual*
            public delegate* unmanaged[Cdecl]<nint, int, int> DefaultDepth;
            public delegate* unmanaged[Cdecl]<nint, int> ConnectionNumber;
            public delegate* unmanaged[Cdecl]<nint, int> QLength;
            public delegate* unmanaged[Cdecl]<nint, int, int> DisplayWidth;
            public delegate* unmanaged[Cdecl]<nint, int, int> DisplayHeight;
            public delegate* unmanaged[Cdecl]<nint, int, int> DisplayWidthMM;
            public delegate* unmanaged[Cdecl]<nint, int, int> DisplayHeightMM;
        }

        // ------------------------------------------------------------------
        //  xrm -- Xrm (X Resource Manager) functions from libX11
        // ------------------------------------------------------------------
        public XrmFunctions xrm = new();

        public unsafe class XrmFunctions
        {
            public delegate* unmanaged[Cdecl]<void> Initialize;
            public delegate* unmanaged[Cdecl]<nint, void> DestroyDatabase;                     // XrmDatabase
            public delegate* unmanaged[Cdecl]<nint, byte*, byte*, byte**, XrmValue*, int> GetResource; // Bool
            public delegate* unmanaged[Cdecl]<byte*, nint> GetStringDatabase;                  // XrmDatabase
            public delegate* unmanaged[Cdecl]<int> UniqueQuark;                                // XrmQuark
        }

        // ------------------------------------------------------------------
        //  randr -- XRandR extension
        // ------------------------------------------------------------------
        public RandrFunctions randr = new();

        public unsafe class RandrFunctions
        {
            public bool available;
            public nint handle;              // dlopen handle
            public int eventBase;
            public int errorBase;
            public int major;
            public int minor;
            public bool gammaBroken;
            public bool monitorBroken;

            public delegate* unmanaged[Cdecl]<int, nint> AllocGamma;                           // XRRCrtcGamma*
            public delegate* unmanaged[Cdecl]<nint, void> FreeCrtcInfo;
            public delegate* unmanaged[Cdecl]<nint, void> FreeGamma;                           // XRRCrtcGamma*
            public delegate* unmanaged[Cdecl]<nint, void> FreeOutputInfo;
            public delegate* unmanaged[Cdecl]<nint, void> FreeScreenResources;
            public delegate* unmanaged[Cdecl]<nint, nuint, nint> GetCrtcGamma;                 // XRRCrtcGamma*
            public delegate* unmanaged[Cdecl]<nint, nuint, int> GetCrtcGammaSize;
            public delegate* unmanaged[Cdecl]<nint, nint, nuint, nint> GetCrtcInfo;            // XRRCrtcInfo*
            public delegate* unmanaged[Cdecl]<nint, nint, nuint, nint> GetOutputInfo;          // XRROutputInfo*
            public delegate* unmanaged[Cdecl]<nint, nuint, nuint> GetOutputPrimary;            // RROutput
            public delegate* unmanaged[Cdecl]<nint, nuint, nint> GetScreenResourcesCurrent;    // XRRScreenResources*
            public delegate* unmanaged[Cdecl]<nint, int*, int*, int> QueryExtension;           // Bool
            public delegate* unmanaged[Cdecl]<nint, int*, int*, int> QueryVersion;             // Status
            public delegate* unmanaged[Cdecl]<nint, nuint, int, void> SelectInput;
            public delegate* unmanaged[Cdecl]<nint, nint, nuint, nuint, int, int, nuint, ushort, nuint*, int, int> SetCrtcConfig; // Status
            public delegate* unmanaged[Cdecl]<nint, nuint, nint, void> SetCrtcGamma;           // XRRCrtcGamma*
            public delegate* unmanaged[Cdecl]<XEvent*, int> UpdateConfiguration;
        }

        // ------------------------------------------------------------------
        //  xkb -- Xkb extension
        // ------------------------------------------------------------------
        public XkbFunctions xkb = new();

        public unsafe class XkbFunctions
        {
            public bool available;
            public bool detectable;
            public int majorOpcode;
            public int eventBase;
            public int errorBase;
            public int major;
            public int minor;
            public uint group;

            public delegate* unmanaged[Cdecl]<nint, uint, int, void> FreeKeyboard;             // XkbDescPtr
            public delegate* unmanaged[Cdecl]<nint, uint, int, void> FreeNames;                // XkbDescPtr
            public delegate* unmanaged[Cdecl]<nint, uint, uint, nint> GetMap;                  // XkbDescPtr
            public delegate* unmanaged[Cdecl]<nint, uint, nint, int> GetNames;                 // Status
            public delegate* unmanaged[Cdecl]<nint, uint, XkbStateRec*, int> GetState;         // Status
            public delegate* unmanaged[Cdecl]<nint, byte, int, int, nuint> KeycodeToKeysym;    // KeySym
            public delegate* unmanaged[Cdecl]<nint, int*, int*, int*, int*, int*, int> QueryExtension; // Bool
            public delegate* unmanaged[Cdecl]<nint, uint, uint, nuint, nuint, int> SelectEventDetails; // Bool
            public delegate* unmanaged[Cdecl]<nint, int, int*, int> SetDetectableAutoRepeat;   // Bool
        }

        // ------------------------------------------------------------------
        //  saver -- screen saver state
        // ------------------------------------------------------------------
        public SaverState saver = new();

        public class SaverState
        {
            public int count;
            public int timeout;
            public int interval;
            public int blanking;
            public int exposure;
        }

        // ------------------------------------------------------------------
        //  xdnd -- drag and drop state
        // ------------------------------------------------------------------
        public XdndState xdnd = new();

        public class XdndState
        {
            public int version;
            public nuint source;             // Window
            public nuint format;             // Atom
        }

        // ------------------------------------------------------------------
        //  xcursor -- Xcursor library
        // ------------------------------------------------------------------
        public XcursorFunctions xcursor = new();

        public unsafe class XcursorFunctions
        {
            public nint handle;              // dlopen handle

            public delegate* unmanaged[Cdecl]<int, int, nint> ImageCreate;                     // XcursorImage*
            public delegate* unmanaged[Cdecl]<nint, void> ImageDestroy;                        // XcursorImage*
            public delegate* unmanaged[Cdecl]<nint, nint, nuint> ImageLoadCursor;              // Cursor
            public delegate* unmanaged[Cdecl]<nint, nint> GetTheme;                            // char*
            public delegate* unmanaged[Cdecl]<nint, int> GetDefaultSize;
            public delegate* unmanaged[Cdecl]<byte*, byte*, int, nint> LibraryLoadImage;       // XcursorImage*
        }

        // ------------------------------------------------------------------
        //  xinerama -- Xinerama extension
        // ------------------------------------------------------------------
        public XineramaFunctions xinerama = new();

        public unsafe class XineramaFunctions
        {
            public bool available;
            public nint handle;              // dlopen handle
            public int major;
            public int minor;

            public delegate* unmanaged[Cdecl]<nint, int> IsActive;                             // Bool
            public delegate* unmanaged[Cdecl]<nint, int*, int*, int> QueryExtension;           // Bool
            public delegate* unmanaged[Cdecl]<nint, int*, nint> QueryScreens;                  // XineramaScreenInfo*
        }

        // ------------------------------------------------------------------
        //  x11xcb -- X11-XCB bridge
        // ------------------------------------------------------------------
        public X11XcbFunctions x11xcb = new();

        public unsafe class X11XcbFunctions
        {
            public nint handle;              // dlopen handle

            public delegate* unmanaged[Cdecl]<nint, nint> GetXCBConnection;                    // xcb_connection_t*
        }

        // ------------------------------------------------------------------
        //  vidmode -- XF86VidMode extension
        // ------------------------------------------------------------------
        public VidModeFunctions vidmode = new();

        public unsafe class VidModeFunctions
        {
            public bool available;
            public nint handle;              // dlopen handle
            public int eventBase;
            public int errorBase;

            public delegate* unmanaged[Cdecl]<nint, int*, int*, int> QueryExtension;           // Bool
            public delegate* unmanaged[Cdecl]<nint, int, int, ushort*, ushort*, ushort*, int> GetGammaRamp; // Bool
            public delegate* unmanaged[Cdecl]<nint, int, int, ushort*, ushort*, ushort*, int> SetGammaRamp; // Bool
            public delegate* unmanaged[Cdecl]<nint, int, int*, int> GetGammaRampSize;          // Bool
        }

        // ------------------------------------------------------------------
        //  xi -- XInput2 extension
        // ------------------------------------------------------------------
        public XiFunctions xi = new();

        public unsafe class XiFunctions
        {
            public bool available;
            public nint handle;              // dlopen handle
            public int majorOpcode;
            public int eventBase;
            public int errorBase;
            public int major;
            public int minor;

            public delegate* unmanaged[Cdecl]<nint, int*, int*, int> QueryVersion;             // Status
            public delegate* unmanaged[Cdecl]<nint, nuint, XIEventMask*, int, int> SelectEvents;
        }

        // ------------------------------------------------------------------
        //  xrender -- XRender extension
        // ------------------------------------------------------------------
        public XRenderFunctions xrender = new();

        public unsafe class XRenderFunctions
        {
            public bool available;
            public nint handle;              // dlopen handle
            public int major;
            public int minor;
            public int eventBase;
            public int errorBase;

            public delegate* unmanaged[Cdecl]<nint, int*, int*, int> QueryExtension;           // Bool
            public delegate* unmanaged[Cdecl]<nint, int*, int*, int> QueryVersion;             // Status
            public delegate* unmanaged[Cdecl]<nint, nint, nint> FindVisualFormat;              // XRenderPictFormat*
        }

        // ------------------------------------------------------------------
        //  xshape -- XShape extension
        // ------------------------------------------------------------------
        public XShapeFunctions xshape = new();

        public unsafe class XShapeFunctions
        {
            public bool available;
            public nint handle;              // dlopen handle
            public int major;
            public int minor;
            public int eventBase;
            public int errorBase;

            public delegate* unmanaged[Cdecl]<nint, int*, int*, int> QueryExtension;           // Bool
            public delegate* unmanaged[Cdecl]<nint, nuint, int, int, int, nint, int, void> ShapeCombineRegion;
            public delegate* unmanaged[Cdecl]<nint, int*, int*, int> QueryVersion;             // Status
            public delegate* unmanaged[Cdecl]<nint, nuint, int, int, int, nuint, int, void> ShapeCombineMask; // Pixmap
        }

        // ------------------------------------------------------------------
        //  libc -- dynamically loaded libc functions
        // ------------------------------------------------------------------
        public LibcFunctions libc = new();

        public unsafe class LibcFunctions
        {
            public nint handle;              // dlopen handle for libc.so.6

            public delegate* unmanaged[Cdecl]<int*, int> pipe;
            public delegate* unmanaged[Cdecl]<int, int, int, int> fcntl;
            public delegate* unmanaged[Cdecl]<int, int> close;
            public delegate* unmanaged[Cdecl]<int, byte*, nuint, nint> write;
            public delegate* unmanaged[Cdecl]<int, byte*, nuint, nint> read;
            public delegate* unmanaged[Cdecl]<int> getpid;
            public delegate* unmanaged[Cdecl]<PollFd*, nuint, int, int> poll;
            public delegate* unmanaged[Cdecl]<int*> __errno_location;

            /// <summary>
            /// Returns the current errno value. Since we use function pointers
            /// instead of DllImport (which would capture errno via SetLastError),
            /// we call __errno_location() directly.
            /// </summary>
            public int errno
            {
                get
                {
                    if (__errno_location != null)
                        return *__errno_location();
                    return 0;
                }
            }
        }
    }

    // -------------------------------------------------------------------
    //  _GLFWmonitorX11 -> GlfwMonitorX11
    // -------------------------------------------------------------------

    public class GlfwMonitorX11
    {
        public nuint output;                 // RROutput (XID)
        public nuint crtc;                   // RRCrtc (XID)
        public nuint oldMode;                // RRMode (XID)

        // Index of corresponding Xinerama screen,
        // for EWMH full screen window placement
        public int index;
    }

    // -------------------------------------------------------------------
    //  _GLFWcursorX11 -> GlfwCursorX11
    // -------------------------------------------------------------------

    public class GlfwCursorX11
    {
        public nuint handle;                 // Cursor (XID)
    }


    // -------------------------------------------------------------------
    //  XIRawEvent -- XInput2 raw event structure
    // -------------------------------------------------------------------

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct XIRawEvent
    {
        public int type;
        public nuint serial;
        public int send_event;
        public nint display;
        public int extension;
        public int evtype;
        public nuint time;
        public int deviceid;
        public int sourceid;
        public int detail;
        public int flags;
        // XIValuatorState valuators
        public int valuators_mask_len;
        public byte* valuators_mask;
        public double* valuators_values;
        // raw values
        public double* raw_values;
    }

    // -------------------------------------------------------------------
    //  X11 event type constants
    // -------------------------------------------------------------------

    public static class X11Events
    {
        public const int KeyPress            = 2;
        public const int KeyRelease          = 3;
        public const int ButtonPress         = 4;
        public const int ButtonRelease       = 5;
        public const int MotionNotify        = 6;
        public const int EnterNotify         = 7;
        public const int LeaveNotify         = 8;
        public const int FocusIn             = 9;
        public const int FocusOut            = 10;
        public const int KeymapNotify        = 11;
        public const int Expose              = 12;
        public const int GraphicsExpose      = 13;
        public const int NoExpose            = 14;
        public const int VisibilityNotify    = 15;
        public const int CreateNotify        = 16;
        public const int DestroyNotify       = 17;
        public const int UnmapNotify         = 18;
        public const int MapNotify           = 19;
        public const int MapRequest          = 20;
        public const int ReparentNotify      = 21;
        public const int ConfigureNotify     = 22;
        public const int ConfigureRequest    = 23;
        public const int GravityNotify       = 24;
        public const int ResizeRequest       = 25;
        public const int CirculateNotify     = 26;
        public const int CirculateRequest    = 27;
        public const int PropertyNotify      = 28;
        public const int SelectionClear      = 29;
        public const int SelectionRequest    = 30;
        public const int SelectionNotify     = 31;
        public const int ColormapNotify      = 32;
        public const int ClientMessage       = 33;
        public const int MappingNotify       = 34;
        public const int GenericEvent        = 35;
        public const int LASTEvent           = 36;
    }

    // -------------------------------------------------------------------
    //  X11 misc constants used by GLFW
    // -------------------------------------------------------------------

    public static class X11Constants
    {
        // AllocNone/AllocAll for XCreateColormap
        public const int AllocNone = 0;
        public const int AllocAll  = 1;

        // Window class for XCreateWindow
        public const uint InputOutput = 1;
        public const uint InputOnly   = 2;

        // CW* attribute masks for XCreateWindow / XChangeWindowAttributes
        public const nuint CWBackPixmap       = 1 << 0;
        public const nuint CWBackPixel        = 1 << 1;
        public const nuint CWBorderPixmap     = 1 << 2;
        public const nuint CWBorderPixel      = 1 << 3;
        public const nuint CWBitGravity       = 1 << 4;
        public const nuint CWWinGravity       = 1 << 5;
        public const nuint CWBackingStore     = 1 << 6;
        public const nuint CWBackingPlanes    = 1 << 7;
        public const nuint CWBackingPixel     = 1 << 8;
        public const nuint CWOverrideRedirect = 1 << 9;
        public const nuint CWSaveUnder        = 1 << 10;
        public const nuint CWEventMask        = 1 << 11;
        public const nuint CWDontPropagate    = 1 << 12;
        public const nuint CWColormap         = 1 << 13;
        public const nuint CWCursor           = 1 << 14;

        // Event masks for XSelectInput
        public const nint KeyPressMask              = 1 << 0;
        public const nint KeyReleaseMask            = 1 << 1;
        public const nint ButtonPressMask           = 1 << 2;
        public const nint ButtonReleaseMask         = 1 << 3;
        public const nint EnterWindowMask           = 1 << 4;
        public const nint LeaveWindowMask           = 1 << 5;
        public const nint PointerMotionMask         = 1 << 6;
        public const nint PointerMotionHintMask     = 1 << 7;
        public const nint Button1MotionMask         = 1 << 8;
        public const nint Button2MotionMask         = 1 << 9;
        public const nint Button3MotionMask         = 1 << 10;
        public const nint Button4MotionMask         = 1 << 11;
        public const nint Button5MotionMask         = 1 << 12;
        public const nint ButtonMotionMask          = 1 << 13;
        public const nint KeymapStateMask           = 1 << 14;
        public const nint ExposureMask              = 1 << 15;
        public const nint VisibilityChangeMask      = 1 << 16;
        public const nint StructureNotifyMask       = 1 << 17;
        public const nint ResizeRedirectMask        = 1 << 18;
        public const nint SubstructureNotifyMask    = 1 << 19;
        public const nint SubstructureRedirectMask  = 1 << 20;
        public const nint FocusChangeMask           = 1 << 21;
        public const nint PropertyChangeMask        = 1 << 22;
        public const nint ColormapChangeMask        = 1 << 23;
        public const nint OwnerGrabButtonMask       = 1 << 24;

        // Grab modes
        public const int GrabModeSync  = 0;
        public const int GrabModeAsync = 1;

        // Grab status
        public const int GrabSuccess     = 0;

        // Focus revert modes
        public const int RevertToNone         = 0;
        public const int RevertToPointerRoot  = 1;
        public const int RevertToParent       = 2;

        // Special windows
        public const nuint None_         = 0;
        public const nuint PointerRoot   = 1;

        // Atoms (predefined)
        public const nuint XA_PRIMARY    = 1;
        public const nuint XA_ATOM       = 4;
        public const nuint XA_STRING     = 31;
        public const nuint XA_CARDINAL   = 6;
        public const nuint XA_WINDOW     = 33;

        // PropertyNotify state
        public const int PropertyNewValue = 0;
        public const int PropertyDelete   = 1;

        // XEventsQueued modes
        public const int QueuedAlready       = 0;
        public const int QueuedAfterReading  = 1;
        public const int QueuedAfterFlush    = 2;

        // CurrentTime
        public const nuint CurrentTime = 0;

        // Shape extension constants
        public const int ShapeBounding  = 0;
        public const int ShapeClip      = 1;
        public const int ShapeInput     = 2;
        public const int ShapeSet       = 0;
        public const int ShapeUnion     = 1;
        public const int ShapeIntersect = 2;
        public const int ShapeSubtract  = 3;
        public const int ShapeInvert    = 4;

        // VisualInfo mask for XGetVisualInfo
        public const nint VisualIDMask    = 0x01;
        public const nint VisualScreenMask = 0x02;

        // XRandR notify event
        public const int RRScreenChangeNotify = 0;
        public const int RRNotify             = 1;
        public const int RRNotify_CrtcChange  = 0;
        public const int RRNotify_OutputChange = 1;

        // XRandR select input mask
        public const int RRScreenChangeNotifyMask = 1 << 0;
        public const int RRCrtcChangeNotifyMask   = 1 << 1;
        public const int RROutputChangeNotifyMask = 1 << 2;
        public const int RROutputPropertyNotifyMask = 1 << 3;

        // XInput2 event types
        public const int XI_RawMotion     = 17;
        public const int XI_LASTEVENT     = XI_RawMotion;
        public const int XIAllMasterDevices = 1;

        // XInput2 masks
        public static void XISetMask(Span<byte> mask, int evt)
        {
            mask[evt >> 3] |= (byte)(1 << (evt & 7));
        }
    }
}
