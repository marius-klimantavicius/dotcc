//type:fp
//options::--gnu_version 40200:--clang
//options_all:--c++11 -w

struct S {
  char c1: 3, c2: 32;
  unsigned char uc1: 3, uc2: 32;
  short s1: 3, s2: 32;
  unsigned short us1: 3, us2: 32;
  int i1: 3, i2: 32;
  unsigned int ui1: 3, ui2: 32;
  long l1: 3, l2: 32;
  unsigned long ul1: 3, ul2: 32;
  long long ll1:3, ll2: 32;
  unsigned long long ull1: 3, ull2: 32;
};

template<typename T, typename U> struct same;
template<typename T> struct same<T,T> {};
#define SAME(x,y) same<decltype(+x),y>()

void f(S s) {
  SAME(s.c1, int);
  SAME(s.c2, int);
  SAME(s.uc1, int);
#ifdef __clang__
  SAME(s.uc2, unsigned int);
#else
  SAME(s.uc2, int);
#endif /* __clang__ */
  SAME(s.s1, int);
  SAME(s.s2, int);
  SAME(s.us1, int);
#ifdef __clang__
  SAME(s.us2, unsigned int);
#else
  SAME(s.us2, int);
#endif /* __clang__ */
  SAME(s.i1, int);
  SAME(s.i2, int);
  SAME(s.ui1, int);
  SAME(s.ui2, unsigned int);
  SAME(s.l1, int);
  SAME(s.l2, int);
  SAME(s.ul1, int);
  SAME(s.ul2, unsigned int);
  SAME(s.ll1, int);
  SAME(s.ll2, int);
  SAME(s.ull1, int);
  SAME(s.ull2, unsigned int);
}
