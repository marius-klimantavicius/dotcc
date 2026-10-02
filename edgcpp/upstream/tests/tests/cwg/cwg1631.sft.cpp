//type:fp
//options_all:--c++17 -tused -A
  struct A { int a[1]; };
  struct B { B(int); };
  void f(B, int);
  void f(int, A);

  int main() {
    f({0}, {{1}});
  }

//cwg: 1631
//title: Incorrect overload resolution for single-element initializer-list (See 1467)
//meeting: Urbana-Champaign 11/14
//edg_status: Passes
