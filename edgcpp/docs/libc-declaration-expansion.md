# Portable declaration expansion

The headers promoted to `backend/runtime/libc/include/` fill the C declaration
surface needed by the synthetic libc++18 probes. The original draft copies
under `backend/tests/libc-declarations-include/` remain preserved. Promotion
occurred after the coordinator isolated the broad regression source snapshot.

The headers contain the C17 stdlib declarations and div result structures,
171 standard math function declarations across all three floating widths,
float_t/double_t, classification constants/macros and typed relational
predicates, all 14 C ctype declarations, the missing string collation/error
functions, and C17 time declarations. Existing bounded/POSIX string extensions
remain declared. Time also includes reentrant conversion extensions.

All types describe the translated ABI: signed 64-bit time_t/clock_t,
16-byte timespec, and a 56-byte broken-down tm with the nine standard int
members plus musl-shaped gmtoff/zone extensions. No native host struct is
passed through this ABI. This is a declaration choice for upcoming BCL-backed
time services, not an implemented clock/timezone provider.

Merely declaring a function does not make it callable. Unbound uses must keep
failing through the exact-signature registry; in particular, standard math
headers must not be mistaken for complete transcendental, fenv or libc coverage.
The relational helper macros evaluate each argument once through typed inline
helpers. IEEE quiet-predicate exception flags remain part of the pending fenv
work. Existing math_errhandling exposure is inherited from the current provider
and must be reviewed when the broader implementations are bound.

Header-only synthetic inputs are `backend/tests/libc_declarations.c` and
`backend/tests/libc_declarations_more.c`. Original EDG's C backend parses each
as C11 and C++17, then generated native C passes the declaration/layout/storage
and basic predicate checks. Evidence is retained in:

- [`artifacts/campaign/libc-declarations-20261002-231147/results.json`](../artifacts/campaign/libc-declarations-20261002-231147/results.json)
- [`artifacts/campaign/libc-more-declarations-20261002-232546/results.json`](../artifacts/campaign/libc-more-declarations-20261002-232546/results.json)

These draft checks do not yet establish generated C# or libc++ acceptance.
For the C++ library's own include_next wrappers, search libc++'s headers first
and the active provider include directory second.
