// Ported from glfw/src/x11_platform.h -- GLFW 3.6 X11 native library loading
//
// Loads libX11.so.6, libXrandr.so.2, libXinerama.so.1, libXcursor.so.1,
// libXi.so.6, libXrender.so.1, libXss.so.1, libX11-xcb.so.1,
// libXxf86vm.so.1, and libXext.so.6 at runtime.
//
// Function pointers are resolved into the GlfwLibraryX11 sub-objects,
// matching the C code's pattern of storing them in _glfw.x11.xlib.*,
// _glfw.x11.randr.*, etc.
//
// Uses NativeLibrary.Load / TryGetExport and unmanaged[Cdecl] function
// pointers, following the same pattern as tvgGl.cs.

using System.Runtime.InteropServices;

namespace Glfw
{
    public static unsafe class X11Native
    {
        // ===============================================================
        //  Helper: load a symbol or return null
        // ===============================================================

        private static nint TryLoad(nint lib, string name)
        {
            NativeLibrary.TryGetExport(lib, name, out var ptr);
            return ptr;
        }

        // ===============================================================
        //  LoadXlib — loads libX11.so.6 and populates xlib + xrm
        // ===============================================================

        public static bool LoadXlib(GlfwLibraryX11 x11)
        {
            if (!NativeLibrary.TryLoad("libX11.so.6", out var lib))
                return false;

            x11.xlib.handle = lib;

            nint p;

            // Xutf8 support detection
            x11.xlib.utf8 = NativeLibrary.TryGetExport(lib, "Xutf8LookupString", out _);

            // --- Core Xlib functions ---
            p = TryLoad(lib, "XAllocClassHint");
            x11.xlib.AllocClassHint = (delegate* unmanaged[Cdecl]<nint>)p;

            p = TryLoad(lib, "XAllocSizeHints");
            x11.xlib.AllocSizeHints = (delegate* unmanaged[Cdecl]<nint>)p;

            p = TryLoad(lib, "XAllocWMHints");
            x11.xlib.AllocWMHints = (delegate* unmanaged[Cdecl]<nint>)p;

            p = TryLoad(lib, "XChangeProperty");
            x11.xlib.ChangeProperty = (delegate* unmanaged[Cdecl]<nint, nuint, nuint, nuint, int, int, byte*, int, int>)p;

            p = TryLoad(lib, "XChangeWindowAttributes");
            x11.xlib.ChangeWindowAttributes = (delegate* unmanaged[Cdecl]<nint, nuint, nuint, XSetWindowAttributes*, int>)p;

            p = TryLoad(lib, "XCheckIfEvent");
            x11.xlib.CheckIfEvent = (delegate* unmanaged[Cdecl]<nint, XEvent*, nint, nint, int>)p;

            p = TryLoad(lib, "XCheckTypedWindowEvent");
            x11.xlib.CheckTypedWindowEvent = (delegate* unmanaged[Cdecl]<nint, nuint, int, XEvent*, int>)p;

            p = TryLoad(lib, "XCloseDisplay");
            x11.xlib.CloseDisplay = (delegate* unmanaged[Cdecl]<nint, int>)p;

            p = TryLoad(lib, "XCloseIM");
            x11.xlib.CloseIM = (delegate* unmanaged[Cdecl]<nint, int>)p;

            p = TryLoad(lib, "XConvertSelection");
            x11.xlib.ConvertSelection = (delegate* unmanaged[Cdecl]<nint, nuint, nuint, nuint, nuint, nuint, int>)p;

            p = TryLoad(lib, "XCreateColormap");
            x11.xlib.CreateColormap = (delegate* unmanaged[Cdecl]<nint, nuint, nint, int, nuint>)p;

            p = TryLoad(lib, "XCreateFontCursor");
            x11.xlib.CreateFontCursor = (delegate* unmanaged[Cdecl]<nint, uint, nuint>)p;

            // XCreateIC has varargs -- store the raw function pointer
            x11.xlib.CreateIC_ptr = TryLoad(lib, "XCreateIC");

            p = TryLoad(lib, "XCreateRegion");
            x11.xlib.CreateRegion = (delegate* unmanaged[Cdecl]<nint>)p;

            p = TryLoad(lib, "XCreateWindow");
            x11.xlib.CreateWindow = (delegate* unmanaged[Cdecl]<nint, nuint, int, int, uint, uint, uint, int, uint, nint, nuint, XSetWindowAttributes*, nuint>)p;

            p = TryLoad(lib, "XDefineCursor");
            x11.xlib.DefineCursor = (delegate* unmanaged[Cdecl]<nint, nuint, nuint, int>)p;

            p = TryLoad(lib, "XDeleteContext");
            x11.xlib.DeleteContext = (delegate* unmanaged[Cdecl]<nint, nuint, int, int>)p;

            p = TryLoad(lib, "XDeleteProperty");
            x11.xlib.DeleteProperty = (delegate* unmanaged[Cdecl]<nint, nuint, nuint, int>)p;

            p = TryLoad(lib, "XDestroyIC");
            x11.xlib.DestroyIC = (delegate* unmanaged[Cdecl]<nint, void>)p;

            p = TryLoad(lib, "XDestroyRegion");
            x11.xlib.DestroyRegion = (delegate* unmanaged[Cdecl]<nint, int>)p;

            p = TryLoad(lib, "XDestroyWindow");
            x11.xlib.DestroyWindow = (delegate* unmanaged[Cdecl]<nint, nuint, int>)p;

            p = TryLoad(lib, "XDisplayKeycodes");
            x11.xlib.DisplayKeycodes = (delegate* unmanaged[Cdecl]<nint, int*, int*, int>)p;

            p = TryLoad(lib, "XEventsQueued");
            x11.xlib.EventsQueued = (delegate* unmanaged[Cdecl]<nint, int, int>)p;

            p = TryLoad(lib, "XFilterEvent");
            x11.xlib.FilterEvent = (delegate* unmanaged[Cdecl]<XEvent*, nuint, int>)p;

            p = TryLoad(lib, "XFindContext");
            x11.xlib.FindContext = (delegate* unmanaged[Cdecl]<nint, nuint, int, nint*, int>)p;

            p = TryLoad(lib, "XFlush");
            x11.xlib.Flush = (delegate* unmanaged[Cdecl]<nint, int>)p;

            p = TryLoad(lib, "XFree");
            x11.xlib.Free = (delegate* unmanaged[Cdecl]<nint, int>)p;

            p = TryLoad(lib, "XFreeColormap");
            x11.xlib.FreeColormap = (delegate* unmanaged[Cdecl]<nint, nuint, int>)p;

            p = TryLoad(lib, "XFreeCursor");
            x11.xlib.FreeCursor = (delegate* unmanaged[Cdecl]<nint, nuint, int>)p;

            p = TryLoad(lib, "XFreeEventData");
            x11.xlib.FreeEventData = (delegate* unmanaged[Cdecl]<nint, XGenericEventCookie*, void>)p;

            p = TryLoad(lib, "XGetErrorText");
            x11.xlib.GetErrorText = (delegate* unmanaged[Cdecl]<nint, int, byte*, int, int>)p;

            p = TryLoad(lib, "XGetEventData");
            x11.xlib.GetEventData = (delegate* unmanaged[Cdecl]<nint, XGenericEventCookie*, int>)p;

            // XGetICValues / XGetIMValues have varargs
            x11.xlib.GetICValues_ptr = TryLoad(lib, "XGetICValues");
            x11.xlib.GetIMValues_ptr = TryLoad(lib, "XGetIMValues");

            p = TryLoad(lib, "XGetInputFocus");
            x11.xlib.GetInputFocus = (delegate* unmanaged[Cdecl]<nint, nuint*, int*, int>)p;

            p = TryLoad(lib, "XGetKeyboardMapping");
            x11.xlib.GetKeyboardMapping = (delegate* unmanaged[Cdecl]<nint, byte, int, int*, nint>)p;

            p = TryLoad(lib, "XGetScreenSaver");
            x11.xlib.GetScreenSaver = (delegate* unmanaged[Cdecl]<nint, int*, int*, int*, int*, int>)p;

            p = TryLoad(lib, "XGetSelectionOwner");
            x11.xlib.GetSelectionOwner = (delegate* unmanaged[Cdecl]<nint, nuint, nuint>)p;

            p = TryLoad(lib, "XGetVisualInfo");
            x11.xlib.GetVisualInfo = (delegate* unmanaged[Cdecl]<nint, nint, XVisualInfo*, int*, nint>)p;

            p = TryLoad(lib, "XGetWMNormalHints");
            x11.xlib.GetWMNormalHints = (delegate* unmanaged[Cdecl]<nint, nuint, XSizeHints*, nint*, int>)p;

            p = TryLoad(lib, "XGetWindowAttributes");
            x11.xlib.GetWindowAttributes = (delegate* unmanaged[Cdecl]<nint, nuint, XWindowAttributes*, int>)p;

            p = TryLoad(lib, "XGetWindowProperty");
            x11.xlib.GetWindowProperty = (delegate* unmanaged[Cdecl]<nint, nuint, nuint, nint, nint, int, nuint, nuint*, int*, nuint*, nuint*, byte**, int>)p;

            p = TryLoad(lib, "XGrabPointer");
            x11.xlib.GrabPointer = (delegate* unmanaged[Cdecl]<nint, nuint, int, uint, int, int, nuint, nuint, nuint, int>)p;

            p = TryLoad(lib, "XIconifyWindow");
            x11.xlib.IconifyWindow = (delegate* unmanaged[Cdecl]<nint, nuint, int, int>)p;

            p = TryLoad(lib, "XInitThreads");
            x11.xlib.InitThreads = (delegate* unmanaged[Cdecl]<int>)p;

            p = TryLoad(lib, "XInternAtom");
            x11.xlib.InternAtom = (delegate* unmanaged[Cdecl]<nint, byte*, int, nuint>)p;

            p = TryLoad(lib, "XLookupString");
            x11.xlib.LookupString = (delegate* unmanaged[Cdecl]<XKeyEvent*, byte*, int, nuint*, nint, int>)p;

            p = TryLoad(lib, "XMapRaised");
            x11.xlib.MapRaised = (delegate* unmanaged[Cdecl]<nint, nuint, int>)p;

            p = TryLoad(lib, "XMapWindow");
            x11.xlib.MapWindow = (delegate* unmanaged[Cdecl]<nint, nuint, int>)p;

            p = TryLoad(lib, "XMoveResizeWindow");
            x11.xlib.MoveResizeWindow = (delegate* unmanaged[Cdecl]<nint, nuint, int, int, uint, uint, int>)p;

            p = TryLoad(lib, "XMoveWindow");
            x11.xlib.MoveWindow = (delegate* unmanaged[Cdecl]<nint, nuint, int, int, int>)p;

            p = TryLoad(lib, "XNextEvent");
            x11.xlib.NextEvent = (delegate* unmanaged[Cdecl]<nint, XEvent*, int>)p;

            p = TryLoad(lib, "XOpenIM");
            x11.xlib.OpenIM = (delegate* unmanaged[Cdecl]<nint, nint, byte*, byte*, nint>)p;

            p = TryLoad(lib, "XPeekEvent");
            x11.xlib.PeekEvent = (delegate* unmanaged[Cdecl]<nint, XEvent*, int>)p;

            p = TryLoad(lib, "XPending");
            x11.xlib.Pending = (delegate* unmanaged[Cdecl]<nint, int>)p;

            p = TryLoad(lib, "XQueryExtension");
            x11.xlib.QueryExtension = (delegate* unmanaged[Cdecl]<nint, byte*, int*, int*, int*, int>)p;

            p = TryLoad(lib, "XQueryPointer");
            x11.xlib.QueryPointer = (delegate* unmanaged[Cdecl]<nint, nuint, nuint*, nuint*, int*, int*, int*, int*, uint*, int>)p;

            p = TryLoad(lib, "XRaiseWindow");
            x11.xlib.RaiseWindow = (delegate* unmanaged[Cdecl]<nint, nuint, int>)p;

            p = TryLoad(lib, "XRegisterIMInstantiateCallback");
            x11.xlib.RegisterIMInstantiateCallback = (delegate* unmanaged[Cdecl]<nint, nint, byte*, byte*, nint, nint, int>)p;

            p = TryLoad(lib, "XResizeWindow");
            x11.xlib.ResizeWindow = (delegate* unmanaged[Cdecl]<nint, nuint, uint, uint, int>)p;

            p = TryLoad(lib, "XResourceManagerString");
            x11.xlib.ResourceManagerString = (delegate* unmanaged[Cdecl]<nint, nint>)p;

            p = TryLoad(lib, "XSaveContext");
            x11.xlib.SaveContext = (delegate* unmanaged[Cdecl]<nint, nuint, int, byte*, int>)p;

            p = TryLoad(lib, "XSelectInput");
            x11.xlib.SelectInput = (delegate* unmanaged[Cdecl]<nint, nuint, nint, int>)p;

            p = TryLoad(lib, "XSendEvent");
            x11.xlib.SendEvent = (delegate* unmanaged[Cdecl]<nint, nuint, int, nint, XEvent*, int>)p;

            p = TryLoad(lib, "XSetClassHint");
            x11.xlib.SetClassHint = (delegate* unmanaged[Cdecl]<nint, nuint, XClassHint*, int>)p;

            p = TryLoad(lib, "XSetErrorHandler");
            x11.xlib.SetErrorHandler = (delegate* unmanaged[Cdecl]<nint, nint>)p;

            p = TryLoad(lib, "XSetICFocus");
            x11.xlib.SetICFocus = (delegate* unmanaged[Cdecl]<nint, void>)p;

            // XSetIMValues has varargs
            x11.xlib.SetIMValues_ptr = TryLoad(lib, "XSetIMValues");

            p = TryLoad(lib, "XSetInputFocus");
            x11.xlib.SetInputFocus = (delegate* unmanaged[Cdecl]<nint, nuint, int, nuint, int>)p;

            p = TryLoad(lib, "XSetLocaleModifiers");
            x11.xlib.SetLocaleModifiers = (delegate* unmanaged[Cdecl]<byte*, nint>)p;

            p = TryLoad(lib, "XSetScreenSaver");
            x11.xlib.SetScreenSaver = (delegate* unmanaged[Cdecl]<nint, int, int, int, int, int>)p;

            p = TryLoad(lib, "XSetSelectionOwner");
            x11.xlib.SetSelectionOwner = (delegate* unmanaged[Cdecl]<nint, nuint, nuint, nuint, int>)p;

            p = TryLoad(lib, "XSetWMHints");
            x11.xlib.SetWMHints = (delegate* unmanaged[Cdecl]<nint, nuint, XWMHints*, int>)p;

            p = TryLoad(lib, "XSetWMNormalHints");
            x11.xlib.SetWMNormalHints = (delegate* unmanaged[Cdecl]<nint, nuint, XSizeHints*, void>)p;

            p = TryLoad(lib, "XSetWMProtocols");
            x11.xlib.SetWMProtocols = (delegate* unmanaged[Cdecl]<nint, nuint, nuint*, int, int>)p;

            p = TryLoad(lib, "XSupportsLocale");
            x11.xlib.SupportsLocale = (delegate* unmanaged[Cdecl]<int>)p;

            p = TryLoad(lib, "XSync");
            x11.xlib.Sync = (delegate* unmanaged[Cdecl]<nint, int, int>)p;

            p = TryLoad(lib, "XTranslateCoordinates");
            x11.xlib.TranslateCoordinates = (delegate* unmanaged[Cdecl]<nint, nuint, nuint, int, int, int*, int*, nuint*, int>)p;

            p = TryLoad(lib, "XUndefineCursor");
            x11.xlib.UndefineCursor = (delegate* unmanaged[Cdecl]<nint, nuint, int>)p;

            p = TryLoad(lib, "XUngrabPointer");
            x11.xlib.UngrabPointer = (delegate* unmanaged[Cdecl]<nint, nuint, int>)p;

            p = TryLoad(lib, "XUnmapWindow");
            x11.xlib.UnmapWindow = (delegate* unmanaged[Cdecl]<nint, nuint, int>)p;

            p = TryLoad(lib, "XUnsetICFocus");
            x11.xlib.UnsetICFocus = (delegate* unmanaged[Cdecl]<nint, void>)p;

            p = TryLoad(lib, "XVisualIDFromVisual");
            x11.xlib.VisualIDFromVisual = (delegate* unmanaged[Cdecl]<nint, nuint>)p;

            p = TryLoad(lib, "XWarpPointer");
            x11.xlib.WarpPointer = (delegate* unmanaged[Cdecl]<nint, nuint, nuint, int, int, uint, uint, int, int, int>)p;

            p = TryLoad(lib, "XUnregisterIMInstantiateCallback");
            x11.xlib.UnregisterIMInstantiateCallback = (delegate* unmanaged[Cdecl]<nint, nint, byte*, byte*, nint, nint, int>)p;

            // Xutf8 functions (may not exist)
            p = TryLoad(lib, "Xutf8LookupString");
            x11.xlib.utf8LookupString = (delegate* unmanaged[Cdecl]<nint, XKeyEvent*, byte*, int, nuint*, int*, int>)p;

            p = TryLoad(lib, "Xutf8SetWMProperties");
            x11.xlib.utf8SetWMProperties = (delegate* unmanaged[Cdecl]<nint, nuint, byte*, byte*, byte**, int, XSizeHints*, XWMHints*, XClassHint*, void>)p;

            // --- Display accessor functions ---
            p = TryLoad(lib, "XOpenDisplay");
            x11.xlib.OpenDisplay = (delegate* unmanaged[Cdecl]<byte*, nint>)p;

            p = TryLoad(lib, "XDefaultScreen");
            x11.xlib.DefaultScreen = (delegate* unmanaged[Cdecl]<nint, int>)p;

            p = TryLoad(lib, "XRootWindow");
            x11.xlib.RootWindow = (delegate* unmanaged[Cdecl]<nint, int, nuint>)p;

            p = TryLoad(lib, "XDefaultVisual");
            x11.xlib.DefaultVisual = (delegate* unmanaged[Cdecl]<nint, int, nint>)p;

            p = TryLoad(lib, "XDefaultDepth");
            x11.xlib.DefaultDepth = (delegate* unmanaged[Cdecl]<nint, int, int>)p;

            p = TryLoad(lib, "XConnectionNumber");
            x11.xlib.ConnectionNumber = (delegate* unmanaged[Cdecl]<nint, int>)p;

            p = TryLoad(lib, "XQLength");
            x11.xlib.QLength = (delegate* unmanaged[Cdecl]<nint, int>)p;

            p = TryLoad(lib, "XDisplayWidth");
            x11.xlib.DisplayWidth = (delegate* unmanaged[Cdecl]<nint, int, int>)p;

            p = TryLoad(lib, "XDisplayHeight");
            x11.xlib.DisplayHeight = (delegate* unmanaged[Cdecl]<nint, int, int>)p;

            p = TryLoad(lib, "XDisplayWidthMM");
            x11.xlib.DisplayWidthMM = (delegate* unmanaged[Cdecl]<nint, int, int>)p;

            p = TryLoad(lib, "XDisplayHeightMM");
            x11.xlib.DisplayHeightMM = (delegate* unmanaged[Cdecl]<nint, int, int>)p;

            // --- Xkb functions (from libX11) ---
            p = TryLoad(lib, "XkbFreeKeyboard");
            x11.xkb.FreeKeyboard = (delegate* unmanaged[Cdecl]<nint, uint, int, void>)p;

            p = TryLoad(lib, "XkbFreeNames");
            x11.xkb.FreeNames = (delegate* unmanaged[Cdecl]<nint, uint, int, void>)p;

            p = TryLoad(lib, "XkbGetMap");
            x11.xkb.GetMap = (delegate* unmanaged[Cdecl]<nint, uint, uint, nint>)p;

            p = TryLoad(lib, "XkbGetNames");
            x11.xkb.GetNames = (delegate* unmanaged[Cdecl]<nint, uint, nint, int>)p;

            p = TryLoad(lib, "XkbGetState");
            x11.xkb.GetState = (delegate* unmanaged[Cdecl]<nint, uint, XkbStateRec*, int>)p;

            p = TryLoad(lib, "XkbKeycodeToKeysym");
            x11.xkb.KeycodeToKeysym = (delegate* unmanaged[Cdecl]<nint, byte, int, int, nuint>)p;

            p = TryLoad(lib, "XkbQueryExtension");
            x11.xkb.QueryExtension = (delegate* unmanaged[Cdecl]<nint, int*, int*, int*, int*, int*, int>)p;

            p = TryLoad(lib, "XkbSelectEventDetails");
            x11.xkb.SelectEventDetails = (delegate* unmanaged[Cdecl]<nint, uint, uint, nuint, nuint, int>)p;

            p = TryLoad(lib, "XkbSetDetectableAutoRepeat");
            x11.xkb.SetDetectableAutoRepeat = (delegate* unmanaged[Cdecl]<nint, int, int*, int>)p;

            // --- Xrm functions (from libX11) ---
            p = TryLoad(lib, "XrmDestroyDatabase");
            x11.xrm.DestroyDatabase = (delegate* unmanaged[Cdecl]<nint, void>)p;

            p = TryLoad(lib, "XrmGetResource");
            x11.xrm.GetResource = (delegate* unmanaged[Cdecl]<nint, byte*, byte*, byte**, XrmValue*, int>)p;

            p = TryLoad(lib, "XrmGetStringDatabase");
            x11.xrm.GetStringDatabase = (delegate* unmanaged[Cdecl]<byte*, nint>)p;

            p = TryLoad(lib, "XrmUniqueQuark");
            x11.xrm.UniqueQuark = (delegate* unmanaged[Cdecl]<int>)p;
            x11.xrm.Initialize = (delegate* unmanaged[Cdecl]<void>)TryLoad(lib, "XrmInitialize");

            // Verify critical functions loaded
            if (x11.xlib.CloseDisplay == null ||
                x11.xlib.CreateWindow == null ||
                x11.xlib.InternAtom == null)
            {
                return false;
            }

            return true;
        }

