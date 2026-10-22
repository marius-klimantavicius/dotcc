#include <stdio.h>
extern unsigned char key[];
extern unsigned char *pointer;
extern int rows[][3];
extern unsigned char known[32];
int untouched(void);

static int fill(void *buffer) {
    unsigned char *bytes = buffer;
    bytes[0] = 21;
    bytes[31] = 42;
    return 1;
}

int main(void) {
    if (!untouched()) return 1;
    if ((void *)&key != (void *)key) return 2;
    if (!fill(&key) || key[0] != 21 || key[31] != 42) return 3;
    unsigned char **slot = &pointer;
    if (*slot != key || (void *)slot == (void *)key) return 4;
    *slot = key + 31;
    if (pointer != key + 31 || *pointer != 42) return 5;
    if ((void *)&known != (void *)known || sizeof known != 32) return 6;
    unsigned char (*whole)[32] = &known;
    if ((unsigned char *)(whole + 1) - (unsigned char *)whole != 32) return 7;
    if (sizeof rows[0] != 3 * sizeof(int) || rows[1][2] != 6) return 8;
    int (*row)[3] = &rows[1];
    if (sizeof *row != 3 * sizeof(int) || (*row)[2] != 6) return 9;
    if ((char *)(row + 1) - (char *)row != 3 * sizeof(int)) return 10;
    printf("array %d %d %d\n", key[0], key[31], *pointer);
    return 0;
}
