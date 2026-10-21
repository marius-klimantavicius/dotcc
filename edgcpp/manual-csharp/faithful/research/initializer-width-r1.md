# Initializer width research, r1 — final HOLD

Authority: current PLAN.md, AGENTS.md, coordination/RESUME-20261021.md;
explicit resume supersedes historical STOP. Read p0-contract-r1.md, INIT-001,
NATIVE-PROFILE.md, and the cpfe baseline compile entry. No builds, tests, native
preparation, Git commands, upstream edits, semantic implementation, or
subdelegation. This is a bounded source investigation, not parity acceptance.
The initial snapshot entered temporary write-HOLD for serialized validation.
Supervisor subsequently released ownership, supplied fresh ABI/leaf receipts
and an isolated sanitizer failure, and authorized this final update. No
production C# changed. The measured evidence appendix supersedes initial
pending-evidence statements; full startup remains unqualified.

## Actual cpfe path and width writes

`cfe.c:cfe_main` sets f_error using default_error_output_file before early
initialization; then fe_early_init, get_timer, proc_command_line,
fe_one_time_init, trans_unit_init, process_translation_unit. The primary
translation unit calls fe_init_part_1 (trans_unit.c:800).

`fe_early_init` (fe_init.c:1752–1793) calls, in order, the enabled
check_int_type_config, mem_manage_early_init, host_envir_early_init,
target_early_init, cmd_line_early_init, error_early_init, and further subsystem
initializers. `target_early_init` first selects the configured default or
NO_TARGET_CONFIG, calls set_target_configuration, initializes other target
globals, and finally assigns bitint_maxwidth_value = DEFAULT_BITINT_MAXWIDTH_VALUE
(target.c:1029). A whole upstream/src identifier search finds no other assignment
to that global. target.h:194–196 declares it as a_targ_size_t.

`proc_command_line` is between early and one-time initialization.
`optk_target` (cmd_line.c:12180–12196) looks up the target and calls
set_target_configuration, or str_command_line_error for an unknown target.
Repeated target options use the last setting. set_target_configuration invokes
the selected target_map.h-based setter and constructs auxiliary_info_dir_name;
the target map contains no bitint_maxwidth_value/DEFAULT_BITINT_MAXWIDTH_VALUE
mapping. There is no width command-line option/assignment in the inspected tree.

`fe_one_time_init` calls target_one_time_init before host_envir_one_time_init
and other subsystem initializers. target_one_time_init initializes character
sizes, conditionally field alignments, selects targ_sizeof_largest_integer
(int128 for this profile), and invokes check_target_configuration under CHECKING.
It does not rewrite bitint_maxwidth_value. The checking routine tests the
ordinary/fixed-point representation sizes; it contains no bitint width check.

`fe_init_part_1` initializes error/folding/interpreter/memory/host/IL/lexical/
symbol/scope/declaration/type and other state, then calls preproc_init,
target_init, const_ints_init, float_pt_init in that order. target_init sets the
plain-char kind and live int_kind_is_signed[ik_char], sharing/intmax/object-offset
state, Microsoft sized kinds and is_64bit_target. It does not rewrite bitint
width. The standalone-utility early/late routines are different entry paths and
are not used as evidence for cpfe initialization.

## Selected configuration, not an oracle-target mixture

Use the cpfe const_ints.c entry from build/compile_commands.json (and the matching
target.c/fe_init.c/cfe.c entries for full startup). Recorded host compile is
/usr/bin/c++, C++14 via -x c++, -O3 -DNDEBUG -flto -fno-exceptions
-fno-unwind-tables -fno-rtti, USE_CMAKE_DEFINES, build/src/includes, with
BACK_END_IS_C_GEN_BE=1, COMPILE_MULTIPLE_TRANSLATION_UNITS=1, DO_IL_LOWERING=1,
LOWER_FIXED_POINT=1, LOWER_IFUNC=1, NEED_IL_DISPLAY=1,
RECORD_BACKING_EXPRS_WITH_IL_LOWERING=1. No bitint maximum override is present.
The cpfe-dot-oracle-specific DO_UNORDERED_EH_PROCESSING=0 and variant
pointer-to-member definition must not be imported.

