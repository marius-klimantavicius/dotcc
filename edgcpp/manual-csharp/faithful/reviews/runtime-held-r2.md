# Independent exact produced-code runtime review — round 2

Date: 2026-10-21. **HOLD.** Owned outputs are only
`reviews/runtime-held-r2.md` and `reviews/runtime-held-r2.json`.
No EDG/managed body, build, test, native preparation, Git mutation or
subdelegation was executed by this reviewer. Work was limited to static
source/evidence/hash inspection and these two reports. Production, previous
reports, ledgers, tests, tools, upstream, prototype and dirty work are unchanged.

The current worktree PLAN/AGENTS, faithful/AGENTS and explicit
RESUME-20261021 were read. Resume supersedes historical STOP. The entire previous
`runtime-contract-r1.md` was read at SHA-256
`23177d37596938e67c60e43ee8497988d6c9a6e5adbcf139e5c81b62ee57cf73`.
None of its unresolved objections is waived.

## Decision and counting boundary

**Approve the exact four source functions below for supervisor qualification at
the dependency-closed Linux x86_64 LP64 leaf boundary.** Their actual source
bodies, including every operation and branch, agree on native-defined live
storage inputs. No outgoing global, literal, allocator, diagnostic or termination
edge exists in those bodies. Matching representation and held finite execution
evidence support that source comparison; test counts are not its proof.

**The six position counterparts receive only a restricted local-storage and
pure-argument review. Give them zero unrestricted/full-completion count credit.**
Three are source macro adapters and three are actual free operator bodies.
Unresolved original-valid global retention remains a blocker, independently of
the demonstrated local computations. All 77 pending counterparts remain
uncompiled and unaccepted, including source-shaped bodies without direct
outgoing calls. Nothing in this report accepts a pending caller merely because
leaves pass.

The machine report contains 87 counterpart records: 69 original
function/operator/constructor bodies and 18 macro adapters, with **323 branch
records**. Original and managed enclosing ranges/hashes, fine block maps where
present, active/inactive selection, state/alias/lifetime/diagnostic/dependency
decisions and exact domain are recorded. The pending error ledger originally
had narrative branches with whole-routine hashes; this review adds 330 exact
predicate/operation anchor hashes to its 166 branches, together with enclosing
body ranges/hashes for all effects. Anchors are explicitly identified as anchors,
not represented as hashes of an entire multi-statement branch.

No authoritative qualification/count file was edited. The four closed source
functions are the only new count-eligible recommendations. The integer total
remains the supervisor's existing 18; its two restricted mask/signextension
items remain excluded. INIT-001 and full P0–P6 completion remain open.

## Held receipt and provenance

The original `artifacts/foundation-r1/gate-receipt.json` and
`evidence/foundation-first-gate/gate-receipt.json` have identical SHA-256:

`c96f70af2336f5bef4e406c969d79d540a08cbf12ce694ba513ce766f4ecc91e`

It records all writers HOLD, 31 successful serial steps, passing ABI/JIT/AOT/
differential work and equal snapshots. All recorded log digests were independently
rehashed without mismatch. All four owned source manifests match: memory9,
host10, positions8, error8 entries. All 755 source/managed range references
(721 unique ranges) match exact original line-terminated bytes. Every one of the
219 compiler-expanded header records and 822 archived binary/artifact records
was rehashed successfully. The six differential input and raw stdout digests
also match their receipts. These are accountability checks, not executed tests
or semantic acceptance of dependencies.

**Exact final snapshot scope:** both final JSON snapshots contain 80 files,
including pending/error.cs, checking.cs, error.types.cs, mem_manage.cs,
host_envir.runtime.cs, host_envir.shims.cs, basics.positions.cs, pending/README,
all four contracts, and original host/memory c/h files. Their evidence copies
match as well. These pending/contract bytes are hash-frozen but were not
compiled, tested or parity-qualified. Future numberbuffer/region source files
are absent from this snapshot.

