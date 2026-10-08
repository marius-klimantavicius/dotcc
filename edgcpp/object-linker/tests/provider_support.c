int provider_helper(int value) { return value * 3; }
extern int another_unprovided(void);
int another_unused_provider(void) { return another_unprovided(); }
