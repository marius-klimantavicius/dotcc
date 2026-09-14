# Explicit nonterminal descriptors and termios storage

The private descriptor model has captured standard streams, regular files,
directories, and virtual TCP sockets. None is a terminal device.
`InstanceIo.Terminal.cs` checks the shared descriptor table under its existing
lock: a current descriptor yields `ENOTTY` (25); unknown, closed, and disposed
descriptors yield `EBADF`. It neither consults the process descriptor table
nor invokes operating-system terminal operations. `HostTerminalBridge.cs`
uses the existing worker-local `BindHostIo` binding; unbound calls return
`ENODEV`.

`src/HostTerminal/HostTerminal.c` defines the campaign's redirected `ioctl`,
`tcgetattr`, `tcsetattr`, `tcdrain`, `tcflow`, `tcflush`, `tcsendbreak`,
`tcgetpgrp`, `tcsetpgrp`, and `tcgetsid` functions. Ordinary C argument
evaluation is retained. The variadic `ioctl` wrapper consumes no optional
argument, and no refused call dereferences a record or request payload.
This safely handles the service's stdout `TIOCGWINSZ` query.

The policy rejects every ioctl on these private descriptors, including
unknown requests and nonterminal extensions such as socket `FIONREAD`.
It does not claim those extensions or terminal devices are implemented.
Descriptor validity takes precedence over action/payload validation;
an existing descriptor always yields `ENOTTY`, including for an invalid
action. Native libc can choose different error precedence for invalid
actions or invalid pointers. Only valid-action native comparisons are used
to qualify the common nonterminal behavior. Private tests additionally
verify the stronger no-payload-access rule with address 1 and null pointers.

## Storage-only speed helpers

Actual upstream `XlatTermiosToHost` calls `cfsetispeed` and `cfsetospeed`,
and `XlatTermiosToLinux` calls `cfgetospeed`. These operations are useful
independently of terminal devices. `src/HostTerminal/HostTermios.c` implements
them for the existing native-measured 60-byte Linux LP64 termios record.

The native libc record convention is more specific than reading/writing
`c_ispeed` and `c_ospeed`: getters read the encoded baud bits `0x100f` in
`c_cflag`, and input speed zero is represented by bit `0x80000000` in
`c_iflag`. Output setters update `c_ospeed` and the baud bits. Input setters
update `c_ispeed`; a zero value sets the input-zero marker without changing
the baud bits, while a nonzero value clears the marker and updates the
baud bits. Other record bytes remain unchanged. Codes with bits outside
`0x100f` fail with `EINVAL` and preserve the record. This includes the
native accepted encoding `0x1000`, even though the profile does not expose
it as a named speed constant. Successful operations preserve `errno`.
The private helper additionally reports `EFAULT` for a null record;
native null dereferences are not used as an oracle.

These are native-qualified host record operations, not changes to Blink's
guest termios translation. They install no terminal settings and make no
device-speed claims. Their storage convention is qualified only for this
campaign's measured ABI.

## Reproduction

Run `python3 blink/tests/HostTerminal/run.py`. The runner compares an
untouched native-libc executable with a native staged executable and raw
and optimized JIT/AOT consumers of translated C. It freezes complete Host
sources, adapter sources, profile headers, compiler hashes, and the raw
generated source before optimizing a copy; `CS8500` is a build error.
Receipt `artifacts/host-terminal/attempt-jdgw9umm/receipt.json` records all
six native/staged/managed executions passing. Both JIT builds reported
zero warnings and zero errors.

The native fixture creates a regular file, both ends of a pipe, and a
socket, and checks every declared terminal operation with valid arguments
against those nonterminal descriptors and invalid/closed descriptors.
The native staged descriptor callback is test-only: it validates those
native fixture descriptors with native `fcntl`, then reports `ENOTTY`.
That callback is excluded from the product and translated input set.

The speed oracle exercises 8194 speed codes, four different initial
full-record byte patterns, and four consecutive setter operations per
combination: 131,104 setter calls. It hashes each result, `errno`, all 60
record bytes, and both getters. The native reference digest is
`470068537395828981`. Invalid codes, input zero, output changes after input
zero, and unrelated record bytes are included.

Managed checks additionally cover standard descriptors, private files and
sockets, duplicated descriptors surviving the original close, owner
disposal, unbound calls, payload side effects evaluated exactly once,
omitted ioctl payloads, and two separately bound workers observing
different fd-1 validity across forced compacting GC. This qualifies the
authored boundary; it does not establish that the full interpreter or
service has executed successfully.
