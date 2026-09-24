#include <stdio.h>
typedef int state;
struct server { _Atomic(int) state; state next; };
typedef struct wrapped { _Atomic(int) state; state next; } wrapped;
int read_state(_Atomic(int) *state);
state after_prototype;
int read_state(_Atomic(int) *state) { return *state; }
state after_function;
int local_state(void) { _Atomic(int) state=3; state++; return state; }
int main(void) {
    struct server s={0};
    wrapped w={0};
    s.state=9; s.next=3;
    w.state=12; w.next=5;
    after_prototype=read_state(&s.state);
    after_function=read_state(&w.state);
    printf("%d %d %d %d %d\n", after_prototype, s.next,
        after_function, w.next, local_state());
    return 0;
}
