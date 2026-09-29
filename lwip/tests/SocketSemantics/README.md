Finite BSD socket cases on the actual translated loopback/core: dual-stack UDP
options, ioctl/fcntl nonblocking flags, timeout/errno, select/poll readiness, peek;
TCP peer address, partial reads and half close; an observed blocked UDP caller
released by concurrent close under LWIP_NETCONN_FULLDUPLEX. An independent native
C program runs the same cases with the matching profile and deliberate failure.
The just-closed valid slot retains a native pending read event; an out-of-range
descriptor returns -1 with POLLNVAL. Both are compared explicitly instead of
assuming host kernel poll conventions. All sockets close before owner
disposal. This finite suite is not full pending-owner shutdown qualification.
