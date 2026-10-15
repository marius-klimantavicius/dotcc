# Bounded normal C++ lifetimes

[The frozen receipt](../acceptance/lifetimes/normal-report.json) passes all 15
expected phase outcomes, including two expected rejections. Production CLI
publication, JIT and NativeAOT use immutable `t5/normal-source-3`. Both execution
modes exactly match the separate native oracle twice: result 26, all nine values
and all 13 ordered events, with zero live objects, prior-run leaks or overflow.
The generated dependencies are only Checks, Generated and the separate runtime.

All 16 required definitions and one record are represented. The selected void
copy-result routine receives its recorded result pointer; constructor/destructor
receivers and const references use the existing natural pointer operands once.
No hidden argument is added. Complete and subobject entry bodies remain separate;
the three subobject variants are represented and C#-compiled, but this fixture
does not activate them at runtime. Single-unit CLI admission preserves actual
COMDAT/ABI-entry metadata without GNU alias resolution or identity substitution.

The normal adapter validates the recorded noexcept body/callees, simple direct
local cleanup actions, matching metadata recipes, state/region identity and
actual null state. Normal destruction executes only at its retained explicit
call occurrences, including early return and the full-expression comma. The
unused compiler table has an actual cleanup projection and empty static
descriptors; descriptor presence is not executable initialization. No metadata
destructor list is replayed and no lexical cleanup is reconstructed.

The published source/project repeat byte-identically; only the output directory
is normalized when comparing program reports. Completion hashes validate. JIT
build takes 5.70 seconds and NativeAOT publish 9.16 seconds, within frozen
600-second/8-GiB budgets. Receipts retain sampled process-tree RSS and waited
high-water RSS separately; tiny execution samples are not startup peak claims.

The genuine exceptional fixture still fails before publication. Weak CLI policy
and the existing ProgramAssembler weak/alias/ifunc checks pass. An earlier test
mistakenly expected rejection of an unused alias declaration: its required=false
classification and successful publication of only its target are preserved,
without claiming alias execution. The missing-resource snapshot and preliminary
table-guard attempts are also retained.

T5 remains open. Next is the [bounded integer EH contract](T5-EH-CONTRACT.md),
including destination search before cleanup and handler-specific payload ownership.
Mutable/rvalue references, native C++ lifetime boundaries, cross-unit lifetime
adaptation, constructor failure/partial arrays, rethrow/termination, inheritance,
RTTI/member pointers and initialization guards/TLS require separate evidence.
