# Threaded execution owner

This authored C# API consumes the stable translated interpreter library at
`generated/TranslatedBlink/TranslatedBlink.csproj`. The raw-socket NativeAOT
service, ASP.NET Core/Kestrel and actual worker lifecycle have selected-profile
execution receipts. See the campaign progress
ledger for the exact frozen inputs and result boundaries.

The constructor's optional `memoryLimitBytes` accepts page-aligned limits from
8 KiB through 256 MiB; its low-level default remains 64 MiB. The public
`BlinkMachine` passes its own 256 MiB default explicitly. The owner passes that value to shared backing creation;
existing resource initialization sets guest RLIMIT_AS/DATA to the same limit.
Thus reserved guest addresses count toward admission even before physical pages
are touched. This is not a measurement of process RSS.

`Run(..., allowElfInterpreter: true)` enables the translated upstream ELF
interpreter handoff; the default is false. Public callers set
`ExecutionOptions.AllowElfInterpreter` instead. The interpreter and its libraries
must exist in the supplied guest filesystem. The preflight checks PT_INTERP
policy and interpreter availability; upstream loading and guest `ld.so` perform
the actual loading and relocations. See [the public contract](../Managed.Emulation/MACHINE-API.md)
and [ELF qualification](../../tests/ElfRuntime/README.md). This policy is not a
security boundary: explicitly executing a loader ELF is still possible.

The staged upstream clone path retains flag validation, Machine allocation,
register/TLS/stack setup and TID writes. Its narrow callback transfers an existing
Machine only after a dedicated C# thread starts. Each worker attaches the same
HostMemory context and private IO, acquires its own stop/sleep and virtual signal
mask, and arms its own jump-buffer identity. CPU dispatch remains upstream.

Thread/group exit records the real syscall request and unwinds on that worker.
Workers release syscall/page locks, clear the actual child-TID word and free their
own Machine. The main Machine stays alive until every child Thread has actually
joined, preventing last-Machine system teardown under an active child. Final
exit callbacks, host contexts and backing disposal occur afterward. Errors in
cancellation notification or binding cleanup prevent successful completion.

The owner limits concurrently owned workers, including main, to
`maximumWorkers` (16 by default; the public API supplies 64). A child slot is
reused only after Machine/binding cleanup, host-thread join and synchronization
resource disposal. Completed results remain in history. The instruction budget
is shared, and stopped syscall state is checked before cleanup. Each owner runs
once and creates a private translated program context. A later public run uses
a fresh owner/context while retaining the machine's guest filesystem.

The caller borrows IO and external stop until `IsQuiescent` is true. If a worker
cannot join or upstream ownership fails to release, shared backing is retained
and the caller must discard the containing worker process before releasing those
borrowed resources. No concurrent forced free or native host signal is used.
Guest signals use the translated signal machinery and managed worker
notifications; they never raise a native host signal. Ordinary group shutdown
follows the managed callback path. Pinned robust-list
cleanup remains upstream's existing no-op and is not newly qualified.
