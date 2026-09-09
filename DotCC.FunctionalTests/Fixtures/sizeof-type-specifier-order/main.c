#include <stdio.h>
#include <stddef.h>
typedef unsigned long long int sqlite_uint64;
typedef sqlite_uint64 u64;
typedef u64 Bitmask;
#define BMS ((int)(sizeof(Bitmask)*8))
struct MaskSet { int n; int ix[BMS]; };
struct Planner { struct MaskSet masks; long long tail; };
typedef int Number;
static void scoped(void) {
    printf("%d ", (int)sizeof(Number));
    {
        typedef long long int Number;
        printf("%d ", (int)sizeof(Number));
        {
            typedef struct Pair { int a; int b; int c; } Number;
            printf("%d ", (int)sizeof(Number));
        }
        printf("%d ", (int)sizeof(Number));
    }
    printf("%d\n", (int)sizeof(Number));
}
int main(void) {
    printf("%d %d %d %d\n", BMS, (int)sizeof(struct MaskSet), (int)sizeof(struct Planner), (int)offsetof(struct Planner, tail));
    printf("%d %d %d %d %d %d\n", (int)sizeof(long int), (int)sizeof(int long unsigned), (int)sizeof(short int), (int)sizeof(int short signed), (int)sizeof(unsigned), (int)sizeof(char *const));
    scoped();
    return 0;
}