        // ===============================================================
        //  LoadRandr — loads libXrandr.so.2
        // ===============================================================

        public static bool LoadRandr(GlfwLibraryX11 x11)
        {
            if (!NativeLibrary.TryLoad("libXrandr.so.2", out var lib))
                return false;

            x11.randr.handle = lib;

            nint p;

            p = TryLoad(lib, "XRRAllocGamma");
            x11.randr.AllocGamma = (delegate* unmanaged[Cdecl]<int, nint>)p;

            p = TryLoad(lib, "XRRFreeCrtcInfo");
            x11.randr.FreeCrtcInfo = (delegate* unmanaged[Cdecl]<nint, void>)p;

            p = TryLoad(lib, "XRRFreeGamma");
            x11.randr.FreeGamma = (delegate* unmanaged[Cdecl]<nint, void>)p;

            p = TryLoad(lib, "XRRFreeOutputInfo");
            x11.randr.FreeOutputInfo = (delegate* unmanaged[Cdecl]<nint, void>)p;

            p = TryLoad(lib, "XRRFreeScreenResources");
            x11.randr.FreeScreenResources = (delegate* unmanaged[Cdecl]<nint, void>)p;

            p = TryLoad(lib, "XRRGetCrtcGamma");
            x11.randr.GetCrtcGamma = (delegate* unmanaged[Cdecl]<nint, nuint, nint>)p;

            p = TryLoad(lib, "XRRGetCrtcGammaSize");
            x11.randr.GetCrtcGammaSize = (delegate* unmanaged[Cdecl]<nint, nuint, int>)p;

            p = TryLoad(lib, "XRRGetCrtcInfo");
            x11.randr.GetCrtcInfo = (delegate* unmanaged[Cdecl]<nint, nint, nuint, nint>)p;

            p = TryLoad(lib, "XRRGetOutputInfo");
            x11.randr.GetOutputInfo = (delegate* unmanaged[Cdecl]<nint, nint, nuint, nint>)p;

            p = TryLoad(lib, "XRRGetOutputPrimary");
            x11.randr.GetOutputPrimary = (delegate* unmanaged[Cdecl]<nint, nuint, nuint>)p;

            p = TryLoad(lib, "XRRGetScreenResourcesCurrent");
            x11.randr.GetScreenResourcesCurrent = (delegate* unmanaged[Cdecl]<nint, nuint, nint>)p;

            p = TryLoad(lib, "XRRQueryExtension");
            x11.randr.QueryExtension = (delegate* unmanaged[Cdecl]<nint, int*, int*, int>)p;

            p = TryLoad(lib, "XRRQueryVersion");
            x11.randr.QueryVersion = (delegate* unmanaged[Cdecl]<nint, int*, int*, int>)p;

            p = TryLoad(lib, "XRRSelectInput");
            x11.randr.SelectInput = (delegate* unmanaged[Cdecl]<nint, nuint, int, void>)p;

            p = TryLoad(lib, "XRRSetCrtcConfig");
            x11.randr.SetCrtcConfig = (delegate* unmanaged[Cdecl]<nint, nint, nuint, nuint, int, int, nuint, ushort, nuint*, int, int>)p;

            p = TryLoad(lib, "XRRSetCrtcGamma");
            x11.randr.SetCrtcGamma = (delegate* unmanaged[Cdecl]<nint, nuint, nint, void>)p;

            p = TryLoad(lib, "XRRUpdateConfiguration");
            x11.randr.UpdateConfiguration = (delegate* unmanaged[Cdecl]<XEvent*, int>)p;

            return true;
        }

