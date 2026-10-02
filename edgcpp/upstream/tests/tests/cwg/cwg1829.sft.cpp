//type:fp
//options_all:--c++20 -tused -A
  template <typename T> struct A {
    struct { } obj;
    void foo() {
      bar(obj); // lookup for bar when/where?
    }
  };

  void bar(...);

  int main() {
    A<int> a;
    a.foo();    // calls bar(...)?
  }

//cwg: 1829
//title: Dependent unnamed types
//meeting: Virtual 11/20*
//edg_status: Passes