The generated definitions select multipart integers, int128 and long-long,
fixed point, CHECKING=1 and DEBUG=0. targ_def.h defaults
DEFAULT_BITINT_MAXWIDTH_VALUE=65535. Eight ushort limbs provide 128 bits.
Legacy configuration name is linux_x86_64; named additional targets are
linux_i686, win64, win32, linux_aarch64, linux_armv7, linux_riscv64,
linux_riscv32. DEFAULT_TARGET_CONFIGURATION_NAME is absent from the inspected
generated header/command. Explicit --target linux_x86_64 selects legacy table
entry 0 with a named auxiliary directory; implicit NO_TARGET_CONFIG uses the
same target values with the unsuffixed auxiliary directory. This distinction
must remain intact. Generated definitions are textual evidence; supervisor owns
the exact effective macro/ABI and executable provenance receipts.

## Exact branch arithmetic and limits of the finding

const_ints_init iterates every integer kind from ik_char to ik_last exclusive.
The il_def.h enum and signedness table include ik_bit_precise (signed), followed
by ik_unsigned_bit_precise; no strict-mode or extension predicate removes them.
init_int_kind_min_max_values branches on is_bit_precise_kind: both kinds call
get_bit_precise_integer_size_and_alignment(bitint_maxwidth_value), the others
get_integer_size_and_alignment. The bit-precise helper asserts width != 0,
computes (width + targ_char_bit - 1) / targ_char_bit, then selects alignment
through the size thresholds (short/int/long/long-long/int128, else largest).
At width 65535 and 8-bit bytes, size=8192; storage bit_size=65536.

For the signed kind, bit_size-- gives 65535; for unsigned it stays 65536.
Both call make_integer_value_mask. Signed then sets one, copies maximum to
minimum, adds one unsigned (err is ignored), and sign extends at bit_size+1
=65536. Unsigned sets minimum to zero. Those are source branches, not proposals
to adjust the maximum or disable either kind.

make_integer_value_mask first writes zero, complements all limbs, and calls
shift_right_integer_value with `(int)(BITS_IN_AN_INTEGER_VALUE - bits)`.
bits is size_t: subtraction wraps in unsigned size_t (defined); narrowing an
out-of-range size_t result to int is implementation-defined in this C++14
configuration, not itself undefined behavior. On the recorded usual GCC
low-bit conversion the two counts are -65407 and -65408 respectively.
The supervisor subsequently measured exactly these conversions; see the
evidence appendix. Narrowing remains implementation-defined in C++14.

The multipart right-shift computes op_2/16, op_2%16, and 16-remainder. C++14
division truncates toward zero. Thus:

| Caller | count after GCC narrowing | part_offset | first shift | second shift |
| --- | ---: | ---: | ---: | ---: |
| signed maximum, bits=65535 | -65407 | -4087 | -15 | 31 |
| unsigned maximum, bits=65536 | -65408 | -4088 | 0 | 16 |
| signed extension, bits=65536 | -65408 | -4088 | 0 | 16 |

For signed maximum, the expression `get_part(...) >> first_part_shift` has
negative shift count -15: undefined behavior if executed, even when get_part
returns zero. get_part's out-of-range-index guard protects the dereference; it
does not protect the enclosing shift. The supervisor subsequently reproduced
this negative shift in the original arithmetic callee. The research child ran
no compiler/frontend/probe. Full startup may encounter another earlier
condition; isolated reproduction does not establish actual startup arrival.

The unsigned mask and signed extension counts are negative overall but have
remainders zero, and their executed limb shifts are 0 and 16 in 64-bit unsigned
work values. They must not be described as necessarily negative-shift UB merely
because the outer count is negative. Their results remain unqualified and the
documented legal-shift caller contract is not established by this observation.
Subsequent supervisor runs returned zero limbs without a sanitizer diagnostic
in these two isolated modes; that does not qualify negative outer counts in
general.

For ordinary initialized kinds, storage widths 8/16/32/64/128 and the signed
width-minus-one masks give nonnegative counts within the representation.
For any reviewed mask call with 1 <= bits <= 128, component shifts are 0..16
(well below the selected 64-bit work width); sign extension has the same
component property on 1..128. Other negative counts, extreme int arithmetic and
all other callers need separate domain review. Native signed large-value
conversions and arithmetic are not granted blanket wrap semantics.

Some newer type-range helpers already contain source-defined representation
bounds (const_ints.c:integer_value_range_for_type clamps only its local bit_size;
trim_integer_value_to_type returns for bit_size >= representation width).
Those branches do not constrain init_int_kind_min_max_values and must not be
invented at its call site. Preserve current upstream bytes, including these
local changes; assume neither a pristine import nor a native bug fix permission.

