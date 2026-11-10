//========================================================================
// GLFW 3.6 - www.glfw.org
//------------------------------------------------------------------------
// Copyright (c) 2002-2006 Marcus Geelnard
// Copyright (c) 2006-2019 Camilla Löwy <elmindreda@glfw.org>
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
//========================================================================

// Managed port; provenance is in docs/glfw-update.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using static Glfw.GLFW;

namespace Glfw;

public sealed class GlfwGamepadState
{
    public byte[] Buttons = new byte[GLFW_GAMEPAD_BUTTON_LAST + 1];
    public float[] Axes = new float[GLFW_GAMEPAD_AXIS_LAST + 1];
}

internal struct GlfwMapElement
{
    internal byte Type, Index;
    internal sbyte AxisScale, AxisOffset;
}
internal sealed class GlfwMapping
{
    internal string Guid = "", Name = "";
    internal readonly GlfwMapElement[] Buttons = new GlfwMapElement[GLFW_GAMEPAD_BUTTON_LAST + 1];
    internal readonly GlfwMapElement[] Axes = new GlfwMapElement[GLFW_GAMEPAD_AXIS_LAST + 1];
}
public sealed class GlfwJoystick
{
    internal bool Allocated, Connected;
    internal string Name = "", Guid = "";
    internal float[] Axes = Array.Empty<float>();
    internal byte[] Buttons = Array.Empty<byte>(), Hats = Array.Empty<byte>();
    internal int ButtonCount;
    internal object? UserPointer;
    internal GlfwMapping? Mapping;
    internal GlfwJoystickLinux? Linux;
    internal GlfwJoystickWin32? Win32;
}

public static partial class _glfw
{
    internal static readonly GlfwJoystick[] joysticks = new GlfwJoystick[GLFW_JOYSTICK_LAST + 1];
    internal static readonly List<GlfwMapping> mappings = new();
}

