This fixture checks alignment, writable odd-sized allocations, `free`/`realloc`
ownership, invalid alignment, full-width LP64 arguments, impossible-size failure,
and zero-size allocation. It also matches the Linux gcc native oracle.

The native comparison checks unchanged `errno` on success and invalid alignment.
It deliberately does not compare `errno` after an impossible-size `ENOMEM`: the
host glibc used during validation changed it to `ENOMEM` on that path. dotcc's
runtime preserves `errno` on every return, including `ENOMEM`; the direct
`LibcPosixMemalignTests` enforce that stronger contract separately.

The emitted standalone program was also published with `PublishAot=true` for
`linux-x64` and run with both the normal heap and
`DOTCC_DEBUG_HEAP=1 DOTCC_DEBUG_HEAP_SCAN=1`. No native imports are needed for
allocation; the runtime uses BCL `NativeMemory`, collections, and synchronization.
