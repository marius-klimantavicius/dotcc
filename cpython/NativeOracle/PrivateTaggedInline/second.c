struct Wrapper { long pad; int value; };
static inline int helper(struct Wrapper *p) { return p->value; }
static struct Wrapper second_state = {100,42};
int second_value(void) { return helper(&second_state); }
