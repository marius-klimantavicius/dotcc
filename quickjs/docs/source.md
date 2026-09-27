# Source and configuration

The product uses the official Bellard QuickJS **2026-06-04** release. The exact
archive URL, layout and SHA-256 are recorded in
[`config/source.json`](../config/source.json). The digest was computed from the
official archive; it is not an independently signed upstream checksum. Shared
campaign acquisition preserves the cached archive and extracted source tree.

The source closure is `quickjs.c`, `dtoa.c`, `libregexp.c`, `libunicode.c` and
`cutils.c`, with bundled generated Unicode/opcode/atom headers. The five embedding
inlines listed in `profile.json` are selected at link time with `--export-inline`:
`JS_FreeValue`, `JS_DupValue`, `JS_FreeValueRT`, `JS_DupValueRT` and
`JS_NewCFunctionMagic`. The facade calls these upstream methods directly. Exact
selectors preserve their original names and diagnose ambiguity if the source
closure changes. No forwarding C bridge is needed.
`quickjs-libc.c`, the command shell, workers and external Test262 are excluded.
Bundled tests use the same pinned archive.

Object linking inherits campaign defaults: 256 KB splitting
(`--split=size --split-size=262144`), `--literal-pool` and
`--deduplicate-inline`. Equivalent static
inline helpers, including `__js_rc`, share one implementation; helpers with
observed function addresses, distinct mutable state or different behavior retain
their translation-unit identities. The generated-product audit requires exactly
one unmangled `__js_rc` definition and one original-name definition of each
selected inline in each form; forwarding `DotCC_JS_*` wrappers are rejected.

[`profile.json`](../config/profile.json) specifies Linux x64 little-endian LP64,
unsigned plain char, `_GNU_SOURCE`, `__SIZEOF_INT128__=16` and the pinned version.
Native controls use GNU C11 and `-funsigned-char`; DotCC uses its C11 mode with
GNU extensions. Actual header probes verify 16-byte aggregate JSValue, the
64-bit short-BigInt tag and 32-byte JSMallocState.

[`dotcc-overrides.json`](../config/dotcc-overrides.json) replaces the active
`DIRECT_DISPATCH` macro body with zero, guarded by its expected source definition.
The override is applied only to `quickjs.c`; retained reports prove expansion
selects upstream switch dispatch. Staging preserves `CONFIG_ATOMICS` and
all acquired source definitions. Stack checks remain enabled and use typed
host bindings that account for available stack and requested allocation size.
Allocator callbacks use tracked, aligned native storage. The reviewed empty
`fenv.h` is sufficient because these units include it without using its APIs.

The `native-switch-dispatch` text adaptation is **tests only**, used to build a
native switch-dispatch oracle with a C compiler that lacks DotCC macro overrides.
It is **not used for actual translation**; DotCC uses the guarded macro override.

`CONFIG_ATOMICS` remains enabled. JavaScript `Atomics` uses DotCC's C11 atomic
primitives and pthread-backed wait/notify; upstream's class-ID mutex protects
registration. The facade retains upstream's default `can_block=false`, so an
ordinary evaluation cannot enter a blocking `Atomics.wait`. Low-level hosts may
explicitly use `JS_SetCanBlock`; their waiters must finish or be notified before
the runtime and shared backing storage are released. JavaScript workers and
`Atomics.waitAsync` are not supplied by this embedding/upstream pin.

The engine retains its own per-context xorshift64star PRNG. `js_random_init`
seeds it from `gettimeofday` in microseconds and changes a zero seed to one.
This is upstream noncryptographic behavior; no cryptographic randomness claim
is made. Custom host-class IDs call actual `JS_NewClassID`, which now uses its
upstream registration mutex without an additional host lock.

Native stock and embedded controls compare dispatch/configuration behavior.
Actual translated execution and dependency evidence are listed in
[validation](validation.md). Distribution includes the upstream MIT license
and source-derived additional notices.

Branch hints are translation-profile overrides. The pinned headers contain only
four `__builtin_expect` occurrences: `likely`/`unlikely` in `cutils.h`, and
`js_likely`/`js_unlikely` in `quickjs.h`. Each passes literal `0` or `1` as the
expected argument. Guarded replacements keep the first value as `(!!(x))`,
evaluate it once, and omit the expected argument. Auxiliary object profiles are
derived from the same canonical override rules. Native controls retain their
original builtin hints; translated products are audited for surviving calls.
