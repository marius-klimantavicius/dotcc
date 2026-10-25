# Public normal ReadyToRun service

This bounded consumer qualifies the R23 **normal ReadyToRun** service through
public `BlinkMachine.ImportImage`, `StartAsync`, `Ready`, published endpoints,
and `WaitAsync`. `/dotnet/dotnet /apps/service/CoreClrService.dll 8080` executes
inside translated Blink. CoreCLR, its R2R native code, and any residual JIT code
are interpreted guest instructions. `nativeaot` always describes the **host
consumer and matching host worker**; the guest remains CoreCLR.

Completed qualification covers three cells: processed/NativeAOT-host in both
execution modes, and raw/JIT-host/InProcess. Each completed the three exact HTTP
responses and natural guest exit within the original finite bounds, followed by
complete cleanup. Five of the eight possible cells remain unqualified. See
[measured results and limits](../../docs/P6-CORECLR.md#timed-normal-readytorun-result)
and `artifacts/coreclr-coordination/r3-adoption/all-final-audit.json` for the
combined source/deployment/report/event audit (6,408 checks, 8,154 files).
The prior automated content-filter stop and safeguards remain unchanged.
Existing IL console evidence (24/24), the original Slim service failures, minimal
IL control, and native-only R23 preparation remain independent evidence.

From the repository root, run the initial single processed NativeAOT-host cell:

```sh
python3 -B blink/tests/GuestReadyToRunPublic/run.py \
  --clean-delivery blink/artifacts/campaign/current-threaded.json \
  --profile-receipt blink/artifacts/coreclr-runtime/readytorun-native/attempt-wcalj6ct/receipt.json \
  --form processed --host-runtime nativeaot --mode InProcess \
  --seconds 3600 --instruction-limit 3000000000
```

Add `--compile-only` to build/freeze the selected host and ordinary worker
without starting a guest. Omit both `--host-runtime` and `--mode` for the four
service cells for one form. Both flags can be repeated to choose a subset;
`--form raw` selects the other form in a separate attempt. Builds and cells are
serial. Every attempt has a new private tree under
`blink/artifacts/coreclr-runtime/readytorun-public-{form}/attempt-*`. There is
no deployment reuse option. No translation or shared generated-source edit is
performed. The consumer projects use the normal worker deployment targets.

Defaults and maximums are 3,600 execution seconds and 3,000,000,000 guest
instructions per cell. Smaller positive finite budgets can be selected.
`--memory-mib 256` and `--virtual-memory-mib 4096` are exposed but fixed and
validated; all other machine resource limits retain product defaults. The
guest environment must exactly equal the retained CoreClrRuntime environment.
W^X, tiering, ReadyToRun selection, GC settings, networking, and timeouts are
not relaxed. Each HttpClient timeout is the same finite execution budget;
its cancellation token shares one global deadline beginning at StartAsync.
Startup, readiness, all response bodies, and the normal wait share that bound.
The outer command allows 60 seconds for cleanup after that bound. The minimal
guest source retains its finite 30-minute RequestHeadersTimeout.

Qualification requires exactly these responses, followed by normal
`RunExitReason.Exited`, exit 0, positive bounded instructions, empty guest
stderr, and exact stdout `SERVICE READY\nSERVICE STOPPED\n`:

- `GET /health`: 200, `CORECLR SERVICE OK\n`
- `POST /echo`, body `payload`: 200, `CORECLR:payload\n`
- `POST /stop`: 200, `STOP OK\n`

The HTTP `/stop` endpoint stops the application through its host lifetime
after completing the response. Emulator `StopAsync` and optional separate
worker `KillAsync` are failure cleanup only. A cleanup stop, cancellation,
timeout, signal, missing report, or incomplete body cannot qualify as normal.
ResourcesReleased, RunDisposed, MachineDisposed, WorkerGone, and an actual
positive separate-process worker PID are checked by both the probe and runner.
Python always drains its owned command process group, including on exceptions.
Failures and command logs remain in the attempt directory.

Each cell emits a small host-only `*.events.jsonl`, separate from captured
guest stdout. Every event is flushed, with a sequence number, UTC timestamp,
Stopwatch elapsed time, execution time since Start, and instruction count when
the public Completion result makes it available. Events identify image import,
Start request/return, Ready, each request/full response (status and body), guest
exit, wait completion, and disposal. There is no timer polling. The completion
observer and event writer catch their errors without aborting the guest;
such errors prevent evidence qualification afterward. Guest exit can race
the stop response, so its actual event order is retained.

The report retains import/start/Ready wait durations, Ready since Start,
each HTTP request's headers/full-body duration and time since Start,
post-stop wait time, guest exit since Start, total probe time, and final
instructions. The receipt also retains the probe command's outer process time.
The historical minimal IL control used 1,268,653,381 instructions and 2,586.742
guest-command seconds; 2,591.098 seconds describes its complete Python workflow,
including preparation. Those timing scopes must remain separate. Its public
startup and individual HTTP phase timings were not recorded. Different publish
metadata, probes and overlapping follow-ups prevent a causal guest speedup claim.
R23's native IL/R2R controls showed no native startup speedup. Static PE presence
does not prove runtime method selection or predict translated performance.

The runner consumes R23's actual native receipt schema, without an adapter:

- `passed`, `completed`, `native_runtime_execution`, and
  `native_validation.passed` must be true; `blink_execution` must be false.
- `profile` must bind normal non-composite framework-dependent R2R,
  `PublishAot=false`, `RID=linux-x64`, SDK 10.0.112, runtime/compiler 10.0.12,
  full commit `95017c711e6afc1085133d440e42b4bd78155701`, compiler package
  `Microsoft.NETCore.App.Crossgen2.linux-x64`, and default tiering.
- Top-level `closure`, `closure_sha256`, and `image_files` must agree with
  `profiles.normal`. `closure.json` uses the existing `Images`, `Files`, and
  `Environment` schema. `--closure` may explicitly select an identical closure
  document; its hash must match the receipt. No implicit schema fallback exists.
- `pe_proof` and `source_manifest` bind `{path, sha256}`. Both PE proof flags
  must prove native payload. The pure pinned R23 PE inspector rechecks the
  exact imported service bytes, R2R 16.0, native entrypoints/code, and retained
  IL, then compares them with the proof. The source manifest binds the minimal
  control Program.cs hash
  `0090390e0cb8d705375b0272d735130f13b3e5d30429c905fcc4bf606820bf7b`.
- Normal native measurements must contain all three exact responses, normal
  exit and no cleanup signals. Native input, source, runtime, compiler,
  package, frozen-tree and command-log identities are retained/rechecked.

The known native receipt SHA256 is
`81b8bf799e1c63079b0be2537ef96002a5a4d5cce69d155d7a4106ac742d5cd2`.
Its closure hash is
`f9f9dfec63eeab8b79595b26a88bd33ae0848463d511f847218e5a45569118f0`.
Its PE proof hash is
`2e0fcd5607b988c18ab23b7f1b7d89b2c7b8dd1d6a753714e2aa6de2ed0724ae`.
Its source manifest hash is
`3a00bf9e1b7d1778e72ba8baa9d720c9926c8fd8b371c322a1bf4cb6cd64771b`.
The runner records actual input hashes rather than silently selecting this
historical attempt. Parent-approved replacement normal receipts must satisfy
the same schema and validation.

Normal R2R retains IL for runtime versioning and methods requiring JIT, such
as unsupported/generic instantiations or runtime rejection of a precompiled
method. Tiering can also replace precompiled methods under default policy.
The distributed framework contains 230 R2R assemblies and 83 IL-only facades
with zero executable IL bodies. This does not prove method acceptance or zero
residual JIT. Official linux-x64 reference runtime-pack bytes differ from the
retained Ubuntu runtime at the same full commit; serviceability/version-bubble
checks may affect acceptance. The scope is executing the pinned R2R-bearing
closure with ordinary CoreCLR behavior. It makes no per-method acceptance,
zero-JIT, performance, composite-profile, or NativeAOT-guest claim. See
[R23 profile documentation](../CoreClrReadyToRun/README.md) for payload proof,
runtime comparison, package provenance, and licenses.

The live `current-threaded.json` pointer is pinned and `load_clean_delivery`
is called at both boundaries. An actual strict fresh producer, 110 selected
fresh bodies, zero reused objects, and the raw/processed body hashes are
required. Stale delivery is rejected. Reviewed CoreClrRuntime evidence and
deployment helpers and MachineApi/WorkerInstances source walkers are imported
with exact hashes in `support.py`; a changed helper fails closed. API, ordinary
worker, authored sources, selected generated form, project imports, and build
configuration are captured before the first build into one private tree.
Source membership, host SDK/runtime/compiler inputs, deployments, reports, and
command logs are pinned before and after use. Per-build production locks,
`-m:1`, disabled build servers, and `DOTNET_PROCESSOR_COUNT=2` are retained.

`passed` describes only the selected compile step or requested service cells.
`complete_form_matrix` and `fresh_qualification` require all four service cells
for that one form. Compile-only and subsets never claim a complete matrix.
`complete_eight_cell_matrix` is always false in an individual attempt. The
parent may aggregate four passing raw and four passing processed cells only
after comparing pointer/producer/profile/source/tool identities. This harness
does not alter or requalify the existing 24 IL console cases.
