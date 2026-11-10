// Ported from glfw/src/posix_poll.c (GLFW 3.6)
//
// Copyright (c) 2022 Camilla Loewy <elmindreda@glfw.org>
//
// C uses poll()/ppoll() from libc. This port calls poll() through
// the NativeLibrary-loaded function pointer in x11.libc.poll.

using System.Runtime.InteropServices;

namespace Glfw
{
    /// <summary>
    /// POSIX pollfd structure, matching the C definition:
    ///   struct pollfd { int fd; short events; short revents; };
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct PollFd
    {
        public int fd;
        public short events;
        public short revents;
    }

    public static partial class Glfw
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct PollTimespec { internal nint Seconds, Nanoseconds; }
        [DllImport("libc", EntryPoint = "ppoll", SetLastError = true)]
        private static extern unsafe int NativePPoll(PollFd* fds, nuint count, PollTimespec* timeout, nint signalMask);
        public const short POLLIN = 0x0001;

        private const int EINTR  = 4;
        private const int EAGAIN = 11;

        //////////////////////////////////////////////////////////////////
        //////                  GLFW platform API                   //////
        //////////////////////////////////////////////////////////////////

        /// <summary>
        /// Waits for events on file descriptors using poll().
        /// Ported from posix_poll.c _glfwPollPOSIX.
        ///
        /// If timeout is null, blocks indefinitely.
        /// If *timeout is 0, non-blocking poll.
        /// Otherwise, waits up to *timeout seconds, updating *timeout with
        /// remaining time on return.
        /// </summary>
        internal static unsafe bool _glfwPollPOSIX(PollFd* fds, nuint count, double* timeout)
        {
            var x11 = _glfw.X11!;
            for (;;)
            {
                if (timeout != null)
                {
                    ulong baseTime = _glfwPlatformGetTimerValue();

                    var ts = new PollTimespec { Seconds = (nint)(*timeout), Nanoseconds = (nint)((*timeout - (nint)(*timeout)) * 1e9) };
                    int result = NativePPoll(fds, count, &ts, 0);
                    int error = Marshal.GetLastPInvokeError();

                    *timeout -= (_glfwPlatformGetTimerValue() - baseTime) /
                        (double)_glfwPlatformGetTimerFrequency();

                    if (result > 0)
                        return true;
                    else if (result == -1 && error != EINTR && error != EAGAIN)
                        return false;
                    else if (*timeout <= 0.0)
                        return false;
                }
                else
                {
                    int result = x11.libc.poll(fds, count, -1);
                    if (result > 0)
                        return true;
                    else if (result == -1)
                    {
                        int error = x11.libc.errno;
                        if (error != EINTR && error != EAGAIN)
                            return false;
                    }
                }
            }
        }
    }
}
