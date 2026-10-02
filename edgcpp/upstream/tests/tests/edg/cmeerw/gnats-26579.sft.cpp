//type:fn
//options:--c++11 --gn 130200:--c++11 --clang_version 180200

namespace minimal {
  template<bool>
  struct C {};
  template<int I>
  struct A {
    template<bool B>
    void f(C<I == 1 && B>);
    template<bool B>
    void f(C<I == 2 && B>);
  };
  A<0> a;
}

template<bool>
struct C
{
  using type = int;
};

struct Non_Dependent
{
  template<bool B, typename C<B>::type = 0>
  void f();

  template<bool B, typename C<B>::type = 0>
  void f();                     // error
};


template<int I>
struct Dependent
{
  template<bool B, typename C<B>::type = 0>
  void f1();

  template<bool B, typename C<B>::type = 0>
  void f1();                    // error

  template<bool B, typename C<I == 1 && B>::type = 0>
  void f2();

  template<bool B, typename C<I == 2 && B>::type = 0>
  void f2();                    // accepted by GCC/Clang

  template<bool B, typename C<I == 1>::type = 0>
  void f3();

  template<bool B, typename C<I == 2>::type = 0>
  void f3();                    // error on instantiation

  template<bool B, typename C<B>::type = 0>
  void s1();

  template<bool B, typename C<B>::type = 0>
  static void s1();             // error

  template<bool B, typename C<I == 1 && B>::type = 0>
  void s2();

  template<bool B, typename C<I == 2 && B>::type = 0>
  static void s2();             // accepted by GCC/Clang

};

Dependent<0> d0;


template<int O>
struct Outer
{
  template<int I>
  struct Dependent
  {
    template<bool B, typename C<B>::type = 0>
    void f1();

    template<bool B, typename C<B>::type = 0>
    void f1();                  // error

    template<bool B, typename C<I == 1>::type = 0>
    void f2();

    template<bool B, typename C<I == 2>::type = 0>
    void f2();                  // error on instantiation
  };

  struct Templated
  {
    template<bool B, typename C<B>::type = 0>
    void f1();

    template<bool B, typename C<B>::type = 0>
    void f1();                  // error

    template<bool B, typename C<O == 1>::type = 0>
    void f2();

    template<bool B, typename C<O == 2>::type = 0>
    void f2();                  // error on instantiation
  };
};

Outer<0>::Dependent<0> o0d0;
Outer<0>::Templated ot0;


template<int I>
struct FnType
{
  template<bool B>
  C<B> f1();

  template<bool B>
  C<B> f1();                    // error

  template<bool B>
  C<I == 1 && B> f2();

  template<bool B>
  C<I == 2 && B> f2();          // accepted by GCC/Clang

  template<bool B>
  C<I == 1> f3();

  template<bool B>
  C<I == 2> f3();               // error on instantiation

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
