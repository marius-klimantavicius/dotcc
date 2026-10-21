# Filesystem provider handoff — ready for integration/verification; HOLD

Assignment baseline: `abb239b200817d3fa9cf32888162cce9ef010271`.
This bounded M1 checkpoint does not complete the manual frontend campaign.
Ownership is limited to this handoff, `src/EdgSharp/FileSystemIncludes.cs`, and
`tests/EdgSharp.Tests/FileSystemIncludeTests.cs`. No builds, tests, native compilers,
Git mutations, or existing-file edits are performed by this worker.

Fresh supervisor-authorized continuation task:
`node:delegated-task:command%3Amcp%3Ade81650a-bdeb-45b8-baab-d4ba3b1e94cb%3Athread-send%3Amanual-edg-m1-filesystem-rid-fix-20261021-r1`.
The original filesystem-provider task is historical. This continuation briefly
released only these three files to repair the first executed host gate; a fresh
READY/HOLD acknowledgment below closes that release.

## Implemented public contract (early proposal accepted by supervisor)

- `IStructuredIncludeProvider : IIncludeProvider` adds
  `IncludeProbeResult Probe(string path)` and `IncludeReadResult Read(string path)`.
- Both result classes expose `Status : IncludeInputStatus` (`Found`, `Missing`,
  `Failure`), `CanonicalPath : string?`, and `Failure : IncludeInputFailure?`.
  Read results additionally expose `Text : string?` (non-null only on Found).
- `IncludeInputFailure` exposes `Kind : IncludeFailureKind` and `Message`.
  Kinds: `InvalidEncoding`, `UnsupportedEncoding`, `Access`, `Io`, `InvalidPath`,
  `OutsideAllowedRoots`, `LimitExceeded`, `UnsupportedHost`.
- `FileSystemIncludeProvider(string baseDirectory, IEnumerable<string> allowedRoots,
  int maxInputBytes = 16777216, int maxInputCharacters = 16777216)` requires an
  explicit fully qualified host base and at least one explicit fully qualified
  allowed root. Configuration is copied and immutable. Requests are individual
  searched paths; the provider never selects or reorders include directories.
- Compatibility `TryRead` returns false only for Missing; Failure throws the new
  typed `IncludeInputException : IOException`, exposing `Failure` and
  `CanonicalPath`. Primary should prefer the structured API and diagnose errors.

Presence probes use filesystem metadata only, never read bytes, select/decode an
encoding, tokenize, allocate SourceFile, or mutate preprocessing state. Thus an
existing malformed/oversized header is Found on Probe and Failure on Read.
Filesystem calls occur only on explicit Probe/Read/TryRead, never graph queries.

CanonicalPath means a lexically normalized absolute host path, resolving `.`
and `..`, preserving `/` as a root and removing other trailing separators.
Symlink/hardlink aliases are not unified; allowed roots constrain lexical path
names and are not a race-proof security sandbox. No environment/CWD lookup,
implicit host includes, native compiler, or provider-wide TU/content cache.
Linux x86-64 LP64 little endian with GNU libc major 2, version 2.33 or newer,
is the implemented host ABI profile. A portable `linux-x64` RID or distribution
RID such as `ubuntu.24.04-x64` is informational and never decides support.
Actual running GNU libc identity and its bounded version are checked. Musl,
unknown/older/future-major libc ABIs, Windows/macOS and other architectures return
UnsupportedHost and remain unverified. Configuration/path errors may be reported
before qualification; stat/open/fstat do not execute on unqualified hosts.

## Required primary hooks (not owned here)

1. Keep existing quote/system search ordering. For each explicit candidate,
   use structured Read for inclusion and structured Probe for __has_include.
   Continue search only on Missing; preserve Failure as a frontend diagnostic.
2. Key retained physical sources/activations by the returned CanonicalPath.
   Do not create a source or activation for Probe. Leave macro/#line/pragma-once
   state and PhysicalSources unchanged after a presence-only check.
3. Own stable per-analysis read/probe caches, including misses/failures. A later
   analysis may observe changed host files. Probe Found must not suppress the
   later real Read or activate pragma-once state.
4. Preserve legacy InMemoryIncludeProvider compatibility without filesystem
   fallback. Legacy probe policy belongs to primary.
