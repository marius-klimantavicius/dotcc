#include <stdio.h>
typedef unsigned long digit;
typedef int other;
struct Pair { int first, digit; digit next; };
typedef struct Wrapped { int first, digit; digit next; } Wrapped;
static int sum(int a, int b) { return a+b; }
static int parameter(int first, int digit) { return first+digit; }
digit after_prototype;
static int declaration(void) {
    int half_eps, digit, round_up, negate=0;
    half_eps=1; digit=2; round_up=4;
    return half_eps+digit+round_up+negate;
}
static int initializers(void) {
    int a=sum(1, sizeof(digit)), digit=(a, 3), *other=&digit;
    return a+digit+*other;
}
static int nested(void) {
    int total=0;
    { int first=1, digit=3; total+=first+digit; }
    digit restored=5;
    { typedef short digit; int first=1, other=sizeof(digit); total+=first+other; }
    return total+(int)restored+sizeof(digit)+sizeof(other);
}
static int loops(void) {
    int total=0;
    for (int first=0, digit=1; first<2; ++first) { total+=digit; }
    digit restored=sizeof(digit);
    for (int first=0, digit=2; first<2; ++first) total+=digit;
    digit again=sizeof(digit);
    for (int first=0, digit=3; first<2; ++first)
        if (first) total+=digit; else total+=2;
    digit after_if=sizeof(digit);
    for (int first=0, digit=2; first<2; ++first)
        for (int j=0; j<2; ++j) total+=digit;
    for (int first=0, digit=1; first<2; ++first)
        do { const char *text="; for }"; total+=digit+text[0]-';'; } while(0);
    digit after_do=sizeof(digit);
    return total+(int)restored+(int)again+(int)after_if+(int)after_do;
}
int main(void) {
    struct Pair p={1,2,3};
    Wrapped w={1,2,3};
    if (w.first+w.digit+w.next!=6) return 8;
    printf("%d %d %d %d %d %d\n", declaration(), initializers(), nested(),
        loops(), parameter(2,3), p.first+p.digit+(int)p.next);
    return 0;
}
