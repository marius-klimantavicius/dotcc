//type:fp
//options:--c++11 -A:--clang:--c++20 --clang_version 220100

namespace minimal {
  template<typename T> void f();
  template<typename T> struct C {
    T f;
  };
  template<typename T> bool g(C<T> c) {
    return c.f < 0;
  }
  template bool g(C<int>);
}

namespace template_member_function {
  template<typename T> void f();
  template<typename T> struct C {
    template<int>
    T f();
  };
  template<typename T> bool g(C<T> c) {
    return c.template f<0>();
  }
  template bool g(C<int>);
}
