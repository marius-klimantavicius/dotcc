# Fresh faithful-port baseline (2026-10-21)

Research is held at a bounded checkpoint (see `HOLD.md`). This directory records the actual local sources and
existing native artifacts without rebuilding, preparing, or executing EDG. The
explicit resume in `../coordination/RESUME-20261021.md` supersedes historical STOP
records. Prototype acceptance and native-defect deviations do not carry forward.

The import record is `edgcpp/upstream-origin.json`: EDG compiler repository
`https://github.com/edgcpp/compiler.git`, base revision
`89e67e07c7f6d0fc38622f9778d8e79f40deaa0f`. It explicitly identifies an embedded,
locally overlaid tree, not a pristine revision. Local file hashes are essential.
Upstream license: Apache-2.0 WITH LLVM-exception; preserve its attribution.

Early findings for the implementation worker:

- `build/compile_commands.json` has distinct `cpfe`, `cpfe-cp`, `cdisp`,
  `cpfe-dot`, and `cpfe-dot-oracle` command sets. Do not mix their definitions.
- The cache selects Release and macro configuration `all-extensions`. The
  generated `build/src/includes/cmake_defines.h` is the operative header when
  `USE_CMAKE_DEFINES` is passed. Historical Linux defaults in `defines_linux.h`
  are not the operative profile.
- `INTEGER_VALUE_REPR_IS_A_HOST_INTEGER=0`, `INT128_EXTENSIONS_ALLOWED=1`,
  `FIXED_POINT_ALLOWED=1`, `USE_BOOL_FOR_BOOLEAN_IN_CPLUSPLUS=1`, `CHECKING=1`,
  `DEBUG=0`. In particular, `-DNDEBUG` does not disable EDG checking.
- Source-derived integer representation: eight 16-bit unsigned-short parts,
  high part first, 128 bits total; host large signed/unsigned are
  `ptrdiff_t`/`size_t` (64 bits on the recorded x86_64 Linux ABI). This is not
  a single native integer or a dynamically sized big integer.
- The generated `TYPE_FOR_AN_INTEGER_VALUE=unsigned long long` and
  `TYPE_FOR_A_SIGNED_INTEGER_VALUE=long long` belong to the excluded scalar
  representation branch; they do not define `an_integer_value` in this build.
- Strict C++20/LP64 is a runtime qualification profile, separate from these
  compile-time features. Enabling strict mode does not remove compiled branches.

Source hashes (SHA-256, whole file):

```
5a16a2ebd362068f1757cc30f5b4828d67cc1bc16a8e6bfc519e43c0059deb2c  edgcpp/upstream/src/const_ints.c
23280caa487a713f3b1f382056a57c569c9a8d15ff07a218c47ecdea21158d3f  edgcpp/upstream/src/const_ints.h
e39ed09cdd9d150092231dce1d2b39032ffd442efe57ced0ad7636e12cf1958b  edgcpp/upstream/src/targ_def.h
65b5752f8f2e0e1eda6dccb59996bf996452df53b8ebd071dab5cb397918b0e9  edgcpp/upstream/src/basics.h
```

These facts are from source/configuration inspection, not a new runnable ABI
probe. The supervisor owns runnable layout/arithmetic probes and serialized
builds. An existing executable hash alone does not prove that it was built from
all current local source bytes. Detailed inventories remain partial until their
unresolved entries are closed.
