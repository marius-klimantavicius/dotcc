# Managed main arguments

The backend accepts `int main()` and `int main(int, char**)`. The latter emits
`Run(string[] arguments)`; this array includes `argv[0]`, and an empty array
explicitly requests `argc == 0`. `Run()` and executable entry points use
`Environment.GetCommandLineArgs()`, including the BCL-provided program name.
Three-argument environment forms still receive an explicit ABI diagnostic.

The wrapper validates null arrays, null elements and embedded NUL characters,
then creates one native allocation containing the pointer vector, a final null
pointer and individually NUL-terminated UTF-8 strings. The encoding follows
.NET UTF-8 replacement behavior for unpaired UTF-16 surrogates. C/C++ can modify
these bytes and pointers without modifying the managed strings. The program's
heap ledger owns this allocation until `Shutdown()`, after global destructors;
no pointer into a movable managed string is retained. A repeated managed Run
creates a fresh allocation and retains previous argv storage until shutdown.
The generated host must retain the owner while using any returned pointer.

Synthetic evidence in `artifacts/campaign/main-arguments-001/results.json`
contains nine C++ checks; `artifacts/campaign/main-arguments-c-001/results.json` adds four C checks: native C++ argument behavior, static/instance executable
JIT/NativeAOT, and managed-host tests with empty/Unicode arguments, mutation,
validation, forced collection, owner isolation and global-destructor access.
This evidence runs on Linux x64; the implementation uses BCL encoding and
NativeMemory and still requires execution on the other planned platforms.
