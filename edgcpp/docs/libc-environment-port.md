# Owner-local translated environment

The component passes native and all generated startup/ownership checks and is
ready for selected-provider registration.
Root owns `LibcEnvironment.cs`, the six-TU `runtime/musl/environment` component,
its fragment and fixtures. The coordinator owns import/service/driver integration.

Translate pinned musl1.2.6 `getenv`, `setenv`, `putenv`, `unsetenv`, `clearenv` and
their shared allocation tracking. A small sixth source owns the public `environ`
cell and initial startup function. Internal musl `__environ` spelling aliases
that cell in the private compatibility header; native platform-specific symbol
aliases are not part of this component's acceptance. Source adaptations provide
private declarations, replace native weak fallback definitions with the selected
strong `__env_rm_add`, spell C++ allocation casts, and rename C's variable `new`.
Original algorithms, duplicate-name rules and ownership behavior are retained.

`LibcEnvironment` has one narrow job: capture the .NET host environment into an
initial pinned UTF8 string arena and pinned pointer vector for each owner. It
never calls `Environment.SetEnvironmentVariable`. The byte-based translated
algorithms subsequently handle changes, including raw non-UTF8 values, borrowed
`putenv` strings, callers replacing `environ`, and removal of duplicate names.
`setenv` and resized vectors use translated `malloc/realloc/free`, hence the
existing owner NativeMemory heap. Borrowed strings remain caller-owned. Heap
shutdown reclaims outstanding allocations after translated destruction; snapshot
storage is released with its owner. Lookup performs no managed string conversion.

Initial managed host strings use strict UTF8 encoding. An embedded NUL or an
unrepresentable UTF16 surrogate in an explicit managed seed fails snapshot
construction instead of silently changing the data. The initial OS snapshot is
the BCL's managed environment view; preserving non-UTF8 Unix bytes already lost
at that boundary requires a separately audited native input adapter. Translated
byte strings supplied after startup are preserved exactly. Portable lookup is
case-sensitive on every OS, following the selected musl implementation.

The runtime's startup snapshot uses reserved implementation constructor priority
zero, before ordinary user priorities101..65535. An early user constructor probe
observes the inherited seed before main/export entry. Each engine captures its
initial environment independently; translated `setenv("TZ",...)` will therefore
provide owner-local timezone selection without modifying another engine or host.
Calendar timezone interpretation itself remains separate work.

Evidence retained:

- `libc-environment-native-002`: original/adapted native digest
  `3385434606866502156`, with20000 modeled mutations, explicit replacement of
  environ, duplicate names, borrowed pointer mutation and raw bytes.
- `libc-environment-runtime-003`: standalone JIT/NativeAOT stable pinned pointers,
  independent snapshots, strict encoding, zero-allocation warmed reads and
  disposal. The first two runs retained allocation-measurement failures from a
  cold test loop; measuring the warmed isolated non-inlined loop passes without
  changing runtime code.
- `libc-environment-translated-001`: native oracle plus all eight C/C++
  static/instance JIT/NativeAOT configurations. Includes inherited host snapshots,
  host/owner isolation, GC, borrowed global addresses, shared read-only thread
  access, and rejection after shutdown. This version predates the additional
  early-constructor test and must not substitute for that startup-order gate.
- `libc-environment-translated-002`: nine checks pass, including all eight
  generated configurations with the early-constructor gate. Each owner observes
  its initial seed at constructor priority101 after implementation priority0.
- `libc-environment-patch-replay-001`: all five original source patches replay
  with zero fuzz and match the pinned adapted hashes.

The native oracle provides a deterministic initial pointer vector and executes
only synthetic inputs. No real-world project verification is included.

As in the selected original musl implementation, concurrent read-only lookups
are supported; callers must serialize environment mutations. This component does
not invent stronger lifetime guarantees for pointers retained across mutation.
