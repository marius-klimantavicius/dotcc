# Final ManagedConsumer baseline qualification

Run from the repository root after the threaded public translation is available:

```bash
python3 blink/tests/ManagedConsumerDelivery/run.py \
  --guest blink/artifacts/dotnet-guest-musl/attempt-8za50rji/publish/DotNetService \
  --delivery-receipt blink/artifacts/translation/attempt-i4a5mfa8/receipt.json
```

The runner builds the actual `ManagedConsumer.slnx`, runs its managed sample with
the actual managed worker, publishes both real projects with NativeAOT for
Linux x64, then executes that native sample/worker pair with the same guest ELF.
The sample checks exact HTTP health/stop responses, normal guest exit, fresh
worker restart, cooperative stop, exact guest output, instruction bounds and
the worker's joined/quiescent/IO-disposed/memory-released cleanup report.
Each successful sample creates two actual workers. Custom fault and invalid-ELF
tests are not part of this check.

Every invocation retains a new `artifacts/managed-consumer-delivery/attempt-*`
receipt with commands, logs, exit status, process-group cleanup, environment,
source snapshots and hashes, guest/delivery identities, and complete execution
directory hashes before and after each sample run. Native published binaries
remain in that attempt's `worker-aot/` and `sample-aot/` directories. The runner
requires the final generated files to match the supplied passing public delivery.
It uses the existing WorkerInstances evidence helpers without executing that
suite. Other agents must not modify the recorded source closure during the run.

`attempt-8k2fus34/receipt.json` passes all six commands and four actual workers.
The receipt SHA-256 is
`6c0647e95d8fe1df01b3a907e8343b3bd138fe5d913f0db142c7737590398f0b`.
All 121 input snapshots and command-log hashes were independently rechecked.
The solution build reports zero warnings/errors; NativeAOT publishes succeed
with the existing nullable-annotation warnings in authored translated bridges.
No cleanup signal was required. Both sample runs print:

```text
Guest health: ok
Guest stopped: exit 0
Guest health: ok
Restart and cooperative stop: passed
```

This qualifies the existing raw-socket .NET NativeAOT fixture only. The newly
required ASP.NET Core/Kestrel fixture remains pending; this receipt does not
complete P5, broaden guest compatibility or start P6.
