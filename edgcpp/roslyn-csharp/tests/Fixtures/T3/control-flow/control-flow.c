/* Defined C control flow: retained targets, skipped initialization, fallthrough,
   nested loop/switch exits, single selector evaluation and end-tested continue. */
static int observations[32];
static int observation_count;
static int event(int value) { observations[observation_count++] = value; return value; }
int control_observation_count(void) { return observation_count; }
int control_observe(int selector) { return observations[selector]; }
int fixture(void) {
    observation_count = 0;
    int turns = 0;
    event(10);
    goto entered;
    if (event(11)) {
        int skipped = event(12);
        (void)skipped;
entered:
        event(20);
        if (++turns < 2) goto entered;
    }
    event(30);
    if (event(31)) {
        event(32);
        goto other_arm;
    } else {
        event(999);
other_arm:
        event(33);
    }
    for (int index = 0; index < 4; ++index) {
        switch (event(index + 40)) {
            case 40: event(50);
            case 41: event(51); break;
            case 42: continue;
            default: event(53); goto finished;
        }
        event(60);
    }
finished:
    ;
    int count = 0;
    do {
        ++count;
        if (count < 2) continue;
        event(70 + count);
    } while (count < 3);
    switch (event(90)) { case 91: event(999); }
    switch (event(102)) { case 101 ... 103: event(76); break; default: event(999); }
    event(80);
    return observation_count;
}
