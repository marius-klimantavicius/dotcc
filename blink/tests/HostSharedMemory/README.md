# Shared backing-memory ownership

Native legacy/shared and all four managed modes passed. Run the focused fixture
after serial execution release:

```sh
python3 blink/tests/HostSharedMemory/run.py
```

`HostMemory.c` retains its existing thread-local owner when
`BLINK_MANAGED_GUEST_THREADS` is absent. The opt-in variant adds generation tokens
for shared contexts, per-thread attachments, and one real process-private pthread
mutex protecting the registry and complete shared operations. Native code uses
system pthread; translated code uses the reviewed threaded pthread header and
the generic BCL-backed mutex/condition implementation. No new managed locking ABI
or guest execution loop is introduced.

Creation returns an unattached token. Attach rejects an already active owner;
detach releases a worker's lifetime reference without freeing backing. Only the
creating thread may destroy a context, with zero attachments, after all workers
have joined and all upstream pointers have been discarded. Tokens are never
reused. A lookup after destruction cannot dereference a stale context. Legacy
End/DisposeWorker refuse an attached shared owner instead of freeing its memory.
The C# owner must use checked shared destruction, rather than treating the legacy
void disposal API as evidence of shared release.

Quota includes the C context/registry record and a conservative full charge for
the static registry roots and mutex per shared owner. Each mapping additionally
charges rounded backing plus its ownership/protection metadata. This is explicit
C storage accounting, not native allocator overhead, BCL synchronization object
size, total process memory or RSS. Native pthread storage differs from the
managed scalar ABI; the test retains both exact metadata reports and compares
normalized semantic results. The gate persists for the process lifetime.

Lock order is upstream mapping lock, memory-owner gate, then private I/O. File
callbacks are synchronous and must not reenter memory-owner APIs. The existing
pread/read-length bridges take private I/O locks but do not call HostMemory.
Every managed worker binds the same InstanceIo before attachment and unbinds only
after its shared-memory work. Contains/protection queries provide current
metadata, not a lasting pointer lease; raw accesses and guest page-table lifetime
remain governed by upstream synchronization.

The positive fixture runs two identical shared lifecycles. Two native pthreads
or two authored C# threads attach, allocate zeroed anonymous backing and real
file-backed copies, observe each other's mappings and bytes, change page metadata
RW to R and back while using permitted accesses, and concurrently create/unmap
128 temporary mappings. At stable barriers it requires four mappings and two
attachments; after explicit unmap only the charged context remains. Workers
detach and join before creator destruction. A 6,000-byte source file proves real
offset reads, its 1,904-byte final page content and 2,192-byte zero tail; shared
descriptor cursor 17 must remain unchanged. Native uses pread/fstat; managed uses
the unchanged HostIoBridge and a real private InstanceIo image file.

Legacy zeroing/protection/unmap/End and retained-backing DisposeWorker run both in
the native shared-off build and at the start of the shared native/managed build.
Native reference and raw/optimized JIT/NativeAOT execute the same C worker body;
managed threads are created by the authored C# consumer. Full GC occurs between
managed lifecycles. No quota exhaustion, invalid accesses, injected failures,
guest clone syscall, translated service startup or threading-profile completion
is claimed.

Each attempt snapshots C/header/bridge/Host sources, generic and profile headers,
compiler/postprocessor binaries, and build configuration. It retains native
single-owner/shared logs and all four managed logs, hashes execution closures
before/after, restores original authored consumer/bridge sources after semantic
postprocessing, and rechecks inputs/tools/source identities. ABI accounting rows
remain in logs and receipts; only those rows are excluded from native/managed
text equality. Workers must finish and join before normal shared destruction;
on timeout the outer process runner terminates its process group instead of
freeing memory concurrently from another thread.

Qualified receipt:
`blink/artifacts/host-shared-memory/attempt-if0_xhrl/receipt.json`, SHA256
`82ace64abe504f9216e77483081de36a7c27ab00116926c4c6f1f73e9fc14bc8`.
All 17 commands completed successfully; independent final review verified 879
source, snapshot, tool, log and execution-file hashes. Both shared cycles passed
in native, raw JIT, raw NativeAOT, optimized JIT and optimized NativeAOT. Native
shared-off legacy behavior also passed. Context/registry quota charge was 136
bytes natively and 100 bytes in each managed mode; the four live mapping records
cost 102 bytes in every mode, in addition to 24,576 bytes of backing. Each cycle
returned to its context-only charge with zero mappings and zero attachments
before successful creator destruction.

Three failed fixture-build attempts remain preserved:
`attempt-7v27eh32` lacked emitted vector storage and the mman overlay;
`attempt-ytx6vw7j` selected generic headers after the profile headers;
`attempt-gquko2cj` lacked the fcntl overlay's AT_FDCWD definition. Every attempt's
native checks passed. Repairs only completed the fixture's reviewed header
closure and exercised real iovec storage; HostMemory implementation and the
unchanged HostIoBridge were not modified between attempts. The final runner
asserts threaded-header selection and emitted `blink_host_iovec` storage. No
generated C# repairs or compiler changes were made.
