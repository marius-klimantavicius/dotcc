//type:cp
//options:;fn:--nonstd_anonymous_unions:--g++:--clang:--microsoft
//options_all:--c++20
//fixing_pr:22112
struct A {
  struct {
    char c;
  };
};

A f() {
  A a{ .c = 1 };
  return a;
}
