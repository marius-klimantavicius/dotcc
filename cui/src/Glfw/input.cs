// Ported from glfw/src/input.c — GLFW 3.6 input event handlers and public API
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

using System.Diagnostics;

namespace Glfw;

public static partial class Glfw
{
    // Internal key state used for sticky keys
    private const byte _GLFW_STICK = 3;

    private const int GLFW_MOD_MASK =
        GLFW.GLFW_MOD_SHIFT |
        GLFW.GLFW_MOD_CONTROL |
        GLFW.GLFW_MOD_ALT |
        GLFW.GLFW_MOD_SUPER |
        GLFW.GLFW_MOD_CAPS_LOCK |
        GLFW.GLFW_MOD_NUM_LOCK;

    //----------------------------------------------------------------------
    //                         GLFW event API
    //----------------------------------------------------------------------

    /// <summary>
    /// Notifies shared code of a physical key event.
    /// Ported from _glfwInputKey.
    /// </summary>
    internal static void _glfwInputKey(GlfwWindow window, int key, int scancode, int action, int mods)
    {
        Debug.Assert(window != null);
        Debug.Assert(key >= 0 || key == GLFW.GLFW_KEY_UNKNOWN);
        Debug.Assert(key <= GLFW.GLFW_KEY_LAST);
        Debug.Assert(action == GLFW.GLFW_PRESS || action == GLFW.GLFW_RELEASE);
        Debug.Assert(mods == (mods & GLFW_MOD_MASK));

        if (key >= 0 && key <= GLFW.GLFW_KEY_LAST)
        {
            bool repeated = false;

            if (action == GLFW.GLFW_RELEASE && window.Keys[key] == GLFW.GLFW_RELEASE)
                return;

            if (action == GLFW.GLFW_PRESS && window.Keys[key] == GLFW.GLFW_PRESS)
                repeated = true;

            if (action == GLFW.GLFW_RELEASE && window.StickyKeys)
                window.Keys[key] = _GLFW_STICK;
            else
                window.Keys[key] = (byte)action;

            if (repeated)
                action = GLFW.GLFW_REPEAT;
        }

        if (!window.LockKeyMods)
            mods &= ~(GLFW.GLFW_MOD_CAPS_LOCK | GLFW.GLFW_MOD_NUM_LOCK);

        window.Callbacks.Key?.Invoke(window, key, scancode, action, mods);
    }

    /// <summary>
    /// Notifies shared code of a Unicode codepoint input event.
    /// The 'plain' parameter determines whether to emit a regular character event.
    /// Ported from _glfwInputChar.
    /// </summary>
    internal static void _glfwInputChar(GlfwWindow window, uint codepoint, int mods, bool plain)
    {
        Debug.Assert(window != null);
        Debug.Assert(mods == (mods & GLFW_MOD_MASK));

        if (codepoint < 32 || (codepoint > 126 && codepoint < 160))
            return;

        if (!window.LockKeyMods)
            mods &= ~(GLFW.GLFW_MOD_CAPS_LOCK | GLFW.GLFW_MOD_NUM_LOCK);

        window.Callbacks.Charmods?.Invoke(window, codepoint, mods);

        if (plain)
        {
            window.Callbacks.Character?.Invoke(window, codepoint);
        }
    }

    /// <summary>
    /// Notifies shared code of a scroll event.
    /// Ported from _glfwInputScroll.
    /// </summary>
    internal static void _glfwInputScroll(GlfwWindow window, double xoffset, double yoffset)
    {
        Debug.Assert(window != null);
        Debug.Assert(double.IsFinite(xoffset));
        Debug.Assert(double.IsFinite(yoffset));

        window.Callbacks.Scroll?.Invoke(window, xoffset, yoffset);
    }

