# Managed API configuration control

This source-linked consumer runs the authored facade against the actual BCL host
and translated MsQuic library. It opens real registrations, configurations and an
unstarted connection, with no listening socket or transport handshake. It checks
all three effective version lists at global, configuration and connection scope.
The pinned upstream setter consumes host-order version values; its getter copies
the internal network-order lists (`src/core/settings.c:2191`).

It also covers synchronous and inline asynchronous credential completion, disposal
of caller credential snapshots immediately after creation begins, ALPN copying,
typed settings updates, configuration lease drain, registration identity and
invalid/canceled creation. It does not establish transport or certificate policy
callback correctness; those require the transport and TLS adapter campaigns.

Run `python3 msquic/scripts/test-managed-api.py` only with the shared validation
slot. The driver checks raw/optimized translations in JIT and NativeAOT and records
the authored and generated input hashes. A restricted probe does not clear the
full matrix gate.

`--resumption` selects a separate real loopback endpoint control and receipt in
`artifacts/managed-api-resumption`. It checks synchronous and asynchronous server
ticket approval/rejection, full-handshake fallback, copied application state,
fresh 1-RTT stream data, and closing an owner while its policy ignores
cancellation. The ordinary configuration control still opens no transport.
