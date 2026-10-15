struct Pair {
    int value;
    unsigned short tag;
};

static int adjust(int value, int delta) {
    return value + delta;
}

int fixture(void) {
    struct Pair pair;
    pair.value = 7;
    pair.tag = 65530;
    int *pointer = &pair.value;
    int total = 0;
    for (int index = 0; index < 5; ++index) {
        *pointer = adjust(*pointer, index);
        if ((index & 1) == 0) {
            total += *pointer;
        } else {
            total -= 2;
        }
    }
    unsigned char small = (unsigned char)(pair.tag + 13);
    while (total < 40) {
        total += 3;
    }
    if (total == 42) {
        total += small;
    }
    return total + *pointer;
}
