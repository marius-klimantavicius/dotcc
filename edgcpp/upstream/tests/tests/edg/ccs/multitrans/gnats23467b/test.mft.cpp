//type:cp
//options::-DTARG1:-DTARG2:-DTARG1 -DTARG2
//options_all:--gnu_version 40902 --c++11 --multi_trans_unit
//source_files:gnats23467b-2.C test.h
//require:COMPILE_MULTIPLE_TRANSLATION_UNITS 1

#ifdef TARG1
#pragma GCC target "sse2"
#endif

#include "test.h"
