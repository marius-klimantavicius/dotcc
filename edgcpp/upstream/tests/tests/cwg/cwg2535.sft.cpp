//options_all:--c++20  -A
  struct A { int i; };
  struct B { int j; };
  struct D : A, B {};
  void f() {
    D d;
    static_cast<B&>(d).j;       // OK, object expression designates the B subobject of d
    reinterpret_cast<B&>(d).j;  // undefined behavior
  }

//cwg: 2535
//title: Type punning in class member access
//meeting: Virtual 7/22
//edg_status: N/A
//fixed_in: N/A
