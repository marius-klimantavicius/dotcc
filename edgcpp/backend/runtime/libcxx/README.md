# Pinned libc++ provider bring-up

This directory contains source acquisition and vendor configuration for the
initial LLVM 18.1.8 compatibility provider. It is not yet a completed standard
library port. `source.json` fixes the release URL, archive SHA-256, license and
ABI namespace. The compiler remains in GNU 13.3 compatibility mode; no newer
compiler identity is spoofed to bypass a library's requirements.

Acquire a fresh, retained tree:

```sh
python3 edgcpp/backend/runtime/libcxx/acquire.py \
  --output-dir edgcpp/artifacts/libcxx-18-provider
```

Use `--archive PATH` to reuse an already downloaded archive. Its complete hash
is still checked. Acquisition extracts the original source unchanged, copies
the explicit `__config_site`, obtains `__assertion_handler` from the upstream
vendor template, verifies the license, and hashes every extracted/configured
file into `acquisition.json`. An existing output directory is rejected rather
than replaced. Downloads, failed inputs, source, configuration and manifests
remain available for inspection. Upstream tests are neither built nor run.

Header order is configuration, libc++ includes, and portable C provider headers.
This preserves libc++ wrapper headers and their `include_next` behavior. Do not
add `backend/runtime/include` or preinclude `exceptions.cpp`: those contain
competing `std::exception`, `std::type_info` and allocation declarations.
The managed ABI provider must implement the declarations from this libc++
revision consistently for both library sources and consumers.

The config keeps exceptions, RTTI, threads and wide characters enabled. It
selects Itanium structural conventions, the external thread API, libc++'s
portable default rune table, and serial
parallel algorithms, with no host OS identity or native visibility attributes.
External thread support uses `config/__external_threading` and the separately
tested `LibcxxThreads.cs` BCL runtime. Its 19 C imports pass generated static
and instance JIT/NativeAOT binding/lifetime acceptance; see
`docs/libcxx-thread-abi.md` for precise coverage. No native thread layout is
exposed by this header.

Run only synthetic seed consumers during this campaign:

```sh
python3 edgcpp/backend/tests/libcxx_probe.py \
  --acquisition edgcpp/artifacts/libcxx-18-provider \
  --output-dir edgcpp/artifacts/libcxx-seed-run --nativeaot
```

Select one with `--fixture traits` (or `utility`, `array`, `vector`, `memory`,
`exception_ptr`). Add explicit adaptation/library translation units with
`--source PATH`; there is no automatic native C++ library linkage. Additional
portable header overlays use `-I PATH` after libc++ includes. Each stage keeps
stdout/stderr and its exact command; incomplete translation/build/execution
returns failure and is recorded explicitly. A translated header consumer does
not prove the out-of-line library or complete language coverage.

Library source probes default to C++20 because this revision's out-of-line
sources use `constinit`; header-only compatibility probes may pass
`--std c++17`. `--unity` is an explicit single-translation-unit diagnostic mode
that includes the selected synthetic consumer and library sources together.
Its report labels this narrower scope; it does not establish cross-unit
identity, linkage or separate compilation. The initial C++20 multi-unit
allocation probe reports constrained-template declaration conflicts in both
the unchanged EDG C backend and our frontend profile
(`libcxx-core-source-audit-004/`). Those diagnostics remain unsuppressed.

`abi_core.cpp` supplies only the base exception class operations selected from
the pinned release's `exception_fallback.ipp`, with its license retained, plus
three compiler-inserted failure entry points implemented with ordinary C++
throws. It excludes the fallback's unimplemented uncaught-exception functions.
Use the original `src/typeinfo.cpp`, `src/new.cpp`, `src/new_helpers.cpp` and
`src/new_handler.cpp` for their corresponding definitions. The exception handle
and termination provider is separate; no native C++ ABI library is linked.