    /// <summary>
    /// Notifies shared code of a mouse button click event.
    /// Ported from _glfwInputMouseClick.
    /// </summary>
    internal static void _glfwInputMouseClick(GlfwWindow window, int button, int action, int mods)
    {
        Debug.Assert(window != null);
        Debug.Assert(button >= 0);
        Debug.Assert(action == GLFW.GLFW_PRESS || action == GLFW.GLFW_RELEASE);
        Debug.Assert(mods == (mods & GLFW_MOD_MASK));

        if (button < 0 || (!window.DisableMouseButtonLimit && button > GLFW.GLFW_MOUSE_BUTTON_LAST))
            return;

        if (!window.LockKeyMods)
            mods &= ~(GLFW.GLFW_MOD_CAPS_LOCK | GLFW.GLFW_MOD_NUM_LOCK);

        if (button <= GLFW.GLFW_MOUSE_BUTTON_LAST)
        {
            if (action == GLFW.GLFW_RELEASE && window.StickyMouseButtons)
                window.MouseButtons[button] = _GLFW_STICK;
            else
                window.MouseButtons[button] = (byte)action;
        }

        window.Callbacks.MouseButton?.Invoke(window, button, action, mods);
    }

    /// <summary>
    /// Notifies shared code of a cursor motion event.
    /// The position is specified in content area relative screen coordinates.
    /// Ported from _glfwInputCursorPos.
    /// </summary>
    internal static void _glfwInputCursorPos(GlfwWindow window, double xpos, double ypos)
    {
        Debug.Assert(window != null);
        Debug.Assert(double.IsFinite(xpos));
        Debug.Assert(double.IsFinite(ypos));

        if (window.VirtualCursorPosX == xpos && window.VirtualCursorPosY == ypos)
            return;

        window.VirtualCursorPosX = xpos;
        window.VirtualCursorPosY = ypos;

        window.Callbacks.CursorPos?.Invoke(window, xpos, ypos);
    }

    /// <summary>
    /// Notifies shared code of a cursor enter/leave event.
    /// Ported from _glfwInputCursorEnter.
    /// </summary>
    internal static void _glfwInputCursorEnter(GlfwWindow window, bool entered)
    {
        Debug.Assert(window != null);

        window.Callbacks.CursorEnter?.Invoke(window, entered ? GLFW.GLFW_TRUE : GLFW.GLFW_FALSE);
    }

    /// <summary>
    /// Notifies shared code of files or directories dropped on a window.
    /// Ported from _glfwInputDrop.
    /// </summary>
    internal static void _glfwInputDrop(GlfwWindow window, int count, string[] paths)
    {
        Debug.Assert(window != null);
        Debug.Assert(count > 0);
        Debug.Assert(paths != null);

        window.Callbacks.Drop?.Invoke(window, count, paths);
    }

    //----------------------------------------------------------------------
    //                       GLFW internal API
    //----------------------------------------------------------------------

    /// <summary>
    /// Center the cursor in the content area of the specified window.
    /// Ported from _glfwCenterCursorInContentArea.
    /// </summary>
    internal static void _glfwCenterCursorInContentArea(GlfwWindow window)
    {
        _glfw.platform!.GetWindowSize(window, out int width, out int height);
        _glfw.platform.SetCursorPos(window, width / 2.0, height / 2.0);
    }

    //----------------------------------------------------------------------
    //                        GLFW public API
    //----------------------------------------------------------------------

