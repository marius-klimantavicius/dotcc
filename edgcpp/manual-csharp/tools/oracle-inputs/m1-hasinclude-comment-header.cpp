#define HI __has_include
#if !__has_include(<outer/*comment*/inner.h>) || !HI(<outer/*comment*/inner.h>)
#error literal header contents must preserve comment-looking characters
#endif
int value = 1;
