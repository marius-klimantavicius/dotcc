# Independent baseline review — 2026-10-20

Reviewer scope: read-only inspection of the campaign contracts and EDG sources;
write access only to `evidence/review-*.md`. No builds, tests, native compiler
processes, Git mutations, implementation edits or generated implementation input
were used in this review.

Status: **design review only; no milestone approved**. At inspection time no
`src/EdgSharp/` or `tests/EdgSharp.Tests/` implementation files existed. Produced
code and executed validation must receive a later independent review.

## Contract assessment

The manual campaign explicitly supersedes the parent's exporter-only semantic
authority restriction for this directory. `PLAN.md`, `PROGRESS.md` and
`docs/DESIGN.md` require an interleaved parser/semantic analyzer, session-owned
identity, explicit LP64 target semantics, separate invalid/unsupported results,
retained facts, lexical replay and eventual template rescanning. This is an
appropriate contract for the initial slice. `docs/DESIGN.md` correctly limits
cursor replay: semantic/speculative rollback is an additional obligation when
introduced. `SUPPORT.md` does not yet claim implemented support.

No current implementation defect is asserted below. These are concrete review
risks and acceptance probes, with severity **if the implementation exhibits the
described failure**. A future test that exercises unimplemented syntax may
legitimately report Unsupported; it must not return successful invented facts.

## Required review probes

1. **High — canonical symbols must survive redeclarations.** EDG
   `decls.c:function_declaration` (18210 onward) processes declaration/definition
   together with existing symbols/types; `types.c:param_types_are_compatible_full`
   (8558 onward) separates compatible signatures from identity. For
   `int f(int); int g(){return f(1);} int f(int x){return x;}`, the reference in
   `g` must still designate the canonical `f` after its definition is seen.
   Preserve each declaration's range and parameters separately from canonical
   entity identity. Verify `int f(int); long f(int);` and duplicate definitions
   diagnose invalid input. Parameter-name changes must not change function type.

2. **High — do not let a function body's root block hide its parameters.**
   `statements.c:compound_statement_full` (8193, 8245 onward) treats a function
   body differently from a nested block; `symbol_tbl.c:
   symbols_may_coexist_in_curr_scope` (4804 onward) applies dialect-specific
   parameter hiding checks. `int f(int x){int x=1;return x;}` must diagnose;
   `int f(int x){{int x=1;}return x;}` must resolve the final `x` to the parameter.
   Duplicate named parameters must diagnose. Ordinary local declarations also
   need source-order visibility: `int f(){return x;int x=1;}` cannot retroactively
   resolve the earlier reference.

3. **High — aliases are declarations, not new arithmetic type identities.**
   `decls.c:alias_declaration` (17293–17411) uses `decl_typedef` while preserving
   alias/source information; `types.c:skip_typerefs*` (296 onward) and conversion
   routines deliberately inspect underlying types. In
   `using I=int; I f(I); int f(int x){return x;}`, prototype and definition must
   match, while the alias remains queryable. A local value named `I` must hide
   an outer alias for later declaration disambiguation. `using I=int; using
   I=long;` must diagnose; repeat aliases to the same type need the proper
   compatibility rule or an explicit documented unsupported restriction.

4. **High — lookup ordering must remain explicit.** EDG
   `symbol_tbl.c:link_symbol_into_symbol_table` (5483 onward) keeps the active
   list ordered innermost-to-outermost (5555 onward), distinguishes inactive
   lists whose order is insignificant (5548, 5682 onward), and keeps symbols
   in source order within scope (`add_symbol_to_scope_list`, 6111 onward).
   `lookup.c:normal_id_lookup` (4279 onward) has explicit lookup modes and may
   trigger instantiation. A scope dictionary is a bounded adaptation for the
   initial lexical subset, not a port of namespace/class lookup. Verify scope
   exit restores the prior binding; a local scalar `f` must stop lookup before
   an outer function `f`, so `int f(); int g(){int f=0;return f();}` diagnoses.
   Candidate sets must not later depend on hash iteration order.

