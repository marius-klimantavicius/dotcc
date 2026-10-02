//type:fp
//options_all:--microsoft_version 1900
//remark:[4.11] Microsoft compatibility: __declspec(allocator)
// 12/15/15 [EDGcpfe/16123,EDGcpfe/16300,EDGcpfe/16704]
//
// Microsoft compatibility: __declspec(allocator)
//
// The "allocator" __declspec attribute is now accepted (in Microsoft emulation
// mode when microsoft_version >= 1900) on function types.  No action (other than
// recording the attribute) is taken.
// with --microsoft_version 1900:
__declspec(allocator) void *f();
