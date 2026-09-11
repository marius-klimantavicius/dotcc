typedef struct { int (*cb)(void); int omitted; } ptls_get_time_t;
int get_time(void) { return 17; }
extern ptls_get_time_t ptls_get_time;
ptls_get_time_t ptls_get_time = {get_time};
int main(void) { return ptls_get_time.cb() + ptls_get_time.omitted - 17; }
