// No host headers: exercise runtime paths, not just folded constants.
int state = 3;
int sum(int n) {
    int total = 0;
    for (int i = 0; i < n; ++i) {
        if (i == 4) continue;
        total += i;
        if (total > 100) break;
    }
    return total;
}
unsigned int wrap(unsigned int x) { return x + 1U; }
long long wide(long long x) { return (x * 3) >> 1; }
int touch() { ++state; return state; }
int logical(int a, int b) { return (a && touch()) || (b && touch()); }
unsigned long long bits(unsigned long long x) { return (x << 32) | 0xffffffffULL; }
long negative(long x) { return x / -3L; }
static_assert(sizeof(int) == 4 && sizeof(long) == 8 && sizeof(void*) == 8, "scalar ABI");
int main() {
    if (sum(8) != 24) return 1;
    if (wrap(4294967295U) != 0U) return 2;
    if (wide(10000000000LL) != 15000000000LL) return 3;
    if (logical(0, 0) != 0) return 4;
    if (state != 3) return 5;
    if (logical(1, 1) != 1 || state != 4) return 7;
    if (bits(0xffffffffULL) != 0xffffffffffffffffULL) return 8;
    if (negative(21) != -7) return 9;
    int i = 0;
    do { ++i; } while (i < 3);
    while (i > 1) --i;
    return i == 1 ? 0 : 6;
}
