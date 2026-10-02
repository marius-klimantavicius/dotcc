//type:fp
//options:--c++17:--c++17 --gn 150100:--c++17 --clang_version 201000:--ms_c++17 --microsoft_version 1944

  #include <initializer_list>

namespace minimal
{
  template<typename T>
  struct C {
    C(std::initializer_list<T>);
  };
  C<int> f(const C<int> &c) {
    return C{c};
  }
}

namespace with_tpyedef
{
  template<typename T>
  struct C
  {
    C(std::initializer_list<T>);
  };

  using CINT = C<int>;

  C<int> f(CINT c)
  {
    return C{c};
  }
}
