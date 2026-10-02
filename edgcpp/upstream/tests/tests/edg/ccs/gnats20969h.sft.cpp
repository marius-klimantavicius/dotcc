//type:cp
//options::--g++:--clang
//options_all:--c++11

typedef unsigned int uint32_t;

template<typename, typename> struct same;
template<typename T> struct same<T, T> {};

enum E1 : unsigned long long
  {
   e1_1,
   e1_2
  };

enum E2 : long long
  {
   e2_1,
   e2_2
  };

struct TEST_STRUCT
{
  enum E1 a:4;
  enum E2 b:4;
  unsigned int c:4;
} ctx = { e1_2, e2_2, 1 };

void f() {
  same<decltype(ctx.a << 8U), unsigned long long>();
  same<decltype(ctx.b << 8U), long long>();
  same<decltype(ctx.c << 8U), int>();
}
