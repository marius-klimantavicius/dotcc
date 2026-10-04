# Portable self-translation headers

`include/` declares the host services used by the original EDG compiler that
are absent from the managed libc headers. It is a private declaration overlay
for EDG self-translation, not a complete POSIX implementation or a replacement
for public musl headers. All types use the portable LP64 translation profile.
None of the records may be passed directly to native OS functions.

The overlay supplies no implementations. With partial external contracts
enabled, unresolved functions remain required partial methods. They must not
be described as working libc services merely because translation succeeds.

The host adapter still needs to supply:

- File metadata (`stat`, `fstat`), descriptor/stream bridging (`fileno`,
  `fdopen`), secure temporary creation (`mkstemp`), unlinking, current-directory
  operations, process ID and terminal detection.
- Directory enumeration (`opendir`, `readdir`, `closedir`). `DIR` is opaque;
  the adapter owns the returned `dirent` storage and must reject names that
  cannot fit its 4096-byte UTF-8 buffer rather than truncate them.
- CPU resource limits (`getrlimit`, `setrlimit`). These need actual platform
  support or honest error returns, not successful no-op implementations.
- Read-only private file mappings (`mmap`, `munmap`) for portable C++/CLI
  assembly metadata. A provider must track ownership and honor offset/length
  and lifetime semantics.

`quadmath.h` declares binary128 parsing and formatting (`strtoflt128`,
`quadmath_snprintf`) without copying libquadmath code or native headers. The
original `USE_FLOAT128_FOR_HOST_FP_VALUE=1` and `USE_QUADMATH_LIBRARY=1` settings
are retained, so EDG includes this header. However, its existing
`USE_HOST_FP_CONVERSION_ROUTINES=0` and disabled hexadecimal code-generation
formatting select internal conversion routines: neither quadmath function is
referenced in the current self output, and no quadmath partial is emitted.
Configurations that activate these paths would require a provider preserving
binary128 precision and `%Q` formatting. The contract-only smoke test exercises
this explicit unresolved boundary without pretending to implement it.

Use EDG's supported `USE_MMAP_FOR_MEMORY_REGIONS=0` configuration to select
its allocation/read fallback for compiler memory and module storage. This
does not disable C++ modules. Keep `READ_CPPCLI_PORTABLE_ASSEMBLIES=1`; its
separate metadata mapping calls remain explicit unresolved contracts.

The `stat` record contains only the fields consumed by this EDG configuration.
In particular, device/inode values require real file identity semantics for
hard links; hashing path strings is not an equivalent implementation. The
`sys/dirent.h` forwarding header supports EDG's generic Unix branch without
claiming that the managed target is Linux.

The `stdio.h` wrapper includes the selected libc++ and managed libc headers
before adding POSIX descriptor/stream declarations. Some EDG translation units
use `fileno` without including `unistd.h`, as permitted by its POSIX declaration
in `stdio.h`.

The pinned libc++18 `math.h` wrapper removes C classification macros, but its
floating-point trait excludes the `__float128` extension. The self overlay's
`quadmath.h` therefore adds a global `isnan(__float128)` overload using EDG's
classification builtin. It does not narrow the value to double or long double
and does not alter EDG or libc++ sources.

The original compiler sources require `<new>`, but do not use the C++ standard
container/iostream headers. Their own utility templates are defined in EDG
sources. C++ runtime/library declarations and any remaining required bodies
are separate from this host-service overlay.
