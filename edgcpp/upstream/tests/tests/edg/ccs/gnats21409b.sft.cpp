//type:cp
//options_all:--c++11

struct A {
  int x;
  constexpr A(int i) : x(i) {}
  constexpr A(const A& obj) : x(obj.x + 10) {}
};

struct _AAA {
  A val;
  constexpr operator A() const { return val; }
};

constexpr A a(37);
constexpr A b(_AAA{a});

static_assert(a.x == 37, "a.x should be 37");
static_assert(b.x == 57, "b.c should be 57");
