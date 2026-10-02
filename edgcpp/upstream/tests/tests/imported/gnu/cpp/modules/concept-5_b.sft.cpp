//type: s
//options:  --c++20 --modules
# 0 "./modules/concept-5_b.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/concept-5_b.C"


# 1 "./modules/concept-5.h" 1
template<typename T>
requires (sizeof (T) == 1)
constexpr int f1 (T x) { return 1; }

template<typename T>
requires (sizeof (T) != 1)
constexpr int f1 (T x) { return 0; }
# 4 "./modules/concept-5_b.C" 2
import "concept-5_a.H";

static_assert (f1 ('a') == 1);
static_assert (f1 (0xa) == 0);
