//========================================================================
// GLFW 3.6 Linux - www.glfw.org
//------------------------------------------------------------------------
// Copyright (c) 2002-2006 Marcus Geelnard
// Copyright (c) 2006-2017 Camilla Löwy <elmindreda@glfw.org>
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
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using static Glfw.GLFW;

namespace Glfw;

internal sealed class GlfwJoystickLinux
{
    internal int Fd;
    internal string Path = "";
    internal readonly int[] KeyMap = new int[0x300 - 0x100];
    internal readonly int[] AbsMap = new int[0x40];
    internal readonly LinuxInputAbsInfo[] AbsInfo = new LinuxInputAbsInfo[0x40];
    internal readonly int[,] Hats = new int[4, 2];
}
[StructLayout(LayoutKind.Sequential)]
internal struct LinuxInputAbsInfo { internal int Value, Minimum, Maximum, Fuzz, Flat, Resolution; }
[StructLayout(LayoutKind.Sequential)]
internal struct LinuxInputEvent { internal nint Seconds, Microseconds; internal ushort Type, Code; internal int Value; }
[StructLayout(LayoutKind.Sequential)]
internal struct LinuxInputId { internal ushort Bus, Vendor, Product, Version; }

public static unsafe partial class Glfw
{
    private static int linuxInotify, linuxWatch;
    private static bool linuxDropped;
    private static readonly Regex LinuxEventName = new("^event[0-9]+$", RegexOptions.CultureInvariant);
    [DllImport("libc", EntryPoint = "open", SetLastError = true)] private static extern int LinuxOpen([MarshalAs(UnmanagedType.LPUTF8Str)] string path, int flags);
    [DllImport("libc", EntryPoint = "close")] private static extern int LinuxClose(int fd);
    [DllImport("libc", EntryPoint = "read", SetLastError = true)] private static extern nint LinuxRead(int fd, void* data, nuint size);
    [DllImport("libc", EntryPoint = "ioctl", SetLastError = true)] private static extern int LinuxIoctl(int fd, nuint request, void* data);
    [DllImport("libc", EntryPoint = "inotify_init1", SetLastError = true)] private static extern int LinuxInotifyInit(int flags);
    [DllImport("libc", EntryPoint = "inotify_add_watch", SetLastError = true)] private static extern int LinuxInotifyAdd(int fd, [MarshalAs(UnmanagedType.LPUTF8Str)] string path, uint mask);
    [DllImport("libc", EntryPoint = "inotify_rm_watch")] private static extern int LinuxInotifyRemove(int fd, int watch);
    private static nuint EvIoRead(int code, int size) => (nuint)(0x80000000U | (uint)size << 16 | (uint)'E' << 8 | (uint)code);
    private static void handleKeyEvent(GlfwJoystick js, int code, int value) => _glfwInputJoystickButton(js, js.Linux!.KeyMap[code - 0x100], (byte)(value != 0 ? GLFW_PRESS : GLFW_RELEASE));
    private static void handleAbsEvent(GlfwJoystick js, int code, int value)
    {
        var lin = js.Linux!;
        int index = lin.AbsMap[code];
        if (code >= 0x10 && code <= 0x17)
        {
            byte[,] states = { { 0, GLFW_HAT_UP, GLFW_HAT_DOWN }, { GLFW_HAT_LEFT, GLFW_HAT_LEFT_UP, GLFW_HAT_LEFT_DOWN }, { GLFW_HAT_RIGHT, GLFW_HAT_RIGHT_UP, GLFW_HAT_RIGHT_DOWN } };
            int hat = (code - 0x10) / 2, axis = (code - 0x10) % 2;
            lin.Hats[hat, axis] = value == 0 ? 0 : value < 0 ? 1 : 2;
            _glfwInputJoystickHat(js, index, states[lin.Hats[hat, 0], lin.Hats[hat, 1]]);
        }
        else
        {
            var info = lin.AbsInfo[code];
            float normalized = value;
            int range = info.Maximum - info.Minimum;
            if (range != 0) { normalized = (normalized - info.Minimum) / range; normalized = normalized * 2f - 1f; }
            _glfwInputJoystickAxis(js, index, normalized);
        }
    }
    private static void pollAbsState(GlfwJoystick js)
    {
        for (int code = 0; code < 0x40; code++)
        {
            if (js.Linux!.AbsMap[code] < 0) continue;
            LinuxInputAbsInfo info;
            if (LinuxIoctl(js.Linux.Fd, EvIoRead(0x40 + code, sizeof(LinuxInputAbsInfo)), &info) < 0) continue;
            js.Linux.AbsInfo[code] = info;
            handleAbsEvent(js, code, info.Value);
        }
    }
    private static bool openJoystickDevice(string path)
    {
        foreach (var js in _glfw.joysticks) if (js.Connected && js.Linux?.Path == path) return false;
        var lin = new GlfwJoystickLinux { Fd = LinuxOpen(path, 0x800 | 0x80000) };
        if (lin.Fd == -1) return false;
        byte* evBits = stackalloc byte[4]; new Span<byte>(evBits, 4).Clear();
        byte* keyBits = stackalloc byte[96]; new Span<byte>(keyBits, 96).Clear();
        byte* absBits = stackalloc byte[8]; new Span<byte>(absBits, 8).Clear();
        LinuxInputId id;
        if (LinuxIoctl(lin.Fd, EvIoRead(0x20, 4), evBits) < 0 || LinuxIoctl(lin.Fd, EvIoRead(0x21, 96), keyBits) < 0 || LinuxIoctl(lin.Fd, EvIoRead(0x23, 8), absBits) < 0 || LinuxIoctl(lin.Fd, EvIoRead(0x02, sizeof(LinuxInputId)), &id) < 0)
        {
            _glfwInputError(GLFW_PLATFORM_ERROR, "Linux: Failed to query input device: {0}", new System.ComponentModel.Win32Exception(Marshal.GetLastPInvokeError()).Message);
            LinuxClose(lin.Fd); return false;
        }
        static bool Bit(int bit, byte* bits) => (bits[bit / 8] & (1 << (bit % 8))) != 0;
        if (!Bit(3, evBits)) { LinuxClose(lin.Fd); return false; }
        byte* nameBytes = stackalloc byte[256]; new Span<byte>(nameBytes, 256).Clear();
        if (LinuxIoctl(lin.Fd, EvIoRead(0x06, 256), nameBytes) < 0) Encoding.UTF8.GetBytes("Unknown", new Span<byte>(nameBytes, 256));
        string name = Marshal.PtrToStringUTF8((nint)nameBytes) ?? "";
        string Word(ushort value) => ((byte)value).ToString("x2") + ((byte)(value >> 8)).ToString("x2") + "0000";
        string guid = id.Vendor != 0 && id.Product != 0 && id.Version != 0 ? Word(id.Bus) + Word(id.Vendor) + Word(id.Product) + Word(id.Version) : Word(id.Bus) + Convert.ToHexString(new ReadOnlySpan<byte>(nameBytes, 11)).ToLowerInvariant() + "00";
        int axisCount = 0, buttonCount = 0, hatCount = 0;
        for (int code = 0x100; code < 0x300; code++) if (Bit(code, keyBits)) lin.KeyMap[code - 0x100] = buttonCount++;
        for (int code = 0; code < 0x40; code++)
        {
            lin.AbsMap[code] = -1;
            if (!Bit(code, absBits)) continue;
            if (code >= 0x10 && code <= 0x17) { lin.AbsMap[code] = hatCount++; code++; }
            else
            {
                LinuxInputAbsInfo info;
                if (LinuxIoctl(lin.Fd, EvIoRead(0x40 + code, sizeof(LinuxInputAbsInfo)), &info) < 0) continue;
                lin.AbsInfo[code] = info; lin.AbsMap[code] = axisCount++;
            }
        }
        var allocated = _glfwAllocJoystick(name, guid, axisCount, buttonCount, hatCount);
        if (allocated == null) { LinuxClose(lin.Fd); return false; }
        lin.Path = path; allocated.Linux = lin;
        pollAbsState(allocated); _glfwInputJoystick(allocated, GLFW_CONNECTED); return true;
    }
    private static void closeJoystickLinux(GlfwJoystick js)
    {
        _glfwInputJoystick(js, GLFW_DISCONNECTED);
        LinuxClose(js.Linux!.Fd); _glfwFreeJoystick(js);
    }
    internal static void _glfwDetectJoystickConnectionLinux()
    {
        if (linuxInotify <= 0) return;
        byte* buffer = stackalloc byte[16384];
        nint size = LinuxRead(linuxInotify, buffer, 16384);
        for (int offset = 0; offset < size;)
        {
            uint mask = *(uint*)(buffer + offset + 4), length = *(uint*)(buffer + offset + 12);
            string name = Marshal.PtrToStringUTF8((nint)(buffer + offset + 16)) ?? "";
            offset += 16 + (int)length;
            if (!LinuxEventName.IsMatch(name)) continue;
            string path = "/dev/input/" + name;
            if ((mask & (0x100 | 0x04)) != 0) openJoystickDevice(path);
            else if ((mask & 0x200) != 0)
                foreach (var js in _glfw.joysticks) if (js.Linux?.Path == path) { closeJoystickLinux(js); break; }
        }
    }
    internal static bool _glfwInitJoysticksLinux()
    {
        linuxDropped = false; linuxWatch = 0;
        linuxInotify = LinuxInotifyInit(0x800 | 0x80000);
        if (linuxInotify > 0) linuxWatch = LinuxInotifyAdd(linuxInotify, "/dev/input", 0x100 | 0x04 | 0x200);
        int count = 0;
        try
        {
            foreach (string path in Directory.EnumerateFileSystemEntries("/dev/input"))
                if (LinuxEventName.IsMatch(Path.GetFileName(path)) && openJoystickDevice(path)) count++;
        }
        catch (IOException) { } catch (UnauthorizedAccessException) { }
        Array.Sort(_glfw.joysticks, 0, count, Comparer<GlfwJoystick>.Create((a, b) => StringComparer.Ordinal.Compare(a.Linux!.Path, b.Linux!.Path)));
        return true;
    }
    internal static void _glfwTerminateJoysticksLinux()
    {
        foreach (var js in _glfw.joysticks) if (js.Connected) closeJoystickLinux(js);
        if (linuxInotify > 0) { if (linuxWatch > 0) LinuxInotifyRemove(linuxInotify, linuxWatch); LinuxClose(linuxInotify); }
        linuxInotify = linuxWatch = 0;
    }
    internal static bool _glfwPollJoystickLinux(GlfwJoystick js, int mode)
    {
        for (;;)
        {
            LinuxInputEvent e;
            nint length = LinuxRead(js.Linux!.Fd, &e, (nuint)sizeof(LinuxInputEvent));
            if (length < 0) { if (Marshal.GetLastPInvokeError() == 19) closeJoystickLinux(js); break; }
            if (length != sizeof(LinuxInputEvent)) break; // no complete event to consume
            if (e.Type == 0)
            {
                if (e.Code == 3) linuxDropped = true;
                else if (e.Code == 0) { linuxDropped = false; pollAbsState(js); }
            }
            if (linuxDropped) continue;
            if (e.Type == 1) handleKeyEvent(js, e.Code, e.Value);
            else if (e.Type == 3) handleAbsEvent(js, e.Code, e.Value);
        }
        return js.Connected;
    }
}
