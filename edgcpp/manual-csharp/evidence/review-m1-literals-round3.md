# Independent literal/builtin review, round 3 — phase ordering

Reviewer `/root/manual_edg_supervisor/review_m3`, 2026-10-21. Source inspection
only; the implementation is still evolving and no batch acceptance is given.
Canonical M1L-01..07 retain their prior meanings. This round adds M1L-08 and
M1L-09; no older ID is reassigned.

**M1L-08 — High: phase2 raw-spelling exemption uses stale comment context.**
`LogicalInput.Normalize` recognizes raw literals before the actual preprocessor
has established opaque header context. A literal header containing the filename
characters `/*` can leave its independent prepass in blockComment state. A
following actual raw literal is then not recognized by that prepass, and its
backslash-newline is removed. Replacing eager phase3 comment removal with lazy
lexing does not by itself close this separate defect.

Finite source shape, with the configured header present:

```cpp
#define HI __has_include
#if HI(<outer/*unterminated.h>)
#endif
R"(one\
two)"
```

The supervisor has native evidence for the header form; prior accepted raw
controls establish r-char restoration. The required managed control must assert
the complete raw PP token spelling, including backslash and physical newline,
and physical contributors after this header. Ordinary comment/raw controls and
the changed-stream difficult gate must also continue to execute.

Subsequent exact native evidence refines the comparison: the ordinary frontend
value witness accepts sizeof9 with backslash at index3 and newline at index4
(`native-m1-header-then-raw-value-contract-round1.json`). Its preprocessing-only
printer nevertheless emits a collapsed raw spelling (`native-m1-header-then-raw-contract-round1.json`).
Thus preserved managed raw spelling is a deliberate richer query representation;
the actual native value witness proves restoration semantics. The PP printer's
lossy serialization must not be presented as an exact raw-spelling differential.

The general invariant is to preserve phase2 modifications and restore raw
r-char spelling when the actual lexer recognizes that raw token in its real
context. EDG's `conv_single_char` next_orig_line_modif/raw restoration path is
an actual source reference for this distinction. A prepass that predicts a
finite list of macro alias shapes is not general context handling.

Correction to exploratory examples from review discussion: the supervisor's
native receipts reject object `HI=ID(__has_include)` and do not establish
`ID(__has_include)(...)` as a positive language gate (the latter receipt ends
with catastrophic #error status4). Those historical assumed-positive receipts
are excluded. A pasted name `CAT(__has_,include)(<outer//slash.h>)` is instead
native-accepted and supplies the discriminating general expansion/header
scheduling control for M1L-01. Ordinary stringization of comment-looking text
must still perform normal comment replacement outside header context.

**M1L-09 — High: #line bias anchored before trailing multiline trivia.**
The initial streaming `ReadActive` computed physicalLine from the last
significant token's presumed line. In `#line 100 /*a` newline `b*/` newline
`__LINE__`, this anchors the directive at physical line1, while its terminating
logical newline follows physical line2. The next token then incorrectly gets
101 instead of100. `native-m1-line-trailing-multiline-comment-contract-round1.json`
accepts the exact corrected-value witness. Required repair must retain an actual
directive-end/trivia physical anchor; the last significant token is insufficient.
Direct #line output/presumed coordinates and the existing multiline-comment
macro definition grouping must execute together after the lazy lexer change.

No reviewer build, test, compiler/native process, Git operation or production
edit was performed. Actual held code and matching executed evidence remain
necessary for closure. Full PLAN work continues after any bounded gate.
