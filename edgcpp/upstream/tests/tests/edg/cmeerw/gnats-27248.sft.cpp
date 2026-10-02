//type:fp
//options:--c++11 --clang_version 180100:--c++20 --clang_version 180100

namespace minimal
{
  struct A;
  template<typename T>
  struct C {
    T t;
  };
  void f(C<A> *c) {
    __builtin_operator_delete(c);
  }
}

namespace global_scoped
{
  struct A;

  template<typename T>
  struct C
  {
    T t;
  };

  void f(C<A> *c)
  {
    ::__builtin_operator_delete(c);
  }
}

namespace parenthesized_id
{
  struct A;

  template<typename T>
  struct C
  {
    T t;
  };

  void f(C<A> *c)
  {
    (__builtin_operator_delete)(c);
  }
}
