# Ancillary control-record traversal

HostAncillary.c supplies the previously unresolved blink_host_cmsg_nxthdr using
the measured LP64 cmsghdr/msghdr records. It advances only when the current
header, aligned current record, and complete next header fit in the caller's
logical control buffer. It rejects length and address arithmetic overflow and
preserves errno. Null or logically out-of-range inputs return null. As with
native CMSG_NXTHDR, caller-declared header storage must actually be readable;
this helper cannot validate arbitrary native allocation provenance.

The existing profile defines CMSG_LEN/SPACE, so ancillary.c's fallback probe
with an artificial unbounded message is not selected. Actual ancillary.c uses
this helper to traverse copied host control records; guest byte marshalling
remains upstream. The helper performs no socket operation and does not enable
Unix sockets, credentials, descriptor passing, or ancillary send/receive.

The native libc walk and authored walk agree across137655 valid-buffer cases,
including truncated record lengths and alignment boundaries, with digest
3842816263117141786, header16bytes and alignment8. Overflow and additional private
invalid-input checks pass. Raw/optimized JIT and NativeAOT match native exactly
at artifacts/host-ancillary/attempt-edgk8yzj/receipt.json; forced GC occurs before
the translated call. A separate native authored ASan/UBSan run also passes.
Run python3 blink/tests/HostAncillary/run.py to reproduce the matrix.
