# Allocating strings

`strdup.c` and `strndup.c` are pinned musl 1.2.6 string algorithms. Their
only adaptations are explicit `char *` casts for the provider's C++ translation
mode. Original sources and exact patches are retained beside the adapted files;
`source.json` records archive, source and patch SHA-256 values.

Both functions use the existing translated `malloc`/`free` heap owner. Allocated
storage comes from native memory, is independently writable, and follows the
usual C allocation ownership and `errno` behavior. They add no platform imports
or managed callbacks. Provider reachability pruning removes unused routines.
