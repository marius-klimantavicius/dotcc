# Actual backend host services

This owner implements missing host contracts observed in the complete 876-object
backend selection. The bounded host gate, including directory durability,
passes native, emitted JIT and normally trimmed Linux x64 NativeAOT on immutable
1852. Complete backend linking/building, initialization,
SQL/error recovery, persistence and the product matrix remain supervisor-owned
acceptance work. No SQL product completion is inferred from this host fixture.

The subsequent coherent `20261022-191828-3a8ebaa0` campaign emitted all 876
objects and passed strict linking. Its first raw build exposed missing header
declarations and compiler errors; it did not expose another missing OS service
method. The focused header fixes below supplement the frozen host runtime.

## Selection and ownership

The readonly original source is
`build/native-control/14abb02f16cfb8013f7c/source`. Its original callers include
`utils/init/miscinit.c`, `utils/misc/stack_depth.c`, `storage/file/fd.c`,
`storage/file/copydir.c`, `libpq/ifaddr.c`, `utils/adt/float.c`,
`utils/adt/genfile.c`, `utils/error/{elog,assert}.c` and the original portable
directory-creation code. Their database algorithms and reviewed configuration
selection remain unchanged.

New runtime implementations are in `DotCC.Libc/PosixBackend*Lib.cs`. The
supervisor explicitly granted only the existing `UnsupportedPosixLib.cs`
umask/getrlimit routes and `PosixFsLib.cs` OpenLinuxStream/mkdir hooks to this
owner. The supervisor supplied shared header declarations and the
`DisposeBackendHostServices(this)` hook before descriptor/heap disposal. Other
runtime/compiler/initializer/recipe edits retain their existing owners. There
are no generated-source repairs, child commits or staging operations.

The immutable 1812 run emitted all 876 objects but failed strict aggregate
linking. Its external import-candidate metadata has no unresolved names once
definitions are combined; that is insufficient to audit runtime calls, because
synthetic-header declarations are deliberately excluded from native import
candidates. A separate bare-call/runtime audit exposed the missing host names
implemented here. The supervisor's 1830 regeneration carries the later aggregate
repair and these frozen runtime resources. Full raw-build diagnostics remain the
authoritative check for any additional missing names or signature mismatches.

## Runtime contracts

`PosixBackendProcessLib.cs` supplies an owner-local creation mask and the real
Linux x64 LP64 getrlimit query. Each owner's initial mask is captured lazily from
the read-only `Umask` entry in `/proc/self/status`. umask exchanges only its low
nine permission bits, returns the previous mask, and preserves guest errno on
success. It never calls process-global native umask. Owners preserve independent
values across nested bindings and fresh lifetimes. Unavailable procfs is an
explicit failure. `EffectiveCreationMask` and `ApplyCreationMode(uint)` are the
shared internal creation hooks. Explicit chmod/fchmod and existing objects never
apply this mask. getrlimit passes a private 16-byte result to the real host query
and copies it to caller storage only on success; it never changes process limits.
setrlimit retains its explicit unsupported contract.

`PosixBackendStorageLib.cs` applies exact guest permissions to newly created
Linux files, including ordinary open and stdio opens through OpenLinuxStream.
It first attempts native exclusive creation; only a descriptor proven newly
created receives fchmod. For existing objects it retries without O_CREAT, so
neither normal creation nor truncation changes existing permissions. A dangling
final symlink is resolved after the existing-object open reports ENOENT; bounded
retries and a 40-link bound retain actual exclusive/nofollow/cycle errors. Guest
umask zero can create less restrictive permissions than the host's mask without
changing that host mask.

Linux mkdir uses actual atomic mkdir rather than recursively creating ancestors.
Missing parents, existing files/directories and existing symlinks retain real
errors. An O_PATH/O_DIRECTORY/O_NOFOLLOW descriptor anchors the created directory;
chmod through `/proc/self/fd` applies exact permissions even for mode 0000 and
does not follow a later symlink replacement. Failures to apply permissions are
errors. The initial serialized-owner scope does not promise protection against
hostile concurrent directory renames during the mkdir/open interval.

