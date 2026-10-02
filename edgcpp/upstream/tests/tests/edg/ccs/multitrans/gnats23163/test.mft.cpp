//type:fn
//options:;cp:-A:-DDIFF:-DDIFF2
//options_all:--c++11 -tused --multi_trans_unit
//source_files:test.h gnats23163-p2.C
//require:COMPILE_MULTIPLE_TRANSLATION_UNITS 1

#undef DIFF
#undef DIFF2

#include "test.h"
