# Normal host memory lifecycle

Run `python3 blink/tests/HostMemory/run.py` after building the compiler and native
Blink archive. The runner compares the authored memory boundary and staged
upstream `InitMap` in native/profile and raw/optimized JIT/NativeAOT forms.

Current cases cover aligned zero-filled allocations, valid cross-page copies,
read/write protection metadata, additional live allocations, exact unmap,
64-page slabs, owner disposal/reinitialization, and independent host threads
retaining distinct allocations through compacting GC. The normal native core
also executes through the same memory boundary. This does not qualify all
upstream guest page-table algorithms or establish a hardened isolation boundary.

Earlier quota exhaustion, invalid arguments, foreign-owner mutation, unsupported
mappings and operations outside owner lifetime are historical excluded cases.
They are not executed by this harness or reported as current passes. Normal
assertion failures still terminate the test with a nonzero result.

Current normal-scope execution passes all four forms at
`artifacts/host-memory/attempt-16k81wpo/receipt.json` (SHA256
`0e5dd6bb725edc507ff58989ad82edeba00a6d3776c9a2a32af68f19c17d5bbb`).