5. Classify UnsupportedHost as an explicit unsupported frontend input outcome;
   InvalidEncoding is invalid input, UnsupportedEncoding is unsupported input,
   LimitExceeded is a resource outcome, and Access/Io/path failures are input
   failures. Missing alone permits candidate search to continue.
6. Register the exact direct/integration groups and special-child CLI hook below;
   supervisor owns existing files, execution and acceptance evidence.

## Root-authorized filesystem-only interop adaptation (M1L-07)

The initial BCL-only choice was superseded by the monitoring root's explicit
authorization for a handwritten Linux x64 filesystem adapter. Production uses
GNU libc identity/version identification plus libc `stat`, `open`, and `fstat`,
followed by BCL SafeFileHandle, FileStream and strict decoders. All explicit
libc bindings use the same `libc.so.6` SONAME. There is no frontend/native compiler wrapper,
new native binary, source translator, dynamic code generation, or external
package. The test child additionally uses filesystem-only `mkfifo` to create
its own controlled FIFO.

Why BCL-only was insufficient: .NET 10's Unix FileStatus.GetAttributes omits
FIFO/socket types and GetUnixFileMode masks out type bits. SafeFileHandle's
PreOpenConfigurationFromOptions maps read to O_RDONLY, without O_NONBLOCK;
FileOptions.Asynchronous does not change open flags. A later CanSeek check
cannot prevent the initial FIFO open from blocking. TarWriter's private stat
does not provide a general public stat API, and its regular-file path still
opens by pathname. No reflection/archive workaround is used.

Final descriptor algorithm:

1. Normalize against captured explicit roots; reject invalid/NUL/unpaired-surrogate
   paths before UTF-8 marshaling. Qualify Linux + X64 + 64-bit pointers + little
   endian + actual GNU libc major 2, version 2.33+; no RID equality/allowlist.
   NativeLibrary.GetMainProgramHandle/TryGetExport first require the running
   process to expose gnu_get_libc_version, before resolving a named libc library.
   This borrowed search pseudo-handle is never freed. The version function then
   comes from the same explicit libc.so.6 as the filesystem calls, and its owned
   static string is inspected for at most 32 bytes (31 ASCII version characters
   plus NUL). Missing symbols/library, invalid/overlong version, libc major other
   than 2 or minor below 33 => UnsupportedHost. This prevents treating a musl
   process with an installed compatibility library as qualified just by RID or
   library filename. No ABI discovery or native library I/O occurs at provider
   construction, and the provider retains no mutable ABI/content/TU cache.
2. Probe uses `stat(path)` only; follow symlinks to classify the target. Regular
   files are Found regardless of decoding/size/read permission. Directories are
   Missing. FIFO/socket/device inputs are typed InvalidPath failures. No source
   objects, decoded buffers or preprocessing state are created.
3. Read opens with `O_RDONLY | O_NONBLOCK | O_CLOEXEC` (`0 | 0x800 | 0x80000`).
   Immediately put the descriptor in an owning SafeFileHandle; fstat that same
   descriptor. A FIFO swapped in during path resolution cannot block in open;
   its descriptor fails the S_IFREG check before any read. Directory descriptors
   produce Missing. Regular inode type cannot change after the descriptor opens.
4. Check fstat length against the byte budget before allocating its byte array.
   Construct FileStream over that same SafeFileHandle (buffer size 1, no pathname
   reopen), read at most the reported length plus one overflow-detection byte,
   then dispose. Both owning scopes Dispose the same SafeHandle idempotently;
   every early return/fstat/constructor/read/decoder failure closes it once.
5. Native errno is captured immediately with Marshal.GetLastPInvokeError.
   ENOENT/ENOTDIR/EISDIR => Missing; EPERM/EACCES => Access;
   EINVAL/ENAMETOOLONG/ELOOP => InvalidPath; ENOSYS => UnsupportedHost; others,
   including EINTR/EAGAIN/resource exhaustion => Io. No unbounded retry loop.
   Missing libc symbols/libraries/ABI image report UnsupportedHost. Other BCL
   path/access/I/O failures also retain typed results rather than opaque throws.

