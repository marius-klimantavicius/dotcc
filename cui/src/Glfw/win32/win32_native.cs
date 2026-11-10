// Ported from glfw/src/win32_platform.h -- GLFW 3.6 Win32 native library loading
//
// Loads user32.dll, kernel32.dll, gdi32.dll at startup, and optionally
// dwmapi.dll, shcore.dll, ntdll.dll for DPI and compositor support.
//
// Uses NativeLibrary.Load / TryGetExport and unmanaged[Stdcall] function
// pointers, following the same pattern as x11_native.cs.

using System;
using System.Runtime.InteropServices;

namespace Glfw
{
    public static unsafe class Win32Native
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
        //  Core user32.dll functions (always available)
        // ===============================================================

        public static User32Core? user32;

        public unsafe class User32Core
        {
            public nint handle; // HMODULE

            // Window creation / destruction
            public delegate* unmanaged[Stdcall]<uint, char*, char*, uint, int, int, int, int, nint, nint, nint, nint, nint> CreateWindowExW;
            public delegate* unmanaged[Stdcall]<nint, int> DestroyWindow;
            public delegate* unmanaged[Stdcall]<nint, int, int> ShowWindow;
            public delegate* unmanaged[Stdcall]<nint, nint, int, int, int, int, uint, int> SetWindowPos;
            public delegate* unmanaged[Stdcall]<nint, RECT*, int> GetWindowRect;
            public delegate* unmanaged[Stdcall]<nint, RECT*, int> GetClientRect;
            public delegate* unmanaged[Stdcall]<nint, int, int, int, int, int, int> MoveWindow;
            public delegate* unmanaged[Stdcall]<nint, char*, int> SetWindowTextW;

            // Window class registration
            public delegate* unmanaged[Stdcall]<WNDCLASSEXW*, ushort> RegisterClassExW;
            public delegate* unmanaged[Stdcall]<char*, nint, int> UnregisterClassW;
            public delegate* unmanaged[Stdcall]<nint, uint, nuint, nint, nint> DefWindowProcW;

            // Message pump
            public delegate* unmanaged[Stdcall]<MSG*, nint, uint, uint, int> GetMessageW;
            public delegate* unmanaged[Stdcall]<MSG*, nint, uint, uint, uint, int> PeekMessageW;
            public delegate* unmanaged[Stdcall]<MSG*, int> TranslateMessage;
            public delegate* unmanaged[Stdcall]<MSG*, nint> DispatchMessageW;
            public delegate* unmanaged[Stdcall]<nint, uint, nuint, nint, int> PostMessageW;
            public delegate* unmanaged[Stdcall]<nint, uint, nuint, nint, nint> SendMessageW;
            public delegate* unmanaged[Stdcall]<nint, nuint, uint, nint, nuint> SetTimer;
            public delegate* unmanaged[Stdcall]<nint, nuint, int> KillTimer;

            // Focus / foreground
            public delegate* unmanaged[Stdcall]<nint> GetFocus;
            public delegate* unmanaged[Stdcall]<nint, nint> SetFocus;
            public delegate* unmanaged[Stdcall]<nint, int> SetForegroundWindow;
            public delegate* unmanaged[Stdcall]<nint> GetForegroundWindow;
            public delegate* unmanaged[Stdcall]<nint> GetActiveWindow;

            // Window long
            public delegate* unmanaged[Stdcall]<nint, int, nint, nint> SetWindowLongPtrW;
            public delegate* unmanaged[Stdcall]<nint, int, nint> GetWindowLongPtrW;
            public delegate* unmanaged[Stdcall]<nint, int, nuint> GetClassLongPtrW;

            // Layered window
            public delegate* unmanaged[Stdcall]<nint, uint, byte, uint, int> SetLayeredWindowAttributes;

            // Window placement
            public delegate* unmanaged[Stdcall]<nint, WINDOWPLACEMENT*, int> SetWindowPlacement;
            public delegate* unmanaged[Stdcall]<nint, WINDOWPLACEMENT*, int> GetWindowPlacement;

            // Monitor
            public delegate* unmanaged[Stdcall]<nint, uint, nint> MonitorFromWindow;
            public delegate* unmanaged[Stdcall]<POINT, uint, nint> MonitorFromPoint;
            public delegate* unmanaged[Stdcall]<nint, MONITORINFOEXW*, int> GetMonitorInfoW;
            // EnumDisplayMonitors: BOOL(HDC, LPRECT, MONITORENUMPROC, LPARAM)
            public delegate* unmanaged[Stdcall]<nint, RECT*, nint, nint, int> EnumDisplayMonitors;
            // EnumDisplaySettingsW: BOOL(LPCWSTR, DWORD, DEVMODEW*)
            public delegate* unmanaged[Stdcall]<char*, uint, DEVMODEW*, int> EnumDisplaySettingsW;
            // EnumDisplaySettingsExW: BOOL(LPCWSTR, DWORD, DEVMODEW*, DWORD)
            public delegate* unmanaged[Stdcall]<char*, uint, DEVMODEW*, uint, int> EnumDisplaySettingsExW;
            // EnumDisplayDevicesW: BOOL(LPCWSTR, DWORD, DISPLAY_DEVICEW*, DWORD)
            public delegate* unmanaged[Stdcall]<char*, uint, DISPLAY_DEVICEW*, uint, int> EnumDisplayDevicesW;
            // ChangeDisplaySettingsExW: LONG(LPCWSTR, DEVMODEW*, HWND, DWORD, LPVOID)
            public delegate* unmanaged[Stdcall]<char*, DEVMODEW*, nint, uint, nint, int> ChangeDisplaySettingsExW;

            // Keyboard
            public delegate* unmanaged[Stdcall]<uint, uint, uint> MapVirtualKeyW;
            public delegate* unmanaged[Stdcall]<int, short> GetKeyState;
            public delegate* unmanaged[Stdcall]<int, short> GetAsyncKeyState;
            public delegate* unmanaged[Stdcall]<int, char*, int, int> GetKeyNameTextW;
            public delegate* unmanaged[Stdcall]<uint, uint, nint, uint, int> SystemParametersInfoW;

