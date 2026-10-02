//type:fn
//options::--gnu_version 40800;cp:--gnu_version 40900:--clang
//options_all:--c++11

struct A {
  constexpr int f() const { return x+1; }
  int const x;
};
int x[A().f()]; // A() deleted with gnu_version < 40900
