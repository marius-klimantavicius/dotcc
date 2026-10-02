# EDG C/C++ compiler investigation

Built and investigated through **2026-10-02**, Linux x86-64, GCC 13.3.0,
.NET SDK 10.0.111. EDG calls this release **7.0**. The repository is
[`edgcpp/compiler`](https://github.com/edgcpp/compiler), not `edgecpp`.

## Deliverables

1. A working, broadly enabled EDG build in [`build/bin`](build/bin), including
   `cpfe` (C output), `cpfe-cp` (C++ output), `cdisp` (IL inspection),
   `edg_prelink`, `edg_munch`, `edg_decode`, and the `eccp` native driver.
   Native runtime archives are in `build/lib/libC.a` and
   `build/lib_linux_x86_64/libC.a`.
2. [C++ → C → dotcc → C# feasibility](docs/c-lowering-to-dotcc.md).
3. [Independent EDG → C# backend feasibility](docs/direct-csharp-backend.md).
4. [The compiler itself lowered to C](artifacts/self-lowered/README.md): all
   75 `cpfe` translation units, 164.6 MiB of unmodified generated C. All pass
   GCC's C syntax check; no whole-program link has been attempted.
5. [Combined compiler C file](artifacts/self-lowered-combined/README.md): all
   75 translation units merged by EDG into 110.1 MiB. Generation succeeds,
   but GCC validation finds missing generated types and helper declarations.
6. [Implementation plan for dot_gen_be.c](docs/dot-gen-be-plan.md): direct
   unsafe C# emission, CLR exception unwinding, cross-platform library/runtime
   integration, NativeAOT acceptance and optional Roslyn post-processing.
7. [C# backend implementation](backend/README.md): `cpfe-dot`, direct scalar/
   pointer/aggregate emission, stack/heap frames, pinned globals/literals, TLS,
   managed function pointers, RAII cleanup for ordinary objects, fixed arrays and ordered/conditional temporaries,
   explicit instance mode, executable/library output,
   type placement, output splitting and runnable
   Linux x64 NativeAOT fixtures, now including floating point, bitfields, switch, dynamic initialization/TLS and owner allocation/shutdown. This is an experimental subset.
8. [SQLite acceptance attempt](evidence/sqlite-edg.json): original EDG C generation and native object compilation pass; direct C# emission currently stops at the external math-function address `trunc`.

EDG sources and its upstream test suite are embedded in `upstream/`, alongside
the canonical backend, configuration, probes and evidence. Build products and
intermediate artifacts remain ignored and are retained throughout implementation.
The imported revision is recorded in [upstream-origin.json](upstream-origin.json);
the original license remains in [upstream/LICENSE.txt](upstream/LICENSE.txt).
See [AGENTS.md](AGENTS.md) for the current implementation and verification rules.

## Reproduce

```bash
# From the dotcc repository root; requires Python 3, CMake, Ninja and GCC.
./edgcpp/build.sh
python3 edgcpp/verify.py

# Requires the .NET 10 SDK and dotcc's normal dependencies.
dotnet build DotCC/DotCC.csproj -c Release
python3 edgcpp/probe.py
python3 edgcpp/probe.py --system-vector

# Generate inspectable C for every compiler translation unit; check with GCC.
# Temporary binary IL uses /dev/shm by default (--temporary-dir overrides it).
python3 edgcpp/lower_self.py

# Merge all translation units into one C file (currently fails GCC validation).
python3 edgcpp/lower_self.py --combined
```

`build.sh` pins upstream commit
`89e67e07c7f6d0fc38622f9778d8e79f40deaa0f` (2026-09-30), recorded in the embedded
source provenance manifest. It copies the custom
[configuration](config/base.cmakedef) into the embedded source tree and
uses the official CMake build. `EDG_BUILD_JOBS` overrides the default 12 jobs.
The inspected checkout occupies about 4 GB; build products about 203 MB.

The runtime driver needs `EDG_BASE` and the actual GCC version. The script
sets both. Upstream's sample configuration otherwise defaults to GCC 14 and
passes a warning option GCC 13 does not recognize. Enabling Sun extensions
also requires a `SUN_TARGET_VERSION_NUMBER` for the C++ output backend; the
custom profile supplies one. Neither required a compiler source patch.

Runtime rebuilds are explicitly enabled when the compiler changes. The build
is Release with checks enabled, without ASan. It builds the default and Linux
x86-64 runtime profiles, not runnable runtime libraries for every cross target.

## Meaning of “all extensions allowed”

The profile compiles in all extension families exposed by the audited
`*_EXTENSIONS_ALLOWED` and `*_ENABLING_POSSIBLE` switches in `lang_feat.h`,
`targ_def.h`, and `host_envir.h`, plus optional GNU attributes, assembler
functions, legacy C constructs, named address spaces/registers, and VLAs.
It retains IL serialization and display. `verify.py` checks **37 effective
configuration gates**, after preprocessing the same headers/configuration
used by the C backend; all are enabled. The exact list is in
[the evidence snapshot](evidence/features.json).

“Allowed” means present in the executable, with modes selectable per input.
It does not mean enabling every dialect simultaneously or implementing every
extension in the C output backend. For example, UPC selects C, C++/CLI and
C++/CX are distinct modes, and obsolete assignment-to-`this` is explicitly
incompatible with the selected Itanium ABI plus lowering. That incompatible
switch retains its upstream setting. There is no claim that CUDA, OpenMP,
SYCL, or another vendor feature absent from this configuration has been added.

| Syntax/semantic smoke test | Result |
|---|---|
| GNU statement expressions, vectors, `__int128`, `__float128` | Pass |
| Clang emulation | Pass |
| Microsoft `__declspec` and `__int64` | Pass |
| Sun linker-scope extension | Pass |
| UPC `shared` | Pass |
| Embedded C fixed point | Pass |
| Variable length arrays | Pass |
| C++20 coroutine | Pass |
| C++26 reflection and type splicing | Pass |
| C++/CLI | Compiled in; test stops at missing `mscorlib.dll` metadata |
| C++/CX | Compiled in; test stops at missing `platform.winmd` metadata |

These are focused mode tests, not the full upstream conformance suite. The
portable assembly paths inherited from upstream have no corresponding
metadata bundle in this checkout. Building the gates does not supply a Windows
SDK or a CLR backend. GNU mode also enables Clang compatibility support.

## Use the built compiler

The name of EDG's C-generating executable is `cpfe`:

```bash
mkdir -p edgcpp/artifacts/manual
cd edgcpp/artifacts/manual
../../build/bin/cpfe \
  --clear_flag use_predefined_macro_file \
  --c++20 --instantiate used --no_inlining \
  --gen_c_file_name arithmetic.c -o arithmetic.il \
  ../../probes/arithmetic.cpp
```

**Use `--gen_c_file_name` for C output.** In this IL-enabled build, `-o` names
the binary EDG IL file. EDG IL is its native compiler data representation, not
.NET CIL. The working directory also receives template-instantiation metadata
(`*.ti`), so use an artifact directory.

`--clear_flag use_predefined_macro_file` makes header-free probes independent
of the bundled target macro file. Real code needs matching include paths,
predefined macros and ABI selection; the native `eccp` driver supplies these:

```bash
# From repository root.
edg_root="$PWD/edgcpp"
export EDG_BASE="$edg_root/upstream/bases/docker/dev-env/gcc"
export PYTHONPATH="$edg_root/upstream/dev_tools/pylibs"
export EDG_GCC_VER_SCRAPE=$("$edg_root/upstream/dev_tools/bin/edg-scrape-compiler" gcc version)
cd "$edg_root/artifacts"
"$edg_root/build/bin/eccp" --c++20 --instantiate used \
  "$edg_root/probes/arithmetic.cpp" -o arithmetic-native
./arithmetic-native
```

The arithmetic program also round-tripped through `cpfe-cp` → GCC, and `cdisp`
successfully read its serialized, lowered IL.

## Evidence and limits

[evidence/](evidence) records the final matrix, feature gates, revisions,
and binary hashes. Full commands, diagnostics, generated C, generated C#,
IL files and native executables remain in `artifacts/`. A zero exit code in
the matrix means success for that stage; omitted stages were not reached.
The probe runner intentionally records failures without treating every
unsupported case as a harness failure. The layout case intentionally detects
a semantic mismatch and exits 1 on the managed path.

Ten header-free C++ probes lower and run natively, including exceptions and
RTTI; all eleven source probes pass GCC. The bundled EDG headers do not
provide `<vector>`. With the host libstdc++ headers configured, that example
also builds and runs through EDG after working around a serialized-IL reader
internal error using `--set_flag skip_il_read`. The default serialized route
still fails; the workaround is recorded, not presented as a fix.

Upstream declares [Apache-2.0 WITH LLVM-exception](https://github.com/edgcpp/compiler/blob/89e67e07c7f6d0fc38622f9778d8e79f40deaa0f/LICENSE.txt).
This investigation keeps the external source separate from dotcc's own source.

Primary build references: [BUILD.md](https://github.com/edgcpp/compiler/blob/89e67e07c7f6d0fc38622f9778d8e79f40deaa0f/BUILD.md),
[HACKING.md](https://github.com/edgcpp/compiler/blob/89e67e07c7f6d0fc38622f9778d8e79f40deaa0f/HACKING.md),
[CMake configuration](https://github.com/edgcpp/compiler/tree/89e67e07c7f6d0fc38622f9778d8e79f40deaa0f/cmake/macro-conf).