posix_fallocate reserves actual filesystem space through the owner's retained
SafeFileHandle. It returns numeric errors directly, preserves guest errno,
rejects invalid ranges and access modes, preserves the file cursor and existing
data, and leaves unsupported filesystem behavior to libc. It never substitutes
truncation or zero writes. fdatasync flushes owned buffers and calls real Linux
fdatasync. fseeko uses the existing 64-bit stdio seek after validating its origin.
copy_file_range uses the real kernel/filesystem service; explicit offsets preserve
stream cursors, and null offsets advance the FileStream logical cursors. Native
file numbers are never confused with owner-local descriptors. Unsupported host
platforms and missing symbols fail explicitly.

`PosixBackendTextLib.cs` supplies isascii and implicit-locale wide scalar
classification/case mapping using the existing owner's global/thread locale and
validated native scalar service. C/POSIX remains ASCII; C.UTF-8 supports Unicode
scalars. No native wide strings or host locale mutations cross the boundary.
strtof uses an immutable explicit C numeric locale and real binary32 parsing,
including native errno, signed zero, hex forms and end-pointer rules. This avoids
the double rounding from converting an intermediate binary64 result.

`PosixBackendMathLib.cs` supplies scalar lgamma/tgamma through the system math
service, following the existing shared erf/erfc boundary. lgamma_r avoids
modifying the host's signgam. Original PostgreSQL SQL/math/error logic stays
translated. These imports are libc/libm primitives, never PostgreSQL symbols.

`PosixBackendNetworkLib.cs` returns real Linux LP64 getifaddrs lists, preserving
all native records, addresses, flags, union aliases and opaque ancillary data.
Only the allocating owner can free a list. A native-list registry rejects foreign
or double release and drains abandoned lists during owner disposal. Guest output
storage is cleared before a failed query, and null output is EFAULT.

`PosixBackendDiagnosticsLib.cs` captures and symbolizes actual native host stack
addresses. A JIT's managed frames may be absent from the system unwinder; no
synthetic database frames are invented. backtrace_symbols copies native strings
into one guest malloc block and immediately releases native allocation, retaining
the ordinary guest free/owner-disposal contract. backtrace_symbols_fd writes
through actual guest descriptor services, handling partial writes and EINTR.
closelog retains the shared syslog service's explicit ENOTSUP contract; this
profile supplies no fake host logging registration.

## Evidence and reproduction

### First complete raw-build header closure

The first complete raw build's five non-`__builtin_unreachable` missing-name
occurrences were two `SIGURG` uses, one `IN6_IS_ADDR_UNSPECIFIED` use and two
`__builtin_frame_address` uses. Only one signal constant was missing. Native
Linux compilation confirms `SIGURG == 23`; its declaration is now in signal.h.
The supervisor supplied an IPv6 unspecified-address macro backed by a real
16-byte, single-evaluation inline predicate. The supervisor owns the reviewed
stack capability fallback and the remaining compiler diagnostics.

Five shmctl signature errors were calls with NULL: the original pgl_shmctl
definition takes `struct shmid_ds *`, but the synthetic sys/shm.h lacked a
prototype, leaving the calling translation units with implicit function typing.
The same missing shmat prototype caused an implicit return-type mismatch in
the original DSM caller's `char *` assignment. Supervisor-added declarations
guarded by `__PGLITE__` and the corresponding remap macro bind these calls to
the existing original C functions. They add no native System V IPC import.

`artifacts/backendhost-owner-20261022-193521-12c105c0/receipt.json` passes all nine
commands: native C, two actual object emissions, strict link, JIT and normally
trimmed Linux x64 NativeAOT. It uses immutable 1908 with copied, hashed current
signal.h, netinet/in.h and sys/shm.h headers, and records fixture and compiler
closure hashes. The fixture compares signal number
and native shmid_ds size/alignment/offsets, exercises signal-set operations,
checks zero IPv6 addresses and every byte's low/high bit, and verifies that the
macro evaluates its operand once. Separate test-only remapped shmctl/shmat
definitions exercise NULL, a void-pointer variable, typed structure storage and
implicit void-pointer return conversion through two fresh generated owners.
Those definitions are fixture callees, never production host services.

