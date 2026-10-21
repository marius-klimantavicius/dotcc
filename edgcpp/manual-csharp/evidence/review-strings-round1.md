# String/array produced-source review, round 1

Reviewer `/root/manual_edg_supervisor/review_m3`, 2026-10-21. Implementation is
still editing; this is not a held or executed acceptance. No reviewer source
edits, builds/tests, native/compiler runs or Git operations were performed.

## Canonical findings

| ID | Severity | Current observation / acceptance requirement |
| --- | --- | --- |
| STR-01 | High | Actual Concatenate rejects invalid UTF8 bytes when widening an ordinary part. EDG conv_single_wide_char1788ff explicitly falls back to one unsigned original byte in is_rescan mode. Native now confirms wide numeric-byte fallback255, ordinary0x100 truncation0 in both orders, direct-u256, valid UTF8-byte combination233 and preserved escaped slash-u data. Repair the wide/u/U rescan algorithm and execute these exact value/bound controls. UTF8-result rescanning is a separate branch. |
| STR-02 | Medium | New public IsReadOnly bool fabricates actual object mutability for Known=false reference locations whose type is only an access view. Use unknown/null metadata like existing liveness/initialization, with a direct unknown assertion. Known direct const-reference aliases must retain a mutable object's original cell flag=false. |
| STR-03 | High, native-confirmed | Raw token spelling correctly retains physical CRLF, but current value decoder emits CR and LF as two units. EDG lexical read-line strips CR before LF; the pinned native raw control confirms size4, LF10 and following b98. Preserve physical raw spelling/provenance while normalizing decoded target units. Phase2 backslash restoration remains distinct. |
| STR-04 | High if retained; source repair now visible | Omitted bound initially reached Array(element,0) while the factory threw for <=0, before bound inference. Factory now permits the incomplete0 sentinel; explicit zero/nonconstant/negative bounds remain invalid in the parser. Need actual inferred-array/no-initializer/sizeof-incomplete controls and no public Analyze exception. |
| STR-05 | High | ObjectSize currently omits ReferenceType, so the accepted alias form `using R=int&; static_assert(sizeof(R)==4);` gets ARR002. EDG sizeof15035 drops references. Add actual reference-type sizing and contrast expression/pointer/wide/array sizes. |
| STR-06 | Medium | `template<class T> unsigned long f(T x){return sizeof(T);}` reaches ARR002 invalid-source classification. Retain an appropriate dependent size_t recipe, or explicitly mark the still-unimplemented branch Unsupported. A valid unused dependent template must not become invalid C++. |

These IDs concern the read editing source and may close before the first hold;
they are not claims that a validated snapshot failed. High findings were sent
promptly to the supervisor. Exact native probes are supervisor-owned.

## Additional produced-source observations

The new literal node is an ArrayType LValue over const target character elements,
with actual decoded parts/units and one final terminator, rather than an integer
fold. Array/Pointer structural caches key only element identity/bound/qualifiers,
and recursive dependency guards now traverse both compound kinds. This is the
right ownership shape, pending lifetime/collection assertions and execution.

Actual optional narrow width errors are recorded per part and postponed until
the final encoding is known. Rawness remains per source part. Converted ordinary
bytes are rescanned without recognizing escapes again. STR-01 records the
remaining mismatch in malformed-byte fallback, not a need to decode original
source under the final prefix.

The parser has an explicit unevaluatedParsing flag, restores it in finally,
and prevents selected template body instantiation in that context. Sizeof keeps
its original operand/type. Subscript has explicit decay and both builtin operand
orders; runtime bounds must remain evaluator work. Actual storage/pointer module
and all typed failure/lifetime behavior are still under construction.

New owner-mediated SetVariableType updates Declaration.Type and Symbol.Type and
journals their restoration. Require a direct checkpoint test of both links
before accepting this new mutable graph contract. Const/readability/lifetime
flags must describe actual allocated declarations, parameters, materializations
and destination array elements, never merely a const reference's access type.

The native UTF8-result ordinary numeric-byte concat probes abort with a
catastrophic status, while direct u8 numeric FF accepts. Those failures are
retained/excluded and establish no status0/2 language result. Keep this branch
qualified separately; do not count it as a negative pass or silently assume the
wide fallback rule applies to the UTF8 conversion path.

All112 accepted Unicode/reference/storage groups remain the baseline. Actual
held main build/JIT/AOT, exact source/native value/type/negative witnesses,
artifact/source maps and independent final produced-code review are required.
No architecture proposal or native-only design probe supplies that acceptance.
