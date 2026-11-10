// GLFW 3.6 - www.glfw.org
// Ported from glfw/src/context.c
//
// Copyright (c) 2002-2006 Marcus Geelnard
// Copyright (c) 2006-2016 Camilla Loewy <elmindreda@glfw.org>
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

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Glfw;

// -----------------------------------------------------------------------
// GlfwContext  (was _GLFWcontext)
// -----------------------------------------------------------------------

/// <summary>
/// Context structure. Corresponds to C++ <c>_GLFWcontext</c>.
/// Holds resolved context attributes and GL function pointers.
/// </summary>
public unsafe partial class GlfwContext
{
    public int Client;
    public int Source;
    public int Major, Minor, Revision;

    public bool Forward, Debug, Noerror;

    public int Profile;
    public int Robustness;
    public int Release;

    // GL function pointers loaded via GetProcAddress
    // glGetString(GLenum) -> const GLubyte*
    internal delegate* unmanaged<uint, nint> GetString;
    // glGetIntegerv(GLenum, GLint*)
    internal delegate* unmanaged<uint, int*, void> GetIntegerv;
    // glGetStringi(GLenum, GLuint) -> const GLubyte*
    internal delegate* unmanaged<uint, uint, nint> GetStringi;
    // glFlush(void)
    internal delegate* unmanaged<void> Flush;

    // Context operation delegates (set by platform backend)
    public Action<GlfwWindow?>? makeCurrent;
    public Action<GlfwWindow>? swapBuffers;
    public Action<int>? swapInterval;
    public Func<string, bool>? extensionSupported;
    public Func<string, nint>? getProcAddress;
    public Action<GlfwWindow>? destroy;
}

// -----------------------------------------------------------------------
// Context functions from context.c
// -----------------------------------------------------------------------

public static unsafe partial class Glfw
{
    // GL constants from internal.h, scoped here to avoid namespace/class ambiguity
    // with GlfwInternal (which lives in the Glfw namespace, shadowed by Glfw class).
    private const uint GL_VERSION                          = 0x1f02;
    private const uint GL_NONE                             = 0;
    private const uint GL_COLOR_BUFFER_BIT                 = 0x00004000;
    private const uint GL_EXTENSIONS                       = 0x1f03;
    private const uint GL_NUM_EXTENSIONS                   = 0x821d;
    private const uint GL_CONTEXT_FLAGS                    = 0x821e;
    private const uint GL_CONTEXT_FLAG_FORWARD_COMPATIBLE_BIT = 0x00000001;
    private const uint GL_CONTEXT_FLAG_DEBUG_BIT           = 0x00000002;
    private const uint GL_CONTEXT_PROFILE_MASK             = 0x9126;
    private const uint GL_CONTEXT_COMPATIBILITY_PROFILE_BIT = 0x00000002;
    private const uint GL_CONTEXT_CORE_PROFILE_BIT         = 0x00000001;
    private const uint GL_RESET_NOTIFICATION_STRATEGY_ARB  = 0x8256;
    private const uint GL_LOSE_CONTEXT_ON_RESET_ARB        = 0x8252;
    private const uint GL_NO_RESET_NOTIFICATION_ARB        = 0x8261;
    private const uint GL_CONTEXT_RELEASE_BEHAVIOR         = 0x82fb;
    private const uint GL_CONTEXT_RELEASE_BEHAVIOR_FLUSH   = 0x82fc;
    private const uint GL_CONTEXT_FLAG_NO_ERROR_BIT_KHR    = 0x00000008;

