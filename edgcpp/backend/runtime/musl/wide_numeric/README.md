# Selected wide numeric conversion

Pinned musl 1.2.6 wrappers share the accepted `../strto` integer and binary80
floating parsers. `originals/`, `patches/`, and `source.json` retain exact
provenance. The original fake-FILE wide-to-ASCII buffer is replaced by the
width-aware `edg_string_scan` cursor. It reads each wchar_t directly, skips musl
wide whitespace, maps non-ASCII input to the same terminating `@` character,
and counts consumed wchar_t elements. No transcoding buffer is allocated.

Three TUs supply nine standard entry points and seven explicit-locale wrappers.
The latter validate owned C/POSIX/C.UTF-8 handles before reading the input;
invalid handles return zero, restore endptr to the input and set EINVAL.
The float wrappers reuse the narrow parser's documented overflow/underflow
policy, including ERANGE for exact nonzero subnormals. Full fenv and arbitrary
locale decimal syntax remain unimplemented.

Regenerate with `backend/tests/vendor_musl_wide_numeric.py --musl-root PINNED
--numeric-root backend/runtime/musl/strto --output-dir NEW_DIRECTORY`.
`backend/tests/libc_wide_numeric_verify.py` preserves native baselines and all
C/C++ static/instance JIT/NativeAOT outputs; `libc_numeric_verify.py` regresses
all existing narrow cases against the shared cursor. See
`docs/libc-wide-numeric-conversion.md` for evidence and remaining limits.
