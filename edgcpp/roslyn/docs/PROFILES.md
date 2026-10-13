# Compatibility profile evidence

Current accepted boundary251:175 distinct files /186 validation instances,
PASS2621 reader assertions,self73,12 strict pristine-pinned profiles.
Contract `c76079ee21b05d278466e87f49f8e8851453b7fe561eb51652e089705e93de71`;
Bounded follow-up:13 strict profiles/four new distinct objects, constructor128,
extern-inline114, native dependent/resolved foreach116 and WORself73 pass.
All older status paragraphs below are milestone history. Remaining required
field/kind proof is tracked in
[CLOSURE.md](CLOSURE.md); optional source positions do not block completion.


The pinned EDG revision is `89e67e07c7f6d0fc38622f9778d8e79f40deaa0f`.
These profiles retain the approved scope; a pending probe is required work.
Compile configuration, resolved values, runtime options and input prerequisites
must all be recorded before a profile is accepted. A successful configure alone
does not demonstrate that an option changed the stock frontend behavior.

The user subsequently instructed: "Ignore C++/CLI support." C++/CLI and C++/CX
implementation and metadata/profile gates are excluded. Native Microsoft C++
extensions and source-semantic native Microsoft ABI remain required.

| Profile | Required behavior | Current evidence | Remaining validation |
|---|---|---|---|
| native | Stock GNU/Linux lowering, partial EH tables, prototypes as metadata | Accepted263 `a409df1be`, lowering1/EH tables1/fullEH0; exact scalar families, UPC dynamic/static and enabled vectors/near-far | Remaining field/kind evidence, unordered EH and alternate host numeric profiles |
| source | Source semantics with native Microsoft ABI | Accepted263 `a409df1be`, source VLA expression evaluation, current MS layout, call/coroutine/init/template/module navigation | Remaining guarded fields and public evidence; optional coordinates are not gates |
| cfront | Stock cfront-like layout with assignment-to-this and folded allocation | Accepted263 `a409df1be`, IA64=0/assignment1/new-fold1 and actual ABI distinctions | Actual USL2.1/3.0 configurations and remaining archaic/virtual-base/control positive evidence |
| exported | Legacy exported templates | Accepted263 `a409df1be`, real C++03 --export markers and runtime prototype/all-template retention1 | Remaining guarded payload/evidence; definition text only where actually retained |

Primary runtime template retention follows `host_envir.c:6895–6900`: both
`prototype_instantiations_in_il` and `all_template_info_in_il` are true. Lowering
skips backend-ignored prototype bodies; those retained trees are metadata rather
than executable C-lowered bodies. A compile-time `ALL_TEMPLATE_INFO_IN_IL` default
alone does not establish an additional retention capability.

The excluded CLI/CX operational boundary was audited before the scope change:
CLI and CX cannot use the general lowered backend: stock `host_envir.h:696–705`
suppresses it. The wrapper detects an invocation that returns without publishing
an object. Source-semantic capture is the planned compatible profile. Stock
`preproc.c:1853–1895` imports core metadata (`mscorlib.dll`, or the configured
core filename); CX also needs `platform.winmd`, `Windows.winmd`, and `vccorlib.h`.
The pristine materialized checkout contains no corresponding metadata assets.
Linux `READ_CPPCLI_PORTABLE_ASSEMBLIES` reads genuine EDG portable metadata rather
than arbitrary .NET PE assemblies (`host_envir.c:6385–6420`). Ordinary PE import
uses the stock Windows metadata implementation. Neither synthetic metadata nor
an upstream semantic bypass is acceptable evidence. No metadata or managed-dialect
implementation work is now required; these observations are historical evidence.

Every object records its compile capabilities and the individual runtime argument
rows. Required representation capabilities distinguish lowered execution from
source-semantic graphs. Optional unavailable original-source facts have explicit
reasons; an unimplemented required exporter payload cannot be relabeled optional.

ABI labels must distinguish cfront-like layout from USL cfront compatibility:
`IA64_ABI=0` alone does not enable `CFRONT_OBJECT_CODE_COMPATIBILITY` or select
the cfront 2.1/3.0 compatibility modes (`host_envir.h:820–838`). Likewise a
Microsoft pointer-to-member sizing option alone is not the complete Microsoft
ABI configuration. Exact resolved compilation and runtime ABI flags remain
required evidence before either profile is accepted.


The b7f52f778 checkpoint validates the stated 232-table slice across all four
profiles; it does not establish whole-profile closure. The 0cae3dd0b checkpoint additionally validates the 243 semantic slice with 750
assertions across 75 genuine snapshot paths. Full profile closure remains open. Maximum native coverage additionally requires resolved guard
and genuine activation probes for UPC, fixed-point, supported vector/host numeric
forms, native COM/IDL attributes and C++26 reflection/token/data-member descriptors.
Current overlays that disable an optional stock feature do not remove that feature
from the approved native scope. MAINTAIN_CLASS_MEMBER_LIST is enabled by the source profile (stock default0);
its genuine ordered member-declaration acceptance belongs to source semantic
identity/order, separately from optional coordinates.


Source location is optional following the user's 2026-10-13 steering. Preserve
direct retained metadata and established physical/presumed distinction without
reconstruction. Profile acceptance prioritizes actual semantic operations/types/
lifetimes/identity; incomplete optional coordinates do not block it.


The active 243 configuration enables stock REFLECTION_ENABLING_POSSIBLE. A genuine
GNU-mode `--set_flag reflection` input publishes nine reflection values and one
retained prospective data-member specification with zero foreign-key failures;
reader acceptance passed in 0cae3dd0b. Source-only RECORD_UNRECOGNIZED_ATTRIBUTES is
enabled and actual shared vendor-attribute descriptor lists are verified. These are
retained semantic identity probes, not optional coordinate reconstruction. Both
resolved capability values and exact runtime arguments accompany refreshed objects.


