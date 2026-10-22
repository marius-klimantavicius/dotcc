# POSIX shared memory, terminal attributes and caller-owned NSS

This bounded implementation supplies real OS services needed by the actual
initializer and backend callers. It does not qualify PostgreSQL initialization,
SQL, persistence or the full product matrix. The supervisor retains P6 and
complete integration. No upstream source, emitted C#, source-acquisition policy,
shared recipe or parent-owned plan is changed here.

## Runtime contracts

`DotCC.Libc/PglitePosixHostLib.cs` implements `shm_open`, `shm_unlink`,
`tcgetattr` and `tcsetattr` for Linux x64 LP64. Other platforms return ENOTSUP
for supported arguments. These are libc OS-service imports, with no native
database backend or replacement SQL implementation.

Shared memory uses native libc `shm_open` and `shm_unlink`. The newly opened
native descriptor transfers to an owning SafeFileHandle and FileStream, then
to the current runtime owner's existing descriptor table. Only the table index
is returned to C. Existing `ftruncate`, positioned I/O, `close` and `mmap`
therefore use the same owner and actual file handle; no raw native descriptor
is mistaken for a guest descriptor. The existing mapping service retains its
own owner and unmapping lifetime.

Portable names must start with one slash, contain a nonempty single component
other than `.` or `..`, and have at most 255 component bytes. Invalid names
return EINVAL, overlong names ENAMETOOLONG, and NULL EFAULT. No normalization or
UTF-8 roundtrip changes the native name bytes. Supported flags are O_RDONLY or
O_RDWR, optionally O_CREAT, O_EXCL, O_TRUNC and O_CLOEXEC; other access/flag
values return EINVAL. This deliberately does not advertise Linux's additional
non-POSIX open extensions. Native creation supplies atomic exclusivity, real
permissions filtered by the host umask, existing-object semantics and actual
truncation. Even without O_CLOEXEC, successful shm_open descriptors report
FD_CLOEXEC, matching the native service. Native failures retain their errno.

The POSIX shared namespace is kernel-wide, while descriptors belong to runtime
owners. Unlink removes the name without invalidating existing descriptors or
mappings. Closing a descriptor does not unmap an existing mapping. Owner
disposal closes its real handles; it does not silently unlink persistent names
owned by the translated application's explicit lifetime protocol.

Terminal operations accept the actual emitted struct pointer as `void*`.
Linux x64 libc's 60-byte, 4-aligned termios is passed unchanged. File-backed
slots use their owning SafeFileHandle; marshalling retains it across the native
call. Only the owner's actual In/Out/Err console kinds at descriptors 0/1/2
use OS standard descriptors. A reopened standard file uses its FileStream,
and an owned managed pipe/socket returns ENOTTY. Ordinary files and nonterminal
devices receive the real native ENOTTY. Unknown descriptors return EBADF,
NULL storage EFAULT, and native action/terminal failures retain native errno.
All three standard tcsetattr actions are served by libc. No host signal action,
process terminal policy, fake terminal success or extra termios API is added.
The existing unrelated prototypes in termios.h remain separate capabilities.

`DotCC.Libc/PasswdLib.cs` adds `getpwuid_r` using its existing private
`PosixGetpwuidR` reentrant NSS service. `pwd.h` adds the matching prototype.
The caller owns the record, buffer and result-pointer storage. The runtime
accepts both generated struct pointers and the generated struct** result
address as `void*`, performing the native ABI cast inside the runtime. UID/GID
remain unsigned 32-bit; size_t and pointers remain 64-bit. The numeric error is
returned directly, including ERANGE and NSS provider errors, without replacing
the guest errno. A native 0,NULL no-entry stays exactly 0,NULL. NULL result,
record or buffer is diagnosed as numeric EFAULT. The runtime does not copy,
relocate, free or assume ownership of native NSS string pointers; the caller
must supply valid, sufficiently sized storage and observe the native borrow
contract, including any provider-specific foreign storage.

## Actual callers and layout evidence

The readonly configured source is
`build/native-control/14abb02f16cfb8013f7c/source` at the selected campaign pin.
The required callers remain unchanged:

- `src/bin/initdb/initdb.c` creates/exclusively probes and unlinks a real shared
  memory object while choosing the upstream default implementation.
- `src/backend/storage/ipc/dsm_impl.c` implements the real PostgreSQL shared
  memory state machine using shm_open/unlink, ftruncate and mmap.