## Supervisor instrumentation path

initializer-width-probe.cpp is test-only and deliberately does not initialize a
frontend. Default mode prints the header-derived arithmetic only; independent
modes call original make_integer_value_mask for 127/128/65535/65536 bits, or
original sign_extend_integer_value at 65536. One suspect operation per process
allows halt-on-error UBSan to retain the first failure. No source routine is
copied, no failure stub is supplied, and no production code is changed.

Supervisor may reuse prepare_integer_oracle.py's exact cpfe compile-entry
extraction and original const_ints.c object with function/data sections and
--gc-sections, substituting this wrapper. Header include search must resolve
fe_common.h to the pinned upstream source. Instrument BOTH original object and
wrapper with the same chosen sanitizer flags; record every deviation from
Release/LTO and keep original failures/output. Probe calls resolve just mask,
set/complement, shifts (and sign helper), avoiding full startup's assertions and
target globals. This is a minimum arithmetic reproduction, not equivalent full
initialization. Do not link the existing wrapper's main as well.

To prove actual startup arrival, separately rebuild/instrument the unchanged
cpfe source path under its own profile receipt, and observe target_early_init
exit, command-line processing exit, target_one_time_init exit, target_init exit,
const_ints_init entry, then each init_int_kind_min_max_values kind and the first
shift count. Record bitint_maxwidth_value, target index, targ_char_bit,
representation width, and kind. A source breakpoint/debugger watchpoint or a
test-only instrumented COPY may observe these points; do not mutate upstream.
Full startup needs its normal runtime; substituting target state in the leaf
probe is not a substitute. INIT-001 stays OPEN, with no clamp, skip or bug label.

The temporary write-HOLD was observed until explicit supervisor release. The
final measured evidence, current-tool manifest and final HOLD follow below.

## Initially inspected source snapshot (historical tool bytes)

SHA-256, exact whole files; source review only, not execution or full routine acceptance.

```text
568c80d00f1887a37a076c35bd8123fd1de1910cbecf73f18151d25bfb86d609  edgcpp/build/compile_commands.json
35c763753d20fe99c59649eb51aafccec7d7d61b6c227824d52d362fcc9e111f  edgcpp/build/src/includes/cmake_defines.h
7457e4f82e18fb9b1fe543d1420d56e7748ed2393583503316e9a39e4980d264  edgcpp/manual-csharp/faithful/tools/prepare_integer_oracle.py
65b5752f8f2e0e1eda6dccb59996bf996452df53b8ebd071dab5cb397918b0e9  edgcpp/upstream/src/basics.h
85ad71d47918c6686cf44099915ec0e9544b52c8d5c702cf31a4d72a2b9aa140  edgcpp/upstream/src/cfe.c
27aa5d201622e328f9b543af1e5b664c76777a19aa51b3364b2502dfd1cb874c  edgcpp/upstream/src/checking.h
5a16a2ebd362068f1757cc30f5b4828d67cc1bc16a8e6bfc519e43c0059deb2c  edgcpp/upstream/src/const_ints.c
23280caa487a713f3b1f382056a57c569c9a8d15ff07a218c47ecdea21158d3f  edgcpp/upstream/src/const_ints.h
4112aa9ba2c076dd9b83877bdfae6f107bd12952b6a12776baa6942ad114f18e  edgcpp/upstream/src/defines.h
e9a2fee3c39c8958361236285b57a87a98ceb02592a1de5e1035bd65e7bf3a60  edgcpp/upstream/src/error.c
6611a39baaf1de999aefa446966935878c7786d6fe142240526e830bda32d3e2  edgcpp/upstream/src/error.h
7224d6888ef3bf52e37019f8554a609dc4521b3cf3685612d4c5f948a31e827f  edgcpp/upstream/src/fe_init.c
7089dc53eb8fca9f3c55ed299850914d39a8fdd533937e8809d210a5f6459f2c  edgcpp/upstream/src/host_envir.c
a8c9c5e79dbdd6be8c82f360c58d8681f91015e5e14e61039076b7f8c3b9c2c1  edgcpp/upstream/src/host_envir.h
5d2e1ee19def818fc0012c7a862072c2149f3dfc382b032647863a9b0bb43fa6  edgcpp/upstream/src/il_def.h
4127523bae47ab55d89ed8ed99c067f70081bd44ee7aee3dfa5ea9c643b38305  edgcpp/upstream/src/mem_manage.c
4ca1801049d9ad90b47536dd4c097a0623b589ece80352c2153e28c23a8b50b6  edgcpp/upstream/src/mem_manage.h
59e87adb1b47d456609fbd71c5ae09c33e6cb61f446d803b028c94f910034d15  edgcpp/upstream/src/mem_tables.h
e39ed09cdd9d150092231dce1d2b39032ffd442efe57ced0ad7636e12cf1958b  edgcpp/upstream/src/targ_def.h
c9566f5e831009c33946f1a25277dd376606b2a9bc60a21e48011472fa398b67  edgcpp/upstream/src/target.c
b1bdf56f2fa73f1f3fdb62f5bcf296c55a39f4350ad9d80467c6253a2365330e  edgcpp/upstream/src/target.h
63af3263fe35ac20336506353707f889d7c867f8cee749e9ed9bc966273ae639  edgcpp/upstream/src/target_cfg.h
60d76a8e67fe309e406ba32cf1e20d1e4014d0a81556086d055fec02db644990  edgcpp/upstream/src/target_map.h
72693d1744a5c0b16e388f3ac5f474baca3a63bdd3064d572822282bfdc73771  edgcpp/upstream/src/trans_unit.c
dfdac880faf2620d6098b570c46021b1bc56bdeac6a0d0ba0ba52a69b404306f  edgcpp/upstream/src/util.h
```

