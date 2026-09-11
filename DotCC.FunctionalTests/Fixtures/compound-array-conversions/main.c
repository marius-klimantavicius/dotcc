#include <stdio.h>

struct flags { unsigned value : 10; };
enum code { green = 260 };
static int calls;
static int next(void) { return 256 + ++calls; }

int main(void)
{
    int value = 257;
    struct flags flags = {259};
    enum code color = green;
    unsigned char *bytes = (unsigned char[]){value, flags.value, value > 0, !!value, -1, next(), next(), next(), color};
    void *raw = &value;
    int **pointers = (int *[]){0, raw};
    printf("%u %u %u %u %u %u %d %u\n", (unsigned)bytes[0], (unsigned)bytes[1], (unsigned)bytes[2],
           (unsigned)bytes[3], (unsigned)bytes[4], (unsigned)(bytes[5] + bytes[6] + bytes[7]), calls, (unsigned)bytes[8]);
    printf("%d %d\n", pointers[0] == 0, *pointers[1]);
    return 0;
}
