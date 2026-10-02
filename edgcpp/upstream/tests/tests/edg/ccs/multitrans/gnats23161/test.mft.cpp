//type:cp
//options_all:--c++11 --multi_trans_unit
//source_files:test.h gnats23161-2.C
//require:COMPILE_MULTIPLE_TRANSLATION_UNITS 1

#include "test.h"

foo::~foo() = default;
