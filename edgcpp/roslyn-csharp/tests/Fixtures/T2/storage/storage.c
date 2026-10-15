/* Application fixture: this file alone is an object-export input. */
#include "storage.h"

struct Bits {
    unsigned prefix : 3;
    signed signed_part : 9;
    unsigned payload : 13;
    unsigned : 0;
    unsigned tail : 5;
};

struct Small {
    short left;
    unsigned char right;
};

struct __attribute__((packed)) Packed {
    unsigned char lead;
    unsigned value;
};

union Word {
    unsigned value;
    unsigned char bytes[4];
};

struct Node {
    int value;
    struct Node *next;
    int *element;
};

int sequence[7] = {[0] = 3, [2] = 8, [5] = 21};
extern struct Node second;
struct Node first = {11, &second, &sequence[2]};
struct Node second = {17, &first, &sequence[5]};
int *sequence_end = sequence + 7;

static struct Bits bits = {5, -17, 0x1234, 19};
static const struct Small rows[2] = {{-2, 5}, {7, 9}};
static struct Packed packed = {0x5a, 0x10203040u};
static union Word word = {.value = 0x12345678u};
static const int duplicate_a[2] = {7, 9};
static const int duplicate_b[2] = {7, 9};
static const char text[] = "ro\0x";
static const unsigned short utf16_text[] = u"A\u03a9";
static const unsigned utf32_text[] = U"A\U0001f642";

static int copied_row(void) {
    struct Small copy = rows[1];
    copy.left += rows[0].left;
    return copy.left + copy.right;
}

int storage_observe(int selector) {
    if (selector == 0) return sizeof(struct Bits);
    if (selector == 1) return _Alignof(struct Bits);
    if (selector == 2) return bits.signed_part;
    if (selector == 3) return bits.payload;
    if (selector == 4) return bits.tail;
    if (selector == 5) return first.next == &second && second.next == &first;
    if (selector == 6) return *first.element;
    if (selector == 7) return *second.element;
    if (selector == 8) return sequence_end - sequence;
    if (selector == 9) return duplicate_a != duplicate_b;
    if (selector == 10) return text[2];
    if (selector == 11) return text[3];
    if (selector == 12) return utf16_text[1];
    if (selector == 13) return utf32_text[1];
    if (selector == 14) return copied_row();
    if (selector >= 15 && selector <= 18) return word.bytes[selector - 15];
    if (selector == 19) return sizeof(struct Packed);
    if (selector == 20) return _Alignof(struct Packed);
    if (selector == 21) return __builtin_offsetof(struct Packed, value);
    if (selector == 22) return packed.value & 0xffffu;
    if (selector == 23) return 2[sequence];
    if (selector == 24) return sequence[1] + sequence[3] + sequence[4] + sequence[6];
    if (selector == 25) return sizeof(rows[0]);
    if (selector == 26) return sizeof(text);
    if (selector == 27) return sizeof(utf16_text);
    if (selector == 28) return sizeof(utf32_text);
    if (selector == 29) return duplicate_a[0] + duplicate_a[1] + duplicate_b[0] + duplicate_b[1];
    if (selector == 30) return first.value + second.value;
    if (selector == 31) return sizeof(sequence);
    return -1; /* Test observation API: out-of-range selector. */
}

int fixture(void) {
    bits.prefix = 5;
    bits.signed_part = -17;
    bits.payload = 0x1234;
    bits.tail = 19;
    bits.signed_part += 3;
    bits.payload ^= 0x155;
    int total = 0;
    for (int selector = 0; selector < STORAGE_OBSERVATION_COUNT; ++selector) {
        total += storage_observe(selector);
    }
    return total;
}
