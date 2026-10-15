# Retained constant storage and selected loader provider

The loader reproduction passes all 21 measured phases at
`/mnt/storage/roslynobj-csharp-20261015/t3/loader-provider-reproduction/t3-loader-provider-report.json`.
Receipt SHA256:
`dcc40e28e37437d81467879417229150b4f06ebc3baba404a9b559c19b3c73d2`.
It uses the immutable frozen snapshot 4 C# implementation; later working changes described
below are outside that receipt and checkpoint source index.

| Genuine input | Native / JIT / NativeAOT | Evidence |
| --- | --- | --- |
| static-constants.c | 306 then 313 | Two calls, all fifteen observations; independent same-name locals, omitted array zeros, cyclic static fixup, exact embedded-NUL string bytes, size/offset/alignment and GC address checks |
| aggregate-zero.c | 4 | Actual C record ConstantOperation: Item 24/8, prvalue, pointer-zero first element and omitted remainder; copied value materialized into an ordinary local at each occurrence |
| loader-state.c | 17 | Seventeen observations twice per checker; raw-loader/CLR/GC interference, read-clear/buffer lifetime, canonical addresses and unknown indirect selected-provider dispatch |

The first static checker incorrectly expected 406/413 from a manual sum. Its
fifteen expected observations were correct; native execution and generated code
both produced 306/313. The checker expectation was corrected without changing
product code, and the earlier failure remains separate evidence. The C++
aggregate-value candidate lowered through explicit locals/static references and
did not activate the new aggregate ConstantOperation helper; it is excluded from
that coverage claim. The actual C aggregate-zero fixture does activate it.
Addressable automatic compound-literal storage is not synthesized from a shared
constant backing allocation.

Static-duration constant locals use one pinned allocation and class-level
constant/fixup initialization. Their declaration and admitted initializer
occurrences perform no per-call reset. Arrays and records retain their exact
layout and zero-filled omitted elements. Scalar static locals can retain direct
`initk_static` integer constants without a LocalStaticInitialization row; the
SQLite 132 region-based local statics instead retain one `initk_static` recipe,
123 aggregate and nine integer values. Dynamic initialization, thread-local
storage, cleanup, weak/common/alias selection and unsupported representation
obligations remain guarded.

Strings use their captured target bytes, including embedded and terminal zeros;
no host string decoding or added terminator is involved. Their retained array
lvalue supplies a place to the selected decay/address plan. Record constants
keep their captured prvalue category, copy constant backing bytes to an ordinary
local value at the retained occurrence, and supply that materialized place to
aggregate copying. Branch-local prefixes and operand ordering remain the typed
planner's responsibility.

The selected glibc oracle establishes that successful dlopen, dlsym and dlclose
each clear an unread error; successful dlsym may return NULL without an error.
The generated provider registers the exact bound addresses of those entries and
dlerror. Before a selected loader call it clears ambient raw error state; after
the target it reads through the registered error function and copies exact native
message bytes before CLR activity. It replaces logical pending state even on
success. Selected dlerror direct/canonical calls and the registered raw provider
address passed through an unknown indirect gateway use one logical read-clear
accessor. No linked-name fallback reads a different provider's error state.

The byte buffer is native thread-owned storage, released by a pthread-key
destructor. Reading clears the pending flag and keeps the returned bytes alive
until the next selected loader call; raw loader interference and GC do not
invalidate them. Snapshot allocation/read-clear bookkeeping restores the native
errno already captured by the outgoing errno protocol. Native glue contains only
TLS, error-byte copying, registration and typed ABI forwarding; application
algorithms remain C#. The absolute zero-symbol provider has no executable body,
and the raw-loader helper is checker-only interference.

The loader driver pins the genuine object, source, export recipe, libc and public
SQLite-use audit, repeats emission identically, and audits the five exact imports,
canonical fields, generated native exports/copies and dependencies. It records
producer/compiler/provider/source/build hashes. GCC/cc 13.3 executable SHA256 is
`1b99826121ae6682a634e5efe09bd3e3df58ce58e0b28f849114ab5b89139c26`.
Generated ABI SHA256 is
`b0d328ec7c9dfab87466ff4a584ea7ebbba13c8e692b2f1b9bb1c31bf545b588`.
Managed dependencies are only LoaderChecks, Generated and Runtime. The native
oracle is never a generated dependency, and no LD_LIBRARY_PATH is required.

The public SQLite loader audit is
`t3/providers/sqlite-loader-use-audit-1.json`, SHA256
`4a648ed17fdcb1fe7b3389bc2c942df6e839e9106b1827192a74fe285e3f4c0a`.
It finds direct dlopen/dlerror/dlclose uses and one dlsym address constant.
unixDlError casts the returned pointer into local zErr, tests it, passes it to
translated sqlite3_snprintf, and then leaves its mutex. The retained pointer must
survive that managed formatting path. This is bounded actual-use evidence, not
general source alias/ownership analysis or arbitrary loader interposition.

Reproduce the loader slice in a serialized slot under a source freeze:

```sh
python3 edgcpp/roslyn-csharp/tests/run-t3-loader-provider.py \
  --object /mnt/storage/roslynobj-csharp-20261015/t3/providers/export/loader-state.roslynobj \
  --artifacts /mnt/storage/roslynobj-csharp-20261015/t3/NEW-LOADER-ATTEMPT
```

Omit `--object` to export through the frozen producer with the recorded GNU 13.3
host flags. The script shares receipt helpers with run-t3-errno-provider.py.

The next five actual SQLite static failures are documented in
`t3/storage-constants/sqlite-static-five-facts.json`: four recipe-free
`initk_static` string arrays (zOrd 9/1, zAff 10/1, zText 25/1, zKeyText 34/1) and
dummy, a 24/8 VdbeOp with static duration, sc_static, initk_none and no
initializer/constant/recipe/TLS/cleanup. Future working code admits those exact
constant/zero-storage forms; static-string-zero.c and its physical checker are
pending next-snapshot execution and are outside the receipt above.

A subsequent working correction clears the matching native loader TLS pointer
before its state destructor frees storage, preventing a dangling pointer if
another native TLS destructor reenters later. It is also outside frozen snapshot 4's
receipt. Native TLS-destructor reentry itself remains unexercised; the current
receipt proves ordinary selected application entry paths. Incoming arbitrary
foreign native loader-error state, cancellation, native C++ exception propagation,
the remaining SQLite provider contracts and SQLite execution are unclaimed.
