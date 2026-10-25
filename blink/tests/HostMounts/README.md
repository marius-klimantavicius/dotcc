# BCL filesystem and mount qualification

`python3 blink/tests/HostMounts/run.py`
builds a normal Linux C file-description witness and runs the authored Host
filesystem contracts in separate managed JIT and NativeAOT processes. It does
not execute a translated guest or qualify the full public machine API.

The C witness checks normal duplicate offsets, positioned writes, append,
rename, unlink with an open descriptor, and final close. The managed program
also checks live host writes and externally changed data, read-only denial,
explicit overlay/unmount restoration, nested mount precedence, cross-mount
rename rejection, namespace freeze, sequential sessions and descriptor cleanup,
eager COW import/export/discard, and aggregate root/COW storage limits. The
ordinary symlink-policy check uses a real valid symlink; there are no malformed
ELFs, invalid pointers, provider failures or injected faults.

The managed regressions additionally check independent opens versus `dup`, a
100-byte storage quota retaining a 60-byte unlinked file only once, capacity
queries and writes after unlink, shared mode/time/length updates, path recreation,
rename replacement, removed open directories, and byte/node release only after
the last open description's final close. These additions are host contracts;
the native C witness still covers the basic file-description operations above.
The same regressions run against both the direct host store and a mounted private
root, matching the storage composition used by `MachineWorkerHost`.

The runner freezes authored/copied source and tool identities, uses private
build and filesystem directories, preserves separate output/error streams and
first-failure receipts, and checks executable identities before/after each run.
Linux x64 is the qualification target; using BCL APIs does not constitute a
Windows execution pass.

The earlier fixture passed native, managed JIT and NativeAOT in
`artifacts/host-mounts/attempt-rmljxlg0/receipt.json` (SHA-256
`6d5cd50f78ccd4e71efc7647ed85cf52f233686ffd8b3cb658d0066a867627e0`).
All eight commands exited successfully; 168 source/tool/log/binary identities
matched in the independent completion audit. The fixture includes aggregate
private byte and node quotas. This paragraph is a documentation-only update
after that run, so that receipt records its previous README hash; executable
test and filesystem sources were unchanged for that historical documentation update.

The independent-open/unlink/replacement metadata and quota regressions passed
managed JIT/NativeAOT alongside the basic native witness in
`artifacts/host-mounts/attempt-zw8g4s1s/receipt.json` (SHA-256
`59a072d0564c52283eb0d210164f0b851c69fee01bb857784e9fdefaadb26b9b`).
This includes the mounted-private-root repetitions. The receipt records the README
before this result paragraph was updated; tested C# sources are unchanged.
No translated guest was executed.
