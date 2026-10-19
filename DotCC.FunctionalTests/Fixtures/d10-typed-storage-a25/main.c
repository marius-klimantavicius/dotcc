#include <stdio.h>
#include <stdint.h>
#include <uchar.h>
#include <math.h>

/* The native oracle reads each owning C object's bytes. It does not derive
   expected data using the managed emitter or its packing routine. */
signed char signed8[] = {-128, 127, -1, 0};
unsigned char unsigned8[] = {0, 255, 128, 1};
short signed16[] = {-32768, 32767, -1, 0};
unsigned short unsigned16[] = {0, 65535, 32768, 1};
unsigned short twin16[] = {0, 65535, 32768, 1};
_Alignas(64) unsigned int aligned[] = {0xffffffffU, 0x80000000U};
int signed32[] = {(-2147483647-1), 2147483647, -1, 0};
unsigned int unsigned32[] = {0, 4294967295U, 2147483648U, 1};
long signed64[] = {(-9223372036854775807L-1), 9223372036854775807L, -1, 0};
unsigned long unsigned64[] = {0, 18446744073709551615UL, 9223372036854775808UL, 1};
char16_t characters[] = {0, 65535, 32768, 65};
_Bool booleans[] = {0, 1, -7, 256};
enum Choice { choice_zero = 0, choice_high = 0x7fffffff };
enum Choice choices[] = {choice_zero, choice_high};
float singles[] = {-0.0f, 0.0f, 0x1p-149f, INFINITY, -INFINITY, NAN, 1.25f};
double doubles[] = {-0.0, 0.0, 0x1p-1074, HUGE_VAL, -HUGE_VAL, (double)NAN, 1.25};
unsigned int chunks[8201] = {[0] = 1, [4095] = 0xffffffffU, [4096] = 0x80000000U, [8200] = 73};

struct Nested { signed char tag; unsigned short code; };
union Selected { long number; void *pointer; };
struct Row {
    unsigned short length;
    unsigned short token;
    struct Nested nested;
    union Selected selected;
    struct Row *next;
};
struct Row rows[3] = {
    {.length = 5, .token = 65535, .nested = {-17, 32768}, .selected = {.number = -9}, .next = &rows[2]},
    {.length = 2, .token = 32768, .nested = {127, 65535}, .selected = {.pointer = &rows[0]}, .next = &rows[0]},
    {.next = &rows[2]},
};
struct WithArray { unsigned int data[8201]; long tail; };
struct WithArray containing = {.data = {[0] = 3, [8200] = 91}, .tail = 19};
struct WithBits { unsigned int bit:3; int value; } bitrows[] = {{5, -11}, {7, 23}};
static int plus(int value) { return value + 4; }
static int minus(int value) { return value - 2; }
int (*callbacks[])(int) = {plus, minus, plus};
int *error[] = {signed32, signed32 + 2};
int *p[] = {signed32 + 1};
int *storage[] = {signed32 + 3};
_Thread_local unsigned short thread_values[] = {7, 65535, 32768};
_Thread_local struct WithArray thread_record = {.data = {[0] = 17, [8200] = 33}, .tail = 5};

int a25_thread_step(void) { return thread_values[0]++ + thread_record.data[0]++; }
unsigned long a25_thread_address(void) { return (uintptr_t)thread_values; }
unsigned long a25_rows_address(void) { return (uintptr_t)rows; }
int a25_static_step(void) { static unsigned short local[] = {31, 65535}; return local[0]++; }
static unsigned int single_bits(unsigned int index) {
    union { float value; unsigned int bits; } data;
    data.value = singles[index]; return data.bits;
}
static unsigned long double_bits(unsigned int index) {
    union { double value; unsigned long bits; } data;
    data.value = doubles[index]; return data.bits;
}
int a25_scalars(void) {
    return sizeof(char) == 1 && sizeof(short) == 2 && sizeof(int) == 4 && sizeof(long) == 8
        && signed8[0] == -128 && signed8[1] == 127 && signed8[2] == -1
        && unsigned8[1] == 255 && unsigned8[2] == 128
        && signed16[0] == -32768 && signed16[1] == 32767 && signed16[2] == -1
        && unsigned16[1] == 65535 && unsigned16[2] == 32768
        && signed32[0] == (-2147483647-1) && signed32[1] == 2147483647 && signed32[2] == -1
        && unsigned32[1] == 4294967295U && unsigned32[2] == 2147483648U
        && signed64[0] == (-9223372036854775807L-1) && signed64[1] == 9223372036854775807L && signed64[2] == -1
        && unsigned64[1] == 18446744073709551615UL && unsigned64[2] == 9223372036854775808UL
        && characters[1] == 65535 && characters[2] == 32768
        && booleans[0] == 0 && booleans[1] == 1 && booleans[2] == 1 && booleans[3] == 1
        && choices[0] == choice_zero && choices[1] == choice_high
        && single_bits(0) == 0x80000000U && single_bits(1) == 0
        && single_bits(2) == 1 && single_bits(3) == 0x7f800000U && single_bits(4) == 0xff800000U
        && (single_bits(5) & 0x7fffffffU) > 0x7f800000U && single_bits(6) == 0x3fa00000U
        && double_bits(0) == 0x8000000000000000UL && double_bits(1) == 0
        && double_bits(2) == 1 && double_bits(3) == 0x7ff0000000000000UL && double_bits(4) == 0xfff0000000000000UL
        && (double_bits(5) & 0x7fffffffffffffffUL) > 0x7ff0000000000000UL && double_bits(6) == 0x3ff4000000000000UL
        && (uintptr_t)aligned % 64 == 0 && aligned[1] == 0x80000000U;
}
int a25_mutable(void) { unsigned16[0] = 97; return unsigned16 != twin16 && twin16[0] == 0 && unsigned16[0] == 97; }
int a25_check(void) {
    return a25_scalars() && rows[0].next == &rows[2] && rows[1].selected.pointer == &rows[0]
        && rows[2].next == &rows[2] && rows[0].nested.tag == -17
        && rows[0].token == 65535 && rows[1].token == 32768
        && chunks[4095] == 0xffffffffU && chunks[4096] == 0x80000000U
        && chunks[4097] == 0 && chunks[8199] == 0 && chunks[8200] == 73
        && containing.data[8199] == 0 && containing.data[8200] == 91 && containing.tail == 19
        && bitrows[0].bit == 5 && bitrows[0].value == -11
        && callbacks[0](8) == 12 && callbacks[1](8) == 6 && callbacks[0] == callbacks[2]
        && error[1] == signed32 + 2 && p[0] == signed32 + 1 && storage[0] == signed32 + 3;
}

static void show(const void *data, unsigned long length) {
    const unsigned char *bytes = data;
    for (unsigned long i = 0; i < length; i++) printf("%02x", (unsigned int)bytes[i]);
    printf("\n");
}
int main(void) {
    show(signed8, sizeof(signed8)); show(unsigned8, sizeof(unsigned8));
    show(signed16, sizeof(signed16)); show(unsigned16, sizeof(unsigned16));
    show(signed32, sizeof(signed32)); show(unsigned32, sizeof(unsigned32));
    show(signed64, sizeof(signed64)); show(unsigned64, sizeof(unsigned64));
    show(characters, sizeof(characters)); show(booleans, sizeof(booleans));
    show(choices, sizeof(choices)); show(singles, sizeof(singles)); show(doubles, sizeof(doubles));
    int thread_first = a25_thread_step();
    int static_first = a25_static_step();
    int static_second = a25_static_step();
    printf("%d %d %d %d\n", a25_check(), thread_first, static_first, static_second);
    return !a25_check();
}