    /// <summary>
    /// Returns the value of an input mode for the specified window.
    /// Ported from glfwGetInputMode.
    /// </summary>
    public static int glfwGetInputMode(GlfwWindow window, int mode)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return 0;
        }

        Debug.Assert(window != null);

        switch (mode)
        {
            case GLFW.GLFW_CURSOR:
                return window.CursorMode;
            case GLFW.GLFW_STICKY_KEYS:
                return window.StickyKeys ? GLFW.GLFW_TRUE : GLFW.GLFW_FALSE;
            case GLFW.GLFW_STICKY_MOUSE_BUTTONS:
                return window.StickyMouseButtons ? GLFW.GLFW_TRUE : GLFW.GLFW_FALSE;
            case GLFW.GLFW_LOCK_KEY_MODS:
                return window.LockKeyMods ? GLFW.GLFW_TRUE : GLFW.GLFW_FALSE;
            case GLFW.GLFW_RAW_MOUSE_MOTION:
                return window.RawMouseMotion ? GLFW.GLFW_TRUE : GLFW.GLFW_FALSE;
            case GLFW.GLFW_UNLIMITED_MOUSE_BUTTONS:
                return window.DisableMouseButtonLimit ? GLFW.GLFW_TRUE : GLFW.GLFW_FALSE;
        }

        _glfwInputError(GLFW.GLFW_INVALID_ENUM, "Invalid input mode 0x{0:X8}", mode);
        return 0;
    }

    /// <summary>
    /// Sets an input mode for the specified window.
    /// Ported from glfwSetInputMode.
    /// </summary>
    public static void glfwSetInputMode(GlfwWindow window, int mode, int value)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        Debug.Assert(window != null);

        switch (mode)
        {
            case GLFW.GLFW_CURSOR:
            {
                if (value != GLFW.GLFW_CURSOR_NORMAL &&
                    value != GLFW.GLFW_CURSOR_HIDDEN &&
                    value != GLFW.GLFW_CURSOR_DISABLED &&
                    value != GLFW.GLFW_CURSOR_CAPTURED)
                {
                    _glfwInputError(GLFW.GLFW_INVALID_ENUM,
                                    "Invalid cursor mode 0x{0:X8}", value);
                    return;
                }

                if (window.CursorMode == value)
                    return;

                window.CursorMode = value;

                _glfw.platform!.GetCursorPos(window,
                                             out window.VirtualCursorPosX,
                                             out window.VirtualCursorPosY);
                _glfw.platform.SetCursorMode(window, value);
                return;
            }

            case GLFW.GLFW_STICKY_KEYS:
            {
                bool boolValue = value != 0;
                if (window.StickyKeys == boolValue)
                    return;

                if (!boolValue)
                {
                    // Release all sticky keys
                    for (int i = 0; i <= GLFW.GLFW_KEY_LAST; i++)
                    {
                        if (window.Keys[i] == _GLFW_STICK)
                            window.Keys[i] = GLFW.GLFW_RELEASE;
                    }
                }

                window.StickyKeys = boolValue;
                return;
            }

            case GLFW.GLFW_STICKY_MOUSE_BUTTONS:
            {
                bool boolValue = value != 0;
                if (window.StickyMouseButtons == boolValue)
                    return;

                if (!boolValue)
                {
                    // Release all sticky mouse buttons
                    for (int i = 0; i <= GLFW.GLFW_MOUSE_BUTTON_LAST; i++)
                    {
                        if (window.MouseButtons[i] == _GLFW_STICK)
                            window.MouseButtons[i] = GLFW.GLFW_RELEASE;
                    }
                }

                window.StickyMouseButtons = boolValue;
                return;
            }

            case GLFW.GLFW_LOCK_KEY_MODS:
            {
                window.LockKeyMods = value != 0;
                return;
            }

            case GLFW.GLFW_RAW_MOUSE_MOTION:
            {
                if (!_glfw.platform!.RawMouseMotionSupported())
                {
                    _glfwInputError(GLFW.GLFW_PLATFORM_ERROR,
                                    "Raw mouse motion is not supported on this system");
                    return;
                }

                bool boolValue = value != 0;
                if (window.RawMouseMotion == boolValue)
                    return;

                window.RawMouseMotion = boolValue;
                _glfw.platform.SetRawMouseMotion(window, boolValue);
                return;
            }

            case GLFW.GLFW_UNLIMITED_MOUSE_BUTTONS:
            {
                window.DisableMouseButtonLimit = value != 0;
                return;
            }
        }

        _glfwInputError(GLFW.GLFW_INVALID_ENUM, "Invalid input mode 0x{0:X8}", mode);
    }

    /// <summary>
    /// Returns whether raw mouse motion is supported on the current system.
    /// Ported from glfwRawMouseMotionSupported.
    /// </summary>
    public static int glfwRawMouseMotionSupported()
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return GLFW.GLFW_FALSE;
        }

        return _glfw.platform!.RawMouseMotionSupported() ? GLFW.GLFW_TRUE : GLFW.GLFW_FALSE;
    }

    /// <summary>
    /// Returns the layout-specific name of the specified printable key.
    /// Ported from glfwGetKeyName.
    /// </summary>
    public static string? glfwGetKeyName(int key, int scancode)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return null;
        }

        if (key != GLFW.GLFW_KEY_UNKNOWN)
        {
            if (key < GLFW.GLFW_KEY_SPACE || key > GLFW.GLFW_KEY_LAST)
            {
                _glfwInputError(GLFW.GLFW_INVALID_ENUM, "Invalid key {0}", key);
                return null;
            }

            if (key != GLFW.GLFW_KEY_KP_EQUAL &&
                (key < GLFW.GLFW_KEY_KP_0 || key > GLFW.GLFW_KEY_KP_ADD) &&
                (key < GLFW.GLFW_KEY_APOSTROPHE || key > GLFW.GLFW_KEY_WORLD_2))
            {
                return null;
            }

            scancode = _glfw.platform!.GetKeyScancode(key);
        }

        return _glfw.platform!.GetScancodeName(scancode);
    }

    /// <summary>
    /// Returns the platform-specific scancode of the specified key.
    /// Ported from glfwGetKeyScancode.
    /// </summary>
    public static int glfwGetKeyScancode(int key)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return 0;
        }

        if (key < GLFW.GLFW_KEY_SPACE || key > GLFW.GLFW_KEY_LAST)
        {
            _glfwInputError(GLFW.GLFW_INVALID_ENUM, "Invalid key {0}", key);
            return -1;
        }

        return _glfw.platform!.GetKeyScancode(key);
    }

    /// <summary>
    /// Returns the last reported state of a keyboard key for the specified window.
    /// Ported from glfwGetKey.
    /// </summary>
    public static int glfwGetKey(GlfwWindow window, int key)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return GLFW.GLFW_RELEASE;
        }

        Debug.Assert(window != null);

        if (key < GLFW.GLFW_KEY_SPACE || key > GLFW.GLFW_KEY_LAST)
        {
            _glfwInputError(GLFW.GLFW_INVALID_ENUM, "Invalid key {0}", key);
            return GLFW.GLFW_RELEASE;
        }

        if (window.Keys[key] == _GLFW_STICK)
        {
            // Sticky mode: release key now
            window.Keys[key] = GLFW.GLFW_RELEASE;
            return GLFW.GLFW_PRESS;
        }

        return window.Keys[key];
    }

    /// <summary>
    /// Returns the last reported state of a mouse button for the specified window.
    /// Ported from glfwGetMouseButton.
    /// </summary>
    public static int glfwGetMouseButton(GlfwWindow window, int button)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return GLFW.GLFW_RELEASE;
        }

        Debug.Assert(window != null);

        if (button < GLFW.GLFW_MOUSE_BUTTON_1 || button > GLFW.GLFW_MOUSE_BUTTON_LAST)
        {
            _glfwInputError(GLFW.GLFW_INVALID_ENUM, "Invalid mouse button {0}", button);
            return GLFW.GLFW_RELEASE;
        }

        if (window.MouseButtons[button] == _GLFW_STICK)
        {
            // Sticky mode: release mouse button now
            window.MouseButtons[button] = GLFW.GLFW_RELEASE;
            return GLFW.GLFW_PRESS;
        }

        return window.MouseButtons[button];
    }

    /// <summary>
    /// Retrieves the position of the cursor relative to the content area of the window.
    /// Ported from glfwGetCursorPos.
    /// </summary>
    public static void glfwGetCursorPos(GlfwWindow window, out double xpos, out double ypos)
    {
        xpos = 0;
        ypos = 0;

        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        Debug.Assert(window != null);

        if (window.CursorMode == GLFW.GLFW_CURSOR_DISABLED)
        {
            xpos = window.VirtualCursorPosX;
            ypos = window.VirtualCursorPosY;
        }
        else
        {
            _glfw.platform!.GetCursorPos(window, out xpos, out ypos);
        }
    }

    /// <summary>
    /// Sets the position of the cursor, relative to the content area of the window.
    /// Ported from glfwSetCursorPos.
    /// </summary>
    public static void glfwSetCursorPos(GlfwWindow window, double xpos, double ypos)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        Debug.Assert(window != null);

        if (!double.IsFinite(xpos) || !double.IsFinite(ypos))
        {
            _glfwInputError(GLFW.GLFW_INVALID_VALUE,
                            "Invalid cursor position {0} {1}", xpos, ypos);
            return;
        }

        if (!_glfw.platform!.WindowFocused(window))
            return;

        if (window.CursorMode == GLFW.GLFW_CURSOR_DISABLED)
        {
            // Only update the accumulated position if the cursor is disabled
            window.VirtualCursorPosX = xpos;
            window.VirtualCursorPosY = ypos;
        }
        else
        {
            // Update system cursor position
            _glfw.platform.SetCursorPos(window, xpos, ypos);
        }
    }

    /// <summary>
    /// Creates a custom cursor.
    /// Ported from glfwCreateCursor.
    /// </summary>
    public static GlfwCursor? glfwCreateCursor(in GlfwImage image, int xhot, int yhot)
    {
        Debug.Assert(image.Pixels != null);

        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return null;
        }

        if (image.Width <= 0 || image.Height <= 0)
        {
            _glfwInputError(GLFW.GLFW_INVALID_VALUE, "Invalid image dimensions for cursor");
            return null;
        }

        var cursor = new GlfwCursor();
        cursor.Next = _glfw.cursorListHead;
        _glfw.cursorListHead = cursor;

        if (!_glfw.platform!.CreateCursor(cursor, in image, xhot, yhot))
        {
            glfwDestroyCursor(cursor);
            return null;
        }

        return cursor;
    }

    /// <summary>
    /// Creates a cursor with a standard shape.
    /// Ported from glfwCreateStandardCursor.
    /// </summary>
    public static GlfwCursor? glfwCreateStandardCursor(int shape)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return null;
        }

        if (shape != GLFW.GLFW_ARROW_CURSOR &&
            shape != GLFW.GLFW_IBEAM_CURSOR &&
            shape != GLFW.GLFW_CROSSHAIR_CURSOR &&
            shape != GLFW.GLFW_POINTING_HAND_CURSOR &&
            shape != GLFW.GLFW_RESIZE_EW_CURSOR &&
            shape != GLFW.GLFW_RESIZE_NS_CURSOR &&
            shape != GLFW.GLFW_RESIZE_NWSE_CURSOR &&
            shape != GLFW.GLFW_RESIZE_NESW_CURSOR &&
            shape != GLFW.GLFW_RESIZE_ALL_CURSOR &&
            shape != GLFW.GLFW_NOT_ALLOWED_CURSOR)
        {
            _glfwInputError(GLFW.GLFW_INVALID_ENUM, "Invalid standard cursor 0x{0:X8}", shape);
            return null;
        }

        var cursor = new GlfwCursor();
        cursor.Next = _glfw.cursorListHead;
        _glfw.cursorListHead = cursor;

        if (!_glfw.platform!.CreateStandardCursor(cursor, shape))
        {
            glfwDestroyCursor(cursor);
            return null;
        }

        return cursor;
    }

    /// <summary>
    /// Destroys a cursor.
    /// Ported from glfwDestroyCursor.
    /// </summary>
    public static void glfwDestroyCursor(GlfwCursor? cursor)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        if (cursor == null)
            return;

        // Make sure the cursor is not being used by any window
        for (var window = _glfw.windowListHead; window != null; window = window.Next)
        {
            if (window.Cursor == cursor)
                glfwSetCursor(window, null);
        }

        _glfw.platform!.DestroyCursor(cursor);

        // Unlink cursor from global linked list
        if (_glfw.cursorListHead == cursor)
        {
            _glfw.cursorListHead = cursor.Next;
        }
        else
        {
            var prev = _glfw.cursorListHead;
            while (prev != null && prev.Next != cursor)
                prev = prev.Next;
            if (prev != null)
                prev.Next = cursor.Next;
        }
    }

    /// <summary>
    /// Sets the cursor for the window.
    /// Ported from glfwSetCursor.
    /// </summary>
    public static void glfwSetCursor(GlfwWindow window, GlfwCursor? cursor)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        Debug.Assert(window != null);

        window.Cursor = cursor;

        _glfw.platform!.SetCursor(window, cursor);
    }

    /// <summary>
    /// Sets the key callback.
    /// Returns the previously set callback, or null.
    /// Ported from glfwSetKeyCallback.
    /// </summary>
    public static GlfwKeyFun? glfwSetKeyCallback(GlfwWindow window, GlfwKeyFun? cbfun)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return null;
        }

        Debug.Assert(window != null);

        var previous = window.Callbacks.Key;
        window.Callbacks.Key = cbfun;
        return previous;
    }

    /// <summary>
    /// Sets the Unicode character callback.
    /// Returns the previously set callback, or null.
    /// Ported from glfwSetCharCallback.
    /// </summary>
    public static GlfwCharFun? glfwSetCharCallback(GlfwWindow window, GlfwCharFun? cbfun)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return null;
        }

        Debug.Assert(window != null);

        var previous = window.Callbacks.Character;
        window.Callbacks.Character = cbfun;
        return previous;
    }

    /// <summary>
    /// Sets the Unicode character with modifiers callback.
    /// Returns the previously set callback, or null.
    /// Ported from glfwSetCharModsCallback.
    /// </summary>
    public static GlfwCharModsFun? glfwSetCharModsCallback(GlfwWindow window, GlfwCharModsFun? cbfun)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return null;
        }

        Debug.Assert(window != null);

        var previous = window.Callbacks.Charmods;
        window.Callbacks.Charmods = cbfun;
        return previous;
    }

    /// <summary>
    /// Sets the mouse button callback.
    /// Returns the previously set callback, or null.
    /// Ported from glfwSetMouseButtonCallback.
    /// </summary>
    public static GlfwMouseButtonFun? glfwSetMouseButtonCallback(GlfwWindow window, GlfwMouseButtonFun? cbfun)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return null;
        }

        Debug.Assert(window != null);

        var previous = window.Callbacks.MouseButton;
        window.Callbacks.MouseButton = cbfun;
        return previous;
    }

    /// <summary>
    /// Sets the cursor position callback.
    /// Returns the previously set callback, or null.
    /// Ported from glfwSetCursorPosCallback.
    /// </summary>
    public static GlfwCursorPosFun? glfwSetCursorPosCallback(GlfwWindow window, GlfwCursorPosFun? cbfun)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return null;
        }

        Debug.Assert(window != null);

        var previous = window.Callbacks.CursorPos;
        window.Callbacks.CursorPos = cbfun;
        return previous;
    }

    /// <summary>
    /// Sets the cursor enter/leave callback.
    /// Returns the previously set callback, or null.
    /// Ported from glfwSetCursorEnterCallback.
    /// </summary>
    public static GlfwCursorEnterFun? glfwSetCursorEnterCallback(GlfwWindow window, GlfwCursorEnterFun? cbfun)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return null;
        }

        Debug.Assert(window != null);

        var previous = window.Callbacks.CursorEnter;
        window.Callbacks.CursorEnter = cbfun;
        return previous;
    }

    /// <summary>
    /// Sets the scroll callback.
    /// Returns the previously set callback, or null.
    /// Ported from glfwSetScrollCallback.
    /// </summary>
    public static GlfwScrollFun? glfwSetScrollCallback(GlfwWindow window, GlfwScrollFun? cbfun)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return null;
        }

        Debug.Assert(window != null);

        var previous = window.Callbacks.Scroll;
        window.Callbacks.Scroll = cbfun;
        return previous;
    }

    /// <summary>
    /// Sets the file drop callback.
    /// Returns the previously set callback, or null.
    /// Ported from glfwSetDropCallback.
    /// </summary>
    public static GlfwDropFun? glfwSetDropCallback(GlfwWindow window, GlfwDropFun? cbfun)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return null;
        }

        Debug.Assert(window != null);

        var previous = window.Callbacks.Drop;
        window.Callbacks.Drop = cbfun;
        return previous;
    }

    /// <summary>
    /// Sets the clipboard to the specified string.
    /// Ported from glfwSetClipboardString.
    /// </summary>
    public static void glfwSetClipboardString(GlfwWindow? window, string text)
    {
        Debug.Assert(text != null);

        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        _glfw.platform!.SetClipboardString(text);
    }

    /// <summary>
    /// Returns the contents of the clipboard as a string.
    /// Ported from glfwGetClipboardString.
    /// </summary>
    public static string? glfwGetClipboardString(GlfwWindow? window)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return null;
        }

        return _glfw.platform!.GetClipboardString();
    }

    /// <summary>
    /// Returns the GLFW time.
    /// Ported from glfwGetTime.
    /// </summary>
    public static double glfwGetTime()
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return 0.0;
        }

        return (double)(_glfwPlatformGetTimerValue() - _glfw.timer.Offset) /
            _glfwPlatformGetTimerFrequency();
    }

    /// <summary>
    /// Sets the GLFW time.
    /// Ported from glfwSetTime.
    /// </summary>
    public static void glfwSetTime(double time)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        if (!double.IsFinite(time) || time < 0.0 || time > 18446744073.0)
        {
            _glfwInputError(GLFW.GLFW_INVALID_VALUE, "Invalid time {0}", time);
            return;
        }

        _glfw.timer.Offset = _glfwPlatformGetTimerValue() -
            (ulong)(time * _glfwPlatformGetTimerFrequency());
    }

    /// <summary>
    /// Returns the current value of the raw timer.
    /// Ported from glfwGetTimerValue.
    /// </summary>
    public static ulong glfwGetTimerValue()
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return 0;
        }

        return _glfwPlatformGetTimerValue();
    }

    /// <summary>
    /// Returns the frequency, in Hz, of the raw timer.
    /// Ported from glfwGetTimerFrequency.
    /// </summary>
    public static ulong glfwGetTimerFrequency()
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return 0;
        }

        return _glfwPlatformGetTimerFrequency();
    }

    //----------------------------------------------------------------------
    // Timer platform functions (using System.Diagnostics.Stopwatch)
    //----------------------------------------------------------------------

    /// <summary>
    /// Returns the current value of the high-resolution timer.
    /// Ported from _glfwPlatformGetTimerValue.
    /// </summary>
    internal static ulong _glfwPlatformGetTimerValue()
    {
        return (ulong)Stopwatch.GetTimestamp();
    }

    /// <summary>
    /// Returns the frequency of the high-resolution timer.
    /// Ported from _glfwPlatformGetTimerFrequency.
    /// </summary>
    internal static ulong _glfwPlatformGetTimerFrequency()
    {
        return (ulong)Stopwatch.Frequency;
    }
}
