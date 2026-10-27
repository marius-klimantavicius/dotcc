/* Static storage bytes are the oracle, including zero padding. Every table is
   at least 1024 bytes so --aggregate-data can be selected independently. */
enum Choice { Minus = -7, Plus = 0x1234 };

struct __attribute__((packed)) Packet {
    unsigned char marker;
    unsigned a:3;
    unsigned b:9;
    signed c:7;
    unsigned d:5;
    unsigned short tail;
};
#define PACKETS { \
    [0] = {0x91, 5, 0x12f, -17, 27, 0x1234}, \
    [1] = {.tail=0xabcd, .c=-64, .a=7, .b=1, .d=3}, \
    [255] = {.marker=0xee, .b=0x1ff, .c=63, .tail=0xfedc} }
static struct Packet packets[256] = PACKETS;
static struct Packet twin[256] = PACKETS;

/* payload starts at bit 3 and occupies parts of NINE bytes. */
struct __attribute__((packed)) Wide {
    unsigned lead:3;
    unsigned long long payload:64;
    unsigned last:2;
    unsigned char tail;
};
static struct Wide wide[128] = {
    [0] = {5, 0xfedcba9876543210ULL, 2, 0xa5},
    [1] = {7, 0xffffffffffffffffULL, 3, 0x5a},
    [127] = {.lead=1, .payload=0x8000000000000001ULL, .tail=0x69}
};

/* Ordinary members share the underlying allocation units with mixed types. */
struct Mixed {
    unsigned char prefix;
    unsigned char a:3;
    unsigned b:7;
    unsigned short c:5;
    signed int d:6;
    unsigned char tail;
};
static struct Mixed mixed[128] = {
    [0] = {0x12, 5, 93, 19, -17, 0x34},
    [1] = {.prefix=0xfe, .a=7, .b=127, .c=31, .d=-32, .tail=0xdc},
    [127] = {.prefix=9, .d=31, .tail=8}
};

struct Scalar {
    signed char narrow;
    unsigned char wrap;
    short signed16;
    unsigned short unsigned16;
    _Bool yes;
    _Bool no;
    enum Choice choice;
    unsigned high;
};
static struct Scalar scalars[64] = {
    [0] = {(signed char)239, (unsigned char)-1, (short)65519,
           (unsigned short)-2, -3, 0, Minus, 0x80000000u},
    [1] = {.narrow=-128, .wrap=(unsigned char)256, .signed16=-32768,
           .unsigned16=65535, .yes=256, .choice=Plus, .high=0xffffffffu},
    [63] = {.narrow=127, .no=9, .choice=Minus}
};

struct Nested { unsigned char tag; short cells[2][3]; unsigned char tail; };
static struct Nested nested[80] = {
    [0] = {.tag=0x19, .cells={{-1, 2}, {3, 4, 5}}, .tail=0x27},
    [1] = {.cells={{0}, {0, 0, 99}}},
    [1] = {.tag=0x44, .cells={{7}}, .tail=0x55},
    [79] = {.cells={[1][2]=-1234}, .tail=0xa7}
};

union Overlay { unsigned word; unsigned char bytes[4]; };
static union Overlay overlays[256] = {
    [0] = {.word=0x11223344u, .bytes={0xaa, 0xbb}},
    [1] = {.bytes={1, 2, 3, 4}, .word=0x55667788u},
    [255] = {.word=0xaabbccddu}
};

struct Floating { double wrapped; float rounded; double negative_zero; double comparison; };
static struct Floating floating[40] = {
    [0] = {.wrapped=(double)(0xffffffffu + 1u), .rounded=16777217.0f,
           .negative_zero=-0.0, .comparison=(double)((0u - 1u) > 0u)},
    [1] = {.wrapped=0.1, .rounded=-0.0f, .negative_zero=-0.0},
    [39] = {.wrapped=(double)(unsigned char)257, .rounded=1.25f}
};

