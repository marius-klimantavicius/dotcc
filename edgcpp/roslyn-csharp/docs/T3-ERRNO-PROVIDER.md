# Linux x64 logical errno provider slice

The focused genuine application proof returns `6223` with all 24 native
observations, twice per checker, in both JIT and NativeAOT. It also checks raw
errno interference, nested qsort callbacks and a foreign pthread. The initial
measured proof is under
`/mnt/storage/roslynobj-csharp-20261015/t3/providers`; the complete reproduction
receipt is written by [the driver](../tests/run-t3-errno-provider.py) as
`t3-errno-provider-report.json` in its fresh output directory. This is a bounded
provider slice. It does not establish SQLite execution or all 88 native provider
requirements.

The explicit configuration is
`runtime.nativeErrorProtocol: "linuxX64LogicalErrno"`, represented by
`NativeErrorProtocol.LinuxX64LogicalErrno` in the emitter and runtime request.
The default remains `None`. Admission requires the retained little-endian,
eight-bit addressable-unit, 64-bit pointer/alignment-eight Itanium profile;
generated execution additionally checks Linux x64. Native function bindings
retain their exact captured linkage names, explicit library paths and provider
entry points. There is no prefix stripping or semantic remangling.

The selected `__errno_location` contract substitutes a native thread-local
application slot for raw libc errno. Direct calls, indirect calls and an
address-exposed function constant all use the same generated accessor,
`__roslynobj_application_errno_location`. Its stable per-thread `int*` is
intentionally distinct from the raw libc slot. Application writes through that
pointer update the application value directly. The canonical address exported
to application storage is the generated accessor, never raw libc
`__errno_location`.

Every outgoing native gateway obtains the application and raw slots, copies the
application value into raw errno immediately before the typed target call, and
captures raw errno immediately after the call, before returning through a CLR
transition. It then updates the application slot. Success does not clear errno;
a native call which leaves errno untouched preserves the application-written
value. Pthread return codes remain ordinary return codes, with no conversion
into errno. Known generated function addresses dispatch to ordinary managed
bodies. Unknown native addresses use the protected native gateway.

An address-exposed generated application function has a concrete C entry and a
fixed managed callback bridge. The C entry snapshots incoming raw errno before
TLS resolution or CLR entry, seeds the application slot, calls the bridge, then
publishes the final application value to raw errno after the CLR return. Nested
callbacks retain their resulting value instead of restoring an outer saved
application value. A foreign thread's first native entry starts from its
incoming raw errno; a managed-origin thread's native application TLS begins at
zero. Managed exceptions reaching these native callback bridges terminate via
`Environment.FailFast`; they do not unwind through C.

The fixture exercises the following exact imports from the pinned libc:

| Boundary | Captured imports |
| --- | --- |
| errno and process | `__errno_location`, `getpid` |
| failure and callback | variadic `open`, `qsort` |
| thread lifecycle | `pthread_create`, `pthread_join` |
| mutex attributes | `pthread_mutexattr_init`, `pthread_mutexattr_settype`, `pthread_mutexattr_destroy` |
| mutex operations | `pthread_mutex_init`, `pthread_mutex_lock`, `pthread_mutex_trylock`, `pthread_mutex_unlock`, `pthread_mutex_destroy` |

The only variadic shape here is `open(pointer, int)` with two fixed arguments
and no tail arguments. Incoming generated C entries are the two ordinary
qsort comparators and the ordinary pthread worker. Generated native glue
contains typed forwarding, registration, logical TLS and immediate errno
synchronization. All sorting comparisons, application observations and worker
logic are emitted C# from the genuine object. The original source is compiled
only into the separate native oracle executable. The separate raw-interference
helper exposes exactly `raw_errno_address`, `raw_errno_read` and
`raw_errno_write`; it is a checker-only helper with no application binding.

The native adapter compiles retained size, alignment and recursive member-offset
checks against the actual host pthread declarations:

| Native storage | Size / alignment in bytes |
| --- | ---: |
| `pthread_mutexattr_t` | 4 / 4 |
| `pthread_mutex_t` | 40 / 8 |
| `pthread_t` | 8 / 8 |
| `pthread_attr_t` | 56 / 8 |

