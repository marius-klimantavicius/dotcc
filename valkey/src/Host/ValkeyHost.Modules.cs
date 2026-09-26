using CoreLibc = Managed.Database.ValkeyCore.Libc;

namespace Managed.Database;

public static unsafe partial class ValkeyHost
{
    // Native module exports and API callbacks use the native Valkey ABI. The
    // translated ABI carries an explicit managed owner and cannot call them.
    // Keep MODULE commands admitted; fail at the unavailable loading operation.
    public static int LoadNativeModule(ValkeyCore core, byte* path, void** arguments, int count, int loadEx)
    {
        fixed (byte* message = "Native module loading is unavailable: native Valkey module ABI is incompatible with managed instance callbacks.\0"u8)
            core._serverLog(ValkeyCore.LL_WARNING, message);
        CoreLibc.errno = CoreLibc.ENOTSUP;
        return ValkeyCore.C_ERR;
    }
}