Focused original `387-pg_shmem` and `449-dsm_impl` emissions also pass using the
exact 191828 receipt flags with only the current sys/shm.h overlay and private
outputs. Their report is
`artifacts/backendhost-owner-actual-shm-20261022-193251-3c00c368/report.json`.
All five original NULL calls now emit typed null arguments; the DSM shmat
assignment emits its required byte-pointer cast. No full backend build or
whole-frontend emission was repeated. The runtime remains frozen at 43ff500f5;
full backend build/execution and SQL acceptance remain open.

`run_header_compat(ctx, suite=None)` runs this focused gate independently:

```bash
python3 -B pglite/scripts/backendhost-probe.py \
  --compiler /home/marius/p/dotcc/pglite/build/tools/supervisor-20261022-1908/dotcc.dll \
  --headers-only --mode all
```

### Directory durability extension

The continuing actual-source audit found that the former directory-open EISDIR
failure caused original `fsync_fname`/`fsync_fname_ext` to take their portable
unsupported-platform success path. That is insufficient for the selected Linux
durability contract. `PosixBackendDirectoryLib.cs` now adopts actual native
directory descriptors without passing them to FileStream. It reuses the
existing captured shared-stream/refcount backing for dup/dup2 and disposal;
the final close releases the original SafeFileHandle. FileSlot distinguishes
directories, including when duplicated onto a standard descriptor. Native fstat
is mapped into the existing 96-byte guest stat layout; fsync and lseek call the
real Linux services on the retained handle. Read returns actual directory EISDIR,
write on the read-only descriptor returns EBADF, and stdio reading sets its error
indicator. Existing file duplication/offset/status behavior remains covered.

Additional explicitly coordinated existing-file hooks are in FileLib.cs,
PosixLib.cs, FileDurabilityLib.cs and DescriptorDupLib.cs. The first old
FileDurabilityTests case now checks the real Linux capability; its write failures
and other-platform capability expectations remain. No headers changed.

`directory-jit.log` passes 809 direct assertions and `directory-aot-run.log`
passes 816, with the same native-stack count variation described below.
`directory-regressions.log` passes all 45 existing filesystem, metadata,
durability, owned-I/O, stdio and three file-duplication cases through a private
runtime-only test project. No compiler project is built. New cases compare real
native directory inode/device/mode/size and read/write/seek errors, preserve
metadata canaries, verify owner separation and retained native handles after
close/rename/unlink, check dup/dup2 standard-target lifetimes, and prove owner
disposal closes captured directory backings without disturbing the ambient owner.
The first directory harness attempt omitted directory-target links by using
EnumerateFiles on procfs; corrected discovery uses EnumerateFileSystemEntries
and native readlink. Its diagnostic logs remain retained.

`durable-rename-native-{build,run}.log` passes the same-revision original complete
frontend `common/file_utils.c` from the readonly native archives, including
fsync_fname, fsync_parent_path and replacement/absent-target durable_rename.
`run_durable_rename(ctx, objects, source, mode)` in the owned helper relinks
the existing complete original frontend objects with current runtime resources
and executes those same original functions through two generated owners in
JIT/NativeAOT. It performs no frontend C re-emission. Optional strace evidence
records actual directory fsyncs and renames. This is a host-boundary diagnostic;
fresh final frontend regeneration and initialization/SQL acceptance remain open.

The current combined receipt is
`artifacts/backendhost-owner-20261022-185240-6d6c9391/receipt.json`: all 14 commands
pass with immutable `build/tools/supervisor-20261022-1852/dotcc.dll`.
`backendhost/report.json` records the exact compiler closure and seven new
runtime/header source identities. Native and untouched emitted C pass the host
and directory contract through JIT and normally trimmed NativeAOT. Each managed
mode uses two fresh generated owners and compares all 18 native observations.
`durable-rename/report.json` records the complete 75-object frontend input set
(73 original sources plus original fork libc and authored adapter) and its exact
hashes. That resource-only relink builds and runs the original complete
frontend fsync/rename functions in both managed modes. Each mode's syscall log
records 20 successful real fsyncs, including 10 directory fsyncs, and four
successful native renames. The diagnostic reuses 1820 C objects with 1852 runtime
resources; it does not establish fresh final frontend source regeneration.
`artifacts/backendhost-owner/resource-freeze-1852.json` independently reads the
immutable compiler's embedded manifest resources and verifies 173 runtime/header
resource byte hashes against current authored sources, including all seven new
backend-host files and the coordinated existing runtime/header hooks. The
verification uses a private metadata utility, with no compiler build. The
runtime is frozen at those tested source bytes.