`capture_snapshot.py` initially writes 64 files; `run_foundation_gate.py`'s
`snapshot(name)` extends that set and recalculates the manifest. The capture logs
therefore still print 64. An earlier commentary incorrectly inferred exclusions
from the helper script; that inference is withdrawn. The exact final JSON,
its extension code and all 80 actual file hashes were checked.

- Before/after/evidence-copy snapshot file SHA-256:
  `f01215b250df38a8f7b5cdbece82dfb9c2adc08725bc89ab87aea0c006cd75f3`.
- Final canonical 80-file manifest SHA-256:
  `e5d8f857548e0997684188da15fe93ef8654809a944266e7e4df5693ccc5f9f6`.
- Native preparation receipt SHA-256:
  `90097d76b41395289bbfb5e0e5aa3416cdb3b616c0bbe50dc7466da15ce61d04`.

The native preparation opens original host_envir.c/mem_manage.c and includes
original basics.h declarations/operators. Recorded cpfe compilation flags
include C++14, -O3, -flto, signed char, and actual generated definitions.
Test-only function/data sections and isolated default dynamic linking with
--gc-sections are disclosed. Full cpfe's static/gold link and frontend startup
are not reproduced. Native preparation and test sources were opened; there is
no copied semantic native leaf implementation or production EDG fallback.

The independent host/memory macro dumps confirm CHECKING=1 despite NDEBUG,
CHECK_SWITCH_DEFAULT_UNEXPECTED=1, DEBUG=0, TRACK_GENERAL_ALLOCATION=false,
EXPENSIVE_CHECKING=false, MAKE_FRONT_END_CALLABLE=false, EXIT_ON_INTERNAL_ERROR=false,
standalone/driver=false, MULTIBYTE=true, Unicode=1 and native/SJIS/test MBC=false.
FULLY_RESOLVED_MACRO_POSITIONS and RECORD_MACRO_INVOCATIONS are false. Inactive
declarations/initializers and body alternatives stay explicit profile backlog;
none has become an enabled dependency exemption.

## Exact produced leaf bodies and types

Paths in this section's managed column are relative to src/EdgFaithful.

| Original → produced | Complete source decision |
| --- | --- |
| mem_manage.c1693–1699 → mem_manage.leaves.cs12–15, reset_text_buffer | Sole size=0 store. Capacity, increment, payload pointer, payload bytes and aliases unchanged. No allocation/free/checking/global edge. |
| mem_manage.c1782–1792 → mem_manage.leaves.cs18–24, remove_null_terminator_from_text_buffer | size>0 gates final-byte load; NUL gates one unsigned decrement. Both false arms preserve all state. No payload write or loop removing several NULs. |
| host_envir.c2764–2780 → host_envir.cs16–26, smemcmp | cmp=0; unsigned length>0; current-byte loads and independent local postincrements; each unsigned8 cast before signed32 subtraction; nonzero break before decrement/next pair; equal arm decrements; return full difference. Zero length dereferences neither pointer. Same/overlapping ordinary read-only inputs valid. |
| host_envir.c4985–5018 → host_envir.cs29–60, unicode_to_utf8 | Ordered <=7f, <=7ff, <=ffff, else; len assigned before 1/2/3/4 increasing stores. Every shift/mask/OR/truncation matches. Entire ulong64 input domain retained, including surrogates, out-of-range scalar patterns and high bits. No validation or NUL store. |

C# orders smemcmp's independent nonvolatile current-byte reads where C++14
permits either operand first; neither order changes defined observations here.
Invalid storage, volatile/concurrent races or undefined native arithmetic are
not given new portable behavior.

Native/managed measured a_text_buffer size32/align8/offsets0,8,16,24 matches
mem_manage.h652–664 and mem_manage.types.cs14–21. Struct and payload have separate
identity/lifetimes; no managed owner/finalizer/automatic payload rebasing exists.
The native severity's fixed a_byte base matches managed :byte: all twelve
source discriminants retain order0..11; native size1/last11 was measured.
The diagnostic-number typedefs retain signed/unsigned64 aliases, not wrappers.
No pending diagnostic layout or bool-marshalling acceptance follows.

