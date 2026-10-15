# T2 bounded program assembly slice

The public `ProgramAssembler` selects strong definitions from independent
RoslynObj TUs and preserves their captured identities. This is a verified
selection slice, not generated multi-unit execution or scale acceptance.
The default explicit bound is two simultaneously open fixture objects; all
returned public entities require those input owners to remain open.

The [reproduced receipt](../acceptance/assembly/report.json) records 16 genuine
objects, source/compiler/object/build hashes, measured commands, native C
observation `32`, native C++ observations `75 8` and the passing managed harness.
It also records an isolated source-subset build/run to prove the assembly slice
does not depend on ongoing emitter/runtime edits. Reproduce with:

```sh
python3 edgcpp/roslyn-csharp/tests/Assembly/run-assembly.py \
  --artifacts /mnt/storage/roslynobj-assembly-new
```

An existing immutable object directory can be supplied with `--objects`.

Exercised selection includes cross-unit strong C functions/data, distinct
same-named internal functions/data, actual GNU assembly names, C++ overload and
member names already mangled by EDG, and concrete current signatures. Opaque
data pointers may bind across incomplete/complete pointee declarations;
top-level parameter source qualifiers do not become ABI equality requirements.
No source overload lookup, remangling or C++ ODR/type-semantic validation occurs.

The harness verifies deterministic reports, explicit open-unit bounds and
diagnostics for duplicate strong definitions, conflicting function/data final
names, unresolved externals and differing selected ABI representations. Genuine
weak/common/alias/ifunc and concrete inline/COMDAT inputs activate explicit
unsupported-policy diagnostics; they do not count as supported selection.
The COMDAT fixture actually records both COMDAT and inline flags.

Aggregate-by-value representation checking remains conservative. Inheritance,
member-pointer/vector ABI adaptations and weak/common/alias/ifunc/COMDAT/alternate
entry selection remain unsupported. Declared external providers are unverified
assembly contracts. Emission, global cross-unit fixups, callback/variadic
execution and complete CLI integration still need their own JIT/NativeAOT
evidence before T2 closes.
