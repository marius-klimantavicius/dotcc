static int ticks;
static void tick(void) { ++ticks; }
static int identity(int value) { return value; }
static int returned(int value) { return (tick(), value + 3); }
int main(void) {
    int values[4] = { 10, 20, 30, 40 };
    int value = (tick(), 7);
    if (value != 7 || ticks != 1) return 1;
    value = 0 ? (tick(), 11) : (tick(), 12);
    if (value != 12 || ticks != 2) return 2;
    value = 0 && (tick(), 1);
    value += 1 || (tick(), 0);
    if (value != 1 || ticks != 2) return 3;
    value = (tick(), 0) || (tick(), 1);
    if (value != 1 || ticks != 4) return 4;
    value = identity((tick(), values[(tick(), 2)]));
    if (value != 30 || ticks != 6) return 5;
    int count = 0;
    while ((tick(), count < 3)) {
        ++count;
        if (count == 2) continue;
        value += count;
    }
    if (count != 3 || ticks != 10 || value != 34) return 6;
    count = 0;
    do {
        ++count;
        if (count == 1) continue;
    } while ((tick(), count < 2));
    if (count != 2 || ticks != 12) return 7;
    for (count = (tick(), 0); (tick(), count < 3); count = (tick(), count + 1)) {
        if (count == 1) continue;
        value += count;
    }
    if (count != 3 || ticks != 20 || value != 36) return 8;
    int *pointer = (tick(), &values[0]);
    pointer[(tick(), 1)] = (tick(), 29);
    if (values[1] != 29 || ticks != 23) return 9;
    value = returned((tick(), 4));
    if (value != 7 || ticks != 25) return 10;
    return 0;
}
