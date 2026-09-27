# Final Kestrel ManagedConsumer qualification

The actual Kestrel solution/JIT/NativeAOT qualification **passes** at
`../../artifacts/managed-consumer-delivery/attempt-ljvj6fxh/receipt.json`
(SHA-256 `502992598acf8897e45020c17aeed4c2007e08d980cac0796fe16433f965aaaf`).
All six commands pass: actual solution build, JIT sample, real worker/sample
NativeAOT publishes and AOT sample. Four workers and six native HTTP semantic
comparisons pass normal exit, restart, cooperative stop and cleanup; no cleanup
signal supplies success. Independent review checks 2,080 file/tree/source/log/
binary identities. The solution build retains 17 CS8632 nullable-context
warnings and no errors; this is not a warning-free-build claim.

Reproduce against the current threaded delivery:

```bash
bash blink/scripts/translate.sh --profile threaded
bash blink/scripts/test.sh --profile threaded --suite consumer-delivery
```

The recipe builds a fresh static-musl guest with Docker using
`tests/KestrelService/build-native.py --engine docker`, then executes
`native-profile.py --guest-receipt` against that exact producer receipt.
The official image digest is declared in `config/script-inputs.json`; image
acquisition and NuGet restore require their normal caches or network access.
Guest paths and native/profile receipt digests come directly from these commands
and are retained in the current campaign receipt. No historical attempt directory
is a prerequisite. Direct specialist invocation requires explicit
`--delivery-receipt`/`--delivery-sha256`, `--native-receipt`/`--native-sha256`,
`--profile-receipt`/`--profile-sha256`, and `--guest` arguments.

The runner accepts actual shared-framework or historical delivery schemas and
validates the complete
public raw/final manifests, 108 translation objects, staged headers, compiler and
producer logs, original authored sources, native source/tool/package/binary
identities and profile evidence. It never substitutes a reconstructed receipt.

The runner builds the actual `ManagedConsumer.slnx`, retains byte-identical copies
of the real solution's managed output, and runs its sample with the actual worker.
It then publishes both real projects with NativeAOT for Linux x64 and executes
that native sample/worker pair with the same Kestrel ELF and six guest environment
entries. The selected Kestrel profile couples the guest AS/DATA allowance and
backing cap at 128 MiB, while the guest GC cap remains 16 MiB. It permits ordinary
runtime address reservations without claiming 128 MiB physical use. Instruction
and wall bounds remain 100 million/60 seconds; the passing receipt uses these bounds.

Each pair checks health, normal HTTP stop and exit zero, a fresh worker process,
health again, cooperative stop, exact guest output, instruction bounds and the
worker's joined/quiescent/IO-disposed/memory-released cleanup report. The runner
compares actual request bytes and response semantics to the pinned native
profile. Only the validated actual RFC1123 Date value and header ordering may
vary; status, other headers and body must match. Four actual workers complete a
passing run. Custom fault and invalid-ELF tests are not part of this check.

Every invocation retains a new `artifacts/managed-consumer-delivery/attempt-*`
receipt with commands, logs, exit status, process-group cleanup, environment,
source snapshots and hashes, guest/delivery identities, raw HTTP, result and
cleanup reports, and complete execution directory hashes before/after each run.
JIT output bytes remain in `sample-jit/` and `worker-jit/` before subsequent
publishes can modify the original `bin/` directories; native published binaries
remain in `sample-aot/` and `worker-aot/`. Original authored files stay referenced
from `src`; copying build artifacts for evidence does not change project links.
It uses the existing WorkerInstances evidence helpers without executing that
suite. Other agents must keep the recorded source closure frozen during a run.

## Historical raw-socket baseline

`attempt-8k2fus34/receipt.json` passed all six commands and four actual workers
with the previous raw-socket DotNetService fixture and previous sample sources.
Its receipt SHA-256 is
`6c0647e95d8fe1df01b3a907e8343b3bd138fe5d913f0db142c7737590398f0b`.
All 121 input snapshots and command-log hashes were independently rechecked.
The solution build reported zero warnings/errors; NativeAOT publishes succeeded
with existing nullable-annotation warnings in authored translated bridges.
No cleanup signal was required. Both historical sample runs printed:

```text
Guest health: ok
Guest stopped: exit 0
Guest health: ok
Restart and cooperative stop: passed
```

That unchanged receipt qualifies only its preserved raw-socket baseline. It does
not qualify current Kestrel sources, complete P5 or start P6.
