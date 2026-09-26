# Remaining qualification limits

Windows x64 is deferred by explicit user instruction and does not block the
current Linux scope. The following limits remain separate from passing consumer
and upstream test results; see [VALIDATION.md](VALIDATION.md) for executed evidence.

- Additional malformed-module/debugger investigation was stopped by an automated
  safety check on the native subagent. No specific rejected command was supplied.
  That new diagnostic work was paused rather than routed through another agent.
  Authored optional probes are unvalidated; they are not part of the default
  passing corpus. Ordinary existing assertions, ABI, callbacks and exhaustion
  checks continue separately.
- The fixture inventory must not be confused with execution coverage. Six
  upstream receipt/configuration files are empty, and some nonempty fixtures or
  dormant test registrations lack direct qualification. Native callback/file-open
  receipts identify actual fixture use; filenames alone do not establish support.
- Passing the saved corpus does not establish complete malformed-bytecode safety
  or a hostile-code sandbox. The translated implementation remains unsafe C#
  with the upstream allocator, object layouts and bytecode semantics.

These entries do not waive unfinished plan items. They explain the practical
limits of the available evidence and why an overall P0–P6 completion claim is
not appropriate while those items remain open.
