# libc++ core source bring-up

The provider is still incomplete. These records describe synthetic consumers
and library implementation components, not real-world project acceptance.
All listed artifacts remain under `artifacts/campaign/`.

The current acquisition is `libcxx-acquired-005`, using the unchanged 18.1.8
archive plus explicit vendor configuration. It adds the accepted BCL-backed
`clock_gettime` capability to the earlier `004` configuration. Do not combine
the libc++ public headers with the competing EDG bootstrap exception classes.

## Accepted components

- `constant-intrinsics-translated-003`: five checks against the original EDG C
  backend and generated static/instance JIT/NativeAOT. Proven constant queries
  remain folded by EDG; surviving `__builtin_constant_p` calls return conservative
  false without evaluating calls, increments or volatile reads in the operand.
- `type-info-regression-001`: all 12 existing public RTTI checks, including
  negative metadata operations. `type-info-fields-translated-005`: nine field
  adapter checks including compiler tokens, constructed derived objects and GC.
- `libcxx-atomic-protocol-translated-002`: four checks for the adapted original
  contention algorithm, with 4,608 handshakes per mode/runtime. See
  [atomic wait ownership](libcxx-atomic-wait.md). The component uses explicit
  one-TU compilation; it does not establish separate-TU provider acceptance.
- `libcxx-allocation-seed-native-003`: the allocation consumer itself passes
  native GCC; its translated provider is still incomplete. This is a baseline,
  not generated C# acceptance.

## Current source dependencies and failures

The minimal allocation probe includes `abi_core.cpp` and the original
`new.cpp`, `new_handler.cpp`, `new_helpers.cpp`, and `typeinfo.cpp`.
`libcxx-allocation-seeds-001` fails in the multi-TU frontend configuration;
`002/003`, explicitly combining those files, reach a missing external RTTI
descriptor for `bad_variant_access` while emitting dynamic-cast support.
The diagnostic now names that descriptor. Do not replace unresolved metadata
with fabricated class definitions.

The pinned upstream `CMakeLists.txt:559/564` compiles library sources with
`_LIBCPP_BUILDING_LIBRARY` and `_LIBCPP_REMOVE_TRANSITIVE_INCLUDES`. A diagnostic
preamble in `libcxx-library-build-preamble-001.h` now applies those exact flags.
They initially remove the incidental variant dependency from the allocation
probe. Subsequent real providers (`memory.cpp`, `stdexcept.cpp`, value exceptions,
`system_error.cpp`, `error_category.cpp`) still require wider standard-library
definitions; `libcxx-allocation-seeds-008` reaches `ios_base::failure`. Production
separate compilation must distinguish library-source flags from consumer flags.

The broader array consumer currently pulls original `memory.cpp`, allocation
sources, `stdexcept.cpp`, `system_error.cpp`, `error_category.cpp`, `ios.cpp`,
`variant.cpp`, `optional.cpp`, `any.cpp`, `functional.cpp`, and `locale.cpp`,
plus `abi_core.cpp` and `abi_atomic.cpp`. The diagnostic unity preamble includes
`__config` and removes only the legacy inline-export compatibility selector,
as explained in [the port audit](libcxx-port-audit.md).

`libcxx-core-seeds-024` passes parsing with the canonical portable C headers.
It then exposed a `ck_designator` constant in a union initializer. The reduced
`union_constant_members.cpp` fixture reproduces the same rejection without
library headers. The original C backend's `dump_initializer_part` selects the
designated field and consumes the following initializer; its lowering contract
explicitly retains such designators for unions. The C# initializer now follows
that rule, and `union-constant-translated-001` passes five original-C and managed
checks. `libcxx-core-seeds-025` advances to the `__builtin_memcmp` binding;
audited memory/string builtin aliases are the next integration step. Earlier
wide/locale declarations alone are not proof of runtime bindings; those
services remain tracked by the libc worker's API ledger.

