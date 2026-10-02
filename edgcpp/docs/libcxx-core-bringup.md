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
