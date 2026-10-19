# Use the owning CPython API

## B34 execution and managed stack contract

Initialize and use the runtime synchronously on its external initializing caller.
Translated owning work runs on one private thread requesting64 MiB. Synchronous
callbacks from that executor run on the waiting caller through a narrow mailbox;
Python worker callbacks stay on their invoking workers. Arbitrary synchronization
context work is not pumped. Reentrant/foreign calls reject before submission.
Cancellation requests Python interruption and drains its registration; the caller
waits until in-flight work and callback arguments are safe to release.

Retain the SAME existing Libc program context and null/explicit binding mode
across Python runtime generations. Its static globals/pins/main allocator may
outlive Py_Finalize; a program context is not a per-PythonRuntime container.
Disposal while a runtime is active rejects through the retained lease. After that
program context's final use it may be disposed, but reusing this one static VM
under a different context/binding rejects BEFORE translated entry. The API creates
no second Libc assembly/context or permanent program-context lease.

The opt-in worker policy applies to both pthread routes, including null/default/
explicit-small attributes. Logical attributes retain exactly the requested value;
the managed backend requests a physical reservation at least max(requested,64 MiB).
Other programs retain1 MiB defaults, larger requests are preserved, creation
resource errors remain EAGAIN, and there is no hidden worker cap. Retirement
retains policy through workers/descendants/TLS destructors until drain.

For requests no larger than64 MiB, executor+2 workers requests192 MiB virtual stack
reservation; executor+8 requests576 MiB. Guards/runtime/commit overhead are extra.
This is a reservation cost, not zero resident memory. Current-Libc49 tests and
JIT/AOT bounds pass; actual RAW JIT smallcaller/ThreadPool/legacy owning, worker,
callback, error/cancellation and lifecycle controls pass. The EXACT unchanged
required JSON recursion case now passes with interpreter reuse. The same focused RAW NativeAOT contracts and unchanged JSON case/reuse pass.
The full RAW JIT suite completes4867 unique IDs:4751 passes,12 IO failures,
101 required skips and3 XFAIL; no errors/fatal/unrun IDs.4759 pass events include
8 repeated IDs. Nonpasses keep acceptance open. Final raw/processed/new193/D10/
ABI/thread/rooted/trimmed/delivery/audit/downstream/reproduction gates remain open.
[Current actual proof](../artifacts/owner-b-r34-20261018/actual-jit-same-owner-input-proof.json),
[JSON result](../artifacts/owner-b-r34-20261018/actual-json-initial-json-case.log)
and [original failures/limits](../artifacts/owner-b-r34-20261018/first-actual-blocker.json)
retain exact scope; no algorithm, recursion limit or upstream test changed.


B34 hands the genuine safe execution lane to B35 for the12 newly exposed
ftruncate/offset failures; that nonowned runtime path was reported without B34
editing it. [Direct safe release](../artifacts/owner-b-r34-20261018/serial-slot-release-to-b35.json)
records37 terminal PIDs, no active/queued work and three independent free checks.
Initial d9 legacy-binding and first-harness AV failures remain retained; no
algorithm, recursion limit, required method/decorator/exclusion or generated
source/resource was patched.64 MiB is measured for these paths, not a universal
guarantee for arbitrary inputs or the still-open complete matrix.

## Owner B32 — new RAW metadata regression passes; recursion remains blocking

Actual V18 owning startup/finalization passes two cycles, two callbacks, eight
original frozen-finder executions and deleted-cwd recovery. The exact unchanged
R30 native/actual pyc fixture now agrees with physical modes0600/0644/0664 and
uid/gid1000, and real py_compile/SourcelessFileLoader returns42. The original
first importlib failure and all38 previous error IDs pass unchanged in focused
replay and the immediately following full required journal. The newly exercised
consumer library has its own SHA256 `a9997b05…`/MVID
`5b4b8cc0-7ea0-4e36-89e4-11b9bb682ea2`, PE/PDB/source/runtime bindings and B31
stat section proof; canonical `96d7a662…` is recorded separately.

