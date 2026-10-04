# EDG's libc++ implementation provider

`libcxx.py:prepare_provider` assembles the pinned LLVM libc++ 18.1.8 allocation
and exception implementation as the **primary** C++14 translation unit. The
original compiler files remain separate translation units and use the same
public libc++ headers. Add the generated `provider.cpp` before `cfe.c`, followed
by the remaining compiler sources, with the acquisition's `config/` and libc++
`include/` directories in the header search path.

The provider uses existing managed exception ABI adapters (`abi_core.cpp` and
`abi_exceptions.cpp`), actual upstream `new.cpp`, `new_handler.cpp`,
`new_helpers.cpp`, and `typeinfo.cpp`. Allocation therefore uses libc++'s real
retry/new-handler/throw behavior and the selected musl/managed allocator. The
LLVM fallback definitions provide actual exception classes and virtual methods;
there are no unresolved exception-class vtables or manually invented layouts.

Two private implementation adaptations preserve the original compiler's C++14
language mode and avoid unrelated library dependencies:

- For `new_handler.cpp` only, spell its `constinit` as libc++'s own
  `_LIBCPP_CONSTINIT` compatibility macro. The sole object is a null-initialized
  function pointer, whose initialization is already constant in C++14.
- Preinclude a generated copy of private `src/include/atomic_support.h` with its
  unused `<memory>` include removed. No body in that private header references
  the comment's `__libcpp_relaxed_load`; `new_handler.cpp` only uses the local
  atomic load/exchange templates. Their upstream bodies and default sequentially
  consistent memory order are unchanged. This avoids pulling shared_ptr,
  threading, locale and their unrelated RTTI/source dependencies into EDG.

The original acquisition stays unchanged. The helper verifies pinned adapter
and source hashes, plus the acquisition's public headers, config and complete
implementation source tree. Returned provenance records the generated provider
and private header hashes and both adaptations. Keep those records with the
self-translation results.

Verification is `backend/tests/self_libcxx_verify.py --acquisition PATH
--output-dir NEW_PATH --nativeaot`. It compiles the consumer separately from the
provider, rejects unresolved externs, compares with a native C++14 oracle, and
executes JIT/NativeAOT checks for ordinary/array/sized/nothrow allocation,
allocation failure and handlers, exception classes, RAII unwinding, and
`exception_ptr`. This establishes the dependency closure needed by EDG; it does
not claim complete libc++ library support.