        // ===============================================================
        //  LoadXcursor — loads libXcursor.so.1
        // ===============================================================

        public static bool LoadXcursor(GlfwLibraryX11 x11)
        {
            if (!NativeLibrary.TryLoad("libXcursor.so.1", out var lib))
                return false;

            x11.xcursor.handle = lib;

            nint p;

            p = TryLoad(lib, "XcursorImageCreate");
            x11.xcursor.ImageCreate = (delegate* unmanaged[Cdecl]<int, int, nint>)p;

            p = TryLoad(lib, "XcursorImageDestroy");
            x11.xcursor.ImageDestroy = (delegate* unmanaged[Cdecl]<nint, void>)p;

            p = TryLoad(lib, "XcursorImageLoadCursor");
            x11.xcursor.ImageLoadCursor = (delegate* unmanaged[Cdecl]<nint, nint, nuint>)p;

            p = TryLoad(lib, "XcursorGetTheme");
            x11.xcursor.GetTheme = (delegate* unmanaged[Cdecl]<nint, nint>)p;

            p = TryLoad(lib, "XcursorGetDefaultSize");
            x11.xcursor.GetDefaultSize = (delegate* unmanaged[Cdecl]<nint, int>)p;

            p = TryLoad(lib, "XcursorLibraryLoadImage");
            x11.xcursor.LibraryLoadImage = (delegate* unmanaged[Cdecl]<byte*, byte*, int, nint>)p;

            return true;
        }