unsigned long aggregate_address(void) { return (unsigned long)packets; }
int aggregate_first(void) { return packets[0].marker; }
int aggregate_mutate(void) {
    packets[0].marker = 0x42;
    packets[0].b = 7;
    return &packets[0] != &twin[0] && twin[0].marker == 0x91 && twin[0].b == 0x12f
        && packets[0].marker == 0x42 && packets[0].b == 7;
}
int aggregate_values(void) {
    return wide[0].lead == 5 && wide[0].payload == 0xfedcba9876543210ULL
        && wide[0].last == 2 && wide[0].tail == 0xa5
        && wide[1].payload == 0xffffffffffffffffULL && wide[127].payload == 0x8000000000000001ULL
        && mixed[0].prefix == 0x12 && mixed[0].a == 5 && mixed[0].b == 93
        && mixed[0].c == 19 && mixed[0].d == -17 && mixed[0].tail == 0x34
        && scalars[0].narrow == -17 && scalars[0].wrap == 255 && scalars[0].signed16 == -17
        && scalars[0].unsigned16 == 65534 && scalars[0].yes == 1 && scalars[0].no == 0
        && scalars[0].choice == Minus && scalars[1].yes == 1 && scalars[63].no == 1
        && nested[1].cells[1][2] == 0 && nested[1].cells[0][0] == 7
        && overlays[0].bytes[0] == 0xaa && overlays[0].bytes[1] == 0xbb
        && overlays[0].bytes[2] == 0 && overlays[0].bytes[3] == 0
        && overlays[1].word == 0x55667788u && floating[0].wrapped == 0.0
        && floating[0].rounded == 16777216.0f && floating[0].comparison == 1.0;
}
int aggregate_count(void) { return 7; }
int aggregate_row_size(int table) {
    switch (table) {
    case 0: return sizeof(packets[0]);
    case 1: return sizeof(wide[0]);
    case 2: return sizeof(mixed[0]);
    case 3: return sizeof(scalars[0]);
    case 4: return sizeof(nested[0]);
    case 5: return sizeof(overlays[0]);
    default: return sizeof(floating[0]);
    }
}
int aggregate_rows(int table) {
    switch (table) {
    case 0: return 256;
    case 1: return 128;
    case 2: return 128;
    case 3: return 64;
    case 4: return 80;
    case 5: return 256;
    default: return 40;
    }
}
unsigned char *aggregate_bytes(int table) {
    switch (table) {
    case 0: return (unsigned char *)packets;
    case 1: return (unsigned char *)wide;
    case 2: return (unsigned char *)mixed;
    case 3: return (unsigned char *)scalars;
    case 4: return (unsigned char *)nested;
    case 5: return (unsigned char *)overlays;
    default: return (unsigned char *)floating;
    }
}
int aggregate_zero_gaps(void) {
    for (int table=0; table<aggregate_count(); ++table) {
        int size=aggregate_row_size(table), rows=aggregate_rows(table);
        unsigned char *bytes=aggregate_bytes(table);
        for (int i=2*size; i<(rows-1)*size; ++i) if (bytes[i] != 0) return 0;
    }
    return 1;
}
int aggregate_wide_store(void) {
    wide[0].payload = 0x0123456789abcdefULL;
    if (wide[0].lead != 5 || wide[0].last != 2 || wide[0].tail != 0xa5) return 0;
    if (wide[0].payload != 0x0123456789abcdefULL) return 0;
    wide[0].payload = 0xfedcba9876543210ULL;
    return wide[0].payload == 0xfedcba9876543210ULL && wide[0].lead == 5 && wide[0].last == 2;
}

struct Local { int first; int last; };
#define LOCALS {{17, 19}, [127]={23, 29}}
static _Alignas(64) struct Local aligned_global[128] = LOCALS;
int aggregate_aligned_global(void) {
    return (unsigned long)aligned_global % 64 == 0 && aligned_global[0].first == 17
        && aligned_global[1].first == 0 && aligned_global[127].last == 29;
}
int aggregate_local(void) {
    struct Local first[128] = LOCALS;
    struct Local second[128] = LOCALS;
    int valid = first[0].first == 17 && second[0].first == 17
        && first[1].last == 0 && first[127].last == 29;
    first[0].first = 99;
    return valid && &first[0] != &second[0] && second[0].first == 17;
}
int aggregate_aligned_local(void) {
    _Alignas(64) struct Local aligned[128] = LOCALS;
    int valid = (unsigned long)aligned % 64 == 0 && aligned[0].first == 17
        && aligned[1].first == 0 && aligned[127].last == 29;
    aligned[0].first = 99;
    return valid;
}
static _Thread_local struct Local thread_rows[128] = LOCALS;
int aggregate_tls_step(void) { return thread_rows[0].first++; }
unsigned long aggregate_tls_address(void) { return (unsigned long)thread_rows; }
