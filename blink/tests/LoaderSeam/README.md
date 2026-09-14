# Native loader seam

Run `python3 blink/tests/LoaderSeam/run-native.py` from the repository after the
pinned native archive and service have been built. The harness invokes actual
upstream `LoadProgram` twice in fresh machines, without executing instructions.
It verifies fixed ELF entry bytes/program-header addresses against the pinned
file, and uses actual guest memory reads for argv, environment, aligned stack,
auxiliary-vector fields and the copied random bytes.

Observed receipt: `artifacts/loader-seam/attempt-ujcb9q9w/receipt.json`.
Both loads pass with entry `0x4015c4`, PHDR `0x400040`, six program headers and
13 nonterminal auxiliary entries. The native archive extracts 88 upstream
translation units, five beyond the interpreter probe: `argv.c`, `biosrom.c`,
`endswith.c`, `loader.c` and `tainted.c`. This is dependency evidence for the next
managed closure; it does not qualify real-mode support merely because a retained
loader function pulls the BIOS object.

The embedding owner must initialize the actual overlay table before absolute
paths reach `LoadProgram`. The first authored probe omitted `SetOverlays` and
failed with SIGSEGV in `OverlaysOpen` on its null table; that failed receipt is
preserved at `attempt-bly5yapz`. Calling `SetOverlays("", false)` matches upstream
frontend initialization. The managed version must still route every resulting
filesystem operation through its private owner; native host access here is only
the explicit oracle reading the pinned service file.

Only this valid static ET_EXEC fixture is qualified natively. Malformed files,
permissions, BSS, protection changes, complete guest startup and all managed
execution forms remain separate open gates.
