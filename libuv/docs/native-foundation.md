# Native identity and lazy binding foundation

`src/Managed.Libuv.Native` implements process identity and native module/export
resolution. It contains no translated libuv backend, loop or I/O algorithm. The
required `dispatch` acceptance suite remains incomplete; `dispatch-foundation`
tests this narrower authored boundary only.

Discovery is frozen through a single `Lazy<NativeHost>`. Merely loading the
assembly or referring to a library descriptor does not load native modules.
On Linux it inspects `NativeLibrary.GetMainProgramHandle()`, resolves `dladdr`
in process scope, and verifies that `malloc`, `free`, `read` and the errno
accessor belong to the same already-loaded native module. The GNU marker must
resolve to that same provider for glibc. Musl recognition requires an appropriate
loaded provider filename without a conflicting GNU marker. Android uses its
`__errno` accessor and Bionic marker. Discovery never chooses libc by successfully
loading an arbitrary candidate library.

This intentionally rejects mixed providers, including allocator/function
interposition that cannot yet be validated. A real `LD_PRELOAD` helper interposes
`read` for a subprocess; both JIT and NativeAOT reject it before loading resolver
modules. Interposition is not silently accepted as a new ABI. Missing process
scope symbols, unrecognized libc paths and unsupported OS/architectures fail
explicitly. Glibc versions with a separate non-global `libdl` may need a validated
bootstrap strategy; that combination has not been tested.

Windows and macOS identities currently describe explicit platform binding
policies (Windows 10+ UCRT; macOS libSystem). Their code ships alongside Linux
selection without conditional source inclusion, but these branches have not run.
Musl, Android, 32-bit and other architectures remain unqualified. Other platform
identities need implementations; their source translation is still required by
the plan and is not dropped by this foundation.

`NativeHost.Library(kind)` checks the selected OS/environment before constructing
a descriptor. Foreign library kinds fail without loading them. The first export
request initializes that module once, even under concurrent requests. Linux libc
reload is verified against the observed malloc provider base address. Modules
remain loaded for the process lifetime; there is no finalizer or disposal that
could invalidate native addresses while callbacks/resources still reference them.
A failed validation releases only the newly acquired module reference.

`TryLoad` / `TryGetExport` report genuinely absent libraries or symbols without
fabricating functions. `GetRequiredExport` distinguishes an absent library
(`DllNotFoundException`) from an absent required symbol
(`EntryPointNotFoundException`). Upstream translated code still owns when a feature
is optional, probing/fallback order and invocation signatures. The resolver does
not marshal calls, manipulate errno, replace native descriptors or infer signatures.

Validation on Linux glibc x64:

```bash
bash libuv/scripts/test.sh --suite dispatch-foundation --mode all --rid linux-x64 --fetch never
```

Receipt: `artifacts/campaign/20260929-074236-a382d209/receipt.json`. Both JIT and
NativeAOT pass frozen provider identity, lazy and concurrent loading, foreign-kind
rejection, separate optional/required library and symbol absence, native malloc/free
storage across forced GC, native process ID, native close/errno error capture and
real interposed-provider rejection. Raw/processed translation forms do not apply
to this authored-only project. There is no claim of a registered translated backend.
