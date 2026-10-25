# Bounded HostConsole contract

`python3 blink/tests/HostConsole/run.py` snapshots Host sources and runs ordinary
binary stream cases in JIT and NativeAOT. It checks byte-preserving input/output
and EOF, stdout/stderr capture order under concurrent writes, finite buffers and
nonblocking EAGAIN, cooperative wakeup from blocked input/output, combined output
budget versus capture truncation, and ownership of a console borrowed by InstanceIo.
The default lifetime ceiling additionally permits more than 1 MiB of ordinary
output while retained stdout/stderr capture remains capped at 64 KiB total.

There is no native oracle for this managed stream abstraction and no translated
guest claim. Caller-stream `LeaveOpen`, automatic pump disposal, protocol and both
machine execution modes belong to the later MachineApi integration gate. The runner
pins source copies and execution closures before and after each qualified run.

The updated console fixture passes JIT and NativeAOT on Linux x64 in
`artifacts/host-console/attempt-mi9e1gdi/receipt.json` (SHA-256
`5154937635d0eeae84b0565e7885ea19b13f06f8924c28cf33142aebbcc3d430`).
The receipt predates this README update and a later directory-backend reference
accounting cleanup; `HostConsole.cs` and this console test's executable source
have not changed since that run.
