# MsQuic translation performance, 2026-09-26

The main cost was rebuilding the lexical DFA for each input string. The
LALR.CC 4.7.0 `BytesLexer` constructor compiles its rules into a DFA each time;
`BuildLexer()` supplies regex rules, not an already compiled automaton.
Constant macro discovery exercised this repeatedly, including for empty input
streams and both passes used to assign stable anonymous type names.

A CPU trace of `connection.c` attributed approximately 19.4 of 20.9 seconds on
the compiler's main thread to lexer construction. DFA subset construction
accounted for 18.2 seconds. Much of that work also allocated temporary objects
and incurred GC overhead. These are inclusive samples, not separate additive
phase measurements; tracing increased runtime.

`LexerGrammar` now compiles each generated C/Zig grammar once per process with
the same LALR.CC DFA compiler and UTF-8 lowering. Each input gets a separate
stream with its own cursor and lexical state stack. Preprocessing, includes,
macro discovery/overrides, function overrides, and Zig imports use these tables.
The dependency's lexer has no public constructor accepting compiled tables, so
this uses a small internal scanner over its public DFA API. It preserves the
settings dotcc uses: longest match, rule-order precedence, UTF-8 input,
codepoint columns, byte offsets, and exceptions for invalid tokens.

## Measurements

Baseline: compiler source `d2aa7bf`, Release, .NET 10, Linux x64. Input:
MsQuic `80a065112426bce68c1da42d026478d3e40fd45e`, existing managed platform
headers, all 47 selected translation units. Both measurements used four worker
processes, fresh output objects, the same staged inputs and translation options,
and separate compiler builds. No object cache was reused.

| Measurement | Before | After |
| --- | ---: | ---: |
| All 47 object translations, wall time | 198.081 s | 12.740 s |
| Child-process user CPU time | 807.451 s | 46.251 s |
| `connection.c`, within the four-worker run | 22.306 s | 1.505 s |
| Maximum child-process RSS | 214.8 MiB | 182.4 MiB |

Object translation was **15.5× faster** in this run. All 47 object fragments
were byte-for-byte identical before and after; all ten linked C# files were
also byte-for-byte identical.

Linking took about 2–3 seconds. The unchanged postprocessor took 16.271 seconds
on the new linked output; package restore took 0.644 seconds and building the
postprocessed project took 5.286 seconds. Those are separate phase observations,
not an end-to-end qualification benchmark. The postprocessor rewrote 7,894
`Cond.B` calls, simplified 1,045 comparisons, and removed 1,057 empty blocks.

The normal `translate.sh` additionally performs native/JIT/NativeAOT ABI and
consumer checks. Those checks remain enabled. `--fast --jobs 4` performs parallel
object translation without qualification. The speedup above applies to the
compiler work, not to .NET builds, NativeAOT publishing, or runtime tests.

## Validation

- New lexer differential tests compare tokens, positions, byte offsets, and
  errors with LALR.CC's original lexer, including Unicode, longest-match/rule
  priority, state push/pop, nested streams, and concurrent streams.
- Full unit suite: 2,767 passed, including the new lexer tests.
- All 47 fresh MsQuic objects and ten linked C# files match the baseline exactly.
- The isolated postprocessed MsQuic project builds with no warnings or errors.
- The compiler itself publishes with NativeAOT; its `connection.c` output is
  byte-identical to the original JIT compiler's output (0.600 s native run).
- The first full functional run passed 709 tests and skipped 1,159 optional
  cases, with one failure in the literal-pool collectible-assembly unloading
  test. All four unloading cases subsequently passed in isolation with both
  the unchanged baseline compiler and the optimized compiler.
- A complete functional rerun then passed all 710 enabled tests, with 1,159
  optional cases skipped and no failures. No unloading-test or generated-runtime
  changes were made to obtain that result.

Measurements, command arrays, CPU traces, generated comparison outputs and logs
are retained locally under `msquic/artifacts/compiler-performance/` (ignored).
The active MsQuic generated directories and qualification receipts were not
replaced by the measurements.
