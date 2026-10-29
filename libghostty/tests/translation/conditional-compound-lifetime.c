#include <stdio.h>

struct Row { int array[2]; };
static int effects;
static int mark(int value) { effects += 1; return value; }

int main(void) {
    int flag = 1;
    int no = 0;
    int selected;
    int initial_effects;
    int *retained;
    int *current;
    int *first = 0;
    int iteration = 0;
    int skipped_and;
    int skipped_or;

    selected = flag
        ? (struct Row){{mark(40), mark(2)}}.array[0]
        : (struct Row){{mark(-1), mark(-2)}}.array[0];
    initial_effects = effects;
    retained = flag
        ? (struct Row){{mark(42), mark(7)}}.array
        : (struct Row){{mark(-3), mark(-4)}}.array;

again:
    current = flag
        ? (struct Row){{mark(40 + iteration), mark(2)}}.array
        : (struct Row){{mark(-5), mark(-6)}}.array;
    if (iteration == 0) {
        first = current;
        iteration = 1;
        goto again;
    }

    skipped_and = no && (struct Row){{mark(-7), mark(-8)}}.array[0];
    skipped_or = flag || (struct Row){{mark(-9), mark(-10)}}.array[0];
    printf("conditional %d %d %d %d %d %d %d %d %d\n", selected, initial_effects,
        retained[0], retained[1], first == current, first[0], effects, skipped_and, skipped_or);
    return selected != 40 || initial_effects != 2 || retained[0] != 42 || retained[1] != 7 ||
        first != current || first[0] != 41 || effects != 8 || skipped_and != 0 || skipped_or != 1;
}
