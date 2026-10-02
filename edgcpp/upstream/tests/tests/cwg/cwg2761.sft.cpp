//options_all:--c++23 -A
  struct S{
    ~S() {}
  };

  struct A {
    union {
      S arr_;
    };
    ~A(); // user-provided!
  };

  auto foo() {
    return A{S()};
  }

//cwg: 2761
//title: Implicitly invoking the deleted destructor of an anonymous union member
//meeting: Kona 11/23
//edg_status: Passes