            // Cursor
            public delegate* unmanaged[Stdcall]<nint, char*, nint> LoadCursorW;
            public delegate* unmanaged[Stdcall]<nint, char*, nint> LoadIconW;
            public delegate* unmanaged[Stdcall]<nint, nint> SetCursor;
            public delegate* unmanaged[Stdcall]<int, int> ShowCursor;
            public delegate* unmanaged[Stdcall]<POINT*, int> GetCursorPos;
            public delegate* unmanaged[Stdcall]<int, int, int> SetCursorPos;
            public delegate* unmanaged[Stdcall]<nint, POINT*, int> ClientToScreen;
            public delegate* unmanaged[Stdcall]<nint, POINT*, int> ScreenToClient;
            public delegate* unmanaged[Stdcall]<RECT*, int> ClipCursor;
            public delegate* unmanaged[Stdcall]<TRACKMOUSEEVENT*, int> TrackMouseEvent;
            public delegate* unmanaged[Stdcall]<nint, nint> SetCapture;
            public delegate* unmanaged[Stdcall]<int> ReleaseCapture;

            // Clipboard
            public delegate* unmanaged[Stdcall]<nint, int> OpenClipboard;
            public delegate* unmanaged[Stdcall]<int> CloseClipboard;
            public delegate* unmanaged[Stdcall]<int> EmptyClipboard;
            public delegate* unmanaged[Stdcall]<uint, nint> GetClipboardData;
            public delegate* unmanaged[Stdcall]<uint, nint, nint> SetClipboardData;

            // Window query
            public delegate* unmanaged[Stdcall]<nint, int> IsWindowVisible;
            public delegate* unmanaged[Stdcall]<nint, int> IsIconic;
            public delegate* unmanaged[Stdcall]<nint, int> IsZoomed;
            public delegate* unmanaged[Stdcall]<nint, int> IsWindow;

            // DragAcceptFiles is in shell32, not user32

            // Raw input
            public delegate* unmanaged[Stdcall]<RAWINPUTDEVICE*, uint, uint, int> RegisterRawInputDevices;
            public delegate* unmanaged[Stdcall]<nint, uint, byte*, uint*, uint, uint> GetRawInputData;

            // Misc
            public delegate* unmanaged[Stdcall]<int, void> PostQuitMessage;
            public delegate* unmanaged[Stdcall]<int, int> GetSystemMetrics;
            public delegate* unmanaged[Stdcall]<RECT*, uint, int, uint, int> AdjustWindowRectEx;
            public delegate* unmanaged[Stdcall]<nint, uint, nint> GetAncestor;

            // Icon creation / destruction
            public delegate* unmanaged[Stdcall]<ICONINFO*, nint> CreateIconIndirect;
            public delegate* unmanaged[Stdcall]<nint, int> DestroyIcon;

            // ChangeWindowMessageFilterEx: BOOL(HWND, UINT, DWORD, CHANGEFILTERSTRUCT*)
            public delegate* unmanaged[Stdcall]<nint, uint, uint, CHANGEFILTERSTRUCT*, int> ChangeWindowMessageFilterEx;

            // FlashWindow: BOOL(HWND, BOOL)
            public delegate* unmanaged[Stdcall]<nint, int, int> FlashWindow;

            // SetProcessDPIAware: BOOL(void) (Vista+)
            public delegate* unmanaged[Stdcall]<int> SetProcessDPIAware;

            // Window property functions
            // SetPropW: BOOL(HWND, LPCWSTR, HANDLE)
            public delegate* unmanaged[Stdcall]<nint, char*, nint, int> SetPropW;
            // GetPropW: HANDLE(HWND, LPCWSTR)
            public delegate* unmanaged[Stdcall]<nint, char*, nint> GetPropW;
            // RemovePropW: HANDLE(HWND, LPCWSTR)
            public delegate* unmanaged[Stdcall]<nint, char*, nint> RemovePropW;

            // LoadImageW: HANDLE(HINSTANCE, LPCWSTR, UINT, int, int, UINT)
            public delegate* unmanaged[Stdcall]<nint, char*, uint, int, int, uint, nint> LoadImageW;

            // WindowFromPoint: HWND(POINT)
            public delegate* unmanaged[Stdcall]<POINT, nint> WindowFromPoint;
            // PtInRect: BOOL(const RECT*, POINT)
            public delegate* unmanaged[Stdcall]<RECT*, POINT, int> PtInRect;
            // SetRect: BOOL(LPRECT, int, int, int, int)
            public delegate* unmanaged[Stdcall]<RECT*, int, int, int, int, int> SetRect;
            // OffsetRect: BOOL(LPRECT, int, int)
            public delegate* unmanaged[Stdcall]<RECT*, int, int, int> OffsetRect;

            // GetMessageTime: LONG(void)
            public delegate* unmanaged[Stdcall]<uint> GetMessageTime;

            // WaitMessage: BOOL(void)
            public delegate* unmanaged[Stdcall]<int> WaitMessage;
            // MsgWaitForMultipleObjects: DWORD(DWORD, const HANDLE*, BOOL, DWORD, DWORD)
            public delegate* unmanaged[Stdcall]<uint, nint*, int, uint, uint, uint> MsgWaitForMultipleObjects;

            // GetLayeredWindowAttributes: BOOL(HWND, COLORREF*, BYTE*, DWORD*)
            public delegate* unmanaged[Stdcall]<nint, uint*, byte*, uint*, int> GetLayeredWindowAttributes;

            // BringWindowToTop: BOOL(HWND)
            public delegate* unmanaged[Stdcall]<nint, int> BringWindowToTop;

            // GetWindowLongW: LONG(HWND, int) -- 32-bit compat
            public delegate* unmanaged[Stdcall]<nint, int, int> GetWindowLongW;
            // SetWindowLongW: LONG(HWND, int, LONG)
            public delegate* unmanaged[Stdcall]<nint, int, int, int> SetWindowLongW;

