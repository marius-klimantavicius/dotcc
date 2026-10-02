//type:fp
//options:--c++11:--c++20:--ms_c++20

namespace PR_25971
{
  template <typename T1, typename T2>
  struct MyTuple
  {
    T1 t1;
    T2 t2;
  };

  template <typename T, typename T2>
  T get(T2);

  template <typename T>
  struct decay { using type = T; };

  template<typename... Args>
  struct func_helper
  {
    static void f(int & a, long & b)
    { }

    template<typename T>
    static auto run(T t) ->
      decltype(f(get<typename decay<Args>::type>(t)...))
    { }
  };

  void foo(MyTuple<int &, long &> t)
  {
    func_helper<int &, long &>::run(t);
  }
}

namespace minimal
{
  template<typename T1, typename T2> T2 f(T1, T2);
  template<typename T> T g();
  template<typename T> struct D { using type = T; };
  template<typename... A> struct C {
    template<typename T> auto h() -> decltype(f(g<typename D<A>::type>()...));
  };
  long *l = C<int, long *>().h<int>();
}