Manifest SHA-256 (LF-terminated lines above): `e31c1973f615201faaba50ae2c1ffe8cb19950ea5d1c801b27fd038f65e20b9f`.

Exact recorded cpfe const_ints.c command (not run):

```text
/usr/bin/c++ -DBACK_END_IS_C_GEN_BE=1 -DCOMPILE_MULTIPLE_TRANSLATION_UNITS=1 -DDO_IL_LOWERING=1 -DLOWER_FIXED_POINT=1 -DLOWER_IFUNC=1 -DNEED_IL_DISPLAY=1 -DRECORD_BACKING_EXPRS_WITH_IL_LOWERING=1 -DUSE_CMAKE_DEFINES -I/home/marius/p/dotcc/edgcpp/build/src/includes -O3 -DNDEBUG -std=c++14 -Wall -Wwrite-strings -Wformat-security -pedantic -Werror=shadow -Wimplicit-fallthrough -Wconversion -Wsign-conversion -Wswitch-default -flto -fno-exceptions -fno-unwind-tables -fno-rtti -x c++ -o src/cmake/cpfe/CMakeFiles/cpfe.dir/__/__/const_ints.c.o -c /home/marius/p/dotcc/edgcpp/upstream/src/const_ints.c
```

## Supervisor-run measured evidence after temporary HOLD

I opened the exact supervisor receipts under
`../evidence/integer-first-gate/`: `initializer-ubsan-prepare-r1.json`,
`initializer-ubsan-probe-r1.json`, `integer-r3-gate-receipt.json`,
`native-effective-macros.txt`, the four differential JSON reports, and
`../artifacts/integer-r3/{header-abi.log,native-abi.log}`. The research child
performed no execution. The preparation receipt binds original const_ints.c
SHA-256 `5a16a2ebd362068f1757cc30f5b4828d67cc1bc16a8e6bfc519e43c0059deb2c`.

The freshly observed compiler is GCC 13.3.0, x86_64-linux-gnu, compiler binary
SHA-256 `1353e9bdd29a7295c7226bf6c63abccce056d8cac31f112e5cdbecc3f28c2769`.
Original const_ints.c was compiled using its cpfe command plus
`-fsanitize=undefined -fno-sanitize-recover=undefined`, function/data sections
and dependency-file recording. The wrapper adds the original source quote
include directory. The isolated link uses -O3/-flto/UBSan/GC sections and
normal dynamic/default-linker behavior; it is not the full static/gold cpfe
executable. Both original code and wrapper were freshly compiled, not merely
relinked from the historical native binary.

