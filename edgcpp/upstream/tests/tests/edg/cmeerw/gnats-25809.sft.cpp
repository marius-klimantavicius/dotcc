//type:fp
//options:--c++20:--c++20 --clang:--ms_c++20

namespace minimal
{
  template<bool B>
  using A = decltype([] () {
    if constexpr (B) return 1;
    else return 2; } ());
  template<bool B> using AA = A<B>;
}

namespace func_decl
{
  template<bool B>
  using A = decltype([] () {
    if constexpr (B) return 1;
    else return 2;
  } ());

  template<bool B>
  auto foo() -> A<B>;
}

namespace in_class_specialization
{
  template<bool B>
  struct A
  {
    template<typename T>
    struct C
    { };

    template<>
    struct C<int>
    {
      static int foo()
      {
        if constexpr (B) return 1;
        else return 2;
      }
    };
  };

  void foo()
  {
    A<true>::C<int>::foo();
    A<false>::C<int>::foo();
  }
}
