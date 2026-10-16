# Ordinary const-method checker

The first immutable product campaign passed clean CLI build, production
publication, generic checker build and complete native-equal JIT execution.
This final checker now binds the actual generated Cell and field names; its
physical/JIT/NativeAOT execution and final acceptance remain open until recorded.

Build `Checks.csproj` with `GeneratedProject` set to the actual generated
`ordinary-const-method.cpp` project. This references the existing runtime
transitively and introduces no reader, native application provider, callback
or new runtime implementation. Scalar application observations use the
established `Translated.Program.fixture` / `lifetime_*` interface.

The single run argument is the separate native observation JSON:

```text
Checks /mnt/storage/roslynobj-csharp-20261015/t5/native-ordinary-const-method-run-1/stdout.txt
```

The checker calls the fixture twice, compares every result, all nine scalar
values and every event against the complete native `runs` object, and requires
repeatability. The pinned native observation is result 206, values
`[5,201,201,0,0,0,0,0,0]`, events `[15,105,221,401]`, with zero overflow and zero
live protocol values. This compares the two actual method results before and
after the caller changes its field, including unsigned high-bit value 201.

Cell has no counted resources. `lifetime_live()` and the unused zero observation
slots are protocol, not ownership, lifetime or cleanup evidence. Event capacity
is 64. Explicit JsonNode array values and JSON output are AOT-safe without
reflective DTO serialization.

Actual output `t5/ordinary-const-method-cli-generated-1/Generated.cs` declares
`Translated.Cell` with explicit size 1/pack 1 and public byte `kind` at offset
0. The final checker uses those actual published names directly. An independent
physical JSON result measures complete size 1, kind field offset 0 and natural
alignment 1 using a sequential byte-prefix probe. It stores 201 through the
typed byte field and verifies both its typed read and raw stored byte.
The agreed key is `ordinaryConstMethodPhysical`, with fields `cellSize`,
`kindOffset`, `sequentialPrefixOffset` and `unsignedHighBitValue`; the latter
reads the typed field after byte 201 is stored. The full native comparison
separately proves the generated unsigned-byte method result.
No generated name or physical passing result is inferred from capture alone.
The physical checker has not yet been built or executed; its actual JIT/AOT
result remains open.

This slice concerns an ordinary generated receiver with a scalar result and an
explicit field-return body whose noexcept fact is null. It does not authorize
native receiver/callback ABI use, polymorphic dispatch, EH, reference returns
or arbitrary class/string/link closure. Source and captured inputs remain
frozen; previous checkers, Runtime and providers are unchanged.
