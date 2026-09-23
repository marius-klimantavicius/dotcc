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
