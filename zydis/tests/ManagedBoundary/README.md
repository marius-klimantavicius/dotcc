# Delivered-product managed boundaries

This executable references the delivered `TranslatedZydis` product, including its
owning API and its own emitted runtime. It does not reference the qualification
companion, add C probes, or repeat that companion's ABI inventory.

The project accepts `TranslatedProject` (default: canonical processed product)
and `RootTranslatedLibrary=true`. The latter roots the complete `TranslatedZydis`
assembly for the NativeAOT closure gate. The ordinary sample is a separate,
normally trimmed consumer gate. No packages or test framework are required.

Run from the repository root, substituting the raw product path for the second
form. The parent serializes these builds and supplies per-cell artifact paths.

```bash
dotnet run --project zydis/tests/ManagedBoundary/ManagedBoundary.csproj -c Release \
  -p:TranslatedProject=/home/marius/p/dotcc/zydis/generated/TranslatedZydis/TranslatedZydis.csproj

dotnet publish zydis/tests/ManagedBoundary/ManagedBoundary.csproj -c Release \
  -r linux-x64 --self-contained true -p:PublishAot=true -p:RootTranslatedLibrary=true \
  -p:TranslatedProject=/home/marius/p/dotcc/zydis/generated/TranslatedZydis/TranslatedZydis.csproj \
  --artifacts-path /home/marius/p/dotcc/zydis/build/managed-boundary/processed-aot \
  -o /home/marius/p/dotcc/zydis/build/managed-boundary/processed-aot/published
/home/marius/p/dotcc/zydis/build/managed-boundary/processed-aot/published/Zydis.ManagedBoundary
```

Raw product: `zydis/generated/TranslatedZydis.Raw/TranslatedZydis.csproj`.
The executable takes no arguments, returns 0 on success, 1 on an assertion or
exception, and 2 on invalid usage. Successful output is exactly:

```text
PASS formatter-intel
PASS formatter-att
PASS formatter-status-failure
PASS formatter-contained-exception
PASS owning-snapshots-and-slices
PASS managed-boundary cases=5
```

Assertions:

- Both real formatter callback signatures are registered with the actual
  `ZydisFormatterSetHook` exchange API. Null queries preserve their identities;
  installation returns the default callbacks and restoration returns the exact
  managed callbacks. Saved default callbacks remain callable.
- State has only a normal `GCHandle` root before formatter entry. Compacting
  collections run before entry and inside each callback before state recovery.
  Freshly pinned array addresses match the borrowed operand/output pointers
  after collection. The exact context cookie, instruction pointer, formatter
  pointer, operand pointer, register argument, and 64-bit runtime address survive.
- Intel hooks produce `MOV rax, rbx`; AT&T hooks produce `MOV %rbx, %rax`, with
  exactly one mnemonic and two register callbacks in syntax order. Buffer guards
  remain intact. Hooks restore without reinitializing the formatter. Default
  lowercase formatting works after releasing the context and collecting again.
  The state is collectible after the non-inlined exercise frame returns.
- A callback's exact `ZYAN_STATUS_OUT_OF_RANGE` failure stops downstream register
  callbacks. A deliberately thrown and caught managed exception preserves its
  object identity and becomes `ZYAN_STATUS_INVALID_OPERATION`. These callbacks
  use **managed** function pointers, matching emitted `delegate*` fields; no
  unmanaged entrypoint or native function-pointer conversion is involved. The
  exception mapping is the test host's policy, not a claim that low-level Zydis
  catches arbitrary host exceptions.
- Owning results clear both metadata flag pointers while copying flag values.
  Metadata/union copies and constructor/`Value` encoder-request copies cannot
  mutate the snapshots or their emitted inline operands. Overwriting a sliced
  input and collecting does not change the owned bytes or exact `4883C02A`
  encoding. Successful and short-destination encodes preserve guards around
  borrowed destination slices; failure reports zero written and preserves the
  request for reuse.

Source checks are separate from execution. This implementation worker performs
no managed builds; the parent owns raw/processed JIT and rooted NativeAOT runs.