The unchanged probe source is
`fe067fa053022fac7067433f799dcb35758036cbcc0930fcb73b8dd041d9bc79`;
its executable is
`0c27506bc36d734e71b7d13322a880d209addca8326e7414a2b715b932aa239c`.
The link command names the sanitized original
`../artifacts/initializer-ubsan-r1/const_ints.o`; its preparation receipt was
also opened. No assertion/global failure stub or full initialization was linked.

| Probe mode | Exit | Measured result |
| --- | ---: | --- |
| describe | 0 | maxwidth=65535, char_bits=8, bytes=8192, storage_bits=65536; signed count=-65407/offset=-4087/first=-15/second=31; other counts=-65408/offset=-4088/first=0/second=16 |
| valid-127 | 0 | `7fffffffffffffffffffffffffffffff`; no UBSan diagnostic |
| valid-128 | 0 | `ffffffffffffffffffffffffffffffff`; no UBSan diagnostic |
| signed-max | 1 | original const_ints.c:1024:45: `runtime error: shift exponent -15 is negative` |
| unsigned-max | 0 | `00000000000000000000000000000000`; no UBSan diagnostic |
| signed-extend | 0 | input is exactly 1, bits=65536; zero limbs; no UBSan diagnostic |

The signed-extend mode is independent: input 1 does not claim to be the minimum
produced by startup after the preceding signed-mask operation has encountered
UB. No mode calls target_early_init, proc_command_line, target_one_time_init,
target_init, init_int_kind_min_max_values or const_ints_init. **UB is reproduced
for the isolated original callee/input under this receipt; actual full
initializer execution remains unverified. INIT-001 remains OPEN/unqualified.**
A sanitizer exit is retained failure evidence, not an accepted negative test or
portable semantic value. No native bug label, clamp, reduced width, skipped
kind, CLR-masked substitute or production native fallback is authorized.

The fresh ABI logs measure integer size=16/alignment=2, ushort limb size=2 and
alignment=2, eight limbs/128 bits, part offset=0/stride=2, bool/byte-bool=1,
int=4, long/pointer/size_t/ptrdiff_t=8 and ushort promotion to int. The four
differential reports record 20,844 leaf plus 12,546 arithmetic cases for each
of JIT and NativeAOT: 33,390 cases per execution mode, matching raw results,
alias partitions and other-operand mutations. The supervisor reports both
self-suites passed; the gate receipt records their successful execution.
These defined-domain leaf results do not qualify initialization, unported
runtime or complete parity. Independent held original/C# review remains a
separate gate.

The effective const_ints.c macro receipt now confirms the selected width and
representation, CHECKING=1, DEBUG=0, MAKE_FRONT_END_CALLABLE=FALSE,
EXPENSIVE_CHECKING=FALSE, STANDALONE_UTILITY_PROGRAM=FALSE,
USE_MMAP_FOR_MEMORY_REGIONS=TRUE, EXIT_ON_INTERNAL_ERROR=FALSE,
HOST_ALIGNMENT_REQUIRED=8, HOST_ALLOCATION_INCREMENT=65536 and
ORPHAN_PROCESSING_NEEDED=TRUE. DEFAULT_TARGET_CONFIGURATION_NAME is absent.
Its SHA-256 is `87362eb346c8676da4ae464243efcc7a3c00a0c61fdc540b47b336261a50d310`.
This is the const_ints.c expansion, not a falsely relabeled target.c expansion.
Target startup/caller facts remain directly inspected source evidence.

I reopened current prepare_integer_oracle.py. Since the initial manifest it
adds wrapper-only -iquote, dependency/include hashing, effective macros,
compiler/host receipts and optional sanitizer flags. The supervisor also
corrected macro scope in its header ABI harness. The old tool hash retained
above is historical, not the current tool. The final manifest below records
current tool/evidence bytes; upstream source bytes remain unchanged.

Final source/caller closure: init_int_kind_min_max_values includes both
bit-precise kinds without a dialect guard; its ordinary helper and bit-precise
helper retain their real unexpected-condition/assertion failures. The latter
require the runtime frontier in runtime-next-closure.md. Remaining supervisor
input is actual unchanged cpfe startup arrival/state instrumentation if full
initialization is to be qualified. No question to the user is needed.

