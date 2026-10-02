//type:cp
//options:--c++11

namespace minimal
{
  template<typename> struct C;
  template<typename ... As>
  struct C<int(As ...)> {
    template<typename U> using A = int;
    template<typename U, typename = typename C<int(As ...)>::template A<U>>
    struct V;
    template<typename U, typename V = V<U>> void f();
  };
  C<int(int)> c;
}

namespace outer_alias
{
  template<typename T>
  struct X
  { };

  template<typename T, typename ... B> using P = void (T::*)(X<B> ...);

  struct C
  {
    template<typename T, typename ... A>
    void f(P<T, A ...>);
  };

  template<typename T, typename ... A>
  void C::f(void (T::*)(X<A>...))
  { }
}

namespace inner_alias
{
  template<typename T>
  struct X
  { };

  template<typename ... A>
  struct C
  {
    template<typename T> using P = void (T::*)(X<A> ...);
    template<typename T> void f(P<T>);
  };

  template<typename ... A> template<typename T>
  void C<A ...>::f(void (T::*)(X<A> ...))
  { }
}

int main()
{ }