            // ToUnicode: int(UINT, UINT, const BYTE*, LPWSTR, int, UINT)
            public delegate* unmanaged[Stdcall]<uint, uint, byte*, char*, int, uint, int> ToUnicode;

            // RegisterDeviceNotificationW: HDEVNOTIFY(HANDLE, LPVOID, DWORD)
            public delegate* unmanaged[Stdcall]<nint, void*, uint, nint> RegisterDeviceNotificationW;
            // UnregisterDeviceNotification: BOOL(HDEVNOTIFY)
            public delegate* unmanaged[Stdcall]<nint, int> UnregisterDeviceNotification;
        }

        // ===============================================================
        //  Core kernel32.dll functions (always available)
        // ===============================================================

        public static Kernel32Core? kernel32;

        public unsafe class Kernel32Core
        {
            public nint handle;

            public delegate* unmanaged[Stdcall]<char*, nint> GetModuleHandleW;
            public delegate* unmanaged[Stdcall]<char*, nint> LoadLibraryW;
            public delegate* unmanaged[Stdcall]<nint, int> FreeLibrary;
            public delegate* unmanaged[Stdcall]<nint, byte*, nint> GetProcAddress;
            public delegate* unmanaged[Stdcall]<uint, nuint, nint> GlobalAlloc;
            public delegate* unmanaged[Stdcall]<nint, nint> GlobalLock;
            public delegate* unmanaged[Stdcall]<nint, int> GlobalUnlock;
            public delegate* unmanaged[Stdcall]<nint, nint> GlobalFree;
            // MultiByteToWideChar: int(UINT, DWORD, LPCCH, int, LPWSTR, int)
            public delegate* unmanaged[Stdcall]<uint, uint, byte*, int, char*, int, int> MultiByteToWideChar;
            // WideCharToMultiByte: int(UINT, DWORD, LPCWCH, int, LPSTR, int, LPCCH, LPBOOL)
            public delegate* unmanaged[Stdcall]<uint, uint, char*, int, byte*, int, byte*, int*, int> WideCharToMultiByte;
            public delegate* unmanaged[Stdcall]<uint> GetLastError;
            // GetModuleFileNameW: DWORD(HMODULE, LPWSTR, DWORD)
            public delegate* unmanaged[Stdcall]<nint, char*, uint, uint> GetModuleFileNameW;
            // Sleep: void(DWORD)
            public delegate* unmanaged[Stdcall]<uint, void> Sleep;

            // SetThreadExecutionState: EXECUTION_STATE(EXECUTION_STATE)
            public delegate* unmanaged[Stdcall]<uint, uint> SetThreadExecutionState;

            // GetModuleHandleExW: BOOL(DWORD, LPCWSTR, HMODULE*)
            public delegate* unmanaged[Stdcall]<uint, char*, nint*, int> GetModuleHandleExW;

            // FormatMessageW: DWORD(DWORD, LPCVOID, DWORD, DWORD, LPWSTR, DWORD, va_list*)
            public delegate* unmanaged[Stdcall]<uint, nint, uint, uint, char*, uint, nint, uint> FormatMessageW;

            // VerSetConditionMask: ULONGLONG(ULONGLONG, ULONG, UCHAR)
            public delegate* unmanaged[Stdcall]<ulong, uint, byte, ulong> VerSetConditionMask;

            // GetStartupInfoW: void(LPSTARTUPINFOW)
            public delegate* unmanaged[Stdcall]<void*, void> GetStartupInfoW;
        }

        // ===============================================================
        //  Core gdi32.dll functions (always available)
        // ===============================================================

        public static Gdi32Core? gdi32;

        public unsafe class Gdi32Core
        {
            public nint handle;

            // DC operations
            public delegate* unmanaged[Stdcall]<nint, nint> GetDC;
            public delegate* unmanaged[Stdcall]<nint, nint, int> ReleaseDC;

            // Pixel format
            public delegate* unmanaged[Stdcall]<nint, int, PIXELFORMATDESCRIPTOR*, int> SetPixelFormat;
            public delegate* unmanaged[Stdcall]<nint, PIXELFORMATDESCRIPTOR*, int> ChoosePixelFormat;
            public delegate* unmanaged[Stdcall]<nint, int, uint, PIXELFORMATDESCRIPTOR*, int> DescribePixelFormat;

            // Swap
            public delegate* unmanaged[Stdcall]<nint, int> SwapBuffers;

            // Bitmap / DIB
            // CreateDIBSection: HBITMAP(HDC, const BITMAPINFO*, UINT, void**, HANDLE, DWORD)
            public delegate* unmanaged[Stdcall]<nint, nint, uint, void**, nint, uint, nint> CreateDIBSection;
            public delegate* unmanaged[Stdcall]<nint, int> DeleteObject;
            // CreateCompatibleDC / DeleteDC
            public delegate* unmanaged[Stdcall]<nint, nint> CreateCompatibleDC;
            public delegate* unmanaged[Stdcall]<nint, int> DeleteDC;
            // CreateBitmap
            public delegate* unmanaged[Stdcall]<int, int, uint, uint, nint, nint> CreateBitmap;
            // GetDeviceCaps
            public delegate* unmanaged[Stdcall]<nint, int, int> GetDeviceCaps;

            // GammaRamp
            // GetDeviceGammaRamp / SetDeviceGammaRamp
            public delegate* unmanaged[Stdcall]<nint, void*, int> GetDeviceGammaRamp;
            public delegate* unmanaged[Stdcall]<nint, void*, int> SetDeviceGammaRamp;

            // CreateRectRgn: HRGN(int, int, int, int)
            public delegate* unmanaged[Stdcall]<int, int, int, int, nint> CreateRectRgn;

            // CreateDCW: HDC(LPCWSTR, LPCWSTR, LPCWSTR, const DEVMODE*)
            public delegate* unmanaged[Stdcall]<char*, char*, nint, nint, nint> CreateDCW;
        }

        // ===============================================================
        //  Shell32 functions (for drag-and-drop)
        // ===============================================================

        public static Shell32Core? shell32;

        public unsafe class Shell32Core
        {
            public nint handle;

