#include <stdio.h>
#include <stdint.h>

typedef struct Frame {
    union {
        struct { uint8_t FIN:1; uint8_t LEN:1; uint8_t OFF:1; uint8_t Kind:5; };
        uint8_t Type;
    };
    int tail;
} Frame;

struct Envelope {
    struct {
        struct { int first; int second; };
        unsigned char bytes[3];
    };
    int tail;
};

static Frame global = { .Type = 0x0a, .tail = 19 };
static struct Envelope envelope = { .second = 8, .bytes = {2, 4, 6}, .first = 7, .tail = 9 };
static int calls;
static int value(int x) { calls++; return x; }

int main(void) {
    Frame bits = { .FIN = 1, .LEN = 1, .OFF = 1, .Kind = 3 };
    Frame replaced = { .Type = 0xff, .FIN = 1, .LEN = 1 };
    Frame replaced_again = { .FIN = 1, .Kind = 7, .Type = 0x31 };
    struct Envelope local = { .second = value(12), .tail = value(14), .first = value(11), .bytes = {1, 3, 5} };
    Frame literal = (Frame){ .OFF = 1, .Kind = 2, .tail = 23 };
    Frame repeated = { .LEN = 1, .LEN = 0, .OFF = 1 };
    printf("%u %u %d %u %u %u\n", global.Type, global.LEN, global.tail,
        bits.Type, replaced.Type, replaced_again.Type);
    printf("%d %d %u %u %u %d\n", envelope.first, envelope.second,
        envelope.bytes[0], envelope.bytes[1], envelope.bytes[2], envelope.tail);
    printf("%d %d %u %u %u %d %d\n", local.first, local.second,
        local.bytes[0], local.bytes[1], local.bytes[2], local.tail, calls);
    printf("%u %d %u\n", literal.Type, literal.tail, repeated.Type);
    return 0;
}