`struct stat` is an explicit-layout **host ABI binding**, 144 bytes on Linux
x86-64 libc: st_mode at offset 24 (uint), st_size at offset 48 (long). S_IFMT is
0xf000, S_IFREG 0x8000, S_IFDIR 0x4000. The remaining native bytes are reserved
through the explicit size, not incorrectly omitted from the native output buffer.
These are filesystem descriptors, not an imitation of EDG FILE* ownership.
This binding is not enabled on a different OS/architecture or unrecognized libc
ABI. GNU libc 2.33 is the minimum because the inspected io/Versions exports
stat/fstat in GLIBC_2.33. GNU libc major 3 would require a new ABI review.

There is no wall-clock cancellation or network-filesystem latency guarantee.
Nonblocking open solves the FIFO rendezvous problem; regular filesystem I/O
and metadata may still wait on the kernel. Roots are lexical, symlink targets
can leave them, hardlinks are not unified, and host contents are not an atomic
filesystem snapshot. The caller retains per-analysis stable observations.

## Encoding and resource contract

- Default: strict UTF-8 without BOM. A leading EF BB BF selects UTF-8;
  FF FE selects UTF-16 LE, FE FF selects UTF-16 BE. Strip exactly one BOM.
- UTF-32 LE/BE and UTF-7 BOM signatures explicitly fail UnsupportedEncoding.
  Other BOM-less bytes are interpreted as UTF-8, never an inferred locale or
  UTF-16 encoding. BOM-only and empty regular files return the empty string.
- Exception-fallback encoders reject overlong/truncated UTF-8, UTF-8 surrogates,
  out-of-range scalar values, odd UTF-16 byte counts and unpaired UTF-16 surrogates.
  A strict GetCharCount validates the complete payload and checks the character
  limit before allocating the returned string. No replacement text is returned.
- Defaults are 16 MiB input bytes and 16 Mi UTF-16 code units per input; both
  limits must be positive and no greater than Array.MaxLength. Byte limits
  include the BOM. Character limits exclude the stripped BOM and count UTF-16
  code units, not scalar values. Allocation is bounded by admitted input bytes
  and decoded code units, with BCL decoder/stream overhead.
- Growth past the original length returns Io, or LimitExceeded when the
  original length was the byte limit. No partial success is returned for growth.
  A concurrent shrink may yield the observed shorter contents; same-length host
  rewrites are not atomic snapshots. Primary caches the returned immutable text.
- Decoding a string establishes no C++ Unicode identifier/UCN/literal semantics.

## Consulted EDG implementation and exact source hashes

`../upstream/src/lexical.c` SHA-256:
`e987ffcb7645931564f3a6f262d089c21524e78ce2c60d5e0e0868a5b4b98875`.

- `try_to_open_source_file` (7320–7344): separated open outcome/unicode kind;
  OFF_OKAY_IF_NOT_FOUND/DIRECTORY/NOT_REGULAR/CANNOT_OPEN branches inform typed
  managed outcomes. Missing is distinguished from Access/Io/encoding/limit.
- `try_to_open_source_file_if_not_already_included` (7347–7387): suppression
  versus opening is separate; provider owns no include-once/history state.
- `search_for_input_file` (7531–7760): ordered directory traversal, absolute-path
  bypass, previous search-result reuse, found name versus failed open. Primary
  owns ordering/cache; provider handles one explicit candidate and returns its
  normalized absolute physical name. No implicit CWD/preinclude search, Sun
  suffix substitutions, Microsoft CWD branches or template implicit-inclusion
  suffix lists are claimed here.
- `resolve_header` (7763–7816), `header_can_be_found` (7873–7889): resolution
  without pushing input state. Native resolve opens/closes a stream and may
  inspect its BOM; this authorized managed adaptation is stronger: stat only,
  no content read/decoding/physical source/activation for a presence probe.
  #include_next/#embed search selection remains unimplemented outside this work.
- `open_file_for_input` (7892–8051): open result/full/display name, optional
  failure, quote/system/include-next selection and source encoding are separate
  concerns. Typed managed values replace FILE* and catastrophe diagnostics;
  frontend diagnostics remain primary's responsibility. Stdin/driver/preinclude
  and native input-stack construction are not provider features.

