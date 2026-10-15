int ifunc_target(void) { return 5; }
static int (*ifunc_resolver(void))(void) { return ifunc_target; }
int ifunc_entry(void) __attribute__((ifunc("ifunc_resolver")));
