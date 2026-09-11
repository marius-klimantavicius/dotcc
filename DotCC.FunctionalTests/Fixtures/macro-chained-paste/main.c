#include <stdio.h>

/* Reduced from picotls's callback declarations. */
#define CALLBACK(name) typedef struct st_##name##_t { int (*cb)(struct st_##name##_t *self); } name##_t
CALLBACK(clock);
static int callback(clock_t *self) { return self != 0; }

#define name wrong
#define prename wrong
#define prename_post_end 42
#define VALUE(n) pre##n##_post##_end
#define CAT(a,b,c) a##b##c

int main(void)
{
    clock_t value = {callback};
    int prefix_suffix = 7;
    printf("callback=%d rescan=%d empty=%d\n",
           value.cb(&value), VALUE(name), CAT(prefix_, , suffix));
    return 0;
}
