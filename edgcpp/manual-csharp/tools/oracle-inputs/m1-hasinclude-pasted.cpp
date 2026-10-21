#define CAT(a,b) a##b
#if !CAT(__has_,include)(<outer//slash.h>)
#error pasted header operator must read untouched caller tail
#endif
int value = 1;