            // DragAcceptFiles: void(HWND, BOOL)
            public delegate* unmanaged[Stdcall]<nint, int, void> DragAcceptFiles;
            // DragQueryFileW: UINT(HDROP, UINT, LPWSTR, UINT)
            public delegate* unmanaged[Stdcall]<nint, uint, char*, uint, uint> DragQueryFileW;
            public delegate* unmanaged[Stdcall]<nint, void> DragFinish;
            // DragQueryPoint: BOOL(HDROP, LPPOINT)
            public delegate* unmanaged[Stdcall]<nint, POINT*, int> DragQueryPoint;
        }

        // ===============================================================
        //  Load all core libraries
        // ===============================================================

        public static bool LoadCoreLibraries()
            => LoadCoreLibraries(Glfw._glfwPlatformLoadModule, Glfw._glfwPlatformGetModuleSymbol);

        // The same loader can be exercised with owned native modules on hosts
        // without Windows. Each nonzero load transfers one reference here.
        internal static bool LoadCoreLibraries(Func<string, nint> loadModule, Func<nint, string, nint> getSymbol)
        {
            bool success = false;
            nint Required(nint module, string name)
            {
                nint address = getSymbol(module, name);
                if (address == 0) throw new EntryPointNotFoundException(name);
                return address;
            }
            try
            {
                success = Load();
                return success;
            }
            catch (EntryPointNotFoundException) { return false; }
            finally
            {
                if (!success) FreeLibraries(null);
            }

            bool Load()
            {
                // --- user32.dll ---
                nint hUser32 = loadModule("user32.dll");
                if (hUser32 == 0)
                    return false;

                var u32 = new User32Core { handle = hUser32 };
                user32 = u32;
                nint p;

                p = Required(hUser32, "CreateWindowExW");
                u32.CreateWindowExW = (delegate* unmanaged[Stdcall]<uint, char*, char*, uint, int, int, int, int, nint, nint, nint, nint, nint>)p;

                p = Required(hUser32, "DestroyWindow");
                u32.DestroyWindow = (delegate* unmanaged[Stdcall]<nint, int>)p;

                p = Required(hUser32, "ShowWindow");
                u32.ShowWindow = (delegate* unmanaged[Stdcall]<nint, int, int>)p;

                p = Required(hUser32, "SetWindowPos");
                u32.SetWindowPos = (delegate* unmanaged[Stdcall]<nint, nint, int, int, int, int, uint, int>)p;

                p = Required(hUser32, "GetWindowRect");
                u32.GetWindowRect = (delegate* unmanaged[Stdcall]<nint, RECT*, int>)p;

                p = Required(hUser32, "GetClientRect");
                u32.GetClientRect = (delegate* unmanaged[Stdcall]<nint, RECT*, int>)p;

                p = Required(hUser32, "MoveWindow");
                u32.MoveWindow = (delegate* unmanaged[Stdcall]<nint, int, int, int, int, int, int>)p;

                p = Required(hUser32, "SetWindowTextW");
                u32.SetWindowTextW = (delegate* unmanaged[Stdcall]<nint, char*, int>)p;

                p = Required(hUser32, "RegisterClassExW");
                u32.RegisterClassExW = (delegate* unmanaged[Stdcall]<WNDCLASSEXW*, ushort>)p;

                p = Required(hUser32, "UnregisterClassW");
                u32.UnregisterClassW = (delegate* unmanaged[Stdcall]<char*, nint, int>)p;

                p = Required(hUser32, "DefWindowProcW");
                u32.DefWindowProcW = (delegate* unmanaged[Stdcall]<nint, uint, nuint, nint, nint>)p;

                p = Required(hUser32, "GetMessageW");
                u32.GetMessageW = (delegate* unmanaged[Stdcall]<MSG*, nint, uint, uint, int>)p;

                p = Required(hUser32, "PeekMessageW");
                u32.PeekMessageW = (delegate* unmanaged[Stdcall]<MSG*, nint, uint, uint, uint, int>)p;

                p = Required(hUser32, "TranslateMessage");
                u32.TranslateMessage = (delegate* unmanaged[Stdcall]<MSG*, int>)p;

                p = Required(hUser32, "DispatchMessageW");
                u32.DispatchMessageW = (delegate* unmanaged[Stdcall]<MSG*, nint>)p;

                p = Required(hUser32, "PostMessageW");
                u32.PostMessageW = (delegate* unmanaged[Stdcall]<nint, uint, nuint, nint, int>)p;

                p = Required(hUser32, "SendMessageW");
                u32.SendMessageW = (delegate* unmanaged[Stdcall]<nint, uint, nuint, nint, nint>)p;

                p = Required(hUser32, "SetTimer");
                u32.SetTimer = (delegate* unmanaged[Stdcall]<nint, nuint, uint, nint, nuint>)p;

                p = Required(hUser32, "KillTimer");
                u32.KillTimer = (delegate* unmanaged[Stdcall]<nint, nuint, int>)p;

                p = Required(hUser32, "GetFocus");
                u32.GetFocus = (delegate* unmanaged[Stdcall]<nint>)p;

                p = Required(hUser32, "SetFocus");
                u32.SetFocus = (delegate* unmanaged[Stdcall]<nint, nint>)p;

                p = Required(hUser32, "SetForegroundWindow");
                u32.SetForegroundWindow = (delegate* unmanaged[Stdcall]<nint, int>)p;

                p = Required(hUser32, "GetForegroundWindow");
                u32.GetForegroundWindow = (delegate* unmanaged[Stdcall]<nint>)p;

                p = Required(hUser32, "GetActiveWindow");
                u32.GetActiveWindow = (delegate* unmanaged[Stdcall]<nint>)p;

                p = Required(hUser32, IntPtr.Size == 8 ? "SetWindowLongPtrW" : "SetWindowLongW");
                u32.SetWindowLongPtrW = (delegate* unmanaged[Stdcall]<nint, int, nint, nint>)p;

                p = Required(hUser32, IntPtr.Size == 8 ? "GetWindowLongPtrW" : "GetWindowLongW");
                u32.GetWindowLongPtrW = (delegate* unmanaged[Stdcall]<nint, int, nint>)p;

                p = Required(hUser32, IntPtr.Size == 8 ? "GetClassLongPtrW" : "GetClassLongW");
                u32.GetClassLongPtrW = (delegate* unmanaged[Stdcall]<nint, int, nuint>)p;

                p = Required(hUser32, "SetLayeredWindowAttributes");
                u32.SetLayeredWindowAttributes = (delegate* unmanaged[Stdcall]<nint, uint, byte, uint, int>)p;

                p = Required(hUser32, "SetWindowPlacement");
                u32.SetWindowPlacement = (delegate* unmanaged[Stdcall]<nint, WINDOWPLACEMENT*, int>)p;

                p = Required(hUser32, "GetWindowPlacement");
                u32.GetWindowPlacement = (delegate* unmanaged[Stdcall]<nint, WINDOWPLACEMENT*, int>)p;

                p = Required(hUser32, "MonitorFromWindow");
                u32.MonitorFromWindow = (delegate* unmanaged[Stdcall]<nint, uint, nint>)p;

                p = Required(hUser32, "MonitorFromPoint");
                u32.MonitorFromPoint = (delegate* unmanaged[Stdcall]<POINT, uint, nint>)p;

                p = Required(hUser32, "GetMonitorInfoW");
                u32.GetMonitorInfoW = (delegate* unmanaged[Stdcall]<nint, MONITORINFOEXW*, int>)p;

                p = Required(hUser32, "EnumDisplayMonitors");
                u32.EnumDisplayMonitors = (delegate* unmanaged[Stdcall]<nint, RECT*, nint, nint, int>)p;

                p = Required(hUser32, "EnumDisplaySettingsW");
                u32.EnumDisplaySettingsW = (delegate* unmanaged[Stdcall]<char*, uint, DEVMODEW*, int>)p;

                p = Required(hUser32, "EnumDisplaySettingsExW");
                u32.EnumDisplaySettingsExW = (delegate* unmanaged[Stdcall]<char*, uint, DEVMODEW*, uint, int>)p;

                p = Required(hUser32, "EnumDisplayDevicesW");
                u32.EnumDisplayDevicesW = (delegate* unmanaged[Stdcall]<char*, uint, DISPLAY_DEVICEW*, uint, int>)p;

                p = Required(hUser32, "ChangeDisplaySettingsExW");
                u32.ChangeDisplaySettingsExW = (delegate* unmanaged[Stdcall]<char*, DEVMODEW*, nint, uint, nint, int>)p;

                p = Required(hUser32, "MapVirtualKeyW");
                u32.MapVirtualKeyW = (delegate* unmanaged[Stdcall]<uint, uint, uint>)p;

                p = Required(hUser32, "GetKeyState");
                u32.GetKeyState = (delegate* unmanaged[Stdcall]<int, short>)p;

                p = Required(hUser32, "GetAsyncKeyState");
                u32.GetAsyncKeyState = (delegate* unmanaged[Stdcall]<int, short>)p;

                p = Required(hUser32, "GetKeyNameTextW");
                u32.GetKeyNameTextW = (delegate* unmanaged[Stdcall]<int, char*, int, int>)p;

                p = Required(hUser32, "SystemParametersInfoW");
                u32.SystemParametersInfoW = (delegate* unmanaged[Stdcall]<uint, uint, nint, uint, int>)p;

                p = Required(hUser32, "LoadCursorW");
                u32.LoadCursorW = (delegate* unmanaged[Stdcall]<nint, char*, nint>)p;

                p = Required(hUser32, "LoadIconW");
                u32.LoadIconW = (delegate* unmanaged[Stdcall]<nint, char*, nint>)p;

                p = Required(hUser32, "SetCursor");
                u32.SetCursor = (delegate* unmanaged[Stdcall]<nint, nint>)p;

                p = Required(hUser32, "ShowCursor");
                u32.ShowCursor = (delegate* unmanaged[Stdcall]<int, int>)p;

                p = Required(hUser32, "GetCursorPos");
                u32.GetCursorPos = (delegate* unmanaged[Stdcall]<POINT*, int>)p;

                p = Required(hUser32, "SetCursorPos");
                u32.SetCursorPos = (delegate* unmanaged[Stdcall]<int, int, int>)p;

                p = Required(hUser32, "ClientToScreen");
                u32.ClientToScreen = (delegate* unmanaged[Stdcall]<nint, POINT*, int>)p;

                p = Required(hUser32, "ScreenToClient");
                u32.ScreenToClient = (delegate* unmanaged[Stdcall]<nint, POINT*, int>)p;

                p = Required(hUser32, "ClipCursor");
                u32.ClipCursor = (delegate* unmanaged[Stdcall]<RECT*, int>)p;

                p = Required(hUser32, "TrackMouseEvent");
                u32.TrackMouseEvent = (delegate* unmanaged[Stdcall]<TRACKMOUSEEVENT*, int>)p;

                p = Required(hUser32, "SetCapture");
                u32.SetCapture = (delegate* unmanaged[Stdcall]<nint, nint>)p;

                p = Required(hUser32, "ReleaseCapture");
                u32.ReleaseCapture = (delegate* unmanaged[Stdcall]<int>)p;

                p = Required(hUser32, "OpenClipboard");
                u32.OpenClipboard = (delegate* unmanaged[Stdcall]<nint, int>)p;

                p = Required(hUser32, "CloseClipboard");
                u32.CloseClipboard = (delegate* unmanaged[Stdcall]<int>)p;

                p = Required(hUser32, "EmptyClipboard");
                u32.EmptyClipboard = (delegate* unmanaged[Stdcall]<int>)p;

                p = Required(hUser32, "GetClipboardData");
                u32.GetClipboardData = (delegate* unmanaged[Stdcall]<uint, nint>)p;

                p = Required(hUser32, "SetClipboardData");
                u32.SetClipboardData = (delegate* unmanaged[Stdcall]<uint, nint, nint>)p;

                p = Required(hUser32, "IsWindowVisible");
                u32.IsWindowVisible = (delegate* unmanaged[Stdcall]<nint, int>)p;

                p = Required(hUser32, "IsIconic");
                u32.IsIconic = (delegate* unmanaged[Stdcall]<nint, int>)p;

                p = Required(hUser32, "IsZoomed");
                u32.IsZoomed = (delegate* unmanaged[Stdcall]<nint, int>)p;

                p = Required(hUser32, "IsWindow");
                u32.IsWindow = (delegate* unmanaged[Stdcall]<nint, int>)p;

                // DragAcceptFiles is loaded from shell32 below (not user32)

                p = Required(hUser32, "RegisterRawInputDevices");
                u32.RegisterRawInputDevices = (delegate* unmanaged[Stdcall]<RAWINPUTDEVICE*, uint, uint, int>)p;

                p = Required(hUser32, "GetRawInputData");
                u32.GetRawInputData = (delegate* unmanaged[Stdcall]<nint, uint, byte*, uint*, uint, uint>)p;

                p = Required(hUser32, "PostQuitMessage");
                u32.PostQuitMessage = (delegate* unmanaged[Stdcall]<int, void>)p;

                p = Required(hUser32, "GetSystemMetrics");
                u32.GetSystemMetrics = (delegate* unmanaged[Stdcall]<int, int>)p;

                p = Required(hUser32, "AdjustWindowRectEx");
                u32.AdjustWindowRectEx = (delegate* unmanaged[Stdcall]<RECT*, uint, int, uint, int>)p;

                p = Required(hUser32, "GetAncestor");
                u32.GetAncestor = (delegate* unmanaged[Stdcall]<nint, uint, nint>)p;

                p = Required(hUser32, "CreateIconIndirect");
                u32.CreateIconIndirect = (delegate* unmanaged[Stdcall]<ICONINFO*, nint>)p;

                p = Required(hUser32, "DestroyIcon");
                u32.DestroyIcon = (delegate* unmanaged[Stdcall]<nint, int>)p;

                p = Required(hUser32, "ChangeWindowMessageFilterEx");
                u32.ChangeWindowMessageFilterEx = (delegate* unmanaged[Stdcall]<nint, uint, uint, CHANGEFILTERSTRUCT*, int>)p;

                p = Required(hUser32, "FlashWindow");
                u32.FlashWindow = (delegate* unmanaged[Stdcall]<nint, int, int>)p;

                p = Required(hUser32, "SetProcessDPIAware");
                u32.SetProcessDPIAware = (delegate* unmanaged[Stdcall]<int>)p;

                p = Required(hUser32, "SetPropW");
                u32.SetPropW = (delegate* unmanaged[Stdcall]<nint, char*, nint, int>)p;

                p = Required(hUser32, "GetPropW");
                u32.GetPropW = (delegate* unmanaged[Stdcall]<nint, char*, nint>)p;

                p = Required(hUser32, "RemovePropW");
                u32.RemovePropW = (delegate* unmanaged[Stdcall]<nint, char*, nint>)p;

                p = Required(hUser32, "LoadImageW");
                u32.LoadImageW = (delegate* unmanaged[Stdcall]<nint, char*, uint, int, int, uint, nint>)p;

                p = Required(hUser32, "WindowFromPoint");
                u32.WindowFromPoint = (delegate* unmanaged[Stdcall]<POINT, nint>)p;

                p = Required(hUser32, "PtInRect");
                u32.PtInRect = (delegate* unmanaged[Stdcall]<RECT*, POINT, int>)p;

                p = Required(hUser32, "SetRect");
                u32.SetRect = (delegate* unmanaged[Stdcall]<RECT*, int, int, int, int, int>)p;

                p = Required(hUser32, "OffsetRect");
                u32.OffsetRect = (delegate* unmanaged[Stdcall]<RECT*, int, int, int>)p;

                p = Required(hUser32, "GetMessageTime");
                u32.GetMessageTime = (delegate* unmanaged[Stdcall]<uint>)p;

                p = Required(hUser32, "WaitMessage");
                u32.WaitMessage = (delegate* unmanaged[Stdcall]<int>)p;

                p = Required(hUser32, "MsgWaitForMultipleObjects");
                u32.MsgWaitForMultipleObjects = (delegate* unmanaged[Stdcall]<uint, nint*, int, uint, uint, uint>)p;

                p = Required(hUser32, "GetLayeredWindowAttributes");
                u32.GetLayeredWindowAttributes = (delegate* unmanaged[Stdcall]<nint, uint*, byte*, uint*, int>)p;

                p = Required(hUser32, "BringWindowToTop");
                u32.BringWindowToTop = (delegate* unmanaged[Stdcall]<nint, int>)p;

                p = Required(hUser32, "GetWindowLongW");
                u32.GetWindowLongW = (delegate* unmanaged[Stdcall]<nint, int, int>)p;

                p = Required(hUser32, "SetWindowLongW");
                u32.SetWindowLongW = (delegate* unmanaged[Stdcall]<nint, int, int, int>)p;

                p = Required(hUser32, "ToUnicode");
                u32.ToUnicode = (delegate* unmanaged[Stdcall]<uint, uint, byte*, char*, int, uint, int>)p;

                p = Required(hUser32, "RegisterDeviceNotificationW");
                u32.RegisterDeviceNotificationW = (delegate* unmanaged[Stdcall]<nint, void*, uint, nint>)p;

                p = Required(hUser32, "UnregisterDeviceNotification");
                u32.UnregisterDeviceNotification = (delegate* unmanaged[Stdcall]<nint, int>)p;


                // --- kernel32.dll ---
                nint hKernel32 = loadModule("kernel32.dll");
                if (hKernel32 == 0)
                    return false;

                var k32 = new Kernel32Core { handle = hKernel32 };
                kernel32 = k32;

                p = Required(hKernel32, "GetModuleHandleW");
                k32.GetModuleHandleW = (delegate* unmanaged[Stdcall]<char*, nint>)p;

                p = Required(hKernel32, "LoadLibraryW");
                k32.LoadLibraryW = (delegate* unmanaged[Stdcall]<char*, nint>)p;

                p = Required(hKernel32, "FreeLibrary");
                k32.FreeLibrary = (delegate* unmanaged[Stdcall]<nint, int>)p;

                p = Required(hKernel32, "GetProcAddress");
                k32.GetProcAddress = (delegate* unmanaged[Stdcall]<nint, byte*, nint>)p;

                p = Required(hKernel32, "GlobalAlloc");
                k32.GlobalAlloc = (delegate* unmanaged[Stdcall]<uint, nuint, nint>)p;

                p = Required(hKernel32, "GlobalLock");
                k32.GlobalLock = (delegate* unmanaged[Stdcall]<nint, nint>)p;

                p = Required(hKernel32, "GlobalUnlock");
                k32.GlobalUnlock = (delegate* unmanaged[Stdcall]<nint, int>)p;

                p = Required(hKernel32, "GlobalFree");
                k32.GlobalFree = (delegate* unmanaged[Stdcall]<nint, nint>)p;

                p = Required(hKernel32, "MultiByteToWideChar");
                k32.MultiByteToWideChar = (delegate* unmanaged[Stdcall]<uint, uint, byte*, int, char*, int, int>)p;

                p = Required(hKernel32, "WideCharToMultiByte");
                k32.WideCharToMultiByte = (delegate* unmanaged[Stdcall]<uint, uint, char*, int, byte*, int, byte*, int*, int>)p;

                p = Required(hKernel32, "GetLastError");
                k32.GetLastError = (delegate* unmanaged[Stdcall]<uint>)p;

                p = Required(hKernel32, "GetModuleFileNameW");
                k32.GetModuleFileNameW = (delegate* unmanaged[Stdcall]<nint, char*, uint, uint>)p;

                p = Required(hKernel32, "Sleep");
                k32.Sleep = (delegate* unmanaged[Stdcall]<uint, void>)p;

                p = Required(hKernel32, "SetThreadExecutionState");
                k32.SetThreadExecutionState = (delegate* unmanaged[Stdcall]<uint, uint>)p;

                p = Required(hKernel32, "GetModuleHandleExW");
                k32.GetModuleHandleExW = (delegate* unmanaged[Stdcall]<uint, char*, nint*, int>)p;

                p = Required(hKernel32, "FormatMessageW");
                k32.FormatMessageW = (delegate* unmanaged[Stdcall]<uint, nint, uint, uint, char*, uint, nint, uint>)p;

                p = Required(hKernel32, "VerSetConditionMask");
                k32.VerSetConditionMask = (delegate* unmanaged[Stdcall]<ulong, uint, byte, ulong>)p;

                p = Required(hKernel32, "GetStartupInfoW");
                k32.GetStartupInfoW = (delegate* unmanaged[Stdcall]<void*, void>)p;


                // --- gdi32.dll ---
                nint hGdi32 = loadModule("gdi32.dll");
                if (hGdi32 == 0)
                    return false;

                var g32 = new Gdi32Core { handle = hGdi32 };
                gdi32 = g32;

                // GetDC and ReleaseDC are exported by user32.
                g32.GetDC = (delegate* unmanaged[Stdcall]<nint, nint>)Required(hUser32, "GetDC");
                g32.ReleaseDC = (delegate* unmanaged[Stdcall]<nint, nint, int>)Required(hUser32, "ReleaseDC");

                p = Required(hGdi32, "SetPixelFormat");
                g32.SetPixelFormat = (delegate* unmanaged[Stdcall]<nint, int, PIXELFORMATDESCRIPTOR*, int>)p;

                p = Required(hGdi32, "ChoosePixelFormat");
                g32.ChoosePixelFormat = (delegate* unmanaged[Stdcall]<nint, PIXELFORMATDESCRIPTOR*, int>)p;

                p = Required(hGdi32, "DescribePixelFormat");
                g32.DescribePixelFormat = (delegate* unmanaged[Stdcall]<nint, int, uint, PIXELFORMATDESCRIPTOR*, int>)p;

                p = Required(hGdi32, "SwapBuffers");
                g32.SwapBuffers = (delegate* unmanaged[Stdcall]<nint, int>)p;

                p = Required(hGdi32, "CreateDIBSection");
                g32.CreateDIBSection = (delegate* unmanaged[Stdcall]<nint, nint, uint, void**, nint, uint, nint>)p;

                p = Required(hGdi32, "DeleteObject");
                g32.DeleteObject = (delegate* unmanaged[Stdcall]<nint, int>)p;

                p = Required(hGdi32, "CreateCompatibleDC");
                g32.CreateCompatibleDC = (delegate* unmanaged[Stdcall]<nint, nint>)p;

                p = Required(hGdi32, "DeleteDC");
                g32.DeleteDC = (delegate* unmanaged[Stdcall]<nint, int>)p;

                p = Required(hGdi32, "CreateBitmap");
                g32.CreateBitmap = (delegate* unmanaged[Stdcall]<int, int, uint, uint, nint, nint>)p;

                p = Required(hGdi32, "GetDeviceCaps");
                g32.GetDeviceCaps = (delegate* unmanaged[Stdcall]<nint, int, int>)p;

                p = Required(hGdi32, "GetDeviceGammaRamp");
                g32.GetDeviceGammaRamp = (delegate* unmanaged[Stdcall]<nint, void*, int>)p;

                p = Required(hGdi32, "SetDeviceGammaRamp");
                g32.SetDeviceGammaRamp = (delegate* unmanaged[Stdcall]<nint, void*, int>)p;

                p = Required(hGdi32, "CreateRectRgn");
                g32.CreateRectRgn = (delegate* unmanaged[Stdcall]<int, int, int, int, nint>)p;

                p = Required(hGdi32, "CreateDCW");
                g32.CreateDCW = (delegate* unmanaged[Stdcall]<char*, char*, nint, nint, nint>)p;


                // --- shell32.dll ---
                nint hShell32 = loadModule("shell32.dll");
                if (hShell32 == 0) return false;
                {
                    var s32 = new Shell32Core { handle = hShell32 };
                    shell32 = s32;

                    p = Required(hShell32, "DragAcceptFiles");
                    s32.DragAcceptFiles = (delegate* unmanaged[Stdcall]<nint, int, void>)p;

                    p = Required(hShell32, "DragQueryFileW");
                    s32.DragQueryFileW = (delegate* unmanaged[Stdcall]<nint, uint, char*, uint, uint>)p;

                    p = Required(hShell32, "DragFinish");
                    s32.DragFinish = (delegate* unmanaged[Stdcall]<nint, void>)p;

                    p = Required(hShell32, "DragQueryPoint");
                    s32.DragQueryPoint = (delegate* unmanaged[Stdcall]<nint, POINT*, int>)p;

                }

                return true;
            }
        }

