//type:fp
//options:--c++17 --g++:--c++20:--c++20 --gn 150100:--c++20 --clang_version 201000:--ms_c++20 --microsoft_version 1941

namespace minimal
{
  template<typename, typename = int>
  struct C;
  template<typename, template<typename> class = C>
  using A = int;
  A<int> a;
}

namespace non_default_arg
{
  template<typename T, typename U = int>
  struct C { };

  template<typename T, template<typename> class U = C>
  using A = int;

  template<typename T, template<typename> class U = C>
  struct B { };

  A<int> a0;
  A<int, C> a1;

  B<int> b0;
  B<int, C> b1;
}
