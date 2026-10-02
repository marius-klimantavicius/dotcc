//type: fp
//options:  --c++20 --modules
# 0 "./modules/partial-2_d.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/partial-2_d.C"



export module pr107033;

import "partial-2_c.H";

# 1 "./modules/partial-2.cc" 1
static_assert(is_reference_v<int&>);
static_assert(is_reference_v<int&&>);
static_assert(!is_reference_v<int>);

static_assert(A::is_reference_v<long&>);
static_assert(A::is_reference_v<long&&>);
static_assert(!A::is_reference_v<long>);
# 9 "./modules/partial-2_d.C" 2
