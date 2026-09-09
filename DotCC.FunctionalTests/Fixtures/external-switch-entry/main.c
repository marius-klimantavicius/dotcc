#include <stdio.h>
static int dispatches, initializers;
static int mark(int value) { initializers++; return value; }
static int run(int choice, int external) {
    int result = 0;
    int repeat = 0;
    int local = 31;
    if (external) goto handler;
    switch ((dispatches++, choice)) {
        int skipped = mark(90);
        int values[2] = {mark(1), mark(2)};
        case 0: {
            int local = mark(3);
            handler:
            if (!values) return -99; /* Fail safely if translated storage is missing. */
            skipped = 4;
            values[1] = 7;
            local = 5;
            result += skipped + values[1] + local;
            break;
        }
        case 1: result = 23; break;
    }
    if (!repeat++) goto handler;
    return result + local;
}
static int ordinary(void) {
    int total = 0;
    for (int i = 0; i < 2; i++) {
        switch (i) {
            case 0: case 1: {
                int local = mark(11);
                int values[2] = {mark(12), mark(13)};
                local_label:
                total += local + values[0] + values[1];
                if (i < 0) goto local_label;
                break;
            }
        }
    }
    return total;
}
static int nested_jump_out(void) {
    int n = 0;
    while (n < 1) {
        switch (n) {
            case 0:
                switch (0) { case 0: goto outer_handler; }
                break;
            default:
                outer_handler:
                n++;
                break;
        }
    }
    return n;
}
static int labeled_switch(void) {
    int result = 0;
    goto labeled_handler;
    again:
    switch (mark(0)) {
        case 0:
            labeled_handler:
            result++;
            if (result < 2) goto again;
            break;
    }
    return result;
}
int main(void) {
    int value = run(0, 1);
    printf("%d %d %d\n", value, dispatches, initializers);
    value = run(0, 0);
    printf("%d %d %d\n", value, dispatches, initializers);
    value = run(1, 0);
    printf("%d %d %d\n", value, dispatches, initializers);
    value = ordinary();
    printf("%d %d %d\n", value, dispatches, initializers);
    printf("%d\n", nested_jump_out());
    value = labeled_switch();
    printf("%d %d\n", value, initializers);
    return 0;
}
