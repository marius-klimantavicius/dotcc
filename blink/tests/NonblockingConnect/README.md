# Managed nonblocking connect contract

This host-only check uses ordinary successful IPv4 loopback connections through
explicit outbound grants. It allows either immediate completion or EINPROGRESS;
it never forces a connection to remain pending or injects transport failures.

```sh
dotnet run --project blink/tests/NonblockingConnect/NonblockingConnect.csproj -c Release
dotnet publish blink/tests/NonblockingConnect/NonblockingConnect.csproj -c Release \
  -r linux-x64 -p:PublishAot=true -o blink/artifacts/nonblocking-connect/host-aot
blink/artifacts/nonblocking-connect/host-aot/NonblockingConnect
```

Checks cover completion through poll and edge-triggered epoll with registration
before/after connect, no spurious pending HUP, SO_ERROR success/get-clear reads,
repeat connect, shared descriptor flags and duplicate ownership, caller-scope
cancellation, last-close draining, reused descriptors, concurrent owner disposal,
and blocking connect with SO_SNDTIMEO. The test does not assert that scheduler
timing exercised every transient state, nor qualify nonzero SO_ERROR through a
synthetic failure. Existing upstream error cases may qualify those separately.

The host retains the connect operation after syscall cancellation or send-timeout
expiry. Cancellation ends the wait with Canceled (translated as the existing
interruption contract); timeout returns InProgress. Poll and SO_ERROR can then
observe eventual completion. Last close/disposal terminates and drains the work.
Once establishment fails the socket must be closed and recreated; reading
SO_ERROR clears only the error, and never changes failed/pending state to connected.

The translated NativeAOT HTTP-client fixture and public-machine execution gates
are separate from these host-only checks; see the nonblocking-connect sub-plan.
