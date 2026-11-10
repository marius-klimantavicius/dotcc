//========================================================================
// GLFW 3.6 Win32 - www.glfw.org
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using static Glfw.GLFW;

namespace Glfw;

internal sealed class GlfwJoystickWin32
{
    internal nint Device;
    internal uint Index;
    internal Guid Guid;
    internal List<GlfwJoyObjectWin32> Objects = new();
}
internal struct GlfwJoyObjectWin32 { internal int Offset, Type; }

public static unsafe partial class Glfw
{
    [StructLayout(LayoutKind.Sequential)] private struct DIObjectFormat { internal Guid* Guid; internal uint Offset, Type, Flags; }
    [StructLayout(LayoutKind.Sequential)] private struct DIDataFormat { internal uint Size, ObjectSize, Flags, DataSize, Count; internal DIObjectFormat* Objects; }
    [StructLayout(LayoutKind.Sequential)] private struct DIDeviceInstance { internal uint Size; internal Guid Instance, Product; internal uint Type; internal fixed char InstanceName[260]; internal fixed char ProductName[260]; internal Guid FFDriver; internal ushort UsagePage, Usage; }
    [StructLayout(LayoutKind.Sequential)] private struct DIDeviceObject { internal uint Size; internal Guid Guid; internal uint Offset, Type, Flags; internal fixed char Name[260]; internal uint FFMaxForce, FFForceResolution; internal ushort Collection, Designator, UsagePage, Usage; internal uint Dimension; internal ushort Exponent, ReportId; }
    [StructLayout(LayoutKind.Sequential)] private struct DICaps { internal uint Size, Flags, Type, Axes, Buttons, POVs, FFSamplePeriod, FFMinTimeResolution, FirmwareRevision, HardwareRevision, FFDriverVersion; }
    [StructLayout(LayoutKind.Sequential)] private struct DIPropertyHeader { internal uint Size, HeaderSize, Object, How; }
    [StructLayout(LayoutKind.Sequential)] private struct DIPropertyRange { internal DIPropertyHeader Header; internal int Minimum, Maximum; }
    [StructLayout(LayoutKind.Sequential)] private struct DIPropertyDword { internal DIPropertyHeader Header; internal uint Data; }
    [StructLayout(LayoutKind.Sequential)] private struct XInputGamepad { internal ushort Buttons; internal byte LeftTrigger, RightTrigger; internal short LX, LY, RX, RY; }
    [StructLayout(LayoutKind.Sequential)] private struct XInputState { internal uint Packet; internal XInputGamepad Gamepad; }
    [StructLayout(LayoutKind.Sequential)] private struct XInputCapabilities { internal byte Type, SubType; internal ushort Flags; internal XInputGamepad Gamepad; internal ushort LeftMotor, RightMotor; }
    [StructLayout(LayoutKind.Sequential)] private struct RawDeviceList { internal nint Device; internal uint Type; }
    [StructLayout(LayoutKind.Explicit, Size = 32)] private struct RawDeviceInfo { [FieldOffset(0)] internal uint Size; [FieldOffset(4)] internal uint Type; [FieldOffset(8)] internal uint Vendor; [FieldOffset(12)] internal uint Product; }
    private sealed class DIEnumeration { internal nint Device; internal readonly List<GlfwJoyObjectWin32> Objects = new(); internal int Axes, Sliders, Buttons, POVs; }
    private static readonly Guid DIInterfaceGuid = new("bf798031-483a-4da2-aa99-5d64ed369700");
    private static readonly Guid[] DIObjectGuids = {
        new("a36d02e0-c9f3-11cf-bfc7-444553540000"), new("a36d02e1-c9f3-11cf-bfc7-444553540000"), new("a36d02e2-c9f3-11cf-bfc7-444553540000"),
        new("a36d02f4-c9f3-11cf-bfc7-444553540000"), new("a36d02f5-c9f3-11cf-bfc7-444553540000"), new("a36d02e3-c9f3-11cf-bfc7-444553540000"),
        new("a36d02e4-c9f3-11cf-bfc7-444553540000"), new("a36d02f2-c9f3-11cf-bfc7-444553540000") };
    [DllImport("user32.dll")] private static extern uint GetRawInputDeviceList(RawDeviceList* list, uint* count, uint size);
    [DllImport("user32.dll", EntryPoint = "GetRawInputDeviceInfoA")] private static extern uint GetRawInputDeviceInfo(nint device, uint command, void* data, uint* size);
    private static nint VTable(nint instance, int slot) => (*(nint**)instance)[slot];
    private static void DIRelease(nint instance) { if (instance != 0) ((delegate* unmanaged[Stdcall]<nint, uint>)VTable(instance, 2))(instance); }
    private static int DIInvoke(nint device, int slot) => ((delegate* unmanaged[Stdcall]<nint, int>)VTable(device, slot))(device);
    private static int DISetProperty(nint device, nuint property, DIPropertyHeader* header) => ((delegate* unmanaged[Stdcall]<nint, nuint, DIPropertyHeader*, int>)VTable(device, 6))(device, property, header);
    private static string getDeviceDescription(XInputCapabilities capabilities) => capabilities.SubType switch
    {
        2 => "XInput Wheel", 3 => "XInput Arcade Stick", 4 => "XInput Flight Stick", 5 => "XInput Dance Pad", 6 => "XInput Guitar", 8 => "XInput Drum Kit",
        1 => (capabilities.Flags & 2) != 0 ? "Wireless Xbox Controller" : "Xbox Controller", _ => "Unknown XInput Device"
    };
    private static bool supportsXInput(Guid guid)
    {
        uint count = 0;
        if (GetRawInputDeviceList(null, &count, (uint)sizeof(RawDeviceList)) != 0) return false;
        var devices = new RawDeviceList[count];
        fixed (RawDeviceList* ptr = devices)
        {
            if (GetRawInputDeviceList(ptr, &count, (uint)sizeof(RawDeviceList)) == uint.MaxValue) return false;
            byte* name = stackalloc byte[256];
            uint product = *(uint*)&guid;
            for (int i = 0; i < count; i++)
            {
                if (ptr[i].Type != 2) continue;
                RawDeviceInfo info = default; info.Size = (uint)sizeof(RawDeviceInfo);
                uint size = (uint)sizeof(RawDeviceInfo);
                if (GetRawInputDeviceInfo(ptr[i].Device, 0x2000000b, &info, &size) == uint.MaxValue) continue;
                if ((info.Vendor | info.Product << 16) != product) continue;
                new Span<byte>(name, 256).Clear(); size = 256;
                if (GetRawInputDeviceInfo(ptr[i].Device, 0x20000007, name, &size) == uint.MaxValue) break;
                name[255] = 0;
                if ((Marshal.PtrToStringAnsi((nint)name) ?? "").Contains("IG_", StringComparison.Ordinal)) return true;
            }
        }
        return false;
    }
    private static void closeJoystickWin32(GlfwJoystick js)
    {
        _glfwInputJoystick(js, GLFW_DISCONNECTED);
        if (js.Win32?.Device != 0 && js.Win32 != null) { DIInvoke(js.Win32.Device, 8); DIRelease(js.Win32.Device); }
        _glfwFreeJoystick(js);
    }
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int deviceObjectCallback(DIDeviceObject* doi, nint user)
    {
        var data = (DIEnumeration)GCHandle.FromIntPtr(user).Target!;
        GlfwJoyObjectWin32 obj = default;
        uint type = doi->Type & 0xff;
        if ((type & 3) != 0)
        {
            int axis = Array.IndexOf(DIObjectGuids, doi->Guid);
            if (axis == 6) obj.Offset = 24 + data.Sliders * 4;
            else if (axis >= 0 && axis < 6) obj.Offset = axis * 4;
            else return 1;
            DIPropertyRange range = default;
            range.Header.Size = (uint)sizeof(DIPropertyRange); range.Header.HeaderSize = (uint)sizeof(DIPropertyHeader);
            range.Header.Object = doi->Type; range.Header.How = 2; range.Minimum = -32768; range.Maximum = 32767;
            if (DISetProperty(data.Device, 4, &range.Header) < 0) return 1;
            if (axis == 6) { obj.Type = 1; data.Sliders++; } else { obj.Type = 0; data.Axes++; }
        }
        else if ((type & 0x0c) != 0) { obj.Offset = 48 + data.Buttons++; obj.Type = 2; }
        else if ((type & 0x10) != 0) { obj.Offset = 32 + data.POVs++ * 4; obj.Type = 3; }
        data.Objects.Add(obj);
        return 1;
    }
    private static int SetDIDataFormat(nint device)
    {
        fixed (Guid* guids = DIObjectGuids)
        {
            DIObjectFormat* objects = stackalloc DIObjectFormat[44];
            for (int i = 0; i < 44; i++)
            {
                objects[i] = default;
                if (i < 8) { objects[i].Guid = guids + Math.Min(i, 6); objects[i].Offset = (uint)i * 4; objects[i].Type = 3 | 0x80000000 | 0x00ffff00; objects[i].Flags = 0x100; }
                else if (i < 12) { objects[i].Guid = guids + 7; objects[i].Offset = (uint)i * 4; objects[i].Type = 0x10 | 0x80000000 | 0x00ffff00; }
                else { objects[i].Offset = (uint)(48 + i - 12); objects[i].Type = 0x0c | 0x80000000 | 0x00ffff00; }
            }
            DIDataFormat format = new() { Size = (uint)sizeof(DIDataFormat), ObjectSize = (uint)sizeof(DIObjectFormat), Flags = 2, DataSize = 80, Count = 44, Objects = objects };
            return ((delegate* unmanaged[Stdcall]<nint, DIDataFormat*, int>)VTable(device, 11))(device, &format);
        }
    }
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int deviceCallback(DIDeviceInstance* di, nint user)
    {
        foreach (var connected in _glfw.joysticks) if (connected.Connected && connected.Win32?.Guid == di->Instance) return 1;
        if (supportsXInput(di->Product)) return 1;
        nint api = _glfw.Win32!.dinput8.api, device;
        if (((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, nint, int>)VTable(api, 3))(api, &di->Instance, &device, 0) < 0)
        { _glfwInputError(GLFW_PLATFORM_ERROR, "Win32: Failed to create device"); return 1; }
        if (SetDIDataFormat(device) < 0) { _glfwInputError(GLFW_PLATFORM_ERROR, "Win32: Failed to set device data format"); DIRelease(device); return 1; }
        DICaps caps = default; caps.Size = (uint)sizeof(DICaps);
        if (((delegate* unmanaged[Stdcall]<nint, DICaps*, int>)VTable(device, 3))(device, &caps) < 0)
        { _glfwInputError(GLFW_PLATFORM_ERROR, "Win32: Failed to query device capabilities"); DIRelease(device); return 1; }
        DIPropertyDword mode = default; mode.Header.Size = (uint)sizeof(DIPropertyDword); mode.Header.HeaderSize = (uint)sizeof(DIPropertyHeader);
        if (DISetProperty(device, 2, &mode.Header) < 0) { _glfwInputError(GLFW_PLATFORM_ERROR, "Win32: Failed to set device axis mode"); DIRelease(device); return 1; }
        var data = new DIEnumeration { Device = device };
        var handle = GCHandle.Alloc(data);
        int result;
        try { result = ((delegate* unmanaged[Stdcall]<nint, nint, nint, uint, int>)VTable(device, 4))(device, (nint)(delegate* unmanaged[Stdcall]<DIDeviceObject*, nint, int>)&deviceObjectCallback, GCHandle.ToIntPtr(handle), 0x1f); }
        finally { handle.Free(); }
        if (result < 0) { _glfwInputError(GLFW_PLATFORM_ERROR, "Win32: Failed to enumerate device objects"); DIRelease(device); return 1; }
        data.Objects.Sort((a, b) => a.Type != b.Type ? a.Type - b.Type : a.Offset - b.Offset);
        string name = new(di->InstanceName);
        // WideCharToMultiByte(CP_UTF8, 0, ...) replaces malformed surrogates.
        byte[] bytes = Encoding.UTF8.GetBytes(name);
        if (bytes.Length >= 256) { _glfwInputError(GLFW_PLATFORM_ERROR, "Win32: Failed to convert joystick name to UTF-8"); DIRelease(device); return 0; }
        var product = di->Product.ToByteArray();
        string guid;
        if (Encoding.ASCII.GetString(product, 10, 6) == "PIDVID") guid = "03000000" + Convert.ToHexString(product, 0, 2).ToLowerInvariant() + "0000" + Convert.ToHexString(product, 2, 2).ToLowerInvariant() + "000000000000";
        else { var prefix = new byte[11]; Array.Copy(bytes, prefix, Math.Min(bytes.Length, 11)); guid = "05000000" + Convert.ToHexString(prefix).ToLowerInvariant() + "00"; }
        var js = _glfwAllocJoystick(name, guid, data.Axes + data.Sliders, data.Buttons, data.POVs);
        if (js == null) { DIRelease(device); return 0; }
        js.Win32 = new GlfwJoystickWin32 { Device = device, Guid = di->Instance, Objects = data.Objects };
        _glfwInputJoystick(js, GLFW_CONNECTED); return 1;
    }
    internal static void _glfwDetectJoystickConnectionWin32()
    {
        var win32 = _glfw.Win32!;
        if (win32.xinput.instance != 0)
        {
            for (uint index = 0; index < 4; index++)
            {
                bool present = false;
                foreach (var js in _glfw.joysticks) if (js.Connected && js.Win32?.Device == 0 && js.Win32.Index == index) { present = true; break; }
                if (present) continue;
                XInputCapabilities capabilities;
                if (win32.xinput.GetCapabilities(index, 0, (nint)(&capabilities)) != 0) continue;
                var allocated = _glfwAllocJoystick(getDeviceDescription(capabilities), "78696e707574" + capabilities.SubType.ToString("x2") + "000000000000000000", 6, 10, 1);
                if (allocated == null) continue;
                allocated.Win32 = new GlfwJoystickWin32 { Index = index };
                _glfwInputJoystick(allocated, GLFW_CONNECTED);
            }
        }
        if (win32.dinput8.api != 0 && ((delegate* unmanaged[Stdcall]<nint, uint, nint, nint, uint, int>)VTable(win32.dinput8.api, 4))(win32.dinput8.api, 4, (nint)(delegate* unmanaged[Stdcall]<DIDeviceInstance*, nint, int>)&deviceCallback, 0, 0) < 0)
            _glfwInputError(GLFW_PLATFORM_ERROR, "Failed to enumerate DirectInput8 devices");
    }
    internal static void _glfwDetectJoystickDisconnectionWin32()
    { foreach (var js in _glfw.joysticks) if (js.Connected) _glfwPollJoystickWin32(js, GlfwConstants._GLFW_POLL_PRESENCE); }
    internal static bool _glfwInitJoysticksWin32()
    {
        var win32 = _glfw.Win32!;
        if (win32.dinput8.instance != 0)
        {
            nint api; Guid iid = DIInterfaceGuid;
            if (((delegate* unmanaged[Stdcall]<nint, uint, Guid*, nint*, nint, int>)win32.dinput8.Create)(win32.instance, 0x0800, &iid, &api, 0) < 0)
            { _glfwInputError(GLFW_PLATFORM_ERROR, "Win32: Failed to create interface"); return false; }
            win32.dinput8.api = api;
        }
        _glfwDetectJoystickConnectionWin32(); return true;
    }
    internal static void _glfwTerminateJoysticksWin32()
    {
        foreach (var js in _glfw.joysticks) closeJoystickWin32(js);
        DIRelease(_glfw.Win32!.dinput8.api); _glfw.Win32.dinput8.api = 0;
    }
    internal static bool _glfwPollJoystickWin32(GlfwJoystick js, int mode)
    {
        var win = js.Win32!;
        if (win.Device != 0)
        {
            byte* state = stackalloc byte[80]; new Span<byte>(state, 80).Clear();
            DIInvoke(win.Device, 25);
            var getState = (delegate* unmanaged[Stdcall]<nint, uint, void*, int>)VTable(win.Device, 9);
            int result = getState(win.Device, 80, state);
            if (result == unchecked((int)0x8007000c) || result == unchecked((int)0x8007001e))
            { DIInvoke(win.Device, 7); DIInvoke(win.Device, 25); result = getState(win.Device, 80, state); }
            if (result < 0) { closeJoystickWin32(js); return false; }
            if (mode == GlfwConstants._GLFW_POLL_PRESENCE) return true;
            int axis = 0, button = 0, hat = 0;
            ReadOnlySpan<byte> hats = new byte[] { GLFW_HAT_UP, GLFW_HAT_RIGHT_UP, GLFW_HAT_RIGHT, GLFW_HAT_RIGHT_DOWN, GLFW_HAT_DOWN, GLFW_HAT_LEFT_DOWN, GLFW_HAT_LEFT, GLFW_HAT_LEFT_UP, GLFW_HAT_CENTERED };
            foreach (var obj in win.Objects)
            {
                byte* p = state + obj.Offset;
                if (obj.Type <= 1) _glfwInputJoystickAxis(js, axis++, (*(int*)p + 0.5f) / 32767.5f);
                else if (obj.Type == 2) _glfwInputJoystickButton(js, button++, (byte)((*p & 0x80) != 0 ? 1 : 0));
                else if (obj.Type == 3) { int index = *(ushort*)p / 4500; if (index > 8) index = 8; _glfwInputJoystickHat(js, hat++, hats[index]); }
            }
        }
        else
        {
            XInputState state;
            uint result = _glfw.Win32!.xinput.GetState(win.Index, (nint)(&state));
            if (result != 0) { if (result == 1167) closeJoystickWin32(js); return false; }
            if (mode == GlfwConstants._GLFW_POLL_PRESENCE) return true;
            _glfwInputJoystickAxis(js, 0, (state.Gamepad.LX + 0.5f) / 32767.5f);
            _glfwInputJoystickAxis(js, 1, -(state.Gamepad.LY + 0.5f) / 32767.5f);
            _glfwInputJoystickAxis(js, 2, (state.Gamepad.RX + 0.5f) / 32767.5f);
            _glfwInputJoystickAxis(js, 3, -(state.Gamepad.RY + 0.5f) / 32767.5f);
            _glfwInputJoystickAxis(js, 4, state.Gamepad.LeftTrigger / 127.5f - 1f);
            _glfwInputJoystickAxis(js, 5, state.Gamepad.RightTrigger / 127.5f - 1f);
            ReadOnlySpan<ushort> buttons = new ushort[] { 0x1000, 0x2000, 0x4000, 0x8000, 0x100, 0x200, 0x20, 0x10, 0x40, 0x80 };
            for (int i = 0; i < 10; i++) _glfwInputJoystickButton(js, i, (byte)((state.Gamepad.Buttons & buttons[i]) != 0 ? 1 : 0));
            int dpad = 0;
            if ((state.Gamepad.Buttons & 1) != 0) dpad |= GLFW_HAT_UP;
            if ((state.Gamepad.Buttons & 8) != 0) dpad |= GLFW_HAT_RIGHT;
            if ((state.Gamepad.Buttons & 2) != 0) dpad |= GLFW_HAT_DOWN;
            if ((state.Gamepad.Buttons & 4) != 0) dpad |= GLFW_HAT_LEFT;
            if ((dpad & GLFW_HAT_RIGHT) != 0 && (dpad & GLFW_HAT_LEFT) != 0) dpad &= ~(GLFW_HAT_RIGHT | GLFW_HAT_LEFT);
            if ((dpad & GLFW_HAT_UP) != 0 && (dpad & GLFW_HAT_DOWN) != 0) dpad &= ~(GLFW_HAT_UP | GLFW_HAT_DOWN);
            _glfwInputJoystickHat(js, 0, (byte)dpad);
        }
        return true;
    }
    internal static string _glfwUpdateGamepadGUIDWin32(string guid) => guid.AsSpan(20).SequenceEqual("504944564944") ? "03000000" + guid[..4] + "0000" + guid.Substring(4, 4) + "000000000000" : guid;
}
