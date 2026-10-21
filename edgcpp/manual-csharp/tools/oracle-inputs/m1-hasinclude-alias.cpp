#define h missing
#define HI __has_include
#if !__has_include(<h>) || !HI(<h>)
#error direct and alias header lookup must agree
#endif
int value = 1;