The application uses its generated storage for recursive mutex initialization,
lock/trylock/unlock/destruction and thread creation/join. The independent
HostContracts evidence additionally covers actual native layout, protected
reentry and foreign-thread TLS in
`/mnt/storage/roslynobj-csharp-20261015/t3/host-contracts-first`. Raw errno's
incidental survival across allocation or GC in that finite preparation is not
the production guarantee; the generated protocol supplies the guarantee within
this selected contract.

The oracle observations are:

```text
1201,1201,-1,20,1,555,777,777,444,333,2,9,0,0,0,0,0,0,0,0,1,1,901,1
```

They cover application errno writes and success preservation; native failure;
stable direct/indirect logical accessor identity; nested qsort entry/error
propagation and sorted values; recursive mutex status; pthread lifecycle;
foreign-thread initial errno, separate TLS and observed results; and the main
thread's retained errno/address identity. Additional generated-checker checks
write raw libc errno outside every generated gateway, allocate and collect, and
confirm that application values and pointer writes remain coherent. The
`errno_address` and `process_id` global function-pointer constants are initialized
before any direct call; the latter is used only indirectly. The driver verifies
one canonical address field and one fixup for each, covering imported-address
first registration without duplicate entries.

The read-only public use audit of the unchanged production SQLite object is
`/mnt/storage/roslynobj-csharp-20261015/t3/errno-public-use-audit-2.json`, SHA256
`c41c292517d8451118a92d598f4bc02bd5993f92706e1483870a05fb7022262a`.
It identifies 28 unique `__errno_location` calls and 28 callee-only references.
Each result is immediately dereferenced to a signed 32-bit prvalue with an exact
terminal value consumer: ten ordinary int32 call arguments, seven equalities,
five assignment RHS values, three local initializers, two inequalities and one
return. The three explicit initializer roots revisit those same call identities,
explaining 31 inventory contexts. The audit's no-escape result is limited to
these category-and-terminal patterns. Lvalue/reference/comma/conditional
forwarding is unclassified; this is no source alias-analysis claim. The earlier
incomplete audit1 is not the no-escape evidence.

The driver pins the producer, fixture, reusable object, successful export recipe,
libc and refined public audit hashes. Exports reuse the recorded GNU 13.3 host
predefines and system include paths from `export-errno-thread-2/run.json`, remove
only its source/output operands, and supply the fixture's source/output. All
SQLite defines and timing flags are absent. The failed initial export without
the multiarch include paths remains separate historical evidence. The producer
is never rebuilt and the production SQLite object is never re-exported.

The receipt records compiler executables/versions, all exact commands and
resource measurements, source-set stability, native oracle/helper/object,
translator/reader/runtime/checker builds, generated sources, and ABI source and
binary hashes. It checks identical repeat emission; exact import and native
export allowlists; identical generated ABI copies in build/publish output; and
managed workload dependencies limited to ErrnoChecks, Generated and Runtime.
The generated workload has no reader, SQLite or native application-body
dependency. The ABI library resolves relative to its assembly without
`LD_LIBRARY_PATH`; normal libc IFUNC/vDSO resolution is not rejected.

Reproduce under a complete source freeze, in a serialized build/export slot:

```sh
python3 edgcpp/roslyn-csharp/tests/run-t3-errno-provider.py \
  --object /mnt/storage/roslynobj-csharp-20261015/t3/providers/export/errno-thread.roslynobj \
  --artifacts /mnt/storage/roslynobj-csharp-20261015/t3/NEW-PROVIDER-ATTEMPT
```

Omit `--object` to export the same genuine fixture using the immutable recorded
host flags. `--skip-aot` produces an explicitly incomplete JIT-only result.

The protocol is wired into the already admitted unknown scalar, variadic and
aggregate native gateways, but this receipt proves only its stated shapes.
Additional error contracts, especially `dlerror`'s read-and-clear snapshot,
loader/buffer ownership, the remaining SQLite imports, cancellation/nonlocal
jumps and native C++ exception propagation remain open. This slice does not
enable a production SQLite binding manifest by itself.