Held JIT/AOT selftests report memory26, host131615, positions12402 each.
Each runtime's differential receipts contain memory2314, host67672,
positions17640 exact observations. Memory compares all payload/guard bytes and
state/alias flags; position copy-ref mutates the actual destination through the
returned reference; host compares exact byte differences and raw encoding bytes.
Tests use original local storage, meaningful alias partitions and mutations.
They do not prove arbitrary runtime/lifetime or every future caller.

The host stream transport rejects lengths exceeding its arrays. Production
smemcmp has no such new guard. Separate native and managed protected-page cases
exercise early mismatch with maximal nominal length and output boundaries.
This bounded evidence does not imply full optimizer/read-ahead universality.

## Position bodies, constants and global objection

Selected native positions are two distinct uint32-seq/ushort-column types;
measured size8/align4/offsets0,4 agree. Original padding is not semantic equality.
All eight constants retain exact types/values; seq0 special columns0/1/2/3 are
not normalized. Header definitions are selected by fe_init.c27's
VAR_INITIALIZERS=1. A use-site VAR_INITIALIZERS=0 dump does not erase the explicit
{0,0}, then {0,2} native initializers.

| Original → produced basics.positions.cs | Restricted body decision |
| --- | --- |
| copy_source_position, basics.h962 →49–54 | Whole value assignment, then ref return to the exact destination. Self-copy and independent values preserved; no detached returned copy. Stable designators and original assignment-lvalue uses require caller audit. |
| set_position_to, h992–993 →59–64 | seq store then column store. Source column expression is evaluated after seq write; eager C# operands are equivalent only when independent. set_position_to(*p,1,p->seq) must be expanded in situ. |
| cmp_source_positions, h1003–1006 →68–74 | seq!= selects exactly one arm; unequal rereads seq, casts each operand to signed64, subtracts; equal reads/casts/subtracts columns. No sign normalization, unsigned subtraction or column-first comparison. Repeated designators must be stable. |
| operator<, h1049–1055 →95–100 | Actual function body: seq< short circuits, else seq== guards column<. Pointers preserve original const-reference referents. |
| operator>, h1058–1064 →103–108 | Separate actual body: seq>, then seq==, then column>; no replacement comparator algorithm. |
| operator==, h1067–1072 →111–115 | seq equality guards column equality. No synthetic !=/Equals/GetHashCode or temporary value model. |

**GLOBAL-RETENTION-001 remains OPEN.** Produced globals at79–90 are mutable static
value fields. Native valid callers may retain either writable global address for
the process lifetime, compare it and mutate/read through it after calls and whole
assignments. SourcePositions only examines pointers inside fixed scopes; native
position oracle defines local objects and never instantiates those globals.
Neither finite field-value evidence nor the prior observed runtime-source
mechanisms closes raw-address lifetime or whole startup in exact JIT/AOT/load
modes. No demonstrated move is asserted; no stable-global acceptance is granted.

Supervisor must pin/prove actual permanent storage/lowering/load-mode behavior or
supply an explicitly permanent source-compatible mapping, with retained raw and
tracked-ref aliases and whole-field replacement visibility. A borrowed-only API,
a copy, per-session storage or prohibition of valid native retention is not a
repair. Apply the same obligation to error_position, diagnostic_counters and
octl. Do not substitute default/memzero for reads of the current writable null
global.

## Exact pending operator binding repair

**POS-BINDING-001, OPEN; original checking/error worker owns the repair on a
supervisor followup. This reviewer makes no implementation edit.**

Native error.c3479 uses tail.spos < pos.spos under the original outer &&;
error.c3485 uses ptr.spos > pos.spos and breaks before updating result.
Actual produced pending/error.cs397 and405 still use those C++ operator tokens.
The held a_simple_source_position defines fields, with no corresponding C#
operator overload. The actual source-shaped bodies are named pointer helpers:

