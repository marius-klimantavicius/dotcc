# libc++ compatibility and runtime boundaries

Audit date: 2026-10-02. These are pinned source candidates, not a completed
libc++ port. Verification used three synthetic header consumers; no real-world
applications or upstream libc++ tests were run. Source archives, extracted
files, configurations and every failed probe are retained under
`artifacts/campaign/libcxx-port-audit/`.

## Candidates

| Release | Minimum GCC mode in its headers | Status |
| --- | --- | --- |
| 23.1.1 | GCC 15 | Source audit; requires a deliberate EDG compiler-mode compatibility effort. |
| 21.1.8, 20.1.8, 19.1.7 | GCC 14 | Minimum-version comparison only. |
| 18.1.8 | GCC 13 | Initial synthetic header consumer reaches our emitter in existing GNU 13.3 mode. |

The first candidate is [LLVM 23.1.1](https://github.com/llvm/llvm-project/releases/tag/llvmorg-23.1.1),
commit `6dfe1677ab8dffbc6ec13d53a1e0215d75147689`. Its release archive SHA-256 is
`ebe9be46fe8756d58c5b198ffad0fa2a766257add81a4dc52179bfacc7888ee6`, matching the
GitHub asset digest. [The evidence manifest](../evidence/libcxx-candidate.json)
records audited file hashes and source provenance. Its libc++ subtree contains
1,688 include files and 70 implementation `.cpp` files; counts do not imply
support coverage.

The compatibility candidate is [LLVM 18.1.8](https://github.com/llvm/llvm-project/releases/tag/llvmorg-18.1.8).
The standalone `libcxx-18.1.8.src.tar.xz` SHA-256 is
`bdecf90be0072bc720fd5c9c8ab061cdb197edd0c8ad3e170dc3e6bfaa49f388`.
Keep 18.1.8 as the initial bring-up candidate until runnable synthetic core
tests justify a selection. Do not spoof newer GCC/Clang version macros to
silence compatibility requirements. Supporting a newer library revision or
language mode remains separate work; an older candidate cannot establish
complete modern standard-library coverage.

## What the probes establish

The 18.1.8 probes use explicit vendor configuration, portable target macros,
GNU 13.3 mode, and our C header subset. They keep exceptions and threading
enabled; the configuration selects the external threading interface, Itanium
object conventions, ABI namespace `__dotgen_v1`, no native visibility metadata,
and serial parallel-algorithm execution. These are bring-up configuration
choices, not final coverage claims.

- `<type_traits>` initially failed because our `stdint.h` lacked the standard
  `int_least*`/`uint_least*` and `int_fast*`/`uint_fast*` typedefs. A diagnostic
  overlay supplying those declarations allows EDG parsing/lowering to reach
  the C# emitter, which rejects a template-dependent constant at file scope.
  A header-free [variable-template fixture](../backend/tests/template_entities.cpp)
  reproduces this. The C backend skips dependent variables using
  `ignore_variable_in_back_end`; our global-storage walk now follows that rule,
  and the missing typedef families have been added. The follow-up synthetic
  probe (`header-probe-18-updated-001`) reaches the emitted runtime call to
  `__builtin_is_constant_evaluated`. The original C backend's
  `dump_intrinsic_call` emits a typed zero for that builtin and passes through
  the operand of `__builtin_launder`; the C# emitter needs the same adaptation.
- `<utility>` needs complete C math declarations, including extended-precision
  variants. `<array>` additionally reaches missing stdlib and wide-character
  interfaces. Declarations alone will not supply their eventual runtime bodies.
- The probes exposed implicit `/usr/include` fallback for a missing `wchar.h`.
  EDG's supported `NO_USR_INCLUDE` cross-compiler configuration now disables
  that fallback in `cpfe-dot`. The portable include sentinel test covers
  implicit `USR_INCLUDE` injection and explicit `-I` behavior.
- Header order matters: libc++ headers precede C provider headers, so its
  wrapper headers can use `include_next`. A generated config directory must
  not accidentally shadow those wrappers with C headers.

The subsequent `libcxx-seeds-002/003` traits consumer passes generated JIT and
NativeAOT. Public-header exception-handle tests also pass with the separately
adapted managed ABI provider; see the campaign ledger for current owner/lifetime
coverage. The broader allocation/container source closure remains incomplete.
These failures are our port/backend integration gaps; no EDG fix is proposed.

Source probes through `libcxx-core-seeds-020` select original allocation,
shared-count, error-category, stream-base and value-exception source files.
The explicit diagnostic unity mode also needs an early `__config` include
followed by removal of `_LIBCPP_DEPRECATED_ABI_LEGACY_LIBRARY_DEFINITIONS_FOR_INLINE_FUNCTIONS`:
those exports preserve old native ABI-v1 library symbols for methods now
defined inline. This new managed ABI has no such older native consumers.
ABI-v1 layouts and current inline semantics remain selected. Keeping the
legacy error-category constructor declaration instead would make the original
system-category `constinit` definitions invalid in that combined translation
unit. This diagnostic configuration does not establish separate-TU support.

## Runtime port contract

Generate one revision-specific `__config_site` shared by library sources and
applications. Preserve relevant licenses and patches when vendoring. LLVM's
[vendor configuration documentation](https://libcxx.llvm.org/VendorDocumentation.html)
describes this configuration model. Macro names differ across revisions:
18 uses several `HAS_NO_*` switches while 23 uses numeric `HAS_*` switches.

The 23.1.1 external thread selector includes
`__thread/support/external.h`, which includes the vendor's `__external_threading`.
The contract covers mutexes, recursive mutexes, condition variables, once
state, thread identity/create/join/detach/sleep, and TLS keys. Implement these
with owner-scoped BCL services and explicit translated-thread exit cleanup.
The [threading API documentation](https://libcxx.llvm.org/DesignDocs/ThreadingSupportAPI.html)
is supplementary; the pinned header is the exact signature contract.

Use libc++'s public `exception`, `typeinfo` and `new` definitions consistently.
Do not combine them with the current EDG bootstrap's competing class bodies.
Select or adapt the required libc++ implementation sources, and supply one
allocation/RTTI/EH provider using our managed runtime. Standard allocation
functions continue to use the shared NativeMemory heap and per-owner handler.

The pinned `exception_pointer_cxxabi.ipp` shows a concrete adaptation boundary:
current-primary-exception, increment/decrement reference count and rethrow
primary exception. These operations need rooted managed carrier handles,
atomic ownership across threads, originating-owner lifetime, and exact final
payload destruction. Native libc++abi unwinding cannot implement CLR transport.
`make_exception_ptr` additionally needs construction, type identity and destructor
registration, with copy failure and termination verified using synthetic tests.

The source list includes out-of-line implementations for strings, containers,
allocation resources, errors, exceptions and numerous other families. Core
headers parsing is insufficient. Random-device selection defaults to an OS
device-file path unless explicitly ported; use BCL cryptographic randomness.
Filesystem, locale, clocks, streams and atomics likewise require explicit
portable service implementations rather than native OS macros.

Next acceptance should progress through synthetic traits/utility/array tests,
then containers and failing allocation/construction, followed by exception
handles and concurrency. Record every unresolved symbol and required source.
Broader library families remain mandatory plan work after the initial core.