5. **High — public graph access must not expose mutable backing storage.**
   The contract promises reference identity and retained reliable facts.
   `IReadOnlyList<T>` backed by a publicly returned `T[]` or `List<T>` remains
   mutable by downcast. Constructors retaining caller arrays permit mutation
   without a cast. Review every collection, node setter and options/target
   boundary. Acceptance: public APIs cannot alter canonical types, declaration
   lists, parameters, arguments, diagnostics or token storage after analysis;
   arrays passed into public constructors are copied or wrapped with an
   explicitly safe ownership contract. Two independent analyses of identical
   text must not share mutable identity, even within the same session if reuse
   is supported. A later analysis must not invalidate an earlier returned graph.

6. **High — token replay is not semantic transaction rollback.**
   `lexical.c:rescan_cached_tokens` (4462 onward) preserves or discards the
   current token explicitly; `rescan_reusable_cache` (4543 onward) nests reusable
   caches while retaining the previous rescan stack. `push_lexical_state_stack`
   (19228 onward) saves error position, caching and declaration-start state and
   pushes stop-token state; pop (19254 onward) restores them. `next_token_full`
   (~19960) restores the error position even for lookahead. For a plain array
   cursor, require nested checkpoints, stable EOF, repeated restores and
   rejecting checkpoints from another cursor/buffer. When speculative parsing
   arrives, require restoration of diagnostics, scopes/symbol insertion,
   pending work and relevant semantic flags. Do not advertise the cursor-only
   helper as satisfying the later parser rollback gate.

7. **High — LP64 widths alone do not define integer semantics.**
   `types.c:type_after_integral_promotion` (6114–6252) handles bool/small integral
   promotion before arithmetic; `exprutil.c:
   determine_arithmetic_conversions_full` (~11760–12049) chooses signed/unsigned
   result types using rank and target representability. LP64 `long` and
   `long long` are distinct, same-width types of different rank. Inspect
   `unsigned int + long` (long), `unsigned long + long long` (unsigned long
   long), and bool + bool (int), as those forms become supported. Conversions
   must be represented on operands, including lvalue-to-rvalue, rather than
   merely assigning a plausible result type. Top-level cv handling for by-value
   function signatures must not erase the definition parameter's own cv state.

8. **High — integer literal radix/suffix and constant folding are separate
   decisions.** `lexical.c:scan_number` (12038–13130) scans a full pp-number,
   diagnoses malformed extra text (~12730 onward), then dispatches conversion;
   `literals.c:conv_integer_literal` (130–809) selects kinds by radix, suffix and
   target limits. On LP64, `2147483648` is long and `0xffffffff` is unsigned int.
   Decimal `4294967295` differs in type from that hexadecimal literal. Unary
   minus is separate from literal scanning. Check `08`, `0x`, `1e+`, malformed
   suffixes and oversized values. `const_ints.c:add_integer_values` (711 onward)
   explicitly reports overflow; unbounded BigInteger alone is insufficient.
   Unsigned wrapping and signed overflow need different fold outcomes; a valid
   runtime expression with signed overflow must not become a valid constant
   value in the API merely because host or BigInteger arithmetic succeeds.

9. **High — unsupported syntax must never produce success-shaped graph facts.**
   `statements.c:statement` (~7846 onward) distinguishes statement forms and
   declarations; `return_statement` (6963 onward) inspects the current function
   and return type. Recovery must always progress and preserve a failure result.
   Probe unsupported syntax inside a supported function, nested braces, missing
   `;`/`)`/`}`, a call on a scalar, unresolved identifier, void object/parameter,
   and `return;` from a scalar function. Do not skip a failed body and mark its
   declaration complete. Unsupported source and invalid supported source must
   remain distinguishable; not every parser failure is invalid C++.

