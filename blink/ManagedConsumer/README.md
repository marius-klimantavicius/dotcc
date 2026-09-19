# Translated Blink usage sample

This separate C# application references the final post-processed
`generated/TranslatedBlink/TranslatedBlink.csproj` and its frozen host library.
It binds private host services, invokes the translated owning driver once, and
releases the bindings and host resources before the process exits.

The driver uses the actual translated upstream interpreter for arithmetic and
memory writes, a bounded branch loop, and normal Linux `exit`/`exit_group` calls.
It prints the observed registers, memory, instruction counts and guest exit
statuses, checks their expected values, and returns a nonzero process status on
a mismatch. The arithmetic result is register `ax=42` and memory `43`; the two
guest exit statuses are `37` and `42`. Those guest statuses are observations;
the sample's own successful exit status is zero.

This is the normal interpreter showcase portion of P5. The planned owning
service API, readiness, HTTP request, captured service output and restart example
still require a qualified translated service worker. The independent
`BlinkInstance` controller has no such worker yet and is not used by this sample.

## Generate, build and run

From the repository root on Linux x64, with .NET 10 and the prerequisites in the
[campaign README](../README.md):

```bash
dotnet build dotcc.sln -c Release -p:UseLocalLalrCc=false
bash blink/scripts/translate.sh
dotnet build blink/ManagedConsumer.slnx -c Release
dotnet run --project blink/ManagedConsumer/ManagedConsumer.csproj -c Release --no-build
```

Generation must finish before opening or building the solution: both generated
projects are ignored build products. The sample references the final project
directly; it does not load a cached campaign assembly or a native Blink library.

For NativeAOT, publish and run the sample as a standalone process:

```bash
dotnet publish blink/ManagedConsumer/ManagedConsumer.csproj -c Release -r linux-x64 -p:PublishAot=true -o blink/build/managed-consumer-aot
blink/build/managed-consumer-aot/ManagedConsumer
```

The project roots the entire translated assembly for NativeAOT. Linux x64 is the
current execution target; other platforms require separate execution evidence.

## Ownership

Bindings are thread-local, so the sample stays on one thread throughout core
execution and unbinding. Its private environment and filesystem start empty.
The translated driver owns its interpreter context, bounded memory and exit
callbacks; the application owns the BCL host objects. Teardown runs while those
host bindings are still available. Upstream retains process-global cache state,
so this application deliberately returns immediately after its single driver
invocation; run a fresh process for another invocation.

The driver prints its observations directly. This sample does not present those
lines as captured guest service output or claim HTTP service readiness.
