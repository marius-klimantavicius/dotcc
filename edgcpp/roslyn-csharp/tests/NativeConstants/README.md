# Retained native constants and function-address destination views

The genuine C fixture covers the two observed SQLite compilation failures:

- A routine-pointer constant retains the unsigned all-ones value
  `18446744073709551615`; its C# `nint` cast must use unchecked conversion.
- A global data-pointer initializer retains a function address. Its canonical
  routine address remains unchanged, with an explicit cast to the captured
  destination `void*` view.

The application calls the same generated ordinary body through both stored pointer
views. The checker verifies result `57`, all five observations `[1,1,1,1,42]`,
canonical address equality, native width/all-ones representation, Cdecl entry calls
through both stored values, and address stability after GC. The separate native
oracle is never linked into or loaded by the generated application.

The coordinator's immutable snapshot10 proof observed native/JIT/NativeAOT result
`57` and all five values. Commands and hashes are recorded under
`/mnt/storage/roslynobj-csharp-20261015/t3/native-constants-*-10`; the final receipt
is maintained by the coordinator. This is a bounded constant/address
projection proof, not complete SQLite compilation or SQL execution evidence.

To reproduce from the repository root, select a fresh artifact directory. Campaign
builds and exports are serialized by the coordinator:

```sh
native_constants_artifacts=/absolute/path/to/new-native-constants
mkdir -p "$native_constants_artifacts"

env ROSLYNOBJ_COMPILER=/mnt/storage/roslynobj-duckdb-20261014/export/major2/producer/cf619053cbedabca46da3eab3e8278de43fa4a25ef770a56572454abf9ff25d4-roslyn-cpfe \
  python3 edgcpp/roslyn/build/roslyn-cpfe.py --gcc --c17 --gnu_version 130300 \
  edgcpp/roslyn-csharp/tests/Fixtures/T3/native-constants/native-constants.c \
  -o "$native_constants_artifacts/native-constants.roslynobj"

gcc -std=c17 \
  edgcpp/roslyn-csharp/tests/Fixtures/T3/native-constants/native-constants.c \
  edgcpp/roslyn-csharp/tests/Fixtures/T3/native-constants/native-oracle.c \
  -o "$native_constants_artifacts/native-oracle"

"$native_constants_artifacts/native-oracle"

dotnet build edgcpp/roslyn-csharp/tests/Projection/Projection.csproj -c Release \
  -o "$native_constants_artifacts/emitter"

dotnet "$native_constants_artifacts/emitter/Projection.dll" emit \
  "$native_constants_artifacts/native-constants.roslynobj" \
  "$native_constants_artifacts/generated" \
  "$PWD/edgcpp/roslyn-csharp/src/RoslynObj.CSharp.Runtime/RoslynObj.CSharp.Runtime.csproj"

dotnet build edgcpp/roslyn-csharp/tests/NativeConstants/Checks.csproj -c Release \
  -p:GeneratedProject="$native_constants_artifacts/generated/Generated.csproj" \
  -o "$native_constants_artifacts/jit"

dotnet "$native_constants_artifacts/jit/Checks.dll"

dotnet publish edgcpp/roslyn-csharp/tests/NativeConstants/Checks.csproj -c Release \
  -r linux-x64 -p:PublishAot=true \
  -p:GeneratedProject="$native_constants_artifacts/generated/Generated.csproj" \
  -o "$native_constants_artifacts/aot"

"$native_constants_artifacts/aot/Checks"
```