The full4867-required raw JIT process naturally aborts-6 after241.618s in
`test.test_json.test_recursion.TestCRecursion.test_highly_nested_objects_decoding`.
Its journal records3804 unique started/3803 completed/3712 passed/zero errors,
88 required skips and3 required XFAIL;1064 cases lack completed outcomes.
This remains an unqualified product. No recursion limit, caller stack, upstream
method/decorator/exclusion or production API was changed for this run.
[B32 handoff](../artifacts/owner-b-r32-20261018/handoff.json) links exact commands,
native/actual parity, journal counts and the current fatal. Supervisor-coordinated
production stack safety, required skips/XFAILs, new193, final both-form JIT/AOT
D10/ABI/ownership/thread/lifecycle/rooted/delivery/audit/downstream/reproduction
and P0–P6 remain open. Processed is stale V16 and embedded193 remains oldV11.

## Owner B31 — generic metadata repair SOURCE-READY; actual retry pending

The saved B30 pyc reduction identifies false shared stat metadata: direct-created
files physically have0600/0644/0664 and uid/gid1000 while old FillStat reports
zero permissions/identity. Unchanged importlib then creates bytecode physically
0200 from that reported mode; its loader correctly returns EACCES. The generic
Linux x64 repair maps real host144-byte metadata into the unchanged **96-byte**
target stat ABI, preserves actual owner handles for fstat and implements final-link
lstat semantics. Independent native/header/actual-binary measurements correct the
inherited104-byte size description; no shared header or creation-mode edit occurs.

Original604 checks fail266 before repair and pass604 after it. Final expanded
current-Libc controls pass685 each in JIT/NativeAOT;35 metadata/file/path/owner
unit tests pass with zero skips. PE/PDB and actual176-reference ILC input bindings
are retained. These results do not qualify an interpreter with the new runtime.
B31 released all three locks and its lane directly to A28/supervisor for focused
compiler validation, one normal capture/raw286 retry, then the same unchanged
pyc fixture and early actual required-suite execution. See
[B31 source-ready handoff](owner-b-handoff-r31.md).

B30's actual raw journal remains3804 started/3803 completed:3674 passes,38 errors,
88 required skips and3 required XFAIL;1064 required cases lack completed outcomes.
Its natural JSON recursion stack overflow and all final raw/processed JIT/AOT,
owning/ABI/thread/lifecycle/rooted/delivery/audit/new193/P6 gates remain open.
V17 products are frozen, processed is stale V16 and embedded193 old V11.

## B30 early RAW discovery preparation

The new test-only [discovery entry](../tests/RawDiscoveryR30/Program.cs) uses
ordinary owning API/project references and the unchanged required-case runner.
It is prepared for A26's new RAW286 artifact after the direct safe heavy handoff;
no B30 interpreter execution or acceptance is claimed yet. The current processed
V16 and embedded oldV11 products have their own historical scope. Full final
RAW/processed JIT/AOT, owning/ABI/thread/lifecycle/delivery/audit, expanded D10 and
P6 acceptance remain open. Exact preparation and deferred execution commands are
in the [B30 artifact](../artifacts/owner-b-r30-20261018/source-preparation.json).

## B29 source-ready startup correction

The authored Host now configures both `PyConfig.stdlib_dir` and
`module_search_paths` from `PythonOptions.StandardLibraryDirectory`, using the
real CPython string/list APIs and clearing their owned storage after startup.
This preserves selected-root frozen-module filenames after relocation. Ordinary
invalid options still reject before startup; a failed partial initialization
still reports PyStatus and does not claim that the process can be reused.

The current frozen V16/V11 products predate this Host change and the shared
`getcwd` repair. B29 proves the changed Host natively and the typed configuration
contract on those unchanged PEs; source-ready is not a regenerated owning API
pass. Native/current-Libc JIT/AOT and focused startup evidence, the retained
harness failure and required capture/product/suite follow-up are in the
[B29 handoff](owner-b-handoff-r29.md). Full acceptance remains open.

## Owner B28 — fresh V16 proof; raw-JIT progress and native-confirmed blockers

