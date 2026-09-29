# libuv translation campaign

Implementation of [the plan](docs/PLAN.md) is active. The required product is an
all-platform translation with runtime backend selection and direct native OS
calls. No translated libuv product or qualified managed workflow is available yet.
Normal translation explicitly rejects incomplete all-platform publication.

The pinned native baseline already runs a local TCP binary echo, asynchronous
file write/read/removal, localhost resolution, native worker execution,
cross-thread async notification and bounded timer with clean loop shutdown.
Native success does not establish translated behavior. The authored
[native identity/binding foundation](docs/native-foundation.md) also passes Linux
glibc x64 JIT/AOT checks, including real mixed-provider rejection.

From any working directory (substitute the repository path):

```bash
bash /path/to/dotcc/libuv/scripts/fetch.sh --fetch missing
bash /path/to/dotcc/libuv/scripts/test.sh --suite inventory --fetch never
bash /path/to/dotcc/libuv/scripts/test.sh --suite native-baseline --jobs 4 --fetch never
bash /path/to/dotcc/libuv/scripts/test.sh --suite native-upstream --jobs 4 --fetch never
bash /path/to/dotcc/libuv/scripts/test.sh --suite dispatch-foundation --mode all --rid linux-x64 --fetch never
```

Prerequisites: Python 3.11+, .NET 10 for translation and managed probes, CMake and
a native C toolchain for native controls. Native upstream tests have additional
case-specific filesystem, networking and terminal requirements. Scripts use the
shared campaign runner and ordinary copies; no administrator-mode staging is
required. The Linux native ABI oracle is native-only until compared against actual
emitted layouts. Real Windows/Darwin target headers and native compilation diagnostics are available
through the optional `sdk` source group and `foreign-headers` suite; foreign
execution runners and remaining SDKs remain open. Native-only suites acquire
only product source, keeping SDK downloads out of ordinary native controls.

See [validation evidence](docs/validation.md), [open blockers](docs/blockers.md),
[upstream test scope](docs/upstream-tests.md) and [coordination](docs/coordination.md).
Upstream licenses are preserved in `LICENSE.libuv` and `LICENSE-extra.libuv`.
