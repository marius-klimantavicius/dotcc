//type:fp
//options_all:--c++17 -tused -A
  struct A {
    A(int = 0);
  };

  struct B: A {
    using A::A;
    B();
  };

  int main() {
    B b;  // OK, B::B()
  }

//cwg: 2273
//title: Inheriting constructors vs implicit default constructor
//meeting: Toronto 7/17
//edg_status: EDGcpfe/22592
