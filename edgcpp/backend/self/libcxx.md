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

For the compiler **including `dot_gen_be.c`**, pass that source as the optional
`dot_backend` argument. This adds the unchanged pinned `src/stdexcept.cpp` and
`src/string.cpp` implementations, then includes the dot backend at the end of
the primary provider TU. Omit its standalone TU from the remaining source list.
The library implementation macros are undefined before the backend include.
The returned provenance includes the backend source path/hash and specifies
**C++17**: upstream `string.cpp` implements integer `to_string` using C++17
`to_chars`. The original compiler-only provider remains C++14.

The shared primary TU is deliberate. With libc++'s explicit string template
definitions in one TU and the container/string consumer in another, EDG's
whole-program merge reports a correspondence ownership assertion. Keeping the
definitions and consumer together is a source-assembly choice; the EDG sources
and library algorithms are unchanged. Other EDG compiler files remain separate.

`backend/tests/self_dot_libcxx_verify.py --acquisition PATH --output-dir NEW_PATH
--nativeaot` verifies this closure against a native C++17 behavior oracle. Its
JIT and NativeAOT runs exercise static map/set/string construction and disposal,
vector and stable sorting, allocated string edits/moves, integer formatting at
the limits, and typed out-of-range/length-error exceptions.