        // ===============================================================
        //  Load optional user32.dll DPI functions into GlfwLibraryWin32
        // ===============================================================

        public static void LoadOptionalUser32Functions(GlfwLibraryWin32 win32)
        {
            var lib = user32!.handle;

            nint p;

            p = TryLoad(lib, "EnableNonClientDpiScaling");
            win32.user32.EnableNonClientDpiScaling_ = (delegate* unmanaged[Stdcall]<nint, int>)p;

            p = TryLoad(lib, "SetProcessDpiAwarenessContext");
            win32.user32.SetProcessDpiAwarenessContext_ = (delegate* unmanaged[Stdcall]<nint, int>)p;

            p = TryLoad(lib, "GetDpiForWindow");
            win32.user32.GetDpiForWindow_ = (delegate* unmanaged[Stdcall]<nint, uint>)p;

            p = TryLoad(lib, "AdjustWindowRectExForDpi");
            win32.user32.AdjustWindowRectExForDpi_ = (delegate* unmanaged[Stdcall]<RECT*, uint, int, uint, uint, int>)p;

            p = TryLoad(lib, "GetSystemMetricsForDpi");
            win32.user32.GetSystemMetricsForDpi_ = (delegate* unmanaged[Stdcall]<int, uint, int>)p;

            win32.user32.instance = lib;
        }

