# Relocated source identity proof

The CUI recipe uses `--source-root` for source/object emission. The common root
contains upstream `src/`, `include/`, `examples/` and `tests/`, plus campaign
headers staged at `dotcc/include`. Core and gallery keep that same convention;
object-only links consume the emitted identities without a root rewrite.

After an ordinary full translation, re-emit all 58 core units and both gallery
units in a second external directory:

```sh
TMPDIR="$CUI_TEST_TEMP" python3 -B cui/tests/source-identity/run.py \
  --source "$CUI_SOURCE_ROOT" --reference-work "$CUI_TRANSLATION_STAGING" \
  --reference-core cui/generated/TranslatedCui.Raw \
  --reference-gallery cui/generated/TranslatedCuiGallery.Raw \
  --compiler "$DOTCC_COMPILER" --work-root "$CUI_RELOCATION_WORK" --jobs 4
```

The runner compares complete object fragments, complete generated core/gallery
C# and the exported managed-symbol JSON by exact bytes. It also explicitly
compares all full-length unit and anonymous-type identifiers. It removes no
paths or identifiers and fails on any difference. Logs and `evidence.json` stay
under the external work root. This proves generated source identity and byte
stability for the selected CUI inputs, rather than claiming deterministic .NET
binary packaging or Windows execution.

The compiler option remains opt-in elsewhere. Without it, legacy absolute-path
identities remain unchanged. Every included disk header must be beneath the
specified common root; embedded compiler headers use distinct virtual origins.
See [the compiler contract](../../../docs/cli.md#stable-source-identities).
