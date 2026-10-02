//type:fp
//options:--c++11 --clang_version 80100 --ms_extensions:--c++20 --clang_version 220100 --ms_extensions

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

namespace dll_export_base
{
  template<class T> struct C;
  template<class T>
  struct B {
    __attribute__((exclude_from_explicit_instantiation))
    int f() { return C<T>::v; }
  };
  template struct __declspec(dllexport) B<int>;
}
