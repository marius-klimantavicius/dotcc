#ifdef __cplusplus
extern "C" {
#endif
#include <assert.h>
static int side_effect;
int assertion_enabled(void) { side_effect=0;assert(++side_effect==1);return side_effect; }
#define NDEBUG
#include <assert.h>
int assertion_disabled(void) { side_effect=0;assert(++side_effect);assert(undefined_name_is_not_compiled);return side_effect; }
#undef NDEBUG
#include <assert.h>
int assertion_reenabled(void) { side_effect=0;assert(++side_effect);return side_effect; }
#ifdef __cplusplus
struct Truth { explicit operator bool() const { return true; } bool operator||(int) const = delete; };
int assertion_contextual_bool(void) { assert(Truth{});return 1; }
#else
static_assert(sizeof(int)==4,"portable int");
int assertion_contextual_bool(void) {return 1;}
#endif
void assertion_failure(void) {
#line 71 "synthetic-assert.c"
    assert(side_effect == 7);
}
#include <stdio.h>
#include <wchar.h>
void assertion_redirect_failure(const char *path) {
    FILE *f=fopen(path,"wb");
    if(!f)__assert_fail("fopen succeeded",__FILE__,__LINE__,__func__);
    stderr=f;fwide(stderr,1);assertion_failure();
}

#ifdef __cplusplus
}
#endif
