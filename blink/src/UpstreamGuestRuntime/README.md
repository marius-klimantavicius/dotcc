# Private guest runtime syscall dispatch

Source preparation only. This adapter adds Linux x86-64 syscall 324
(`membarrier`) to the pinned interpreter dispatcher. A small static C function
forwards its command, unsigned flags and CPU argument to
`blink_host_membarrier`. It introduces no execution loop, lifecycle owner,
native syscall fallback or generated-source edit. The separate authored C#
owner remains responsible for execution and binding the reviewed host service.

The adapter is applied **after** `UpstreamExecutionStop`. It requires the exact
immutable revision `f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580` original syscall.c
SHA-256 `4eb3f54173ba37341b300e7668cc4ba3650578cc3d23d713573ffa486ae0e2c3`
and exact stop-stage output SHA-256
`653c1ef70a4db9fef7968cf7cac5e57f1c9883668b6ba7899c2bbe232958f011`.
The checked-in unified patch is relative to that stop-stage predecessor, not
directly to immutable upstream. No existing stop adaptation is modified.

```sh
python3 blink/src/UpstreamGuestRuntime/stage.py \
  --predecessor <stop-output>/syscall.c \
  --predecessor-receipt <stop-receipt.json> \
  --output blink/generated/<fresh-guest-runtime-directory> \
  --receipt blink/artifacts/<fresh-guest-runtime-receipt.json>
```

The script checks original and predecessor hashes, validates the predecessor
receipt against the current reviewed stop script/patch/header, reproduces the
stop adaptation, and requires the new derivation to match `guest-runtime.patch`
exactly. Its receipt identifies both stages, the immutable input, predecessor
receipt, required headers, scripts, patches and resulting source. All source
inputs are rehashed before output. Output files must be fresh and located only
under campaign `generated/` or `artifacts/`; authored and immutable paths are
rejected. The canonical staging pipeline must retain this receipt chain and
freeze the matching host header and C# bridge/service alongside it.

The staged C source rejects compilation unless `DISABLE_THREADS`, `NOLINEAR`
and `DISABLE_JIT` are defined, and rejects either `HAVE_THREADS` or `HAVE_FORK`,
including contradictory external definitions alongside `DISABLE_THREADS`.
This limits the reviewed
guest contract to one guest execution thread with no guest fork. A future
threaded or multiprocess profile must replace/review this contract rather than
silently reuse it.

The intended managed boundary supports the narrowly reviewed query,
registration and private-expedited operation using a real BCL
`Interlocked.MemoryBarrierProcessWide()` fence, with registration owned by the
bound instance. It does not pretend to lock host pages or implement `mlock`,
and unreviewed command/flag combinations must remain explicit errors. The
capability/query values and executable behavior are qualified by the separate
host module and actual guest diagnostic; this source stage alone proves none
of those runtime claims. A successful fence is not evidence of guest thread,
futex, socket timeout or complete NativeAOT service support.
