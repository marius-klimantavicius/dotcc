#include <stdio.h>
#include <stddef.h>
#include <string.h>

struct Byte { signed long bits : 8; unsigned char next; };
struct Short { signed low : 3; unsigned : 2; signed high : 11; unsigned char next; };
struct Word { unsigned long low : 13; unsigned long high : 19; unsigned char next; };
struct SignedWord { signed long bits : 32; unsigned char next; };
struct Nested { unsigned char prefix; struct Short items[2]; unsigned char suffix; };
struct PrefixByte { unsigned short prefix; signed bits : 8; unsigned char next; };
struct PrefixShort { unsigned char prefix; signed low : 3; unsigned : 2; signed high : 11; unsigned char next; };
struct PrefixWord { unsigned int prefix; signed long bits : 32; unsigned char next; };
struct PrefixWide { unsigned char prefix; signed long bits : 37; unsigned char next; };

#define LAYOUT(T) printf(#T " %zu %zu %zu\n", sizeof(struct T), _Alignof(struct T), offsetof(struct T, next))
#define BYTES(x) do { unsigned char *p = (unsigned char *)&(x); for (size_t j = 0; j < sizeof(x); j++) printf("%02x", p[j]); puts(""); } while (0)

int main(void)
{
    LAYOUT(Byte); LAYOUT(Short); LAYOUT(Word); LAYOUT(SignedWord);
    struct Byte b;
    struct Short s;
    struct Word w;
    struct SignedWord sw;
    memset(&b, 0xa5, sizeof(b));
    memset(&s, 0xa5, sizeof(s));
    memset(&w, 0xa5, sizeof(w));
    memset(&sw, 0xa5, sizeof(sw));
    long values[] = {-4294967297L, -2147483648L, -1025, -129, -1, 0, 127, 1023, 2147483647L, 4294967297L};
    for (unsigned i = 0; i < sizeof(values) / sizeof(values[0]); i++) {
        b.bits = values[i];
        s.low = values[i];
        s.high = values[i];
        w.low = values[i];
        w.high = values[i];
        sw.bits = values[i];
        printf("values %ld %d %d %lu %lu %ld neighbors %u %u %u %u\n",
            (long)b.bits, s.low, s.high, (unsigned long)w.low, (unsigned long)w.high,
            (long)sw.bits, b.next, s.next, w.next, sw.next);
        BYTES(b); BYTES(s); BYTES(w); BYTES(sw);
    }
    struct Nested n;
    memset(&n, 0x5a, sizeof(n));
    n.items[0].low = -4; n.items[0].high = -1024;
    n.items[1].low = 3; n.items[1].high = 1023;
    printf("nested %zu %zu %zu stride %ld values %d %d %d %d neighbors %u %u\n",
        sizeof(n), _Alignof(struct Nested), offsetof(struct Nested, items),
        (unsigned char *)&n.items[1] - (unsigned char *)&n.items[0],
        n.items[0].low, n.items[0].high, n.items[1].low, n.items[1].high, n.prefix, n.suffix);
    BYTES(n);
    LAYOUT(PrefixByte); LAYOUT(PrefixShort); LAYOUT(PrefixWord);
    struct PrefixByte pb;
    struct PrefixShort ps;
    struct PrefixWord pw;
    memset(&pb, 0x5a, sizeof(pb));
    memset(&ps, 0x5a, sizeof(ps));
    memset(&pw, 0x5a, sizeof(pw));
    for (unsigned i = 0; i < sizeof(values) / sizeof(values[0]); i++) {
        pb.bits = values[i]; ps.low = values[i]; ps.high = values[i]; pw.bits = values[i];
        printf("rebased %d %d %d %ld neighbors %u %u %u %u %u %u\n",
            pb.bits, ps.low, ps.high, (long)pw.bits, pb.prefix, pb.next, ps.prefix, ps.next, pw.prefix, pw.next);
        BYTES(pb); BYTES(ps); BYTES(pw);
    }
    LAYOUT(PrefixWide);
    struct PrefixWide wide;
    memset(&wide, 0x5a, sizeof(wide));
    long wideValues[] = {-68719476736L, -1, 0, 68719476735L, 137438953473L};
    for (unsigned i = 0; i < sizeof(wideValues) / sizeof(wideValues[0]); i++) {
        wide.bits = wideValues[i];
        printf("split-wide %ld neighbors %u %u\n", (long)wide.bits, wide.prefix, wide.next);
        BYTES(wide);
    }
    return 0;
}
