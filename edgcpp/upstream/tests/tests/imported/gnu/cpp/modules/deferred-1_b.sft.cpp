//type: fp
//options:  --c++20 --modules
# 0 "./modules/deferred-1_b.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/deferred-1_b.C"


# 1 "./modules/deferred-1.h" 1
template<bool _Const>
struct _Iterator
{
private:
  static void mover (const _Iterator &arg = {}) noexcept (noexcept (arg));

public:
  _Iterator() = default;

  friend void move (const _Iterator &arg2) noexcept (noexcept (mover (arg2)))
  {}
};
# 4 "./modules/deferred-1_b.C" 2
import "deferred-1_a.H";
