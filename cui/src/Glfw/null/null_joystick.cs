// Ported from glfw/src/null_joystick.c (GLFW 3.6)
//
// Copyright (c) 2016-2017 Camilla Loewy <elmindreda@glfw.org>
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

namespace Glfw
{
    // Partial class -- upstream joystick operations for the null platform.
    public partial class NullPlatform
    {
        public bool PollJoystick(GlfwJoystick joystick, int mode) => false;
        public string GetMappingName() => "";
        public string UpdateGamepadGUID(string guid) => guid;
        /// <summary>
        /// Initializes joysticks. Always succeeds (no joysticks on null platform).
        /// Corresponds to C++ <c>_glfwInitJoysticksNull</c>.
        /// </summary>
        public bool InitJoysticks()
        {
            return true;
        }

        /// <summary>
        /// Terminates joysticks. No-op on null platform.
        /// Corresponds to C++ <c>_glfwTerminateJoysticksNull</c>.
        /// </summary>
        public void TerminateJoysticks()
        {
            // No-op
        }
    }
}
