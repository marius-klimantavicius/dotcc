//type:fp
//options:--c++20:--ms_c++20

namespace minimal
{
  template<typename T> T f(T);
  template<typename T> using A = decltype(f(T{}));
  template<typename> concept C = true;
  template<typename T> requires C<A<T>>
  struct B {
    B() requires C<A<T>>;
  };
  auto l = [] () {
    B<int> b1;
    B<int *> b2;
  };
}

namespace no_colon_declval
{
  template<class T>
  T &declval() noexcept;

  template<typename T>
  using IR = decltype(* declval<T>() );

  template<typename T>
  using RR = IR<decltype(((T*)0)->begin())>;

  template<typename>
  concept I = true;

  template<typename T1> requires I<RR<T1>>
  struct V {
    V(T1);
    void f() requires I<RR<T1>>;
  };

  template<typename T>
  struct VI
  {
    void operator *() const;
  };

  template<typename T>
  struct VE
  {
    VI<T> begin() const;
  };

  template<typename T>
  struct SR {
    T begin() const;
  };

  void foo(V<VE<int>> v, SR<VI<int *>> a)
  {
    v.f();
    V vv(a);
  }
}

namespace colon_declval
{
  template<class T>
  T &declval() noexcept;

  template<typename T>
  using IR = decltype(* colon_declval::declval<T>() );

  template<typename T>
  using RR = IR<decltype(((T*)0)->begin())>;

  template<typename>
  concept I = true;

  template<typename T1> requires I<RR<T1>>
  struct V {
    V(T1);
    void f() requires I<RR<T1>>;
  };

  template<typename T>
  struct VI
  {
    void operator *() const;
  };

  template<typename T>
  struct VE
  {
    VI<T> begin() const;
  };

  template<typename T>
  struct SR {
    T begin() const;
  };

  void foo(V<VE<int>> v, SR<VI<int *>> a)
  {
    v.f();
    V vv(a);
  }
}

namespace sfinae_context
{
  struct A1
  {
    void operator *() const;
  };

  struct A2
  {
    int operator *() const;
  };

  struct B
  { };

  void h(A1);
  int h(A2);

  template<typename T>
  void operator /(B, T *);

  template<typename ... Args>
  struct C
  {
    template<typename U>
    static auto f() -> decltype(( ..., h(Args{})));

    template<typename U>
    static auto g() -> decltype(( ..., *Args{}));

    template<typename U>
    auto m() -> decltype(( ..., (Args{} / this)));
  };

  template<typename T>
  void operator /(A1, T *);

  template<typename T>
  int operator /(A2, T *);

  void foo()
  {
    C<A1, A2>::f<int>() + 1;
    C<A1, A2>::g<int>() + 2;
    C<A1, A2>().m<int>() + 3;
  }
}
