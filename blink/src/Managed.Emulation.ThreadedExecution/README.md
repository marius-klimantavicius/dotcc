# Threaded execution owner — implementation under qualification

This authored C# API consumes the stable translated interpreter library at
`generated/TranslatedBlink/TranslatedBlink.csproj`. The raw-socket NativeAOT
service and actual worker lifecycle have qualified execution receipts;
ASP.NET Core/Kestrel remains under qualification. See the campaign progress
ledger for the exact frozen inputs and result boundaries.

The constructor's optional `memoryLimitBytes` selects 64 MiB (the unchanged
default) or 128 MiB. The owner passes that value to shared backing creation;
existing resource initialization sets guest RLIMIT_AS/DATA to the same limit.
Thus reserved guest addresses count toward admission even before physical pages
are touched. The 128 MiB Kestrel profile is prepared after an observed ordinary
thread-stack reservation exceeded the 64 MiB address-space ceiling; its actual
execution is still pending. Neither value is a measurement of process RSS.

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

The current finite profile caps **16 total created workers per process**, not16
simultaneously live workers. The instruction budget is shared; stopped syscall
state is checked before cleanup. Both single/threaded APIs share the same
process-use guard. After Run, the process must be discarded; static upstream
caches and bounded process-wide pthread handle registries are not reusable.

The caller borrows IO and external stop until `IsQuiescent` is true. If a worker
cannot join or upstream ownership fails to release, shared backing is retained
and the caller must discard the containing worker process before releasing those
borrowed resources. No concurrent forced free or native host signal is used.
Private `raise` follows the existing explicit unsupported asynchronous-signal
policy; it never raises a native host signal. Nonzero internal pthread signal
notifications and the retained uncoordinated
`KillOtherThreads` export are explicitly unsupported in this initial profile;
ordinary group shutdown follows the managed callback path. Pinned robust-list
cleanup remains upstream's existing no-op and is not newly qualified.
