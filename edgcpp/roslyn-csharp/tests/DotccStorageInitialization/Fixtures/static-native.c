#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>

int static_observe(int);
void static_change(int);
int *static_address(int);

int main(void) {
    int before[16], after[16];
    int *addresses[7];
    for (int i = 0; i < 16; ++i) before[i] = static_observe(i);
    for (int i = 0; i < 7; ++i) addresses[i] = static_address(i);
    if ((uintptr_t)addresses[3] % 64) abort();
    for (int i = 0; i < 7; ++i)
        for (int j = 0; j < i; ++j) if (addresses[i] == addresses[j]) abort();
    if (*addresses[0] != 0 || *addresses[1] != 29 || *addresses[2] != 53 ||
        *addresses[3] != 67 || *addresses[4] != 7 || *addresses[5] != 5 || *addresses[6] != 5) abort();
    static_change(19);
    for (int i = 0; i < 16; ++i) after[i] = static_observe(i);
    if (*addresses[5] != 24 || *addresses[6] != 5) abort();
    for (int i = 0; i < 7; ++i) if (addresses[i] != static_address(i)) abort();
    printf("static-before=");
    for (int i = 0; i < 16; ++i) printf("%s%d", i ? "," : "", before[i]);
    printf("\nstatic-after=");
    for (int i = 0; i < 16; ++i) printf("%s%d", i ? "," : "", after[i]);
    puts("\nstatic-addresses=stable-aligned-distinct");
    return 0;
}