        // ===============================================================
        //  LoadXinerama — loads libXinerama.so.1
        // ===============================================================

        public static bool LoadXinerama(GlfwLibraryX11 x11)
        {
            if (!NativeLibrary.TryLoad("libXinerama.so.1", out var lib))
                return false;

            x11.xinerama.handle = lib;

            nint p;

            p = TryLoad(lib, "XineramaIsActive");
            x11.xinerama.IsActive = (delegate* unmanaged[Cdecl]<nint, int>)p;

            p = TryLoad(lib, "XineramaQueryExtension");
            x11.xinerama.QueryExtension = (delegate* unmanaged[Cdecl]<nint, int*, int*, int>)p;

            p = TryLoad(lib, "XineramaQueryScreens");
            x11.xinerama.QueryScreens = (delegate* unmanaged[Cdecl]<nint, int*, nint>)p;

            return true;
        }

        // ===============================================================
        //  LoadXi — loads libXi.so.6 (XInput2)
        // ===============================================================

        public static bool LoadXi(GlfwLibraryX11 x11)
        {
            if (!NativeLibrary.TryLoad("libXi.so.6", out var lib))
                return false;

            x11.xi.handle = lib;

            nint p;

            p = TryLoad(lib, "XIQueryVersion");
            x11.xi.QueryVersion = (delegate* unmanaged[Cdecl]<nint, int*, int*, int>)p;

            p = TryLoad(lib, "XISelectEvents");
            x11.xi.SelectEvents = (delegate* unmanaged[Cdecl]<nint, nuint, XIEventMask*, int, int>)p;

            return true;
        }

