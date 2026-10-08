struct Data { int count; int numbers[2]; char label[4]; int *pointer; int (*callback)(int); double zero; };
int target = 41;
static int add(int value) { return value + 3; }
static const struct Data empty = { 0 };
static const struct Data populated = { 7, { 11, 13 }, "row", &target, add, -0.0 };
void clear(struct Data *output) { *output = empty; }
struct Data get(void) { return populated; }
int main(void) {
    struct Data value = get();
    if (value.count != 7 || value.numbers[1] != 13 || value.label[2] != 'w' || value.label[3]) return 1;
    if (*value.pointer != 41 || value.callback(5) != 8) return 2;
    union { double value; unsigned long long bits; } negative = { .value = value.zero };
    if (negative.bits != 0x8000000000000000ULL) return 3;
    clear(&value);
    if (value.count || value.numbers[1] || value.label[0] || value.pointer || value.callback || value.zero) return 4;
    return 0;
}
