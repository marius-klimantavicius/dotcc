# Private socket option queries

getsockopt now reads the actual private BCL socket's type, reuse-address state,
send/receive buffer sizes and TCP no-delay setting. It uses the same descriptor
validation and socket owner as setsockopt. No guest descriptor is passed to an
operating-system descriptor API. SO_ERROR, linger, timeouts and other option
families remain unqualified; unsupported options fail explicitly.

The callback copies at most four little-endian bytes into caller storage and
returns the copied length. Short and zero-length buffers are supported, and
errors leave both output bytes and length unchanged. Unknown options at a
supported level return ENOPROTOOPT; unsupported levels return EOPNOTSUPP. Native
Linux distinguishes those cases, which an initial native probe caught. Null
required storage returns EFAULT; closed descriptors EBADF, non-sockets ENOTSOCK,
and unbound callbacks ENODEV. Success preserves errno.

Native and raw/optimized JIT/AOT pass at
artifacts/host-socket-queries/attempt-mheqynnb/receipt.json. Tests exercise real
option changes, type, output lengths/canaries, C function pointers, errors and
two independent worker sockets with opposite settings across compacting GC.
Buffer-size tests check supported bounds rather than assuming identical host
kernel defaults. Existing file/I/O regressions pass against the captured host
sources. Reproduce with python3 blink/tests/HostSocketQueries/run.py.