        // ===============================================================
        //  LoadXrender — loads libXrender.so.1
        // ===============================================================

        public static bool LoadXrender(GlfwLibraryX11 x11)
        {
            if (!NativeLibrary.TryLoad("libXrender.so.1", out var lib))
                return false;

            x11.xrender.handle = lib;

            nint p;

            p = TryLoad(lib, "XRenderQueryExtension");
            x11.xrender.QueryExtension = (delegate* unmanaged[Cdecl]<nint, int*, int*, int>)p;

            p = TryLoad(lib, "XRenderQueryVersion");
            x11.xrender.QueryVersion = (delegate* unmanaged[Cdecl]<nint, int*, int*, int>)p;

            p = TryLoad(lib, "XRenderFindVisualFormat");
            x11.xrender.FindVisualFormat = (delegate* unmanaged[Cdecl]<nint, nint, nint>)p;

            return true;
        }

        // ===============================================================
        //  LoadXshape — loads libXext.so.6 (XShape extension)
        // ===============================================================

        public static bool LoadXshape(GlfwLibraryX11 x11)
        {
            if (!NativeLibrary.TryLoad("libXext.so.6", out var lib))
                return false;

            x11.xshape.handle = lib;

            nint p;

            p = TryLoad(lib, "XShapeQueryExtension");
            x11.xshape.QueryExtension = (delegate* unmanaged[Cdecl]<nint, int*, int*, int>)p;

            p = TryLoad(lib, "XShapeCombineRegion");
            x11.xshape.ShapeCombineRegion = (delegate* unmanaged[Cdecl]<nint, nuint, int, int, int, nint, int, void>)p;

            p = TryLoad(lib, "XShapeQueryVersion");
            x11.xshape.QueryVersion = (delegate* unmanaged[Cdecl]<nint, int*, int*, int>)p;

            p = TryLoad(lib, "XShapeCombineMask");
            x11.xshape.ShapeCombineMask = (delegate* unmanaged[Cdecl]<nint, nuint, int, int, int, nuint, int, void>)p;

            return true;
        }

