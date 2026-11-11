# Managed library qualification

Run from any directory with .NET 10 and a work directory on external storage:

```bash
python3 cui/tests/managed/run.py --work-root "$MANAGED_CUI_WORK" --form all --mode all
```

The runner sets external temporary directories and .NET artifacts paths. It
builds ordinary references, roots the entire wrapper in NativeAOT qualification,
recursively audits the public surface and executes meaningful failure/lifetime
cases. The surface audit checks callback Invoke signatures; CLR-generated
delegate constructors are runtime infrastructure, not wrapper address APIs.

Cases include invalid frames and stale/copied/nested scope disposal, compacting
GC before deferred paint, retained UTF-8 and mutable selection arrays, text/terminal/
texture disposal before painting, callback exceptions and reentry, owner-thread
and wrong-context use, bounds, close/reopen storage and callback-root cleanup.
Eight actual CUI allocator-observed lifecycles must return all allocations made
after observer installation. This excludes pre-observer context/font allocations,
the wrapper's NativeMemory allocations, the software renderer and graphics drivers;
wrapper arena bytes and registration counts are independently observed.

The three equivalent direct/wrapper workloads are 4 and 40 control rows, and a
20,000-row virtualized grid. Before timing, software RGBA and node counts must
match exactly. Every variant runs at least three independent process trials,
each with 200 warmup frames and 1,200 alternating paired samples. Optimized JIT
uses `DOTNET_TieredCompilation=0`; NativeAOT uses its normal compiled code. These
are UI construction and translated layout/paint measurements, without GL, vsync
or software rasterization in the timed region. Rendering is verified separately.
Report p50/p95/p99 microseconds, managed bytes/frame, wrapper arena reservation,
and separately instrumented actual core allocator high water and retention.

Gates reflect the intended small immediate-mode overhead: zero additional steady
managed bytes, construction p95 overhead at most 0.5 microsecond per emitted
node, and total median UI work at most twice direct CUI. The per-node gate limits
added work for the representative 200-node grid to 100 microseconds, below 1% of
a 60 Hz frame budget. Relative and allocation gates avoid claiming a hardware-
independent absolute frame rate. Core and wrapper retained storage must stabilize,
and all observed core allocations must be freed on disposal. Actual full-gallery
timings and page/input parity are separate ManagedGallery qualification.

The retained report records each variant and trial, with exact binary hashes and
log locations. Windows execution remains unrun; Linux results do not qualify it.
