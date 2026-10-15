This is a finite Linux x64 host-contract experiment, independent of the reader,
translator, runtime, SQLite, and all translated application bodies. It targets
.NET 10 / C# 14, using the same unmanaged Cdecl function-pointer / UCO boundary
style as generated callbacks. The native source only observes or forwards host
ABI calls.

The raw `open` PInvoke and managed-later errno reads are recorded separately
from an independent invocation whose errno is captured immediately in C. No
assertion requires errno to survive a managed transition, allocation, GC, or
reentry. The protected experiment saves and restores errno **inside C**, around
managed callbacks and nested reentry; it is design evidence, not an implemented
product bridge. Passing finite observations do not establish universal safety.

The foreign pthread calls a UCO callback, which reenters native and managed code
and allocates/collects managed memory. Checks cover completion, same-thread TLS
identity across reentry, distinct main/worker errno and marker TLS addresses,
and separate main/worker marker state. Exceptions are caught in each callback
and become failure codes. A failed pthread join retains callback context until
process exit; the run fails rather than freeing a potentially live context.

The native mutex probe keeps opaque pthread objects in native storage. Sizes,
alignments, constants, and actual errorcheck lock/trylock/unlock/destroy return
codes come from native accessors. Return codes are not interpreted as errno.
Loaded helper/libc/pthread symbol addresses, module paths, and module hashes are
recorded. There is no claim about arbitrary providers, SQLite execution, broad
thread semantics, errno bridging, callback lifetime, or AOT outside this probe.

Coordinator commands, using a fresh artifact directory (`$probe_artifacts`):

```sh
gcc -std=c17 -O2 -g -fPIC -shared -Wall -Wextra -Werror -pthread \
  edgcpp/roslyn-csharp/tests/Fixtures/HostContracts/host-contracts.c -ldl \
  -o "$probe_artifacts/libhost-contracts-probe.so"
dotnet build edgcpp/roslyn-csharp/tests/HostContracts/HostContracts.csproj \
  -c Release --nologo -o "$probe_artifacts/jit"
dotnet "$probe_artifacts/jit/HostContracts.dll" \
  "$probe_artifacts/libhost-contracts-probe.so" "$probe_artifacts/jit-report.json"
dotnet publish edgcpp/roslyn-csharp/tests/HostContracts/HostContracts.csproj \
  -c Release -r linux-x64 --self-contained true -p:PublishAot=true \
  -o "$probe_artifacts/aot" --nologo
"$probe_artifacts/aot/HostContracts" \
  "$probe_artifacts/libhost-contracts-probe.so" "$probe_artifacts/aot-report.json"
```

Builds and execution are serialized by the campaign coordinator. Preserve both
reports; errno and address observations may differ between JIT, AOT, and runs.
Only named exact/invariant checks are pass/fail assertions.

The measured reproduction driver creates a fresh artifact directory, records
exact source/compiler/helper/provider/report hashes, and runs JIT and NativeAOT
twice each. It compares selected provider/ABI identities and named invariants,
without requiring errno or addresses to match between processes:

```sh
python3 edgcpp/roslyn-csharp/tests/HostContracts/run-host-contracts.py \
  --artifacts /mnt/storage/roslynobj-csharp-20261015/t3/host-contracts-reproduction
```
