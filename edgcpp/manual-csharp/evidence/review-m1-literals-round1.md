# Independent literal/builtin in-flight review, round 1

Date: 2026-10-21. Reviewer: `/root/manual_edg_supervisor/review_m3`.
**No batch acceptance yet.** Actual source/tests were read, but the new batch
has no reviewed executed managed/NativeAOT gate. No implementation edits,
builds/tests/native/compiler processes or Git mutations were performed here.
The prior checkpoint and full continuing PLAN remain as recorded in the baseline.

## Canonical findings

| ID | Severity and exact issue | Source observation / required closure |
| --- | --- | --- |
| M1L-01 | High: macro-produced __has_include initially expanded identifiers inside a literal angle-header operand, unlike a directly lexed HeaderName. Reconstructed angle names also accepted tokens after the first closing greater-than. | Native alias HI(`<h>`) preserves h despite a macro h→missing; extra tokens after first greater-than reject. Current source adds header-operand scanning as soon as its opener is exposed and validates first greater-than is the final operand. Require direct/alias/macro-opener parity, correct search and no activation. |
| M1L-02 | High: unknown escapes with non-ASCII characters bypassed the decoder's encoding qualification, using CLR UTF-16 values as target entities. | Source now diagnoses Unsupported in the unknown-escape branch as well as unescaped non-ASCII input. Direct U-prefixed escaped-non-ASCII assertion exists. Full Unicode/execution encoding remains required work. |
| M1L-03 | High: adjacent character literal ud-suffix was lexed as a separate macro-eligible identifier. E('a'_tag) could expand _tag. | ScanQuoted now collects adjacent identifier suffix in the token, preserving PP spelling and making language UDL semantics explicitly Unsupported. Direct protected-suffix PP assertion exists. Raw string/general UDL semantics are not established by this fix. |
| M1L-04 | High: all PP character operands were immediately represented as signed/unsigned 64-bit values, losing target promotions, widths and rank before mixed arithmetic. | Native confirms u8/u/U'a' > -1, unsigned32 character-only wrap, and invalid ordinary-character shift count32. Actual lexical.c scan_char_constant does not run numeric adjust_pp_int_constant; character operands retain target promotions until usual conversions. Fix needs representation-aware arithmetic, not just an unsigned flag change. Produced repair and discriminating execution still pending. |
| M1L-05 | High integration regression, found/fixed by worker during implementation: new warnings caused template validity/replay completion checks based on diagnostic Count to report failure. | Source uses ErrorCount for validity/completion and preserves Count for checkpoint/truncation. Direct warning template/constexpr instance assertion exists. Independently require warning-only source remains successful and the instance Complete under managed/AOT. |
| M1L-06 | High filesystem search mismatch: a directory at a candidate path was treated as Failure, preventing __has_include from returning false and preventing later valid search roots. | Native directory probe yields missing; actual include falls through a first-root directory to a later header and static-asserts value2. Treat an ordinary nonheader directory candidate as Missing, while preserving actual access/IO/encoding/path failures. Produced repair and execution pending. |
| M1L-07 | High filesystem resource contract: opening a Unix FIFO can block before FileStream.CanSeek is checked. FileAttributes.Device does not identify all Unix special entries. | Root/T3 source investigation confirms .NET10 ordinary read-open lacks O_NONBLOCK and Asynchronous does not add it; Unix attributes omit FIFO/socket type. Post-open CanSeek is not closure. Retain exact runtime source provenance and verify a genuinely bounded regular-file/open policy or explicit unsupported handling before claiming finite input support. |

## Native source/profile facts inspected

`native-m1-character-profile-contract-round1.json` accepts the exact static type/
value witness: wchar_t signed32; char8/16/32 distinct at widths8/16/32; ordinary
single char versus multichar int; unary promotions wide/char8/16→int and
char32→unsigned int; promotion overload choice and first-entity signed packing.
Values include `\377`=-1, `ab`=0x6162, `\377a`=-159, `a\377`=25087 and
`\377\377`=-1 in language and PP contexts.

The five selected malformed controls reject: overlong ordinary, empty, missing
hex digits, hex100 and octal400. **Unknown \q instead accepts with a warning**
and its value equals q in a separate native static assertion. The original
assumed-negative receipt is retained/excluded. This matches actual
`conv_single_char`'s unrecognized-escape branch; do not strengthen it into an
invented error rule.

The subsequent PP promotion receipt confirms all three prefixed comparisons
to negative one are positive, unsigned32 character-only addition wraps, and a
character promoted to int cannot shift by32. The earlier assumed-accept shift
probe is retained/excluded. Those native facts are not managed graph proof.

The boolean-keyword subcase belongs to M1L-04: native `true << 32` and
`(1==1) << 32` both reject for target-int shift count. Replacing true with an
ordinary numeric PP literal accidentally widens it to64; preserve boolean
promotion representation separately from numeric intmax literal collection.

## Positive produced-code observations

The decoder exposes an actual CharacterLiteralExpression with retained encoding,
immutable code-unit collection, target value and distinct canonical type, using
explicit target masks/packing rather than CLR character arithmetic. TypeTable
has distinct WChar/Char8/Char16/Char32 types; special first-fitting promotion
handles char32→uint despite rank equality with int. Overload ranking asks the
actual promotion result, preserving exact-type preference and existing pairwise
selection. New tests inspect literal nodes, conversion ranks, selected callees
and template argument identity, not just acceptance/folding.

Timestamp configuration is immutable, UTC-normalized and defaults to UnixEpoch;
English month names and space-padded day format are deterministic. Hosted mode
is explicit; reserved generic overrides are rejected. Include probing shares
ordered search but caches contents/misses per analysis without creating physical
sources/activations. Actual filesystem presence-only support is a separate
parallel implementation now entering review; this observation does not qualify
filesystem/strict-encoding behavior.

Root subsequently authorized a narrowly scoped handwritten Linux x64 filesystem
host adapter because the BCL-only provider subtask cannot bound FIFO initial
open. This supersedes that local BCL-only workflow restriction. Required closure
for M1L-07 is nonblocking open followed by descriptor-based regular-file
validation, exact ABI/error/ownership review, explicit unsupported hosts and a
bounded child-process FIFO regression. It is IO interop only: no native
frontend/compiler/generator, new native binary or package. Ordinary/network IO
does not acquire a hard wall-clock cancellation guarantee from this adaptation.

All source repairs require stable produced-code review and matching executed
managed/NativeAOT/native evidence. Full source encoding/UCN, strings/floating/
user-defined values, general predefs/operators, filesystem/platform and remaining
M2–M6 inventory are not closed by this round.
