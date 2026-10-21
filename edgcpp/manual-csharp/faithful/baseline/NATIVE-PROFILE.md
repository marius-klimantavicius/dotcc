# Native build and runtime profiles are separate

Status: configuration and binary inspection only. No compiler or frontend was
executed by the research child. `local-files.sha256` pins original source,
header, table, configuration and existing executable bytes. Re-run
`python3 edgcpp/manual-csharp/faithful/baseline/pin_baseline.py` from the repository
root to refresh only the owned reports; review any changed input before accepting
the new manifest. The script is deterministic for the same inputs and paths.
`build-commands.json` preserves all 326 compile commands as tokenized arguments
and their exact text/hashes, grouped by target, plus recorded Ninja link blocks.

## Operative compile-time configuration

The recorded build is `edgcpp/build`, cache macro configuration
`EDG_MACRO_CONF=all-extensions`, Release, C++ host language `-std=c++14 -x c++`.
It is not a C compiler merely because files end in `.c`, nor is `-std=c++14`
the C++ dialect that the frontend must accept. The configuration input chain is
the local `edgcpp/config/base.cmakedef` importing `linux-gcc-release/base`, whose
imports and command-line overrides are separately hashed.

The `cpfe` target records 75 source commands, including backend/lowering files
whose compiled presence is not evidence that all routines run under
`--no_code_gen`. The distinct `cpfe-cp`, `cdisp`, `cpfe-dot`, and
`cpfe-dot-oracle` targets record 75, 17, 77, and 75 commands respectively.
`cpfe-dot` includes extra local backend sources and SQLite linkage; preserve it
but do not treat it as the new managed port.

Every `cpfe` source command inspected uses:

```
/usr/bin/c++
-DBACK_END_IS_C_GEN_BE=1
-DCOMPILE_MULTIPLE_TRANSLATION_UNITS=1
-DDO_IL_LOWERING=1
-DLOWER_FIXED_POINT=1
-DLOWER_IFUNC=1
-DNEED_IL_DISPLAY=1
-DRECORD_BACKING_EXPRS_WITH_IL_LOWERING=1
-DUSE_CMAKE_DEFINES
-I/home/marius/p/dotcc/edgcpp/build/src/includes
-O3 -DNDEBUG -std=c++14 -Wall -Wwrite-strings -Wformat-security
-pedantic -Werror=shadow -Wimplicit-fallthrough -Wconversion
-Wsign-conversion -Wswitch-default -flto -fno-exceptions
-fno-unwind-tables -fno-rtti -x c++
```

Source/output arguments vary by compilation unit and are retained verbatim in
the command report. Link flags are `-static -flto=auto -fuse-ld=gold` in the
recorded `cpfe` Ninja block. There is no recorded `-fwrapv`, `-fno-strict-aliasing`,
`-fshort-enums`, or `-funsigned-char` in this target. Do not infer blanket wrapping
semantics for signed arithmetic, or byte-sized enums, from managed defaults.

`cpfe-dot-oracle` differs materially: it additionally defines
`DO_UNORDERED_EH_PROCESSING=0` and
`TARG_IA64_ABI_USE_VARIANT_PTR_TO_MEMBER_FUNCTION_REPR=1`. The name “oracle” does
not make it interchangeable with `cpfe`. Supervisor must choose a target for
each qualified comparison and retain its exact definitions.

`defines.h:30–38` includes generated `cmake_defines.h` under
`USE_CMAKE_DEFINES`. `generated-definitions.json` contains every textual generated
definition (including target-suffixed values), not an effective `-dM` dump.
Command-line definitions take precedence through `#ifndef` and later headers
may derive/change macros. Source review establishes:

- `INTEGER_VALUE_REPR_IS_A_HOST_INTEGER=0`, `INT128_EXTENSIONS_ALLOWED=1`,
  `LONG_LONG_ALLOWED=1`, `FIXED_POINT_ALLOWED=1`.
- `USE_BOOL_FOR_BOOLEAN_IN_CPLUSPLUS=1`, `USE_EDG_NAMESPACE=1`,
  `CHECKING=1`, `DEBUG=0`. EDG assertion paths remain compiled despite `NDEBUG`.
- GNU, Microsoft, UPC, embedded-C, C++/CLI, C++/CX, reflection, coroutine,
  export, vector, VLA, and other feature-enabling macros are compiled in.
  Their exact textual definitions are retained rather than pruned for strict
  C++20. Runtime predicates and invalid-input diagnostics still require ports.
