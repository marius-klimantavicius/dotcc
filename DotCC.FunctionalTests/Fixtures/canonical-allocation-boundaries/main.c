#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <string.h>

typedef int (*Callback)(int);
struct Box { int value; };
static int add(int x) { return x + 1; }
static Callback table[] = {add, &add};
static int *saved;

static struct Box *make(int value) {
    struct Box *box = malloc(sizeof(struct Box));
    if (box) box->value = value;
    return box;
}

int main(void) {
    Callback local = add;
    void *erased = (void *)&add;
    int (*runtime)(int) = abs;
    void *(*allocate)(size_t) = malloc;
    int *values = malloc(4 * sizeof(int));
    struct Box *box = make(1);
    void *extra = allocate(sizeof(struct Box));
    if (!values || !box || !extra) return 1;
    int **slot = &saved;
    *slot = values;
    saved[0] = 41;
    if (local(41) != 42 || table[0] != local || table[1] != &add
        || erased != (void *)add || runtime != &abs || runtime(-42) != 42
        || strlen("typed") != 5) return 2;
    if ((intptr_t)saved != (intptr_t)values || *slot != values) return 3;
    if (!(sizeof(struct Box) < -1) || sizeof(struct Box) + (size_t)-1 != 3) return 4;
    int result = saved[0] + box->value;
    free(extra);
    free(box);
    free(values);
    *slot = NULL;
    if (saved != NULL) return 5;
    printf("canonical allocation %d\n", result);
    return 0;
}
