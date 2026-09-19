# libsmb2 lexer/parser probe

Current result: **53/53 units preprocess, lex, and parse cleanly**, with 53/53
native syntax controls passing. Fresh full-source validation followed include/
header commit `81b3769` and grammar/statement-expression commit `9cac4d1` on
2026-09-19. The compiler now resolves the previously missing includes and accepts
all three reduced grammar cases. This closes the frontend probe only; IR,
emission, managed compilation, and networking require separate validation.

The original findings below are preserved as the pre-implementation baseline.

## Initial baseline

Result: **dotcc cannot yet parse the complete configured library unchanged.**
The Linux x64 probe ran on 2026-09-19 with .NET SDK 10.0.111, against freshly built
dotcc. All 53 upstream compilation units pass native C17 syntax checking.

| Stage | Clean | Completed with include diagnostics | Failed |
| --- | ---: | ---: | ---: |
| dotcc preprocessing, macro expansion, and C token validation | 36 | 17 | 0 |
| dotcc parsing, stopping before IR binding | 30 | 7 | 16 |
| Native `cc -std=c17 -fsyntax-only` | 53 | 0 | 0 |

No lexical/token-validation exception occurred in the source actually reached.
Missing includes prevent a claim that the entire transitive input lexes. The
seven parses with include diagnostics are not passes: dotcc can continue after
an unresolved include and thereby parse incomplete input. The 16 parser failures
include two wrappers whose included C implementation was not resolved.

The 30 clean units include the command codecs (`smb2-cmd-*.c`), portable AES,
MD4/MD5, HMAC-MD5, and several metadata codecs. This validates only each unit's
active configured syntax, not every conditional branch, semantic correctness,
linking, C# emission, or runtime behavior.

## Inputs and profile

- Upstream revision: `99d5cffc85e4aa8d517649568ff8ec2008e35e90`.
- [Pinned source archive](https://codeload.github.com/sahlberg/libsmb2/tar.gz/99d5cffc85e4aa8d517649568ff8ec2008e35e90).
- Archive SHA-256: `e67e8803969336a50b3c7063870376f944a0ab9638cd39629fc8fcd76bff9031`.
- Sources: all 53 entries in upstream CMake's `compile_commands.json`, with
  `ENABLE_LIBKRB5=OFF`, `ENABLE_GSSAPI=OFF`, `ENABLE_LIBDCERPC=OFF`, and
  `ENABLE_EXAMPLES=OFF`. The minimal share-enum DCE/RPC wrappers remain included.
- Configuration: native-generated Linux `config.h` and compile definitions,
  including `_U_=__attribute__((unused))`; explicit `__linux__=1` for dotcc's
  diagnostic platform selection. No host header overlay or source rewrite.
- Includes: the native build's project include directories plus dotcc's embedded
  standard/system headers. Native system headers are used only by native `cc`.

The script checks the archive checksum and compares every extracted source file
against its archive member before probing. Its ignored manifest records the git
revision, SDK, actual compiler/harness assembly hashes, configuration hash,
complete commands, native results, and per-stage totals. The pin is a probe
snapshot, not a claim that it is the latest stable release or the final product pin.

## Confirmed blockers

| Cause | First affected location | Units failing there |
| --- | --- | ---: |
| Standalone anonymous enum definition rejected at its closing semicolon | `lib/sha.h:74` | 7 |
| GNU `__attribute__((unused))` rejected on parameters/local declarations | `_U_` expansions in AES, initialization, NTLMSSP, file-info, sockets, and sync code | 6 |
| GNU statement expression `({ ... })` rejected | `lib/alloc.c:116`, expanding `container_of` defined at line 69 | 1 |
| Included C implementations not resolved, leaving an empty wrapper for parsing | `lib/libsmb2-dcerpc.c`, `lib/libsmb2-dcerpc-srvsvc.c` | 2 |

The enum failures occur in `hmac.c`, `libsmb2.c`, `sha1.c`, `sha224-256.c`,
`sha384-512.c`, `smb2-signing.c`, and `usha.c`. The attribute failures occur in
`aes128ccm.c`, `init.c`, `ntlmssp.c`, `smb2-data-file-info.c`, `socket.c`, and
`sync.c`. These are first failures; later blockers can remain hidden behind them.

Three [small reproducers](../tests/parse-blockers) isolate the grammar failures.
Each passes native `cc -std=c17 -fsyntax-only` and dotcc token validation, then
fails dotcc parsing. The attribute and statement-expression cases are GNU
extensions accepted by that native command, not claims of strict ISO C17 syntax.

Unresolved includes observed across the 17 affected units:

```text
endian.h                 byteswap.h
sys/unistd.h             sys/errno.h
sys/random.h             sys/poll.h
sys/uio.h                netdb.h
sys/ioctl.h
../libdcerpc/dcerpc.c
../libdcerpc/dcerpc-srvsvc.c
```

The last two files exist in the unchanged snapshot. Their diagnostics therefore
indicate an include-resolution gap in this invocation, not an absent upstream
dependency. The other entries are gaps between the native Linux configuration
and dotcc's supplied headers. No declarations were stubbed to conceal them.

## Reproduction and evidence

From any working directory, invoke the script by its path; from the repository:

```sh
python3 libsmb2/scripts/probe-parse.py
```

Prerequisites: Python 3.12+, .NET 10 SDK, CMake, and native `cc`; initial download
and restore require network access. Exit status 1 means the completed probe found
blocked units. This probe does not implement or invoke the planned full
`scripts/translate.sh` pipeline.

The [parse harness](../tests/ParseProbe/Program.cs) reproduces CFrontend's lexer,
source mapping, preprocessor, macro expansion, token validation, dialect/typedef
rewriting, sizeof folding, and source-located parser. It deliberately stops before
`IrBuilder.AddUnit`. It uses the existing `DotCC.Tests` friend-assembly name in an
isolated project/output directory, without changing compiler visibility or loading
the repository's test executable. Future frontend pipeline changes should be
reflected in this diagnostic harness.

Self-controls check that valid syntax with an unresolved function still parses,
invalid grammar fails only parsing, and an invalid token fails lexical validation.
This prevents an emission or name-resolution failure from being mislabeled as a
parser limitation. Every unit exhausts token validation separately before its
parse attempt, so early parser failures do not hide later lexical diagnostics.

Local evidence under `libsmb2/artifacts/parse-probe/`:

- `results.json`: every unit's stage completion, diagnostics, exception, token
  count, and timing; `run.log` contains the console transcript.
- `manifest.json` and `request.json`: exact tool/input/profile identity and commands.
- `*.native.log`: native syntax controls for all units and the reduced cases.
- `reducers.json` and `reducers.log`: the three isolated grammar failures.
- `controls/`: harness positive/negative controls and their results.

Only probe infrastructure, reduced cases, and documentation were added. Compiler,
runtime, upstream C, and generated product sources were not modified. Next work
would address these parser/header/include blockers and rerun the same source
closure before attempting IR lowering or C# generation.
