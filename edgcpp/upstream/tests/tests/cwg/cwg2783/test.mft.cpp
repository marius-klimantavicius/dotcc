//options_all:--c++23 -tused -A
//source_files:S.h
module;
#include "S.h"

export module M;
export using ::S;

//cwg: 2783
//title: Handling of deduction guides in global-module-fragment
//meeting: Kona 11/23
//edg_status: Passes
