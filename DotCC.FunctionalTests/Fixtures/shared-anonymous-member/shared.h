#ifndef SHARED_ANONYMOUS_H
#define SHARED_ANONYMOUS_H
struct shared {
    char prefix;
    union {
        struct { int number; unsigned int mask; } first;
        struct { int number; unsigned int mask; } second;
        long bits;
    };
    struct { short code; short omitted; } named;
    enum shared_state { READY = 7, DONE = 9 } state;
};
int read_shared(struct shared *value);
int shared_layout(void);
#endif
