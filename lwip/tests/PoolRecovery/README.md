Finite lifecycle/error witness exhausts actual generated UDP PCB and pbuf pools,
checks configured capacity and live used counters, proves owner B still allocates,
then frees every object and allocates again with zero-used steady-state counters.
Independent native C repeats capacity/exhaustion/recovery checks. A translated
core dispatch callback exception is contained until the initiating caller resumes.
Finally two registered UDP-owning services are disposed in reverse order while
the core is still alive, without caller-side endpoint cleanup. Pending blocked
app/link workers remain separate service-specific teardown cases.

Threaded profiles also register an already-blocked translated UDP receive as an
owned service. Stack disposal invokes real lwIP close, waits for the receive to
return native ENOTCONN, checks natural NETCONN/UDP pool baselines, then releases
the caller lease before the TCP/IP stop marker. No generic sys_arch wait is
interrupted. Separate failures exercise service-stop retry while the core is
live and a recorded worker failure after the marker: first disposal reports the
error; retry releases the retained owner without posting to the stopped core.

A translated fs_open/fs_close witness retains a real host HTTP content context.
Disposal refuses it before the stop marker, ordinary admissions remain closed,
and CleanupForDisposal runs the actual close before retry. Negative paths cover
cleanup before failure, after marker, after owner release, throwing cleanup,
and recursive facade disposal/service creation from that callback.

An original sys_timeout callback separately attempts Invoke, Dispose and Poll.
The owner worker/core context (or NO_SYS Poll scope) rejects each before taking
the facade gate, then a subsequent core dispatch verifies continued operation.