## Remaining maximum-native configurations

Read-only preprocessing of each actual staged Ninja backend command resolves
UPC_EXTENSIONS_ALLOWED, NEAR_AND_FAR_ALLOWED, GNU_VECTOR_TYPES_ALLOWED,
RISCV_VECTOR_BUILTINS_ENABLED, FIXED_POINT_ALLOWED, host float128 and float16
to1 in all four builds. These need genuine runtime activation, not an additional
convenience overlay. Stock options are --upc/--upc_threads and
--upc_strict/--upc_relaxed; near/far requires --microsoft_16 (ordinary Microsoft
mode explicitly clears that runtime choice). GNU scalable syntax is
__edg_scalable_vector_type__(int,2).

Target-specific predefined SVE/vector-count/RISC-V families additionally require
the corresponding stock ARM64/RISC-V target and version: ARM64 SVE GNU10/Clang10;
SVCount GNU14/Clang17; RISC-V GNU13/Clang14; tuples GNU14/Clang17; bfloat vector
GNU15/Clang19 (sys_predef3150-3193). Separate stock target profiles, preserving
all actual resolved/runtime facts, must activate these where supported.

SUN_EXTENSIONS_ALLOWED, CFRONT_2_1_OBJECT_CODE_COMPATIBILITY,
CFRONT_3_0_OBJECT_CODE_COMPATIBILITY and USE_SOFTFLOAT resolve0 in the four
current builds. Sun needs its stock compile flag plus --sun. USL2.1 and3.0 are
mutually exclusive actual ABI builds, distinct from cfront dialect/IA64=0.
Alternate host numeric storage follows the stock mutually exclusive
USE_FLOAT128/LONG_DOUBLE/DOUBLE choices; SoftFloat3e+ is an external dependency
with all three host selectors disabled. These remain explicit M4 profile gates,
subject only to authoritative upstream platform/configuration limitations.


Source Win64 Microsoft layout needs suffixed stock configuration overrides, not
only target/dialect labels or the unsuffixed legacy flag. The prior Linux-built
source target setter resolved TARG_MICROSOFT_PTR_TO_MEMBER_SIZING_win64 and
TARG_MICROSOFT_BIT_FIELD_ALLOCATION_win64 to0 through BACK_END_IS_CP_GEN_BE.
Verified258 activation uses source-only suffix1 for both and enables
RECORD_BIT_FIELD_CONTAINER_OFFSETS_IN_IL=1. Actual resolved/runtime values and
genuine pointer-member/container-byte assertions pass on the refreshed profile:
ABI=microsoft, both runtime decisions1, seven actual container offsets. Other
profiles retain their own actual ABI decisions.

The258 milestone passes93 genuine paths /1028 reader assertions and explicit
1KiB writer pages. This extends tested family coverage; maximum-native guard
activation and full per-field/union closure remain open.

263 milestone acceptance adds137 genuine objects /1795 reader assertions across
all four current profiles. Dynamic/static UPC, native16 near/far, GNU fixed
vectors, ARM scalable/vector-count/storage-floating and RISC-V fractional/tuple/
boolean vectors now have actual positive reader evidence. Source VLA expression
sites and ordinary unevaluated expressions are distinguished; default/source/
numeric switch cases preserve canonical identities and recorded orders. This
closes these activated families, not whole-profile field completeness. Sun,
actual USL2.1/3.0, alternate host numeric/SoftFloat and remaining active field
branches still require their planned evidence or authoritative limitations.


## Current266 integration boundary

All four strict builds and146 genuine outputs pass native/FK/fingerprint gates.
The reader migration has positive source/Microsoft pilots; full146 acceptance is
in progress. Seven narrow measured reverse-FK indexes and complete retained-scope
preindex/final using-owner reconciliation require one final strict refresh.
Scale before correction:87.933s /7,571,456bytes; after measurement pending. This is
an active integration checkpoint, not completion of configured-surface closure.
Remaining semantic/profile/performance gates are separated in CLOSURE.md.


Final owner-corrected266 boundary is accepted pending commit:146 objects /
PASS1915, self73, current framed fingerprint and all independent native pin gates.
The initial migration failures were resolved by exact list-versus-reverse-parent
navigation, plural attribute owners, nullable fabricated-name projection and
removing the generated constructor's inferred using owner. Seven measured indexes
reduce writer87.933→30.381s, with residual M5 optimization open. Further configured
native profiles and field/kind closure remain required, as CLOSURE.md records.


Current accepted performance267 boundary:10 strict profiles/176 genuine objects,
full reader PASS2092/self73 and18 additional profile pilots PASS196. Sun native
linker-scope modifiers, actual mutually-exclusive USL2.1/3.0, host-long-double,
stock-win64 host-double and official SoftFloat are genuine configurations. USL
export support is disabled by stock signature compatibility; host-double disables
fixed-point because long_Accum requires greater exact precision, not via an
imprecision override. Primary/other profiles retain those capabilities. Remaining
automatic-instantiation/minimal-inlining/inline-variable positive guards are
assigned in build/activation267-plan.tsv. CLI/CX and optional location recovery
remain outside scope; all stock source inputs are pinned and unchanged.


Accepted260 adds stock-valid automatic/inline-variable instantiation and tableless
partial EH:12 strict profiles/182 genuine objects/PASS2462. Four retained variable
flags have real positives; three inlining-only flags are incompatible with user
partial-EH scope because pristine inline.c35–44 requires full portable EH.
Tableless uses metadata initialization pointers and null numeric states, never
inferred cleanup tables/storage. Source positions stay optional.