Broader multi-TU header probes still encounter constrained-template/ODR
diagnostics in the frontend configuration. Adding `<__functional/hash.h>` to
the atomic protocol's second TU reproduces that boundary. Separate-TU linking
and owned-IR work remain mandatory. No EDG semantics were changed, and no
frontend diagnostics were suppressed to count these probes as accepted.


## Calendar/wide dependency increment

`libcxx-core-seeds-027` repeats the explicit single-TU source-closure probe with
the 84-TU musl provider (including accepted restartable multibyte, wide numeric,
and UTC/calendar formatting). It advances beyond `strftime_l`; the next missing
operation is the address of `sscanf` in `include/locale:2642`,
`money_get<char>::do_get`. The libc worker owns the pinned scanning family and
its binary80/shared typed-variadic integration. This probe is still incomplete
at translation and is not acceptance of the full library or separate-TU linker.

The 105-TU selected provider now includes accepted environment and scanning
implementations. Probe `028` advances past `sscanf`; `029` adds the accepted
C++ exception ABI adapter, and `030/031` add original `thread.cpp`, `mutex.cpp`,
`condition_variable.cpp`, and `future.cpp` to supply actual referenced methods
and RTTI. Probe `032` adds `abi_mutex.cpp`: the original separate-TU destructor
sources deliberately redeclare historical ABI classes, so the portable external
thread ABI adapter includes the actual public headers and retains the original
destructor bodies. `mutex-source.json` records provenance. This adapter participates in the accepted single-TU synthetic source-closure
runs038–040 below; separate-TU library assembly remains open.

Probe `032` reaches `__builtin_unreachable`; the coordinator's exact intrinsic
binding advances `033` to `__builtin_clz` in `__bit/countl.h`. These probes still
stop during translation. The expanding source list is dependency discovery with
a synthetic consumer, not a claim of full libc++ or separate-TU acceptance.

Probe `034` passes exact bit-count intrinsics and adds the real original
`call_once.cpp` provider in `035`; `036` adds `chrono.cpp` and reaches an
`eok_class_rvalue_adjust` in `chrono::year_month_day_last::day`. The C backend's
lowering configuration is the reference for that cv-adjusted class prvalue.
No EDG behavior is changed to bypass the diagnostic.


## Generated source-closure milestone

`libcxx-core-seeds-038` successfully translates, builds and runs the synthetic
array consumer under JIT and NativeAOT. The explicit source closure contains the
original locale, thread, future, chrono and core implementations, plus the
tracked managed ABI adapters. `zero-bound-padding-translated-001` separately
passes the original EDG C oracle and static/instance JIT/AOT: zero-length array
fields preserve address/layout but need no CLR storage assignment. Populated
neighboring fields, zero/nonzero template padding, globals, TLS and copies are
checked. The C backend explicitly handles empty zero-bound aggregate
initializers in `dump_initializer_part`; the C# emitter now agrees.

`libcxx-core-seeds-039` adds accepted vector and smart-pointer consumers in JIT
and NativeAOT. They exercise growth/move/destruction, `out_of_range`, shared and
weak ownership, and object lifetime counts. Other consumers remain incomplete:
allocation reaches the generated `__cxa_bad_cast` helper despite a corresponding
source definition; algorithms and hash containers request actual `algorithm.cpp`
and `hash.cpp` providers; value types encounter a frontend/lowering configuration
assertion at the variant destructor. EDG semantics remain unchanged. These are
single-TU static-engine results, not full library, instance, or linker acceptance.


`libcxx-core-seeds-040` resolves the exact translated C-linkage throw providers
and adds original algorithm/hash source files. Allocation, algorithms and
containers all pass translation, C# compilation, JIT and NativeAOT. The exact
ordered source recipe and hashes are now retained canonically as
`backend/runtime/libcxx/core-source-profile.json`, with its implementation-only
feature preamble. This recipe explicitly describes the same one-TU bring-up
configuration, including library feature flags visible to its synthetic consumer;
it is not a production `--libcxx` backend or separate-TU conformance claim.
