//type:cp
//options::--g++:--clang
//options_all:--c++11

typedef unsigned int uint32_t;

template<typename, typename> struct same;
template<typename T> struct same<T, T> {};

enum TEST_ENUM : char
  {
   ZERO,
   ONE
  };

struct TEST_STRUCT
{
  enum TEST_ENUM a:4;
  unsigned int b:4;
} ctx = { ONE, 1 };

void f() {
  same<decltype(ctx.a << 8U), int>();
  same<decltype(ctx.b << 8U), int>();
}
