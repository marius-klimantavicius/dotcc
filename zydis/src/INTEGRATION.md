# Owning API integration contract

The API and ordinary consumer are authored here and under
`samples/ManagedConsumer`; they are linked into `TranslatedZydis`, not copied or
generated. Product class/namespace: `Managed.Zydis.Zydis`.

Recipe requirements (for the recipe owner):

- Add every `src/Api/*.cs` to both raw and processed generated projects using
  `Compile Include` with paths to the original files and an `Authored/...` Link.
  Keep these links after publication and semantic postprocessing.
- Export upstream enumerators with `--export-enum ZYDIS_*` and constants with
  `--emit-define ZYDIS_MAX_*`, `--emit-define ZYDIS_ENCODER_MAX_OPERANDS`,
  `--emit-define ZYDIS_RUNTIME_ADDRESS_NONE` and `--emit-define ZYAN_STATUS_*`.
  Zydis status macros are discovered automatically: do not select the empty
  `ZYDIS_STATUS_H` include guard with a broad status glob. Macro selectors apply during object emission;
  enum selectors apply at link (or use the preprocessing profile's enumExports).
  Public mode/style/mnemonic/register arguments and properties use the actual
  emitted enum types. Counts and statuses use generated upstream constants;
  no handwritten enum mirrors are introduced.
- The consumer has an ordinary `ProjectReference` selected by
  `TranslatedProject`. Its default target is the canonical processed product.
- No extra C unit or native product import is currently required. All instruction
  algorithms remain upstream translated functions.

Ownership and concurrency:

- Decoder, formatter, buffers and requests are local to each synchronous call.
  Caller spans are pinned only for the call. There are no retained pointers,
  callbacks, shared mutable formatter/decoder settings, or disposal obligations.
- Decoded results own their instruction bytes, operand snapshots and flag values.
  The two pointer fields in the emitted instruction metadata are cleared before
  retaining or exposing it; upstream calls receive reconstructed local flag
  pointers while the flag storage remains on the stack.
- Encoder requests are immutable owning snapshots of the actual emitted request;
  callers can copy/edit the emitted struct and wrap it again for advanced fields.
  The generated low-level public surface remains available alongside the API.
- Concurrent calls/results may be shared. A caller concurrently modifying its
  input storage is outside the API contract. No global lock is introduced.

Emitted naming inspected in the full 18-unit product at
`build/campaign/20261027-065728-f4b894b4/raw/TranslatedZydis` and its `raw-build.log`:

- C tags keep their trailing underscores (`ZydisDecodedInstruction_`,
  `ZydisDecodedOperand_`, etc.). Enum fields, public arguments and metadata
  properties use the actual emitted enum types, including enum member defaults.
- `ZyanUSize` is emitted as `ulong` for the selected ABI. Encoder length storage
  and all span-length arguments use that exact type.
- Anonymous operand unions expose `reg`, `mem`, `ptr`, `imm` ref properties.

Pending validation:

- The first raw build's missing-tag diagnostics are addressed in original API
  sources. Nullable annotations are enabled locally, independent of the emitted
  project's nullable setting. Parent should rebuild this actual raw project and
  compile/run the ordinary consumer; this worker runs no .NET builds or translation.
- Parent/tests own the raw/processed JIT/AOT matrix and native differential gates.
  No runtime acceptance is claimed until those checks run on actual output.

Implemented surface:

- `InstructionCodec.Decode`, `TryDecode`, and `DecodeBuffer`; the latter reports
  exact byte offsets on upstream errors. All six upstream machine modes and
  optional generated stack widths are accepted, with natural 16/32/64 defaults.
- `DecodedInstruction` exposes owning bytes/all operands/visible operands,
  actual emitted metadata, copied CPU/FPU flags, Intel/AT&T/MASM formatting,
  encoder conversion, and absolute-address lookup.
- `EncoderRequest` owns the complete emitted request, supports new requests,
  operand/mnemonic/immediate mutation by immutable copies, ordinary/absolute
  encoding, and bounded `TryEncode`. Advanced callers edit a copy of `Value`.
- Stateless NOP filling and mnemonic/register name helpers call upstream.
  Register-context absolute lookup takes the actual emitted context by value
  for the duration of the call.
- Consumer defaults to an asserting workflow and has `disassemble`, `file`,
  `roundtrip`, and `rewrite-immediate` CLI modes over caller-supplied input.
  `samples/ManagedConsumer/README.md` documents the reference and API usage.

Worker checks: read upstream Decode/RewriteCode/EncodeFromScratch examples and
selected headers; inspected every referenced Core symbol against the actual
18-unit emitted sources (all present); checked consumer project XML and ordinary
`$(TranslatedProject)` reference; reviewed fixed/local storage and cleared flag
pointers; whitespace checks. No managed build, translation, runtime execution,
commit, push, or PR was performed by this worker.
