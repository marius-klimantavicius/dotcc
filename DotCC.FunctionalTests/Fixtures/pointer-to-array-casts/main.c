#include <stdlib.h>
#include <stdio.h>

static int selections;
static void *choose(void *memory) { ++selections; return memory; }

int main(void) {
    int *memory = calloc(18, sizeof(int));
    int (*rows)[3] = (int (*)[3])choose(memory);
    rows[1][2] = 17;
    ((int (*)[3])memory)[2][1] = 29;
    int (*cube)[2][3] = (int (*)[2][3])memory;
    cube[1][1][2] = 43;
    const int (*view)[2][3] = (const int (*)[2][3])memory;
    long row_stride = (char*)(rows + 2) - (char*)rows;
    long cube_stride = (char*)(cube + 1) - (char*)cube;
    if (selections != 1 || memory[5] != 17 || memory[7] != 29 || memory[11] != 43) return 1;
    if (sizeof(*rows) != 12 || row_stride != 24 || cube_stride != 24 || view[1][1][2] != 43) return 2;
    typedef unsigned int BlockNumber;
    enum { MAX_FORKNUM = 3 };
    BlockNumber (*blocks)[MAX_FORKNUM + 1];
    blocks = (BlockNumber (*)[MAX_FORKNUM + 1])memory;
    blocks[3][2] = 0xf0000000U;
    if (((BlockNumber*)memory)[14] != 0xf0000000U || (char*)(blocks + 1) - (char*)blocks != 16) return 3;
    printf("%d %d %d %ld %ld %d\n", memory[5], memory[7], memory[11], row_stride, cube_stride, selections);
    free(memory);
    return 0;
}
