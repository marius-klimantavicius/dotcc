#define ID(x) x
#define HI ID(__has_include)
#if !HI(<outer//slash.h>)
#error expanded header operator must read untouched caller header
#endif
int value = 1;