Both published forms pass fresh eight-value target-header layout, 24 common
public ABI fields plus three explicit wchar differences, and exactly38 actual
ABI assertions/two lifecycles. Their own source/AST/address/cache/IL/RVA reviews
find6259 regions,1963 caches/9070 uses,454 pins and nine concrete lifetime
exceptions per form.1533 byte getters cover944670 exact bytes/1706 calls with
getter newarr0. These are safety/legacy-byte preflights; expanded D10 primitive,
readability and full allocation acceptance remain OPEN.

Actual raw-JIT receipt `20261018-083246-e8989539` discovers all23 seeds,
5134 unique IDs/5142 occurrences and selects the unchanged4867 required/267
excluded IDs. Its own actual consumer PE/MVID/PDB/project/source/IL/RVA/lifetime
proof passes before execution; wrong identity/mapping/reference/socket-edge
controls reject. The journal retains2609 passes, one required UTF-8 skip, two
required expected failures and20 failed-subtest events across four IDs:
2616 unique completed cases, one started without outcome and2251 required IDs
without a completed outcome. The last ordinary report snapshot ends earlier at
2440 executed/2437 passed; it is preserved without overwriting its counts.
The prior top-level-await, ENOTDIR and signal/yield-from cases now pass.

The process naturally aborts134 at importlib `test_deleted_cwd`: shared getcwd
lets FileNotFoundException escape. Fresh native returns NULL/ENOENT2; the exact
actual suite PE reduction reproduces the escaping exception. Native embedding
also reproduces missing frozen filenames when PyConfig.stdlib_dir is omitted;
setting that field corrects the native reduction, while the actual PE retains
None. Both unchanged descriptor expectedFailure methods reproduce natively.
Native UTF-8 modes1/0 confirm the skip guard/configuration distinction, without
an actual candidate-mode case pass or a waiver. No runtime/header/compiler,
source/case/decorator/exclusion or feature-policy repair is made.

The first framework prerequisite unexpectedly rebuilt MetadataAudit, and existing
specialist commands initially lacked nonblocking -n. Both deviations and exact
original argv remain retained; focused adapters now require the explicit pinned
reader and nonblocking locks. V16 tools/capture copies/resources/products remain
frozen, independent of A25's authorized source-only compiler edits.92 audit and23
reporting controls pass. See the [B28 handoff](../artifacts/owner-b-r28-20261018/b28-final-handoff.json)
for actual inputs, native REDs, journal/snapshot distinctions and genuine release.
Other expensive cells remain unqueued. All four-cell/owning/thread/GC/lifecycle/
rooted/trimmed/delivery/audit, new193, expanded D10 and final production/reproduction
P6 gates remain open; P0 is in progress and P1–P6 remain open.

The shared socket header is [B27 source-ready](owner-b-handoff-r27.md) with native
`SOMAXCONN=4096`, default backlog128,28 real TCP controls and23 focused network
unit passes. V15 still embeds the old header; normal capture and fresh emitted
controls precede a new286 retry. A22's twelve dependent test-only socket flags
are live. Delivered193/286 products, B22's1995 required passes/four nonpasses/
2868 unrun, expanded D10 and full P0–P6 acceptance remain unchanged/open.

The `Managed.CPython` facade embeds translated CPython **3.13.15**, commit
`4061bc4c35f7c26f25264666d4ba083b93d2f6f9`. The selected `embedded-gil`
product uses CPython's evaluator, reference counting, cyclic collector and GIL,
with shared `DotCC.Libc` services.

**Qualification is provisional.** This guide documents the authored API and
ordinary sample. Final D10, helper-profile/upstream, reproduction and downstream
gates remain open. Historical passing receipts apply to their own artifacts;
they do not qualify a regenerated library or establish production readiness.
See [API progress](api-progress.md), [ABI progress](abi-progress.md) and the
[plan](PLAN.md) for current evidence.

