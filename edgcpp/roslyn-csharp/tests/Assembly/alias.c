int alias_target(void) { return 4; }
int alias_entry(void) __attribute__((alias("alias_target")));