        // ===============================================================
        //  LoadVidmode — loads libXxf86vm.so.1 (XF86VidMode)
        // ===============================================================

        public static bool LoadVidmode(GlfwLibraryX11 x11)
        {
            if (!NativeLibrary.TryLoad("libXxf86vm.so.1", out var lib))
                return false;

            x11.vidmode.handle = lib;

            nint p;

            p = TryLoad(lib, "XF86VidModeQueryExtension");
            x11.vidmode.QueryExtension = (delegate* unmanaged[Cdecl]<nint, int*, int*, int>)p;

            p = TryLoad(lib, "XF86VidModeGetGammaRamp");
            x11.vidmode.GetGammaRamp = (delegate* unmanaged[Cdecl]<nint, int, int, ushort*, ushort*, ushort*, int>)p;

            p = TryLoad(lib, "XF86VidModeSetGammaRamp");
            x11.vidmode.SetGammaRamp = (delegate* unmanaged[Cdecl]<nint, int, int, ushort*, ushort*, ushort*, int>)p;

            p = TryLoad(lib, "XF86VidModeGetGammaRampSize");
            x11.vidmode.GetGammaRampSize = (delegate* unmanaged[Cdecl]<nint, int, int*, int>)p;

            return true;
        }

        // ===============================================================
        //  LoadX11Xcb — loads libX11-xcb.so.1
        // ===============================================================