`../upstream/src/host_envir.c` SHA-256:
`7089dc53eb8fca9f3c55ed299850914d39a8fdd533937e8809d210a5f6459f2c`.
`do_check_for_byte_order_mark` (1443–1514) has UTF-8/UTF-16 LE/BE selection and
non-BOM rewind branches; `open_source_file` (1576–1607) gates BOM handling on
UNICODE_SOURCE_SUPPORTED/check_for_byte_order_mark. Managed selection is fixed
and explicit above; locale/default_unicode_source_kind configuration is not
silently inherited. Malformed-byte validation is an explicit managed contract.

`../upstream/src/error.c` SHA-256:
`e9a2fee3c39c8958361236285b57a87a98ceb02592a1de5e1035bd65e7bf3a60`.
`open_source_file_with_error_handling` (5949–5980) separates open status from
diagnostic severity/flags. Managed result failures preserve that separation;
no native longjmp/catastrophe/error globals are copied.

M1L-06 directory policy follows supervisor-executed exact native receipts:
`evidence/native-m1-directory-probe-contract-round1.json` and
`evidence/native-m1-directory-fallback-contract-round1.json`. A directory
candidate is Missing; search can continue to a later regular header. This worker
did not execute either oracle. No whole-file port or full M1 support is claimed.

## Runtime and host ABI references

Inspected upstream .NET runtime **v10.0.0** sources:

