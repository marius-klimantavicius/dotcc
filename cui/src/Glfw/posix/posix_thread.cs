// Ported from glfw/src/posix_thread.c (GLFW 3.6)
//
// Copyright (c) 2002-2006 Marcus Geelnard
// Copyright (c) 2006-2017 Camilla Loewy <elmindreda@glfw.org>
//
// In C#, TLS is handled via [ThreadStatic] attributes (_glfw.contextSlot,
// _glfw.errorSlot) and mutexes via `lock` (_glfw.errorLock).
// These platform functions are therefore trivial no-ops or thin wrappers.

namespace Glfw
{
    public static partial class Glfw
    {
        //////////////////////////////////////////////////////////////////
        //////                  GLFW platform API                   //////
        //////////////////////////////////////////////////////////////////

        internal static bool _glfwPlatformCreateTls()
        {
            // [ThreadStatic] fields are automatically per-thread in .NET.
            // Nothing to allocate.
            return true;
        }

        internal static void _glfwPlatformDestroyTls()
        {
            // [ThreadStatic] fields are managed by the runtime; no cleanup needed.
        }

        // _glfwPlatformGetTls / _glfwPlatformSetTls are not needed:
        // callers access _glfw.contextSlot and _glfw.errorSlot directly.

        internal static bool _glfwPlatformCreateMutex()
        {
            // _glfw.errorLock is a pre-allocated `object` used with `lock`.
            // Nothing to create.
            return true;
        }

        internal static void _glfwPlatformDestroyMutex()
        {
            // Managed lock objects do not need explicit destruction.
        }

        internal static void _glfwPlatformLockMutex()
        {
            System.Threading.Monitor.Enter(_glfw.errorLock);
        }

        internal static void _glfwPlatformUnlockMutex()
        {
            System.Threading.Monitor.Exit(_glfw.errorLock);
        }
    }
}
