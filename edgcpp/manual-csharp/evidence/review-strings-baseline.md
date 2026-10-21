# Independent string/array baseline review

Reviewer `/root/manual_edg_supervisor/review_m3`, 2026-10-21. Starting after
accepted Unicode integration `6c3848ca5` (112-group baseline). This is source/
design review only, not approval of future code. No implementation edits,
builds/tests, compiler/native execution or Git operations were performed.

## Actual EDG branches inspected

- `lexical.c:2748–2767` explicitly defers ordinary narrow string width/escape
  diagnostics because later concatenation can change the final kind.
- `literals.c:conv_string_literal_chars2204–2322`: scalar versus target-unit
  conversion, strict ordinary nonraw multibyte restriction and final null.
  Raw strings disable escapes and reverse original phase modifications.
- `literals.c:conv_string_literal2325–2525`: prefix/target character kind,
  actual corrected code-unit array size, rescan state and raw restoration.
  A rescan consumes already-converted narrow text with escape processing off.
- `lexical.c:concat_adjacent_string_literals15959–16210`: collect the whole
  adjacent sequence, choose compatible final character kind, allow ordinary
  parts alongside one nonordinary kind, distinguish UTF8 from wide kinds,
  preserve raw-seen and UDL branches without conflating their semantics.
- `literals.c:concat_string_literals2547–2747`: remove only intermediate
  terminators, retain embedded nulls, clear deferred narrow overflow when
  widening, and rescan converted ordinary values into the final kind.
- `literals.c:conv_single_char1698ff`: numeric range errors in a narrow part
  can be deferred, and return_point masks its value to the narrow code unit.
  Whether that truncated value is later widened must follow the pinned oracle,
  not a raw-source decode under the eventual prefix.
- `il.c:string_literal_type12779–12820`: a string is an array of const target
  character elements with a bound including the terminator. Cached type keys
  contain kind/length, never literal AST, source, or TU-owned storage.
- `expr.c:scan_sizeof_operator14773ff,14915ff,15035ff,15251ff`: sizeof has an
  unevaluated expression context, checks the operand, suppresses glvalue loads,
  array decay and function decay, drops references for type-size computation,
  and returns target size_t. Dependence is retained where needed.
- `exprutil.c:conv_array_expr_to_pointer23226ff` and
  `do_array_to_pointer_conversion23333ff`: explicit decay/category changes,
  preserving source and semantic reference information.
- `interpret.c:32137ff,7081ff`: array string glvalues get reusable static
  object identity, per-element initialized storage and target signed extension;
  distinct AST literals are not automatically one canonical semantic object.
- `interpret.c:27927ff,31730–31820`: decay retains base/length; both operand
  orders of builtin subscript are supported. One-past address formation differs
  from a dereference, and real loads retain lifetime/initialization checks.

## Required finite proofs for the produced implementation

1. Prefix, rawness and target type/unit/bound are actual graph facts. Ordinary
   strict multibyte errors differ from raw ordinary and UTF8 literals; UTF16
   supplementary scalars require two units; numeric surrogate code units differ
   from invalid surrogate UCNs. Include empty and embedded-null strings.
2. Concatenation chooses the whole sequence's compatible kind and adds one final
   terminator. Test both orders of ordinary+nonordinary, incompatible prefixes,
   scalar widening and already-processed escaped backslash data. Native-check
   narrow numeric overflow before widening rather than assuming final-prefix
   source decoding is equivalent.
3. String expressions are array lvalues with const element identity. Decay,
   element locations and subsequent loads are explicit. Constant element reads
   use real array storage, not AST-specific integer folds. Bounds include the
   terminator. Runtime/unevaluated out-of-bounds syntax must not become an
   unconditional parser error; required executed invalid accesses must fail.
4. sizeof never evaluates a forbidden read or runtime call, and must not force
   an otherwise unneeded template body merely to obtain a known return type.
   It still checks names/types. Test scalar/type/reference sizes, no array decay,
   null bounds, supplementary byte sizes and semantically invalid operands.
5. Retain each adjacent source part, raw and effective spellings, macro/include
   contributors and replay positions after disposal. No cross-file physical
   range fabrication or presentation retokenization is justified.
6. Any new ArrayType/PointerType must extend recursive dependency/ownership
   guards under references/functions/arrays/pointers. Session interning must
   retain only source-independent structural keys; failed/dependent analyses
   need direct collection/isolation controls. New address values must not widen
   claimed source pointer support silently.
7. Preserve all112 accepted Unicode/reference/storage/ErrorCount/replay tests
   and add a coupled real string/include/template/overload/constexpr storage
   gate with matching native type/unit/value/negative witnesses. The separately
   authored scalar-control-flow contribution has no main ownership; later
   integration must explicitly merge shared evaluator/type hooks.

Actual produced-code/assertion review and matching held JIT/NativeAOT/native/
artifact evidence are mandatory. Full string/array/pointer, general declarator,
control-flow, class/template/lifetime and remaining M1–M6 work is not accepted by
this baseline or by an architecture proposal.
