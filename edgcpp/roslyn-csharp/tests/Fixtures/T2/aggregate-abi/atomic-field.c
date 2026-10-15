/* Negative gateway admission: atomic raw scalar storage must not match the
   ordinary two-int byvalue layout merely because size/alignment are equal. */
struct AtomicPair { _Atomic int first; int second; };
extern struct AtomicPair native_atomic_echo(struct AtomicPair value);
int aggregate_observation_count(void) { return 0; }
int aggregate_observe(int selector) { return selector; }
int fixture(void) {
    struct AtomicPair original = {4, 7};
    struct AtomicPair echoed = native_atomic_echo(original);
    return echoed.first + echoed.second;
}
