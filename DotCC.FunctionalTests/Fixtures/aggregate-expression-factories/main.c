#include <stdio.h>
#include <stdbool.h>

/* Every make_* return is an expression aggregate with an inline C array.
   These are not standalone static-table or local-array initialization tests. */
struct U16 { unsigned short values[257]; };
struct U32 { unsigned values[257]; };
struct U64 { unsigned long long values[257]; };
struct Signed { signed char a[257]; short b[257]; int c[257]; long long d[257]; };
struct Floating { float single[257]; double wide[257]; };
struct Zero { unsigned long long values[8192]; };
struct RGB { unsigned char r, g, b; };
struct Colors { struct RGB values[512]; };
struct Bits { unsigned a:3; unsigned b:9; signed c:7; unsigned d:13; };
struct PackedBits { unsigned char low:4; unsigned char high:4; };
struct BitRows { struct Bits values[256]; struct PackedBits packed[8192]; };
enum Tag { TAG_ZERO, TAG_SEVEN = 7 };
struct Flags { bool flags[257]; enum Tag tags[257]; };

_Static_assert(sizeof(unsigned short) == 2, "16-bit short fixture");
_Static_assert(sizeof(unsigned) == 4, "32-bit int fixture");
_Static_assert(sizeof(unsigned long long) == 8, "64-bit long long fixture");
_Static_assert(sizeof(struct RGB) == 3, "RGB layout");
_Static_assert(sizeof(struct Bits) == 4, "complete 32-bit bitfield allocation unit");
_Static_assert(sizeof(struct PackedBits) == 1, "complete 8-bit bitfield allocation unit");

struct U16 make_u16(void) {
    return (struct U16){{0x1234, [128]=0x8001, [256]=0xffff}};
}
struct U32 make_u32(void) {
    return (struct U32){{0x12345678u, [128]=0x80000001u, [256]=0xffffffffu}};
}
struct U64 make_u64(void) {
    return (struct U64){{0x0123456789abcdefULL, [128]=0x8000000000000001ULL,
                        [256]=0xffffffffffffffffULL}};
}
struct Signed make_signed(void) {
    return (struct Signed){
        {-128, [128]=-1, [256]=127},
        {-32768, [128]=-1, [256]=32767},
        {(-2147483647-1), [128]=-1, [256]=2147483647},
        {(-9223372036854775807LL-1), [128]=-1, [256]=9223372036854775807LL}
    };
}
struct Floating make_floating(void) {
    return (struct Floating){
        {-0.0f, 0x1p-149f, [128]=0x1p-126f, [256]=0x1.fffffep127f},
        {-0.0, 0x1p-1074, [128]=0x1p-1022, [256]=0x1.fffffffffffffp1023}
    };
}
struct Zero make_zero(void) { return (struct Zero){{0}}; }
struct Colors make_colors(void) {
    return (struct Colors){{{1,2,3}, [255]={0x80,0x91,0xa2}, [511]={0xfe,0xfd,0xfc}}};
}
struct BitRows make_bits(void) {
    return (struct BitRows){
        {{5,0x12f,-17,0x1234}, [255]={7,511,63,8191}},
        {{3,10}, [4096]={15,8}, [8191]={7,12}}
    };
}
struct Flags make_flags(void) {
    return (struct Flags){{true, [128]=true, [256]=true},
                         {TAG_SEVEN, [128]=TAG_SEVEN, [256]=TAG_SEVEN}};
}

static unsigned float_bits(float value) {
    union { float value; unsigned bits; } result = { .value=value };
    return result.bits;
}
static unsigned long long double_bits(double value) {
    union { double value; unsigned long long bits; } result = { .value=value };
    return result.bits;
}

