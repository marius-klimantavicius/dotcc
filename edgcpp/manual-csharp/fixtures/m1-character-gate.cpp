#if __has_include("m1-character-gate.h") && 'A' == 65 && u'\xffff' == 65535 && __STDC_HOSTED__
#include "m1-character-gate.h"
constexpr int pick(int value){return value+1;}
constexpr int pick(long value){return value+2;}
constexpr int run(){RETURN(pick(id(CHAR_VALUE)));}
static_assert(run()==66);
static_assert(id(TARGET_CHAR)==65);
static_assert(id('B')==66);
#else
#error target character profile mismatch
#endif
