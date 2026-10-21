#define h missing
#define H <h>
#define HI __has_include
#if HI(H)
#error macro-produced header content must expand h to missing
#endif
int value = 1;
