# Shared compiler repairs qualified by Zstandard

All fixes are shared structural changes. Upstream codec sources and generated C#
are unchanged by hand. Each failure was reduced, compared with native C, fixed
with regressions and followed by a complete selected-source retry.

| Blocker | Shared change | Before/after evidence |
| --- | --- | --- |
| C001 | Preserve dimensions and per-row zero initialization for arrays of structs; retain compatible aggregate-copy initialization. | Original CLI reproducer fails; new native-backed global/local/static/3D fixtures pass. Combined initializer/inline/neighbor unit selection: 24 pass; three execution fixtures pass. |
| C002 | Promote GNU `__inline`/`__inline__` aliases after macro expansion; explicit macros retain precedence. | Four alias cases fail before, macro-precedence control passes; repaired selection and native execution pass. No grammar/table change. |
| C003 | Convert rendered C boolean values through an explicit integer at enum boundaries, including casts, stores, returns and arguments. | Eight focused failures and emitted CS0030 before; 13 focused/neighbor cases and two execution fixtures pass after. Native side-effect count agrees exactly. |
| C004 | Preserve leading/trailing whitespace across eager object/function substitutions independently of token source locations. | Version macro and nested trailing-empty cases fail before; final 91 preprocessing/source-mapping/boundary cases and two execution fixtures pass. Native punctuation, spaces, comments, escaping and nested empty expansion strings agree exactly. |

Exact native and test argv, before/after TRX counts, logs and report hashes:
`artifacts/compiler-regressions/shared-fix-evidence.json`. Committed functional
fixtures include their exact expected stdout. Native controls compile with
`cc -std=c11 -pedantic-errors`; the execution fixtures run the actual emitted C#.

Full-source final retry:

```bash
DOTCC_CAMPAIGN_COMMAND_LOCKS=production bash zstd/scripts/translate.sh --tools build --fetch never --hashes warn --jobs 4
```

`artifacts/campaign/20261019-201437-73006882/receipt.json` passes all 31 selected
units, link, raw/processed semantic builds and both canonical final-path builds.
The actual generated version function returns the exact `1.5.7` string.

Affected production checks completed before the shared-fix milestone commit:

```bash
bash sqlite/scripts/translate.sh --tools reuse --fetch never --hashes warn --jobs 2
bash sqlite/scripts/test.sh --suite consumer --form all --mode jit --tools reuse --fetch never
bash sqlite/scripts/test-api-native.sh
bash sqlite/scripts/test-translated.sh api
bash quickjs/scripts/verify.sh --suite behavior --suite lifecycle --suite upstream --tools reuse --fetch never --form all --mode jit --jobs 2
bash picotls/scripts/verify.sh --suite qualification --tools reuse --fetch never --form all --mode jit --jobs 2
```

All six commands pass. They ran serially with
`DOTCC_CAMPAIGN_COMMAND_LOCKS=production` and explicit current compiler and
postprocessor paths. Commands/tool identities/exit statuses/logs are retained in
`artifacts/downstream/report.json`. SQLite native and translated API transcripts
are identical, including its independent-model transaction corpus. QuickJS
behavior/lifecycle/upstream passes both forms. Picotls's retained qualification
includes ABI, provider/upstream vectors, TLS and independent peer sessions.

This validates the repaired compiler paths and affected production workflows.
Zstandard's complete feature, owning-consumer and NativeAOT acceptance is recorded
separately in [validation.md](validation.md).
