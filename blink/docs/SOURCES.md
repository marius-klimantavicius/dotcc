# Pinned inputs and native interpreter baseline

The campaign uses the unchanged `jart/blink` archive at revision
`f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580`. Its downloaded archive SHA-256 is
`e0bad68ba2927a1ca1d53537afee1644e11a1e683d26676a36e5b9b17a63d6af`.
[The machine-readable manifest](../config/source-manifest.json) records the URL,
all five archive `LICENSE` files, bundled bootstrap executables, selected test
sources, and assembly macro include with individual hashes.

Run from any directory:

```sh
/path/to/dotcc/blink/scripts/fetch.sh
/path/to/dotcc/blink/scripts/native-oracle.sh --offline
```

`fetch.sh` verifies the archive before extraction, then compares every extracted
file and symbolic link against the archive and rejects added files. The ignored
`ref/` directory stays immutable. The native script copies it to
`build/native/source/`, configures and builds there, and writes the executable to
`build/native/blink`. Only the initial fetch requires network access; neither
configuration nor the selected build/test commands download toolchains. Python
3.12 or later, curl, GCC, GNU binutils, GNU Make, and host development libraries
are prerequisites. Native execution requires Linux x86-64.

## Exact native configuration

The manifest supplies `--disable-jit`, `--disable-x87`, `--disable-threads`,
`--disable-fork`, `--disable-mmx`, `--disable-bmi2`, `--disable-metal`,
`--disable-bcd`, and `--disable-rom`. Sockets, syscall tracing, and native
filesystem overlays retain their upstream defaults. This native oracle is not
the managed filesystem isolation implementation.

There is no `--disable-linear` configure switch in this revision. The build
defines upstream's `NOLINEAR` macro through `CPPFLAGS`, disabling
`CanHaveLinearMemory()` in `blink/machine.h`, and each execution additionally
passes `-jm` (disable JIT and linear memory). Generated `config.h` confirms
`DISABLE_JIT`, `DISABLE_X87`, `DISABLE_THREADS`, and the selected optional CPU
feature exclusions; `HAVE_FORK` is unset and `DISABLE_SOCKETS` is unset.

The exact Make target is `o//blink/blink` for upstream's empty default `MODE`.
The repeated slash is significant to upstream's rules. The script overrides
embedded upstream Git identity and build timestamp so the enclosing dotcc Git
checkout and current wall time cannot silently become Blink build metadata.

## Licenses and auxiliary inputs

| Archive input | License / role | Used by this baseline |
| --- | --- | --- |
| Root `LICENSE` and Blink sources | ISC; retain individual source notices | Yes |
| `third_party/libz/LICENSE` | zlib; bundled 1.2.13 source available | Host zlib selected by configure; native binary has no observed zlib dynamic import |
| `third_party/sectorlisp/LICENSE` | ISC | No |
| `third_party/xterm.js/LICENSE` | MIT plus component notices in that file | No |
| `third_party/coi-serviceworker/LICENSE` | MIT | No |
| `build/bootstrap/mkdeps.com` | Bundled dependency scanner, pinned as part of archive | Yes |
| `build/bootstrap/make.com` | Bundled Make, pinned as part of archive | No; system GNU Make used |

Other third-party download recipes mention additional projects and licenses,
including GPL guest images/QEMU. They are not fetched, built, or included in this
baseline. A future expansion must inventory its newly used inputs separately.
In particular, upstream `make check` is deliberately not used because its guest
test recipes acquire extra GCC/QEMU and guest corpora. The selected assembly
inputs are assembled with the installed GCC/binutils and upstream's static ELF
link flags, without a guest libc or downloaded guest toolchain.

The static musl service and its toolchain are recorded separately by the guest
fixture build script and documentation. Host GCC 13.3.0 (Ubuntu
13.3.0-6ubuntu2~24.04.1), GNU binutils 2.42, and GNU Make 4.3 were observed for
the native baseline. `artifacts/native/receipt.json` records executable paths,
versions and hashes. This is an observed host-toolchain record, not a hermetic
host toolchain distribution; changing installed compilers, headers, or host
libraries requires a new receipt.

## Observed baseline

On 2026-09-14 Linux x64, all **25 selected upstream assembly cases passed**
both direct Linux execution and native Blink with `-jm`. Each case exited zero
and produced matching stdout. The selection covers integer arithmetic,
comparisons, flags, moves, stack operations, atomic instructions, REP moves,
and selected SSE/SSE2 moves/shuffles. It is not exhaustive CPU qualification.

The first scripted native binary SHA-256 is
`fac5c0d9db337243d2cb90579d22cef25116122414aaf21df010f3b013bc6e7b`;
its configured header SHA-256 is
`f5d3de0c0e43b8e2fae316fde009efce8f79191535a3857f5462edb3127317c0`.
Raw logs, exact tool and command records, executable hashes, case-level exits,
stdout and stderr are under ignored `artifacts/native/`. Each script rerun
rebuilds from verified source and refreshes these local receipts. Native build
hashes may vary with build path and installed host toolchain.

The same binary also passed the static musl HTTP fixture via:

```sh
python3 blink/scripts/test-native-service.py --blink blink/build/native/blink
```

All six exact-response cases passed: health, file, fragmented request, 128-KiB
body, missing path, and stop. The guest exited zero. The fixture executable
SHA-256 was `916eeadfde4092c55bd2d05d5cacbe20482f4cabda8c0436fcfb68d690f2a9b8`.
`artifacts/guest/native-blink/` contains the service receipt and syscall trace.
This supplies a native behavior baseline; it does not establish any translated
managed execution, managed host isolation, or Windows behavior.
