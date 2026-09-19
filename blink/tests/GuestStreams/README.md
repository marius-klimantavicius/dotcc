# Inherited guest standard streams and terminal probe

Native and raw/optimized JIT/NativeAOT qualification passed against canonical
assembly `73428887715220efa9380f0dc8632efa11c202dbe67fe72630abda978cd4db75`.
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

Observed receipts:

- Native-only: `artifacts/guest-streams/attempt-wl8t5nv8/receipt.json`, SHA256
  `5825dd864319ef666939d6bf2cebfb6bae68a8462c89391b1dc1270f7c2fa6cb`.
- Native plus all four managed modes:
  `artifacts/guest-streams/attempt-21z_o_3s/receipt.json`, SHA256
  `0f62de4b0ffed1af3c25a4a9b09863cd8ba098ad790912fd173d455be3c81104`.

Every execution matched the exact 46-byte stdout and 14-byte stderr fixtures
after consuming 34 input bytes. The separate report confirmed two lifecycles,
six inherited descriptor records, two metadata cleanups, two ordinary ENOTTY
results, and three surviving standard descriptors. Managed process diagnostics
were empty. Final independent checks matched the frozen input, captures, logs,
executed binaries, retained producers, Host snapshot and tool/source identities.
This is selected stream/terminal contract evidence, not service startup or
execution-stop qualification.

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
