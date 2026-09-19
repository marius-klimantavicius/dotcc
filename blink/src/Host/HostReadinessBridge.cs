using global::System;
using Managed.Emulation.Host;

namespace Managed.Emulation;

#if BLINK_FULL_CORE
public static partial class BlinkCore
#else
public static partial class Blink
#endif
{
    public static unsafe int blink_host_poll(blink_host_pollfd* descriptors, ulong count, int timeout)
    {
        try
        {
            if (io == null) return IoError(19);
            if (count > 1024) return IoError(22);
            if (descriptors == null && count != 0) return IoError(14);
            var requests = new PollRequest[(int)count];
            for (int i = 0; i < requests.Length; ++i) requests[i] = new(descriptors[i].fd, descriptors[i].events);
            var result = io.PollAsync(requests, timeout, ioCancellation).GetAwaiter().GetResult();
            if (!result.Succeeded) return IoError((int)result.Error);
            for (int i = 0; i < requests.Length; ++i) descriptors[i].revents = result.Value.Events[i];
            return result.Value.Count;
        }
        catch (Exception error) { return IoException(error); }
    }
}
