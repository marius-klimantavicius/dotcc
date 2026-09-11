namespace Managed.Security;

/// <summary>Creates the upstream empty-buffer representation. A null base is a
/// failed/unavailable buffer in picotls, even when its capacity is zero.</summary>
public static unsafe class PicotlsBuffer
{
    public static st_ptls_buffer_t Create() => new() { @base = Libc.L("\0"u8) };
}
