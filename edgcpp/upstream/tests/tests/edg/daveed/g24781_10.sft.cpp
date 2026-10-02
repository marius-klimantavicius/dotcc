//remark:Explicit-this member functions
//options:--c++23;fp

struct B {
  consteval int f(this B const&) { return 1; }
};

static_assert(B{}.f() == 1);

struct D: B {
  using B::f;
  consteval int f(this B &) { return 2; }
} d;

static_assert(D{}.f() == 1);
static_assert(d.f() == 2);