    //////////////////////////////////////////////////////////////////////////
    //////                       GLFW internal API                      //////
    //////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Checks whether the desired context attributes are valid.
    /// Ported from <c>_glfwIsValidContextConfig</c>.
    /// </summary>
    internal static bool _glfwIsValidContextConfig(GlfwCtxConfig ctxconfig)
    {
        if (ctxconfig.Source != GLFW.GLFW_NATIVE_CONTEXT_API &&
            ctxconfig.Source != GLFW.GLFW_EGL_CONTEXT_API &&
            ctxconfig.Source != GLFW.GLFW_OSMESA_CONTEXT_API)
        {
            _glfwInputError(GLFW.GLFW_INVALID_ENUM,
                "Invalid context creation API 0x{0:X8}", ctxconfig.Source);
            return false;
        }

        if (ctxconfig.Client != GLFW.GLFW_NO_API &&
            ctxconfig.Client != GLFW.GLFW_OPENGL_API &&
            ctxconfig.Client != GLFW.GLFW_OPENGL_ES_API)
        {
            _glfwInputError(GLFW.GLFW_INVALID_ENUM,
                "Invalid client API 0x{0:X8}", ctxconfig.Client);
            return false;
        }

        if (ctxconfig.Share != null)
        {
            if (ctxconfig.Client == GLFW.GLFW_NO_API ||
                ctxconfig.Share.context.Client == GLFW.GLFW_NO_API)
            {
                _glfwInputError(GLFW.GLFW_NO_WINDOW_CONTEXT, null);
                return false;
            }

            if (ctxconfig.Source != ctxconfig.Share.context.Source)
            {
                _glfwInputError(GLFW.GLFW_INVALID_ENUM,
                    "Context creation APIs do not match between contexts");
                return false;
            }
        }

        if (ctxconfig.Client == GLFW.GLFW_OPENGL_API)
        {
            if ((ctxconfig.Major < 1 || ctxconfig.Minor < 0) ||
                (ctxconfig.Major == 1 && ctxconfig.Minor > 5) ||
                (ctxconfig.Major == 2 && ctxconfig.Minor > 1) ||
                (ctxconfig.Major == 3 && ctxconfig.Minor > 3))
            {
                // OpenGL 1.0 is the smallest valid version
                // OpenGL 1.x series ended with version 1.5
                // OpenGL 2.x series ended with version 2.1
                // OpenGL 3.x series ended with version 3.3
                // For now, let everything else through

                _glfwInputError(GLFW.GLFW_INVALID_VALUE,
                    "Invalid OpenGL version {0}.{1}",
                    ctxconfig.Major, ctxconfig.Minor);
                return false;
            }

            if (ctxconfig.Profile != 0)
            {
                if (ctxconfig.Profile != GLFW.GLFW_OPENGL_CORE_PROFILE &&
                    ctxconfig.Profile != GLFW.GLFW_OPENGL_COMPAT_PROFILE)
                {
                    _glfwInputError(GLFW.GLFW_INVALID_ENUM,
                        "Invalid OpenGL profile 0x{0:X8}", ctxconfig.Profile);
                    return false;
                }

                if (ctxconfig.Major <= 2 ||
                    (ctxconfig.Major == 3 && ctxconfig.Minor < 2))
                {
                    // Desktop OpenGL context profiles are only defined for version 3.2
                    // and above

                    _glfwInputError(GLFW.GLFW_INVALID_VALUE,
                        "Context profiles are only defined for OpenGL version 3.2 and above");
                    return false;
                }
            }

            if (ctxconfig.Forward && ctxconfig.Major <= 2)
            {
                // Forward-compatible contexts are only defined for OpenGL version 3.0
                // and above
                _glfwInputError(GLFW.GLFW_INVALID_VALUE,
                    "Forward-compatibility is only defined for OpenGL version 3.0 and above");
                return false;
            }
        }
        else if (ctxconfig.Client == GLFW.GLFW_OPENGL_ES_API)
        {
            if (ctxconfig.Major < 1 || ctxconfig.Minor < 0 ||
                (ctxconfig.Major == 1 && ctxconfig.Minor > 1) ||
                (ctxconfig.Major == 2 && ctxconfig.Minor > 0))
            {
                // OpenGL ES 1.0 is the smallest valid version
                // OpenGL ES 1.x series ended with version 1.1
                // OpenGL ES 2.x series ended with version 2.0
                // For now, let everything else through

                _glfwInputError(GLFW.GLFW_INVALID_VALUE,
                    "Invalid OpenGL ES version {0}.{1}",
                    ctxconfig.Major, ctxconfig.Minor);
                return false;
            }
        }

        if (ctxconfig.Robustness != 0)
        {
            if (ctxconfig.Robustness != GLFW.GLFW_NO_RESET_NOTIFICATION &&
                ctxconfig.Robustness != GLFW.GLFW_LOSE_CONTEXT_ON_RESET)
            {
                _glfwInputError(GLFW.GLFW_INVALID_ENUM,
                    "Invalid context robustness mode 0x{0:X8}", ctxconfig.Robustness);
                return false;
            }
        }

        if (ctxconfig.Release != 0)
        {
            if (ctxconfig.Release != GLFW.GLFW_RELEASE_BEHAVIOR_NONE &&
                ctxconfig.Release != GLFW.GLFW_RELEASE_BEHAVIOR_FLUSH)
            {
                _glfwInputError(GLFW.GLFW_INVALID_ENUM,
                    "Invalid context release behavior 0x{0:X8}", ctxconfig.Release);
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Chooses the framebuffer config that best matches the desired one.
    /// Ported from <c>_glfwChooseFBConfig</c>.
    /// </summary>
    /// <returns>The index of the best match in <paramref name="alternatives"/>, or -1 if none.</returns>
    internal static int _glfwChooseFBConfig(GlfwFbConfig desired,
                                            GlfwFbConfig[] alternatives,
                                            int count)
    {
        uint leastMissing = uint.MaxValue;
        uint leastColorDiff = uint.MaxValue;
        uint leastExtraDiff = uint.MaxValue;
        int closestIndex = -1;

        for (int i = 0; i < count; i++)
        {
            var current = alternatives[i];

            if (desired.Stereo && !current.Stereo)
            {
                // Stereo is a hard constraint
                continue;
            }

            // Count number of missing buffers
            uint missing = 0;

            if (desired.AlphaBits > 0 && current.AlphaBits == 0)
                missing++;

            if (desired.DepthBits > 0 && current.DepthBits == 0)
                missing++;

            if (desired.StencilBits > 0 && current.StencilBits == 0)
                missing++;

            if (desired.AuxBuffers > 0 &&
                current.AuxBuffers < desired.AuxBuffers)
            {
                missing += (uint)(desired.AuxBuffers - current.AuxBuffers);
            }

            if (desired.Samples > 0 && current.Samples == 0)
            {
                // Technically, several multisampling buffers could be
                // involved, but that's a lower level implementation detail and
                // not important to us here, so we count them as one
                missing++;
            }

            if (desired.Transparent != current.Transparent)
                missing++;

            // These polynomials make many small channel size differences matter
            // less than one large channel size difference

            // Calculate color channel size difference value
            uint colorDiff = 0;

            if (desired.RedBits != GLFW.GLFW_DONT_CARE)
            {
                colorDiff += (uint)((desired.RedBits - current.RedBits) *
                                    (desired.RedBits - current.RedBits));
            }

            if (desired.GreenBits != GLFW.GLFW_DONT_CARE)
            {
                colorDiff += (uint)((desired.GreenBits - current.GreenBits) *
                                    (desired.GreenBits - current.GreenBits));
            }

            if (desired.BlueBits != GLFW.GLFW_DONT_CARE)
            {
                colorDiff += (uint)((desired.BlueBits - current.BlueBits) *
                                    (desired.BlueBits - current.BlueBits));
            }

            // Calculate non-color channel size difference value
            uint extraDiff = 0;

            if (desired.AlphaBits != GLFW.GLFW_DONT_CARE)
            {
                extraDiff += (uint)((desired.AlphaBits - current.AlphaBits) *
                                    (desired.AlphaBits - current.AlphaBits));
            }

            if (desired.DepthBits != GLFW.GLFW_DONT_CARE)
            {
                extraDiff += (uint)((desired.DepthBits - current.DepthBits) *
                                    (desired.DepthBits - current.DepthBits));
            }

            if (desired.StencilBits != GLFW.GLFW_DONT_CARE)
            {
                extraDiff += (uint)((desired.StencilBits - current.StencilBits) *
                                    (desired.StencilBits - current.StencilBits));
            }

            if (desired.AccumRedBits != GLFW.GLFW_DONT_CARE)
            {
                extraDiff += (uint)((desired.AccumRedBits - current.AccumRedBits) *
                                    (desired.AccumRedBits - current.AccumRedBits));
            }

            if (desired.AccumGreenBits != GLFW.GLFW_DONT_CARE)
            {
                extraDiff += (uint)((desired.AccumGreenBits - current.AccumGreenBits) *
                                    (desired.AccumGreenBits - current.AccumGreenBits));
            }

            if (desired.AccumBlueBits != GLFW.GLFW_DONT_CARE)
            {
                extraDiff += (uint)((desired.AccumBlueBits - current.AccumBlueBits) *
                                    (desired.AccumBlueBits - current.AccumBlueBits));
            }

            if (desired.AccumAlphaBits != GLFW.GLFW_DONT_CARE)
            {
                extraDiff += (uint)((desired.AccumAlphaBits - current.AccumAlphaBits) *
                                    (desired.AccumAlphaBits - current.AccumAlphaBits));
            }

            if (desired.Samples != GLFW.GLFW_DONT_CARE)
            {
                extraDiff += (uint)((desired.Samples - current.Samples) *
                                    (desired.Samples - current.Samples));
            }

            if (desired.SRGB && !current.SRGB)
                extraDiff++;

            // Figure out if the current one is better than the best one found so far
            // Least number of missing buffers is the most important heuristic,
            // then color buffer size match and lastly size match for other buffers

            if (missing < leastMissing)
                closestIndex = i;
            else if (missing == leastMissing)
            {
                if ((colorDiff < leastColorDiff) ||
                    (colorDiff == leastColorDiff && extraDiff < leastExtraDiff))
                {
                    closestIndex = i;
                }
            }

            if (closestIndex == i)
            {
                leastMissing = missing;
                leastColorDiff = colorDiff;
                leastExtraDiff = extraDiff;
            }
        }

        return closestIndex;
    }

    /// <summary>
    /// Retrieves the attributes of the current context.
    /// Ported from <c>_glfwRefreshContextAttribs</c>.
    /// </summary>
    internal static bool _glfwRefreshContextAttribs(GlfwWindow window,
                                                     GlfwCtxConfig ctxconfig)
    {
        string[] prefixes =
        {
            "OpenGL ES-CM ",
            "OpenGL ES-CL ",
            "OpenGL ES "
        };

        window.context.Source = ctxconfig.Source;
        window.context.Client = GLFW.GLFW_OPENGL_API;

        var previous = _glfw.contextSlot;
        glfwMakeContextCurrent(window);
        if (_glfw.contextSlot != window)
            return false;

        // Load GetIntegerv, GetString and Flush via getProcAddress
        window.context.GetIntegerv = (delegate* unmanaged<uint, int*, void>)
            window.context.getProcAddress!("glGetIntegerv");
        window.context.GetString = (delegate* unmanaged<uint, nint>)
            window.context.getProcAddress!("glGetString");
        window.context.Flush = (delegate* unmanaged<void>)
            window.context.getProcAddress!("glFlush");

        if (window.context.GetIntegerv == null ||
            window.context.GetString == null ||
            window.context.Flush == null)
        {
            _glfwInputError(GLFW.GLFW_PLATFORM_ERROR,
                "Entry point retrieval is broken");
            glfwMakeContextCurrent(previous);
            return false;
        }

        nint versionPtr = window.context.GetString(
            GL_VERSION);
        if (versionPtr == 0)
        {
            if (ctxconfig.Client == GLFW.GLFW_OPENGL_API)
            {
                _glfwInputError(GLFW.GLFW_PLATFORM_ERROR,
                    "OpenGL version string retrieval is broken");
            }
            else
            {
                _glfwInputError(GLFW.GLFW_PLATFORM_ERROR,
                    "OpenGL ES version string retrieval is broken");
            }

            glfwMakeContextCurrent(previous);
            return false;
        }

        string version = Marshal.PtrToStringAnsi(versionPtr) ?? string.Empty;

        for (int i = 0; i < prefixes.Length; i++)
        {
            if (version.StartsWith(prefixes[i], StringComparison.Ordinal))
            {
                version = version.Substring(prefixes[i].Length);
                window.context.Client = GLFW.GLFW_OPENGL_ES_API;
                break;
            }
        }

        // Parse version string: "%d.%d.%d" (revision is optional)
        if (!_glfwParseVersionString(version,
                out int parsedMajor, out int parsedMinor, out int parsedRevision))
        {
            if (window.context.Client == GLFW.GLFW_OPENGL_API)
            {
                _glfwInputError(GLFW.GLFW_PLATFORM_ERROR,
                    "No version found in OpenGL version string");
            }
            else
            {
                _glfwInputError(GLFW.GLFW_PLATFORM_ERROR,
                    "No version found in OpenGL ES version string");
            }

            glfwMakeContextCurrent(previous);
            return false;
        }

        window.context.Major = parsedMajor;
        window.context.Minor = parsedMinor;
        window.context.Revision = parsedRevision;

        if (window.context.Major < ctxconfig.Major ||
            (window.context.Major == ctxconfig.Major &&
             window.context.Minor < ctxconfig.Minor))
        {
            // The desired OpenGL version is greater than the actual version
            // This only happens if the machine lacks {GLX|WGL}_ARB_create_context
            // /and/ the user has requested an OpenGL version greater than 1.0

            // For API consistency, we emulate the behavior of the
            // {GLX|WGL}_ARB_create_context extension and fail here

            if (window.context.Client == GLFW.GLFW_OPENGL_API)
            {
                _glfwInputError(GLFW.GLFW_VERSION_UNAVAILABLE,
                    "Requested OpenGL version {0}.{1}, got version {2}.{3}",
                    ctxconfig.Major, ctxconfig.Minor,
                    window.context.Major, window.context.Minor);
            }
            else
            {
                _glfwInputError(GLFW.GLFW_VERSION_UNAVAILABLE,
                    "Requested OpenGL ES version {0}.{1}, got version {2}.{3}",
                    ctxconfig.Major, ctxconfig.Minor,
                    window.context.Major, window.context.Minor);
            }

            glfwMakeContextCurrent(previous);
            return false;
        }

        if (window.context.Major >= 3)
        {
            // OpenGL 3.0+ uses a different function for extension string retrieval
            // We cache it here instead of in glfwExtensionSupported mostly to alert
            // users as early as possible that their build may be broken

            window.context.GetStringi = (delegate* unmanaged<uint, uint, nint>)
                window.context.getProcAddress!("glGetStringi");
            if (window.context.GetStringi == null)
            {
                _glfwInputError(GLFW.GLFW_PLATFORM_ERROR,
                    "Entry point retrieval is broken");
                glfwMakeContextCurrent(previous);
                return false;
            }
        }

        if (window.context.Client == GLFW.GLFW_OPENGL_API)
        {
            // Read back context flags (OpenGL 3.0 and above)
            if (window.context.Major >= 3)
            {
                int flags;
                window.context.GetIntegerv(
                    GL_CONTEXT_FLAGS, &flags);

                if (((uint)flags & GL_CONTEXT_FLAG_FORWARD_COMPATIBLE_BIT) != 0)
                    window.context.Forward = true;

                if (((uint)flags & GL_CONTEXT_FLAG_DEBUG_BIT) != 0)
                    window.context.Debug = true;
                else if (glfwExtensionSupported("GL_ARB_debug_output") &&
                         ctxconfig.Debug)
                {
                    // HACK: This is a workaround for older drivers (pre KHR_debug)
                    //       not setting the debug bit in the context flags for
                    //       debug contexts
                    window.context.Debug = true;
                }

                if (((uint)flags & GL_CONTEXT_FLAG_NO_ERROR_BIT_KHR) != 0)
                    window.context.Noerror = true;
            }

            // Read back OpenGL context profile (OpenGL 3.2 and above)
            if (window.context.Major >= 4 ||
                (window.context.Major == 3 && window.context.Minor >= 2))
            {
                int mask;
                window.context.GetIntegerv(
                    GL_CONTEXT_PROFILE_MASK, &mask);

                if (((uint)mask & GL_CONTEXT_COMPATIBILITY_PROFILE_BIT) != 0)
                    window.context.Profile = GLFW.GLFW_OPENGL_COMPAT_PROFILE;
                else if (((uint)mask & GL_CONTEXT_CORE_PROFILE_BIT) != 0)
                    window.context.Profile = GLFW.GLFW_OPENGL_CORE_PROFILE;
                else if (glfwExtensionSupported("GL_ARB_compatibility"))
                {
                    // HACK: This is a workaround for the compatibility profile bit
                    //       not being set in the context flags if an OpenGL 3.2+
                    //       context was created without having requested a specific
                    //       version
                    window.context.Profile = GLFW.GLFW_OPENGL_COMPAT_PROFILE;
                }
            }

            // Read back robustness strategy
            if (glfwExtensionSupported("GL_ARB_robustness"))
            {
                // NOTE: We avoid using the context flags for detection, as they are
                //       only present from 3.0 while the extension applies from 1.1

                int strategy;
                window.context.GetIntegerv(
                    GL_RESET_NOTIFICATION_STRATEGY_ARB,
                    &strategy);

                if ((uint)strategy == GL_LOSE_CONTEXT_ON_RESET_ARB)
                    window.context.Robustness = GLFW.GLFW_LOSE_CONTEXT_ON_RESET;
                else if ((uint)strategy == GL_NO_RESET_NOTIFICATION_ARB)
                    window.context.Robustness = GLFW.GLFW_NO_RESET_NOTIFICATION;
            }
        }
        else
        {
            // Read back robustness strategy
            if (glfwExtensionSupported("GL_EXT_robustness"))
            {
                // NOTE: The values of these constants match those of the OpenGL ARB
                //       one, so we can reuse them here

                int strategy;
                window.context.GetIntegerv(
                    GL_RESET_NOTIFICATION_STRATEGY_ARB,
                    &strategy);

                if ((uint)strategy == GL_LOSE_CONTEXT_ON_RESET_ARB)
                    window.context.Robustness = GLFW.GLFW_LOSE_CONTEXT_ON_RESET;
                else if ((uint)strategy == GL_NO_RESET_NOTIFICATION_ARB)
                    window.context.Robustness = GLFW.GLFW_NO_RESET_NOTIFICATION;
            }
        }

        if (glfwExtensionSupported("GL_KHR_context_flush_control"))
        {
            int behavior;
            window.context.GetIntegerv(
                GL_CONTEXT_RELEASE_BEHAVIOR, &behavior);

            if ((uint)behavior == GL_NONE)
                window.context.Release = GLFW.GLFW_RELEASE_BEHAVIOR_NONE;
            else if ((uint)behavior == GL_CONTEXT_RELEASE_BEHAVIOR_FLUSH)
                window.context.Release = GLFW.GLFW_RELEASE_BEHAVIOR_FLUSH;
        }

        // Clearing the front buffer to black to avoid garbage pixels left over from
        // previous uses of our bit of VRAM
        {
            var glClear = (delegate* unmanaged<uint, void>)
                window.context.getProcAddress!("glClear");
            glClear(GL_COLOR_BUFFER_BIT);

            if (window.doublebuffer)
                window.context.swapBuffers!(window);
        }

        glfwMakeContextCurrent(previous);
        return true;
    }

    /// <summary>
    /// Searches an extension string for the specified extension.
    /// Ported from <c>_glfwStringInExtensionString</c>.
    /// </summary>
    internal static bool _glfwStringInExtensionString(string target, string extensions)
    {
        int start = 0;

        for (;;)
        {
            int where = extensions.IndexOf(target, start, StringComparison.Ordinal);
            if (where < 0)
                return false;

            int terminator = where + target.Length;
            if ((where == start || extensions[where - 1] == ' ') &&
                (terminator >= extensions.Length || extensions[terminator] == ' '))
            {
                break;
            }

            start = terminator;
        }

        return true;
    }

    //////////////////////////////////////////////////////////////////////////
    //////                        GLFW public API                       //////
    //////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Makes the context of the specified window current for the calling thread.
    /// Ported from <c>glfwMakeContextCurrent</c>.
    /// </summary>
    public static void glfwMakeContextCurrent(GlfwWindow? handle)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        GlfwWindow? window = handle;
        var previous = _glfw.contextSlot;

        if (window != null && window.context.Client == GLFW.GLFW_NO_API)
        {
            _glfwInputError(GLFW.GLFW_NO_WINDOW_CONTEXT,
                "Cannot make current with a window that has no OpenGL or OpenGL ES context");
            return;
        }

        if (previous != null)
        {
            if (window == null || window.context.Source != previous.context.Source)
                previous.context.makeCurrent!(null);
        }

        if (window != null)
            window.context.makeCurrent!(window);
    }

    /// <summary>
    /// Returns the window whose OpenGL or OpenGL ES context is current on the
    /// calling thread.
    /// Ported from <c>glfwGetCurrentContext</c>.
    /// </summary>
    public static GlfwWindow? glfwGetCurrentContext()
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return null;
        }

        return _glfw.contextSlot;
    }

