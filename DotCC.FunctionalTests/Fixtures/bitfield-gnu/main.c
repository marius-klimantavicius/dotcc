#include <stdio.h>
#include <stddef.h>
#include <string.h>
struct Prefix { unsigned char prefix; unsigned a:2; unsigned b:7; unsigned short tail; };
struct Mixed { unsigned char prefix; unsigned char a:3; unsigned b:7; unsigned short c:5; signed int d:6; unsigned char tail; };
struct Cross { unsigned char prefix[3]; unsigned a:12; unsigned char tail; };
struct Zero { unsigned char prefix; unsigned :0; unsigned char tail; };
struct Unnamed { unsigned char prefix; unsigned :2; unsigned char tail; };
union U { unsigned a:3; unsigned b:5; };
#define SHOW(T) printf(#T " %zu %zu %zu\n",sizeof(struct T),_Alignof(struct T),offsetof(struct T,tail))
#define BYTES(x) do { unsigned char*image=(unsigned char*)&x;for(size_t i=0;i<sizeof(x);i++)printf("%02x",image[i]);puts(""); }while(0)
int main(void)
{
    SHOW(Prefix); SHOW(Mixed); SHOW(Cross); SHOW(Zero); SHOW(Unnamed);
    struct Prefix p = {0};
    memset(&p, 0, sizeof(p));
    p.prefix = 0xa5; p.a = 3; p.b = 99; p.tail = 0x5a5a;
    BYTES(p);
    struct Mixed m = {0};
    memset(&m, 0, sizeof(m));
    m.prefix = 0xa5; m.a = 5; m.b = 99; m.c = 19; m.d = -7; m.tail = 0x5a;
    BYTES(m);
    m.prefix = 0x3c; m.tail = 0xc3;
    printf("mixed values %u %u %u %d %u %u\n", m.a, m.b, m.c, m.d, m.prefix, m.tail);
    m.d = 31; m.b = 127; m.c = 0; m.a = 7;
    BYTES(m);
    union U u = {0};
    u.a = 7; u.b = 16;
    printf("union %u %u\n", u.a, u.b);
    return 0;
}
