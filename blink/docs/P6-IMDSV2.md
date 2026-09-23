# P6 extension: private IMDSv2 simulation

Implement the selected small BCL HTTP listener, owned by each execution. Route
guest `169.254.169.254:80` to its private loopback endpoint through the existing
nonblocking TCP implementation. No ASP.NET production dependency, native host
helper, host network configuration, implicit outbound grant or credential discovery.

- I0: immutable public options and worker serialization; bounded listener;
  IMDSv2 token issuance, expiration and execution isolation; explicit metadata,
  identity document, user data and role credentials. Commit implementation.
- I1: guest socket protocol/lifecycle checks and a real AWS SDK NativeAOT guest;
  JIT and NativeAOT consumers, both execution modes, independent concurrent
  machines and same-machine restart. Refresh translation receipts and run
  relevant network/API regression gates. Commit verified tests and delivery.
- I2: usage documentation and evidence ledger. Commit final milestone.

Credentials are supplied explicitly with a fixed expiration for each execution.
SDK cache refresh is supported; automatic credential rotation, STS, signed
identity documents, IPv6 IMDS, and full EC2 metadata coverage are separate work.
No custom fault injection, malformed ELF or manufactured transport failure tests.
Keep actual failures and incomplete checks visible; do not substitute compilation
for execution evidence. Record Linux qualification; Windows remains unrun.

I1 runtime prerequisite found during actual SDK execution: the selected upstream
non-linear-memory path appends/reallocates `g_hostpages` in `TrackHostPage` without
synchronizing concurrent writers or `FindHostPage` readers. Replace this pair
using typed internal function overrides and a per-program BCL page table. Keep
allocation/free and page-table logic upstream. Publish immutable page slots;
retain old array snapshots for in-flight readers during growth. Validate ordinary
concurrent registration/lookup and repeat the actual guest. No upstream patch or
synthetic guest fault is required. Also mirror upstream `Blink()` resumption
after a handled architectural signal; unhandled signals remain terminal.

Actual HTTP/SDK repeats also exposed child execution before `SysSpawn` publishes
`CLONE_PARENT_SETTID`. Gate new managed workers until their creating instruction
finishes; publish even when that instruction unwinds, and dispose gates only
after workers join. Cover the child's initial TID read and immediate clear-TID exit with
an ordinary valid ELF, plus actual NativeAOT runtime startup. Do not increase
instruction budgets to mask startup stalls.
