//type: fp
//options:  --c++20 --modules
# 0 "./modules/pr98531-2_b.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/pr98531-2_b.C"


# 1 "./modules/pr98531-2.h" 1

struct __waiters
{
  __waiters() noexcept;
  ~__waiters () noexcept;

  static __waiters &_S_for()
  {
    static __waiters w;

    return w;
  }
};
# 4 "./modules/pr98531-2_b.C" 2
import "pr98531-2_a.H";