```csharp
// src/EdgFaithful/basics.positions.cs:95–100
public static bool operator_less_than(
    a_simple_source_position* p1, a_simple_source_position* p2);

// src/EdgFaithful/basics.positions.cs:103–108
public static bool operator_greater_than(
    a_simple_source_position* p1, a_simple_source_position* p2);
```

Required call lowering after the real pragma type exists:

```csharp
// pending/error.cs:397, original error.c:3479
if (pragma_diag_tail != null &&
    basics.operator_less_than(&pragma_diag_tail->spos, &pos->spos))

// pending/error.cs:405, original error.c:3485
if (basics.operator_greater_than(&ptr->spos, &pos->spos)) break;
```

Addresses must denote the SAME original inline spos subobjects, with the
original tail fast path, outer short circuit, list order, break-before-update,
next traversal, returned node identity and lifetime. No value overloads,
temporary comparison records or new comparer graph. The actual
a_pragma_diag_elem declaration/constructors/bitfield/union and further severity/
source/type closure remain dependencies. This repair alone gives no pending
caller acceptance.

## Pending memory/host/checking/error frontier

All bodies were compared against their original source ranges. The machine
report records each branch, not just a success-path summary. No pending source
or plan/vector has executed acceptance.

Memory's twelve pending actual functions retain oversize-before-malloc, NULL
catastrophe, distinct NULL-old allocation/reallocation arms, no added realloc
size guard, free(NULL,size) checking before skipping free, separate struct-first
text allocation, unsigned round-up with OLD increment, payload assignment then
increment doubling then capacity, inclusive pointer assertion, ensure before
memcpy before logical-size store, strlen-before-append, and truncate checking.
The five text/allocation macro adapters retain their source-shaped bodies, but
each actual use still needs evaluation/alias review. Zero allocation sizes,
realloc failure/zero lifetime, division by zero, wrapped invalid storage,
same-array bounds/subtraction and overlapping/stale memcpy sources acquire no
new safe or managed-defined behavior.

Host's pending path scan retains signed-byte comparison, MBC traversal,
terminator handling, interior aliases, both live path flags, drive-colon length
guards and original separator selection. **HOST-NULL-REL-001 remains OPEN:**
native host_envir.c809 and managed pending/host_envir.runtime.cs60 compare
last_backslash>last_slash even if the former is NULL and the latter non-NULL.
Qualify selected-compiler/platform behavior; do not add a NULL guard as a fix
or exclude ordinary slash-only paths with backslash mode enabled.

Pending f_mbc_length clears optional err BEFORE first byte read, retains the
Unicode/native split, ASCII1, all 2/3/4-byte recognition predicates and short
circuits, optional error write and continuation-run recovery; native Latin1
returns1. Source overlong/surrogate/out-of-range acceptance must remain. No BCL
validator. It has not run in this gate. set_multibyte_locale sets its flag only
in this selected configuration, but full host_init/reset is absent.

Signoff retains counter value copies, unsigned all-error subtraction, each
reported/error/catastrophe/plural/suppression/conjunction/parenthesis branch,
including source diags_total.errors>0 for suppressed errors. Exact native
assertion origins2437/write_text_signoff and2492/write_signoff are retained.
Filename formatting still uses the shared reusable text buffer and real
encoding/MBC/Small_string<20>/octal closure. It is not a string-format shortcut.
error_text/format-file call ordering and literal purity remain closure work.

Termination retains text before switch; none/remark0, warning0 as a distinct
arm, error2, catastrophe/command-error4; internal/default, including other
enumerators, fflush(live f_error) then abort with ignored flush result.
term_compilation writes signoff first. Source native nonreturn, signals,
buffering, atexit and lifecycle must hold; no CLR exception/unwind policy is
accepted. Inactive callable setjmp/longjmp/status/signal cleanup is separate
unported configuration work.

