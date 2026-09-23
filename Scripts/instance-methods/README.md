This finite Linux x64 gate reuses the normal C and managed consumer in
`ManagedLibraryTests.InstanceMethods.cs`. It compiles a native C witness, emits
an instance-ABI object, links a nested split managed library, and runs the same
consumer under JIT and NativeAOT. The consumer checks two owners, callback tables
and casts, pointer-width layout, shared libc sort/search/thread callbacks, pinned
ordinary/aligned globals, TLS, nested and concurrent ownership, and disposal.

Build the Release CLI first, then run `python3 Scripts/instance-methods/run.py`.
The runner creates a fresh attempt below `/tmp/dotcc-instance-methods`, snapshots
the actual compiler, records commands and output hashes, and checks authored and
compiler inputs again after completion. It stops at the first failure. This gate
does not claim native ABI compatibility for instance function pointers: the
native witness checks ordinary C behavior; the managed consumer checks the
explicit instance extension. Broader diagnostics and cross-object managed
boundary/variadic coverage are in the unit and functional suites.