The new A20 V14 tools contain B23 and B24R2, but [B25's unchanged-C retry](owner-b-handoff-r25.md)
stops at a compiler function-pointer return mismatch after native controls pass.
Its narrow direct-call control passes raw JIT; processed/AOT and full new product
qualification remain open. Use the recorded historical products only within
their receipt-specific evidence until fresh tools and complete after-controls pass.

The separate286 test profile now has the A17 V13 hostname publication;
embedded193 remains its older V11 product. [B22 actual results](../artifacts/owner-b-r22-20261018/actual-required-suite-result.json)
bind fresh layout/ABI/source and actual-consumer PE/D10 guards, then real PID
and hostname bootstrap and all23 discovery seeds. The raw-JIT suite executes
1,999 unique required cases:1,995 pass, one error, one failure, one required skip
and one interruption;2,868 required IDs remain unrun. Socket feature selection,
directory ENOTDIR semantics and synchronous SIGINT delivery are reduced blockers.
B23 has repaired the three shared directory/signal source paths and passed native
and direct current-Libc JIT/AOT controls, including real default SIGINT and owner
lifetimes. The existing V13 resources and both products still contain the old
runtime. [Source-ready results](../artifacts/owner-b-r23-20261018/source-ready.json)
do not establish an emitted or interpreter pass; fresh capture, unchanged-C
controls and profile regeneration are required. A owns the socket/resolver closure.
No required source, ID or decorator is changed. Full interpreter/all-property
allocation/GC/matrix, new193 and P6 gates remain open.

## Build and select a library

Use the repository's .NET 10 / C# 14 toolchain. Translation orchestration needs
Python 3.11+, Bash and prepared same-pin CPython generator tools. Linux NativeAOT
publishing also needs the .NET native build prerequisites. Python is a build
tool/oracle dependency; the application runs delivered assets without an
installed Python executable or native libpython.

Run these reproduction commands from the repository root. `translate.sh`
prepares the whole closure, builds raw output, postprocesses a private copy,
builds processed output and publishes both. `build.sh` and `test.sh` consume
existing products.

```bash
bash Scripts/campaign.sh layout cpython --check
flock -n --close /tmp/dotcc-cpython-shared-build.lock bash cpython/scripts/translate.sh --fetch never --jobs 2 --form all
flock -n --close /tmp/dotcc-cpython-shared-build.lock dotnet build cpython/ManagedConsumer.slnx -c Release --disable-build-servers -m:1
```

In the shared implementation checkout, the compiler/publication owner must first
release the build lane and campaign `run.lock`. These commands must not compete
with an active publication. A busy nonblocking flock exits without queuing work.
Specialist suites lock their managed commands internally; do not nest another
shared flock around those hooks.

`--fetch never` reuses local upstream inputs. NuGet restore is separate:
`--restore none` requires existing managed restore assets. The default hash
policy is `warn`; strict reproduction is an explicit selection. See the
[framework guide](../../docs/campaigns.md) for these policies.

The root solution selects processed output. The sample and facade both use the
`CPythonProject` MSBuild property:

| Form | Project relative to repository root |
| --- | --- |
| Processed | `cpython/generated/TranslatedCPython/TranslatedCPython.csproj` |
| Raw | `cpython/generated/TranslatedCPython.Raw/TranslatedCPython.csproj` |

For example, build the raw sample with an absolute selection and isolated outputs:

```bash
flock -n --close /tmp/dotcc-cpython-shared-build.lock dotnet build cpython/samples/ManagedConsumer/ManagedConsumer.csproj -c Release -p:CPythonProject="$PWD/cpython/generated/TranslatedCPython.Raw/TranslatedCPython.csproj" --artifacts-path "$PWD/cpython/build/usage/raw/products" -o "$PWD/cpython/build/usage/raw/app" --disable-build-servers -m:1
```

An application references
[`Managed.CPython.csproj`](../src/Managed.CPython/Managed.CPython.csproj).
The [sample project](../samples/ManagedConsumer/ManagedConsumer.csproj) shows
ordinary project references, forwarding `CPythonProject` to the facade, copying
`app/**` to build/publish output and importing `DeliveryNotices.targets` for
AOT notices. Select the same form throughout the reference graph and use separate
artifacts/output paths for each form. Both forms have the same assembly identity.

## Deliver assets and initialize

The translated project copies `cpython/assets.json`, the selected
`cpython/stdlib/` tree and `cpython/notices/` beside the application. The sample
also copies `app/campaign_app/`. Keep the complete output directory together
when relocating, including facade/DotCC notices and resolved .NET runtime notices
for AOT. A DLL alone is insufficient.

[`PythonOptions`](../src/Managed.CPython/PythonOptions.cs) takes exactly two
directories: `standardLibraryDirectory` and `applicationDirectory`. Both must
exist; relative paths become absolute at construction. The sample uses
`Path.Combine(AppContext.BaseDirectory, "cpython", "stdlib")` and
`Path.Combine(AppContext.BaseDirectory, "app")`. Optional argument 1 overrides
the stdlib directory; argument 2 selects a writable work directory. By default
it creates a new work directory under the system temporary directory.

`PythonRuntime.Initialize(options)` uses isolated startup, explicit stdlib then
application search paths, UTF-8 mode, no site import, no bytecode writes and no
installed signal handlers. Ambient `PYTHONHOME`, `PYTHONPATH`, user site packages
and the working directory do not supply startup imports. Standard input starts
at EOF; Python output/error streams are host-owned text streams. Isolation
configures startup; it does not sandbox code or restrict file permissions.

An absent options directory throws `DirectoryNotFoundException` before startup.
A CPython initialization status becomes `PythonInitializationException`; its
`Status` contains `Kind`, `Function`, `Message` and `ExitCode`. An exit status
does not terminate the host process. Missing startup files can fail after
initialization begins. Uncertain partial initialization retains the process
reservation and prevents another runtime from starting in that process.

## Run the JSON workflow

The complete runnable example is
[`Program.cs`](../samples/ManagedConsumer/Program.cs), with Python code in
[`campaign_app/__init__.py`](../samples/ManagedConsumer/app/campaign_app/__init__.py).
This excerpt uses its existing calls; `input`/`output` are file paths,
`stdlib`/`application` are the directories above, and `Require` is the sample's
assertion helper. Add `using Managed.CPython;` as in the sample.

```csharp
int callbacks = 0;
using (var runtime = PythonRuntime.Initialize(new PythonOptions(stdlib, application)))
{
    runtime.RegisterInt64Callback("host", "double", item => { Interlocked.Increment(ref callbacks); return checked(item * 2); });
    using (var package = runtime.Import("campaign_app"))
    using (var run = package.GetAttribute("run_captured"))
    using (var inputValue = runtime.FromString(input))
    using (var outputValue = runtime.FromString(output))
    using (var result = run.Call(inputValue, outputValue))
        Require(result.AsInt64() == 20, "Python function result");
}
```

The sample first writes UTF-8 input `{"items":[2,3,5]}`. Python reads it with
`json`, calls `host.double` three times and writes exactly
`{"sum":20,"values":[4,6,10]}`, without a trailing newline, using
`json.dumps(result, separators=(",", ":"), sort_keys=True)`. The host checks
exact bytes, callback count, return value, captured streams and traceback frames.
It disposes the runtime, then repeats with a fresh runtime.

Run the ordinary consumer on existing products after the shared lane release:

```bash
flock -n --close /tmp/dotcc-cpython-shared-build.lock bash cpython/scripts/test.sh --suite consumer --form all --mode jit --rid linux-x64 --fetch never
```

Use `--mode all` for the required JIT/NativeAOT consumer matrix. Consumer
execution does not run the complete qualification selection.

## Own values, callbacks and threads

Initialize and use a runtime synchronously on one external managed owner thread.
Only one runtime/main interpreter may be initialized per process. Value access
and disposal also require that initializing caller; translated work executes on
the runtime's private execution thread. An `await` that resumes on
another thread does not preserve the contract; dispatch owning operations to a
dedicated thread in an asynchronous host.

| API | Result and ownership |
| --- | --- |
| `Execute(source, filename, cancellationToken)` | Compiles/executes statements in the runtime's `__main__` globals; returns `PythonExecutionResult` containing copied `StandardOutput` and `StandardError`. |
| `Evaluate(expression, filename, cancellationToken)` | Compiles/evaluates an expression in the same globals; returns an owning `PythonValue`. |
| `Import(module)`, `FromInt64(value)`, `FromString(value, surrogates)` | Return owning values. `FromInt64` takes a signed .NET `long`. |
| `PythonValue.GetAttribute(name)`, `Call(...)`, `Clone()` | Each returns an owning value. Call arguments remain owned by the caller. `Clone` owns another reference to the same Python object. |
| `AsInt64()`, `AsString(surrogates)` | Copy a value to .NET; conversion errors propagate. Ownership remains with the caller. |
| `PythonValue.Dispose()` | Releases that reference on its runtime's owner thread; repeated disposal of an already released value is harmless. |

Dispose **every** owning value before disposing the runtime, using nested
`using` scopes as in the sample. Outstanding values reject finalization; release
them and retry disposal. Values cannot cross runtime generations or be used
after disposal. Neither runtime nor values have finalizers that call Python,
so .NET collection does not replace explicit disposal.

`RegisterInt64Callback(module, name, Func<long,long>)` installs a one-argument
Python callable. The delegate stays rooted through shutdown; there is no public
unregister operation. Arguments/results use signed 64-bit integer conversion.
Managed exceptions become Python `RuntimeError` diagnostics. The sample uses
`checked` arithmetic to surface overflow. A callback must not reenter owning
API operations or dispose the runtime. It may run on a Python worker thread:
synchronize shared managed state, as the sample's `Interlocked.Increment` does.

Python `threading` workers use the real GIL/thread backend. The private execution
thread saves its GIL/thread state between API operations and restores it on entry. Arbitrary
managed workers cannot call the facade; it exposes no managed-thread attachment
API. Arrange for application workers to finish. Disposal invokes Python's
threading shutdown and drains active host waits/callbacks before releasing roots
and finalizing. It has no general shutdown deadline or thread-abort facility.
An uncertain boundary/shutdown failure prevents runtime reuse in that process.
Successful finalization allows a fresh runtime, as the sample demonstrates twice.

## Errors, output and text

Python failures throw `PythonException` with `PythonType`, `Message`,
`PythonTraceback`, `StandardOutput` and `StandardError`. Supply a useful
`filename` to `Execute`/`Evaluate` so frames identify the host operation. The
sample checks `ValueError`, `consumer_exception.py`, `raise_for_host` and
`inner`, then evaluates `40 + 2` successfully. Output written before a failure
remains on the exception.

`Execute` returns captured text and drains the host streams. Evaluate, import,
attribute lookup and calls also drain capture internally, but successful value
results expose no output properties. For a function's output, use an application
capture wrapper: the sample's `run_captured` uses `contextlib.redirect_stdout`
and `redirect_stderr`, stores `last_stdout`/`last_stderr`, then the host reads
those attributes. This captures Python text streams, not arbitrary OS descriptor
writes.

`FromString`/`AsString` use length-bearing code points, preserving non-BMP
characters and embedded NUL. `PythonSurrogatePolicy.Reject` is the default for
lone UTF-16 surrogates; explicit `Preserve` allows them in Python/.NET value
conversion. The sample round-trips `"λ🐍"`, `"a\0b"` and
`"\ud800x\udfff"` with `Preserve`. `PythonText.EncodeUtf8`/`DecodeUtf8`
always use strict scalar UTF-8. Names, options paths, source text and filenames
use NUL-terminated UTF-8 boundaries and reject embedded NUL/lone surrogates
before entry. A path passed as a Python string value still receives Python's
filesystem validation.

## Cooperative cancellation

`Execute` and `Evaluate` accept `cancellationToken`; `PythonValue.Call` has an
overload with the token first. Another thread may cancel while the owner runs.
An uncaught interruption is a real Python `KeyboardInterrupt`, reaching .NET
as `PythonException`, rather than `OperationCanceledException`. Python can
catch it and continue, so cancellation is not a guaranteed operation deadline.

Cancellable operations temporarily use the owner thread's Python trace hook and
restore the prior hook at operation end. Ordinary operations without a
cancellable token leave it alone. Python `host.wait_ms(milliseconds)` accepts
0 through 86,400,000, releases the GIL in timed waits and polls cancellation
at intervals of at most 10 ms, plus scheduling/GIL reacquisition delays. This
bounded polling applies to that host wait and cooperative Python execution;
arbitrary blocking I/O, other C calls and managed callbacks are not guaranteed
interruptible. Cancellation does not stop all Python workers.

## Publish, relocate and interpret evidence

For example, publish the processed sample after the shared lane release:

```bash
flock -n --close /tmp/dotcc-cpython-shared-build.lock dotnet publish cpython/samples/ManagedConsumer/ManagedConsumer.csproj -c Release -r linux-x64 -p:PublishAot=true -p:CPythonProject="$PWD/cpython/generated/TranslatedCPython/TranslatedCPython.csproj" --artifacts-path "$PWD/cpython/build/usage/processed-aot/products" -o "$PWD/cpython/build/usage/processed-aot/app" --disable-build-servers -m:1
```

Keep the whole publish directory. Assets resolve from
`AppContext.BaseDirectory`, allowing the stdlib/application package to move
together and run from an unrelated working directory. A normally trimmed
consumer proves reached application paths; the separate `boundary` suite roots
both the whole translated library and facade under AOT and remains required.

Historical product `20261017-210915-401ea00d` has receipt-bound selected ABI,
behavior/thread/lifecycle, trimmed consumer, relocated delivery and rooted AOT
results indexed in
[the R8 summary](../artifacts/owner-b-r8-20261017/r8-summary.json).
Its complete address gate failed. Strict V3 product `20261017-222803-2283f397`
has preserved source/PE/RVA checks but two unresolved pure-address groups and no
final runtime release. Neither closes current D10. The
[R11 index](../artifacts/owner-b-r11-20261017/r11-summary.json) records those
distinctions. This guide adds no runtime qualification result.

The target remains Linux x64, LP64 little endian with unsigned 16-bit `wchar_t`;
native Linux controls use 32-bit `wchar_t`. Managed callers use facade text
conversions. Selected stdlib/static builtins are recorded in
[stdlib.json](../config/stdlib.json) and [modules.json](../config/modules.json).
The separate `upstream-tests-static` profile supplies test helpers/assets and
does not expand the delivered API.

Native extension loading, arbitrary binary wheels/third-party packages,
subinterpreters/simultaneous runtimes, free-threaded Python, CPython's
experimental JIT, full CLI/REPL compatibility, fork/exec/subprocess/multiprocessing,
network/asyncio integration and OS signal-handler installation are excluded.
Windows-capable orchestration does not establish Windows execution support.
Required frozen upstream cases, the final product matrix, complete D10 audits,
reproduction and affected downstream regeneration remain plan acceptance gates.


## Explicit startup for source qualification

Ordinary isolated startup keeps bytecode writing disabled and UTF-8 mode enabled.
The owning options can explicitly select a writing-enabled, locale-sensitive
qualification runtime before any import or test discovery:

```csharp
var options = new PythonOptions(consumerOwnedStdlibCopy, applicationDirectory)
{
    WriteBytecode = true,
    Utf8Mode = false
};
using var runtime = PythonRuntime.Initialize(options);
```

Copy the selected test stdlib and fixtures into a fresh writable consumer-owned
directory before running upstream tests. Keep pinned ref/source trees read-only.
Use separate cache/fixture copies for each form/mode/run, and an explicit supported
locale (C.UTF-8 for the native controls). Select `Utf8Mode = true` in the
complementary cell. Isolated explicit configuration ignores ambient `PYTHONUTF8`
and `PYTHONDONTWRITEBYTECODE`; do not change sys.flags, decorators or frozen cases.
The caller owns and retains the configured directories for the runtime lifetime.

After normal regeneration,
`tests/StartupQualificationR37/Managed/StartupQualificationR37.csproj` checks
defaults/all option combinations across six owning generations.
`tests/StartupQualificationR37/Qualification/Qualification.csproj` accepts
`<writable-stdlib-copy> <report.json> <utf8-mode:0|1>` and uses `WriteBytecode=true`
before importing the unchanged upstream runner and frozen inventory. Build with
the ordinary explicit `CPythonProject` selection and run all final raw/processed
JIT/NativeAOT cells. These new actual-product harnesses remain unrun; the native
Host controls passed. Required SKIP/XFAIL outcomes still return failure.

B37's additional ordinary-suite wiring selects the same explicit qualification
options in `tests/Upstream/Program.cs` before discovery. Two temporary owning
PythonValues assert the actual bytecode/UTF-8 flags and are disposed before
bootstrap/runner imports. The existing framework hook still passes the fresh
consumer-owned target stdlib/app copy; runner, inventory, failure handling and
product defaults are unchanged.
[Additive ordinary-suite seal](../artifacts/owner-b-r37-20261018/ordinary-upstream-startup-source-ready.json)
retains the original ten-source startup seal separately. Actual new-product
execution remains pending the combined regeneration.

B37 final feature review selects common `HAVE_SYMLINK=1` after B39's validated
Linux-x64 source `4d9c60fa…`: native 361, current-Libc JIT/AOT 180 each, 21 units
with zero skips, and matching legacy/owner errno/result/extent observations. The
original immutable-V21 RED remains retained. Existing shared libc OS primitives
`symlink`/`readlink` provide the private Linux boundary; no native Python backend
is introduced. Other-host fallback remains unqualified. Both PIPE and SYMLINK
selectors pass the final ordered 193/286-unit/Clinic/header audit and five pure
guard controls. The final [B37 source seal](../artifacts/owner-b-r37-20261018/source-ready.json)
requires ONE combined A31 capture and fresh same-C/actual-product tests; no
CPython pass or completion-policy waiver follows from these service controls.


## B40 — current actual startup, raw counts, and explicit acceptance semantics

The actual V22 consumer binds its own library `0ff85f9f…` / MVID
`e016dffa-a975-4813-b651-6aa0c13ac835`, 98 library/14 facade/four entry
Compile/PDB documents, separately from canonical `cc68e84b…`. Four invalid
boolean controls and six direct Host generations match the exact native
transcript; six owning generations pass all options/defaults/callback/cache
checks with one static-program context. The unchanged original104 mode-0
comparison is 99 PASS/2 SKIP/3 XFAIL with all native/source bindings matching.

The first unchanged full4867 RAW JIT run completes 4859 unique PASS, four SKIP,
three XFAIL and one ERROR; zero FAIL/fatal/unrun. Its 4867 PASS events include
eight repeated loader IDs. The original failed report/exit remains preserved.
Native-faithful isolated embedding retains C unless the consumer selects its
locale before preinit. Explicit and legacy actual owners match native: explicit
consumer C.UTF-8 fixes the Unicode-directory reduction. Both surrogatepass cases
still SKIP natively/actually in mode0, and both PASS in supported mode1 with
unchanged methods and matched actual/native source/configuration bindings.

The user explicitly accepts only five identity-bound native exceptions (two
unconditional SKIPs/three XFAILs). The coordinator separately authorizes exact
two-case supported configuration coverage; mode0 skips are retained, and no
other skip/error/unexpected success/fatal/unrun or wrong source/PE is covered.
Eighteen focused acceptance controls pass. Revised ordinary Upstream and R37
qualification startup sources select C.UTF-8 on the existing program owner before
preinit; separate reference-only entries compile/bind to the exact actual PE.
The final correctly-started primary full run must still execute the metadata case
PASS. Public defaults and Host/runtime/header algorithms are unchanged.

[Exact evidence and remaining gates](../artifacts/owner-b-r40-20261018/final-handoff.json),
[five-exception manifest](../tests/ActualQualificationR40/five-exceptions.json),
[integrated gate](../tests/ActualQualificationR40/acceptance.py) and
[retained first-full decision](../artifacts/owner-b-r40-20261018/first-full-integrated-acceptance.json)
keep raw counts separate from acceptance. New193/both286 forms/JIT+AOT/full
D10/address/lifetime/ABI/thread/GC/lifecycle/rooted/trimmed/delivery/audit/downstream/
reproduction/P0–P6 remain required. No final matrix pass is claimed.