10. **Medium — lexical and preprocessing omissions must remain visible.**
    `lexical.c:get_token` (17382 onward) handles identifiers, punctuation,
    comments, literals and language modes; scanning numbers does not by itself
    implement preprocessing. `preproc.c` is an independent subsystem. Deferred
    macros, includes, conditional directives, line splicing, string/character
    forms, universal characters and alternative tokens need exact supported
    boundaries. Lexical acceptance requires token kind/spelling/physical source
    ranges, comment handling, malformed EOF and replay tests. If unsupported
    preprocessing would change token interpretation, reject it visibly instead
    of interpreting a different program. M1 must remain partial until its stated
    preprocessing acceptance cases are actually implemented and compared.

11. **High — correspondence requires algorithms and invariant evidence.**
    Routine-name comments alone do not establish a manual EDG port. Later review
    will check how declaration entry, source-order lookup, parse-time binding,
    conversion construction and error recovery interleave in the actual code.
    Record the selected EDG branches and omissions per authored routine; do not
    claim entire source files. Production dependencies must contain no native
    fallback, compiler process, generated EDG assembly/source input, fixture
    answer path, dynamic emission or reflection-only serializer requirement.
    AOT smoke evidence is separate from a compatible-looking project file.

## Verification evidence required before acceptance

- Source-text tests inspect identities, selected declarations/callees, types,
  categories, inserted conversions, ranges and structured body relationships.
- Unit tests cover collection ownership, ordered scope behavior, cursor replay,
  independent sessions and explicit target-width/rank behavior.
- Negative/malformed/unsupported tests include embedded syntax and a termination
  guard; test inventory records passes, failures and skips.
- Native EDG receipts use identical source bytes and a checked C++20/LP64
  profile; acceptance alone is not a proof of graph facts. Static assertions or
  another reviewed discriminating native witness should establish selected
  facts. Record which exact facts each native witness proves.
- Default managed tests run without native EDG. Native oracle use is explicit
  and confined to test tooling. Production dependency inspection and executed
  NativeAOT smoke remain required before an AOT support claim.
- Every EDG-derived authored source has applicable SPDX attribution. Supervisor
  ledgers record exact consulted source hashes and license/provenance bounds.

## Consulted source hashes

SHA-256 values are of the current working files, not an assertion that the
imported base commit has pristine content.

| File under `../upstream/src/` | SHA-256 |
| --- | --- |
| `lexical.c` | `e987ffcb7645931564f3a6f262d089c21524e78ce2c60d5e0e0868a5b4b98875` |
| `lexical.h` | `805ade6649e8b2e0d2d11c2eaf8364904e51d54bbaa3701d2011f8609e74e8b3` |
| `preproc.c` | `e0927cba0b5ba139152d69063a6ec73114e103be957effd90eec4616438a65af` |
| `decls.c` | `1b362f931dd532b477c272d4cc4481baa822660ffb278c00475ae2da51b8fdc0` |
| `types.c` | `89e6393ab4109ef3924eb25ca8fe04503e87db8fcd053be559bac52713ba11fc` |
| `symbol_tbl.c` | `ef2fc5af96a478fc1d6cf02927de2845c4f61814e6ddb084d36455baed2c7eff` |
| `scope_stk.c` | `8f781d2ded6e32ec02943ab98a9ffbb1ae800812b8d6eada6484c698c7b682e4` |
| `lookup.c` | `251e21f04cc94b0e8da1c5e1031ca3f832aaf5ec2b19bd6ff4ece4116e543af2` |
| `expr.c` | `056f8e7416d6f948846abbe17c3401e9eb4ca8ed1ce0ae7673cd573b314df3c2` |
| `exprutil.c` | `1b01609700d2a72f64f94e218a3f210e7e9503394bc3cf16168e8a1a50e50812` |
| `statements.c` | `a4b9476b67189ea0ed2034239f4494bc32a4f50a885bb258ab01eb8e01bac703` |
| `literals.c` | `de5276f87f073a67bb4b7c6a4cd152a66eab0a155fda84f0d991e3d296213d95` |
| `const_ints.c` | `5a16a2ebd362068f1757cc30f5b4828d67cc1bc16a8e6bfc519e43c0059deb2c` |
| `error.c` | `e9a2fee3c39c8958361236285b57a87a98ceb02592a1de5e1035bd65e7bf3a60` |