The selected entry wrapper is cfe.c:220–241, EDG_MAIN. Under the measured
MAKE_FRONT_END_CALLABLE=FALSE it directly calls cfe_main; host_envir.h maps
EDG_MAIN to main. The setjmp/fe_cleanup/returned-exit-status alternative is
inactive for this cpfe target. process_translation_unit:746–880 saves existing
TU state if present, sets current initialization positions and primary/export
flags, computes IL prefix size, and calls fe_init_part_1 only for the primary
TU, BEFORE alloc_translation_unit. These earlier state writes/calls are part of
the real path and cannot be replaced by running the three target routines in
isolation. They contain no bitint_maxwidth_value assignment.

## Exact researched source ranges

All rows are source-researched / implementation-unstarted in this task; no C#
acceptance. Hashes retain original bytes and line endings, inclusive one-based
ranges. Startup/dispatch summaries above describe relevant branches and edges;
full implementation ledgers and original/C# review remain worker obligations.

| Entry | Native source range | SHA-256 |
| --- | --- | --- |
| find_target_configuration | target.c:267–284 | `ff6cfe28a73d162cc609584fb71ff31692b4f9544244dbe33dbb5dec15fce8ca` |
| set_target_configuration | target.c:287–329 | `f9d8aa86f2943084befeadf26a7cd8608339568ad62332c7103adf66fa613a76` |
| check_target_configuration | target.c:482–711 | `31766a8bc394fdccfc2fab453722c313e7972a84f34b62effdccb3d545a071ea` |
| target_early_init | target.c:933–1030 | `66bce48b88ebaccf9c2212667014aa565298711ec75291ccf209444c7c8bdf66` |
| target_one_time_init | target.c:1033–1063 | `031990dc89b3ad54cae390eed019e42b70ca5425ef88b545a37e179ef245e34b` |
| target_init | target.c:1066–1137 | `f795c7210b0e7331c93ff303ee900673c3f4e3a1d46dff30336453852cc2da3b` |
| make_integer_value_mask | const_ints.c:829–850 | `11aaa3e8db571d9a89c273ce683afefa2da6b79f2bfcf75efb9190d42e533877` |
| sign_extend_integer_value | const_ints.c:853–865 | `5c320ebeded1145e8df74252dfb1773da955485d6ba650aed751f230e1fbf7cc` |
| shift_left_integer_value | const_ints.c:921–977 | `8b8c950dbf5b7d205c1cf2f3c785555efb41bf967dbf2789cdbde2955eb2d9d6` |
| shift_right_integer_value | const_ints.c:980–1029 | `c87053cc3bef929a3d03d7c42c64731ffdd844b674e37e5b196bcb9ad7b18faf` |
| get_integer_size_and_alignment | const_ints.c:2426–2479 | `b75944da32c291ed7cf3913f5dc1668b1ce8b555301856a1450faaded5207a0c` |
| get_bit_precise_integer_size_and_alignment | const_ints.c:2482–2522 | `e550ba043ee8b7a968378bc1dbb0e2cad22eb2c6d87a8d1ef200cb8cbbebd050` |
| init_int_kind_min_max_values | const_ints.c:2681–2721 | `8b05e1a62eb765528be51e790cbd9b0a1d09ff4a50786c36e2240e20c03b1528` |
| const_ints_init | const_ints.c:2724–2736 | `c758e486cf6f453b65737d606ba81c79f08fb285de32482759e26f61552c44da` |
| cfe_main | cfe.c:65–214 | `0986a2106df22c3414f2eaa08de44e1d29da2b20bffdd8e5570a1b52af10215d` |
| EDG_MAIN | cfe.c:220–241 | `754a3bd06e7e449bb3346766702f69ceaf757a9e5b037f7d53f690602c841829` |
| fe_early_init | fe_init.c:1752–1793 | `564b8c39de5e1364428baf0e90367006f2783b2421a8ed27eb8766b9242964ca` |
| fe_one_time_init | fe_init.c:1796–1881 | `f722f3b882b67af228abf30447393935b19fcc1beb7833cf24b641b17cff03cd` |
| fe_init_part_1 | fe_init.c:1884–2076 | `2987e06d8b36af286d9f47b44656c723932852f6110f9fa44c4d8d14e0a0e1c1` |
| process_translation_unit | trans_unit.c:746–880 | `963d282d73d793f5d1dfd6a0723121fc847593b52014e9bbc7cd1aa9096c5703` |
| optk_target source block | cmd_line.c:12180–12197 | `a76a054d8b9656478b1c293b99aa15c23d9e888ecec1f675a6a1200f4c287a4e` |

## Final current source/tool/evidence snapshot

