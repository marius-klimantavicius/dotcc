# Portable translated-program target

`portable-lp64-binary80-v1` describes the translated program's layout, not the
native ABI of the machine running the translator or generated .NET code. It
is an experimental profile with explicit LP64 sizes and software binary80
`long double`, matching the current emitter and EDG constant evaluation.
The plan's proposed binary128 target remains a separate implementation task.

The JSON layout and `.def` macro table are versioned together. The custom
`dot_sys_predef.c` wrapper includes the unchanged upstream system support file,
renames its host macro entry point and supplies the portable macro entry point
only for this profile. It retains EDG's other builtin/dialect implementation.
It does not alter parsing, lowering, or the original C backend. The legacy
profile still calls the original macro entry point.

The portable hook also disables the host predefined-macro file, validates
critical frontend sizes/formats, and defines no OS or CPU instruction-set
macros. Merely disabling that file is insufficient: upstream's system hook
defines immutable Linux/x86 macros, which `-U` cannot override. Retained audit
evidence is in `artifacts/campaign/portable-target-audit/`.

Portable libc headers live in `runtime/libc/include`. They do not expose native
`FILE`, pthread, Windows or POSIX structure layouts. Runtime services must use
BCL or runtime-selected adapters with explicit representation conversion.
NativeAOT execution on other platforms remains unverified until tested there.
