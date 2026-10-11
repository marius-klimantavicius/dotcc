int *selected(int *value, int *trace) {
    *trace = *trace * 10 + 2;
    return value;
}
int rhs(int *trace) {
    *trace = *trace * 10 + 1;
    return 6;
}
int exercise() {
    int trace = 0;
    int value = 8;
    *selected(&value, &trace) |= rhs(&trace);
    if (trace != 12 || value != 14) return 1;
    value &= 11;
    if (value != 10) return 2;
    value ^= 3;
    if (value != 9) return 3;
    unsigned char narrow = 250;
    narrow |= 256;
    narrow ^= 3;
    narrow &= 511;
    narrow <<= 1;
    if (narrow != 242) return 4;
    signed char small = -64;
    small >>= 2;
    if (small != -16) return 5;
    unsigned int large = 0x80000000U;
    large >>= 3;
    if (large != 0x10000000U) return 6;
    int negative = -16;
    negative >>= 2;
    if (negative != -4) return 7;
    unsigned short product = 65000;
    product *= 2U;
    if (product != 64464) return 8;
    unsigned char promoted = 1;
    promoted <<= 9;
    if (promoted != 0) return 9;
    bool flag = false;
    flag |= 2;
    if (!flag) return 10;
    int mixed = -16;
    mixed |= 0x80000003U;
    if (mixed != -13) return 11;
    int aliased = 8;
    int *address = &(aliased |= 2);
    if (address != &aliased || aliased != 10) return 12;
    int &reference = (aliased ^= 3);
    reference = 20;
    if (aliased != 20) return 13;
    int skipped = 0;
    int skipped_trace = 0;
    if (trace == 0 && (*selected(&skipped, &skipped_trace) |= rhs(&skipped_trace))) return 14;
    if (skipped != 0 || skipped_trace != 0) return 15;
    int conditional = trace == 12 ? (aliased |= 1) : (*selected(&skipped, &skipped_trace) |= rhs(&skipped_trace));
    if (conditional != 21 || skipped_trace != 0) return 16;
    return 0;
}
