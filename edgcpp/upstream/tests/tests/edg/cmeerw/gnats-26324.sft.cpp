//type:fp
//options:-w --c++03 --exceptions --pending_instantiations 1100 -DDEPTH=500

namespace minimal
{
  template<typename, typename> struct A;
  template<typename T, int N>
  struct C {
    typedef typename C<A<T, T>, N - 1>::type type;
  };
  template<typename T>
  struct C<T, 0> {
    typedef int type;
  };
  C<int, 100>::type t;
}

#ifndef DEPTH
#define DEPTH 3
#endif

template<typename T, typename U>
struct X
{ };

template<typename T, int D>
struct M
{
  typedef typename M<X<T, T>, D-1>::type type;
};

template<typename T>
struct M<T, 0>
{
  typedef X<T, T> type;
};

template<typename T>
struct C
{
  int f() { return 0; }
};

M<int, DEPTH> m;
M<int, DEPTH>::type t;
int i = C<M<int, DEPTH>::type>().f();

int f(M<int, DEPTH> pm, M<int, DEPTH>::type pt)
{
  M<int, DEPTH> lm;
  M<int, DEPTH>::type lt;

  C<M<int, DEPTH> > cm;
  C<M<int, DEPTH>::type> ct;

  return cm.f() + ct.f();
}
