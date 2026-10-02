//type: fp
//options:  --c++20 --modules --c++20
# 0 "./modules/using-enum-1_b.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/using-enum-1_b.C"


# 1 "./modules/using-enum-1_a.H" 1



enum class E {a, b, c};
struct C
{
  using enum E;
};

struct D: C
{
  int foo ()
  {
    return int (a);
  }
};
# 4 "./modules/using-enum-1_b.C" 2
import "using-enum-1_a.H";
