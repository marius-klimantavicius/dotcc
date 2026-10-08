#define FORCE_EVAL(value) do { volatile long double result = (value); (void)result; } while (0)
volatile long double global_value = 1.0L;
_Thread_local volatile long double thread_value = -0.0L;
int alignment(void *address) { return (unsigned long)address & 15; }
int main(void) {
    volatile long double local = global_value;
    if (alignment((void *)&local) || alignment((void *)&global_value) || alignment((void *)&thread_value)) return 1;
    local += 0x1p-63L;
    if (local - global_value != 0x1p-63L) return 2;
    global_value = local;
    thread_value = global_value * 2.0L;
    if (thread_value - 2.0L != 0x1p-62L) return 3;
    FORCE_EVAL(thread_value / 3.0L);
    union { long double value; unsigned char bytes[16]; } bits;
    thread_value = -0.0L;
    bits.value = thread_value;
    if (bits.bytes[9] != 128 || bits.bytes[8] || bits.bytes[0]) return 4;
    return 0;
}
