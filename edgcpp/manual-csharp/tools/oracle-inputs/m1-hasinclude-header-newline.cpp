#define HI __has_include
#if HI(<outer/*comment
*/inner.h>)
#endif
int value = 1;