public static partial class Glfw
{
    private const byte JoystickAxis = 1, JoystickButton = 2, JoystickHatBit = 3;
    private static bool initJoysticks()
    {
        if (!_glfw.joysticksInitialized && !_glfw.platform!.InitJoysticks())
        {
            _glfw.platform.TerminateJoysticks();
            return false;
        }
        return _glfw.joysticksInitialized = true;
    }
    private static GlfwMapping? findMapping(string guid) => _glfw.mappings.Find(m => m.Guid == guid);
    private static bool isValidElementForJoystick(GlfwMapElement e, GlfwJoystick js)
    {
        if (e.Type == JoystickHatBit && (e.Index >> 4) >= js.Hats.Length) return false;
        if (e.Type == JoystickButton && e.Index >= js.ButtonCount) return false;
        if (e.Type == JoystickAxis && e.Index >= js.Axes.Length) return false;
        return true;
    }
    private static GlfwMapping? findValidMapping(GlfwJoystick js)
    {
        var mapping = findMapping(js.Guid);
        if (mapping != null)
        {
            foreach (var e in mapping.Buttons) if (!isValidElementForJoystick(e, js)) return null;
            foreach (var e in mapping.Axes) if (!isValidElementForJoystick(e, js)) return null;
        }
        return mapping;
    }
    private static readonly string[] MappingButtonNames = { "a", "b", "x", "y", "leftshoulder", "rightshoulder", "back", "start", "guide", "leftstick", "rightstick", "dpup", "dpright", "dpdown", "dpleft" };
    private static readonly string[] MappingAxisNames = { "leftx", "lefty", "rightx", "righty", "lefttrigger", "righttrigger" };
    private static bool parseMapping(GlfwMapping mapping, string text)
    {
        int comma = text.IndexOf(',');
        if (comma != 32) { _glfwInputError(GLFW_INVALID_VALUE, null); return false; }
        mapping.Guid = text[..comma];
        int next = text.IndexOf(',', comma + 1);
        if (next < 0 || Encoding.UTF8.GetByteCount(text.AsSpan(comma + 1, next - comma - 1)) >= 128)
        { _glfwInputError(GLFW_INVALID_VALUE, null); return false; }
        mapping.Name = text[(comma + 1)..next];
        foreach (string field in text[(next + 1)..].Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            if (field[0] == '+' || field[0] == '-') return false; // upstream output modifiers remain unsupported
            int colon = field.IndexOf(':');
            if (colon < 0) continue;
            string name = field[..colon], value = field[(colon + 1)..];
            if (name == "platform")
            {
                if (!value.StartsWith(_glfw.platform!.GetMappingName(), StringComparison.Ordinal)) return false;
                continue;
            }
            int index = Array.IndexOf(MappingButtonNames, name);
            bool axis = index < 0;
            if (axis) index = Array.IndexOf(MappingAxisNames, name);
            if (index < 0 || value.Length == 0) continue;
            int pos = 0, minimum = -1, maximum = 1;
            if (value[pos] == '+') { minimum = 0; pos++; }
            else if (value[pos] == '-') { maximum = 0; pos++; }
            if (pos == value.Length) continue;
            GlfwMapElement e = default;
            e.Type = value[pos] switch { 'a' => JoystickAxis, 'b' => JoystickButton, 'h' => JoystickHatBit, _ => (byte)0 };
            if (e.Type == 0) continue;
            pos++;
            static ulong Number(string s, ref int p)
            {
                // strtoul skips leading white space and accepts an optional sign.
                int start = p;
                while (p < s.Length && " \t\r\n\f\v".IndexOf(s[p]) >= 0) p++;
                bool negative = p < s.Length && s[p] == '-';
                if (p < s.Length && (s[p] == '+' || s[p] == '-')) p++;
                int first = p;
                ulong n = 0, limit = OperatingSystem.IsWindows() ? uint.MaxValue : ulong.MaxValue;
                bool overflow = false;
                while (p < s.Length && s[p] >= '0' && s[p] <= '9')
                {
                    uint digit = (uint)(s[p++] - '0');
                    if (n > (limit - digit) / 10) overflow = true;
                    if (!overflow) n = n * 10 + digit;
                }
                if (p == first) p = start;
                if (overflow) return limit;
                return negative ? unchecked(0UL - n) : n;
            }
            ulong n = Number(value, ref pos);
            if (e.Type == JoystickHatBit)
            {
                if (pos < value.Length) pos++;
                ulong bit = Number(value, ref pos);
                e.Index = unchecked((byte)((n << 4) | bit));
            }
            else e.Index = unchecked((byte)n);
            if (e.Type == JoystickAxis)
            {
                e.AxisScale = (sbyte)(2 / (maximum - minimum));
                e.AxisOffset = (sbyte)-(maximum + minimum);
                if (pos < value.Length && value[pos] == '~') { e.AxisScale = (sbyte)-e.AxisScale; e.AxisOffset = (sbyte)-e.AxisOffset; }
            }
            if (axis) mapping.Axes[index] = e; else mapping.Buttons[index] = e;
        }
        var guid = mapping.Guid.ToCharArray();
        for (int i = 0; i < guid.Length; i++) if (guid[i] >= 'A' && guid[i] <= 'F') guid[i] += (char)('a' - 'A');
        mapping.Guid = _glfw.platform!.UpdateGamepadGUID(new string(guid));
        return true;
    }
    internal static void _glfwInitGamepadMappings()
    {
        _glfw.mappings.Clear();
        for (int i = 0; i < _glfw.joysticks.Length; i++) _glfw.joysticks[i] = new GlfwJoystick();
        // The C array depends on compiled backends, not the selected platform;
        // Null also receives the host's defaults (its mapping name is empty).
        foreach (string text in GlfwDefaultMappings.Get(OperatingSystem.IsWindows() ? "Windows" : OperatingSystem.IsLinux() ? "Linux" : ""))
        {
            var mapping = new GlfwMapping();
            if (parseMapping(mapping, text)) _glfw.mappings.Add(mapping);
        }
    }
    internal static GlfwJoystick? _glfwAllocJoystick(string name, string guid, int axisCount, int buttonCount, int hatCount)
    {
        for (int i = 0; i < _glfw.joysticks.Length; i++)
        {
            if (_glfw.joysticks[i].Allocated) continue;
            var js = new GlfwJoystick { Allocated = true, Name = name, Guid = guid, Axes = new float[axisCount], Buttons = new byte[buttonCount + hatCount * 4], Hats = new byte[hatCount], ButtonCount = buttonCount };
            js.Mapping = findValidMapping(js);
            return _glfw.joysticks[i] = js;
        }
        return null;
    }
    internal static void _glfwFreeJoystick(GlfwJoystick js)
    {
        int index = Array.IndexOf(_glfw.joysticks, js);
        if (index >= 0) _glfw.joysticks[index] = new GlfwJoystick();
        js.Connected = js.Allocated = false;
        js.Axes = Array.Empty<float>(); js.Buttons = js.Hats = Array.Empty<byte>();
        js.Name = js.Guid = ""; js.UserPointer = null; js.Mapping = null; js.Linux = null; js.Win32 = null;
    }
    internal static void _glfwInputJoystick(GlfwJoystick js, int evt)
    {
        js.Connected = evt == GLFW_CONNECTED;
        _glfw.joystickCallback?.Invoke(Array.IndexOf(_glfw.joysticks, js), evt);
    }
    internal static void _glfwInputJoystickAxis(GlfwJoystick js, int axis, float value) { Debug.Assert(float.IsFinite(value)); js.Axes[axis] = value; }
    internal static void _glfwInputJoystickButton(GlfwJoystick js, int button, byte value) => js.Buttons[button] = value;
    internal static void _glfwInputJoystickHat(GlfwJoystick js, int hat, byte value)
    {
        for (int i = 0; i < 4; i++) js.Buttons[js.ButtonCount + hat * 4 + i] = (byte)((value & (1 << i)) != 0 ? GLFW_PRESS : GLFW_RELEASE);
        js.Hats[hat] = value;
    }
    private static GlfwJoystick? GetJoystick(int jid, int mode)
    {
        if (!_glfw.initialized) { _glfwInputError(GLFW_NOT_INITIALIZED, null); return null; }
        if (jid < 0 || jid > GLFW_JOYSTICK_LAST) { _glfwInputError(GLFW_INVALID_ENUM, "Invalid joystick ID {0}", jid); return null; }
        if (!initJoysticks()) return null;
        var js = _glfw.joysticks[jid];
        return js.Connected && _glfw.platform!.PollJoystick(js, mode) ? js : null;
    }
    public static bool glfwJoystickPresent(int jid) => GetJoystick(jid, GlfwConstants._GLFW_POLL_PRESENCE) != null;
    public static float[]? glfwGetJoystickAxes(int jid, out int count) { var js = GetJoystick(jid, GlfwConstants._GLFW_POLL_AXES); count = js?.Axes.Length ?? 0; return js?.Axes; }
    public static byte[]? glfwGetJoystickButtons(int jid, out int count) { var js = GetJoystick(jid, GlfwConstants._GLFW_POLL_BUTTONS); count = js == null ? 0 : js.ButtonCount + (_glfw.hints.Init.HatButtons ? js.Hats.Length * 4 : 0); return js?.Buttons; }
    public static byte[]? glfwGetJoystickHats(int jid, out int count) { var js = GetJoystick(jid, GlfwConstants._GLFW_POLL_BUTTONS); count = js?.Hats.Length ?? 0; return js?.Hats; }
    public static string? glfwGetJoystickName(int jid) => GetJoystick(jid, GlfwConstants._GLFW_POLL_PRESENCE)?.Name;
    public static string? glfwGetJoystickGUID(int jid) => GetJoystick(jid, GlfwConstants._GLFW_POLL_PRESENCE)?.Guid;
    public static void glfwSetJoystickUserPointer(int jid, object? pointer)
    {
        if (!_glfw.initialized) { _glfwInputError(GLFW_NOT_INITIALIZED, null); return; }
        Debug.Assert(jid >= 0 && jid <= GLFW_JOYSTICK_LAST);
        if (_glfw.joysticks[jid].Allocated) _glfw.joysticks[jid].UserPointer = pointer;
    }
    public static object? glfwGetJoystickUserPointer(int jid)
    {
        if (!_glfw.initialized) { _glfwInputError(GLFW_NOT_INITIALIZED, null); return null; }
        Debug.Assert(jid >= 0 && jid <= GLFW_JOYSTICK_LAST);
        return _glfw.joysticks[jid].Allocated ? _glfw.joysticks[jid].UserPointer : null;
    }
    public static GlfwJoystickFun? glfwSetJoystickCallback(GlfwJoystickFun? callback)
    {
        if (!_glfw.initialized) { _glfwInputError(GLFW_NOT_INITIALIZED, null); return null; }
        if (!initJoysticks()) return null;
        var old = _glfw.joystickCallback; _glfw.joystickCallback = callback; return old;
    }
    public static bool glfwUpdateGamepadMappings(string text)
    {
        if (!_glfw.initialized) { _glfwInputError(GLFW_NOT_INITIALIZED, null); return false; }
        foreach (string line in text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (!((line[0] >= '0' && line[0] <= '9') || (line[0] >= 'a' && line[0] <= 'f') || (line[0] >= 'A' && line[0] <= 'F')) || Encoding.UTF8.GetByteCount(line) >= 1024) continue;
            var mapping = new GlfwMapping();
            if (!parseMapping(mapping, line)) continue;
            var previous = findMapping(mapping.Guid);
            if (previous == null) _glfw.mappings.Add(mapping); else _glfw.mappings[_glfw.mappings.IndexOf(previous)] = mapping;
        }
        foreach (var js in _glfw.joysticks) if (js.Connected) js.Mapping = findValidMapping(js);
        return true;
    }
    public static bool glfwJoystickIsGamepad(int jid) => GetJoystick(jid, GlfwConstants._GLFW_POLL_PRESENCE)?.Mapping != null;
    public static string? glfwGetGamepadName(int jid) => GetJoystick(jid, GlfwConstants._GLFW_POLL_PRESENCE)?.Mapping?.Name;
    public static bool glfwGetGamepadState(int jid, out GlfwGamepadState state)
    {
        state = new GlfwGamepadState();
        var js = GetJoystick(jid, GlfwConstants._GLFW_POLL_ALL);
        if (js?.Mapping == null) return false;
        for (int i = 0; i < state.Buttons.Length; i++)
        {
            var e = js.Mapping.Buttons[i];
            if (e.Type == JoystickAxis)
            {
                float value = js.Axes[e.Index] * e.AxisScale + e.AxisOffset;
                if (e.AxisOffset < 0 || (e.AxisOffset == 0 && e.AxisScale > 0)) { if (value >= 0f) state.Buttons[i] = GLFW_PRESS; }
                else if (value <= 0f) state.Buttons[i] = GLFW_PRESS;
            }
            else if (e.Type == JoystickHatBit) { if ((js.Hats[e.Index >> 4] & (e.Index & 15)) != 0) state.Buttons[i] = GLFW_PRESS; }
            else if (e.Type == JoystickButton) state.Buttons[i] = js.Buttons[e.Index];
        }
        for (int i = 0; i < state.Axes.Length; i++)
        {
            var e = js.Mapping.Axes[i];
            if (e.Type == JoystickAxis) state.Axes[i] = MathF.Min(MathF.Max(js.Axes[e.Index] * e.AxisScale + e.AxisOffset, -1f), 1f);
            else if (e.Type == JoystickHatBit) state.Axes[i] = (js.Hats[e.Index >> 4] & (e.Index & 15)) != 0 ? 1f : -1f;
            else if (e.Type == JoystickButton) state.Axes[i] = js.Buttons[e.Index] * 2f - 1f;
        }
        return true;
    }
}
