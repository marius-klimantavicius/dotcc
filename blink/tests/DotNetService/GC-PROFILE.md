# Native static-musl GC configuration witness

Native witness passed. This witness reuses the already passing
static .NET 10.0.12 ELF from `dotnet-guest-musl/attempt-8za50rji`. It does not publish,
start a container, modify the service, or execute translated Blink.

From the repository root:

```sh
python3 blink/tests/DotNetService/native-gc-profile.py
```

The runner pins the producer receipt, static ELF, archived build inputs and original
HTTP oracle. Its child environment contains exactly these four entries:

| Entry | Value | Interpretation |
|---|---|---|
| LANG | C | Locale |
| DOTNET_GCHeapHardLimit | 1000000 | Hexadecimal 16 MiB GC heap hard limit |
| DOTNET_GCRegionRange | 2000000 | Hexadecimal 32 MiB GC region reservation range |
| DOTNET_GCRegionSize | 100000 | Hexadecimal 1 MiB GC region size |

The package's repository pin is `95017c711e6afc1085133d440e42b4bd78155701`.
Its `GCToEEInterface::GetIntConfigValue` reads the private configuration key through
`RhConfig::ReadConfigValue`, whose default `decimal=false` selects hexadecimal.
The environment parser does not accept a `0x` prefix. Source references:
[GC configuration dispatch](https://github.com/dotnet/dotnet/blob/95017c711e6afc1085133d440e42b4bd78155701/src/runtime/src/coreclr/nativeaot/Runtime/gcenv.ee.cpp),
[parser](https://github.com/dotnet/dotnet/blob/95017c711e6afc1085133d440e42b4bd78155701/src/runtime/src/coreclr/nativeaot/Runtime/RhConfig.cpp),
[default radix](https://github.com/dotnet/dotnet/blob/95017c711e6afc1085133d440e42b4bd78155701/src/runtime/src/coreclr/nativeaot/Runtime/RhConfig.h).

The unchanged archived oracle parses the real READY port, performs `/health` and
`/stop`, compares exact request/response bytes, and requires STOPPED, exit zero,
and empty stderr. The new wrapper independently compares HTTP artifact hashes to
the original passing receipt. It records full strace output, completed mmap
requests and sizes, PROT_NONE reservation requests, observed thread IDs and
clone/futex rows. Unfinished mmap rows remain explicitly visible; parsed sizes
are not asserted to be GC allocations, live memory totals or committed bytes.

Each attempt lives under `blink/artifacts/dotnet-guest-gc-profile/`, retaining
failure evidence. Before/after checks cover the binary, archived service/build
sources, source receipt, both runners, Python and strace executable files. Shared
libraries and the host kernel are not a hermetic closure. The oracle has bounded
readiness/HTTP waits and the wrapper adds a 60-second execution alarm. Cleanup
sends TERM then KILL to the owned process group and checks that it disappeared;
the kernel cannot promise bounded completion for unkillable processes. No timeout
or failure injection is part of this witness.

A pass establishes this frozen native service's HTTP behavior under the recorded
configuration. It does not establish translated startup, a 64 MiB whole-process
bound, thread support, or `mremap` support. Any later translated comparison must
use the same four-entry guest environment. Implementation proposals remain
conditional on the next actual translated trace.

Observed result: `blink/artifacts/dotnet-guest-gc-profile/attempt-lp8kf_q3/receipt.json`,
SHA256 `f145da71d5032421fdd40bd368d248cad4918065b0b07ee545e32772fab7bb55`.
Both exact HTTP comparisons passed; the process printed READY then STOPPED,
returned zero, and wrote no stderr. Cleanup found the owned group gone without
sending signals. All 30 recorded input/artifact hashes were independently checked
after completion. The executable and runner remained unchanged.

The 340-line trace has SHA256
`1e687dddd89a8565e178d4ad7415ec825d001dd7258120be7aac518f87c050ac`.
It includes a successful 33,558,528-byte PROT_NONE request (32 MiB plus 4 KiB),
three successful helper-thread clones, and four observed thread IDs. One
16,384-byte mmap is split across unfinished/resumed lines and is therefore absent
from the completed-single-line mapping summary; its evidence remains in the full
trace. The configured native service thus completed its normal HTTP lifecycle
with a much smaller largest observed reservation than the original unconfigured
trace. This observation does not measure total live or committed memory and does
not remove the translated runtime's actual clone/threading gap.