    /// <summary>
    /// Swaps the front and back buffers of the specified window.
    /// Ported from <c>glfwSwapBuffers</c>.
    /// </summary>
    public static void glfwSwapBuffers(GlfwWindow handle)
    {
        Debug.Assert(handle != null);

        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        GlfwWindow window = handle;

        if (window.context.Client == GLFW.GLFW_NO_API)
        {
            _glfwInputError(GLFW.GLFW_NO_WINDOW_CONTEXT,
                "Cannot swap buffers of a window that has no OpenGL or OpenGL ES context");
            return;
        }

        window.context.swapBuffers!(window);
    }

    /// <summary>
    /// Sets the swap interval for the current context.
    /// Ported from <c>glfwSwapInterval</c>.
    /// </summary>
    public static void glfwSwapInterval(int interval)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        var window = _glfw.contextSlot;
        if (window == null)
        {
            _glfwInputError(GLFW.GLFW_NO_CURRENT_CONTEXT,
                "Cannot set swap interval without a current OpenGL or OpenGL ES context");
            return;
        }

        window.context.swapInterval!(interval);
    }

    /// <summary>
    /// Returns whether the specified extension is available.
    /// Ported from <c>glfwExtensionSupported</c>.
    /// </summary>
    public static bool glfwExtensionSupported(string extension)
    {
        Debug.Assert(extension != null);

        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return false;
        }

        var window = _glfw.contextSlot;
        if (window == null)
        {
            _glfwInputError(GLFW.GLFW_NO_CURRENT_CONTEXT,
                "Cannot query extension without a current OpenGL or OpenGL ES context");
            return false;
        }

        if (extension.Length == 0)
        {
            _glfwInputError(GLFW.GLFW_INVALID_VALUE,
                "Extension name cannot be an empty string");
            return false;
        }

        if (window.context.Major >= 3)
        {
            // Check if extension is in the modern OpenGL extensions string list
            int count;
            window.context.GetIntegerv(
                GL_NUM_EXTENSIONS, &count);

            for (int i = 0; i < count; i++)
            {
                nint enPtr = window.context.GetStringi(
                    GL_EXTENSIONS, (uint)i);
                if (enPtr == 0)
                {
                    _glfwInputError(GLFW.GLFW_PLATFORM_ERROR,
                        "Extension string retrieval is broken");
                    return false;
                }

                string? en = Marshal.PtrToStringAnsi(enPtr);
                if (en == extension)
                    return true;
            }
        }
        else
        {
            // Check if extension is in the old style OpenGL extensions string
            nint extensionsPtr = window.context.GetString(
                GL_EXTENSIONS);
            if (extensionsPtr == 0)
            {
                _glfwInputError(GLFW.GLFW_PLATFORM_ERROR,
                    "Extension string retrieval is broken");
                return false;
            }

            string extensions = Marshal.PtrToStringAnsi(extensionsPtr) ?? string.Empty;
            if (_glfwStringInExtensionString(extension, extensions))
                return true;
        }

        // Check if extension is in the platform-specific string
        return window.context.extensionSupported!(extension);
    }

    /// <summary>
    /// Returns the address of the specified function for the current context.
    /// Ported from <c>glfwGetProcAddress</c>.
    /// </summary>
    public static nint glfwGetProcAddress(string procname)
    {
        Debug.Assert(procname != null);

        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return 0;
        }

        var window = _glfw.contextSlot;
        if (window == null)
        {
            _glfwInputError(GLFW.GLFW_NO_CURRENT_CONTEXT,
                "Cannot query entry point without a current OpenGL or OpenGL ES context");
            return 0;
        }

        return window.context.getProcAddress!(procname);
    }

    //////////////////////////////////////////////////////////////////////////
    //////                    Helper methods                             //////
    //////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Parses a version string of the form "major.minor" or "major.minor.revision"
    /// (with optional trailing text). Replaces the C sscanf call.
    /// </summary>
    private static bool _glfwParseVersionString(string version,
        out int major, out int minor, out int revision)
    {
        major = 0;
        minor = 0;
        revision = 0;

        if (string.IsNullOrEmpty(version))
            return false;

        int pos = 0;

        // Parse major
        if (!ParseIntFromString(version, ref pos, out major))
            return false;

        // Expect '.'
        if (pos >= version.Length || version[pos] != '.')
            return false;
        pos++;

        // Parse minor
        if (!ParseIntFromString(version, ref pos, out minor))
            return false;

        // Optionally parse '.revision'
        if (pos < version.Length && version[pos] == '.')
        {
            pos++;
            ParseIntFromString(version, ref pos, out revision);
        }

        return true;
    }

    /// <summary>
    /// Parses an integer starting at <paramref name="pos"/> in <paramref name="s"/>,
    /// advancing <paramref name="pos"/> past the digits.
    /// </summary>
    private static bool ParseIntFromString(string s, ref int pos, out int value)
    {
        value = 0;
        int start = pos;

        while (pos < s.Length && s[pos] >= '0' && s[pos] <= '9')
        {
            value = value * 10 + (s[pos] - '0');
            pos++;
        }

        return pos > start;
    }
}
