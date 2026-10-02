//type:fp
//options:--c++11:--c++20:--c++20 --gn 140100:--c++20 --clang_version 180100:--ms_c++20 --microsoft_version 1936

namespace minimal
{
  template<typename T>
  struct C {
    template<typename = void> using A = int;
  };
  template<typename T, typename ... Us>
  typename C<T>::template A<> f(Us ...);
  auto v = f<int>();
}

namespace alias_template
{
  template<typename T>
  struct C {
    template<typename = void>
    using A = int;
  };
  template<typename T, typename ... Us>
  typename C<T>::template A<> f(Us ... u);

  auto v0 = f<int>();
  auto v1 = f<int>(1);
}

namespace class_template
{
  template<typename T>
  struct C {
    template<typename = void>
    struct A
    { };
  };
  template<typename T, typename ... Us>
  typename C<T>::template A<> f(Us ... u);

  auto v0 = f<int>();
  auto v1 = f<int>(1);
}
