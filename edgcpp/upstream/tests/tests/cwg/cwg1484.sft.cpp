//type:fn
//options_all:--c++14 -tused -A
  template<typename T> void f() {
    struct B {
      T t;
    };
  }

  int main() {
    f<void>();
  }

//cwg: 1484
//title: Unused local classes of function templates
//meeting: Urbana-Champaign 11/14
//edg_status: Passes
