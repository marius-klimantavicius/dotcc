//type:fp
//options:--c++11 --gn 130200:--c++11 --clang_version 180200

template<bool>
struct C
{
  using type = int;
};

struct Non_Dependent
{
  template<bool B, typename C<B>::type = 0>
  void f();
};


template<int I>
struct Dependent
{
  template<bool B, typename C<I == 1 && B>::type = 0>
  void f2();

  template<bool B, typename C<I == 2 && B>::type = 0>
  void f2();                    // accepted by GCC/Clang

  template<bool B, typename C<I == 1 && B>::type = 0>
  void s2();

  template<bool B, typename C<I == 2 && B>::type = 0>
  static void s2();             // accepted by GCC/Clang

};

Dependent<0> d0;


template<int I>
struct FnType
{
  template<bool B>
  C<I == 1 && B> f2();

  template<bool B>
  C<I == 2 && B> f2();          // accepted by GCC/Clang

  template<bool B>
  void f4(typename C<I == 1 && B>::type);

  template<bool B>
  void f4(typename C<I == 2 && B>::type); // accepted by GCC/Clang

  template<bool B>
  void f5(decltype(C<I == 1 && B>::v));

  template<bool B>
  void f5(decltype(C<I == 2 && B>::v)); // accepted by GCC/Clang
};

FnType<0> ft0;