The earlier manifest retains its historical inspection; these are current
bytes after supervisor gate and explicit research ownership release. Raw
initial failures and all supervisor receipts remain preserved.

```text
568c80d00f1887a37a076c35bd8123fd1de1910cbecf73f18151d25bfb86d609  edgcpp/build/compile_commands.json
35c763753d20fe99c59649eb51aafccec7d7d61b6c227824d52d362fcc9e111f  edgcpp/build/src/includes/cmake_defines.h
08daa3060ada394fc5fd9e923da52fd8151a149c9ab52003d3ad8308befecd0d  edgcpp/build/src/includes/err_codes.h
95bfae78554e4921117b1ac833ce030f669b5d3c05cdeb9dfd1386cadf71af05  edgcpp/build/src/includes/err_data.h
0c27506bc36d734e71b7d13322a880d209addca8326e7414a2b715b932aa239c  edgcpp/manual-csharp/faithful/artifacts/initializer-probe-r1/initializer-probe
fed24e443d8577ab3f62275d1112fc3891f3b396c6948e1bdd2e6ae9c86a8d68  edgcpp/manual-csharp/faithful/artifacts/initializer-ubsan-r1/const_ints.o
28fb69a0d586fd4c07fdb26746c5c2062ef523cc98a73358a3c1f08ff75b9d85  edgcpp/manual-csharp/faithful/artifacts/integer-r3/header-abi.log
603760d85b6703c3cc66e6dd2f5144e8bc64359de572a9cd0699c26c79a850bd  edgcpp/manual-csharp/faithful/artifacts/integer-r3/native-abi.log
0d2e2c02b449b104410fd1f4e9c0346c3202b96a2a313cdae9f46f064b5b874a  edgcpp/manual-csharp/faithful/baseline/integer_abi_probe.cpp
0bca45789cede1c59d7ea505788c34ba999fbb7ea404361056e96ac038892d05  edgcpp/manual-csharp/faithful/evidence/integer-first-gate/diff-aot-arithmetic.json
5ee5a9e2b6c7e5ebb4feddade6ec19fc8123172463607af240ba8031620eb2de  edgcpp/manual-csharp/faithful/evidence/integer-first-gate/diff-aot-leaves.json
6b650b92d54c596fa8a4b89e8a284e95dfb159032587ac9fbf71182f1795f8cc  edgcpp/manual-csharp/faithful/evidence/integer-first-gate/diff-jit-arithmetic.json
a284d2b0345fd7acf8dc4ef6ca7b2f09ddb8f563e5956511141aaab642175663  edgcpp/manual-csharp/faithful/evidence/integer-first-gate/diff-jit-leaves.json
6934cc24bed9936957843343f90499c75f9161db43029183578635de3903934a  edgcpp/manual-csharp/faithful/evidence/integer-first-gate/initializer-ubsan-prepare-r1.json
43e7da00075bb5422337f79f7f756030ba3cc7d48d18a2dbe2877f74e1aece96  edgcpp/manual-csharp/faithful/evidence/integer-first-gate/initializer-ubsan-probe-r1.json
51eeca0e0be6eb16cd5fb2cd2889321fee88dff02460ed41f5fe69429c30246d  edgcpp/manual-csharp/faithful/evidence/integer-first-gate/integer-r3-gate-receipt.json
87362eb346c8676da4ae464243efcc7a3c00a0c61fdc540b47b336261a50d310  edgcpp/manual-csharp/faithful/evidence/integer-first-gate/native-effective-macros.txt
fe067fa053022fac7067433f799dcb35758036cbcc0930fcb73b8dd041d9bc79  edgcpp/manual-csharp/faithful/research/initializer-width-probe.cpp
93d32d4bc0ef2c01f09651e4ef1d99d9ce76dc20821accea7c00f61a9bcf4cda  edgcpp/manual-csharp/faithful/tools/prepare_integer_oracle.py
65b5752f8f2e0e1eda6dccb59996bf996452df53b8ebd071dab5cb397918b0e9  edgcpp/upstream/src/basics.h
85ad71d47918c6686cf44099915ec0e9544b52c8d5c702cf31a4d72a2b9aa140  edgcpp/upstream/src/cfe.c
27aa5d201622e328f9b543af1e5b664c76777a19aa51b3364b2502dfd1cb874c  edgcpp/upstream/src/checking.h
77ce3949dffb0fd85a8bca85a33ae590fc2c5b7b7f200b47ff672c35cca8bded  edgcpp/upstream/src/cmd_line.c
5a16a2ebd362068f1757cc30f5b4828d67cc1bc16a8e6bfc519e43c0059deb2c  edgcpp/upstream/src/const_ints.c
23280caa487a713f3b1f382056a57c569c9a8d15ff07a218c47ecdea21158d3f  edgcpp/upstream/src/const_ints.h
4112aa9ba2c076dd9b83877bdfae6f107bd12952b6a12776baa6942ad114f18e  edgcpp/upstream/src/defines.h
477909c104df131c14a5b50dfbd263d5f03441e5e2079594090e27dac6a29cdc  edgcpp/upstream/src/direct_allocator.h
e9a2fee3c39c8958361236285b57a87a98ceb02592a1de5e1035bd65e7bf3a60  edgcpp/upstream/src/error.c
6611a39baaf1de999aefa446966935878c7786d6fe142240526e830bda32d3e2  edgcpp/upstream/src/error.h
7224d6888ef3bf52e37019f8554a609dc4521b3cf3685612d4c5f948a31e827f  edgcpp/upstream/src/fe_init.c
7089dc53eb8fca9f3c55ed299850914d39a8fdd533937e8809d210a5f6459f2c  edgcpp/upstream/src/host_envir.c
a8c9c5e79dbdd6be8c82f360c58d8681f91015e5e14e61039076b7f8c3b9c2c1  edgcpp/upstream/src/host_envir.h
535768a37a2bd0f90dd18c41bd867398074289a305c3ff88589da67153514cd0  edgcpp/upstream/src/il.c
6a2125d77c052ee8a98c04d5cb3b6b51ba6abc9220314d273af499c83e334a88  edgcpp/upstream/src/il.h
aa12a01de83a70158cf2fe377630b850e668e66e81a43fc0136a4334524818fd  edgcpp/upstream/src/il_alloc.c
f2671903e3fb7c900bd5723cbbd9a36af5797b546ac033b94908b78e579e030b  edgcpp/upstream/src/il_alloc.h
5d2e1ee19def818fc0012c7a862072c2149f3dfc382b032647863a9b0bb43fa6  edgcpp/upstream/src/il_def.h
4127523bae47ab55d89ed8ed99c067f70081bd44ee7aee3dfa5ea9c643b38305  edgcpp/upstream/src/mem_manage.c
4ca1801049d9ad90b47536dd4c097a0623b589ece80352c2153e28c23a8b50b6  edgcpp/upstream/src/mem_manage.h
59e87adb1b47d456609fbd71c5ae09c33e6cb61f446d803b028c94f910034d15  edgcpp/upstream/src/mem_tables.h
e39ed09cdd9d150092231dce1d2b39032ffd442efe57ced0ad7636e12cf1958b  edgcpp/upstream/src/targ_def.h
c9566f5e831009c33946f1a25277dd376606b2a9bc60a21e48011472fa398b67  edgcpp/upstream/src/target.c
b1bdf56f2fa73f1f3fdb62f5bcf296c55a39f4350ad9d80467c6253a2365330e  edgcpp/upstream/src/target.h
63af3263fe35ac20336506353707f889d7c867f8cee749e9ed9bc966273ae639  edgcpp/upstream/src/target_cfg.h
60d76a8e67fe309e406ba32cf1e20d1e4014d0a81556086d055fec02db644990  edgcpp/upstream/src/target_map.h
72693d1744a5c0b16e388f3ac5f474baca3a63bdd3064d572822282bfdc73771  edgcpp/upstream/src/trans_unit.c
7bc5d8c9b472a2d72c724b628f384a49fa726f3c8d7406fedaf9107596375495  edgcpp/upstream/src/types.h
dfdac880faf2620d6098b570c46021b1bc56bdeac6a0d0ba0ba52a69b404306f  edgcpp/upstream/src/util.h
```

Manifest SHA-256 (exact LF-terminated lines inside the fence): `437282eec1fa425392358ad900e7dcd54ed23dac503e782c906ec785f1a20e57`.

**HOLD.** No more writes remain to any owned file. No child compiler/test/native
preparation/Git execution, upstream change, production C# change, or
subdelegation occurred. Probe bytes were unchanged after supervisor execution.
Only the supervisor can release further writes or run additional validation.