        public static bool LoadX11Xcb(GlfwLibraryX11 x11)
        {
            if (!NativeLibrary.TryLoad("libX11-xcb.so.1", out var lib))
                return false;

            x11.x11xcb.handle = lib;

            nint p;

            p = TryLoad(lib, "XGetXCBConnection");
            x11.x11xcb.GetXCBConnection = (delegate* unmanaged[Cdecl]<nint, nint>)p;

            return true;
        }

        // NOTE: GLX loading is handled in glx/glx_context.cs (_glfwInitGLX)

        // ===============================================================
        //  LoadLibc — loads libc.so.6 (POSIX functions)
        // ===============================================================

        public static bool LoadLibc(GlfwLibraryX11 x11)
        {
            if (!NativeLibrary.TryLoad("libc.so.6", out var lib))
            {
                // Fallback: try "libc" (some distros)
                if (!NativeLibrary.TryLoad("libc", out lib))
                    return false;
            }

            x11.libc.handle = lib;

            nint p;

            p = TryLoad(lib, "pipe");
            x11.libc.pipe = (delegate* unmanaged[Cdecl]<int*, int>)p;

            p = TryLoad(lib, "fcntl");
            x11.libc.fcntl = (delegate* unmanaged[Cdecl]<int, int, int, int>)p;

            p = TryLoad(lib, "close");
            x11.libc.close = (delegate* unmanaged[Cdecl]<int, int>)p;

            p = TryLoad(lib, "write");
            x11.libc.write = (delegate* unmanaged[Cdecl]<int, byte*, nuint, nint>)p;

            p = TryLoad(lib, "read");
            x11.libc.read = (delegate* unmanaged[Cdecl]<int, byte*, nuint, nint>)p;

            p = TryLoad(lib, "getpid");
            x11.libc.getpid = (delegate* unmanaged[Cdecl]<int>)p;

            p = TryLoad(lib, "poll");
            x11.libc.poll = (delegate* unmanaged[Cdecl]<PollFd*, nuint, int, int>)p;

            p = TryLoad(lib, "__errno_location");
            x11.libc.__errno_location = (delegate* unmanaged[Cdecl]<int*>)p;

            return true;
        }

