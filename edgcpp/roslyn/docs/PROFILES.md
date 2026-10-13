# Compatibility profile evidence

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
| native | Stock GNU/Linux lowering, partial exception tables, retained prototypes as metadata | Verified b7f52f778 primary compiler/object/reader checkpoint, lowering=1/EH tables=1/full lowering=0; exact fixed-point/complex/imaginary/float16/float128/bfloat and pointer-member guards | Complete extension/union fields; 243 contextual array/VTT and lifetime acceptance in progress; unordered EH and remaining host/numeric configurations |
| source | Source-semantic capture with native Microsoft ABI | Verified b7f52f778 source compiler/object/reader checkpoint: GNU/native-MS attributes, VLA/statement/member-call/coroutine/initializer traversal, template identity and modules | 243 constructor/lifetime/shared-attribute acceptance and all remaining active semantic fields; optional coordinates are not gates |
| cfront | Stock cfront compatibility including assignment-to-this and allocation folded into constructors | Verified b7f52f778 cfront-like IA64=0 basic/m0/objects/types/numeric checkpoint; overlay assignment=1/new-fold=1 | Genuine archaic operator activation and USL cfront2.1/3.0 configurations, exact virtual-base/control ABI role assertions |
| exported | Legacy exported-template retention | Verified b7f52f778 compiler/object/reader checkpoint and actual C++03 --export input, two retained export-marked templates; runtime prototype/all-template-info flags 1 | All guarded exported template payloads and definition-text availability/profile evidence |

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
profiles; it does not establish whole-profile closure. The current 243 semantic
refresh remains in progress. Maximum native coverage additionally requires resolved guard
and genuine activation probes for UPC, fixed-point, supported vector/host numeric
forms, native COM/IDL attributes and C++26 reflection/token/data-member descriptors.
Current overlays that disable an optional stock feature do not remove that feature
from the approved native scope. MAINTAIN_CLASS_MEMBER_LIST is separately selectable
(default 0) and must be activated before accepting ordered class declarations.


Source location is optional following the user's 2026-10-13 steering. Preserve
direct retained metadata and established physical/presumed distinction without
reconstruction. Profile acceptance prioritizes actual semantic operations/types/
lifetimes/identity; incomplete optional coordinates do not block it.


The active 243 configuration enables stock REFLECTION_ENABLING_POSSIBLE. A genuine
GNU-mode `--set_flag reflection` input publishes nine reflection values and one
retained prospective data-member specification with zero foreign-key failures;
reader acceptance remains pending. Source-only RECORD_UNRECOGNIZED_ATTRIBUTES is
being enabled for actual shared vendor-attribute descriptor lists. These are
retained semantic identity probes, not optional coordinate reconstruction. Both
resolved capability values and exact runtime arguments accompany refreshed objects.
