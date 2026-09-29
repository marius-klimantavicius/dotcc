Finite isolation/lifetime witness: two real generated owners bind the same
loopback address/UDP port and, on threaded profiles, overlapping BSD descriptor
spaces. Owner A closes its resources and is disposed while B's already-open
endpoints and callback timer remain active. Explicit callback owner, GC while a
future callback is retained, and uint32 timer wrap are checked. The endpoints are
closed before disposal; pending-operation owner shutdown remains a separate gate.
