struct Wrapper { int value; };
static inline int helper(struct Wrapper *p) { return p->value; }
static struct Wrapper first_state = {7};
int first_value(void) { return helper(&first_state); }
