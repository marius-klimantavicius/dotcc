//type:cp
//options:--g++:--clang
//options_all:--c++11 --short_enums

typedef unsigned int uint32_t;

template<typename, typename> struct same;
template<typename T> struct same<T, T> {};

enum E1
  {
   e1_1,
   e1_2
  };

enum E2
  {
   e2_1,
   e2_2 = 1U << 7
  };

struct TEST_STRUCT
{
  enum E1 a:4;
  enum E2 b:8;
  unsigned int c:4;
} ctx = { e1_2, e2_2, 1 };

void f() {
  same<decltype(ctx.a << 8U), int>();
  same<decltype(ctx.b << 8U), int>();
  same<decltype(ctx.c << 8U), int>();
}
