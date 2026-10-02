//type:fn
//options::--vla
//options_all:--c++14

struct A {
  constexpr A() noexcept(false) { throw 0; }
  ~A();
};

void f()
{
  static const A a [ new A[1]{} ];
}
