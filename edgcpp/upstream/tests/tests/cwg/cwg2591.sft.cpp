//options_all:--c++20 -tused -A
  union A {
    int x;
    union {
     int y;
    };
  };
  void f() {
    A a = {.x = 1};
    a.y = 2;
  }

//cwg: 2591
//title: Implicit change of active union member for anonymous union in union
//meeting: Kona 11/23
//edg_status: Passes