int factory_constants(void) {
    struct U16 a=make_u16(), twin=make_u16();
    struct U32 b=make_u32();
    struct U64 c=make_u64();
    struct Signed s=make_signed();
    struct Floating f=make_floating();
    struct Zero z=make_zero();
    struct Colors colors=make_colors();
    struct BitRows bits=make_bits();
    struct Flags flags=make_flags();
    if (a.values[0]!=0x1234 || a.values[128]!=0x8001 || a.values[256]!=0xffff ||
        b.values[0]!=0x12345678u || b.values[128]!=0x80000001u || b.values[256]!=0xffffffffu ||
        c.values[0]!=0x0123456789abcdefULL || c.values[128]!=0x8000000000000001ULL ||
        c.values[256]!=0xffffffffffffffffULL) return 0;
    if (s.a[0]!=-128 || s.a[128]!=-1 || s.a[256]!=127 || s.b[0]!=-32768 ||
        s.b[128]!=-1 || s.b[256]!=32767 || s.c[0]!=(-2147483647-1) ||
        s.c[128]!=-1 || s.c[256]!=2147483647 || s.d[0]!=(-9223372036854775807LL-1) ||
        s.d[128]!=-1 || s.d[256]!=9223372036854775807LL) return 0;
    if (float_bits(f.single[0])!=0x80000000u || float_bits(f.single[1])!=1 ||
        float_bits(f.single[128])!=0x00800000u || float_bits(f.single[256])!=0x7f7fffffu ||
        double_bits(f.wide[0])!=0x8000000000000000ULL || double_bits(f.wide[1])!=1 ||
        double_bits(f.wide[128])!=0x0010000000000000ULL ||
        double_bits(f.wide[256])!=0x7fefffffffffffffULL) return 0;
    if (colors.values[0].r!=1 || colors.values[255].g!=0x91 || colors.values[511].b!=0xfc ||
        bits.values[0].a!=5 || bits.values[0].b!=0x12f || bits.values[0].c!=-17 ||
        bits.values[0].d!=0x1234 || bits.values[255].c!=63 || bits.values[255].d!=8191 ||
        ((unsigned char *)&bits.packed[0])[0]!=0xa3 ||
        ((unsigned char *)&bits.packed[4096])[0]!=0x8f ||
        ((unsigned char *)&bits.packed[8191])[0]!=0xc7) return 0;
    for (int i=0; i<257; ++i) {
        if (i==0 || i==128 || i==256) {
            if (!flags.flags[i] || flags.tags[i]!=TAG_SEVEN) return 0;
        } else if (a.values[i] || b.values[i] || c.values[i] || s.a[i] || s.b[i] ||
                   s.c[i] || s.d[i] || flags.flags[i] || flags.tags[i]) return 0;
        if (i!=0 && i!=1 && i!=128 && i!=256 && (float_bits(f.single[i]) || double_bits(f.wide[i]))) return 0;
    }
    for (int i=0; i<8192; ++i) {
        if (z.values[i]) return 0;
        if (i!=0 && i!=4096 && i!=8191 && (bits.packed[i].low || bits.packed[i].high)) return 0;
    }
    for (int i=1; i<512; ++i)
        if (i!=255 && i!=511 && (colors.values[i].r || colors.values[i].g || colors.values[i].b)) return 0;
    for (int i=1; i<255; ++i)
        if (bits.values[i].a || bits.values[i].b || bits.values[i].c || bits.values[i].d) return 0;
    a.values[0]=99;
    return &a.values[0]!=&twin.values[0] && twin.values[0]==0x1234;
}

static int evaluations;
static int evaluated(int value) { ++evaluations; return value; }
struct Dynamic { int values[257]; int marker; };
struct Dynamic make_dynamic(int seed) {
    return (struct Dynamic){{evaluated(seed), 17, [128]=evaluated(seed+3),
                            [256]=evaluated(seed+9)}, evaluated(seed+11)};
}
struct Dynamic make_lazy(int choose, int seed) {
    return choose ? (struct Dynamic){{evaluated(seed), [256]=evaluated(seed+1)}, 3}
                  : (struct Dynamic){{evaluated(seed+20), [128]=evaluated(seed+21)}, 7};
}
int factory_dynamic(void) {
    evaluations=0;
    struct Dynamic first=make_dynamic(5);
    if (evaluations!=4 || first.values[0]!=5 || first.values[1]!=17 ||
        first.values[128]!=8 || first.values[256]!=14 || first.marker!=16) return 0;
    struct Dynamic a=make_lazy(1,40), b=make_lazy(0,60);
    if (evaluations!=8 || a.values[0]!=40 || a.values[256]!=41 || a.marker!=3 ||
        a.values[128] || b.values[0]!=80 || b.values[128]!=81 || b.values[256] || b.marker!=7) return 0;
    for (int pass=0; pass<3; ++pass) {
        struct Dynamic again=make_dynamic(pass+2);
        if (again.values[0]!=pass+2 || again.values[1]!=17 || again.values[128]!=pass+5 ||
            again.values[256]!=pass+11 || again.marker!=pass+13) return 0;
        for (int i=2; i<256; ++i)
            if (i!=128 && again.values[i]) return 0;
        again.values[128]=-999;
    }
    return evaluations==20 && first.values[128]==8;
}

typedef int (*Callback)(int);
struct Callbacks { Callback values[257]; int *addresses[257]; };
static int root_value=23;
static int plus(int value) { return value+3; }
static int times(int value) { return value*2; }
struct Callbacks make_callbacks(void) {
    return (struct Callbacks){{plus, [128]=times, [256]=plus}, {&root_value, [256]=&root_value}};
}
static struct Callbacks saved;
int factory_save(void) { saved=make_callbacks(); return 1; }
int factory_lifetime(void) {
    struct Callbacks first=make_callbacks(), second=make_callbacks();
    if (first.values[0]!=plus || first.values[128]!=times || first.values[256]!=plus ||
        first.values[0]!=second.values[0] || first.values[0]!=saved.values[0] ||
        first.values[128](7)!=14 || saved.values[0](11)!=14 ||
        first.addresses[0]!=&root_value || saved.addresses[256]!=&root_value) return 0;
    for (int i=1; i<256; ++i) {
        if (i!=128 && (first.values[i] || saved.values[i])) return 0;
        if (first.addresses[i] || saved.addresses[i]) return 0;
    }
    first.values[0]=times;
    *saved.addresses[0]=29;
    return first.values[0](4)==8 && second.values[0](4)==7 && saved.values[0](4)==7 &&
           *second.addresses[256]==29 && saved.addresses[0]==&root_value;
}

int main(void) {
    int constants=factory_constants(), dynamic=factory_dynamic();
    factory_save();
    int lifetime=factory_lifetime();
    printf("aggregate-expression constants=%d dynamic=%d callback-lifetime=%d\n", constants,dynamic,lifetime);
    return !(constants && dynamic && lifetime);
}
