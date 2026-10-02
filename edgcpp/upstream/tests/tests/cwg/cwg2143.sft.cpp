//type:fp
//options_all:--c++17 -tused -A
//
// This is really language clarification, but there was an example, so I made a test case


  template <typename T> struct A {
     void foo() {
        A* p = 0;
        bar(p);    // will be found by ADL at the point of instantiation
        bar(this); // same here
     }
  };

  void bar(...);

  int main() {
     A<int> a;
     a.foo();
  }

//cwg: 2143
//title: Value-dependency via injected-class-name
//meeting: Issaquah 11/16
//edg_status: Passes
