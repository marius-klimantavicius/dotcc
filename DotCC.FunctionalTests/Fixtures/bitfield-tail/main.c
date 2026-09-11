#include <stddef.h>
#include <stdio.h>
#include <stdlib.h>

struct Small { unsigned bits : 1; unsigned char tail[3]; };
struct Nested { char prefix; struct Small value; };
struct Signed { int low : 3; unsigned : 2; int high : 12; unsigned char marker; };
struct Wide { long long bits : 33; unsigned char tail[3]; };
struct Barrier { unsigned bits : 1; unsigned : 0; unsigned char marker; };
struct Flexible { unsigned bits : 1; unsigned char data[]; };

int main(void)
{
    struct Small s = {0};
    s.tail[0] = 17; s.tail[1] = 33; s.tail[2] = 65;
    s.bits = 1;
    printf("small size=%zu offset=%zu address=%ld nested=%zu nested-size=%zu\n",
           sizeof(s), offsetof(struct Small, tail), (unsigned char *)s.tail - (unsigned char *)&s,
           offsetof(struct Nested, value), sizeof(struct Nested));
    printf("small values=%u,%u,%u,%u\n", s.bits, s.tail[0], s.tail[1], s.tail[2]);
    s.bits = 2;
    printf("small overflow=%u tail=%u,%u,%u\n", s.bits, s.tail[0], s.tail[1], s.tail[2]);

    struct Signed sign = {0};
    sign.marker = 165;
    sign.low = -3;
    sign.high = -1024;
    printf("signed size=%zu offset=%zu address=%ld values=%d,%d marker=%u\n",
           sizeof(sign), offsetof(struct Signed, marker), (unsigned char *)&sign.marker - (unsigned char *)&sign,
           sign.low, sign.high, sign.marker);
    sign.high = 4097;
    sign.low = 7;
    printf("signed overflow=%d,%d marker=%u\n", sign.low, sign.high, sign.marker);
    sign.low = 2;
    sign.high = 1023;
    printf("signed positive=%d,%d marker=%u\n", sign.low, sign.high, sign.marker);

    struct Wide wide = {0};
    wide.tail[0] = 41; wide.tail[1] = 42; wide.tail[2] = 43;
    wide.bits = -4294967296LL;
    printf("wide size=%zu offset=%zu address=%ld value=%lld tail=%u,%u,%u\n",
           sizeof(wide), offsetof(struct Wide, tail), (unsigned char *)wide.tail - (unsigned char *)&wide,
           (long long)wide.bits, wide.tail[0], wide.tail[1], wide.tail[2]);
    wide.bits = 8589934593LL;
    printf("wide overflow=%lld tail=%u,%u,%u\n", (long long)wide.bits, wide.tail[0], wide.tail[1], wide.tail[2]);

    struct Barrier barrier = {0};
    printf("barrier size=%zu offset=%zu address=%ld\n", sizeof(barrier), offsetof(struct Barrier, marker),
           (unsigned char *)&barrier.marker - (unsigned char *)&barrier);
    struct Flexible *flex = malloc(sizeof(struct Flexible) + 3);
    if (flex == NULL) return 1;
    flex->data[0] = 73; flex->data[1] = 74; flex->data[2] = 75;
    flex->bits = 1;
    printf("flexible size=%zu offset=%zu address=%ld values=%u,%u,%u,%u\n",
           sizeof(struct Flexible), offsetof(struct Flexible, data), flex->data - (unsigned char *)flex,
           flex->bits, flex->data[0], flex->data[1], flex->data[2]);
    free(flex);
    return 0;
}
