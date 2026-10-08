struct WideBits { unsigned long long x:3; signed long long y:3; };
static int wide_promotions(void) {
 struct WideBits bits={5,-2}; volatile int delta=-1;
 if ((bits.x += delta) != 4) return 1;
 if ((bits.x /= delta) != 4) return 2;
 if ((bits.y += delta) != -3) return 3;
 return 0;
}

struct Bits { unsigned x:3; signed s:4; };
struct Tail { int count; void *tail[]; };
struct OuterBits { char tag; struct Bits bits; };
struct OuterTail { char tag; struct Tail tail; };
static struct Bits bits;
static int arithmetic_values(void) {
    if ((bits.x = 15) != 7) return 1;
    if (++bits.x != 0 || bits.x) return 2;
    bits.x = 7;
    if ((bits.x += 3) != 2 || bits.x != 2) return 3;
    bits.x = 7;
    if (bits.x++ != 7 || bits.x) return 4;
    if ((bits.s = 9) != -7 || bits.s != -7) return 5;
    bits.x = 3;
    if ((bits.x <<= 2) != 4 || bits.x != 4) return 6;
    struct OuterBits b;
    struct OuterTail t;
    if ((char*)&b.bits-(char*)&b != 4 || sizeof(b) != 8) return 7;
    if ((char*)&t.tail-(char*)&t != 8 || sizeof(t) != 16) return 8;
    int i = -3; unsigned u=2;
    if ((i /= u) != 2147483646) return 9;
    volatile int v=-3;
    if ((v /= u) != 2147483646) return 10;
    long k=-3; unsigned long q=2;
    if ((k /= q) != 9223372036854775806L) return 11;
    return 0;
}

int main(void) { int value = wide_promotions(); return value ? value : arithmetic_values(); }
