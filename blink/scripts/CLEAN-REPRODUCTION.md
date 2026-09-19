# Clean checkout reproduction

`test-clean-reproduction.py` creates a unique detached worktree at a committed
core revision. It first verifies a clean Git status and absence of compiler
`bin`/`obj` and Blink `ref`/`generated`/`build`/`artifacts` outputs, then copies
only the source-manifest-pinned Blink tar archive into the new `ref` directory.
It does not copy a compiler, native binary/archive, emitted C# object, or build
output. The orchestration script runs from the existing campaign checkout and
records its own hash separately from the reproduced Git revision.

Use explicit paths rather than relying on machine-specific defaults:

```sh
python3 blink/scripts/test-clean-reproduction.py --prepare \
  --revision <committed-core-revision> \
  --worktree-parent <writable-parent>
```

Preparation prints an attempt receipt. It performs no builds or execution gates.
After any concurrent performance measurement is finished, create a dedicated
empty temporary directory outside the new checkout and resume:

```sh
mkdir <attempt-directory>/tmp
env TMPDIR=<attempt-directory>/tmp \
  python3 blink/scripts/test-clean-reproduction.py \
  --resume <attempt-directory>/receipt.json
```

Record that exact command and environment alongside the receipt. The campaign
run stores it in `launch.json`. The isolated TMPDIR also bounds the compiler's
recursive temporary-header discovery to this reproduction attempt.

Resume runs the documented Release solution build with
`-p:UseLocalLalrCc=false`, offline source verification, native interpreter and
25 assembly comparisons, profile staging, all 109 source emissions with zero
object reuse, actual core raw/optimized JIT/NativeAOT, direct IL/ABI checks and
publication inventory. The full repository test suites are not run by this
script. A failure preserves its phase, command, logs and worktree; it does not
turn a partial build or old receipt into a pass. Retry from a new preparation
rather than adding stale artifacts to the failed tree. Worktrees are preserved
for inspection, not automatically deleted.

This is a clean **source/output** reproduction on Linux x64, not a fully
hermetic build. It reuses installed .NET SDK/native tools and the host NuGet
package cache; normal restore may fetch declared packages. Tool hashes,
project-assets identities, native source/tool receipts, emitted producers and
execution/publication identities are retained. Offline applies to the pinned
Blink source fetch, not the whole .NET restore. No source-toolchain distribution
archive, container image or Windows execution is implied.

The experiment covers the bounded valid core only. It does not execute the
service or translated worker. Malformed-ELF work is excluded by user direction;
no unrun cases are counted as passes. Direct IL and published
ELF inventories retain unresolved indirect/framework/runtime limits and do not
establish sandboxing or close the bundled P6 runtime-dependency gate.

The initial campaign launch recorded the preparation hash before resuming but
used an evolving main receipt. Its exact preparation payload was subsequently
recovered by removing only known post-resume fields, and accepted only after
its SHA256 matched that pre-existing launch hash. It is retained separately as
`prepared-receipt.json`, with the method in `prepared-receipt-recovery.json`;
this is explicit recovered provenance, not a claim of an originally saved copy.

Current command timeouts use Python's direct-child termination. Descendant
compiler/build processes are not guaranteed to be drained on timeout. Any
failed/timed-out attempt therefore needs an explicit process check before a
retry; this runner does not claim process-tree containment. A successful run
still requires all recorded commands and independent gates to complete.

## Observed result

Detached commit `717ba668cb0575e459b0ff21dfdf16fba1cdb58a` passed in
`/home/marius/p/dotcc-blink-clean-mukw8i3k`. The retained orchestration receipt is
`/home/marius/p/dotcc-blink-campaign/blink/artifacts/clean-reproduction/attempt-6pfwe3d4/receipt.json`;
`verification.json` independently rechecks its linked receipts, clean final Git
state, unchanged campaign tools, and recovered preparation hash.

- Release solution build: zero errors, 17 existing xUnit analyzer warnings.
- Pinned native interpreter: all 25 selected assembly comparisons pass.
- Managed source: all 109 objects freshly emitted and linked; zero cache reuse.
- Actual core: raw/optimized JIT/NativeAOT pass, execution receipt
  `core-execution/attempt-xo_zqqig` inside that clean worktree.
- Configured ABI: Machine=22576, System=3016; actual native/profile checks pass.
- Direct IL inventories: 5597/5587 methods, zero traversed native imports/errors;
  17 native declarations and 155 indirect/594 virtual sites remain explicit.
- Execution-hash-checked publication audit: `publication-audit/attempt-ciqxzgs4`
  passes in the clean worktree.

The orchestration receipt SHA256 is
`caabce37b242743a4586006c260d092f90aa10cd921b2fb8340ff37e51e091e6`.
The native log records fresh GCC compile commands; the only asset seeded into
the clean worktree was the verified Blink source archive. No failure or timeout
occurred. The reconstructed preparation record and timeout-cleanup limitation
above remain disclosed rather than being hidden by the successful outcome.
