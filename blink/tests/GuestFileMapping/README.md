# Ordinary private guest mapping checks

`python3 blink/tests/GuestFileMapping/run.py` uses the current native archive and
the maintained memory staging helper. It never translates the product or changes
compiler binaries. Run it only after the coordinator releases the serial build
slot; compile commands use the shared/native production locks.

The static ELF workload maps a normal file across its partial final page and
whole absent pages. It reads only available bytes and the partial-page zero
tail, changes protections, writes a private byte, replaces absent pages with
anonymous BSS and real file pages, discards anonymous contents, checks fixed
replacement and no-replace overlap recovery, closes the descriptor, and unmaps.
It creates and deletes its own ordinary data file.

The native adapter invariant consumer inspects actual guest page tables without
accessing absent backing. It verifies reserved file PTEs and EOF availability,
partial-page data, availability surviving protection changes, BSS replacement,
guest virtual/committed/reserved accounting, and final host-owner cleanup.
Its memory and throw sources come from the same staging helper as the product;
the native map capability adapter and host allocation boundary are included.

Each executable runs in a separate process with a 30-second timeout and process
group cleanup. Build commands are bounded to one process. Receipts retain input,
binary and command identities in `blink/artifacts/guest-file-mapping/native-*`.
No custom invalid-access, malformed ELF, or injected resource failure is run.
The [ElfRuntime suite](../ElfRuntime/README.md) builds the same workload as
`mapping-static` and runs it through both public execution modes using fresh
raw/processed products and JIT/NativeAOT. Native success alone is partial
evidence. Signal precedence is source-reviewed, not fault-probe tested.
