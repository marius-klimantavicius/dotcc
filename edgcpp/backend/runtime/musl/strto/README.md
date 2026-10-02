# Selected musl numeric conversion sources

This package preserves musl 1.2.6's `intscan` and `floatscan` algorithms plus
`strto*` and `ato*` wrappers. Exact upstream paths/hashes, adapted hashes and
adapter hashes are recorded in `source.json`; every adapted source has its
original and unified patch alongside it. Copyright and license are retained.
This is a selected library implementation for synthetic verification, not a
build or acceptance run of the whole musl project.

Supply `intscan.c floatscan.c strtol.c strtod.c atoi.c atol.c atoll.c atof.c`
as translation inputs and add this directory to the include path. These public
functions are translated definitions, rather than unchecked runtime imports.
Both C11 and C++17 parsing are intended; generated acceptance is recorded by the
campaign verifier, not inferred merely from native compilation.

The private string cursor implements the zero-width-limit `shgetc` subset used
by these wrappers. It does not masquerade as a real FILE, use host FILE layout,
or support scanf. ASCII whitespace/digits give the same numeric grammar for
C, POSIX and C.UTF-8: decimal point is '.', with no ambient managed culture.
`locale_wrappers.c` adds explicit locale-handle variants. It validates the
owner-scoped handle and uses the same numeric grammar without changing thread
selection.

Dependencies on the selected provider are memcpy/errno and existing exact math
helpers. Private `__edg_scan_scalbnl_nearest` and `__edg_scan_remainder` use
binary80 operations, without narrowing through binary64. Public scalbn/fmod
contracts are not silently enabled. The finite remainder helper's precondition
is satisfied by the retained floatscan algorithm.

The selected arithmetic mode is nearest-even. Directed modes and FE status
flags remain unfinished across the compiler, including these conversions.
Scratch is target-bounded: floatscan's original `uint32_t x[KMAX]` is 8192 bytes
for the binary80 profile. There is no input-sized managed array in the parser.
The underlying current Float80 arithmetic can allocate BigInteger temporaries;
this package does not claim an allocation-free full floating parser.

## Deliberate errno adaptation

The retained pinned core leaves errno unchanged for certain overflowing hex
inputs, such as `0x1p16384`. Numeric final scaling now sets ERANGE on infinity;
public float/double narrowing sets ERANGE when a finite extended result becomes
infinite. Explicit `inf`/`infinity` and `nan(...)` grammar paths keep errno.

The chosen underflow policy sets ERANGE for every nonzero subnormal result,
including exact subnormals. Zero underflow keeps the core's ERANGE behavior.
Invalid input preserves musl's EINVAL extension and returns the original end
pointer. Successful ordinary conversion leaves prior errno unchanged. These
changes are included in the patches; they are not EDG changes.

The native oracle disables glibc's inline atoi/atol wrappers, which would
otherwise replace selected-source calls with the host header's strtol calls.
Native private scaling bridges preserve errno to model pinned musl's pure
arithmetic scalbnl; the explicit adapted wrapper/core owns public error policy.
