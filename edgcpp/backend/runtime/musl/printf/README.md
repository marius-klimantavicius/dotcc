# Selected musl formatting core

This directory preserves musl 1.2.6's original `vfprintf.c`, the adapted copy,
an exact patch and provenance hashes. Keep `COPYRIGHT` with redistributed
source/generated derivatives. `vsnprintf.c` and `format_sink.h` are our adapters.

The original parser and integer/floating digit algorithms remain in C. The
native FILE wrapper is replaced by a private callback sink; the physical VLA
scratch is replaced by a target-derived constant while retaining its original
logical extent. The target supplies the actual varargs, floating, character,
errno and string services.

This is **not yet integrated into generated C#**. Native synthetic prototype
evidence and outstanding dependencies are documented in
[`docs/printf-port-audit.md`](../../../../docs/printf-port-audit.md).
Do not include `vfprintf.original.c` in a build; it is the unmodified patch base.
Do not substitute host FILE or va_list layouts at the managed boundary.
