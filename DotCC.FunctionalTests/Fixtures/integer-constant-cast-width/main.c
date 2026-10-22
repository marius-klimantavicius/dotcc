#include <stdio.h>
_Static_assert((unsigned char)-1 == 255, "unsigned byte wraps");
_Static_assert((unsigned short)-1 == 65535, "unsigned short wraps");
_Static_assert((signed char)255 == -1, "signed byte interpretation");
_Static_assert((_Bool)256 == 1, "boolean is nonzero test");
static int values[(unsigned char)258];
int main(void)
{
    printf("%u %u %d %d %d\n", (unsigned char)-1, (unsigned short)-1,
           (int)(sizeof(values) / sizeof(values[0])), (signed char)255, (_Bool)256);
    return 0;
}
