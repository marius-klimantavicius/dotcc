//type: fp
//options:  --c++20 --modules
# 0 "./modules/pr118920_c.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/pr118920_c.C"


# 1 "./modules/pr118920.h" 1
template <typename T> struct out_ptr_t {
  operator int() const;
};
# 4 "./modules/pr118920_c.C" 2
import "pr118920_b.H";
import "pr118920_a.H";
