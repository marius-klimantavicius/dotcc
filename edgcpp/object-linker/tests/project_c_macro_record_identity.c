#define AS_BITS(value) ((union { double real; unsigned long long bits; }){ (value) }).bits
#define ADD_BITS(first, second) (AS_BITS(first) + AS_BITS(second) + AS_BITS(first))
#define AS_REAL(value) ((union { unsigned long long bits; double real; }){ (value) }).real
int main(void) {
    double one = 1.0, two = 2.0;
    unsigned long long value = ADD_BITS(one, two);
    if (value != 0xbfe0000000000000ULL) return 1;
    if (AS_REAL(AS_BITS(one)) != one) return 2;
    return 0;
}
