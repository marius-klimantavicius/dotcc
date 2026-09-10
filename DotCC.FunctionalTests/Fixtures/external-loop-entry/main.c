#include <stdio.h>

static void while_entry(void) {
    int test = 0, head = 0, total = 0, i = 0;
    goto entered;
    while (++test < 4) {
        head++;
        int local = 90;
        int values[2] = { 5, 6 };
entered:
        local = ++i;
        values[0] = local + 10;
        total += values[0];
        if (i == 1) continue;
        switch (i) { case 2: total += 100; break; default: break; }
        if (i == 3) break;
    }
    printf("while %d %d %d %d\n", test, head, total, i);
}

static void do_entry(void) {
    int test = 0, head = 0, total = 0;
    goto entered;
    do {
        head++;
entered:
        total++;
        if (total == 1) continue;
        if (total == 3) break;
    } while (++test < 10);
    printf("do %d %d %d\n", test, head, total);
}

static void for_entry(void) {
    int init = 0, test = 0, post = 0, head = 0, i = 0;
    goto entered;
    for (init++; ++test < 10; post++) {
        head++;
entered:
        i++;
        if (i == 1) continue;
        for (int j = 0; j < 2; j++) { if (j == 0) continue; break; }
        if (i == 3) break;
    }
    printf("for %d %d %d %d %d\n", init, test, post, head, i);
}

static void tokenizer_entry(int ascii) {
    int i = 0, total = 0, conditions = 0;
    while (1) {
        if (ascii) goto ascii_token;
        goto non_ascii_token;
    }
    while (++conditions && i < 3) {
        if (ascii) {
            if (i < 4) {
ascii_token:
                total += 1;
            }
        } else {
non_ascii_token:
            total += 10;
        }
        i++;
    }
    printf("token %d %d %d\n", i, total, conditions);
}

int main(void) {
    while_entry(); do_entry(); for_entry();
    tokenizer_entry(0); tokenizer_entry(1);
    return 0;
}
