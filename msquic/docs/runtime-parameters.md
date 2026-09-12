# Managed runtime parameters

The selected runtime methods pass the coherent owning API campaign in all four
raw/optimized × JIT/NativeAOT forms. The regenerated core compiles the pinned
`VER_GIT_HASH`; `GetLibrarySourceRevision()` returns those actual bytes and each
test mode verifies the pin. The facade does not substitute a provenance string.
See [API coverage](api-coverage.md) for the mapped controls and their limits.

`QuicRuntime` exposes typed operations for the selected global parameters.
Queries return values from the translated core. They do not turn compiled fields
or zero counters into capability claims. Every operation holds a runtime lifetime
lease; shutdown waits for admitted operations before closing the native-shaped API
table. Settings mutations are serialized with their effective-value preflight.

| API | Meaning |
| --- | --- |
| `GetParameters` / `SetParameters` | Partial Retry threshold and disabled load balancing updates through `QUIC_GLOBAL_SETTINGS`. |
| `GetRetryMemoryLimit` / `SetRetryMemoryLimit` | The native 16-bit fraction of host memory used for the Retry threshold. Zero forces Retry; 65535 means 100 percent. |
| `GetLoadBalancingMode` / `SetLoadBalancingMode` | Only disabled load balancing is selected. Other enum values fail before dispatch. |
| `GetSettings` / `SetSettings` | Effective QUIC settings and atomic validation of updates, using the actual translated settings implementation. |
| `GetVersionPolicy` / `SetVersionPolicy` | Effective acceptable/offered/fully-deployed lists, each restricted to one QUIC v1 entry. |
| `GetCompiledProtocolVersions` | Actual compiled upstream version list, decoded to host numeric order. It can include versions outside the selected effective policy. |
| `GetLibraryVersion` / `GetLibrarySourceRevision` | Actual pinned upstream version and revision, distinct from facade package version. |
| `GetTlsProvider` | Explicit picotls provider identity `0x10000`. |
| `GetPerformanceCounters` | A snapshot indexed by `QuicPerformanceCounter`, retaining the upstream signed counter representation. |
| `GetStatisticsV2Sizes` | The compiled sizes of supported statistics structure revisions. |
| `ProvisionStatelessResetKey` | Copy and dispatch a 32-byte key after lazy initialization; no secret getter. |
| `ConfigureStatelessRetry` | Configure AES-128 or AES-256 Retry secret and a nonzero rotation interval; no secret getter. |

`QuicRuntimeOptions.Parameters` initializes the Retry threshold and load balancing
policy. Unsupported values are rejected before host installation. The reset key
is an explicit operation after opening a registration, because the unchanged core
rejects provisioning before lazy initialization. Applications should provision it
before admitting connections; changes follow the upstream key/token behavior.

Retry secrets require 16 bytes for AES-128 or 32 bytes for AES-256. Reset and Retry
setters copy their input to bounded temporary storage, call the translated setter
synchronously, and clear their temporary copy. They retain no managed reference
to the caller's key buffer. Active Retry keys follow the core's rotation policy.

Global queries and setters execute inline in the upstream API. The connection
parameter priority modifier does not create a scheduling benefit for these calls.
The facade exposes no arbitrary global parameter ID, external worker configuration,
XDP map, native provider, fixed server ID, or secret-bearing DEBUG query.
