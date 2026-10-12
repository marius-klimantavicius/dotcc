int effects = 0;
int effect_value(int* value) { ++effects; *value = 8; return 3; }

int source_mask(int c) {
    c &= 7;
    c |= 16;
    c ^= 3;
    return c;
}

int source_product(int value, int factor, int shift) {
    value *= factor;
    value ^= value >> shift;
    return value;
}

unsigned source_wrap(unsigned value, unsigned factor) {
    value *= factor;
    return value;
}

struct Plain { int value; };
int temporary_assignment() {
    Plain target = {9};
    target = Plain();
    return target.value;
}

int retained_helpers() {
    int c = 15;
    int* selected = &(c &= 7);
    if (selected != &c || c != 7) return 1;
    unsigned char narrow = 250;
    narrow |= 7;
    if (narrow != 255) return 2;
    effects = 0;
    c = 15;
    c &= effect_value(&c);
    // The recorded C++17 RHS is evaluated before loading the old c value.
    if (effects != 1 || c != 0) return 3;
    c = 15;
    c ^= effect_value(&c);
    if (effects != 2 || c != 11) return 5;
    c = 15;
    c *= effect_value(&c);
    if (effects != 3 || c != 24) return 6;
    int product = 4;
    int* product_result = &(product *= 3);
    if (product_result != &product || product != 12) return 7;
    int data[2] = {15, 15};
    int index = 0;
    data[index++] &= 7;
    if (index != 1 || data[0] != 7 || data[1] != 15) return 4;
    return 0;
}
