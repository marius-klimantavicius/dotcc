int for_loops(void)
{
    int sum = 0;
    for (int i = 0; i < 6; ++i) {
        if (i == 2) continue;
        if (i == 5) break;
        sum = sum + i;
    }
    if (sum != 8) return 1;

    int ticks = 0;
    for (;;) {
        ++ticks;
        if (ticks < 3) continue;
        break;
    }
    if (ticks != 3) return 2;

    int outer = 0;
    int inner = 0;
    int visits = 0;
    for (outer = 0; outer < 4; outer++) {
        if (outer == 1) continue;
        for (inner = 0; inner < 5; inner++) {
            if (inner == 1) continue;
            if (inner == 3) break;
            visits++;
        }
    }
    if (outer != 4 || inner != 3 || visits != 6) return 3;

    int i = 20;
    for (int i = 0; i < 2; ++i) {
        int local = 5;
        local++;
        if (local != 6) return 4;
    }
    if (i != 20) return 5;
    for (; i < 23;) ++i;
    if (i != 23) return 6;
    for (; i < 23; ++i) ;
    for (i = 0; i < 3; ++i)
        if (i < 2) continue;
    if (i != 3) return 7;
    while (++i < 10) break;
    if (i != 4) return 8;
    return 0;
}

int do_while_loops(void)
{
    int value = 0;
    int tests = 0;
    do {
        ++value;
        if (value < 3) continue;
        break;
    } while (++tests < 10);
    if (value != 3 || tests != 2) return 10;

    do { ++value; } while (0);
    if (value != 4) return 11;

    int outer = 0;
    int visits = 0;
    do {
        int inner = 0;
        ++outer;
        while (++inner < 4) {
            if (inner == 1) continue;
            if (inner == 3) break;
            ++visits;
        }
        if (outer == 1) continue;
    } while (outer < 3);
    if (outer != 3 || visits != 3) return 12;
    return 0;
}

int scalar_mutations(void)
{
    int value = 4;
    int previous = value++;
    if (previous != 4 || value != 5) return 20;
    int current = ++value;
    if (current != 6 || value != 6) return 21;
    previous = value--;
    if (previous != 6 || value != 5) return 22;
    current = --value;
    if (current != 4 || value != 4) return 23;
    unsigned char narrow = 255;
    unsigned char old = narrow++;
    if (old != 255 || narrow != 0) return 24;
    old = --narrow;
    if (old != 255 || narrow != 255) return 25;
    old = narrow--;
    if (old != 255 || narrow != 254) return 26;
    old = ++narrow;
    if (old != 255 || narrow != 255) return 27;
    int ignored = 0;
    if (0 && ++ignored) return 28;
    if (1 || ++ignored) value = value + 1;
    if (ignored != 0 || value != 5) return 29;
    return 0;
}

int main(void)
{
    int result = for_loops();
    if (result) return result;
    result = do_while_loops();
    if (result) return result;
    return scalar_mutations();
}
