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

The runner freezes authored/copied source and tool identities, uses private
build and filesystem directories, preserves separate output/error streams and
first-failure receipts, and checks executable identities before/after each run.
Linux x64 is the qualification target; using BCL APIs does not constitute a
Windows execution pass.

Native, managed JIT and NativeAOT passed in
`artifacts/host-mounts/attempt-rmljxlg0/receipt.json` (SHA-256
`6d5cd50f78ccd4e71efc7647ed85cf52f233686ffd8b3cb658d0066a867627e0`).
All eight commands exited successfully; 168 source/tool/log/binary identities
matched in the independent completion audit. The fixture includes aggregate
private byte and node quotas. This paragraph is a documentation-only update
after that run, so the receipt records the previous README hash; executable
test and filesystem sources were unchanged after qualification.
