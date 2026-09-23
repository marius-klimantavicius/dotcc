# Bounded HostConsole contract

`python3 blink/tests/HostConsole/run.py` snapshots Host sources and runs ordinary
binary stream cases in JIT and NativeAOT. It checks byte-preserving input/output
and EOF, stdout/stderr capture order under concurrent writes, finite buffers and
nonblocking EAGAIN, cooperative wakeup from blocked input/output, combined output
budget versus capture truncation, and ownership of a console borrowed by InstanceIo.

There is no native oracle for this managed stream abstraction and no translated
guest claim. Caller-stream `LeaveOpen`, automatic pump disposal, protocol and both
machine execution modes belong to the later MachineApi integration gate. The runner
pins source copies and execution closures before and after each qualified run.