        // ===============================================================
        //  Load dwmapi.dll (optional, Vista+)
        // ===============================================================

        public static bool LoadDwmapi(GlfwLibraryWin32 win32)
        {
            if (!NativeLibrary.TryLoad("dwmapi.dll", out var lib))
                return false;

            win32.dwmapi.instance = lib;

            nint p;

            p = TryLoad(lib, "DwmIsCompositionEnabled");
            win32.dwmapi.IsCompositionEnabled = (delegate* unmanaged[Stdcall]<int*, int>)p;

            p = TryLoad(lib, "DwmFlush");
            win32.dwmapi.Flush = (delegate* unmanaged[Stdcall]<int>)p;

            p = TryLoad(lib, "DwmEnableBlurBehindWindow");
            win32.dwmapi.EnableBlurBehindWindow = (delegate* unmanaged[Stdcall]<nint, nint, int>)p;

            p = TryLoad(lib, "DwmGetColorizationColor");
            win32.dwmapi.GetColorizationColor = (delegate* unmanaged[Stdcall]<uint*, int*, int>)p;

            p = TryLoad(lib, "DwmSetWindowAttribute");
            win32.dwmapi.SetWindowAttribute = (delegate* unmanaged[Stdcall]<nint, uint, void*, uint, int>)p;

            return true;
        }

