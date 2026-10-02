//options_all:-r -x -tused
//options: --strict;cp

const void a();
volatile void b();
const volatile void c();

const int d();
volatile int e();
const volatile int f();

struct S {
  operator const int();
  operator volatile int();
  operator const volatile int();
};

template <class T> struct X {
  T f();
  operator T();
};
X<const int> x;

