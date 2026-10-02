//type:fn
//options:--c++20:--c++20 --gn 120100:--c++20 --gn 130100:--c++20 --clang_version 1700

namespace no_inst
{
  template<typename T> requires true
  struct D
  { };

  template<typename T> requires true
  class C {
    template<typename U> friend class C; // accepted by GCC, clang

    template<typename U> friend class no_inst::D; // error
  };
}

namespace inst
{
  template<typename T> requires true
  class C {
    template<typename U> friend class C; // accepted by GCC <13.0
  };

  C<int> c;
}
