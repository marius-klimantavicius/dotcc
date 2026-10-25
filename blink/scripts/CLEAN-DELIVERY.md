# Clean delivery and public sample reproduction

`test-clean-delivery.py` checks a committed revision in a new detached worktree.
Commit the pipeline, public sample and runner before running it: the runner
requires its own bytes to match the chosen commit, and copies no uncommitted code.

```bash
python3 blink/scripts/test-clean-delivery.py \
  --revision <committed-delivery-revision> \
  --worktree-parent <writable-directory>
```

Run on Linux x64 with .NET 10, GCC/binutils, make and NativeAOT prerequisites.
The runner preserves its worktree and attempt-specific receipt/logs under
`blink/artifacts/clean-delivery/` on either success or failure. It creates no
commits. Schedule it after other shared compiler work.

The checkout starts clean, with no `bin`/`obj`, native build, generated or artifact
directories. The only copied asset is the manifest-verified Blink source archive.
Inherited selected-tool paths are removed from child environments so origin
binaries cannot replace the fresh checkout's compiler or postprocessor.

The runner builds `dotcc.sln` with `UseLocalLalrCc=false`, then invokes
`translate.sh --offline --hashes strict --jobs 4`. It reads the current shared
`artifacts/campaign/current-threaded.json` pointer and validates its completed
receipt, delivery builds, recursive tool identities and original authored sources.
The expected product selection comes from the frozen profile's current manifest
and authored binding list (currently 108 units). Every selected source/object must
match its recorded bytes and be produced in this checkout's new profile, with
zero reused objects. No historical 109-unit schema or translation pointer is used.

Raw and processed delivered source inventories are checked before and after
consumer execution. The current shared framework publishes both as buildable
projects; this check verifies byte stability and does not claim those directories
have read-only filesystem permissions.

A normal static ELF is compiled from `tests/MachineApi/guest.c` and run natively.
The actual public sample is built through `ManagedConsumer.slnx`, published as
Linux x64 NativeAOT, and invoked with `--run` and `--run-process` in both JIT and
NativeAOT forms. Its `memory-cap` guest maps/unmaps 64 MiB and must print exactly
the native `memory-available` result with exit zero and no diagnostics. This
exercises the current mounted-executable workflow, public owner and deployed
worker. It does not substitute a no-argument usage response for guest execution.

The receipt binds the commit, source/archive/runner, recursive fresh compiler
closure, tools, exact commands, NuGet assets, native tests, producer selection and
before/after consumer binaries. Each command owns a process group; timeout or
interruption terminates it and preserves the failed phase. Final source manifests,
compiler identities, guest bytes, HEAD and clean Git status are rechecked.

Installed tools and normal NuGet restore/cache are reused; this is not hermetic.
Offline restricts Blink archive acquisition. No HTTP service, Windows, custom
fault injection, malformed ELF or complete dependency-audit qualification is
claimed. Run the maintained `verify` suites for broader runtime qualification.

## Historical result from the former core sample

Observed clean pass: committed `f38de7f`, receipt
`artifacts/clean-delivery/attempt-0gfaz9m3/receipt.json` (SHA256
`4aaeca06fb629779914d24969e60de2fedcff1d456193f3494f2185107f1143b`).
All 109 objects were fresh, with zero reuse. Both normal samples matched their
native/profile reference; Linux NativeAOT published successfully on its first
attempt. The source/output checkout and all recorded compiler identities passed
the final unchanged checks. This records that exact revision, not later changes.
