//options:--clang_version 50000
//options_all:--c++11
//type:fp

#define CHECK_TRAITS(T) \
static_assert(__has_trivial_assign(T), "trivial assign " # T); \
static_assert(__has_nothrow_assign(T), "nothrow assign " # T); \
static_assert(__has_trivial_move_assign(T), "trivial move assign " # T); \
static_assert(__has_nothrow_move_assign(T), "nothrow move assign " # T); \

CHECK_TRAITS(char)
CHECK_TRAITS(short)
CHECK_TRAITS(int)
CHECK_TRAITS(long)
CHECK_TRAITS(long long)

CHECK_TRAITS(unsigned char)
CHECK_TRAITS(unsigned short)
CHECK_TRAITS(unsigned int)
CHECK_TRAITS(unsigned long)
CHECK_TRAITS(unsigned long long)

CHECK_TRAITS(void *)
CHECK_TRAITS(int *)

struct POD {
  int foo;
  char bar;
};
static_assert(__is_pod(POD), "struct POD isn't POD");
CHECK_TRAITS(POD *)

class nonPOD {
  public:
    nonPOD(int);
};
static_assert(!__is_pod(nonPOD), "class nonPOD is POD");
CHECK_TRAITS(nonPOD *)

CHECK_TRAITS(float)
CHECK_TRAITS(double)

enum E1 { red=1 };
static_assert(__has_trivial_assign(E1), "trivial assign E1");
static_assert(__has_nothrow_assign(E1), "nothrow assign E1");
static_assert(__has_trivial_move_assign(E1), "trivial move assign E1");
static_assert(__has_nothrow_move_assign(E1), "nothrow move assign E1");
