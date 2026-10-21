# Floating integration review round one

Status: initial produced source was reviewed during authoring; the supervisor
subsequently reports first-held 164-group JIT/NativeAOT success. Final evidence
audit and the finite FLI-01 long-suffix complement below remain outstanding.
No acceptance is granted by this round report.

## FLI-01 — exponent-like integer UDL routed as malformed floating token

Severity: high diagnostic/lexical correctness; open pending produced repair
and execution. Distinct from isolated foundation FLT-01 (floating cores with a
decimal point).

Observed `Tokens.cs` chooses FloatingLiteral whenever a decimal pp-number
contains e/E. The repaired foundation decoder still correctly refuses a
floating core without a decimal point or exponent digits, so `Lex("1e_tag")`
acquires LEX011 and active analysis routes it to FLT001 InvalidSource. It is an
integer UDL token, with identifier suffix e_tag. `1Efoo` has the same problem.
The current IntegerLiteral fallback also recognizes only underscore-start UDL
suffixes, so changing one classifier branch alone is insufficient.

The first in-flight repair adds an exponent-aware decimal classifier and a
validated generic identifier-suffix fallback. Reinspection found the same
ambiguity remains in its unconditional p/P hexadecimal branch:
`0x1p_tag`/`0x1Pfoo` have integer core 0x1 with a UDL suffix when no dot or
exponent digits occur. This is the same FLI-01, not a separate finding. The
actual upstream no-decimal-point fallback is not restricted to decimal radix.
Supervisor was asked for an exact cooked-ULL native complement. Direct Lex
invalid-separator controls (`1e'1`, `0x1p'1`) must also survive disambiguation;
the earlier digit/sign-only test could otherwise hide their lexical errors by
misrouting them as integer tokens. No repair is marked closed while source is
still active and the held tests have not run.

Actual EDG `lexical.c:12461–12482` records possible_start_of_ud_suffix and jumps
to fixed_point_suffix when exponent digits are absent and decimal_point_seen
is false. With a decimal point it retains the floating core instead. Read the
later potential_ud_suffix/convert_literal_value branches at 12798–12891.

Supervisor executed exact native control
`tools/oracle-inputs/m1-floating-contracts/integer-udl-exponent.cpp`, defining
cooked unsigned-long-long operators for e_tag/Efoo and asserting results 7/9.
Receipt `native-floating-contract-integer-udl-exponent-round1.json` accepts 0,
matched true, under explicit UTF-8 strict C++20/used profile; native warnings
about reserved suffix spelling are retained. Exact copied input SHA is
e4ad0c2010821bd900e1ba06da752e90c6fd1b2b701bfe4ef909e66e6630fc5c.

Acceptance: correct integer-versus-floating token distinction; no false
malformed lexical diagnostic for these valid UDL tokens; active unsupported
literal-operator boundary instead of invented malformed numeric syntax. Retain
malformed signed exponent and separator controls, actual numeric exponents,
hexadecimal exponent requirements, deferred inactive/unused PP validation and
the existing Unicode suffix diagnostic contract. Add direct Lex and active
analysis assertions; literal-operator implementation is not claimed.

## Existing numerical exception read

Read `floating-subnormal-qualification.md` and exact receipts/inputs. Both
below-midpoint literals are proved by an accepted native equality to be
half-minnormal, unlike exact nearest rounding. The six failed expected-accept
explorations remain excluded. Root explicitly authorizes the narrow exact
nearest adaptation; it is neither silently counted as parity nor a reason to
mark supported valid literals Unsupported. The independent foundation reviewer
owns final numeric-module acceptance. Main must retain this qualification and
the actual target payloads through its separate integration gate.

## Subsequent active-source recheck

The author has now repaired the decimal and hexadecimal classifier, retained
apostrophe/signed-exponent diagnostics, removed the unused old float grammar
helper and routed all unknown integer suffixes through Unicode validity and
unsupported-syntax classification. Actual authored tests cover direct Lex
token kinds and errors, active invalid/unsupported results and unused/inactive
PP deferral. FLI-01 is source-repaired; executed closure is still pending.

Read the actual nine frontend groups and coupled source/header/witness. They
cover typed payloads with integer Value null, promotion and crossed ambiguity,
repeated deduction conflict, one actual reference/array cell identity,
operation snapshots, expired temporaries, ordinary const-global float read
rejection versus constexpr success, uninitialized reads, numerical failures,
same-cell mutation, resource recovery and concurrent/repeated-session identity.
The combined gate uses two concrete template instances, frozen two-member
reference overload lookup despite a later overload and caller-local hiding,
macro-origin retention, float array writes and loops in a required context.
The reversed-subscript positive initially addressed one-past a two-element
array; the author corrected its test to three elements and the matching return
element before any execution. This was a test-control correction, not a
production defect or a pass.

Independently compared the accepted 145-file map with main: none missing. Read
changes to the five earlier test files against byte-verified accepted copies
from the condition-header worktree. Existing char16/storage/control checks keep
their meaning through explicit IntegerType casts; the former unsupported 1.5
expectations are replaced by actual supported floating behavior. No existing
semantic assertion was otherwise weakened in those diffs. Shared PP/provider,
Unicode data and tool files remain unchanged at this inspection. Final held
maps and registrations still require a fresh audit after numerical import.

## Final finite FLI-01 complement before round-two hold

The first-held source kept a legacy `suffix.Contains("lL")/Contains("Ll")`
malformed guard before unknown suffix fallback. A three-letter suffix such as
llL therefore remained InvalidSource despite being a valid UDL identifier.
Read actual EDG integer suffix accumulation at lexical.c:12312 onward, the
potential_ud_suffix scan at 12720 onward and restoration at 12826 onward. A
third L becomes extra identifier text and restores the entire suffix; the
two-letter mixed-case builtin path instead reaches literals.c:247's invalid
integer-suffix diagnostic. The exact pinned distinction governs this scope.

Supervisor native `integer-udl-llL.cpp`, cooked ULL operator returning 7 and
assertion `1llL==7`, accepts 0/matched true; exact input SHA
f7c1cf9df30796b42a20f519fdc93edb5dbb68a1b20123cd113d9657be7c0e99.
The proposed `integer-udl-lL.cpp` positive returns 2 for invalid integral
suffix; its original expected-accept receipt is retained **excluded**, not a
matching rejection or reason to force managed acceptance. This refined an
initial grammar-only hypothesis before any policy change. A bounded builtin
mixed-case guard must follow unknown whole-suffix classification, retaining
two-letter invalid controls and three-letter Unsupported controls. The
supervisor narrowly released the author for this source/test repair and will
obtain a fresh hold and complete combined rerun. No numerical foundation change
is requested or authorized by this finding.
