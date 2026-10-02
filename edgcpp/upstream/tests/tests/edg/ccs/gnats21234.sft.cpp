//type:cp
//options::-DNEG;fn
//options_all:--c++11 -tused

struct A {};
struct B {};

template<typename T>
T ident(T t) { return t; }

template <typename T>
void foo(T t)
{
  auto p = ident(t);
  A a{ident(p)};
}

void f()
{
  foo(A{});
#ifdef NEG
  foo(B{});
#endif /* NEG */
}
