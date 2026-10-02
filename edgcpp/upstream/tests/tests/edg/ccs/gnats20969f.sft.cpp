//type:fp
//options::--gnu_version 30200:--gnu_version 40200:--clang:--microsoft_version 1920
//options_all:--c++11 -w --target linux_x86_64
//require:TARG_SIZEOF_LONG_linux_x86_64 8

enum E { a, b = 1ULL << 24 };
enum E2 { a2, b2 = 1ULL << 31 };
enum E3 { a3, b3 = 1ULL << 48 };
enum E4 { a4, b4 = 1ULL << 63 };

struct S {
  E e: 3;
  E2 e2: 3;
  E3 e3: 3;
  E4 e4: 3;
};

template<typename T, typename U> struct same;
template<typename T> struct same<T,T> {};
#define SAME(x,y) same<decltype(+x),y>()

void f(S s) {
#ifdef _MSC_VER
  SAME(s.e, int);
  SAME(s.e2, int);
  SAME(s.e3, int);
  SAME(s.e4, int);
#else
  SAME(s.e, int);
  SAME(s.e2, unsigned int);
  SAME(s.e3, long);
  SAME(s.e4, unsigned long);
#endif /* _MSC_VER */
}
