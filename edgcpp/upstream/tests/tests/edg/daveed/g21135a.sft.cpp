//remark:noexcept and calls through folded ptr-to-member expression
//options:--c++17;fn:--gnu_version=80100 --c++17;fn

struct S {
  void f(void) noexcept {}
  void g(void) {}
} s;
 
static_assert(noexcept((s.*(true ? &S::f : &S::g))()), "");
