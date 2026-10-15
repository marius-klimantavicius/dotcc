/* Independent native oracle and ABI-only identity adapter; no application body. */
struct AtomicPair { _Atomic int first; int second; };
struct AtomicPair native_atomic_echo(struct AtomicPair value) { return value; }
