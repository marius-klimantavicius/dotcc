extern int provider_helper(int);
extern int intentionally_unprovided(void);
static int callback(int value) { return provider_helper(value) + 1; }
int (*provider_callback)(int) = callback;
int provider_api(int value) { return provider_helper(value) + 2; }
int unused_provider(void) { return intentionally_unprovided(); }
int (*unused_provider_storage)(void) = intentionally_unprovided;