        // ===============================================================
        //  Load shcore.dll (optional, Win8.1+)
        // ===============================================================

        public static bool LoadShcore(GlfwLibraryWin32 win32)
        {
            if (!NativeLibrary.TryLoad("shcore.dll", out var lib))
                return false;

            win32.shcore.instance = lib;

            nint p;

            p = TryLoad(lib, "SetProcessDpiAwareness");
            win32.shcore.SetProcessDpiAwareness_ = (delegate* unmanaged[Stdcall]<int, int>)p;

            p = TryLoad(lib, "GetDpiForMonitor");
            win32.shcore.GetDpiForMonitor_ = (delegate* unmanaged[Stdcall]<nint, int, uint*, uint*, int>)p;

            return true;
        }

        // ===============================================================
        //  Load ntdll.dll (optional)
        // ===============================================================

        public static bool LoadNtdll(GlfwLibraryWin32 win32)
        {
            if (!NativeLibrary.TryLoad("ntdll.dll", out var lib))
                return false;

            win32.ntdll.instance = lib;

            nint p;

            p = TryLoad(lib, "RtlVerifyVersionInfo");
            win32.ntdll.RtlVerifyVersionInfo_ = (delegate* unmanaged[Stdcall]<nint, uint, ulong, int>)p;

            return true;
        }

