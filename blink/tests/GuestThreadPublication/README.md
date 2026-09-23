# Clone TID publication

The child's first memory read must observe its published TID; it compares this
with `gettid`, then exits immediately. The parent waits for clear-TID and exits.
This is ordinary successful thread creation, without injected scheduling delays,
invalid guest inputs or a deliberately failing syscall. Linux publishes
`CLONE_PARENT_SETTID` before waking the child (see
[`kernel_clone`](https://github.com/torvalds/linux/blob/v6.17/kernel/fork.c#L2466-L2481)).
The managed owner gates the child
until the creating instruction finishes publishing that state.

```sh
mkdir -p blink/build/thread-publication
gcc -nostdlib -static -no-pie -Wl,--build-id=none \
  blink/tests/GuestThreadPublication/fixture.S -o blink/build/thread-publication/probe
blink/build/thread-publication/probe
dotnet run --project blink/tests/GuestThreadPublication -c Release -- \
  blink/build/thread-publication/probe
dotnet publish blink/tests/GuestThreadPublication -c Release -r linux-x64 \
  -p:PublishAot=true -o blink/build/thread-publication/aot
blink/build/thread-publication/aot/GuestThreadPublication blink/build/thread-publication/probe
```

Each managed executable performs 64 fresh owning executions and verifies normal
exit and complete release. The actual NativeAOT HTTP and IMDS guest matrices
separately cover higher-level runtime startup and both public execution modes.
