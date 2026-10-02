//type:fp
//options:--clang_version 220100:--c++11 --clang_version 80100:--c++20 --clang_version 220100
//options_all:-tused

namespace minimal
{
  template<typename T> struct C;
  template<typename T>
  struct B {
    __attribute__((exclude_from_explicit_instantiation)) int f() {
      return C<T>::v;
    }
  };
  template struct B<int>;
}

namespace attr_underscore
{
  template<typename T> struct C;
  template<typename T>
  struct B {
    __attribute__((__exclude_from_explicit_instantiation__))
    int f() { return C<T>::v; }
  };
  template struct B<int>;
}

namespace attr_no_underscope
{
  template<typename T> struct C;
  template<typename T>
  struct B {
    __attribute__((exclude_from_explicit_instantiation))
    int f() { return C<T>::v; }
  };
  template struct B<int>;
}

namespace cpp_attr_clang_prefix
{
  template<typename T> struct C;
  template<typename T>
  struct B {
    [[clang::exclude_from_explicit_instantiation]]
    int f() { return C<T>::v; }
  };
  template struct B<int>;
}

namespace static_data_member
{
  template<typename T> struct C;
  template<typename T>
  struct B {
    [[clang::exclude_from_explicit_instantiation]]
    static int value;
  };
  template<typename T>
  int B<T>::value = C<T>::v;
  template struct B<int>;
}

namespace member_class
{
  template<typename T> struct C;
  template<typename T>
  struct B {
    struct [[clang::exclude_from_explicit_instantiation]] N {
      int f() { return C<T>::v; }
    };
  };
  template struct B<int>;
}

namespace extern_template
{
  template<typename T> struct C { static const int v = 1; };
  template<typename T>
  struct B {
    [[clang::exclude_from_explicit_instantiation]]
    int f() { return C<T>::v; }
  };
  extern template struct B<int>;
  int use() { return B<int>().f(); }
}

namespace pragma_inst
{
  template<typename T> struct C;
  template<typename T>
  struct B {
    __attribute__((exclude_from_explicit_instantiation))
    int f();
  };
  template<typename T>
  int B<T>::f() { return C<T>::v; }
#pragma instantiate B<int>
}
