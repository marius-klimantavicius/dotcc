struct Payload { char prefix; long double value; long double items[2]; };
union Bits { long double value; unsigned char bytes[16]; };
long double global_one = 1.0L;
long double global_step = 0x1p-63L;
long double global_huge = 0x1p16380L;
long double global_tiny = 0x1p-16445L;
_Thread_local long double tls_value = 1.5L;
struct Payload global_payload = { 7, 0x1.0000000000000002p0L, { 3.0L, 5.0L } };
int alignment(void *address) { return (unsigned long)address & 15; }
long double difference(long double left, long double right) {
    if (alignment(&left) || alignment(&right)) return -1.0L;
    return (left + right) - left;
}
struct Payload copy_payload(struct Payload value) {
    if (alignment(&value) || alignment(&value.value) || alignment(value.items)) value.prefix = -1;
    value.items[1] += 2.0L;
    return value;
}
long double first_argument(int marker, ...) {
    __builtin_va_list arguments;
    __builtin_va_start(arguments, marker);
    long double value = __builtin_va_arg(arguments, long double);
    __builtin_va_end(arguments);
    return value;
}
int main(void) {
    long double value = global_one;
    long double array[2] = { 3.0L, 5.0L };
    if (alignment(&value) || alignment(array) || alignment(&global_one) || alignment(&tls_value)) return 1;
    if (difference(global_one, global_step) != global_step) return 2;
    if (global_huge / 0x1p16379L != 2.0L || global_tiny * 2.0L != 0x1p-16444L) return 3;
    if ((double)(global_one + global_step) != 1.0 || (unsigned long long)tls_value != 1ULL) return 4;
    struct Payload copied = copy_payload(global_payload);
    if (alignment(&copied) || copied.prefix != 7 || copied.items[1] != 7.0L || global_payload.items[1] != 5.0L) return 5;
    if (copied.value - global_one != global_step || array[1] != 5.0L) return 6;
    union Bits negative = { .value = -0.0L };
    if (negative.bytes[9] != 128 || negative.bytes[8] != 0 || negative.bytes[0] != 0) return 7;
    if (first_argument(0, global_payload.value) - global_one != global_step) return 8;
    long double zero = global_one - value;
    long double not_number = zero / zero;
    if (not_number == not_number || !(not_number != not_number)) return 9;
    return 0;
}