        // ===============================================================
        //  Free all loaded libraries
        // ===============================================================

        private static void TryFreeLibrary(nint handle)
        {
            if (handle == nint.Zero) return;
            NativeLibrary.Free(handle);
        }

        public static void FreeLibraries(GlfwLibraryWin32? win32)
        {
            if (win32 != null)
            {
                // Borrowed from user32.handle; no extra load/reference.
                win32.user32.instance = nint.Zero;
                TryFreeLibrary(win32.dwmapi.instance);
                win32.dwmapi.instance = nint.Zero;

                TryFreeLibrary(win32.shcore.instance);
                win32.shcore.instance = nint.Zero;

                TryFreeLibrary(win32.ntdll.instance);
                win32.ntdll.instance = nint.Zero;

                TryFreeLibrary(win32.dinput8.instance);
                win32.dinput8.instance = nint.Zero;

                TryFreeLibrary(win32.xinput.instance);
                win32.xinput.instance = nint.Zero;
            }

            if (user32 != null)
            {
                TryFreeLibrary(user32.handle);
                user32 = null;
            }
            if (kernel32 != null)
            {
                TryFreeLibrary(kernel32.handle);
                kernel32 = null;
            }
            if (gdi32 != null)
            {
                TryFreeLibrary(gdi32.handle);
                gdi32 = null;
            }
            if (shell32 != null)
            {
                TryFreeLibrary(shell32.handle);
                shell32 = null;
            }
        }
    }
}
