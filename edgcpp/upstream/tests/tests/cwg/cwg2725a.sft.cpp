//type:fn
//options_all:--c++20 -tused
  struct A {
    static void f();
    static void f(int);
  } x;
  void (*p)() = x.f;   // error

//cwg: 2725
//title: Overload resolution for non-call of class member access
//meeting: Kona 11/23
//edg_status: EDGcpfe/26784
