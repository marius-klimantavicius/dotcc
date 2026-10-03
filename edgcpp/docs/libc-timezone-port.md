# musl timezone boundary and pinned IANA data

Status: the BCL/TZif boundary and translated `tzset`, local-time, DST and
`mktime` provider pass strict C/C++ static/instance JIT and NativeAOT tests on
Linux x64. Cross-platform execution and custom Windows default-zone handling
remain open; no full calendar-platform completion claim is made.

The C component retains musl 1.2.6's `__tz.c`, `localtime_r`, `localtime`,
`mktime`, `ctime_r`, and `ctime`. POSIX rule parsing, TZif transition search,
ambiguous/nonexistent local-time policy, integer-year range, and normalization
stay in C and are translated. The calendar component supplies arithmetic and
formatting. Explicit wrappers replace linker aliases. Private C++ keyword and
allocation casts and declaration placement permit both input languages; EDG
semantics are unchanged. Scalar initializers remain at their original execution
points when declarations move above a C `goto` target.
`originals/`, exact hashes, and a zero-fuzz patch preserve the source audit.

`LibcTimezone` supplies owner locking and bounded immutable file views. Named
zone searches use the bundled IANA 2026e data on all systems. BCL `FileStream`
reads explicitly requested paths and `/etc/localtime` on Unix. Windows default
selection maps `TimeZoneInfo.Local.Id` to a bundled IANA name using BCL APIs.
Unknown/custom Windows defaults and invariant-globalization mapping still need
an explicit accepted policy; this slice must not be presented as full default
zone support. Generated owners do not have native setuid execution mode. The
host controls file access; there is no native syscall or `struct tm` ABI bridge.

`runtime/tzdb/source.json` pins the official tzdata and tzcode archives. The
retained acquisition script builds their native `zic` only as a dependency-data
generator, with upstream main/backward data, fat TZif output and POSIX time
(no leap-second correction). It packages 598 names into 345 deduplicated TZif
payloads, 420,419 bytes including the index. This is required library data, not
real-world translated-project acceptance. Driver resource hashing and packaging
are exercised by the generated tests, including replacement/history of binary
resources on repeated generation. The data license and source/configuration manifests must
travel with generated output.

The bundle uses one immutable pinned managed array shared across engines.
Explicit file views use pinned owner metadata; pointer maps retain them until
musl unmaps a previous selection or the owner is disposed. C cached names,
`timezone`, `daylight`, `tzname`, and `TZ` remain owner-specific translated
state. C malloc caches still use the native heap. Repeated conversion must not
allocate managed objects after initialization. Concurrent read queries are
serialized by the owner timezone lock; callers must coordinate concurrent
mutation of `environ` as required by the environment provider's contract.

The byte boundary validates headers, counts, all table ranges, transition
indices/order, abbreviation termination, flags and POSIX footer grammar before
exposing a pointer. Standard TZif v1's NUL version marker becomes ASCII `1` in
the private file view, matching this selected musl parser's private v1 branch;
the source file is unchanged. A bad or missing deployment resource fails before
translated C acquires its timezone lock. Named-zone metadata is rooted globally,
whereas arbitrary stale timezone pointers do not gain a new lifetime guarantee.
Musl's name-address validation remains responsible for safe `%Z` formatting.

Evidence retained:

- `libc-timezone-runtime-001`: JIT + NativeAOT pass for all 598 names, malformed
  counts/indices/footer/truncation, private v1 normalization, owner refcounts,
  forced collections, concurrent queries and allocation-free warm validation.
- `libc-timezone-patch-replay-002`: all six original source files replay with
  zero fuzz and match recorded adapted hashes.
- `libc-timezone-native-004`: original/adapted digest `6522545524584554995`.
- `libc-timezone-translated-003`: native parity plus all eight generated modes;
  northern/southern/non-hour zones, DST gaps/overlaps, extended integer years,
  second-resolution synthetic TZif, libc global/buffer ownership, forced GC,
  concurrent reads, safe `%Z`, named IANA data and zero-allocation hot queries.
- `tzdb-build-2026e-002`: a clean retained dependency-data build reproduces the
  canonical bundle SHA-256 exactly.

Earlier failed attempts remain preserved: native001 pending public declarations,
native002/003 test-harness locale ABI/duplicate wrapper mistakes, translated001
C++ declaration placement, and translated002 missing C-linkage fixture exports.
None is counted as full acceptance.

Remaining gates include Windows/macOS/Arm64 execution and custom/default-zone
mapping decisions. The earlier
`libc-local-time-design.md` remains the required semantics checklist.
