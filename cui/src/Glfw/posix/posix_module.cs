// Ported from glfw/src/posix_module.c (GLFW 3.6)
//
// Copyright (c) 2021 Camilla Loewy <elmindreda@glfw.org>
//
// C uses dlopen/dlsym/dlclose. C# uses System.Runtime.InteropServices.NativeLibrary.

using System.Runtime.InteropServices;

namespace Glfw
{
    public static partial class Glfw
    {
        //////////////////////////////////////////////////////////////////
        //////                  GLFW platform API                   //////
        //////////////////////////////////////////////////////////////////

        /// <summary>
        /// Loads a shared library / native module.
        /// Ported from posix_module.c _glfwPlatformLoadModule (dlopen).
        /// </summary>
        internal static nint _glfwPlatformLoadModule(string path)
        {
            if (NativeLibrary.TryLoad(path, out nint handle))
                return handle;
            return 0;
        }

        /// <summary>
        /// Frees a previously loaded native module.
        /// Ported from posix_module.c _glfwPlatformFreeModule (dlclose).
        /// </summary>
        internal static void _glfwPlatformFreeModule(nint module)
        {
            if (module != 0)
                NativeLibrary.Free(module);
        }

        /// <summary>
        /// Looks up a symbol in a loaded native module.
        /// Ported from posix_module.c _glfwPlatformGetModuleSymbol (dlsym).
        /// </summary>
        internal static nint _glfwPlatformGetModuleSymbol(nint module, string name)
        {
            if (NativeLibrary.TryGetExport(module, name, out nint address))
                return address;
            return 0;
        }
    }
}