- Legacy target name is `linux_x86_64`. Other named target configurations are
  compiled in: `linux_i686`, `win64`, `win32`, `linux_aarch64`, `linux_armv7`,
  `linux_riscv64`, `linux_riscv32`. These do not change the host ABI.
- Legacy target `long` and pointers are 8 bytes, 8-byte aligned; target `size_t`
  kind is `ik_unsigned_long`, target `ptrdiff_t` kind is `ik_long`; other legacy
  integer widths come from the source defaults pinned in `targ_def.h`.
- Scalar-representation type macros in the generated header are present but
  unused by the selected multipart integer typedef branch.

## Recorded compiler, host, and executable

`build/CMakeFiles/3.28.3/CMakeCXXCompiler.cmake` records GNU 13.3.0,
`/usr/bin/c++`, ELF, x86_64-linux-gnu, little-endian, 8-byte pointer. The system
file records Linux x86_64, kernel 6.8.0-138-generic at configuration time.
These are historical build records, not a claim about the current running kernel.
The current compiler driver and frontend binary are hashed separately without
executing them, so a changed installation cannot silently become old provenance.

Read-only `readelf` inspection of the existing `build/bin/cpfe` records ELF64
LSB, AMD x86-64, GNU build ID `be070c527ea39490b4110f557378f31569642295`, and
`.comment` GCC `(Ubuntu 13.3.0-6ubuntu2~24.04.1) 13.3.0`.

```
e669059e2068f623052ad8a21c146e776ce7cfb331fa849795208e3bcfdf07c3  edgcpp/build/bin/cpfe
384be5ccb0f0a330f635c029815db314cb58719a3999095bcd4f7291b19b6a8c  edgcpp/build/bin/cpfe-dot-oracle
35c763753d20fe99c59649eb51aafccec7d7d61b6c227824d52d362fcc9e111f  edgcpp/build/src/includes/cmake_defines.h
08daa3060ada394fc5fd9e923da52fd8151a149c9ab52003d3ad8308befecd0d  edgcpp/build/src/includes/err_codes.h
95bfae78554e4921117b1ac833ce030f669b5d3c05cdeb9dfd1386cadf71af05  edgcpp/build/src/includes/err_data.h
```

The existing binary is not proven to correspond to every current source/header
byte. File times and CMake commands cannot prove that correspondence. Supervisor
owns any rebuild and runnable provenance gate after all writers HOLD.

## Proposed strict C++20/LP64 runtime qualification

Proposed explicit new-port command profile (not executed here):

```
<pinned-cpfe> --clear_flag use_predefined_macro_file --c++20 --strict
              --target linux_x86_64 --no_code_gen --instantiate used <input.cpp>
```

The source registers `--c++20` at `cmd_line.c:1550`, `--strict` at 246,
`--target` at 1712 and handles target selection at 12180. Explicit target
selection is source-supported. It can change auxiliary-info directory selection
relative to the implicit legacy target (`targ_def.h:5835–5852`); clearing the
predefined-macro-file flag does not authorize erasing that distinction.

The superseded prototype's `tools/oracle.py` uses the same strict/dialect/no-code
generation/instantiation flags but does not pass `--target`. Its command builder
was inspected and hashed only as historical evidence. It is not imported into
the new frontend and its test results give no acceptance credit.

Input/include bytes, command line, current directory, preinclude/predefined macro
file policy, source encoding, host endianness and target configuration belong in
each supervisor receipt. Do not add feature-disabling flags to obtain a smaller
closure. Runtime-disabled semantic branches remain inventoried and source-mapped.

## Supervisor-only next evidence

1. Choose `cpfe` versus `cpfe-dot-oracle` explicitly; retain definitions per target.
2. After all shared writers HOLD, compile/run `integer_abi_probe.cpp` using that
   target's recorded include and definition arguments. It includes original
   header declarations and static checks; no copied native type substitutes.
3. Capture the compiler identity/current binary hash and full command used for
   the probe, then compare size/alignment/part offsets/promotions to
   `INTEGER-ABI.md`. A new probe proves the new compile, not the existing old
   executable's source correspondence.
4. A compiler-expanded macro dump under identical `-D/-I` arguments is needed
   to close the entire effective-feature inventory. No such dump is fabricated
   by the textual inventory tools. Any instrumented/rebuilt oracle also needs
   its own source and executable hashes.
