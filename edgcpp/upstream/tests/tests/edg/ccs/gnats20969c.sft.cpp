//type:fp
//options:--gnu_version 30200:--gnu_version 40100:--microsoft_version 1920
//options_all:--c++11 -w

struct S {
#ifdef _MSC_VER
  char c1: 3, c2: 8;
  unsigned char uc1: 3, uc2: 8;
  short s1: 3, s2: 16;
  unsigned short us1: 3, us2: 16;
#else
  char c1: 3, c2: 32;
  unsigned char uc1: 3, uc2: 32;
  short s1: 3, s2: 32;
  unsigned short us1: 3, us2: 32;
#endif /* _MSC_VER */
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
  SAME(s.uc2, int);
  SAME(s.s1, int);
  SAME(s.s2, int);
  SAME(s.us1, int);
  SAME(s.us2, int);
  SAME(s.i1, int);
  SAME(s.i2, int);
  SAME(s.ui1, unsigned int);
  SAME(s.ui2, unsigned int);
  SAME(s.l1, long);
  SAME(s.l2, long);
  SAME(s.ul1, unsigned long);
  SAME(s.ul2, unsigned long);
  SAME(s.ll1, long long);
  SAME(s.ll2, long long);
  SAME(s.ull1, unsigned long long);
  SAME(s.ull2, unsigned long long);
}
