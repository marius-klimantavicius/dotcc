# Independent literal/builtin/filesystem review, round 4 — executed gate

Reviewer `/root/manual_edg_supervisor/review_m3`, 2026-10-21. Review only;
no reviewer build/test/native/compiler process, Git operation or implementation
edit. Canonical IDs M1L-01..09 retain their previous meanings.

The first combined build failed with CS1061 before testing. The next Release
build succeeds with zero warnings/errors, and the managed run reports67 passing
groups and12 failures. These are **not79 passing groups**, and the batch is
not accepted. The passing groups include the new literal/builtin/header/raw/
#line/character integration assertions and the prior60-group regression gate.

**M1L-10 — High: host qualification rejects the actual intended ABI host.**
The provider's exact RuntimeIdentifier equality to `linux-x64` rejects this
Linux x64/glibc runtime because it reports `ubuntu.24.04-x64`. Every filesystem
group therefore fails UnsupportedHost before exercising its promised semantics.
The narrow repair must qualify the portable RID plus explicitly recognized
glibc distro RID(s), while retaining OS/architecture and musl/unknown-host
exclusions. Merely accepting every string ending in x64 or every name without
the substring musl is insufficient ABI qualification. Documentation must name
the actual supported profile, and all filesystem groups must execute under the
same final source snapshot before acceptance.

The supervisor released only the provider writer for this repair; the primary
frontend writer remains held. Re-execution and NativeAOT receipts must reflect
the actual final bytes, not the earlier snapshots.

The filesystem child-launch source handles both direct apphost/NativeAOT launch
and dotnet-hosted DLL launch without reflection. Prior `dotnet run` commands
normally select the apphost, so source inspection alone does not establish that
the dotnet+DLL branch executed. A direct `dotnet EdgSharp.Tests.dll` run (or an
equivalent targeted bounded-child execution) was requested in addition to the
NativeAOT run. The child deadline/kill is a test resource control for special
files, not a general network-filesystem wall-clock guarantee.

Receipt inspection confirms round2 already uses direct
`dotnet tests/EdgSharp.Tests/bin/Release/net10.0/EdgSharp.Tests.dll`; its child
launch executed and failed UnsupportedHost. Thus the final successful direct
DLL run plus NativeAOT run are sufficient; an extra duplicate launch-mode run
is not required. Round2 build/test receipts both record53 snapshot files.

No full M1, complete character encoding/literal repertoire, filesystem-platform
coverage or campaign completion is established by these partial executed
results. The complete PLAN remains active.

The subsequent round3 Release build has zero warnings/errors and the managed
run reports78/79 passing, one failure and zero skips. The repaired host gate
reaches real filesystem operations and the bounded child passes. The sole
failure is an incorrect test expectation: the final ACTIVE/__LINE__ source is
on physical line5, but the assertion expected6. Exact native
`native-m1-session-probe-line-contract-round1.json` accepts and prints `7 5`.
The provider writer was narrowly released for that assertion and handoff only;
production provider bytes remain unchanged. This is an expectation correction,
not a new implementation finding and not another canonical ID. Final matching
79/79 managed and NativeAOT execution is still required.