```bash
dotnet test pglite/tests/BackendHost/Regressions.csproj -c Release --nologo \
  -m:1 --artifacts-path pglite/build/backendhost-owner/directory-regressions
python3 -B pglite/scripts/backendhost-probe.py \
  --compiler /home/marius/p/dotcc/pglite/build/tools/supervisor-20261022-1852/dotcc.dll \
  --mode all \
  --frontend-objects /home/marius/p/dotcc/pglite/build/initdb-owner-20261022-r2/frontend-1820/initialization \
  --source /home/marius/p/dotcc/pglite/build/native-control/14abb02f16cfb8013f7c/source
```

### Initial 1830 host gate

Private direct-runtime results under `artifacts/backendhost-owner/`:

- `runtime-jit.log`: 742 assertions pass.
- `runtime-aot-publish.log` and `runtime-aot-run.log`: normally trimmed NativeAOT
  publishes and passes 749 assertions. The count differs with real native stack
  frame count; the assertions and deterministic cases are identical.
- `runtime-jit-expanded-first.log`: retained failed harness expectation that a
  negative copy offset returns EINVAL; Linux actually returns EOVERFLOW. The
  runtime already forwarded the real error; the assertion was corrected.

These assertions cover owner/host mask isolation, existing/new/truncated/stdio
permissions, dangling relative and absolute symlinks, exclusive/nofollow/cycle
errors, atomic mkdir and missing-parent behavior, mode 0000, real allocation and
unchanged bytes/cursors, descriptor access/lifetime failures, copy offsets/data,
large seeks, all 16 real resource limits with canaries, affected scalar locale
wrappers, exact float/gamma bits and errors, native interface contents and owner
cleanup, and stack-symbol allocation/guest output.

The actual C gate is
`artifacts/backendhost-owner-20261022-183538-0ef89268/receipt.json`, with detailed
`backendhost/report.json`. It uses immutable
`build/tools/supervisor-20261022-1830/dotcc.dll`. Native compilation uses
`-Wall -Wextra -Werror`; emitted source is untouched. Both emitted JIT and
normally trimmed Linux x64 NativeAOT execute two fresh generated owners and
compare all 18 native observations, including real ifaddrs storage/offsets,
rlimit values, exact file/stdio/directory permissions, a seek above 2^32 and
interface inventory size. Actual emitted C also asserts every new host family,
real copy/allocation/durability, errno, float rounding, Unicode scalars and native
stack symbol ownership. The earlier 183454 receipt remains failed: native and
emission passed, but a harness project-name collision prevented its C# build.

```bash
dotnet run --project pglite/tests/BackendHost/BackendHost.csproj -c Release \
  --artifacts-path pglite/build/backendhost-owner/runtime -m:1
dotnet publish pglite/tests/BackendHost/BackendHost.csproj -c Release -m:1 \
  -r linux-x64 -p:PublishAot=true \
  -o pglite/build/backendhost-owner/runtime-aot \
  --artifacts-path pglite/build/backendhost-owner/runtime-aot-artifacts
pglite/build/backendhost-owner/runtime-aot/BackendHost
python3 -B pglite/scripts/backendhost-probe.py \
  --compiler /home/marius/p/dotcc/pglite/build/tools/supervisor-20261022-1830/dotcc.dll \
  --mode all
```

The direct harness compiles runtime sources privately and never builds the shared
compiler. `scripts/backendhost-probe.py` exports `run(ctx, suite=None)` for normal
recipe registration. It uses Context logging, shared link_flags, exact authored
header copies and private build/artifact paths; records selected compiler,
runtime/header identities and observations; and performs no acquisition,
whole-backend emission or publication. Standalone invocation preserves the normal
campaign's latest-attempt pointer. Native service controls are independent
evidence, not a delivered native database backend.
