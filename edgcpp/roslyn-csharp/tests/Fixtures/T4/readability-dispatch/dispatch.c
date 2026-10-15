/* Exact selected integer dispatch; case bodies keep their retained labels.
   Reuse the existing ControlFlow checker/native observer API. */
static int observations[32];
static int observation_count;
static int event(int value) { observations[observation_count++] = value; return value; }
int control_observation_count(void) { return observation_count; }
int control_observe(int index) { return observations[index]; }

static int unsigned_dispatch(unsigned long long value) {
    switch ((event(10), value)) {
        case 0: event(11); return 1;
        case 18446744073709551615ULL: event(12); return 2;
        default: event(13); return 3;
    }
}
static int signed_dispatch(long long value) {
    switch ((event(20), value)) {
        case (-9223372036854775807LL - 1): event(21); return 4;
        case 9223372036854775807LL: event(22); return 5;
        default: event(23); return 6;
    }
}
int fixture(void) {
    observation_count = 0;
    (void)0;
    (void)123;
    (void)0.0;
    event(unsigned_dispatch(0));
    event(unsigned_dispatch(18446744073709551615ULL));
    event(unsigned_dispatch(7));
    event(signed_dispatch(-9223372036854775807LL - 1));
    event(signed_dispatch(9223372036854775807LL));
    event(signed_dispatch(7));
    switch ((unsigned char)event(255)) { case 0: event(999); break; case 255: event(31); }
    (void)event(40);
    return observation_count;
}
