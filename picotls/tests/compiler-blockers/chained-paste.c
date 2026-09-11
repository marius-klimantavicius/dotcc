/* Reduced from include/picotls.h PTLS_CALLBACK_TYPE0. Native C returns 0. */
#define CALLBACK(name) typedef struct st_##name##_t { int (*cb)(struct st_##name##_t *self); } name##_t
CALLBACK(clock);
static int callback(clock_t *self) { return self != 0; }
int main(void) { clock_t value = {callback}; return value.cb(&value) != 1; }
