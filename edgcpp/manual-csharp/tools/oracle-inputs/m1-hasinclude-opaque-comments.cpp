#define HI __has_include
#if !HI(<outer/*unterminated.h>) || !HI(<outer//slash.h>)
#error literal header comment-like bytes must remain opaque
#endif
int value = 1;
