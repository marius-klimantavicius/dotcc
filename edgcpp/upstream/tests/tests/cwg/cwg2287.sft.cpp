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

//cwg: 2287
//title: Pointer-interconvertibility in non-standard-layout unions
//meeting: Toronto 7/17
//edg_status: Passes
