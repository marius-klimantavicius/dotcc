//options_all:--c++23 
  template <auto V>
  void foo() {}

  void bar() {
    foo<+[]{}>();                   // #3: OK, a function pointer is a structural type
  }
