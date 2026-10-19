# Source and test inventory

The source pin is in `config/source.json`. `config/translation.json` explicitly
lists all 30 C sources from upstream `lib/common`, `lib/compress`,
`lib/decompress` and `lib/dictBuilder`; preparation compares the directory closure
to that manifest and fails on unlisted/missing units. Those are the standard
library groups selected in upstream `lib/Makefile` and `lib/libzstd.mk`.
`src/Native/zstd_bridge.c` is the 31st, authored boundary/probe unit and is also
translated. No upstream codec source text is changed during staging.

The source groups include FSE/Huffman entropy coding, xxhash checksums, fast,
double-fast, greedy/lazy, binary-tree and optimal compression strategies,
long-distance matching, block splitting, full current-format decompression and
COVER/fastCover/suffix-sort dictionary training. The single-thread configuration
leaves upstream thread/worker implementations inactive without deleting their
units. `ZSTD_NO_INTRINSICS`, `ZSTD_DISABLE_ASM`, BMI2 controls and scalar xxhash
select portable upstream implementations. No `ZSTD_EXCLUDE_*_BLOCK_COMPRESSOR`
macros are selected.

Excluded upstream areas:

| Area | Reason |
| --- | --- |
| `lib/legacy` | Frame decoders through v0.7 are outside the selected current-format product; `ZSTD_LEGACY_SUPPORT=0`. |
| `lib/deprecated` | Optional deprecated ZBUFF wrapper API; upstream `ZSTD_LIB_DEPRECATED` defaults to 0 and CMake `ZSTD_BUILD_DEPRECATED` defaults OFF. Standard supported streaming APIs remain required. |
| Assembly and ISA-specific code paths | Portable C profile uses upstream fallback algorithms. |
| Optional native tracing | `ZSTD_TRACE=0` pins host weak-symbol trace callbacks off identically for native and translated sources; these hooks do not implement codec algorithms. |
| Worker scheduling | `ZSTD_MULTITHREAD` remains undefined; requesting workers must return the upstream unsupported-parameter error. |
| `programs`, `contrib`, examples, benchmarks | Separate command-line/tools/extensions outside the delivered library. The managed consumer is authored separately. |

The native oracle is a custom exact-source control, not the upstream fuzzer. Its
11 named cases cover version, compression strategies, empty frame, long-distance
matching, streaming flush/end, metadata, ordinary errors, single-thread behavior,
trained/prepared dictionaries, raw dictionaries and context reuse/reset. Its
explicit failing CHECK proves assertions remain active with NDEBUG. Native
interop uses the identical core/profile and compares actual frame/dictionary
bytes and decoded payloads.

The `upstream` suite stages exact original bodies of three deterministic
`tests/fuzzer.c` routines plus guarded codec/dictionary/streaming case excerpts:

| Original routine | Assertions retained |
| --- | --- |
| `test_compressBound` | Bound function/macro agreement across powers of two; oversized-input rejection. |
| `test_decompressBound` | Known-size bound; unknown-size streaming bound; more than 4 GiB bound without a 32-bit intermediate overflow, using 32,769 small blocks. |
| `test_setCParams` | Valid compression parameters accepted; invalid windowLog rejected. |
| `fuzzer-compression-roundtrip` | 11 original cases: actual 5 MiB codec corpus, decompression, metadata/error checks and exact decoded-byte comparisons. |
| `fuzzer-training-and-dictionary` | 13 original cases: actual 5 MiB fixture, cyclic/normal training, dictionary IDs, trained/prepared dictionaries and decoded-byte comparisons. |
| `zstream-dictionary-skip-reuse` | Six original cases: actual 10 MiB fixture, streaming, raw dictionary, skippable frame, decoder reuse and decoded-byte comparisons. |

The six groups contain 33 original case labels. The case excerpts retain 54
assertion sites, in addition to assertions in the three complete routines.
The extractor checks fixed per-block case/assertion counts and exact staged-body
uniqueness. It records source line ranges and hashes and retains original
CHECK/display macros and license notices. Authored fixture/cleanup/entrypoint
wrappers supply the exact excerpts' required variables and deterministic corpus
setup. Real upstream `programs/datagen.c` is a test-only translated dependency.
Native execution and an intentional failing original CHECK control validate the
oracle. A supplementary exact native/emitted context-size observation is checked
separately from the 33 original cases; final explicit trace-off results agree
at 582672 bytes in every cell. A separate test-only DotCC library links the complete actual core object
set plus the extracted unit, preserving raw and processed forms; its JIT/AOT
runners execute those actual translated routines. It does not replace or add test
entrypoints to the delivered product. Ordinary managed API/consumer suites use
the published product projects directly.

The unmodified zstream basic-unit driver unconditionally requests two workers,
which conflicts with this single-thread product; its portable cases are extracted
instead. The complete fuzzer basic-unit driver also mixes worker-dependent
training, large allocation tests and specialized parameter/ratio regressions;
selected codec/training/dictionary cases run without those unrelated scopes.
Randomized fuzzing and long stress are outside this bounded deterministic
selection. Shell/CLI play tests target the excluded upstream CLI. These complete
drivers and unselected routines are not counted as executed. The specialist
upstream report lists each selection/exclusion and its concrete reason.
Custom managed tests are reported separately as behavior,
lifecycle, ABI, consumer and interoperability evidence.

Generated distributions preserve original root LICENSE/COPYING plus the exact
initial source notices of xxhash and libdivsufsort. The generated project copies
these into each build/publish output; qualification checks their actual bytes.
