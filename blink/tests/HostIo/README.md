# Normal file and stream callbacks

`python3 blink/tests/HostIo/run.py` compares real C file operations through native
libc and the authored managed callback boundary in raw/optimized JIT/NativeAOT.
Cases cover shared descriptor cursors and duplication/close, vector I/O, sparse
zero fill, 128 KiB transfer with short-I/O loops, captured streams and two private
host owners. Ordinary missing-file and exclusive-create errors remain covered.

Historical invalid-descriptor and unbound-owner calls are excluded from current
execution. No custom allocation/host failure is injected. Passing this standalone
callback harness does not qualify actual guest service startup or the worker API.

Current normal-scope execution passes all four forms at
`artifacts/host-io/attempt-f61wdrei/receipt.json` (SHA256
`f48c848dbedea45a5edf045aa6cd37406e197e7bc3eca92260009a07cc806531`).
