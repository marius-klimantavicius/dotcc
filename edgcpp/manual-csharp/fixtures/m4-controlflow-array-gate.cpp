// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
#include "m4-controlflow-array-gate.h"
void helper(char16_t&);
constexpr int run(){int helper=0;return collect(3);}
constexpr int reversed(){char cells[]="abc";int counter=0;(counter=1)[cells+(counter=2)-1]='Z';return counter*100+cells[2];}
static_assert(run()==198);
static_assert(collect(3L)==198);
static_assert(collect(1)==97);
static_assert(collect(3)==198);
static_assert(reversed()==290);
