#include <stdio.h>

/* GCC LP64 conversion policy: retain the low bits on signed/enum conversion. */
typedef enum Flags { NONE = 0, ONE = 1 } Flags;
static long completion_mask(void) { return (long)0x8000000000000000ULL; }
static Flags set_flags(Flags flags) { flags |= (Flags)0x80000000U; return flags; }
static long implicit_mask(void) { return 0xffffffffffffffffULL; }
static Flags implicit_flags(void) { return 0x80000000U; }
int main(void) {
    printf("%ld %d %ld %d\n", completion_mask(), (int)set_flags(ONE),
           implicit_mask(), (int)implicit_flags());
    return 0;
}
