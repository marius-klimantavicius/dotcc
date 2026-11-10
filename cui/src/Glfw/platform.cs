// Ported from glfw/src/internal.h (_GLFWplatform struct, lines 680-761)
// and glfw/src/platform.c (_glfwSelectPlatform)

using System;
using System.Runtime.InteropServices;

namespace Glfw;

/// <summary>
/// Platform ID constants from glfw3.h.
/// </summary>
public static class GlfwPlatformId
{
    public const int AnyPlatform = 0x00060000;
    public const int Win32       = 0x00060001;
    public const int Cocoa       = 0x00060002;
    public const int Wayland     = 0x00060003;
    public const int X11         = 0x00060004;
    public const int Null        = 0x00060005;
}

// IGlfwPlatform interface is defined in internal.cs.

/// <summary>
/// Selects and connects the appropriate platform implementation.
/// Ported from glfw/src/platform.c <c>_glfwSelectPlatform</c>.
/// </summary>
public static class GlfwPlatformSelector
{
    /// <summary>
    /// Delegate matching the C++ <c>connect</c> function pointer used in the
    /// <c>supportedPlatforms</c> table.  Each platform module provides one of
    /// these; it returns <c>null</c> if the platform is not available at
    /// runtime and a fully wired <see cref="IGlfwPlatform"/> otherwise.
    /// </summary>
    public delegate IGlfwPlatform? PlatformConnector(int platformId);

    /// <summary>
    /// Entry in the supported-platforms table.
    /// </summary>
    public readonly record struct PlatformEntry(int Id, PlatformConnector Connect);

    /// <summary>
    /// Builds the supported-platforms table at runtime using
    /// <see cref="RuntimeInformation.IsOSPlatform"/>.
    /// The Null platform is handled separately and is always available.
    /// </summary>
    internal static PlatformEntry[] BuildSupportedPlatforms()
    {
        var list = new System.Collections.Generic.List<PlatformEntry>();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            list.Add(new PlatformEntry(GlfwPlatformId.Win32, ConnectWin32));
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // Cocoa connector would be registered here once ported.
            // list.Add(new PlatformEntry(GlfwPlatformId.Cocoa, ConnectCocoa));
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // Both Wayland and X11 may be available on Linux.
            // list.Add(new PlatformEntry(GlfwPlatformId.Wayland, ConnectWayland));
            list.Add(new PlatformEntry(GlfwPlatformId.X11, ConnectX11));
        }

        return list.ToArray();
    }

    /// <summary>
    /// Connects the Null (headless) platform.
    /// Delegates to <see cref="NullPlatformConnector.Connect"/>.
    /// </summary>
    private static IGlfwPlatform? ConnectNull(int platformId)
    {
        return NullPlatformConnector.Connect(platformId);
    }

    /// <summary>
    /// Connects the Win32 platform on Windows.
    /// Delegates to <see cref="Win32PlatformConnector.Connect"/>.
    /// </summary>
    private static IGlfwPlatform? ConnectWin32(int platformId)
    {
        return Win32PlatformConnector.Connect(platformId);
    }

    /// <summary>
    /// Connects the X11 platform on Linux.
    /// Delegates to <see cref="X11PlatformConnector.Connect"/>.
    /// </summary>
    private static IGlfwPlatform? ConnectX11(int platformId)
    {
        return X11PlatformConnector.Connect(platformId);
    }

    /// <summary>
    /// Selects a platform, mirroring the logic of the C++ <c>_glfwSelectPlatform</c>.
    /// </summary>
    /// <param name="desiredId">
    /// One of the <see cref="GlfwPlatformId"/> constants, or
    /// <see cref="GlfwPlatformId.AnyPlatform"/> for automatic detection.
    /// </param>
    /// <returns>
    /// A connected <see cref="IGlfwPlatform"/> instance, or <c>null</c> if no
    /// suitable platform could be found.
    /// </returns>
    public static IGlfwPlatform? SelectPlatform(int desiredId)
    {
        if (desiredId != GlfwPlatformId.AnyPlatform &&
            desiredId != GlfwPlatformId.Win32 &&
            desiredId != GlfwPlatformId.Cocoa &&
            desiredId != GlfwPlatformId.Wayland &&
            desiredId != GlfwPlatformId.X11 &&
            desiredId != GlfwPlatformId.Null)
        {
            Glfw._glfwInputError(GLFW.GLFW_INVALID_ENUM, "Invalid platform ID 0x{0:X8}", desiredId);
            return null;
        }

        // Only allow the Null platform if specifically requested
        if (desiredId == GlfwPlatformId.Null)
            return ConnectNull(desiredId);

        PlatformEntry[] supported = BuildSupportedPlatforms();

        if (supported.Length == 0)
        {
            Glfw._glfwInputError(GLFW.GLFW_PLATFORM_UNAVAILABLE, "This binary only supports the Null platform");
            return null;
        }

        // On Linux, check XDG_SESSION_TYPE to prefer Wayland or X11
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) &&
            desiredId == GlfwPlatformId.AnyPlatform)
        {
            string? session = Environment.GetEnvironmentVariable("XDG_SESSION_TYPE");
            if (session != null)
            {
                if (Array.Exists(supported, p => p.Id == GlfwPlatformId.Wayland) && session == "wayland" &&
                    Environment.GetEnvironmentVariable("WAYLAND_DISPLAY") != null)
                {
                    desiredId = GlfwPlatformId.Wayland;
                }
                else if (session == "x11" &&
                         Environment.GetEnvironmentVariable("DISPLAY") != null)
                {
                    desiredId = GlfwPlatformId.X11;
                }
            }
        }

        if (desiredId == GlfwPlatformId.AnyPlatform)
        {
            // If there is exactly one platform, let it emit the error on failure
            if (supported.Length == 1)
                return supported[0].Connect(supported[0].Id);

            // Try each platform in order
            foreach (var entry in supported)
            {
                IGlfwPlatform? platform = entry.Connect(desiredId);
                if (platform != null)
                    return platform;
            }

            Glfw._glfwInputError(GLFW.GLFW_PLATFORM_UNAVAILABLE, "Failed to detect any supported platform");
            return null;
        }
        else
        {
            // A specific platform was requested
            foreach (var entry in supported)
            {
                if (entry.Id == desiredId)
                    return entry.Connect(desiredId);
            }

            Glfw._glfwInputError(GLFW.GLFW_PLATFORM_UNAVAILABLE, "The requested platform is not supported");
            return null;
        }
    }
}

public static partial class Glfw
{
    public static int glfwGetPlatform()
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return 0;
        }
        return _glfw.platform!.PlatformID;
    }

    public static bool glfwPlatformSupported(int platformID)
    {
        if (platformID == GLFW.GLFW_PLATFORM_NULL)
            return true;
        if (platformID != GLFW.GLFW_PLATFORM_WIN32 && platformID != GLFW.GLFW_PLATFORM_COCOA &&
            platformID != GLFW.GLFW_PLATFORM_WAYLAND && platformID != GLFW.GLFW_PLATFORM_X11)
        {
            _glfwInputError(GLFW.GLFW_INVALID_ENUM, "Invalid platform ID 0x{0:X8}", platformID);
            return false;
        }
        foreach (var entry in GlfwPlatformSelector.BuildSupportedPlatforms())
            if (entry.Id == platformID) return true;
        return false;
    }

    public static string glfwGetVersionString()
    {
        string platforms = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? " Win32 WGL" :
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? " X11 GLX" : "";
        return "3.6.0" + platforms + " Null EGL OSMesa .NET";
    }
}
