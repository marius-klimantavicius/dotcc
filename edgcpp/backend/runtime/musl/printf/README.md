# Selected musl formatting core

This directory preserves musl 1.2.6's original `vfprintf.c`, the adapted copy,
an exact patch and provenance hashes. Keep `COPYRIGHT` with redistributed
source/generated derivatives. `vsnprintf.c`, `format_sink.h`, `format_file.h`
and `format_wrappers.c` are our adapters. The wrapper file supplies the eight
public printf-family entries; FILE output uses a validated managed lock import.

The original parser and integer/floating digit algorithms remain in C. The
native FILE wrapper is replaced by a private callback sink; the physical VLA
scratch is replaced by a target-derived constant while retaining its original
logical extent. The target supplies the actual varargs, floating, character,
errno and string services.

The selected core and wrappers now pass generated C# JIT/NativeAOT synthetic
acceptance for C/C++ and static/instance modes. This uses nearest-even
arithmetic and the bounded C/C.UTF-8 provider; it is not full fenv or locale
coverage. Evidence and outstanding contracts are documented in
[`docs/printf-port-audit.md`](../../../../docs/printf-port-audit.md).
Do not include `vfprintf.original.c` in a build; it is the unmodified patch base.
Do not substitute host FILE or va_list layouts at the managed boundary.
