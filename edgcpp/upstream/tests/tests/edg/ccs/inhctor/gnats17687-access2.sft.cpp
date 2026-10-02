//type:cp
//options::-DNEG;fn
//options_all:--c++17

template<typename T>
struct Outer {
  class InnerA {
    InnerA(int);
#ifndef NEG
    friend void f();
#endif
  };

  class InnerB : InnerA {
    using InnerA::InnerA;
  };
};

void f() {
  Outer<int>::InnerB var(0);
}
