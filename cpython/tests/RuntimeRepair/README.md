These controls exercise the actual shared Libc sources without a compiler project
reference or tool/resource rebuild. The native executable is an independent Linux
x64 C control. The managed executable tests JIT and NativeAOT, real OS delivery,
registration/raising owners, in-flight replacement/disposal, reentrant installation,
thread/errno restoration, directory ownership, and host-handle closure.

After the serial lane is granted, run from any directory:

```bash
python3 -B /home/marius/p/dotcc/cpython/tests/RuntimeRepair/run_controls.py
```

Each invocation retains its own timestamped outputs, logs and commands. Every
actual native/dotnet build holds the appropriate nonblocking flock with --close.
No compiler emission, canonical replacement or CPython qualification occurs.
The default subprocess must die by SIGINT without reaching unexpected-return;
exit130 alone is not accepted. The blocked subprocess restores the actual host
action/mask, installs a real handler and consumes its pending signal exactly once.
Host struct size/offset controls are distinct from DotCC's public signal structs.
Linux defines64 kernel signals, while glibc sigset_t reserves128 bytes; reserved
query-output bytes and padding cannot be compared as initialized native state.

B23's first AOT default RED, intermediate native reserved-tail control failure,
and final successful controls-r3 results remain separate immutable evidence.
