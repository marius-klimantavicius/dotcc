#include <stdio.h>
#include <stddef.h>
struct Packet {
    union {
        struct { unsigned Type : 2; unsigned LEN : 6; };
        unsigned char Raw;
    };
    int tail;
};
int main(void) {
    struct Packet packet = {0};
    packet.Type = 3;
    packet.LEN = 12;
    packet.tail = 77;
    printf("%u %u %u %d %d %d\n", packet.Type, packet.LEN, packet.Raw, packet.tail,
        (int)sizeof(struct Packet), (int)offsetof(struct Packet, tail));
    return 0;
}
