//type: fp
//options:  --c++20 --modules
# 0 "./modules/pmf-1_b.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/pmf-1_b.C"


# 1 "./modules/pmf-1.h" 1

struct X
{
  int mfn ();
};

inline void bob (X &)
{
  int (X::*pmf) () = &X::mfn;
}
# 4 "./modules/pmf-1_b.C" 2
import "pmf-1_a.H";
