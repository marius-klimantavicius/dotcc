# libsmb2 translation campaign

The [plan](docs/PLAN.md) is being implemented. The pinned native client already
passes authenticated file operations against an isolated Samba server. The full
managed translation pipeline now produces buildable raw and post-processed C#;
the managed sample and lifecycle suites pass against Samba under JIT and NativeAOT.
Full plan acceptance remains open. Fault injection follows the explicit
[upstream test scope](docs/test-scope.md).

The [Kerberos and DFS implementation](docs/enterprise-client.md) has been merged
from the supplied patch and adapted to the async facade. Enterprise qualification
on Windows and Linux remains open; see [review results](docs/kerberos-dfs-review.md).
The [async transport](docs/async-transport.md) uses completion-driven BCL sockets,
fd callbacks and a four-byte authored socket handle. The separate
`scripts/translate-libc.sh` retains the Libc/poll baseline.

```sh
./libsmb2/scripts/fetch.sh
python3 libsmb2/scripts/probe-parse.py
./libsmb2/scripts/oracle.sh
./libsmb2/scripts/translate.sh
```

Run from the repository root or invoke these scripts by absolute path from any
directory. Prerequisites: Python 3.12+, .NET 10 SDK, CMake and a native C compiler.
The native oracle also requires Docker. Source acquisition/tool restore and the
first oracle image build require network access.

`translate.sh` is the full source-to-product entry point: verified fetch, tool
builds, all-unit emission/linking, raw build, semantic postprocessing, and final
build. It only promotes successful output to `generated/TranslatedLibsmb2/`;
raw comparison output goes to `generated/TranslatedLibsmb2.Raw/`. While blockers
remain it exits nonzero and preserves diagnostics instead of publishing a partial
product. `build.sh` builds an already generated product without translating.

See [configuration](docs/configuration.md), [provenance](docs/source.md), and
[validation](docs/validation.md) for current capabilities and evidence. The
`ManagedConsumer.slnx` builds and the sample performs real managed file operations;
see [usage](docs/usage.md) and [API contracts](docs/api.md).
