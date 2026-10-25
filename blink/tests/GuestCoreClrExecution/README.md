The ordinary `getcpu.c` fixture uses pinned Linux libc and its loader. It selects
the first advertised CPU, then checks raw `getcpu`, glibc `sched_getcpu`, optional
outputs, the four-byte output ABI, and the ignored historical cache on the main
thread and a pthread. It uses valid pointers throughout.

Run `python3 -B blink/tests/GuestCoreClrExecution/run-getcpu.py` from the repository.
Builds and outputs stay in a fresh attempt with command, input and cleanup
receipts. The resulting `closure.json` is ready for the parent's fresh public
BlinkMachine integration. Native Linux CPU/node numbers are not forced to zero.

The private focused C check compiles the exact authored adapted handler and
checks its guest copy boundary. It does not qualify translated execution.

The service failure in captured r4C closure `attempt-jx5tip6e` is grounded in its
exact runtime binary: `libcoreclr.so` SHA256
`b5845ca190a41c502a96df196ea603b38d4893a022964fb55598ba6d07861f19`.
Service load base `0x200000669000` makes trap `0x200000bda526` RVA `0x571526`.
The preceding call through RVA `0x2f2710` reaches `sched_getcpu`; trace call 3644
returns `-ENOSYS` for syscall 309. Its `-1` result becomes an unsigned CPU index
for the two-byte table store. The fault address minus `2 * 0xffffffff` is
`0x110001098720`, consistent with the ordinary CPU table allocation.

The guest's existing portable `GetCpuCountImpl` returns one and `GetTscAux`
reports CPU/node zero. The new syscall reports that same software identity.
Existing fallback `sched_setaffinity` is a no-op after PID validation; this change
does not establish a complete affinity implementation.