All six checking macros retain source failure payload roles. Conditional
string operands are lazy natively but eager in method adapters. **MACRO-001
remains a per-call obligation:** pure/stable/valid metadata and strings, deferred
add_char ch, repeated ensure/string/position designators and source expression
results must be checked at actual use. Otherwise lower the original expansion
in place. CHECKING=0 must not evaluate omitted operands. Current error body
expands lazy failure sites; assertion_failed retains BOTH
line_number_buffer.as_temp_characters() occurrences in its append call.

Error allocation/reuse/free retains global-region predicates, next-before-free,
four recursive sublist order, fill-in splice and final diagnostic push; the
general-memory false arm does not gain a free. All fill-in union arms and
initialization writes, including reflection/module and no whole-storage zero,
remain visible. General diagnostics retain all nine nullable fill-ins in source
order and the list/issue split. Creation retains severity override BEFORE
source conversion, original source/TU identity, value copies and ordered lists.

Severity/pragma code retains thresholds, system-header/command-macro guards,
default/pragma choices, global-position fallback, constructor, tail/scan,
push/pop/prev traversal and checking. wrap_up retains cache invalidation,
once/prototype repeated counters, command-macro redirect, active-template pointer
mutation, total before suppression, catastrophic exclusion, local counter even
when suppressed, buffer first-use/reset, recursion/context, output dispatch,
end actions then free. End actions use dp->severity, enabled IL cancellation,
raw listing and source termination order. Real callbacks/callees/types remain
dependencies, not permission for stubs.

**DIAG-REGION-TYPES-001 remains OPEN.** Need actual Small_string<50>/Dyn_array/
allocator/numeric formatting/lifetime; complete generated an_error_code,
message_text/error_codes bitfields and lifecycle; real pragma constructors,
source-file/TU/symbol/type/template/scope/reflection/a_tagged_pointer structures;
unfixed diagnostic enum underlying ABI and full bool/union layout; real
alloc_general_or_in_region/frontend regions, sequence/file conversion, duplicate
tables, contexts/text/SARIF/output-control and IL/listing closure. A general-only
allocator, guessed opaque IL body, reduced error table, replacement formatter,
Unsupported or no-op error path is not acceptable.

**LITERAL-001 remains OPEN.** C markers have no implementation; explicit-NUL
u8 fixed scopes are proposals needing exact payload, static/retained lifetime,
compiler/JIT/AOT storage/pooling/alias and effect-free binding review. Original
filename/line/function data must remain original, not C# caller attributes.
error_text returns pointers that must survive subsequent calls.

**LIBC-001 remains OPEN.** The shim class has no bindings. Preserve same raw
allocator family, exact zero/NULL/failure/lifetime/alignment/errno, real byte
string/memcpy contracts, actual FILE/stderr/f_error/f_raw_listing identity and
stdio return/error/flush ordering. NativeMemory size-one/OOM behavior and
SetLastError=true's errno clearing are not transparent replacements. A small
handwritten fixed-signature libc-only fprintf bridge is a valid route for the
inventoried zero/one/two string and ulong-plus-string(s) shapes after real ABI/
errno/stdio review; no formatter/interpreter or native EDG fallback.

**STARTUP-INIT-001 remains OPEN.** Managed defaults do not implement original
error_early_init thresholds/limit/f_error, error_init and one-time/TU/PCH
registration, or host_init/reset/cleanup and path/locale reset ordering.
No full native startup or retained global pointer was measured. INIT-001
remains separate and unchanged; no source bug correction is authorized.

## Report accountability and handoff

Full per-routine/macro branch/dependency/source/managed hash records, exact
configuration alternatives, constants/types/global decisions, input file hashes,
receipt scope and repair requests are in the companion JSON.

Companion JSON SHA-256:
`d286dc89de77d2f1716a8e75067ea6b763beefc7174205cd2d4d156792126726`

**HOLD.** No outstanding implementation, test, native, Git or delegation action.
Only these two owned review files were written. Supervisor owns qualification,
the original error-worker binding followup and all serialized future gates after
shared writers HOLD. Any later code/configuration/shim/closure bytes require
their own exact independent review; none is preaccepted here.