- `src/common/sprompt.c` saves attributes, disables ECHO and restores them.
- `src/port/path.c` supplies its own passwd record, 1024-byte buffer and typed
  struct** result pointer. `src/backend/libpq/auth.c` has another such caller.

`tests/PosixHost/posix_host.c` is compiled natively against actual system
headers and emitted against the authored shared headers. Twenty-four size,
alignment and field-offset values match, including actual CLR record storage
and field addresses independently of emitted offsetof metadata. The measured
passwd size/alignment is 48/8, with fields at 0,8,16,20,24,32,40. Termios is
60/4, with fields at 0,4,8,12,16,17,52,56. UID/GID are 4 bytes, size_t 8,
cc_t 1 and speed_t 4.

Both real native and emitted C exercise exclusive creation, CLOEXEC,
ftruncate, writable shared mappings, cross-descriptor visibility, read-only
mapping rejection, unlink/close/mapping lifetimes, recreation and truncation.
They exercise ERANGE with NULL result, real caller-buffer NSS lookups and a
native/emitted uid/gid/name/home/shell digest. Typed struct** passes through the
actual generated call and C# compilation, not a mirror-signature assumption.

The default host NSS returns numeric ENOENT and NULL for uid UINT32_MAX.
That exact result is compared rather than normalized to success. A separate
native process and each separate managed probe process select the real libc
`files` NSS provider with test-only `__nss_configure_lookup`, demonstrating
actual 0,NULL no-entry. No provider is stubbed. This import exists only in the
test harness, never the product runtime. It changes no configuration file or
parent process; each private child exits after its test. Both default and files
provider observations remain in separate JSON reports.

Real PTYs are available on this host. Native, emitted JIT and normally trimmed
NativeAOT each roundtrip ECHO and VEOF through all three actions, check remaining
flags/speeds, verify invalid-action EINVAL and restore the original state in
cleanup. Slave opens use O_NOCTTY, so the private session leader never acquires
a controlling terminal. Missing PTY creation is recorded as unavailable rather
than a successful roundtrip; this run has no such omission.

Focused authored-runtime tests additionally verify native descriptor numbers
are distinct from returned guest indices, actual handle closure on owner
disposal, unknown/foreign-owner errors, equal numeric descriptors in two owners
accessing different objects, zero-mode permissions, real file/pipe errors,
genuine console descriptor reads, and reopened owner stdin routing. NSS tests
check record and buffer canaries, local-provider string pointer ranges,
independence from borrowed/static lookup storage and numeric EFAULT without
changing guest errno. They do not generalize the local-provider pointer range
observation to every possible NSS module.

## Reproduction and current results

All output paths begin with `posixhost-owner`. The standalone runner uses the
shared campaign Context logger and cancellation. It neither acquires/prepares
sources nor builds compiler tools, changes campaign pointers, publishes product
output or edits emitted files. Exact authored headers are copied into a private
include directory and hashed; the runtime comes only from the selected immutable
compiler. All managed builds use private --artifacts-path output and one MSBuild
node. The optional --frontend calls the existing initializer helper against the
same readonly stage, retaining its normal complete-source/build evidence.

```bash
dotnet test DotCC.Tests/DotCC.Tests.csproj -c Release -m:1 \
  --artifacts-path pglite/build/posixhost-owner-unit \
  --filter 'FullyQualifiedName~PglitePosixHostTests|FullyQualifiedName~PglitePosixIoTests|FullyQualifiedName~LibcPosixFsTests|FullyQualifiedName~PosixMetadataTypeTests'
python3 -B pglite/scripts/posixhost-probe.py \
  --compiler /home/marius/p/dotcc/pglite/build/tools/supervisor-20261022-1802/dotcc.dll \
  --source /home/marius/p/dotcc/pglite/build/native-control/14abb02f16cfb8013f7c/source \
  --mode all --frontend
```

The selected current-runtime unit/regression run passes **36 cases, zero
failures/skips** (18 owned host cases plus 18 existing filesystem/I/O/metadata
checks). Log: `artifacts/posixhost-owner-unit/regressions.log`.

