#define ID(x) x
#if !ID(__has_include)(<outer//slash.h>)
#error header operator must read untouched caller tail
#endif
int value = 1;
