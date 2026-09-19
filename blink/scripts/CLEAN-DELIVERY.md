# Clean delivery and sample reproduction

`test-clean-delivery.py` checks the final public workflow at a committed revision
in a new detached worktree. Commit the translation pipeline, sample and this
runner before running it. The runner checks that its own bytes match the chosen
commit; uncommitted delivery code is never copied into the new checkout.

From the repository root on Linux x64:

```bash
python3 blink/scripts/test-clean-delivery.py \
  --revision <committed-delivery-revision> \
  --worktree-parent <writable-directory>
```

This command starts the whole reproduction. Schedule it after other shared
compiler work and performance measurements. It preserves a unique attempt
receipt and stdout/stderr logs under `blink/artifacts/clean-delivery/`. It also
preserves its detached worktree on success or failure; retry with a new worktree
instead of reusing partial output. It makes no source changes or commits.

The checkout begins with clean tracked files and no `bin`/`obj`, native build,
generated or artifact directories. The only copied asset is the exact Blink
archive pinned in that revision's source manifest. The runner then performs:

1. A fresh Release `dotcc.sln` build with `UseLocalLalrCc=false`.
2. `blink/scripts/translate.sh --offline`, requiring all 109 selected objects to
   be freshly produced with zero reuse, plus passing pinned upstream native tests.
3. Verification of the separate read-only raw snapshot and final post-processed
   project's complete source manifests.
4. `dotnet build blink/ManagedConsumer.slnx -c Release` and the actual JIT sample.
5. A Linux x64 NativeAOT sample publication and actual execution.

Both sample transcripts must exactly match a freshly executed native core
reference for normal arithmetic, memory, bounded execution and guest exit rows.
The ABI line uses a separately compiled profile-layout reference because the
managed signal-jump record intentionally differs from the native one. The final
ownership accounting line must report nonnegative retained mappings and bytes
within the driver's 64 MiB bound. These numbers describe caches before driver
teardown; they do not by themselves prove absence of leaks.

Receipts record the Git revision, runner/source/archive identities, exact
commands, installed tools, produced compiler dependencies, NuGet asset files,
native upstream revision/test paths, delivery producers and before/after sample
binary hashes. Raw/final source identities, compiler identities and clean final
Git status are checked again after execution. Each command runs in its own
process group; a timeout or interruption terminates that group and preserves the
failed phase and logs. An isolated `TMPDIR` belongs to each attempt.

This is a clean source/output reproduction using installed SDK/native tools and
normal NuGet cache/restore, not a hermetic build. Offline applies to Blink's
pinned archive acquisition. The gate qualifies the normal core showcase; it
does not execute an HTTP service, translated worker, Windows runtime, custom
fault-injection or invalid-ELF tests, or the separate complete dependency audit.
No prior campaign result substitutes for execution in this checkout.
