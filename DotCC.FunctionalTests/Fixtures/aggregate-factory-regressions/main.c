#include <stdio.h>
#include <stdbool.h>

struct Pair { int values[2]; };
struct Mixed { int head; int values[2]; int tail; };
struct Flags { bool values[2]; };
struct Addresses { int *values[2]; };
struct Row { int value; };
struct Rows { struct Row values[2]; };
static int trace;
static int mark(int value) { trace=trace*10+value; return value; }
static void note(int value) { trace=trace*10+value; }
static struct Row row(int value) { return (struct Row){mark(value)}; }
static int pair_last(struct Pair value) { return value.values[1]; }

/* C initializer operands are indeterminately sequenced: complete operands may
   be reordered, but their comma subexpressions cannot interleave. The backend
   chooses left-to-right evaluation, also used by this native reference. */
int factory_comma_trace(void) {
    trace=0;
    struct Pair pair=(struct Pair){{(mark(1),mark(2)),(mark(3),mark(4))}};
    return pair.values[0]==2 && pair.values[1]==4 ? trace : -1;
}

int factory_mixed_sequence(void) {
    trace=0;
    struct Mixed mixed=(struct Mixed){mark(1),
        {(mark(2),mark(3)),(mark(4),mark(5))},(mark(6),mark(7))};
    if (trace!=1234567 || mixed.head!=1 || mixed.values[0]!=3 ||
        mixed.values[1]!=5 || mixed.tail!=7) return 0;
    trace=0;
    struct Rows rows=(struct Rows){{(note(1),row(2)),(note(3),row(4))}};
    if (trace!=1234 || rows.values[0].value!=2 || rows.values[1].value!=4) return 0;
    trace=0;
    struct Flags flags=(struct Flags){{(note(1),mark(2)>1),(note(3),mark(4)<1)}};
    return trace==1234 && flags.values[0] && !flags.values[1];
}

int factory_addressed_sequence(int choose) {
    int local=7;
    int *address=&local;
    trace=0;
    /* A void prefix would require a capturing delegate if every value were
       forced through NoHoist. The addressed local must stay on the C stack. */
    struct Pair pair=choose
        ? (struct Pair){{(note(1),local=2,*address),(note(3),local=4,*address)}}
        : (struct Pair){{(note(5),local=6,*address),(note(7),local=8,*address)}};
    if (trace!=(choose ? 13 : 57) || pair.values[0]!=(choose ? 2 : 6) ||
        pair.values[1]!=(choose ? 4 : 8)) return 0;
    trace=0;
    struct Addresses pointers=(struct Addresses){{(note(1),&local),(note(2),address)}};
    *pointers.values[0]=19;
    return trace==12 && pointers.values[0]==address && *pointers.values[1]==19;
}

int factory_lazy_and_loop(void) {
    trace=0;
    int choose=0;
    struct Pair lazy=choose
        ? (struct Pair){{(mark(5),mark(6)),(mark(7),mark(8))}}
        : (struct Pair){{(mark(1),mark(2)),(mark(3),mark(4))}};
    if (trace!=1234 || lazy.values[0]!=2 || lazy.values[1]!=4) return 0;
    int loops=0;
    while (loops<2 && pair_last((struct Pair){{(mark(1),mark(2)),(mark(3),mark(4))}})==4) {
        if (trace!=12341234 && trace!=1234) return 0;
        trace=0;
        ++loops;
    }
    return loops==2 && trace==0;
}

struct Floating { float single[8]; double wide[10]; };
struct Floating make_typed_floating(void) {
    return (struct Floating){
        {(float)(0xffffffffu+1u), (float)(7/2), (float)(unsigned char)257,
         (float)(unsigned short)65537, (float)(0u-1u), (float)(-7/2), -0.0f, 0x1p-149f},
        {(double)(0xffffffffu+1u), (double)(7/2), (double)(unsigned char)257,
         (double)(unsigned short)65537, (double)(0u-1u), (double)(-7/2), -0.0, 0x1p-1074,
         (double)((0xffffffffu+2u)/2u), (double)(0xffffffffffffffffULL+1ULL)}
    };
}
static unsigned float_bits(float value) {
    union { float value; unsigned bits; } result={.value=value};
    return result.bits;
}
static unsigned long long double_bits(double value) {
    union { double value; unsigned long long bits; } result={.value=value};
    return result.bits;
}
int factory_typed_floating(void) {
    struct Floating f=make_typed_floating();
    return float_bits(f.single[0])==0 && f.single[1]==3.0f && f.single[2]==1.0f &&
        f.single[3]==1.0f && float_bits(f.single[4])==0x4f800000u && f.single[5]==-3.0f &&
        float_bits(f.single[6])==0x80000000u && float_bits(f.single[7])==1 &&
        double_bits(f.wide[0])==0 && f.wide[1]==3.0 && f.wide[2]==1.0 &&
        f.wide[3]==1.0 && f.wide[4]==4294967295.0 && f.wide[5]==-3.0 &&
        double_bits(f.wide[6])==0x8000000000000000ULL && double_bits(f.wide[7])==1 &&
        double_bits(f.wide[8])==0 && double_bits(f.wide[9])==0;
}

int main(void) {
    int comma=factory_comma_trace(), mixed=factory_mixed_sequence();
    int addressed=factory_addressed_sequence(0) && factory_addressed_sequence(1);
    int lazy=factory_lazy_and_loop(), floating=factory_typed_floating();
    printf("aggregate-regressions trace=%d mixed=%d addressed=%d lazy=%d floating=%d\n",
        comma,mixed,addressed,lazy,floating);
    return !(comma==1234 && mixed && addressed && lazy && floating);
}
