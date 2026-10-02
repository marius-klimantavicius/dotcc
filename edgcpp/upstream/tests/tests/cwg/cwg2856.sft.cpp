//options_all:--c++23 -tused -A
struct A
{
  explicit A(int = 10);
  A()= default;
};

A a = {}; //msvc ok but gcc and clang fails here
  int f(A);
  int x = f({});  // #2

//cwg: 2856
//title: Copy-list-initialization with explicit default constructors
//meeting: Tokyo 3/24
//edg_status: Passes