- [SafeFileHandle.Unix.cs](https://github.com/dotnet/runtime/blob/v10.0.0/src/libraries/System.Private.CoreLib/src/Microsoft/Win32/SafeHandles/SafeFileHandle.Unix.cs),
  PreOpenConfigurationFromOptions (211–280), Init/fstat (282 onward),
  ReleaseHandle (114–139).
- [FileStatus.Unix.cs](https://github.com/dotnet/runtime/blob/v10.0.0/src/libraries/System.Private.CoreLib/src/System/IO/FileStatus.Unix.cs),
  GetAttributes (169–195), GetUnixFileMode (408–420), cache stat/type handling
  (442–494). Public metadata loses special file-type bits.
- [TarWriter.Unix.cs](https://github.com/dotnet/runtime/blob/v10.0.0/src/libraries/System.Formats.Tar/src/System/Formats/Tar/TarWriter.Unix.cs),
  ConstructEntryForWriting (16–92), inspected to exclude an archive workaround;
  not used by the provider.
- [NativeLibrary.cs](https://github.com/dotnet/runtime/blob/v10.0.0/src/libraries/System.Private.CoreLib/src/System/Runtime/InteropServices/NativeLibrary.cs),
  GetMainProgramHandle (225–244) and TryGetExport (150–164): borrowed process
  search pseudo-handle and nonthrowing symbol presence; no library Free call.

ABI specification references are glibc **glibc-2.40**
[x86 struct_stat.h](https://github.com/bminor/glibc/blob/glibc-2.40/sysdeps/unix/sysv/linux/x86/bits/struct_stat.h),
[Linux stat.h](https://github.com/bminor/glibc/blob/glibc-2.40/sysdeps/unix/sysv/linux/bits/stat.h),
[x86 fcntl.h](https://github.com/bminor/glibc/blob/glibc-2.40/sysdeps/unix/sysv/linux/x86/bits/fcntl.h);
Linux **v6.12**
[fcntl.h](https://github.com/torvalds/linux/blob/v6.12/include/uapi/asm-generic/fcntl.h),
[errno-base.h](https://github.com/torvalds/linux/blob/v6.12/include/uapi/asm-generic/errno-base.h),
[errno.h](https://github.com/torvalds/linux/blob/v6.12/include/uapi/asm-generic/errno.h).
Only standard ABI numbers/layout are bound; no compiler/frontend code is reused.

Fresh host-qualification research, glibc **glibc-2.40**:

- [csu/version.c](https://github.com/bminor/glibc/blob/glibc-2.40/csu/version.c),
  gnu_get_libc_version (53–59) returns GNU libc's static version string.
  Fetched SHA-256:
  `030ec9119f1f01995bc595af1872a1f9edf6bafb80477cc417ff96f5a41067d3`.
- [io/Versions](https://github.com/bminor/glibc/blob/glibc-2.40/io/Versions),
  stat/fstat public symbol group is GLIBC_2.33 (131–134).
  Fetched SHA-256:
  `5ce15a2f2289269a06b585442cc5a5d4028ffb9b570af28cc03454c5f4c1004a`.

These source reads and byte hashes are provenance inspection, not compilation or
runtime ABI experiments. This worker has not executed GNU libc identification,
filesystem calls, tests or any compiler against the repaired code.

Actual installed headers were also read, with SHA-256:

- `/usr/include/x86_64-linux-gnu/bits/struct_stat.h`:
  `00b3b33c165454470de0f21b324867ec6e9b43e34f5d503121adf021cad8202e`.
- `/usr/include/x86_64-linux-gnu/bits/stat.h`:
  `94cb3b45d5826d7d5ce6d81e9eb8b053dedd43dd9cde5b074e47c3a28d47bd8d`.
- `/usr/include/x86_64-linux-gnu/bits/fcntl-linux.h`:
  `8d50bc7737bcc76f42b1f406244acb287bcf0463d180b6b0566d27d1f23b3e98`.
- `/usr/include/asm-generic/errno-base.h`:
  `2c148e92b8318deeb767fabd60822113e575ee664ff09a1873aed8f7a495793c`.
- `/usr/include/asm-generic/errno.h`:
  `fb7b5a504015f3a9074c641e7371b250d867d751d90e4a22a8ac17fced3d50af`.

These host headers are read-only ABI research, never test inputs or runtime
includes; tests create every header under a fresh owned temporary directory.

## Exact authored test inventory — NOT EXECUTED by this worker

Register these 12 Action methods in the existing partial `Tests` runner:

1. `FileSystemConfiguration`: immutable explicit snapshots, validation, strict
   path marshaling, no construction I/O, default/in-memory no host fallback.
   Its new private `FileSystemPublicAndHostContracts` helper adds finite public
   result/exception invariants (all failure kinds, nullable failed path, empty
   successful text, immutable failure identity, rejected null/empty/invalid
   construction), plus a pure host ABI matrix. Accepted GNU version controls are
   2.33/2.39/2.40/2.40.9000; musl/no GNU identity, null/empty/malformed/overlong
   version, older/future-major version, non-Linux, ARM64/x86, non-LP64 and big
   endian are rejected. This pure predicate contains no RID input or I/O.
   No new test-runner registration is required for these controls.
2. `FileSystemRequestedPaths`: each ordered explicit request, no provider
   search, lexical root boundary/escape rejection, directory Missing, Unix `/`
   preserved, absolute/root-relative requests.
3. `FileSystemEncodings`: UTF-8 no BOM/BOM, UTF-16 LE/BE, non-ASCII/supplementary
   decoded text, CRLF preservation, BOM-only/empty files, single BOM stripping,
   explicit BOM-less UTF-8 choice and compatibility TryRead.
4. `FileSystemFailures`: missing versus strict malformed bytes, overlong/
   surrogate/out-of-range/truncated UTF-8 and bad UTF-16, UTF-32/UTF-7 unsupported,
   typed TryRead exception and metadata-only malformed-header presence.
5. `FileSystemAccessFailures`: ordinary read-only file works; no-read regular
   file is Probe Found/Read Access; nonsearchable parent is Probe/Read Access.
   Linux unprivileged execution required; privileged bypass causes a failure,
   never a skip or fabricated pass. Restores permissions only on owned entries.
6. `FileSystemLimits`: exact/over byte/UTF-16-unit bounds, BOM byte cost,
   supplementary scalar consumes two units, sparse oversized file preflight.
7. `FileSystemConcurrency`: parallel same/independent provider reads with
   different captured bases, immutable retained text after host edits, no stale
   provider miss/content cache. No global CWD/environment mutation.
8. `FileSystemSessionIncludes`: physical versus presumed #line directory,
   quote/system order, exact observed candidate reads, canonical physical
   sources, native-corresponding directory miss/later-root fallback and graph
   constant 2, directory-first presence search.
9. `FileSystemSessionProbes`: malformed/pragma-once header presence without
   content read, macro/#line/once activation or retained source/frame; repeated
   probes cached once per analysis, later real include activates once, real
   remapped source/include provenance and cached absent probes.
10. `FileSystemSessionDiagnostics`: malformed header stops search with PP063
    rather than missing/fallback; unsupported encoding and byte resource class;
    absent input PP028; oversized presence succeeds without decoding.
11. `FileSystemSessionCachingQueries`: changed host text stable within an
    analysis/new text next analysis, real template/constexpr/header macro graph
    after session disposal and host edit; provider forbids all query-time I/O.
12. `FileSystemSpecialFilesBounded`: launches this executable as a child with
    an owned temporary root; hard 10-second deadline, kill on timeout, actual
    nonzero status/timeout is failure. Runs under managed and NativeAOT runner.

Required Program.cs CLI hook, **before existing argument rejection**:

```csharp
if (args.Length == 2 && args[0] == "--filesystem-special-child")
{
    Tests.FileSystemSpecialFileChild(args[1]);
    return 0;
}
```

`FileSystemSpecialFileChild(string root)` creates a FIFO with no writer, checks
32 metadata-probe/read rejections, a symlink to that FIFO, a Unix-domain socket,
a normal file/symlink, and a symlink loop. Creation is filesystem-only mkfifo,
never a compiler process. The child does not spawn another test runner. The
bounded parent creates/owns the entire root and cleans it after normal exit or
timeout/kill. No unrelated retained artifacts are deleted. Parent launch handles
both apphost/NativeAOT and explicit dotnet+assembly execution without reflection
or NativeAOT-incompatible Assembly.Location. No hanging reproduction was run by
this worker; supervisor owns all execution after both writers acknowledge hold.

## Integration/verification status and edit hold

Only the three assigned new files were created/modified; no fixture directory
was needed. Existing Program/Preprocessor/session/source/token/type files,
ledgers and manifests remain primary/supervisor-owned. Primary reports the
structured search/cache/diagnostic hooks and registration/CLI are integrated.

Supervisor's first executed combined Release round-2 build passed with zero
warnings/errors. Its JIT receipt `evidence/m1-literals-tests-round2.json` records
79 groups, 67 passes, 12 failures, zero skips: every filesystem group failed at
the original UnsupportedHost RID gate before input I/O. Supervisor reports
the real process is Linux X64 GNU libc on Zorin 18 with distro RID
`ubuntu.24.04-x64`, .NET runtime 10.0.12. The original `== "linux-x64"` gate was
incorrect; that failed evidence remains retained and is not a provider pass.

This continuation replaces RID equality with actual GNU ABI qualification above,
adds the finite public/error/ABI tests in the already registered configuration
group, and improves every shared Found read/probe assertion to report canonical
path, actual Status, Failure.Kind and Failure.Message. Failure assertions now
report the same detail rather than hiding UnsupportedHost behind a status-only
or null-reference message. Public include interfaces and preprocessor hooks are
unchanged. The repair itself has not been built or run by this worker.

Historical round-3 expectation correction: supervisor reports Release with zero
warnings/errors and JIT 79 groups, 78 passed/1 failed. The sole failure was this
worker's `FileSystemSessionProbes` expected final line 6; actual physical source
line 5 is correct. Supervisor's exact native control
`tools/oracle-inputs/m1-session-probe/{main.cpp,active.h}` accepts with trailing
`7 5` in `evidence/native-m1-session-probe-line-contract-round1.json`. This narrow
follow-up changes only the expected trailing 6 to 5 and this handoff; production
code and failed round-3 evidence are preserved. All other filesystem groups,
including bounded FIFO/socket handling, passed in the supervisor's round-3 run.
No validation was executed by this worker. **READY/HOLD re-acknowledged
immediately after this expectation-only correction; no further edits until an
explicit supervisor release.** Fresh follow-up task ID is tracked by supervisor.

**Fresh READY/HOLD acknowledged after the narrow release:** source and test authoring for this checkpoint is ready
for supervisor integration, independent produced-code review and serialized
validation. From this handoff onward this worker performs **no source/test/
fixture edits until an explicit supervisor release**, including during review
or failed validation. Both implementation writers must separately acknowledge
HOLD before fingerprinting/builds/tests/native checks. This statement is an edit
hold, not an acceptance claim or campaign completion.

Outstanding: supervisor's fresh combined build and all managed/NativeAOT tests,
bounded FIFO/socket subprocess
and exact native/include/encoding controls; independent review of produced
code, interop ABI/descriptor disposal/error mapping and matching execution
receipts; supervisor documentation and verified commit. No repaired-code tests,
passes, oracle execution, NativeAOT compatibility or platform qualification are
claimed by this worker. Windows/macOS, other architectures, musl and unknown
libc ABIs are explicitly unsupported/unverified, never counted as successful skips.
