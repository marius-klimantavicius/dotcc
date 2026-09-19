# Inherited guest standard streams and terminal probe

Source-only; native and managed qualification await review and runtime release.
This fixture fills the gap between standalone host stream callbacks and actual
guest standard-descriptor setup. It retains 108 canonical core objects, replacing
only the authored frontend, and uses the frozen Host and bridge snapshot from
the supplied assembly receipt.

Each of two normal upstream lifecycles calls the real `AddStdFd` for 0, 1 and 2.
That path queries host flags, creates the upstream `Fd` with `kFdCbHost`, and
checks inherited socket type. The fixture verifies three records, ordinary
read-only stdin/write-only stdout and stderr, and non-socket status. Native
extra open flags are not assumed equal to private bookkeeping flags.

The frontend executes actual `0f 05` guest instructions for:

- `read` (0): consume the 17-byte binary input for that cycle into a mapped
  cross-page buffer, checking exact bytes and surrounding canaries. Only the
  second cycle requests and checks normal EOF.
- `writev` (20): write a six-byte binary prefix followed by the guest input to
  stdout through a cross-page Linux iovec table and payload.
- `write` (1): write a distinct seven-byte binary payload to stderr.
- `ioctl` (16): request Linux `TIOCGWINSZ` (`0x5413`) on captured stdout with a
  valid cross-page eight-byte winsize buffer. Require guest return `-ENOTTY`
  (`-25`) and unchanged buffer/canaries. This ordinary nonterminal probe is
  observed in the selected service startup; it is not fault injection.

All transfer loops require positive progress and accept actual short counts
without requiring identical native and managed chunk boundaries. There are no
forced shorts, output-limit exhaustion, invalid buffers or injected failures.
Exact fixtures are recorded in `fixtures.json`, including embedded NUL, high-bit
and newline bytes. Two cycles consume 34 bytes, produce 46 stdout bytes and 14
stderr bytes. These are explicit byte comparisons, not decoded-text matches.

The C execution frontend never prints diagnostics to stdout or stderr. Native
execution receives a frozen stdin file and has real fd1/fd2 redirected to guest
capture artifacts. Its wrapper writes a separate bounded C-produced diagnostic
sidecar. Managed execution binds the same input to `InstanceIo`, exports private
`CapturedOutput` through harness-only BCL file writes, and copies the C report
before teardown. Managed process stdout/stderr must remain empty. Any native
diagnostic noise contaminating guest streams fails the exact byte comparison.
Partial captures and reports are retained on ordinary test failure where they
were produced.

Both cycles free guest pages and their machine/system state. They deliberately
do not call guest `close(0..2)`: upstream `DestroyFds` frees metadata without
closing the underlying descriptors. Afterward all three host standard
descriptors must still answer `F_GETFL`; the managed owner also checks its three
remaining descriptors before disposal. This qualifies metadata cleanup and
retained stream lifetime, not descriptor-close behavior. GuestIo separately
qualifies guest file close and duplication.

Managed execution forces compacting GC before the run, checks the same bounded
retained HostMemory pool after both cycles, then invokes final disposal. No
translated call uses the disposed owner, and no post-disposal zero counters are
claimed. No service, worker, ELF loader or socket scope is added.

The runner copies and identifies native archive/config/headers, fixture files,
frontend/consumer templates, retained canonical C/object/producer receipts,
Host/bridges, compiler/postprocessor and tools. It records hashes for closed
logs, exact captures and sidecars, verifies executed binaries before/after, and
rechecks input/capture/source identities at completion. Raw output is retained
separately from ordinary semantic postprocessing, with the full derived library
rooted for NativeAOT.

After coordinator review and serialized release:

```sh
python3 blink/tests/GuestStreams/run.py --native-only \
  --assembly-receipt blink/artifacts/core/objects/<qualified-key>/receipt.json
python3 blink/tests/GuestStreams/run.py \
  --assembly-receipt blink/artifacts/core/objects/<qualified-key>/receipt.json
```

Attempts use isolated `TMPDIR` and output directories under
`generated/guest-streams/` and `artifacts/guest-streams/`. The full run compares
native and raw/optimized JIT/NativeAOT, without rebuilding the shared compiler.
