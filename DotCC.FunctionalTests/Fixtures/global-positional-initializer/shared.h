#ifndef SHARED_H
#define SHARED_H
typedef struct { int (*cb)(void); int omitted; } Clock;
extern Clock shared_clock;
int read_clock(void);
#endif
