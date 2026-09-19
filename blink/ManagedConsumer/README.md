# Translated Blink C# consumer

The sample uses the separate authored `Managed.Emulation.Execution` C# project.
That project owns initialization, ELF loading, the interpreter loop, cancellation
and cleanup through public translated Blink functions and types. CPU execution
remains the translated upstream implementation. The generated library does not
contain the campaign `CoreProbe` or a C execution driver.

The sample mounts the pinned service ELF and instance file in a private filesystem,
starts one synchronous interpreter thread, waits for captured guest readiness,
publishes its loopback listener, and sends real health and normal stop requests.
It checks exact responses, captured output and exit status. This is one controlled
local invocation; it does not provide the planned subprocess protocol, concurrent
instances or restart API.

## Generate, build and run

From the repository root, with the documented Linux x64 prerequisites:

```bash
dotnet build dotcc.sln -c Release -p:UseLocalLalrCc=false
bash blink/scripts/build-guest.sh
bash blink/scripts/translate.sh
dotnet build blink/ManagedConsumer.slnx -c Release
dotnet run --project blink/ManagedConsumer/ManagedConsumer.csproj -c Release --no-build
```

The two optional positional arguments select the local service ELF and instance
file, for example when running from another directory. `--help` prints usage.
The default files are `blink/build/guest/service` and
`blink/tests/ServiceFixture/instance.txt`.

The solution references original projects under `src`. The generated product
project links original bridge source files and references the original Host
project. Edit authored implementation in `src` and build normally; generation
never overwrites those files. Archival raw/profile copies serve test provenance
only. Postprocessing transforms generated sources in private staging and the
published project is rebuilt against the original authored sources.

For NativeAOT:

```bash
dotnet publish blink/ManagedConsumer/ManagedConsumer.csproj -c Release -r linux-x64 -p:PublishAot=true -o blink/build/managed-consumer-aot
blink/build/managed-consumer-aot/ManagedConsumer
```

All translated calls and bindings stay on the interpreter thread. The caller
keeps IO and cancellation alive until execution has returned, then disposes them.
Run a fresh process for each invocation because upstream retains static caches.
A bounded stop deadline and instruction budget apply; these are not a claim of
hostile-code sandboxing or a Windows execution pass.

The revised sample passed the real solution build, JIT and rooted Linux x64
NativeAOT execution against delivery `translation/attempt-4yjaed1_`. Both runs
returned exact health/normal-stop responses and printed `Guest health: ok` and
`Guest stopped: exit 0`, with empty stderr. The retained receipt is
`artifacts/managed-consumer/attempt-3c0qzs2p/receipt.json` (SHA-256
`6a13ccf38f409879ae170305aac97f18c1346bc0cc0c56a944a0e49bb26ae70a`).
Original authored and generated source hashes stayed unchanged. The broader P4
four-mode service and stop/deadline matrices remain separate gates. Older
normal-core sample receipts describe the preceding test-frontend sample.