Current native/emitted evidence is retained under
`artifacts/posixhost-owner-20261022-175642-ce6c4267/`. Native, JIT and normally
trimmed Linux x64 NativeAOT pass the actual layout, shared memory, NSS and PTY
assertions, with both NSS-provider modes. All four actual caller source retries
emit without unresolved-header diagnostics. The receipt and command logs retain
the selected immutable compiler closure, exact argv and source hashes.
The complete frontend retry in the same run emits all **73 selected actual
frontend units**, plus the complete libc and authored adapter, and links them.
Its original option-parser/static-provider contracts pass native and emitted
execution. The subsequent C# build fails with **11 CS1503 diagnostics** on
`strlcpy` argument 3: ten int-to-ulong and one long-to-ulong conversion. This
leaves the overall receipt **failed**, with
`host_status = passed-native-emitted-all` preserving the separate host result.
No remaining diagnostic names one of this task's five host functions.

The first error is `initialization/frontend/Initdb.00001.cs:2106`, from actual
`src/common/exec.c:204`:
`strlcpy(retpath, startp, Min(endp - startp + 1, MAXPGPATH))`.
The emitted argument remains a signed long rather than converting to size_t.
The int cases are in the actual fe-cancel.c, fe-connect.c and common/ip.c
callers. The translated original `src/port/strlcpy.c` definition is present
as an ordinary `ulong strlcpy(byte*, byte*, ulong)` method in
`Initdb.00005.cs:4146`. This is a shared call-conversion/compiler integration
request; it is outside the owned host files. No generated C# or upstream
call is changed to hide it.

Exact source/link commands are in that run's receipt.json; the direct failed
build reproducer from the repository root is:

```bash
dotnet build pglite/build/posixhost-owner-20261022-175642-ce6c4267/initialization/frontend/frontend.csproj \
  -c Release --nologo -m:1 \
  --artifacts-path pglite/build/posixhost-owner-20261022-175642-ce6c4267/initialization/managed
```

The final pwd.h comment clarifies that _r storage is caller-owned; declaration
and layout are identical to the tested snapshot. Final receipt
`artifacts/posixhost-owner-20261022-180105-3624cc82/receipt.json` **passes the
bounded host experiment**, using the exact final authored header bytes and the
same immutable 1802 compiler. All 14 commands pass: real native controls,
emitted JIT/NativeAOT execution in both NSS modes and four actual caller retries.
This host-only receipt does not repeat or supersede the failed complete
frontend build in the earlier combined receipt.

Earlier receipts are retained as failed harness attempts, not successes:
the first used the address of a C# fixed-buffer pointer expression instead of
the buffer storage; the next PTY slave open lacked O_NOCTTY and received SIGHUP
at master close; the first AOT run used reflection-based test JSON serialization.
The authored harness fixes use the actual fixed-buffer address, O_NOCTTY and
Utf8JsonWriter. No emitted source was changed. The initial unknown-UID unit
assumption was likewise corrected to compare the actual NSS numeric return.

## Supervisor integration

The runtime part and both headers are embedded automatically by the existing
resource glob on the next supervisor compiler build. No descriptor-disposal,
mapping-disposal, recipe or initializer-owned runtime hook is needed. Existing
PglitePosixIoLib.cs, VectorIoLib.cs, PgliteSignalTimerLib.cs, SetJmpLib.cs and
setjmp.h are untouched. Preserve the parent-owned shared plans and PLAN.md.

Owned changes are exactly these ten authored files:

- `DotCC.Libc/PglitePosixHostLib.cs` (new) and `DotCC.Libc/PasswdLib.cs`.
- `DotCC.Lib/include/pwd.h` and `DotCC.Lib/include/termios.h`.
- `DotCC.Tests/PglitePosixHostTests.cs` (new).
- `pglite/tests/PosixHost/{posix_host.c,Program.cs,PosixHost.csproj}` (new).
- `pglite/scripts/posixhost-probe.py` and `pglite/docs/POSIX-HOST.md` (new).

Rebuild/regenerate final combined products with the final compiler resources,
repair the shared strlcpy argument conversions and repeat the complete frontend
build, then retain complete backend/frontend import closure, actual initialization,
SQL/error recovery, clean reopen and the required raw/processed JIT/NativeAOT
product matrix as supervisor acceptance work. Passing this host experiment or
the complete frontend C# build alone closes none of those required product gates.


Supervisor continuation: the 11 historical strlcpy diagnostics were reduced to
missing declarations, not a demonstrated declared-call conversion defect. The
profile had advertised HAVE_DECL_STRLCPY/STRLCAT despite absent system prototypes.
Selecting the unchanged upstream port.h declarations repairs them. Full original
73-unit frontend now emits, links and builds on snapshots 1812/1820; original
initdb option/exit tests also execute. See INITIALIZATION.md for those later
receipts. Actual backend/initializer SQL acceptance remains open.
