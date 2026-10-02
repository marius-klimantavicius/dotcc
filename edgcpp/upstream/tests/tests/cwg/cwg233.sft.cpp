//options_all:--c++23 -tused
  struct Z {};

  struct A {
    operator Z&();
    operator const Z&();       // #1
  };

  struct B {
    operator Z();
    operator const Z&&();      // #2
  };

  const Z& r1 = A();          // OK, uses #1
  const Z&& r2 = B();         // OK, uses #2

//cwg: 233
//title: References vs pointers in UDC overload resolution
//meeting: St Louis 6/24
//edg_status: EDGcpfe/27412
