# Explicit TCP grants

Normal BCL-level checks cover an isolated owner with no socket creation,
publication of one guest port to an actual loopback endpoint, denial of an
ungranted publication, real peer request/reply bytes, an exact outbound grant,
denial of another destination and source-to-listener escalation, and cleanup.
The policy snapshots caller collections. No DNS or implicit external destination
is granted. Existing callers passing null policy keep the legacy private-network
contract; the new machine API passes an explicit policy, isolated by default.

`python3 blink/tests/HostNetworkGrants/run.py` snapshots authored Host sources,
executes JIT and Linux x64 NativeAOT, and retains source/tool/log/binary hashes.
The policy is specific to this host API: no native Linux policy equivalence or
translated guest execution pass is asserted by this boundary test.

Both forms pass at `artifacts/host-network-grants/attempt-rhpwmuhv/receipt.json`.
This README was added after that run; frozen executable inputs remain in its
attempt. Actual guest networking and simultaneous-machine isolation are separate
P6 public API gates.
