//type:fn
//options::--vla
//options_all:--c++20

struct A {
  A() noexcept(false) = default;
  ~A() {}
};

void f()
{
  static const A a [ new A[1]{} ];
}
