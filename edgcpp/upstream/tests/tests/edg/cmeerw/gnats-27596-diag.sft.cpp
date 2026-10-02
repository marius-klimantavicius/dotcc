//type:fn
//options:--c++20

namespace mismatched_constraints
{
  template<typename> concept X = true;
  template<typename> concept Y = false;

  template<typename T> struct C
  {
    template<typename U>
    void f(U) requires X<T>;

    void g(int) requires Y<T>;

    template<typename U>
    void h(U);
  };

  template<> template<typename U>
  void C<int>::f(U) requires X<int *>;

  template<> template<typename U>
  void C<long>::f(U) requires X<long *>
  { }

  template<>
  void C<int>::g(int);

  template<>
  void C<long>::g(int)
  { }

  template<> template<typename U>
  void C<int>::h(U) requires X<int *>;

  template<> template<typename U>
  void C<long>::h(U) requires X<long *>
  { }
}
