# Test-profile fork hook controls

The consumer references the separately published `upstream-tests-static` PE
through the ordinary owning `Managed.CPython` API. `Program.cs` registers an
observer module, then executes `scenarios.py` twice, requiring six callback
owners to be finalized. The actual upstream Before/Parent hooks, `os.fork` and
`_posixsubprocess.fork_exec` run against the actual rejecting shared service.
The observer supplies no replacement lifecycle algorithm. Child execution is0.

`control.c` is the exact-pin native oracle. Only its native build defines
`FORK_PROBE_NATIVE` and links `--wrap=fork --wrap=vfork`; neither wrapper is in the
managed consumer or product. `libc_reject.c` independently checks actual shared
fork/exec rejection under direct/object JIT/NativeAOT.

`test_policy.py` checks profile resolution, immutable embedded header bytes,
complete unchanged source staging, roles/assets/sysconfig and rejection of
per-unit layout-feature overrides. `audit_features.py` binds every actual ordered
unit option/source/header, both layout-sensitive object signatures and sysconfig
to B's unchanged compiled75-initializer auditor. It does not run frozen tests.
`inspect_inputs.py` checks nonempty real hook algorithms/call edges in each exact
consumer compilation input, including each AOT ILC response file/native object.
These narrow checks do not constitute full D10, ABI, rooted or delivery proof.

Native `layout.c` must compile against DotCC's actual headers with `-nostdinc`,
unsigned16 wchar and DotCC compiler identity (`__GNUC__` unset). System pthread
handle layouts and GCC's packed tracemalloc branch produce different offsets.
`native_layout_syntax.h` supplies GCC parser bindings for DotCC builtin names;
all measured CPython types come from the real headers. No native wide/libpthread
service is called by this layout-only program.

The exact A8 commands, tools, actual input PE hashes/MVIDs, native build inputs,
failed harness attempts and scoped results are retained under
`artifacts/owner-a-r8-20261018`. Read `docs/fork-hook-profile.md` for the contract,
receipts, review boundaries and commands. Shared commands require per-command
`flock --close /tmp/dotcc-cpython-shared-build.lock`; native commands use the
separate native lock. Acquire campaign run locks through ordinary entrypoints.
The helper-profile specialist suite locks its own commands: do not wrap it in an
outer shared lock. A future build requires the coordinator's serial-slot grant.

For the new bootstrap-import policy, audit_features.py requires explicit
`--feature-stage bootstrap-imports` and the prior A8 fork-only published receipt.
It requires the exact three-feature addition, all protected selected/unselected
unit options and sysconfig, retaining only the two concrete upstream wide-service
undefs that leave an already absent feature absent. Compiled product layout and
helper readiness still require the newly generated actual products.
