#define HI __has_include
#if HI(<outer/*unterminated.h>)
R"(one\
two)"
#endif