        // ===============================================================
        //  FreeLibraries — unloads all X11-related shared libraries
        // ===============================================================

        public static void FreeLibraries(GlfwLibraryX11 x11)
        {
            if (x11.xlib.handle != nint.Zero)
            {
                NativeLibrary.Free(x11.xlib.handle);
                x11.xlib.handle = nint.Zero;
            }
            if (x11.randr.handle != nint.Zero)
            {
                NativeLibrary.Free(x11.randr.handle);
                x11.randr.handle = nint.Zero;
            }
            if (x11.xcursor.handle != nint.Zero)
            {
                NativeLibrary.Free(x11.xcursor.handle);
                x11.xcursor.handle = nint.Zero;
            }
            if (x11.xinerama.handle != nint.Zero)
            {
                NativeLibrary.Free(x11.xinerama.handle);
                x11.xinerama.handle = nint.Zero;
            }
            if (x11.x11xcb.handle != nint.Zero)
            {
                NativeLibrary.Free(x11.x11xcb.handle);
                x11.x11xcb.handle = nint.Zero;
            }
            if (x11.vidmode.handle != nint.Zero)
            {
                NativeLibrary.Free(x11.vidmode.handle);
                x11.vidmode.handle = nint.Zero;
            }
            if (x11.xi.handle != nint.Zero)
            {
                NativeLibrary.Free(x11.xi.handle);
                x11.xi.handle = nint.Zero;
            }
            if (x11.xrender.handle != nint.Zero)
            {
                NativeLibrary.Free(x11.xrender.handle);
                x11.xrender.handle = nint.Zero;
            }
            if (x11.xshape.handle != nint.Zero)
            {
                NativeLibrary.Free(x11.xshape.handle);
                x11.xshape.handle = nint.Zero;
            }
            if (x11.libc.handle != nint.Zero)
            {
                NativeLibrary.Free(x11.libc.handle);
                x11.libc.handle = nint.Zero;
            }
        }
    }
}