`abi_atomic.cpp` ports the pinned contention algorithm using the BCL wait/wake
boundary and a shared pinned runtime table. `atomic-source.json` records exact
original/adapted hashes; `atomic-managed.patch` reproduces the adaptation from
the acquired original source. See `docs/libcxx-atomic-wait.md` for the ownership
contract and focused protocol acceptance. The vendor configuration also selects
the C provider's tested `clock_gettime` capability for `chrono.cpp`; this selects
no host OS identity and the implementation uses BCL clocks at runtime.

## Initial source and ABI dependency inventory

| Consumer | Upstream code and required boundary |
| --- | --- |
| Traits/utility/array | Primarily headers; dependent entities and unused header bodies must follow EDG's `ignore_*` and needed-entity decisions. Runtime constexpr queries/launder follow `c_gen_be.c::dump_intrinsic_call`. Bounds/exception paths still require their real providers when emitted. |
| Allocation and unique ownership | `src/new.cpp`, `src/new_handler.cpp` and allocation exceptions. Scalar/array, aligned, sized and nothrow new/delete plus `get_new_handler`/`set_new_handler` must share the existing NativeMemory heap and originating owner. The bootstrap provider cannot be included alongside libc++ class definitions. |
| Vector and string exceptions | `src/stdexcept.cpp`, `src/support/runtime/stdexcept_default.ipp`, and `src/include/refstring.h` provide `logic_error`/`runtime_error` construction, copying, `what()` and derived destructors. Refstrings require allocation, string copying and atomic reference counts. `src/vector.cpp`/`src/string.cpp` retain out-of-line ABI entry points. |
| Shared/weak ownership | `src/memory.cpp`: `__shared_count::__add_shared`, `__release_shared`, `__shared_weak_count` operations and `lock`; atomic reference counts, destruction and mutex services must work across threads and preserve owner lifetime. |
| Exception handles | `src/support/runtime/exception_pointer_cxxabi.ipp` expects `__cxa_current_primary_exception`, `__cxa_increment_exception_refcount`, `__cxa_decrement_exception_refcount`, and `__cxa_rethrow_primary_exception`. Supply managed carrier handles with exact ownership/destruction; never native unwinding. `make_exception_ptr`, nested exceptions, uncaught counts and termination need their complete managed ABI implementation. |
| RTTI and base exception classes | Use libc++'s `typeinfo` and `__exception` definitions. Implement the corresponding out-of-line class operations and map lowered EDG descriptors through the existing managed RTTI/EH machinery; do not equate native ABI pointer tagging/layout with managed function pointers. |
| Mutex/thread/TLS | The pinned `include/__threading_support` defines the contract consumed through vendor `__external_threading`: mutex and recursive mutex, condition variables including absolute timed waits, once, thread IDs/create/join/detach/yield/sleep, TLS key creation/get/set and exit callbacks. Use portable opaque storage and BCL objects, explicit owner binding, and translated thread-exit cleanup. |

This is a source-level dependency inventory, not a complete linker-symbol
report. The seed probes have not yet passed parsing/emission far enough to
establish every instantiated out-of-line dependency. `libcxx-seeds-001`
identified missing C headers, external threading declarations, builtin handling
and an empty-parameter-pack storage placeholder. The original EDG C backend
omits non-needed entities through `scp_is_needed_in_generated_code`; our
backend must follow that rule rather than allocate template placeholders.

The subsequent `libcxx-seeds-002`/`003` runs pass the traits consumer under
JIT and NativeAOT after the needed-entity and intrinsic adaptations. Utility
still encounters an unneeded inline-body dependency; array/vector need broader
wide-I/O and locale declarations. Memory and exception-handle seeds now reach
real out-of-line ABI dependencies. None of those failing seeds is counted as
library acceptance.

Do not select upstream `exception_pointer_unimplemented.ipp` as a working
provider or silently remove exception/thread paths. Library breadth, platform
execution and a newer revision/language compatibility audit remain required.
