//type:cp
//options::--gnu_version 80000:--clang
//options_all:--c++11

void f() {
  struct Z {
    int i;
    int b = ([&] { return i; }());
  } z;
}
