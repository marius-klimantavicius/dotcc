//type: fp
//options:  --c++20 --modules
# 0 "./modules/auto-1_b.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/auto-1_b.C"


# 1 "./modules/auto-1.h" 1

template <typename T> auto frob (T t)
{
  return t;
}

struct Bob
{
  operator auto ()
  {
    return 0;
  }
};

inline auto foo ()
{
  return frob (1) + int (Bob ());
}
# 4 "./modules/auto-1_b.C" 2
import "auto-1_a.H";

int bar ()
{
  return foo () + frob (0u);
}
